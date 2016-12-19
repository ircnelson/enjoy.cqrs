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
using System.Reflection;

namespace EnjoyCQRS.ValueObjects
{
    public abstract class ValueObject<TValueObject> : IEquatable<TValueObject> where TValueObject : ValueObject<TValueObject>
    {
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            TValueObject other = obj as TValueObject;
            
            return Equals(other);
        }

        public override int GetHashCode()
        {
            const int startValue = 17;
            const int multiplier = 59;

            var fields = GetFields();
            var hashCode = startValue;

            foreach (FieldInfo field in fields)
            {
                var value = field.GetValue(this);

                if (value != null)
                    hashCode = hashCode * multiplier + value.GetHashCode();
            }

            return hashCode;
        }

        public virtual bool Equals(TValueObject other)
        {
            if (other == null)
                return false;

            var t = GetType();
            var otherType = other.GetType();
            
            if (t != otherType)
                return false;

            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                object value1 = field.GetValue(other);
                object value2 = field.GetValue(this);

                if (value1 == null)
                {
                    if (value2 != null)
                        return false;
                }

                else if (!value1.Equals(value2))
                    return false;
            }
            
            return true;
        }
        
        private IEnumerable<FieldInfo> GetFields()
        {
            Type t = GetType();
            List<FieldInfo> fields = new List<FieldInfo>();

            while (t != typeof(object))
            {
                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
#if REFLECTIONBRIDGE && (!(NET40 || NET35 || NET20))
                t = t.BaseType;
#else
                t = t.GetTypeInfo().BaseType;
#endif
            }

            return fields;
        }

        public static bool operator ==(ValueObject<TValueObject> x, ValueObject<TValueObject> y)
        {
            if (ReferenceEquals(x, null))
            {
                return ReferenceEquals(y, null);
            }

            return x.Equals(y);
        }

        public static bool operator !=(ValueObject<TValueObject> x, ValueObject<TValueObject> y)
        {
            return !(x == y);
        }
    }
}