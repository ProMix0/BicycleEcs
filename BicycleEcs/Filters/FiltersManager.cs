using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs
{
    public interface IFiltersManager:IDisposable
    {
        ComponentsMask Filter();
        IEcsFilter GetFilter(ComponentsMask mask);
        void Init(IPoolsList poolsList);
    }

    public class FiltersManager : IFiltersManager
    {
        private SortedDictionary<ComponentsMask, IEcsFilter> filters;

        private IPoolsList poolsList;

        public FiltersManager()
        {
            filters = new();
        }

        public void Init(IPoolsList poolsList)
        {
            this.poolsList = poolsList;
        }

        public ComponentsMask Filter()
        {
            return new(poolsList, this);
        }

        public IEcsFilter GetFilter(ComponentsMask mask)
        {
            if (filters.TryGetValue(mask, out IEcsFilter result))
                return result;

            result = new EcsFilter(mask, poolsList);
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
