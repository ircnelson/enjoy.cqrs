using System;
using System.Threading.Tasks;
using System.Web.Http;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.FakeGameAggregate;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FakeGameAggregate;
using EnjoyCQRS.MessageBus;

namespace EnjoyCQRS.Owin.IntegrationTests.Controllers
{
    [RoutePrefix("command/fakeGame")]
    public class FakeGameWritableController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommandDispatcher _dispatcher;
        private readonly IRepository _repository;

        public FakeGameWritableController(IUnitOfWork unitOfWork, ICommandDispatcher dispatcher, IRepository repository)
        {
            _unitOfWork = unitOfWork;
            _dispatcher = dispatcher;
            _repository = repository;
        }
        
        [Route("flood/{times:int}")]
        [HttpPost]
        public async Task<IHttpActionResult> DoFlood(int times)
        {
            var create = new CreateFakeGameCommand(Guid.NewGuid(), "Player 1", "Player 2");

            await _dispatcher.DispatchAsync(create);

            var aggregate = await _repository.GetByIdAsync<FakeGame>(create.AggregateId);

            for (int i = 1; i < times; i++)
            {
                aggregate.ChangePlayerName(2, "P2");
            }
            
            await _unitOfWork.CommitAsync();

            return Ok(create);
        }
    }
}
