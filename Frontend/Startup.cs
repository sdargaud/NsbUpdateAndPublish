using System;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using Shared;

public class Startup
{
    public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddEnvironmentVariables();
        Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        #region EndpointConfiguration

        var endpointConfiguration = new EndpointConfiguration("DomainA-Endpoint");

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(ConnectionStrings.DomainA);
        transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.SqlDialect<SqlDialect.MsSqlServer>();
        persistence.ConnectionBuilder(
            connectionBuilder: () =>
            {
                return new SqlConnection(ConnectionStrings.DomainA);
            });
        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.DisableCache();
        endpointConfiguration.EnableInstallers();

        #endregion

        SqlHelper.EnsureDatabaseExists(ConnectionStrings.DomainA);

        endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

        services.AddSingleton<IMessageSession>(endpoint);
        services.AddSingleton<Func<SqlConnection>>(() => new SqlConnection(ConnectionStrings.DomainA));

        services.AddMvc();
    }


    public void Configure(IApplicationBuilder app, IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        applicationLifetime.ApplicationStopping.Register(OnShutdown);

        app.UseMvc(routeBuilder => routeBuilder.MapRoute(name: "default",
            template: "{controller=AcceptOrder}/{action=Post}"));
    }

    void OnShutdown()
    {
        endpoint?.Stop().GetAwaiter().GetResult();
    }

    IEndpointInstance endpoint;
}