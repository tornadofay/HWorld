using System;
using System.Collections.Generic;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Cashes
{
    internal sealed class SimpleCache<TKey, TValue> : IDisposable
        where TValue : IDisposable
    {
        private readonly int _max;
        private readonly Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();

        public SimpleCache(int max) => _max = max;

        public bool TryGet(TKey key, out TValue value) => _map.TryGetValue(key, out value);

        public void Add(TKey key, TValue value)
        {
            if (_map.TryGetValue(key, out var existing))
            {
                existing.Dispose();
                _map[key] = value;
                return;
            }

            if (_map.Count >= _max)
            {
                foreach (var v in _map.Values) v.Dispose();
                _map.Clear();
            }
            _map.Add(key, value);
        }

        public void Dispose()
        {
            foreach (var v in _map.Values) v.Dispose();
            _map.Clear();
        }
    }
}
