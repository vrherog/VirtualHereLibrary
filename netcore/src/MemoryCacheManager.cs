using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace vrhero.VirtualHere
{
    public class MemoryCacheManager : IDisposable
    {
        private static MemoryCacheManager _default;
        public static MemoryCacheManager Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new MemoryCacheManager();
                }
                return _default;
            }
        }

        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());


        /// <summary>
        /// 判断是否在缓存中
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public bool IsInCache(string key)
        {
            List<string> keys = GetAllKeys();
            foreach (var i in keys)
            {
                if (i == key) return true;
            }
            return false;
        }

        /// <summary>
        /// 获取所有缓存键
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllKeys()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var entries = _cache.GetType().GetField("_entries", flags).GetValue(_cache);
            var cacheItems = entries as IDictionary;
            var keys = new List<string>();
            if (cacheItems == null)
            {
                return keys;
            }
            foreach (DictionaryEntry cacheItem in cacheItems)
            {
                keys.Add(cacheItem.Key.ToString());
            }
            return keys;
        }

        /// <summary>
        /// 获取所有的缓存值
        /// </summary>
        /// <returns></returns>
        public List<T> GetAllValues<T>()
        {
            var cacheKeys = GetAllKeys();
            var vals = new List<T>();
            cacheKeys.ForEach(i =>
            {
                if (_cache.TryGetValue(i, out T t))
                {
                    vals.Add(t);
                }
            });
            return vals;
        }
        /// <summary>
        /// 取得缓存数据
        /// </summary>
        /// <typeparam name="T">类型值</typeparam>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            _ = _cache.TryGetValue(key, out T value);
            return value;
        }

        public bool TryGet<T>(string key, out T value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public long Increase(string key)
        {
            if (!_cache.TryGetValue(key, out long v))
            {
                v = 0;
            }
            var result = v + 1;
            _cache.Set(key, result);
            return result;
        }

        public long Decrease(string key)
        {
            if (!_cache.TryGetValue(key, out long v))
            {
                return 0;
            }
            var result = v - 1;
            _cache.Set(key, result);
            return result;
        }

        /// <summary>
        /// 设置缓存(永不过期)
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="value">缓存值</param>
        public void SetNotExpire<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (_cache.TryGetValue(key, out T _))
            {
                _cache.Remove(key);
            }
            _ = _cache.Set(key, value);
        }

        /// <summary>
        /// 设置缓存(滑动过期:超过一段时间不访问就会过期,一直访问就一直不过期)
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="value">缓存值</param>
        public void SetSlidingExpire<T>(string key, T value, TimeSpan span)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (_cache.TryGetValue(key, out T _))
            {
                _cache.Remove(key);
            }
            _ = _cache.Set(key, value, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = span
            });
        }

        /// <summary>
        /// 设置缓存(绝对时间过期:从缓存开始持续指定的时间段后就过期,无论有没有持续的访问)
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="value">缓存值</param>
        public void SetAbsoluteExpire<T>(string key, T value, TimeSpan span)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (_cache.TryGetValue(key, out T _))
            {
                _cache.Remove(key);
            }
            _ = _cache.Set(key, value, span);
        }

        /// <summary>
        /// 设置缓存(绝对时间过期+滑动过期:比如滑动过期设置半小时,绝对过期时间设置2个小时，那么缓存开始后只要半小时内没有访问就会立马过期,如果半小时内有访问就会向后顺延半小时，但最多只能缓存2个小时)
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="value">缓存值</param>
        public void SetSlidingAndAbsoluteExpire<T>(string key, T value, TimeSpan slidingSpan, TimeSpan absoluteSpan)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (_cache.TryGetValue(key, out T _))
            {
                _cache.Remove(key);
            }
            _ = _cache.Set(key, value, new MemoryCacheEntryOptions()
            {
                SlidingExpiration = slidingSpan,
                AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(absoluteSpan.Milliseconds)
            });
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">关键字</param>
        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            _cache.Remove(key);
        }

        public void Dispose()
        {
            if (_cache != null)
            {
                _cache.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
