using EnjoyCQRS.Projections;
using FluentAssertions;
using System;
using Xunit;

namespace EnjoyCQRS.UnitTests.Projections
{
    public abstract class ProjectionReaderWriterTests
    {
        public IProjectionReader<Guid, int> _reader;
        public IProjectionWriter<Guid, int> _writer;

        public IProjectionReader<Guid, Test1> _guidKeyClassReader;
        public IProjectionWriter<Guid, Test1> _guidKeyClassWriter;

        [Fact]
        public void Get_not_created_entity()
        {
            var key = Guid.NewGuid();

            int entity;

            _reader.TryGet(key, out entity).Should().BeFalse();
        }

        [Fact]
        public void Deleted_not_created_entity()
        {
            var key = Guid.NewGuid();

            _writer.TryDelete(key).Should().BeFalse();
        }
        
        [Fact]
        public void When_not_found_key_get_new_view_and_not_call_action()
        {
            Test1 t = new Test1 { Value = 555 };

            var key = Guid.NewGuid();

            var newValue = _guidKeyClassWriter.AddOrUpdate(key, t, tv => { tv.Value += 1; });

            t.Should().Be(newValue);

            555.Should().Be(newValue.Value);
        }

        [Fact]
        public void When_key_exist_call_action_and_get_new_value()
        {
            Test1 t = new Test1 { Value = 555 };

            var key = Guid.NewGuid();

            _guidKeyClassWriter.AddOrUpdate(key, t, tv => { tv.Value += 1; });

            var newValue = _guidKeyClassWriter.AddOrUpdate(key, t, tv => { tv.Value += 1; });

            t.GetType().Should().Be(newValue.GetType());

            556.Should().Be(newValue.Value);
        }

        [Fact]
        public void When_not_found_key_get_new_view_func_and_not_call_action()
        {
            Test1 t = new Test1 { Value = 555 };

            var key = Guid.NewGuid();

            var newValue = _guidKeyClassWriter.AddOrUpdate(key, () => t, tv => { tv.Value += 1; });

            t.Should().Be(newValue);

            555.Should().Be(newValue.Value);
        }

        [Fact]
        public void When_key_exist_not_call_new_view_func_and_call_action_and_get_new_value()
        {
            Test1 t = new Test1 { Value = 555 };

            var key = Guid.NewGuid();

            _guidKeyClassWriter.AddOrUpdate(key, t, tv => { tv.Value += 1; });

            var newValue = _guidKeyClassWriter.AddOrUpdate(key, () => t, tv => { tv.Value += 1; });

            typeof(Test1).Should().Be(newValue.GetType());

            556.Should().Be(newValue.Value);
        }

        [Fact]
        public void Add_new_value()
        {
            Test1 t = new Test1 { Value = 555 };

            var newValue = _guidKeyClassWriter.Add(Guid.NewGuid(), t);

            t.Should().Be(newValue);

            555.Should().Be(newValue.Value);
        }

        [Fact]
        public void Add_value_when_key_exist()
        {
            Test1 t = new Test1 { Value = 555 };

            var key = Guid.NewGuid();

            Action act = () =>
            {
                _guidKeyClassWriter.Add(key, t);
                _guidKeyClassWriter.Add(key, t);
            };

            act.ShouldThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void Update_value_with_func_when_key_not_found()
        {
            Action act = () =>
            {
                _guidKeyClassWriter.UpdateOrThrow(Guid.NewGuid(), tv =>
                {
                    tv.Value += 1;
                    return tv;
                });
            };

            act.ShouldThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void Update_value_with_func_when_key_exist()
        {
            Test1 t = new Test1 { Value = 555 };

            var key = Guid.NewGuid();

            _guidKeyClassWriter.Add(key, t);

            var newValue = _guidKeyClassWriter.UpdateOrThrow(key, tv =>
            {
                tv.Value += 1;
                return tv;
            });


            556.Should().Be(newValue.Value);
        }

        [Fact]
        public void Update_value_with_action_when_key_not_found()
        {
            Action act = () =>
            {
                _guidKeyClassWriter.UpdateOrThrow(Guid.NewGuid(), tv =>
                {
                    tv.Value += 1;
                });
            };

            act.ShouldThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void Update_value_with_action_when_key_exist()
        {
            Test1 t = new Test1 { Value = 555 };

            var key = Guid.NewGuid();

            _guidKeyClassWriter.Add(key, t);

            var newValue = _guidKeyClassWriter.UpdateOrThrow(key, tv =>
            {
                tv.Value += 1;
            });


            556.Should().Be(newValue.Value);
        }

        [Fact]
        public void Created_new_instance_when_call_update_method_and_key_not_found()
        {
            var key = Guid.NewGuid();

            var newValue = _guidKeyClassWriter.UpdateEnforcingNew(key, tv => { tv.Value += 1; });

            var defaultValue = default(int);

            (defaultValue + 1).Should().Be(newValue.Value);
        }
    }
    
    public class Test1
    {
        public int Value { get; set; }
    }
}
