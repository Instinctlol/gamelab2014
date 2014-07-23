using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;

namespace GestureLib
{
    /// <summary>
    /// Abstract base collection, which methods can be overriden with a full support to List&lt;T&gt;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GenericBaseCollection<T> : IList<T>
    {
        private List<T> _innerList = new List<T>();

        #region IList<T> Members

        public virtual int IndexOf(T item)
        {
            return _innerList.IndexOf(item);
        }

        public virtual void Insert(int index, T item)
        {
            _innerList.Insert(index, item);
        }

        public virtual void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        public virtual T this[int index]
        {
            get
            {
                return _innerList[index];
            }
            set
            {
                _innerList[index] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        public virtual void Add(T item)
        {
            _innerList.Add(item);
        }

        public virtual void Clear()
        {
            _innerList.Clear();
        }

        public virtual bool Contains(T item)
        {
            return _innerList.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public virtual int Count
        {
            get { return _innerList.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public virtual bool Remove(T item)
        {
            return _innerList.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public virtual IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        #endregion

        public virtual void AddRange(IEnumerable<T> collection)
        {
            _innerList.AddRange(collection);
        }

        public virtual ReadOnlyCollection<T> AsReadOnly()
        {
            return _innerList.AsReadOnly();
        }

        public virtual int BinarySearch(T item)
        {
            return _innerList.BinarySearch(item);
        }

        public virtual int BinarySearch(T item, IComparer<T> comparer)
        {
            return _innerList.BinarySearch(item, comparer);
        }

        public virtual int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return _innerList.BinarySearch(index, count, item, comparer);
        }

        public virtual List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            return _innerList.ConvertAll<TOutput>(converter);
        }

        public virtual void CopyTo(T[] array)
        {
            _innerList.CopyTo(array);
        }

        public virtual void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            _innerList.CopyTo(index, array, arrayIndex, count);
        }

        public virtual bool Exists(Predicate<T> match)
        {
            return _innerList.Exists(match);
        }

        public virtual T Find(Predicate<T> match)
        {
            return _innerList.Find(match);
        }

        public virtual List<T> FindAll(Predicate<T> match)
        {
            return _innerList.FindAll(match);
        }

        public virtual int FindIndex(Predicate<T> match)
        {
            return _innerList.FindIndex(match);
        }

        public virtual int FindIndex(int startIndex, Predicate<T> match)
        {
            return _innerList.FindIndex(startIndex, match);
        }

        public virtual int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return _innerList.FindIndex(startIndex, count, match);
        }

        public virtual T FindLast(Predicate<T> match)
        {
            return _innerList.FindLast(match);
        }

        public virtual int FindLastIndex(Predicate<T> match)
        {
            return _innerList.FindLastIndex(match);
        }

        public virtual int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return _innerList.FindLastIndex(startIndex, match);
        }

        public virtual int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return _innerList.FindLastIndex(startIndex, count, match);
        }

        public virtual void ForEach(Action<T> action)
        {
            _innerList.ForEach(action);
        }

        public virtual List<T> GetRange(int index, int count)
        {
            return _innerList.GetRange(index, count);
        }

        public virtual int IndexOf(T item, int index)
        {
            return _innerList.IndexOf(item, index);
        }

        public virtual int IndexOf(T item, int index, int count)
        {
            return _innerList.IndexOf(item, index, count);
        }

        public virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            _innerList.InsertRange(index, collection);
        }

        public virtual int LastIndexOf(T item)
        {
            return _innerList.LastIndexOf(item);
        }

        public virtual int LastIndexOf(T item, int index)
        {
            return _innerList.LastIndexOf(item, index);
        }

        public virtual int LastIndexOf(T item, int index, int count)
        {
            return _innerList.LastIndexOf(item, index, count);
        }

        public virtual int RemoveAll(Predicate<T> match)
        {
            return _innerList.RemoveAll(match);
        }

        public virtual void RemoveRange(int index, int count)
        {
            _innerList.RemoveRange(index, count);
        }

        public virtual void Reverse()
        {
            _innerList.Reverse();
        }

        public virtual void Reverse(int index, int count)
        {
            _innerList.Reverse(index, count);
        }

        public virtual void Sort()
        {
            _innerList.Sort();
        }

        public virtual void Sort(Comparison<T> comparison)
        {
            _innerList.Sort(comparison);
        }

        public virtual void Sort(IComparer<T> comparer)
        {
            _innerList.Sort(comparer);
        }

        public virtual void Sort(int index, int count, IComparer<T> comparer)
        {
            _innerList.Sort(index, count, comparer);
        }

        public virtual T[] ToArray()
        {
            return _innerList.ToArray();
        }

        public virtual void TrimExcess()
        {
            _innerList.TrimExcess();
        }

        public virtual bool TrueForAll(Predicate<T> match)
        {
            return _innerList.TrueForAll(match);
        }
    }
}
