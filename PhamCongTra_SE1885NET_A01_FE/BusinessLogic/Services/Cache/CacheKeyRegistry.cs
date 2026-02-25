using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BusinessLogic.Services
{
    public interface ICacheKeyRegistry
    {
        void Track(string endpoint, string cacheKey);

        IReadOnlyCollection<string> ExtractKeys(string endpoint);
    }

    public class CacheKeyRegistry : ICacheKeyRegistry
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _registry = new(StringComparer.OrdinalIgnoreCase);

        public void Track(string endpoint, string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(cacheKey))
            {
                return;
            }

            var normalized = Normalize(endpoint);
            if (string.IsNullOrEmpty(normalized))
            {
                return;
            }

            var cacheSet = _registry.GetOrAdd(normalized, _ => new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase));
            cacheSet.TryAdd(cacheKey, 0);
        }

        public IReadOnlyCollection<string> ExtractKeys(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return Array.Empty<string>();
            }

            var normalized = Normalize(endpoint);
            if (string.IsNullOrEmpty(normalized))
            {
                return Array.Empty<string>();
            }

            if (_registry.TryRemove(normalized, out var cacheSet))
            {
                return cacheSet.Keys.ToArray();
            }

            return Array.Empty<string>();
        }

        private static string Normalize(string endpoint)
        {
            var trimmed = endpoint.Trim();
            if (trimmed.Length == 0)
            {
                return string.Empty;
            }

            var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
            if (queryIndex >= 0)
            {
                trimmed = trimmed[..queryIndex];
            }

            return trimmed.TrimEnd('/').ToLowerInvariant();
        }
    }
}