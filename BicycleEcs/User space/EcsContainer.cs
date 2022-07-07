using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs
{
    public interface IEcsContainer : IDisposable
    {
        void Init();
        void Run();
    }

    public class EcsContainer : IEcsContainer
    {
        private IEcsWorld world;
        private IEcsSystem[] ecsSystems;

        private IEcsRunSystem[] runSystems;

        public EcsContainer(IEcsWorld world, IEcsSystem[] ecsSystems)
        {
            this.world = world;
            this.ecsSystems = ecsSystems;

            runSystems = ecsSystems.Where(system => system is IEcsRunSystem).Cast<IEcsRunSystem>().ToArray();
        }

        public void Init()
        {
            world.Init();

            for (int i = 0; i < ecsSystems.Length; i++)
                if (ecsSystems[i] is IEcsInitSystem initSystem)
                    initSystem.Init(world);
        }

        public void Run()
        {
            for (int i = 0; i < runSystems.Length; i++)
                runSystems[i].Run();
        }

        public void Dispose()
        {
            if (ecsSystems != null)
                for (int i = 0; i < ecsSystems.Length; i++)
                    if (ecsSystems[i] is IEcsDestroySystem destroySystem)
                        destroySystem.Destroy(world);
            ecsSystems = null;
            runSystems = null;

            world?.Dispose();
            world = null;
        }
    }
}
