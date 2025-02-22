using TutorBot.Abstractions;
namespace TutorBot.Core
{
    internal class ApplicationCore : IApplication
    {
        public ApplicationCore(ApplicationDbContext dbContext)
        {
            HistoryService = new HistoryServiceCore(dbContext);
        }

        public IHistoryService HistoryService { get; }
    }


    internal class HistoryServiceCore(ApplicationDbContext dbContext) : IHistoryService
    {
        public async Task AddHistory(Abstractions.MessageHistory history)
        {
            await dbContext.AddAsync(history.MapCore());
            await dbContext.SaveChangesAsync();
        }
    }
}
