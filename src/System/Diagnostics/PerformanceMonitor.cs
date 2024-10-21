using System.ComponentModel;
using System.Globalization;
using System.Text;

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
        /// Gets the process being monitored.
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to show managed memory usage.
        /// </summary>
        public bool ShowManagedMemory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show peak managed memory usage.
        /// </summary>
        public bool ShowPeakManagedMemory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show peak memory usage.
        /// </summary>
        public bool ShowPeakMemoryUsage { get; set; }

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
                    sb.AppendFormat(_formatProvider, "CPU: {0:N1} %, MEM: {1}", cpuUsage, FormatUtils.FormatSize(_formatProvider, memoryUsage));
                    if (ShowPeakMemoryUsage)
                    {
                        sb.AppendFormat(_formatProvider, ", PMEM: {0}", FormatUtils.FormatSize(_formatProvider, _processMonitor.GetPeakMemoryUsage()));
                    }
                    if (ShowManagedMemory)
                    {
                        var gcTotalMemory = GC.GetTotalMemory(false);
                        sb.AppendFormat(_formatProvider, ", GC: {0}", FormatUtils.FormatSize(_formatProvider, gcTotalMemory));
                        if (gcTotalMemory > peakGcMemory)
                        {
                            peakGcMemory = gcTotalMemory;
                        }
                        if (ShowPeakManagedMemory)
                        {
                            sb.AppendFormat(_formatProvider, ", PGC: {0}", FormatUtils.FormatSize(_formatProvider, peakGcMemory));
                        }
                    }
                    if (ShowThreads)
                    {
                        var threadCount = _processMonitor.GetThreadCount();
                        var workerCount = _processMonitor.GetBusyWorkerThreads();
                        sb.AppendFormat(_formatProvider, ", WRK: {0}/{1}", workerCount, threadCount);
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
                Debug.WriteLine($"Error in QueuesAsync: {ex.Message}");
                Debug.Assert(false, ex.Message);
            }
        }
    }
}
