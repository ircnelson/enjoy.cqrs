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
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Collections;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.EventSource.Storage
{
    /// <summary>
    /// Event Store repository abstraction.
    /// </summary>
    public interface IEventStore : ISnapshotStore, IDisposable
    {
        /// <summary>
        /// Start the transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Confirm modifications.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Revert modifications.
        /// </summary>
        void Rollback();

        /// <summary>
        /// Retrieve all events based on <param name="id"></param>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IEnumerable<IDomainEvent>> GetAllEventsAsync(Guid id);

        /// <summary>
        /// Save the events in Event Store.
        /// </summary>
        /// <param name="collection"></param>
        Task SaveAsync(UncommitedDomainEventCollection collection);
    }
}
