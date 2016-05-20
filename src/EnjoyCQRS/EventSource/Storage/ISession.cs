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
using EnjoyCQRS.EventSource.Snapshots;

namespace EnjoyCQRS.EventSource.Storage
{
    /// <summary>
    /// Represents an abstraction where aggregate events will be persisted.
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Retrieves an <typeparam name="TAggregate"></typeparam> based on your unique identifier property.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate, new();
        
        /// <summary>
        /// Add an instance of <typeparam name="TAggregate"></typeparam>.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="aggregate"></param>
        Task AddAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate;

        /// <summary>
        /// Begin the transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Confirm affected changes.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Save the modifications.
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Revert modifications.
        /// </summary>
        void Rollback();

    }
}