using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs
{
    public struct ComponentsMask : IComparable<ComponentsMask>
    {
        private readonly IPoolsList poolsList;
        private readonly IFiltersManager filtersManager;
        public List<int> include;
        public List<int> exclude;

        public ComponentsMask(IPoolsList poolsList, IFiltersManager filtersManager)
        {
            include = new(8);
            exclude = new(4);
            this.poolsList = poolsList;
            this.filtersManager = filtersManager;
        }

        public ComponentsMask With<T>()
            where T : struct
        {
            include.Add(poolsList.GetIndexOfPool<T>());

            return this;
        }

        public ComponentsMask Without<T>()
            where T : struct
        {
            exclude.Add(poolsList.GetIndexOfPool<T>());

            return this;
        }

        public IEcsFilter Build()
        {
            return filtersManager.GetFilter(this);
        }

        public int CompareTo(ComponentsMask other)
        {
            int diff = include.Count - other.include.Count;
            if (diff != 0) return diff;
            diff = exclude.Count - other.exclude.Count;
            if (diff != 0) return diff;

            for (int i = 0; i < include.Count; i++)
            {
                diff = include[i] - other.include[i];
                if (diff != 0) return diff;
            }
            for (int i = 0; i < exclude.Count; i++)
            {
                diff = exclude[i] - other.exclude[i];
                if (diff != 0) return diff;
            }

            return 0;
        }
    }
}
