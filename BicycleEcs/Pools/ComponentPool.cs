namespace BicycleEcs
{
    public interface IComponentPool : IDisposable
    {
        void DeleteComponent(int entity);
        bool HasComponent(int entity);
        void ResizePool(int newSize);

        event Action<int> OnEntityAdded;

        event Action<int> OnEntityRemoved;
    }

    public interface IComponentPool<T> : IComponentPool
        where T : struct
    {
        ref T AddComponent(int entity);
        ref T GetComponent(int entity);
        bool TryGetComponent(int entity, ref T component);
    }

    public class ComponentPool<T> : IComponentPool<T>
        where T : struct
    {
        // 1-based indexes
        private ReusableStructList<T> components;
        private int[] indexes;

        public event Action<int> OnEntityAdded = null!;
        public event Action<int> OnEntityRemoved = null!;

        public ComponentPool(int capacity)
        {
            components = new(32);
            indexes = new int[capacity];
        }

        public ref T AddComponent(int entity)
        {
            if (indexes[entity] <= 0)
            {
                ref T temp = ref components.Create(out indexes[entity]);
                OnEntityAdded?.Invoke(entity);
                return ref temp;
            }

            return ref components[indexes[entity]];
        }

        public ref T GetComponent(int entity)
        {
            // Return default (components[0]) value if not contain component
            return ref components[indexes[entity]];
        }

        public bool TryGetComponent(int entity, ref T component)
        {
            if (indexes[entity] > 0)
            {
                component = ref components[indexes[entity]];
                return true;
            }
            return false;
        }

        public bool HasComponent(int entity)
        {
            return indexes[entity] > 0;
        }

        public void DeleteComponent(int entity)
        {
            if (indexes[entity] <= 0) return;

            components.Delete(indexes[entity]);
            indexes[entity] = 0;

            OnEntityRemoved?.Invoke(entity);
        }

        public void ResizePool(int newSize)
        {
            Array.Resize(ref indexes, newSize);
        }

        public void Dispose()
        {
            components?.Clear();
            components = null!;

            indexes = null!;
        }
    }
}