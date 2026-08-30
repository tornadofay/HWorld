using System;
using System.Collections.Generic;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Cashes
{
    internal sealed class LruCache<TKey, TValue> : IDisposable
        where TValue : IDisposable
    {
        private sealed class Node { public TKey Key; public TValue Value; }

        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<Node>> _map;
        private readonly LinkedList<Node> _lru = new LinkedList<Node>();

        public LruCache(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
            _map = new Dictionary<TKey, LinkedListNode<Node>>(capacity);
        }

        public int Count => _map.Count;

        public bool TryGet(TKey key, out TValue value)
        {
            if (_map.TryGetValue(key, out var node))
            {
                _lru.Remove(node);
                _lru.AddFirst(node);          // promote to most-recently-used
                value = node.Value.Value;
                return true;
            }
            value = default;
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            if (_map.TryGetValue(key, out var existing))
            {
                existing.Value.Value.Dispose();
                _lru.Remove(existing);
                _map.Remove(key);
            }

            var node = new LinkedListNode<Node>(new Node { Key = key, Value = value });
            _lru.AddFirst(node);
            _map.Add(key, node);

            while (_map.Count > _capacity)       // evict least-recently-used
            {
                var last = _lru.Last;
                _lru.RemoveLast();
                _map.Remove(last.Value.Key);
                last.Value.Value.Dispose();
            }
        }

        public void Clear()
        {
            foreach (var node in _lru)
                node.Value.Dispose();

            _lru.Clear();
            _map.Clear();
        }

        public void Dispose()
        {
            foreach (var node in _lru) node.Value.Dispose();
            _lru.Clear();
            _map.Clear();
        }
    }
}
