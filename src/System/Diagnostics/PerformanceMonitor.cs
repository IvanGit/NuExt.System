using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Diagnostics
{
    /// <summary>
    /// Monitors the performance of a specific process and provides formatted usage statistics.
    /// </summary>
    public sealed class PerformanceMonitor: PropertyChangeNotifier
    {
        private readonly ProcessMonitor _processMonitor;
        private readonly IFormatProvider? _formatProvider;

        public PerformanceMonitor(Process process, IFormatProvider? provider, SynchronizationContext? synchronizationContext) : base(synchronizationContext)
        {
            Process = process ?? throw new ArgumentNullException(nameof(process));
            _formatProvider = provider;
            _processMonitor = new ProcessMonitor(Process);
        }

        public PerformanceMonitor(Process process, IFormatProvider? provider) : this(process, provider, null)
        {

        }

        public PerformanceMonitor(Process process) : this(process, null)
        {

        }

        private string? _formattedUsage;
        /// <summary>
        /// Gets the formatted usage string.
        /// </summary>
        public string? FormattedUsage
        {
            get => _formattedUsage;
            private set => SetProperty(ref _formattedUsage, value);
        }

        /// <summary>
        /// CPU usage format string.
        /// </summary>
        public string FormatCpuUsage { get; set; } = "CPU: {0:N1} %";

        /// <summary>
        /// Memory usage format string.
        /// </summary>
        public string FormatMemoryUsage { get; set; } = "MEM: {0}";

        /// <summary>
        /// Peak memory usage format string.
        /// </summary>
        public string FormatPeakMemoryUsage { get; set; } = "PMEM: {0}";

        /// <summary>
        /// Managed memory usage format string.
        /// </summary>
        public string FormatManagedMemory { get; set; } = "GC: {0}";

        /// <summary>
        /// Peak managed memory usage format string.
        /// </summary>
        public string FormatPeakManagedMemory { get; set; } = "PGC: {0}";

        /// <summary>
        /// Thread info format string.
        /// </summary>
        public string FormatThreads { get; set; } = "WRK: {0}/{1}";

        /// <summary>
        /// Gets the process being monitored.
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to show CPU usage.
        /// </summary>
        public bool ShowCpuUsage { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show Memory usage.
        /// </summary>
        public bool ShowMemoryUsage { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show peak memory usage.
        /// </summary>
        public bool ShowPeakMemoryUsage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show managed memory usage.
        /// </summary>
        public bool ShowManagedMemory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show peak managed memory usage.
        /// </summary>
        public bool ShowPeakManagedMemory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show thread info.
        /// </summary>
        public bool ShowThreads { get; set; }

        /// <summary>
        /// Monitors the process performance and updates <see cref="FormattedUsage"/> asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for task completion.</param>
        /// <returns>A task that represents the asynchronous monitoring operation.</returns>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            long peakGcMemory = 0;
            try
            {
                var sb = new StringBuilder();
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    Process.Refresh();

                    var cpuUsage = _processMonitor.GetCpuUsage();
                    var memoryUsage = _processMonitor.GetCurrentMemoryUsage();
                    sb.Clear();
                    string delimiter = string.Empty;
                    if (ShowCpuUsage)
                    {
                        sb.Append(delimiter);
                        sb.AppendFormat(_formatProvider, FormatCpuUsage, cpuUsage);
                        delimiter = ", ";
                    }
                    if (ShowMemoryUsage)
                    {
                        sb.Append(delimiter);
                        sb.AppendFormat(_formatProvider, FormatMemoryUsage, FormatUtils.FormatSize(_formatProvider, memoryUsage));
                        delimiter = ", ";
                    }
                    if (ShowPeakMemoryUsage)
                    {
                        sb.Append(delimiter);
                        sb.AppendFormat(_formatProvider, FormatPeakMemoryUsage, FormatUtils.FormatSize(_formatProvider, _processMonitor.GetPeakMemoryUsage()));
                        delimiter = ", ";
                    }
                    if (ShowManagedMemory)
                    {
                        var gcTotalMemory = GC.GetTotalMemory(false);
                        sb.Append(delimiter);
                        sb.AppendFormat(_formatProvider, FormatManagedMemory, FormatUtils.FormatSize(_formatProvider, gcTotalMemory));
                        delimiter = ", ";
                        if (gcTotalMemory > peakGcMemory)
                        {
                            peakGcMemory = gcTotalMemory;
                        }
                        if (ShowPeakManagedMemory)
                        {
                            sb.Append(delimiter);
                            sb.AppendFormat(_formatProvider, FormatPeakManagedMemory, FormatUtils.FormatSize(_formatProvider, peakGcMemory));
                        }
                    }
                    if (ShowThreads)
                    {
                        var threadCount = _processMonitor.GetThreadCount();
                        var workerCount = _processMonitor.GetBusyWorkerThreads();
                        sb.Append(delimiter);
                        sb.AppendFormat(_formatProvider, FormatThreads, workerCount, threadCount);
                    }
                    FormattedUsage = sb.ToString();
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in QueuesAsync: {ex.Message}");
                Debug.Assert(false, ex.Message);
            }
        }
    }
}
