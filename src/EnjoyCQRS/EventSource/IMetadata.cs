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

namespace EnjoyCQRS.EventSource
{
    /// <summary>
    /// Store the event metadatas.
    /// Some avaliable keys: <see cref="MetadataKeys"/>.
    /// </summary>
    public interface IMetadata : IReadOnlyDictionary<string, string>
    {
        /// <summary>
        /// Get the stored value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetValue(string key);

        /// <summary>
        /// Get the value, convert it using the <paramref name="converter"/> function and return the type specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        T GetValue<T>(string key, Func<string, T> converter);
    }
}