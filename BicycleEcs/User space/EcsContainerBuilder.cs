using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs
{
    public interface IEcsContainerBuilder
    {
        IEcsContainerBuilder AddSystem(IEcsSystem system);
        IEcsContainer Build();
    }

    public class EcsContainerBuilder : IEcsContainerBuilder
    {
        private readonly IEcsWorld world;
        private List<IEcsSystem> systems;

        public EcsContainerBuilder()
        {
            world = new EcsWorld();
            systems = new(64);
        }

        public IEcsContainerBuilder AddSystem(IEcsSystem system)
        {
            systems.Add(system);

            return this;
        }

        public IEcsContainer Build()
        {
            return new EcsContainer(world, systems.ToArray());
        }
    }
}
