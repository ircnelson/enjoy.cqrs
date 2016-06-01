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
                t = t.BaseType;
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