using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;

namespace TutorBot.Test.TestFramework;

public class TestContainersFixture : IAsyncLifetime
{
    public static readonly bool UseCachedContainers = Environment.GetEnvironmentVariable("TutorBot_UseCachedContainers") == "true";

    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgresContainer; 
      
    private readonly DockerResourceName _resourceName;
    private string? _PostgresConnectionString;

    public TestContainersFixture()
    {
        string prefixName = GetRandomPrefix();

        _resourceName = new DockerResourceName(prefixName, $"tutor_tst_pg_net_{prefixName}", $"tutor_test_db_{prefixName}");

        _network = new NetworkBuilder()
                .WithName(_resourceName.NetworkName)
                .Build();

        PostgreSqlBuilder postgresBuilder = new PostgreSqlBuilder()
            .WithName(_resourceName.PGName)
            .WithImage("postgres:17.4")
            .WithDatabase("DBF_Config")
            .WithUsername("postgres")
            .WithPassword("password")
            .WithCleanUp(!UseCachedContainers)
            .WithNetwork(_network);

        postgresBuilder = postgresBuilder
            .WithTmpfsMount("/var/lib/postgresql/data", AccessMode.ReadWrite)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(5432)
                .UntilCommandIsCompleted("pg_isready -U postgres"));

        _postgresContainer = postgresBuilder.Build();
    }

    public string PostgresConnectionString => Check.NotEmpty(_PostgresConnectionString);
     
    public async ValueTask InitializeAsync()
    { 
        await _network.CreateAsync();
        await _postgresContainer.StartAsync(TestContext.Current.CancellationToken); 
        _PostgresConnectionString = _postgresContainer.GetConnectionString();
    }

    public async ValueTask DisposeAsync()
    {
        if (!UseCachedContainers)
        {
            await _postgresContainer.StopAsync(TestContext.Current.CancellationToken);
            await _network.DeleteAsync(TestContext.Current.CancellationToken);
        }
    } 
     
    private static string GetRandomPrefix()
    {
        if (UseCachedContainers)
            return "Debug";

        Random rnd = new Random();
        char[] letters = new char[8];
        for (int i = 0; i < letters.Length; i++)
            letters[i] = (char)rnd.Next('a', 'z' + 1);

        return new string(letters);
    }

    internal record struct DockerResourceName(string Prefix, string NetworkName, string PGName);
}
 