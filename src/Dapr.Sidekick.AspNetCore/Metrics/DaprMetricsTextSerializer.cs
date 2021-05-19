// This implementation is based on the approach used by prometheus-net.
// See https://github.com/prometheus-net/prometheus-net/blob/master/Prometheus.NetStandard/TextSerializer.cs
// See PROMETHEUS_LICENSE in this directory for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dapr.Sidekick.AspNetCore.Metrics
{
    public sealed class DaprMetricsTextSerializer : IDaprMetricsSerializer
    {
        internal static readonly byte[] NewLine = new[] { (byte)'\n' };

        private readonly Lazy<Stream> _stream;

        public DaprMetricsTextSerializer(Stream stream)
        {
            _stream = new Lazy<Stream>(() => stream);
        }

        // Enables delay-loading of the stream, because touching stream in HTTP handler triggers some behavior.
        public DaprMetricsTextSerializer(Func<Stream> streamFactory)
        {
            _stream = new Lazy<Stream>(streamFactory);
        }

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            // If we never opened the stream, we don't touch it on flush.
            if (!_stream.IsValueCreated)
            {
                return Task.CompletedTask;
            }

            return _stream.Value.FlushAsync(cancellationToken);
        }

        public async Task WriteLineAsync(string value, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var bytes = DaprMetricsConstants.ExportEncoding.GetBytes(value);
            await _stream.Value.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
            await _stream.Value.WriteAsync(NewLine, 0, NewLine.Length, cancellationToken);
        }
    }
}
