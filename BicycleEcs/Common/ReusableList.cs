using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs
{
    internal class ReusableList<T>
        where T : class
    {
        private T[] array;
        private int itemsIndex;

        private Queue<int> deleted = new();

        public T this[int i]
        {
            get
            {
                return array[i];
            }
            set
            {
                array[i] = value;
            }
        }

        public int Capacity => array.Length;

        public ReusableList(int capacity)
        {
            array = new T[capacity];
            itemsIndex = 0;
        }

        public T Create(out int index)
        {
            if (deleted.TryDequeue(out index))
            {
                array[index] = null;
                return array[index];
            }

            EnsureSize(++itemsIndex);

            index = itemsIndex;
            return array[itemsIndex];
        }

        public int Create()
        {
            if (deleted.TryDequeue(out int index))
            {
                array[index] = null;
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
    }
}
