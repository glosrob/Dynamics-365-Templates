using Microsoft.Xrm.Sdk;
using System;
using System.Runtime.Caching;

namespace XRT.Dynamics365.Templates.Tests.Helpers
{
    public class PluginBaseFixture
    {
        //Properties

        /// <summary>
        /// Gets or sets the Id of the user that initiated the plugin.
        /// </summary>
        public Guid InitiatingUserId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the user that is executing the plugin.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the target of the plugin.
        /// </summary>
        public Entity Target { get; set; }

        /// <summary>
        /// Gets or sets an instance of the plugin to be tested.
        /// </summary>
        public CacheWorkerPlugin Plugin { get; set; }

        //Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PluginBaseFixture()
        {
            InitiatingUserId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            Target = new Entity("contact", Guid.NewGuid());
            Plugin = new CacheWorkerPlugin();
        }

        //Methods

        /// <summary>
        /// Retrieves the worker for this fixture.
        /// </summary>
        /// <returns>The worker for this fixture</returns>
        public PluginWorker GetWorkerFromCache()
        {
            var worker = (PluginWorker)MemoryCache.Default.Get("Worker");
            MemoryCache.Default.Remove("Worker");
            return worker;
        }

        /// <summary>
        /// Generates a worker for a verify test.
        /// </summary>
        /// <returns>A worker for a verify test.</returns>
        public PluginWorker GenerateProviderForVerify()
        {
            var serviceProvider = FakePluginServiceProviders.Generate(new PluginServiceProviderRequest
            {
                Depth = 2,
                EventName = "Create",
                Stage = PluginStage.Pre,
                InitiatingUserId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                InputParameters = null,
                PrimaryEntityName = "contact",
                Target = null,
                TargetReference = null
            });
            return new PluginWorker(serviceProvider);
        }
    }
}