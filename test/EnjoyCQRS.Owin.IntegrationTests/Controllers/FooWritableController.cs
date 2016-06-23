using System;
using System.Threading.Tasks;
using System.Web.Http;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.FooAggregate;
using EnjoyCQRS.MessageBus;

namespace EnjoyCQRS.Owin.IntegrationTests.Controllers
{
    [RoutePrefix("command/foo")]
    public class FooWritableController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommandDispatcher _dispatcher;

        public FooWritableController(IUnitOfWork unitOfWork, ICommandDispatcher dispatcher)
        {
            _unitOfWork = unitOfWork;
            _dispatcher = dispatcher;
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
    }
}
