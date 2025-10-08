using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vexillum.util
{
    public class ConcurrentHashSet<T> : IEnumerable<T>, ISet<T>, ICollection<T>
    {
        private HashSet<T> set;
        public object mutex = new object();
        public ConcurrentHashSet()
        {
            set = new HashSet<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return set.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        public bool Add(T item)
        {
            bool elementWasAdded;

            lock (this.mutex)
            {
                elementWasAdded = set.Add(item);
            }

            return elementWasAdded;
        }
        public bool Remove(T item)
        {
            bool foundAndRemoved;

            lock (this.mutex)
            {
                foundAndRemoved = set.Remove(item);
            }

            return foundAndRemoved;
        }


        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            lock (mutex)
            {
                set.Clear();
            }
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return set.Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}
