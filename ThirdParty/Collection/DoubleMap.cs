﻿namespace ET
{
	public class DoubleMap<K, V>
	{
		private readonly Dictionary<K, V> kv = new();
		private readonly Dictionary<V, K> vk = new();

		public DoubleMap()
		{
		}

		public DoubleMap(int capacity)
		{
			kv = new Dictionary<K, V>(capacity);
			vk = new Dictionary<V, K>(capacity);
		}

		public void ForEach(Action<K, V> action)
		{
			if (action == null)
			{
				return;
			}
			Dictionary<K, V>.KeyCollection keys = kv.Keys;
			foreach (K key in keys)
			{
				action(key, kv[key]);
			}
		}

		public List<K> Keys => [..kv.Keys];

		public List<V> Values => [..vk.Keys];

		public void Add(K key, V value)
		{
			if (key == null || value == null || kv.ContainsKey(key) || vk.ContainsKey(value))
			{
				throw new Exception($"kv error or existed: {key} {value}");
			}
			kv.Add(key, value);
			vk.Add(value, key);
		}

		public V GetValueByKey(K key)
		{
			if (key != null && this.kv.TryGetValue(key, out V byKey))
			{
				return byKey;
			}
			return default;
		}

		public K GetKeyByValue(V value)
		{
			if (value != null && this.vk.TryGetValue(value, value: out K byValue))
			{
				return byValue;
			}
			return default;
		}

		public Dictionary<K, V> GetAll()
		{
			return this.kv;
		}

		public void RemoveByKey(K key)
		{
			if (key == null)
			{
				return;
			}

			if (!this.kv.Remove(key, out V value))
			{
				return;
			}

			vk.Remove(value);
		}

		public void RemoveByValue(V value)
		{
			if (value == null)
			{
				return;
			}

			if (!vk.TryGetValue(value, out K key))
			{
				return;
			}

			kv.Remove(key);
			vk.Remove(value);
		}

		public void Clear()
		{
			kv.Clear();
			vk.Clear();
		}

		public bool ContainsKey(K key)
		{
			if (key == null)
			{
				return false;
			}
			return kv.ContainsKey(key);
		}

		public bool ContainsValue(V value)
		{
			if (value == null)
			{
				return false;
			}
			return vk.ContainsKey(value);
		}

		public bool Contains(K key, V value)
		{
			if (key == null || value == null)
			{
				return false;
			}
			return kv.ContainsKey(key) && vk.ContainsKey(value);
		}
	}
}