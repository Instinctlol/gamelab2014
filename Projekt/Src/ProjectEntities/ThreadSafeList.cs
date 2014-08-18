using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public sealed class ThreadSafeList<T>
    {
        private LinkedList<T> m_list = new LinkedList<T>();
        private object m_lock = new object();

        public void AddLast(T value)
        {
            lock (m_lock)
            {
                m_list.AddLast(value);
            }
        }

        public bool TryRemove(T value)
        {
            lock (m_lock)
            {
                if (m_list.Contains<T>(value))
                {
                    return m_list.Remove(value);
                }
                return false;
            }
        }

        public void TryRemoveFirst()
        {
            lock (m_lock)
            {
                if (m_list.Count > 0)
                {
                    m_list.RemoveFirst();
                }
            }
        }

        public bool TryGet(int index, out T value)
        {
            lock (m_lock)
            {
                if (index < m_list.Count)
                {
                    value = m_list.ElementAt(index);
                    return true;
                }
                value = default(T);
                return false;
            }
        }

        public int Count()
        {
            return m_list.Count();
        }

        public bool Contains(T value)
        {
            lock (m_lock)
            {
                return m_list.Contains<T>(value);
            }
        }
    }
}
