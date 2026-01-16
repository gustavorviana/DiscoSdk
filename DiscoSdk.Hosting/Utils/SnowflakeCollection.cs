using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace DiscoSdk.Hosting.Utils
{
    internal class SnowflakeCollection<TModel> : IDictionary<Snowflake, TModel> where TModel : IWithSnowflake
    {
        private readonly ConcurrentDictionary<Snowflake, TModel> _dictionary = [];
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public TModel this[Snowflake key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public ICollection<Snowflake> Keys => _dictionary.Keys;

        public ICollection<TModel> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool IsReadOnly => false;

        public void Clear()
        {
            _semaphore.Wait();
            try
            {
                _dictionary.Clear();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<TModel> GetOrAddAsync(Snowflake id, Func<Snowflake, Task<TModel>> callback)
        {
            _semaphore.Wait();
            try
            {
                if (_dictionary.TryGetValue(id, out var model))
                    return model;

                return _dictionary[id] = await callback(id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool ContainsKey(Snowflake key)
        {
            _semaphore.Wait();
            try
            {
                return _dictionary.ContainsKey(key);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public IEnumerator<KeyValuePair<Snowflake, TModel>> GetEnumerator() => _dictionary.GetEnumerator();

        public bool TryGetValue(Snowflake key, [MaybeNullWhen(false)] out TModel value)
        {
            _semaphore.Wait();
            try
            {
                return _dictionary.TryGetValue(key, out value);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        void IDictionary<Snowflake, TModel>.Add(Snowflake key, TModel value)
        {
            _semaphore.Wait();
            try
            {
                _dictionary.TryAdd(key, value);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        void ICollection<KeyValuePair<Snowflake, TModel>>.Add(KeyValuePair<Snowflake, TModel> item)
        {
            _semaphore.Wait();
            try
            {
                _dictionary.TryAdd(item.Key, item.Value);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        bool ICollection<KeyValuePair<Snowflake, TModel>>.Contains(KeyValuePair<Snowflake, TModel> item)
        {
            _semaphore.Wait();
            try
            {
                return _dictionary.Contains(item);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        void ICollection<KeyValuePair<Snowflake, TModel>>.CopyTo(KeyValuePair<Snowflake, TModel>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Remove(Snowflake key)
        {
            _semaphore.Wait();
            try
            {
                return _dictionary.TryRemove(key, out _);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        bool ICollection<KeyValuePair<Snowflake, TModel>>.Remove(KeyValuePair<Snowflake, TModel> item)
        {
            _semaphore.Wait();
            try
            {
                return _dictionary.TryRemove(item.Key, out _);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}