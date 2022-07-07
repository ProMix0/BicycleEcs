namespace BicycleEcs
{
    public interface IEcsFilter:IDisposable
    {
        IEnumerable<int> FilteredEntities();
    }

    public class EcsFilter : IEcsFilter
    {
        private int[] include;
        private int[] exclude;
        private IPoolsList poolsList;

        private List<int> filteredEntities = new(32);

        public EcsFilter(ComponentsMask mask, IPoolsList poolsList)
        {
            include = mask.include.ToArray();
            exclude = mask.exclude.ToArray();

            this.poolsList = poolsList;

            for (int i = 0; i < include.Length; i++)
            {
                IComponentPool pool = poolsList.GetPoolByIndex(i);
                pool.OnEntityAdded += OnIncludeAdd;
                pool.OnEntityRemoved += OnIncludeRemove;
            }
            for (int i = 0; i < exclude.Length; i++)
            {
                IComponentPool pool = poolsList.GetPoolByIndex(i);
                pool.OnEntityAdded += OnExcludeAdd;
                pool.OnEntityRemoved += OnExcludeRemove;
            }
        }

        private void OnIncludeAdd(int entity)
        {
            for (int i = 0; i < include.Length; i++)
                if (!poolsList.GetPoolByIndex(i).HasComponent(entity))
                    return;

            for (int i = 0; i < exclude.Length; i++)
                if (poolsList.GetPoolByIndex(i).HasComponent(entity))
                    return;

            filteredEntities.Add(entity);
        }

        private void OnIncludeRemove(int entity)
        {
            filteredEntities.Remove(entity);
        }

        private void OnExcludeAdd(int entity)
        {
            filteredEntities.Remove(entity);
        }

        private void OnExcludeRemove(int entity)
        {
            for (int i = 0; i < include.Length; i++)
                if (!poolsList.GetPoolByIndex(i).HasComponent(entity))
                    return;

            for (int i = 0; i < exclude.Length; i++)
                if (poolsList.GetPoolByIndex(i).HasComponent(entity))
                    return;

            filteredEntities.Add(entity);
        }

        public IEnumerable<int> FilteredEntities()
        {
            return filteredEntities;
        }

        public void Dispose()
        {
            if (poolsList != null)
            {
                for (int i = 0; i < include.Length; i++)
                {
                    IComponentPool pool = poolsList.GetPoolByIndex(i);
                    pool.OnEntityAdded -= OnIncludeAdd;
                    pool.OnEntityRemoved -= OnIncludeRemove;
                }
                for (int i = 0; i < exclude.Length; i++)
                {
                    IComponentPool pool = poolsList.GetPoolByIndex(i);
                    pool.OnEntityAdded -= OnExcludeAdd;
                    pool.OnEntityRemoved -= OnExcludeRemove;
                }
            }

            poolsList = null;
            include = null;
            exclude = null;

            filteredEntities?.Clear();
            filteredEntities = null;
        }
    }
}