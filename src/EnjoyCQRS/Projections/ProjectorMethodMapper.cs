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
using System.Reflection;
using System.Reflection.Emit;

namespace EnjoyCQRS.Projections
{
    public class ProjectorMethodMapper
    {
        private static readonly Dictionary<Type, List<Wire>> _mappings = new Dictionary<Type, List<Wire>>();

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
                    var eventType = methodInfo.GetParameters()[0].ParameterType;

                    var info = methodInfo;

                    if (!_mappings.TryGetValue(eventType, out List<Wire> list))
                    {
                        list = new List<Wire>();
                        _mappings.Add(eventType, list);
                    }

                    list.Add(BuildWire(projector, eventType, methodInfo));
                }
            }
        }
        
        public static List<Wire> GetWiresOf(Type @event)
        {
            return _mappings[@event];
        }
        
        private static Wire BuildWire(object o, Type type, MethodInfo methodInfo)
        {
            var info = methodInfo;
            var dm = new DynamicMethod("MethodWrapper", null, new[] { typeof(object), typeof(object) });
            var il = dm.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, o.GetType());
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Castclass, type);
            il.EmitCall(OpCodes.Call, info, null);
            il.Emit(OpCodes.Ret);

            var call = (Action<object, object>)dm.CreateDelegate(typeof(Action<object, object>));
            var wire = new Wire
            {
                Call = o1 => call(o, o1),
                ParameterType = type
            };
            
            return wire;
        }

        public sealed class Wire
        {
            public Action<object> Call;
            public Type ParameterType;
        }
    }
}
