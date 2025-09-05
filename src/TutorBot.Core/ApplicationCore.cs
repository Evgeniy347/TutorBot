using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TutorBot.Abstractions;

namespace TutorBot.Core
{
    internal class ApplicationCore : IApplication
    {
        private ServiceLocator _locator;
        public ApplicationCore(IServiceProvider serviceProvider)
        {
            ServiceLocator locator = _locator = new ServiceLocator(serviceProvider, this);
            HistoryService = new HistoryServiceCore(locator);
            ChatService = new ChatService(locator);
            ALService = new ALServiceService(locator);
        }

        public IHistoryService HistoryService { get; }

        public IChatService ChatService { get; }

        public IALServiceService ALService { get; }

        public async Task EnsureCreated()
        {
            await using (ServiceLocatorScope scope = _locator.CreateAsyncScope())
            {
                scope.DBContext.Database.EnsureCreated();
            }
        }
    }

    public class ServiceLocatorScope(IServiceScope scope) : IAsyncDisposable
    {
        public ApplicationDbContext DBContext => scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        public void Dispose() => scope.Dispose();

        public async ValueTask DisposeAsync()
        {
            await Task.Yield();
            scope.Dispose();
        }
    }

    internal class ServiceLocator(IServiceProvider serviceProvider, ApplicationCore applicationCore)
    {
        public IServiceProvider Services => serviceProvider;

        public ApplicationCore Application => applicationCore;

        public ServiceLocatorScope CreateAsyncScope()
        {
            IServiceScope scope = serviceProvider.CreateScope();
            return new ServiceLocatorScope(scope);
        }
    }

    internal class ChatService(ServiceLocator locator) : IChatService
    {
        public async Task<ChatEntry> Create(long userID, string firstName, string lastName, string username, long chatID)
        {
            await using (ServiceLocatorScope scope = locator.CreateAsyncScope())
            {
                DBChatEntry chatDB = new DBChatEntry()
                {
                    ChatID = chatID,
                    UserID = userID,
                    FirstName = firstName,
                    LastName = lastName,
                    UserName = username,
                    TimeCreate = DateTime.Now,
                    TimeModified = DateTime.Now,
                    IsFirstMessage = true
                };

                scope.DBContext.Chats.Add(chatDB);

                await scope.DBContext.SaveChangesAsync();

                return chatDB.MapCore();
            }
        }

        public async Task<ChatEntry?> Find(long chatID)
        {
            await using (ServiceLocatorScope scope = locator.CreateAsyncScope())
            {
                DBChatEntry? chatDB = await scope.DBContext.Chats.Where(x => x.ChatID == chatID).FirstOrDefaultAsync();

                if (chatDB != null)
                    return chatDB.MapCore();

                return null;
            }
        }

        public async Task<ChatEntry[]> GetChats(GetChatsFilter? filter)
        {
            await using (ServiceLocatorScope scope = locator.CreateAsyncScope())
            {
                IQueryable<DBChatEntry> query = scope.DBContext.Chats;

                if (filter != null)
                {
                    if (filter.IsAdmin)
                        query = query.Where(x => x.IsAdmin);
                    if (filter.EnableAdminError)
                        query = query.Where(x => x.EnableAdminError == true);
                }

                return await query.Select(x => x.MapCore()).ToArrayAsync();
            }
        }

        public async Task Update(ChatEntry chat)
        {
            await using (ServiceLocatorScope scope = locator.CreateAsyncScope())
            {
                DBChatEntry chatDB = chat.MapDB();
                DBChatEntryVersion chatDBVer = chatDB.MapVersion();

                chatDB.TimeModified = DateTime.Now;
                chatDB.Version++;

                scope.DBContext.Chats.Update(chatDB);
                scope.DBContext.ChatsVersions.Add(chatDBVer);

                await scope.DBContext.SaveChangesAsync();
            }
        }

