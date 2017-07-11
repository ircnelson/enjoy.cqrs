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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnjoyCQRS.Projections
{
    public class ProjectionRebuilder
    {
        private readonly IProjectionStore _documentStore;
        private readonly IEnumerable _projectors;
        private readonly EventStreamReader _eventStreamReader;

        public ProjectionRebuilder(EventStreamReader eventStreamReader, IProjectionStore documentStore, IEnumerable projectors)
        {
            _eventStreamReader = eventStreamReader ?? throw new ArgumentNullException(nameof(eventStreamReader));
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            _projectors = projectors ?? throw new ArgumentNullException(nameof(documentStore));

            ProjectorMethodMapper.CreateMap(projectors);
        }

        public async Task ProcessAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // TODO: verify if needs to rebuild projections

                // TODO: logging

                await _eventStreamReader.ReadAsync(cancellationToken, WireEvent)
                    .ConfigureAwait(false);
                
                var tasks = new List<Task>();

                foreach (var bucket in _documentStore.GetBuckets())
                {
                    var task = Task.Run(async () =>
                    {
                        var contents = await _documentStore.EnumerateContentsAsync(bucket);

                        _documentStore.Cleanup(bucket);

                        await _documentStore.ApplyAsync(bucket, contents);
                    });

                    tasks.Add(task);
                }
                
                Task.WaitAll(tasks.ToArray());

                // TODO: logging
            }
            catch (OperationCanceledException)
            {
                // TODO: logging
            }
            catch (Exception)
            {
                // TODO: logging
            }
        }

        public void WireEvent(object @event)
        {
            ProjectorMethodMapper.GetWiresOf(@event.GetType()).ForEach(projectorMethod => projectorMethod.Call(@event));
        }
    }
}
