using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TutorBot.Abstractions;

namespace TutorBot.Core
{
    internal class ApplicationCore : IApplication
    {
        public ApplicationCore(IServiceProvider serviceProvider)
        {
            HistoryService = new HistoryServiceCore(serviceProvider);
            ChatService = new ChatService(serviceProvider);
        }

        public IHistoryService HistoryService { get; }

        public IChatService ChatService { get; }
    }

    internal class ChatService(IServiceProvider serviceProvider) : IChatService
    {
        public async Task<ChatEntry> Create(long userID, string firstName, string lastName, string username, long chatID)
        {
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                DBChatEntry chatDB = new DBChatEntry()
                {
                    ChatID = chatID,
                    UserID = userID,
                    FirstName = firstName,
                    LastName = lastName,
                    UserName = username,
                    TimeCreate = DateTime.Now,
                    TimeLastUpdate = DateTime.Now,
                    IsFirstMessage = true
                };

                dbContext.Chats.Add(chatDB);

                await dbContext.SaveChangesAsync();

                return chatDB.MapCore();
            }
        }

        public async Task<ChatEntry?> Find(long chatID)
        {
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                DBChatEntry? chatDB = await dbContext.Chats.Where(x => x.ChatID == chatID).FirstOrDefaultAsync();

                if (chatDB != null)
                    return chatDB.MapCore();

                return null;
            }
        }

        public async Task Update(ChatEntry chat)
        {
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                DBChatEntry chatDB = chat.MapDB();

                chatDB.TimeLastUpdate = DateTime.Now;

                dbContext.Chats.Update(chatDB);

                await dbContext.SaveChangesAsync();
            }
        }
    }

    internal class HistoryServiceCore(IServiceProvider serviceProvider) : IHistoryService
    {
        public async Task AddStatusService(string status, string? message = null)
        {
            try
            {
                await using (var scope = serviceProvider.CreateAsyncScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await dbContext.ServiceHistories.AddAsync(new ServiceStatusHistory() { Status = status, Message = message ?? string.Empty, Timestamp = DateTime.Now });
                    await dbContext.SaveChangesAsync();
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
                await using (var scope = serviceProvider.CreateAsyncScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await dbContext.MessageHistories.AddAsync(history.MapCore());
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<Abstractions.MessageHistory[]> GetMessages(long chatID, int offcet, int count, bool revers)
        {
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


                if (revers)
                {
                    return await dbContext.MessageHistories
                        .Where(x => x.Id < offcet && !string.IsNullOrWhiteSpace(x.Type))
                        .OrderByDescending(x => x.Id)
                        .Take(count)
                        .Select(x => x.MapCore())
                        .ToArrayAsync();
                }
                else
                {
                    return await dbContext.MessageHistories
                        .Where(x => x.Id > offcet && !string.IsNullOrWhiteSpace(x.Type))
                        .OrderBy(x => x.Id)
                        .Take(count)
                        .Select(x => x.MapCore())
                        .ToArrayAsync();
                }
            }
        }
    }
}
