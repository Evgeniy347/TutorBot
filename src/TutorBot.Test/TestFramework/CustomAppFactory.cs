using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Testing.Platform.Configurations;
using Org.BouncyCastle.Utilities;
using System.Collections.Generic;
using System.Reflection;
using Telegram.Bot;
using TutorBot.App;
using TutorBot.TelegramService;
using TutorBot.Test.DevOps;
using TutorBot.Test.Helpers;
using Xunit.Sdk;
using Xunit.v3;

namespace TutorBot.Test.TestFramework;

public class CustomAppFactory(TestContainersFixture containers) : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly Queue<IHost> _hosts = new Queue<IHost>();
    private Action<IWebHostBuilder>? _webHostBuilderConfiguration;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        IConfigurationRoot customConfig = new ConfigurationBuilder()
               .AddJsonFile(DevOpsHelper.AppSettings_json_path)
               .AddJsonFile(DevOpsHelper.AppSettings_private_json_path)
               .AddJsonFile(DevOpsHelper.AppSettings_test_json_path)
               .Build();

        builder.UseConfiguration(customConfig);
        AppContext.SetSwitch("DisableLoadConfig", true);

        builder.UseSetting("ConnectionStrings:DefaultConnection", containers.PostgresConnectionString);
        builder.UseSetting("DefaultConnection", containers.PostgresConnectionString);

        _webHostBuilderConfiguration?.Invoke(builder);


        builder.ConfigureServices(services =>
        {
            services.AddTransient<Func<string, CancellationToken, ITelegramBot>>(provider =>
                (token, cancellationToken) => new TelegramBotFake(token, cancellationToken: cancellationToken));

            services.AddControllers().AddApplicationPart(typeof(CustomAppFactory).Assembly);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        IHost host = base.CreateHost(builder);
        _hosts.Enqueue(host);

        return host;
    }

    internal async Task<HttpClient> CreateApplication(Action<IWebHostBuilder>? configure = null)
    {
        await Task.Yield();
        //bool needRestore = false;
        _webHostBuilderConfiguration = configure;

        //XunitTestClass @class = TaskHelper.Current?.TestClass as XunitTestClass ??
        //    throw new NullReferenceException("@class is null");

        //DatabaseSnapshotGroupAttribute? snapshotGroup = @class.Class.GetCustomAttribute<DatabaseSnapshotGroupAttribute>() ??
        //    throw new NullReferenceException("snapshotGroup is null");

        //DBSnapshotData snapshot = containers.GetSnapshot(snapshotGroup.Snapshot);

        //if (!DatabaseSnapshotGroupAttribute.Equals(snapshotGroup, _snapshotGroup))
        //{
        //    needRestore = snapshot.HasSnapshot;
        //    _snapshotGroup = snapshotGroup;
        //}

        //if (!snapshot.HasSnapshot)
        //{
        //    await containers.RestoreSourceSnapshot(snapshotGroup.Snapshot);

        //    HttpClient clientSnapShot = this.CreateClient();

        //    ApplicationHelper.UpdateSchema();
        //    await containers.CreateDatabaseSnapshot(snapshotGroup.Snapshot);

        //    return clientSnapShot;
        //}
        //else if (needRestore)
        //{
        //    await containers.RestoreDatabaseSnapshot(snapshotGroup.Snapshot);
        //    await containers.WaitForReady();

        //    ApplicationHelper.ResetStorage();
        //    ApplicationHelper.ResetLock();
        //}

        HttpClient client = this.CreateClient();
        return client;
    }

    public async ValueTask InitializeAsync()
    {
        await Task.Yield();
    }

    public override async ValueTask DisposeAsync()
    {
        while (_hosts.Count > 0)
            await _hosts.Dequeue().StopAsync();
    }
}



[Collection("TestContainerCollection")]
[TestCaseOrderer(typeof(PriorityOrderer))]
[Trait("Category", "Integration")]
public abstract class IntegrationTestsBase : IClassFixture<CustomAppFactory>;


public class PriorityOrderer : ITestCaseOrderer, ITestCollectionOrderer
{
    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, ITestCase
    {
        return testCases
            .Select(x => (testCase: x, groupName: GetGroupName(x), priority: GetPriority(x)))
            .OrderByDescending(x => x.groupName)
            .ThenBy(x => x.priority)
            .ThenBy(x => x.testCase.TestClassName)
            .ThenBy(x => x.testCase.TestMethodName)
            .Select(x => x.testCase)
            .ToArray();
    }

    private static string GetGroupName(ITestCase testCase)
    {
        if (testCase.TestClass is XunitTestClass @class)
        {
            DatabaseSnapshotGroupAttribute? attribute = @class.Class.GetCustomAttribute<DatabaseSnapshotGroupAttribute>();
            if (attribute != null)
                return attribute.GroupName;
        }

        return "DefaultGroup";
    }

    private static int GetPriority(ITestCase testCase)
    {
        if (testCase.TestMethod is XunitTestMethod method)
        {
            TestPriorityAttribute? attribute = method.Method.GetCustomAttribute<TestPriorityAttribute>();
            if (attribute != null)
                return attribute.Priority;
        }

        return 0;
    }

    public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections) where TTestCollection : ITestCollection
    {
        throw new NotImplementedException();
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestPriorityAttribute(int priority) : Attribute
{
    public int Priority { get; } = priority;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal sealed class DatabaseSnapshotGroupAttribute(string groupName = "DefaultGroup") : Attribute
{
    public string GroupName { get; } = groupName;
}

[CollectionDefinition("TestContainerCollection", DisableParallelization = true)]
public class MyContainerCollection : ICollectionFixture<TestContainersFixture>;

