using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs
{
    public interface IPoolsList : IDisposable
    {
        int PoolsCount { get; }

        event Action<IComponentPool> OnAddPool;

        void EnsureCreated<T>() where T : struct;
        IComponentPool<T> GetComponentPool<T>() where T : struct;
        int GetIndexOfPool<T>() where T : struct;
        IComponentPool GetPoolByIndex(int index);
        void Init(IEntitiesManager entityManager);
    }

    public class PoolsList : IPoolsList
    {
        private Dictionary<Type, int> poolsIndexes;
        private IComponentPool[] pools;

        public int PoolsCount { get; private set; } = 0;

        private IEntitiesManager entityManager;

        private int indexesSize;

        public event Action<IComponentPool> OnAddPool;

        public PoolsList(int capacity = 32)
        {
            poolsIndexes = new(capacity);
            pools = new IComponentPool[capacity];
        }

        public void Init(IEntitiesManager entityManager)
        {
            this.entityManager = entityManager;
            entityManager.OnPoolResize += ResizePools;
        }

        public void EnsureCreated<T>()
            where T : struct
        {
            if (poolsIndexes.ContainsKey(typeof(T)))
                return;

            CreatePool<T>();
        }

        private void CreatePool<T>() where T : struct
        {
            IComponentPool<T> pool = new ComponentPool<T>(indexesSize);

            if (PoolsCount == pools.Length)
                Array.Resize(ref pools, pools.Length << 1);
            poolsIndexes[typeof(T)] = PoolsCount;
            pools[PoolsCount++] = pool;
        }

        public int GetIndexOfPool<T>()
            where T : struct
        {
            EnsureCreated<T>();

            return poolsIndexes[typeof(T)];
        }

        public IComponentPool GetPoolByIndex(int index)
        {
            return pools[index];
        }

        public IComponentPool<T> GetComponentPool<T>()
            where T : struct
        {
            EnsureCreated<T>();

            return (IComponentPool<T>)GetPoolByIndex(GetIndexOfPool<T>());
        }

        private void ResizePools(int newSize)
        {
            indexesSize = newSize;

            for (int i = 0; i < PoolsCount; i++)
                pools[i].ResizePool(newSize);
        }

        public void Dispose()
        {
            if (entityManager != null)
                entityManager.OnPoolResize -= ResizePools;
            entityManager = null;

            poolsIndexes?.Clear();
            poolsIndexes = null;

            if (pools != null)
                for (int i = 0; i < PoolsCount; i++)
                    pools[i].Dispose();
            pools = null;
        }
    }
}
