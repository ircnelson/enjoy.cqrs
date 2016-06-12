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
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.EventSource
{
    /// <summary>
    /// Default implementation of <see cref="IUnitOfWork"/>.
    /// Keep the session in transaction state and rollback if do something wrong.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ISession _session;

        public UnitOfWork(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// Start transaction, saves the session and commit.
        /// Rollback will be called if something was wrong.
        /// </summary>
        /// <returns></returns>
        public async Task CommitAsync()
        {
            _session.BeginTransaction();

            try
            {
                await _session.SaveChangesAsync().ConfigureAwait(false);
                await _session.CommitAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                _session.Rollback();

                throw;
            }
        }
    }
}