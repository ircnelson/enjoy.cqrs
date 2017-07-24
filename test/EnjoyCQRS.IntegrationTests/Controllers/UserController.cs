using EnjoyCQRS.EventSource;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.UserAggregate;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EnjoyCQRS.IntegrationTests.Controllers
{
    [Route("command/user")]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommandDispatcher _dispatcher;

        public UserController(IUnitOfWork unitOfWork, ICommandDispatcher dispatcher)
        {
            _unitOfWork = unitOfWork;
            _dispatcher = dispatcher;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var cmd = new CreateUserCommand(Guid.NewGuid(), "Nelson", "Junior", new DateTime(1989, 6, 9));

            await _dispatcher.DispatchAsync(cmd);

            await _unitOfWork.CommitAsync();

            return Ok(cmd);
        }
    }
}
