using System;
using System.Collections.Generic;

namespace MailClient.Collections
{
    public class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        internal Entry[] table;
        internal const float loadFactor = 0.75f;
        internal int count;
        internal int threshold;

        public WeakDictionary()
            : this(1000)
        {
        }

        public WeakDictionary(int initialCapacity)
        {
            threshold = (int)(initialCapacity * loadFactor);
            table = new Entry[initialCapacity];
        }

        internal void Rehash()
        {
            int oldCapacity = table.Length;
            Entry[] oldMap = table;
            int i;
            for (i = oldCapacity; --i >= 0; )
            {
                Entry e, next, prev;
                for (prev = null, e = oldMap[i]; e != null; e = next)
                {
                    next = e.next;
                    TValue obj = (TValue)e.oref.Target;
                    if (obj == null)
                    {
                        count -= 1;
                        e.Clear();
                        if (prev == null)
                            oldMap[i] = next;
                        else
                            prev.next = next;
                    }
                    else
                    {
                        prev = e;
                    }
                }
            }

            if ((uint)count <= ((uint)threshold >> 1))
            {
                return;
            }
            int newCapacity = oldCapacity * 2 + 1;
            Entry[] newMap = new Entry[newCapacity];

            threshold = (int)(newCapacity * loadFactor);
            table = newMap;

            for (i = oldCapacity; --i >= 0; )
            {
                for (Entry old = oldMap[i]; old != null; )
                {
                    Entry e = old;
                    old = old.next;

                    int index = ((int)e.key.GetHashCode() & 0x7FFFFFFF) % newCapacity;
                    e.next = newMap[index];
                    newMap[index] = e;
                }
            }
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            TValue oldValue;
            if (TryGetValue(key, out oldValue))
                throw new ArgumentException("Element already exists", "key");
            this[key] = value;
        }

        public bool ContainsKey(TKey key)
        {
            TValue value;
            return TryGetValue(key, out value);
        }

        public ICollection<TKey> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(TKey key)
        {
            Entry[] tab = table;
            int index = ((int)key.GetHashCode() & 0x7FFFFFFF) % tab.Length;
            for (Entry e = tab[index], prev = null; e != null; prev = e, e = e.next)
            {
                if (e.key.Equals(key))
                {
                    if (prev != null)
                        prev.next = e.next;
                    else
                        tab[index] = e.next;
                    e.Clear();
                    count -= 1;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Entry[] tab = table;
            int index = (key.GetHashCode() & 0x7FFFFFFF) % tab.Length;
            for (Entry e = tab[index]; e != null; e = e.next)
            {
                if (e.key.Equals(key))
                {
                    value = (TValue)e.oref.Target;
                    return value != null;
                }
            }
            value = default(TValue);
            return false;
        }

        public ICollection<TValue> Values
        {
            get { throw new NotImplementedException(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!TryGetValue(key, out value))
                    throw new ArgumentOutOfRangeException("key");
                return value;
            }
            set
            {
                Entry[] tab = table;
                int index = (key.GetHashCode() & 0x7FFFFFFF) % tab.Length;
                for (Entry e = tab[index]; e != null; e = e.next)
                {
                    if (e.key.Equals(key))
                    {
                        e.oref.Target = value;
                        return;
                    }
                }
                if (count >= threshold)
                {
                    // Rehash the table if the threshold is exceeded
                    Rehash();
                    tab = table;
                    index = ((int)key.GetHashCode() & 0x7FFFFFFF) % tab.Length;
                }

                // Creates the new entry.
                tab[index] = new Entry(key, new WeakReference(value), tab[index]);
                count++;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Entry[] tab = table;
            for (int i = 0; i < tab.Length; i++)
            {
                tab[i] = null;
            }
            count = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (Entry entry in this.table)
            {
                if (entry != null)
                {
                    TValue value = (TValue)entry.oref.Target;
                    if (value != null)
                        yield return new KeyValuePair<TKey, TValue>(entry.key, value);
                }
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        internal class Entry
        {
            internal Entry next;
            internal WeakReference oref;
            internal TKey key;

            internal void Clear()
            {
                oref.Target = null;
                oref = null;
                next = null;
            }

            internal Entry(TKey key, WeakReference oref, Entry chain)
            {
                next = chain;
                this.key = key;
                this.oref = oref;
            }
        }
    }
}


