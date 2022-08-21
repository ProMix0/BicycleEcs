using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs
{
    public interface IEntitiesManager : IDisposable
    {
        event Action<int> OnPoolResize;

        int CreateEntity();
        void DeleteEntity(int entity);
        void Init(IPoolsList poolsList);
        IEnumerable<int> Enumerate();
    }

    public class EntitiesManager : IEntitiesManager
    {
        private ReusableStructList<EntityInfo> entities = null!;

        public event Action<int> OnPoolResize = null!;

        private IPoolsList poolsList = null!;

        private Queue<int> noComponents = new();

        public EntitiesManager(int capacity = 128)
        {
            entities = new(capacity);
        }

        public void Init(IPoolsList poolsList)
        {
            this.poolsList = poolsList;

            poolsList.OnAddPool += OnAddPool;
        }

        private void OnAddPool(IComponentPool pool)
        {
            pool.OnEntityAdded += OnAddComponent;
            pool.OnEntityRemoved += OnRemoveComponent;
        }

        public int CreateEntity()
        {
            DeleteNoComponentsEntities(5);

            int poolSize = entities.Capacity;

            int entity = entities.Create();
            entities[entity] = new() { alive = true, componentsCount = 0 };
            if (poolSize != entities.Capacity)
                OnPoolResize?.Invoke(entities.Capacity);

            return entity;
        }

        public void DeleteEntity(int entity)
        {
            for (int i = 0; i < poolsList.PoolsCount; i++)
                poolsList.GetPoolByIndex(i).DeleteComponent(entity);
        }

        private void DeleteNoComponentsEntities(int count)
        {
            for (int i = 0; i < count && noComponents.TryDequeue(out int entity); i++)
                if (entities[entity].componentsCount == 0 && entities[entity].alive)
                {
                    entities[entity].alive = false;
                    entities.Delete(entity);
                }
        }

        private void OnAddComponent(int entity)
        {
            entities[entity].componentsCount++;
        }

        private void OnRemoveComponent(int entity)
        {

            if (--entities[entity].componentsCount == 0)
                noComponents.Enqueue(entity);
        }

        public void Dispose()
        {
            entities?.Clear();
            entities = null!;

            if (poolsList != null)
                poolsList.OnAddPool -= OnAddPool;
            poolsList = null!;

            noComponents?.Clear();
            noComponents = null!;
        }

        public IEnumerable<int> Enumerate()
        {
            for (int i = 1; i < entities.Count; i++)
                if (entities[i].alive)
                    yield return i;
        }
    }

    internal struct EntityInfo
    {
        public bool alive;
        public int componentsCount;

        public EntityInfo()
        {
            alive = false;
            componentsCount = 0;
        }
    }
}
