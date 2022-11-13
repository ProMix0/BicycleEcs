using System.Collections;

namespace BicycleEcs
{
    public interface IEcsFilter : IDisposable, IEnumerable<int>
    {
    }

    public class EcsFilter : IEcsFilter
    {
        private int[] include;
        private int[] exclude;
        private IPoolsList poolsList;

        private List<int> filteredEntities = new(32);
        private int locksCount = 0;
        private Queue<DelayedEntity> delayed = new(16);

        public EcsFilter(ComponentsMask mask, IPoolsList poolsList, IEntitiesManager entitiesManager)
        {
            include = mask.include.ToArray();
            exclude = mask.exclude.ToArray();

            this.poolsList = poolsList;

            for (int i = 0; i < include.Length; i++)
            {
                IComponentPool pool = poolsList.GetPoolByIndex(include[i]);
                pool.OnEntityAdded += OnIncludeAdd;
                pool.OnEntityRemoved += OnIncludeRemove;
            }
            for (int i = 0; i < exclude.Length; i++)
            {
                IComponentPool pool = poolsList.GetPoolByIndex(exclude[i]);
                pool.OnEntityAdded += OnExcludeAdd;
                pool.OnEntityRemoved += OnExcludeRemove;
            }

            filteredEntities.AddRange(entitiesManager.Enumerate().Where(IsCompatible));
        }

        private void OnIncludeAdd(int entity)
        {
            if (IsCompatible(entity))
                if (locksCount == 0)
                    filteredEntities.Add(entity);
                else
                    delayed.Enqueue(new(OperationType.IncludeAdd, entity));
        }

        private void OnIncludeRemove(int entity)
        {
            if (locksCount == 0)
                filteredEntities.Remove(entity);
            else
                delayed.Enqueue(new(OperationType.IncludeRemove, entity));
        }

        private void OnExcludeAdd(int entity)
        {
            if (locksCount == 0)
                filteredEntities.Remove(entity);
            else
                delayed.Enqueue(new(OperationType.ExcludeAdd, entity));
        }

        private void OnExcludeRemove(int entity)
        {
            if (IsCompatible(entity))
                if (locksCount == 0)
                    filteredEntities.Add(entity);
                else
                    delayed.Enqueue(new(OperationType.ExcludeRemove, entity));
        }

        private void HandleDelayedEntities()
        {
            if (locksCount == 0)
            {
                while (delayed.TryDequeue(out DelayedEntity entity))
                {
                    switch (entity.operation)
                    {
                        case OperationType.IncludeAdd:
                            OnIncludeAdd(entity.entity);
                            break;
                        case OperationType.IncludeRemove:
                            OnIncludeRemove(entity.entity);
                            break;
                        case OperationType.ExcludeAdd:
                            OnExcludeAdd(entity.entity);
                            break;
                        case OperationType.ExcludeRemove:
                            OnExcludeRemove(entity.entity);
                            break;
                    };
                }
            }
        }

        private bool IsCompatible(int entity)
        {
            for (int i = 0; i < include.Length; i++)
                if (!poolsList.GetPoolByIndex(include[i]).HasComponent(entity))
                    return false;

            for (int i = 0; i < exclude.Length; i++)
                if (poolsList.GetPoolByIndex(exclude[i]).HasComponent(entity))
                    return false;

            return true;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new FixingEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new FixingEnumerator(this);
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

            poolsList = null!;
            include = null!;
            exclude = null!;

            filteredEntities?.Clear();
            filteredEntities = null!;
        }

        private class FixingEnumerator : IEnumerator<int>
        {
            private bool disposed;
            private readonly EcsFilter filter;
            private readonly IEnumerator<int> enumerator;

            public int Current => enumerator.Current;

            object IEnumerator.Current => enumerator.Current;

            public FixingEnumerator(EcsFilter filter)
            {
                this.filter = filter;
                enumerator = filter.filteredEntities.GetEnumerator();

                filter.locksCount++;
            }

            public bool MoveNext() => enumerator.MoveNext();


            public void Reset() => enumerator.Reset();

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;

                enumerator.Dispose();

                filter.locksCount--;
                filter.HandleDelayedEntities();
            }
        }

        private struct DelayedEntity
        {
            public DelayedEntity(OperationType operation, int entity)
            {
                this.operation = operation;
                this.entity = entity;
            }

            public readonly OperationType operation;
            public readonly int entity;
        }

        private enum OperationType
        {
            IncludeAdd,
            IncludeRemove,
            ExcludeAdd,
            ExcludeRemove
        }
    }
}