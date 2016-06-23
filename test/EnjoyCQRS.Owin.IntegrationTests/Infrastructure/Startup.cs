using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Core;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Shared;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.MessageBus.InProcess;
using Microsoft.Owin;
using Owin;

namespace EnjoyCQRS.Owin.IntegrationTests.Infrastructure
{
    public class Startup
    {
        public IEventStore EventStore { get; set; }

        public void Configuration(IAppBuilder appBuilder)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<IntervalSnapshotStrategy>().As<ISnapshotStrategy>();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
            builder.RegisterType<Session>().As<ISession>().InstancePerRequest();
            builder.RegisterType<Repository>().As<IRepository>();
            builder.RegisterType<AutofacCommandDispatcher>().As<ICommandDispatcher>().InstancePerRequest();
            builder.RegisterType<EventPublisher>().As<IEventPublisher>().InstancePerRequest();
            builder.RegisterType<AutofacEventRouter>().As<IEventRouter>();
            builder.RegisterType<NoopLoggerFactory>().As<ILoggerFactory>().InstancePerRequest();
            builder.RegisterType<EventSerializer>().As<IEventSerializer>();
            builder.RegisterType<JsonTextSerializer>().As<ITextSerializer>();

            builder.Register(c => EventStore).As<IEventStore>();
            
            builder.RegisterAssemblyTypes(typeof(FooAssembler).Assembly)
                   .AsClosedTypesOf(typeof(ICommandHandler<>))
                   .AsImplementedInterfaces();
            
            builder.RegisterAssemblyTypes(typeof(FooAssembler).Assembly)
                   .AsClosedTypesOf(typeof(IEventHandler<>))
                   .AsImplementedInterfaces();

            var typeOfMetadataProvider = typeof(OwinMetadataProvider);

            builder.RegisterAssemblyTypes(typeOfMetadataProvider.Assembly)
                .Where(e => typeOfMetadataProvider.IsAssignableFrom(e))
                .AsImplementedInterfaces();

            builder.RegisterType<OwinContext>().As<IOwinContext>().InstancePerRequest();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            
            var container = builder.Build();

            var config = new HttpConfiguration
            {
                DependencyResolver = new AutofacWebApiDependencyResolver(container)
            };

            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            
            appBuilder.UseWebApi(config);
            appBuilder.UseAutofacWebApi(config);
            appBuilder.UseAutofacMiddleware(container);
        }
    }
}