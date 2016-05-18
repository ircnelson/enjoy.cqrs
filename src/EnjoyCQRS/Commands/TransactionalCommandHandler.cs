// The MIT License (MIT)
// 
// Copyright (c) 2016 Nelson Corrêa V. Júnior
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.Commands
{
    /// <summary>
    /// Keep the <see cref="ICommandHandler{TCommand}" /> in transaction.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public class TransactionalCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> _commandHandler;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionalCommandHandler(
            ICommandHandler<TCommand> commandHandler,
            IUnitOfWork unitOfWork)
        {
            if (commandHandler == null) throw new ArgumentNullException(nameof(commandHandler));
            if (unitOfWork == null) throw new ArgumentNullException(nameof(unitOfWork));

            _commandHandler = commandHandler;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(TCommand command)
        {
            await _commandHandler.ExecuteAsync(command).ConfigureAwait(false);

            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
    }
}