using System;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.BarAggregate;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.EventHandlers
{
    public class ManyDependenciesEvent : IDomainEvent
    {
        public string Text { get; }

        public ManyDependenciesEvent(string text)
        {
            Text = text;
        }
    }

    public class ManyDependenciesEventHandler : IEventHandler<ManyDependenciesEvent>
    {
        private readonly IRepository _repository;
        private readonly IBooleanService _booleanService;
        private readonly IStringService _stringService;

        public string Output { get; private set; }

        public ManyDependenciesEventHandler(IRepository repository, IBooleanService booleanService, IStringService stringService)
        {
            _repository = repository;
            _booleanService = booleanService;
            _stringService = stringService;
        }

        public async Task ExecuteAsync(ManyDependenciesEvent @event)
        {
            if (string.IsNullOrWhiteSpace(@event.Text))
                throw new ArgumentNullException(nameof(@event.Text));

            if (_booleanService.DoSomething())
            {
                Output = _stringService.PrintWithFormat(@event.Text);
            }

            await _repository.AddAsync(Bar.Create(Guid.NewGuid())).ConfigureAwait(false);
        }
    }
}