using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.Helper.VerifyCode
{
    public class VerificationCodeCache
    {
        private readonly ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, long> expiryTimes = new ConcurrentDictionary<string, long>();
        private readonly Timer timer;

        public VerificationCodeCache()
        {
            // Set up a timer to call the RemoveExpiredEntries method every minute
            timer = new Timer(RemoveExpiredEntries, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public void Put(string key, string value, long expiryTimeInMinutes)
        {
            long expiryTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + (long)TimeSpan.FromMinutes(expiryTimeInMinutes).TotalMilliseconds;
            cache[key] = value;
            expiryTimes[key] = expiryTime;
        }

        public string Get(string key)
        {
            // Check if the key exists in the expiry dictionary
            if (expiryTimes.TryGetValue(key, out long expiryTime))
            {
                // If the current time is past the expiry time, remove the entry
                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() > expiryTime)
                {
                    cache.TryRemove(key, out _);
                    expiryTimes.TryRemove(key, out _);
                    return null;
                }

                // Return the cached value if it hasn't expired
                return cache[key];
            }

            // Return null if the key does not exist or has expired
            return null;
        }

        private void RemoveExpiredEntries(object state)
        {
            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // Create a copy of the keys to avoid modifying the dictionary during iteration
            foreach (var key in expiryTimes.Keys)
            {
                if (expiryTimes.TryGetValue(key, out long expiryTime) && currentTime > expiryTime)
                {
                    cache.TryRemove(key, out _);
                    expiryTimes.TryRemove(key, out _);
                }
            }
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
