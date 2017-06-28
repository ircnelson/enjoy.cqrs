using Autofac;
using EnjoyCQRS.Core;
using EnjoyCQRS.DependencyInjection.AutofacExtensions;
using FluentAssertions;
using System;
using Xunit;

namespace EnjoyCQRS.DependencyInjection.AutofacTests
{
    public class DependencyRegisterTests
    {
        private readonly ContainerBuilder _builder;

        public DependencyRegisterTests()
        {
            _builder = new ContainerBuilder();
        }
        
        [Fact]
        public void Should_use_default_dependencies()
        {
            _builder.UseEnjoyCQRS();

            EnjoyDependencies enjoyDependencies = new EnjoyDependencies();

            Action act = () => _builder.Build();

            act.ShouldNotThrow();
        }
    }
}
