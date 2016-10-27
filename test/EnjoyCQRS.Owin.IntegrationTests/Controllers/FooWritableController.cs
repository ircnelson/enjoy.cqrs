using System;
using System.Threading.Tasks;
using System.Web.Http;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.FooAggregate;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.FooAggregate;
using EnjoyCQRS.MessageBus;

namespace EnjoyCQRS.Owin.IntegrationTests.Controllers
{
    [RoutePrefix("command/foo")]
    public class FooWritableController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommandDispatcher _dispatcher;
        private readonly IRepository _repository;

        public FooWritableController(IUnitOfWork unitOfWork, ICommandDispatcher dispatcher, IRepository repository)
        {
            _unitOfWork = unitOfWork;
            _dispatcher = dispatcher;
            _repository = repository;
        }

        [Route]
        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            var cmd = new CreateFooCommand(Guid.NewGuid());

            await _dispatcher.DispatchAsync(cmd);

            await _unitOfWork.CommitAsync();
            
            return Ok(cmd);
        }

        [Route("{id}/doSomething")]
        [HttpPost]
        public async Task<IHttpActionResult> DoSomething(string id)
        {
            var cmd = new DoSomethingCommand(Guid.Parse(id));

            await _dispatcher.DispatchAsync(cmd);

            await _unitOfWork.CommitAsync();

            return Ok(cmd);
        }

        [Route("flood/{times:int}")]
        [HttpPost]
        public async Task<IHttpActionResult> DoFlood(int times)
        {
            var create = new CreateFooCommand(Guid.NewGuid());

            await _dispatcher.DispatchAsync(create);

            var aggregate = await _repository.GetByIdAsync<Foo>(create.AggregateId);

            for (var i = 1; i < times; i++)
            {
                aggregate.DoSomething();
            }

            await _unitOfWork.CommitAsync();

            return Ok(create);
        }
    }
}
