using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace StorageQueueTest
{
    public class StatsDClient : IDisposable
    {
        private readonly string _keyPrefix;
        private readonly Random _random;
        private readonly Socket _sock;
        private readonly IPEndPoint _endPoint;

        public StatsDClient()
        {
            var hostname = "statsd.hostedgraphite.com";
            var port = 8125;
            _keyPrefix = "[redacted]";
            var address = Dns.GetHostAddressesAsync(hostname).Result[0];
            _endPoint = new IPEndPoint(address, port);
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { Blocking = false };
            _random = new Random();
        }

        public bool Timing(string key, long value, double sampleRate = 1.0)
        {
            return MaybeSend(sampleRate, string.Format("{0}:{1}|ms", key, value));
        }

        public bool Decrement(string key, int magnitude = -1, double sampleRate = 1.0)
        {
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            return Increment(key, magnitude, sampleRate);
        }

        public bool Decrement(params string[] keys)
        {
            return Increment(-1, 1.0, keys);
        }

        public bool Decrement(int magnitude, params string[] keys)
        {
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            return Increment(magnitude, 1.0, keys);
        }

        public bool Decrement(int magnitude, double sampleRate, params string[] keys)
        {
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            return Increment(magnitude, sampleRate, keys);
        }

        public bool Increment(string key, int magnitude = 1, double sampleRate = 1.0)
        {
            var stat = string.Format("{0}:{1}|c", key, magnitude);
            return MaybeSend(stat, sampleRate);
        }

        public bool Increment(int magnitude, double sampleRate, params string[] keys)
        {
            var stats = new string[keys.Length];

            for (var i = 0; i < keys.Length; i++)
            {
                stats[i] = string.Format("{0}:{1}|c", keys[i], magnitude);
            }
            return MaybeSend(sampleRate, stats);
        }

        private bool MaybeSend(string stat, double sampleRate)
        {
            return MaybeSend(sampleRate, stat);
        }

        private bool MaybeSend(double sampleRate, params string[] stats)
        {
            // only return true if we sent something
            var retval = false;

            if (sampleRate < 1.0)
            {
                foreach (var stat in stats)
                {
                    if (_random.NextDouble() <= sampleRate)
                    {
                        var sampledStat = string.Format("{0}|@{1}", stat, sampleRate);

                        if (Send(sampledStat))
                        {
                            retval = true;
                        }
                    }
                }
            }
            else
            {
                foreach (var stat in stats)
                {
                    if (Send(stat))
                    {
                        retval = true;
                    }
                }
            }

            return retval;
        }

        public bool Send(string message, Encoding encoding = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_keyPrefix))
                {
                    message = _keyPrefix + "." + message;
                }

                var bytes = (encoding ?? Encoding.UTF8).GetBytes(message);
                _sock.SendTo(bytes, _endPoint);

                return true;
            }
            catch
            {
                // Suppress all exceptions for now
                return false;
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _sock?.Dispose();
        }

        #endregion
    }
}
