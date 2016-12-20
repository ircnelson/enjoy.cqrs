using System;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.FooAggregate;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.FooAggregate;
using Microsoft.AspNetCore.Mvc;

namespace EnjoyCQRS.IntegrationTests.Controllers
{
    [Route("command/foo")]
    public class FooWritableController : Controller
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

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var cmd = new CreateFooCommand(Guid.NewGuid());

            await _dispatcher.DispatchAsync(cmd);

            await _unitOfWork.CommitAsync();

            return Ok(cmd);
        }

        [HttpPost("{id}/doSomething")]
        public async Task<IActionResult> DoSomething(string id)
        {
            var cmd = new DoSomethingCommand(Guid.Parse(id));

            await _dispatcher.DispatchAsync(cmd);

            await _unitOfWork.CommitAsync();

            return Ok(cmd);
        }

        [HttpPost("flood/{times:int}")]
        public async Task<IActionResult> DoFlood(int times)
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