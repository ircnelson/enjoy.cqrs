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

using EnjoyCQRS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace EnjoyCQRS.Projections
{
    public class ProjectorMethodMapper
    {
        private static readonly Dictionary<Type, List<Wire>> _mappings = new Dictionary<Type, List<Wire>>();
        private static object _lock = new object();

        public static void CreateMap(IEnumerable projectors, string methodName = "When")
        {
            foreach (var projector in projectors)
            {
                var methodInfos = projector
                    .GetType()
                    .GetTypeInfo()
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(e => e.Name == methodName)
                    .Where(e => e.GetParameters().Length == 1);

                foreach (var methodInfo in methodInfos)
                {
                    var argument = methodInfo.GetParameters()[0].ParameterType;

                    bool hasMetadata = false;
                    Type eventType = argument;

                    if (argument.GetTypeInfo().IsGenericType && argument.Name.Equals(typeof(Metadata<>).Name))
                    {
                        eventType = argument.GetTypeInfo().GetGenericArguments()[0];
                        hasMetadata = true;
                    }

                    var info = methodInfo;

                    if (!_mappings.TryGetValue(eventType, out List<Wire> list))
                    {
                        list = new List<Wire>();
                        _mappings.Add(eventType, list);
                    }

                    list.Add(BuildWire(projector, argument, hasMetadata, info));
                }
            }
        }

        public static IEnumerable<Type> GetMappedEvents()
        {
            if (_mappings == null) return null;

            return _mappings.Keys.ToList();
        }
        
        public static List<Wire> GetWiresOf(Type @event)
        {
            if (!_mappings.ContainsKey(@event)) return null;

            return _mappings[@event];
        }
        
        private static Wire BuildWire(object projector, Type whenParatemeterType, bool hasMetadata, MethodInfo methodInfo)
        {
            var info = methodInfo;
            var dm = new DynamicMethod("MethodWrapper", null, new[] { typeof(object), typeof(object) });
            var il = dm.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, projector.GetType());
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Castclass, whenParatemeterType);
            il.EmitCall(OpCodes.Call, info, null);
            il.Emit(OpCodes.Ret);

            var call = (Action<object, object>)dm.CreateDelegate(typeof(Action<object, object>));
            var wire = new Wire
            {
                Call = (message, metadata) => 
                {
                    if (hasMetadata)
                    {
                        var obj = Activator.CreateInstance(whenParatemeterType, new[] { message, metadata });

                        call(projector, obj);
                    }
                    else
                    {
                        call(projector, message);
                    }
                },
                ParameterType = whenParatemeterType
            };
            
            return wire;
        }

        public sealed class Wire
        {
            public Action<object, object> Call { get; set; }
            public Type ParameterType { get; set; }
        }
    }
}
