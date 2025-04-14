using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Core.Interfaces.IServices
{
    public interface IRedisService
    {
        Task<bool> PingAsync();
        Task<string> GetStringAsync(string key);
        Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null);
        Task<bool> KeyExistsAsync(string key);
        Task<bool> DeleteKeyAsync(string key);
        Task<TimeSpan?> GetKeyTimeToLiveAsync(string key);
        Task<bool> AddToSortedSetAsync(string setKey, string member, double score);
        Task<List<string>> GetTopSortedSetAsync(string setKey, int count);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getDataFunc, TimeSpan? expiry = null);
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key);
        void Dispose();
    }
}
