﻿namespace System.Diagnostics
{
    /// <summary>
    /// Provides comprehensive monitoring of a specified process, including various metrics such as CPU usage,
    /// memory consumption, thread count, and ThreadPool utilization. This class leverages the .NET System.Diagnostics 
    /// namespace to offer real-time insights into a process's resource usage and performance.
    /// </summary>
    public sealed class ProcessMonitor
    {
        private readonly Stopwatch _watch = Stopwatch.StartNew();
        private TimeSpan _lastTotalProcessorTime;
        private TimeSpan _lastElapsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessMonitor"/> class for the specified process.
        /// </summary>
        /// <param name="process">The process to monitor.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="process"/> is <c>null</c>.</exception>
        public ProcessMonitor(Process process)
        {
            Process = process ?? throw new ArgumentNullException(nameof(process));
            _lastTotalProcessorTime = Process.TotalProcessorTime;
            _lastElapsed = _watch.Elapsed;
        }

        /// <summary>
        /// The process being monitored.
        /// </summary>
        public Process Process { get; }

        private int? _processorCount;
        /// <summary>
        /// Gets the number of processors on the current machine.
        /// </summary>
        public int ProcessorCount => _processorCount ??= Environment.ProcessorCount;

        /// <summary>
        /// Gets the number of busy worker threads in the ThreadPool.
        /// </summary>
        /// <returns>The number of busy worker threads.</returns>
        public int GetBusyWorkerThreads()
        {
            Debug.Assert(ProcessorCount > 1);
            try
            {

                ThreadPool.GetAvailableThreads(out int workerThreads, out _);
                ThreadPool.GetMaxThreads(out int maxWorkerThreads, out _);
                int busyWorkerThreads = maxWorkerThreads - workerThreads;
                return busyWorkerThreads;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error retrieving busy worker threads: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Calculates the current CPU usage percentage for the process.
        /// </summary>
        /// <returns>The percentage of CPU usage for the process, accounting for the number of cores.</returns>
        public float GetCpuUsage()
        {
            try
            {
                TimeSpan currentElapsed = _watch.Elapsed;
                TimeSpan currentTotalProcessorTime = Process.TotalProcessorTime;
                TimeSpan cpuUsed = currentTotalProcessorTime - _lastTotalProcessorTime;
                TimeSpan totalPassed = currentElapsed - _lastElapsed;

                double cpuUsageTotal = (cpuUsed.Ticks / (double)totalPassed.Ticks) * 100;
                double cpuUsagePerCore = cpuUsageTotal / ProcessorCount;

                _lastTotalProcessorTime = currentTotalProcessorTime;
                _lastElapsed = currentElapsed;

                return (float)cpuUsagePerCore;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error calculating CPU usage: {ex.Message}");
                return 0f;
            }
        }

        /// <summary>
        /// Gets the current memory usage of the process in bytes.
        /// </summary>
        /// <returns>The amount of memory in use by the process, in bytes.</returns>
        public long GetCurrentMemoryUsage()
        {
            try
            {
                return Process.WorkingSet64;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error retrieving memory usage: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the peak memory usage of the process in bytes.
        /// </summary>
        /// <returns>The peak amount of memory used by the process, in bytes.</returns>
        public long GetPeakMemoryUsage()
        {
            try
            {
                return Process.PeakWorkingSet64;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error retrieving peak memory usage: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the current number of threads of the process.
        /// </summary>
        /// <returns>The number of threads in the process.</returns>
        public int GetThreadCount()
        {
            try
            {
                return Process.Threads.Count;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error retrieving thread count: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Refreshes the process information.
        /// </summary>
        public void Refresh()
        {
            try
            {
                Process.Refresh();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error refreshing process information: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets the internal CPU usage counters. 
        /// This can be useful if you want to start measuring from a clean state.
        /// </summary>
        public void ResetCpuUsageCounters()
        {
            _lastTotalProcessorTime = Process.TotalProcessorTime;
            _lastElapsed = _watch.Elapsed;
        }
    }
}
