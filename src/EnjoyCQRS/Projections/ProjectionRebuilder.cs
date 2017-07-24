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
using System.Threading;
using System.Threading.Tasks;

namespace EnjoyCQRS.Projections
{
    public class ProjectionRebuilder : IProjectionRebuilder
    {
        protected readonly ProjectorMethodMapper ProjectorMethodMapper = new ProjectorMethodMapper();

        protected readonly IProjectionStore DocumentStore;
        protected readonly IEnumerable Projectors;

        public Action OnStartProcess;
        public Action OnCompleteProcess;

        public ProjectionRebuilder(IProjectionStore documentStore, IEnumerable projectors)
        {
            DocumentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            Projectors = projectors ?? throw new ArgumentNullException(nameof(documentStore));

            ProjectorMethodMapper.CreateMap(projectors);
        }

        public async Task RebuildAsync<T>(T eventStreamReader, CancellationToken cancellationToken = default(CancellationToken))
            where T : EventStreamReader
        {
            if (eventStreamReader == null) throw new ArgumentNullException(nameof(eventStreamReader));
            
            OnStartProcess?.Invoke();

            try
            {
                await eventStreamReader.ReadAsync(cancellationToken, WireEvent).ConfigureAwait(false);

                var tasks = new List<Task>();

                foreach (var bucket in DocumentStore.GetBuckets())
                {
                    var task = Task.Run(async () =>
                    {
                        var contents = await DocumentStore.EnumerateContentsAsync(bucket).ConfigureAwait(false);

                        if (eventStreamReader.DeleteAllRecords)
                        {
                            DocumentStore.Cleanup(bucket);
                        }

                        await DocumentStore.ApplyAsync(bucket, contents).ConfigureAwait(false);
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());

                OnCompleteProcess?.Invoke();

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
        
        protected void WireEvent(object @event, object metadata)
        {
            var wires = ProjectorMethodMapper.GetWiresOf(@event.GetType());

            if (wires != null)
            {
                wires.ForEach(projectorMethod => projectorMethod.Call(@event, metadata));
            }
        }
    }
}
