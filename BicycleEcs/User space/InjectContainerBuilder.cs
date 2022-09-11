using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BicycleEcs
{
    public interface IInjectContainerBuilder : IEcsContainerBuilder
    {
        IInjectContainerBuilder AddSystem<T>()
        where T : IEcsSystem;
        IInjectContainerBuilder AddSystem(Type type);
    }

    public static class Extensions
    {
        public static IInjectContainerBuilder UseSystemInjection(this IEcsContainerBuilder builder, IServiceProvider provider)
        => new InjectContainerBuilder(builder, provider);
    }

    internal class InjectContainerBuilder : IInjectContainerBuilder
    {
        private readonly IEcsContainerBuilder builder;

        private IServiceProvider provider;

        public InjectContainerBuilder(IEcsContainerBuilder builder, IServiceProvider provider)
        {
            this.provider = provider;
            this.builder = builder;
        }

        public IInjectContainerBuilder AddSystem<T>()
        where T : IEcsSystem => AddSystem(typeof(T));

        public IInjectContainerBuilder AddSystem(Type type)
        {
            builder.AddSystem((IEcsSystem)provider.GetRequiredService(type));

            return this;
        }

        public IEcsContainerBuilder AddSystem(IEcsSystem system)
        {
            builder.AddSystem(system);

            return this;
        }

        public IEcsContainer Build() => builder.Build();

    }
}
