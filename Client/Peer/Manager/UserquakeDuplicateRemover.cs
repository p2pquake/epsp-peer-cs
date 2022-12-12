using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Peer.Manager
{
    class UserquakeDuplicateRemover
    {
        private static readonly int ALLOW_MIN_INTERVAL_SECONDS = 300;
        private static readonly int CLEANUP_INTERVAL_COUNT = 512;

        private Dictionary<string, DateTime> userquakes;
        private Func<DateTime> protocolTime;
        private int cleanupCount = 0;

        public UserquakeDuplicateRemover(Func<DateTime> protocolTime)
        {
            this.userquakes = new Dictionary<string, DateTime>();
            this.protocolTime = protocolTime;
        }

        public bool IsDuplicate(string publicKey)
        {
            var now = protocolTime();

            lock(userquakes)
            {
                if (!userquakes.ContainsKey(publicKey))
                {
                    PurgeExpired();
                    userquakes[publicKey] = now;
                    return false;
                }

                if (now.Subtract(userquakes[publicKey]).TotalSeconds >= ALLOW_MIN_INTERVAL_SECONDS)
                {
                    userquakes[publicKey] = now;
                    return false;
                }

                return true;
            }
        }

        private void PurgeExpired()
        {
            cleanupCount += 1;
            if (cleanupCount < CLEANUP_INTERVAL_COUNT)
            {
                return;
            }
            cleanupCount = 0;

            var now = protocolTime();
            lock (userquakes)
            {
                var expiredUserquakes = userquakes.Where(e => now.Subtract(e.Value).TotalSeconds >= ALLOW_MIN_INTERVAL_SECONDS);
                foreach (var expiredUserquake in expiredUserquakes)
                {
                    userquakes.Remove(expiredUserquake.Key);
                }
            }
        }
    }
}
