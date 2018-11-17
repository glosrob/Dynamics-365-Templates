using System;
using System.Runtime.Caching;

namespace XRT.Dynamics365.Templates.Tests.Helpers
{
    /// <summary>
    /// For use in unit tests.
    /// </summary>
    public class CacheWorkerPlugin : PluginBase
    {
        /// <summary>
        /// Implements the plugin business logic.
        /// </summary>
        /// <param name="worker">PluginWorker with the plugin instance properties.</param>
        public override void Execute(PluginWorker worker)
        {
            worker.Prefix = "CacheWorkerPlugin";
            worker.TraceMessage("Test trace nessage");

            var cache = MemoryCache.Default;
            cache.Set("Worker", worker, DateTime.Now.AddMinutes(1));
        }
    }
}