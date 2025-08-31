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
            await using (var scope = _locator.CreateAsyncScope())
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
            var scope = serviceProvider.CreateScope();
            return new ServiceLocatorScope(scope);
        }
    }

    internal class ChatService(ServiceLocator locator) : IChatService
    {
        public async Task<ChatEntry> Create(long userID, string firstName, string lastName, string username, long chatID)
        {
            await using (var scope = locator.CreateAsyncScope())
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
            await using (var scope = locator.CreateAsyncScope())
            {
                DBChatEntry? chatDB = await scope.DBContext.Chats.Where(x => x.ChatID == chatID).FirstOrDefaultAsync();

                if (chatDB != null)
                    return chatDB.MapCore();

                return null;
            }
        }

        public async Task<ChatEntry[]> GetChats(GetChatsFilter? filter)
        {
            await using (var scope = locator.CreateAsyncScope())
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
            await using (var scope = locator.CreateAsyncScope())
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

    }

    internal class HistoryServiceCore(ServiceLocator locator) : IHistoryService
    {
        public async Task AddStatusService(string status, string? message = null)
        {
            try
            {
                await using (var scope = locator.CreateAsyncScope())
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

        public async Task AddHistory(Abstractions.MessageHistory history)
        {
            try
            {
                await using (var scope = locator.CreateAsyncScope())
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

        public async Task<Abstractions.MessageHistory[]> GetMessages(long chatID, int offcet, int count, bool revers)
        {
            await using (var scope = locator.CreateAsyncScope())
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
