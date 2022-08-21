using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs
{
    public interface IEcsWorld : IDisposable
    {
        IEntitiesManager EntitiesManager { get; }
        IFiltersManager FiltersManager { get; }
        IPoolsList PoolsList { get; }

        void Init();
    }

    public class EcsWorld : IEcsWorld
    {
        public IEntitiesManager EntitiesManager { get; private set; }
        public IFiltersManager FiltersManager { get; private set; }
        public IPoolsList PoolsList { get; private set; }

        private bool inited = false;

        public EcsWorld()
        {
            EntitiesManager = new EntitiesManager();
            FiltersManager = new FiltersManager();
            PoolsList = new PoolsList();
        }

        public void Init()
        {
            if (inited) throw new InvalidOperationException("Instance already initiated");

            EntitiesManager.Init(PoolsList);
            PoolsList.Init(EntitiesManager);
            FiltersManager.Init(PoolsList, EntitiesManager);

            inited = true;
        }

        public void Dispose()
        {
            EntitiesManager?.Dispose();
            FiltersManager?.Dispose();
            PoolsList?.Dispose();

            EntitiesManager = null!;
            FiltersManager = null!;
            PoolsList = null!;
        }
    }
}