        public async Task<ChatSummaryReport> GetSummaryInfo()
        {
            await using (ServiceLocatorScope scope = locator.CreateAsyncScope())
            {
                ApplicationDbContext context = scope.DBContext;
                 
                DateTime twentyDaysAgo = DateTime.Now.AddDays(-20);

                // Базовые счетчики
                var numberOfChats = await context.Chats.CountAsync();
                var numberOfMessages = await context.MessageHistories.CountAsync();

                // Статистика по группам
                var groupStats = await context.Chats
                    .GroupBy(c => c.GroupNumber)
                    .Select(g => new
                    {
                        GroupNumber = g.Key,
                        UserCount = g.Select(c => c.UserID).Distinct().Count(),
                        ChatIDs = g.Select(c => c.ChatID).ToList()
                    })
                    .ToListAsync();

                var groupSummaries = new List<GroupSummary>();
                foreach (var group in groupStats)
                {
                    var messageCount = await context.MessageHistories
                        .Where(m => group.ChatIDs.Contains(m.ChatID))
                        .CountAsync();

                    groupSummaries.Add(new GroupSummary
                    {
                        GroupNumber = group.GroupNumber,
                        UserCount = group.UserCount,
                        MessageCount = messageCount
                    });
                }

                // Топ 100 пользователей
                var userMessageCounts = await context.MessageHistories
                    .GroupBy(m => new { m.ChatID, m.UserID })
                    .Select(g => new
                    {
                        ChatID = g.Key.ChatID,
                        UserID = g.Key.UserID,
                        MessageCount = g.Count()
                    })
                    .OrderByDescending(x => x.MessageCount)
                    .Take(100)
                    .ToListAsync();

                var topUsers = new List<UserMessageCount>();
                foreach (var userStat in userMessageCounts)
                {
                    var userInfo = await context.Chats
                        .Where(c => c.ChatID == userStat.ChatID && c.UserID == userStat.UserID)
                        .Select(c => new { c.FullName, c.GroupNumber })
                        .FirstOrDefaultAsync();

                    if (userInfo != null)
                    {
                        topUsers.Add(new UserMessageCount
                        {
                            FullName = userInfo.FullName,
                            GroupNumber = userInfo.GroupNumber,
                            MessageCount = userStat.MessageCount
                        });
                    }
                }

                // Средние обращения по часам
                var hourlyStats = await context.MessageHistories
                    .Where(m => m.Timestamp >= twentyDaysAgo)
                    .Select(m => new
                    {
                        m.ChatID,
                        m.Timestamp.Hour
                    })
                    .ToListAsync();

                var chatGroups = await context.Chats
                    .Select(c => new { c.ChatID, c.GroupNumber })
                    .ToDictionaryAsync(c => c.ChatID, c => c.GroupNumber);

                var hourlyAverages = hourlyStats
                    .GroupBy(m => new
                    {
                        GroupNumber = chatGroups.ContainsKey(m.ChatID) ? chatGroups[m.ChatID] : "Unknown",
                        m.Hour
                    })
                    .Select(g => new HourlyAverage
                    {
                        GroupNumber = g.Key.GroupNumber,
                        Hour = g.Key.Hour,
                        MessageCount = g.Count(),
                        AverageMessages = Math.Round(g.Count() / 20.0, 2)
                    })
                    .OrderBy(a => a.GroupNumber)
                    .ThenBy(a => a.Hour)
                    .ToList();

                var report = new ChatSummaryReport
                {
                    NumberOfChats = numberOfChats,
                    NumberOfMessages = numberOfMessages,
                    GroupSummaries = groupSummaries,
                    TopUsers = topUsers,
                    HourlyAverages = hourlyAverages
                };


                return report;
            }
        }
    }

    internal class HistoryServiceCore(ServiceLocator locator) : IHistoryService
    {
        public async Task AddStatusService(string status, string? message = null)
        {
            try
            {
                await using (ServiceLocatorScope scope = locator.CreateAsyncScope())
                {
                    await scope.DBContext.ServiceHistories.AddAsync(new DBServiceStatusHistory() { Status = status, Message = message ?? string.Empty, Timestamp = DateTime.Now });
                    await scope.DBContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task AddHistory(MessageHistory history)
        {
            try
            {
                await using (ServiceLocatorScope scope = locator.CreateAsyncScope())
                {
                    await scope.DBContext.MessageHistories.AddAsync(history.MapCore());
                    await scope.DBContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<MessageHistory[]> GetMessages(long chatID, int offcet, int count, bool revers)
        {
            await using (ServiceLocatorScope scope = locator.CreateAsyncScope())
            {
                if (revers)
                {
                    return await scope.DBContext.MessageHistories
                        .Where(x => x.Id < offcet && x.ChatID == chatID && !string.IsNullOrWhiteSpace(x.Type))
                        .OrderByDescending(x => x.Id)
                        .Take(count)
                        .Select(x => x.MapCore())
                        .ToArrayAsync();
                }
                else
                {
                    return await scope.DBContext.MessageHistories
                        .Where(x => x.Id > offcet && x.ChatID == chatID && !string.IsNullOrWhiteSpace(x.Type))
                        .OrderBy(x => x.Id)
                        .Take(count)
                        .Select(x => x.MapCore())
                        .ToArrayAsync();
                }
            }
        }
    }
}
