using Microsoft.Extensions.DependencyInjection;
using TutorBot.Abstractions;
namespace TutorBot.Core
{
    internal class ApplicationCore : IApplication
    {
        public ApplicationCore(IServiceProvider serviceProvider)
        {
            HistoryService = new HistoryServiceCore(serviceProvider);
        }

        public IHistoryService HistoryService { get; }
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
    }
}
