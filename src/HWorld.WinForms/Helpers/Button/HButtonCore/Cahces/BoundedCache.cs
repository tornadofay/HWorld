using System;
using System.Collections.Generic;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Cashes
{
    internal sealed class BoundedCache<TKey, TValue> : IDisposable
        where TValue : IDisposable
    {
        private readonly int _max;
        private readonly int _evictionThreshold;
        private readonly Dictionary<TKey, TValue> _map;
        private readonly LinkedList<TKey> _insertionOrder = new LinkedList<TKey>();

        public BoundedCache(int max)
        {
            _max = max;
            _evictionThreshold = (int)(max * 0.75);
            _map = new Dictionary<TKey, TValue>(max);
        }

        public int Count => _map.Count;

        public bool TryGet(TKey key, out TValue value) => _map.TryGetValue(key, out value);

        public void Add(TKey key, TValue value)
        {
            if (_map.TryGetValue(key, out var existing))
            {
                existing.Dispose();
                _map[key] = value;
                _insertionOrder.Remove(key);
                _insertionOrder.AddFirst(key);
                return;
            }

            // Evict oldest entries when we hit 75% capacity to avoid complete cache collapse.
            if (_map.Count >= _evictionThreshold)
            {
                int toRemove = _map.Count - _evictionThreshold + 1;
                for (int i = 0; i < toRemove && _insertionOrder.Count > 0; i++)
                {
                    var oldest = _insertionOrder.Last.Value;
                    _insertionOrder.RemoveLast();
                    if (_map.TryGetValue(oldest, out var old)) old.Dispose();
                    _map.Remove(oldest);
                }
            }

            _map.Add(key, value);
            _insertionOrder.AddFirst(key);
        }

        public void Dispose()
        {
            foreach (var v in _map.Values) v.Dispose();
            _map.Clear();
            _insertionOrder.Clear();
        }
    }
}
