﻿namespace ET
{
    public class MultiMapSet<T, K>: Dictionary<T, HashSet<K>>
    {
        private readonly HashSet<K> Empty = new HashSet<K>();
		
        public void Add(T t, K k)
        {
            this.TryGetValue(t, out var list);
            if (list == null)
            {
                list = new HashSet<K>();
                this.Add(t, list);
            }
            list.Add(k);
        }

        public bool Remove(T t, K k)
        {
            this.TryGetValue(t, out var list);
            if (list == null)
            {
                return false;
            }
            if (!list.Remove(k))
            {
                return false;
            }
            if (list.Count == 0)
            {
                this.Remove(t);
            }
            return true;
        }

        /// <summary>
        /// 不返回内部的list,copy一份出来
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public K[] GetAll(T t)
        {
            this.TryGetValue(t, out var list);
            if (list == null)
            {
                return Array.Empty<K>();
            }
            return list.ToArray();
        }

        /// <summary>
        /// 返回内部的list
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public new HashSet<K> this[T t]
        {
            get
            {
                this.TryGetValue(t, out var list);
                return list ?? Empty;
            }
        }

        public K GetOne(T t)
        {
            this.TryGetValue(t, out var list);
            if (list != null && list.Count > 0)
            {
                return list.FirstOrDefault();
            }
            return default;
        }

        public bool Contains(T t, K k)
        {
            this.TryGetValue(t, out var list);
            if (list == null)
            {
                return false;
            }
            return list.Contains(k);
        }
    }
}