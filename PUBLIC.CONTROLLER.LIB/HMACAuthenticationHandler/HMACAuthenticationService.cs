using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PUBLIC.CONTROLLER.HMACAuthenticationHandler
{
    public interface IHMACAuthenticationService
    {
        Task<bool> IsValidUserAsync(HMACAuthenticationOptions options, string requestApplicationId, string requestTimeStamp, string requestNonce, string requestContentBase64String);
    }

    public class HMACAuthenticationService : IHMACAuthenticationService
    {
        private readonly double _replayAttackDelayInSeconds = 30;

        public Task<bool> IsValidUserAsync(HMACAuthenticationOptions options, string requestApplicationId, string requestTimeStamp, string requestNonce, string requestContentBase64String)
        {
            if (!IsValidTimestamp(requestTimeStamp, out DateTime timestamp))
            {
                return Task.FromResult(false);
            }

            if (!PassesThresholdCheck(timestamp))
            {
                return Task.FromResult(false);
            }

            var applicationId = options.AuthorizedApplications.Where(q => q.ApplicationId.Equals(requestApplicationId)).FirstOrDefault();

            if (applicationId == null)
                return Task.FromResult(false);

            if (!ComputeHash(applicationId.ApplicationSecret, timestamp, requestContentBase64String))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private static bool IsValidTimestamp(string requestTimeStamp, out DateTime timestamp)
        {
            timestamp = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan currentTs = DateTime.UtcNow - timestamp;
            var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);
            if ((serverTotalSeconds - requestTotalSeconds) > HMACAuthenticationDefaults.RequestMaxAgeInSeconds)
            {
                return true;
            }
            else
                return false;
            // Parse a string representing UTC. E.g.: "2013-01-12T16:11:20.0904778Z";
            // Client should create the timestamp like this: var timestampValue = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            //var ts = DateTime.TryParseExact(timestampValue, "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out timestamp);            
        }

        private bool isReplayRequest(string nonce, string requestTimeStamp)
        {
            if (System.Runtime.Caching.MemoryCache.Default.Contains(nonce))
            {
                return true;
            }
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan currentTs = DateTime.UtcNow - epochStart;
            var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);
            if ((serverTotalSeconds - requestTotalSeconds) > requestMaxAgeInSeconds)
            {
                return true;
            }
            System.Runtime.Caching.MemoryCache.Default.Add(nonce, requestTimeStamp, DateTimeOffset.UtcNow.AddSeconds(requestMaxAgeInSeconds));
            return false;
        }

        private bool PassesThresholdCheck(DateTime timestamp)
        {
            // make sure call is made within the allowed threshold
            var ts = DateTime.UtcNow.Subtract(timestamp);
            return ts.TotalSeconds <= _replayAttackDelayInSeconds;
        }

        private static bool ComputeHash(string privateKey, DateTime timestamp, string authenticationHash)
        {
            string hashString;
            var ticks = timestamp.Ticks.ToString(CultureInfo.InvariantCulture);
            var key = Encoding.UTF8.GetBytes(privateKey.ToUpper());
            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(ticks));
                hashString = Convert.ToBase64String(hash);
            }

            return hashString.Equals(authenticationHash);
        }
    }
}
