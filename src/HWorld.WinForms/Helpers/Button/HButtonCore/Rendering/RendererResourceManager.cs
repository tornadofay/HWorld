using System;
using System.Collections.Generic;

namespace HWorld.WinForms.Helpers.Button.HButtonCore.Rendering
{
    /// <summary>
    /// Manages renderer resource pools. 
    /// Lifetime: dispose when no longer needed (e.g., Form.Close, Application.Exit).
    /// Thread-safety: not thread-safe; use one instance per thread or external synchronization.
    /// Retained references: renderers hold references until disposed; provider owns lifetime.
    /// </summary>
    public sealed class RendererResourceManager : IRendererResourceProvider, IDisposable
    {
        private readonly Dictionary<Type, HButtonRendererResources> _resources = new Dictionary<Type, HButtonRendererResources>();
        public static RendererResourceManager Global { get; } = new RendererResourceManager();

        public T GetOrCreate<T>() where T : HButtonRendererResources, new()
        {
            if (!_resources.TryGetValue(typeof(T), out var res)) { res = new T(); _resources.Add(typeof(T), res); }
            return (T)res;
        }
        public void Dispose() { foreach (var r in _resources.Values) r.Dispose(); _resources.Clear(); }
    }
}
