using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs.Internal
{
    internal class ReusableStructList<T>
        where T : struct
    {
        private T[] array;
        private int itemsIndex;

        private Queue<int> deleted = new();

        public ref T this[int i] => ref array[i];

        public int Capacity => array.Length;
        public int Count => itemsIndex + 1;

        public ReusableStructList(int capacity)
        {
            array = new T[capacity];
            itemsIndex = 0;
        }

        public ref T Create(out int index)
        {
            if (deleted.TryDequeue(out index))
            {
                array[index] = new();
                return ref array[index];
            }

            EnsureSize(++itemsIndex);

            index = itemsIndex;
            return ref array[itemsIndex];
        }

        public int Create()
        {
            if (deleted.TryDequeue(out int index))
            {
                array[index] = new();
                return index;
            }

            EnsureSize(++itemsIndex);

            return itemsIndex;
        }

        private void EnsureSize(int size)
        {
            Utils.EnsureSize(ref array, size);
        }

        public void Delete(int index)
        {
            deleted.Enqueue(index);
        }

        public void Clear()
        {
            array = Array.Empty<T>();
            deleted.Clear();
        }

        public IEnumerable<T> Enumerate()
        {
            //TODO
            for (int i = 0; i <= itemsIndex; i++)
                if (!deleted.Contains(i))
                    yield return array[i];
        }
    }
}
