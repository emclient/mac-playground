using System;
using System.Collections.Generic;

namespace System.Drawing
{
	class LruCache<TKey, TValue>
	{
		private int capacity;
		private Dictionary<TKey, LinkedListNode<CacheItem>> cacheMap = new Dictionary<TKey, LinkedListNode<CacheItem>>();
		private LinkedList<CacheItem> lruList = new LinkedList<CacheItem>();
		private object listLock = new object();

		public LruCache(int capacity)
		{
			this.capacity = capacity;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			lock (listLock)
			{
				LinkedListNode<CacheItem> node;
				if (cacheMap.TryGetValue(key, out node))
				{
					value = node.Value.Value;
					lruList.Remove(node);
					lruList.AddLast(node);
					return true;
				}
				value = default(TValue);
				return false;
			}
		}

		public void Set(TKey key, TValue value)
		{
			lock (listLock)
			{
				LinkedListNode<CacheItem> node;
				if (cacheMap.TryGetValue(key, out node))
				{
					node.Value.Value = value;
				    lruList.Remove(node);
					lruList.AddLast(node);
					return;
				}

				if (cacheMap.Count >= capacity)
				{
					var firstNode = lruList.First;
					lruList.RemoveFirst();
					cacheMap.Remove(firstNode.Value.Key);
				}

				node = new LinkedListNode<CacheItem>(new CacheItem(key, value));
				lruList.AddLast(node);
				cacheMap.Add(key, node);
			}
		}

		public int Count
		{
			get { return cacheMap.Count; }
		}

		class CacheItem
		{
			public CacheItem(TKey k, TValue v)
			{
				Key = k;
				Value = v;
			}

			public TKey Key { get; private set; }
			public TValue Value { get; set; }
		}
	}
}
