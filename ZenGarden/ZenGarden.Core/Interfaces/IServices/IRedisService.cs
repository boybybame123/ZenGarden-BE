﻿using System;
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
        void Dispose();
    }
}
