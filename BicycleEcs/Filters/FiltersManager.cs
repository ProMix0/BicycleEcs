using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs
{
    public interface IFiltersManager : IDisposable
    {
        ComponentsMask Filter();
        IEcsFilter GetFilter(ComponentsMask mask);
        void Init(IPoolsList poolsList, IEntitiesManager entitiesManager);
    }

    public class FiltersManager : IFiltersManager
    {
        private SortedDictionary<ComponentsMask, IEcsFilter> filters;

        private IPoolsList poolsList;
        private IEntitiesManager entitiesManager;

        public FiltersManager()
        {
            filters = new();
        }

        public void Init(IPoolsList poolsList, IEntitiesManager entitiesManager)
        {
            this.poolsList = poolsList;
            this.entitiesManager = entitiesManager;
        }

        public ComponentsMask Filter()
        {
            return new(poolsList, this);
        }

        public IEcsFilter GetFilter(ComponentsMask mask)
        {
            if (filters.TryGetValue(mask, out IEcsFilter result))
                return result;

            result = new EcsFilter(mask, poolsList, entitiesManager);
            filters.Add(mask, result);
            return result;
        }

        public void Dispose()
        {
            if (filters != null)
                foreach (var filter in filters.Values)
                    filter.Dispose();
            filters?.Clear();
            filters = null;

            poolsList = null;
        }
    }
}
