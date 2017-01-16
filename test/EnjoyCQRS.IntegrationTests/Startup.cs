using System.Linq;
using System.Reflection;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Core;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.MessageBus.InProcess;
using EnjoyCQRS.UnitTests.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Scrutor;
using Microsoft.Extensions.DependencyInjection;

namespace EnjoyCQRS.IntegrationTests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Move it to another class and simplify for consumer

            services.AddSingleton<ILoggerFactory, NoopLoggerFactory>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ISession, Session>();
            services.AddScoped<ICommandDispatcher, CustomCommandDispatcher>();
            services.AddScoped<IEventPublisher, EventPublisher>();

            services.AddTransient<ISnapshotStrategy, IntervalSnapshotStrategy>();
            services.AddTransient<IRepository, Repository>();
            services.AddTransient<IEventRouter, CustomEventRouter>();
            services.AddTransient<IEventSerializer, EventSerializer>();
            services.AddTransient<ISnapshotSerializer, SnapshotSerializer>();
            services.AddTransient<ITextSerializer, JsonTextSerializer>();
            services.AddTransient<IProjectionSerializer, ProjectionSerializer>();

            services.Scan(e =>
                e.FromAssemblyOf<FooAssembler>()
                    .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces());

            services.Scan(e =>
                e.FromAssemblyOf<FooAssembler>()
                    .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>)))
                    .AsImplementedInterfaces());

            services.AddRouting();
            services.AddMvc();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }
}
