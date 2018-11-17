using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Collections.Generic;

namespace XRT.Dynamics365.Templates.Tests.Helpers
{
    /// <summary>
    /// Generates copies of plugin service providers for different events.
    /// </summary>
    public static class FakePluginServiceProviders
    {
        /// <summary>
        /// Generate the provider for a pre create plugin.
        /// </summary>
        /// <param name="e">The entity to use as the plugin target.</param>
        /// <param name="userId">The Id of the user executing the plugin.</param>
        /// <param name="initiatingUserId">The Id of the user that initiated the plugin.</param>
        /// <returns>IServiceProvider for a pre create plugin.</returns>
        public static IServiceProvider GeneratePreCreate(Entity e, Guid userId, Guid initiatingUserId)
        {
            return Generate(new PluginServiceProviderRequest
            {
                PrimaryEntityName = "contact",
                Depth = 1,
                EventName = "Create",
                InitiatingUserId = initiatingUserId,
                UserId = userId,
                InputParameters = null,
                Stage = PluginStage.Pre,
                Target = e,
                TargetReference = null
            });
        }

        /// <summary>
        /// Generate the provider for a pre update plugin.
        /// </summary>
        /// <param name="e">The entity to use as the plugin target.</param>
        /// <param name="userId">The Id of the user executing the plugin.</param>
        /// <param name="initiatingUserId">The Id of the user that initiated the plugin.</param>
        /// <returns>IServiceProvider for a pre update plugin.</returns>
        public static IServiceProvider GeneratePreUpdate(Entity e, Guid userId, Guid initiatingUserId)
        {
            return Generate(new PluginServiceProviderRequest
            {
                PrimaryEntityName = "contact",
                Depth = 1,
                EventName = "Update",
                InitiatingUserId = initiatingUserId,
                UserId = userId,
                InputParameters = null,
                Stage = PluginStage.Pre,
                Target = e,
                TargetReference = null
            });
        }

        /// <summary>
        /// Generate the provider for an assign plugin.
        /// </summary>
        /// <param name="e">The entity to use as the plugin target.</param>
        /// <param name="userId">The Id of the user executing the plugin.</param>
        /// <param name="initiatingUserId">The Id of the user that initiated the plugin.</param>
        /// <returns>IServiceProvider for an assign plugin.</returns>
        public static IServiceProvider GenerateAssign(EntityReference target, EntityReference assignee, Guid userId, Guid initiatingUserId)
        {
            return Generate(new PluginServiceProviderRequest
            {
                PrimaryEntityName = "contact",
                Depth = 1,
                EventName = "Assign",
                InitiatingUserId = initiatingUserId,
                UserId = userId,
                InputParameters = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Assignee", assignee)
                },
                Stage = PluginStage.Pre,
                Target = null,
                TargetReference = target
            });
        }

        /// <summary>
        /// Generate the provider for a set state plugin.
        /// </summary>
        /// <param name="e">The entity to use as the plugin target.</param>
        /// <param name="userId">The Id of the user executing the plugin.</param>
        /// <param name="initiatingUserId">The Id of the user that initiated the plugin.</param>
        /// <returns>IServiceProvider for a set state plugin.</returns>
        public static IServiceProvider GenerateSetState(EntityReference target, int state, int status, Guid userId, Guid initiatingUserId)
        {
            return Generate(new PluginServiceProviderRequest
            {
                PrimaryEntityName = "contact",
                Depth = 1,
                EventName = "Assign",
                InitiatingUserId = initiatingUserId,
                UserId = userId,
                InputParameters = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("EntityMoniker", target),
                    new KeyValuePair<string, object>("State", new OptionSetValue(state)),
                    new KeyValuePair<string, object>("Status", new OptionSetValue(status))
                },
                Stage = PluginStage.Pre,
                Target = null,
                TargetReference = null
            });
        }

        /// <summary>
        /// Generates a custom IServiceProvider.
        /// </summary>
        /// <param name="request">Details to use to generate the IServiceProvider.</param>
        /// <returns></returns>
        public static IServiceProvider Generate(PluginServiceProviderRequest request)
        {
            //Create plugin context
            var pluginContext = new Moq.Mock<IPluginExecutionContext>();
            pluginContext.Setup(x => x.Depth).Returns(request.Depth);
            pluginContext.Setup(x => x.MessageName).Returns(request.EventName);
            pluginContext.Setup(x => x.InitiatingUserId).Returns(request.InitiatingUserId);
            pluginContext.Setup(x => x.UserId).Returns(request.UserId);
            pluginContext.Setup(x => x.Stage).Returns((int)request.Stage);

            //Add all input parameters
            var parameters = new ParameterCollection();
            request.InputParameters = request.InputParameters ?? new List<KeyValuePair<string, object>>();
            if (request.TargetReference != null)
            {
                parameters.Add(new KeyValuePair<string, object>("Target", request.TargetReference));
            }
            else if (request.Target != null)
            {
                parameters.Add(new KeyValuePair<string, object>("Target", request.Target));
            }
            foreach (var param in request.InputParameters)
            {
                parameters.Add(param.Key, param.Value);
            }
            pluginContext.Setup(x => x.InputParameters).Returns(parameters);
            pluginContext.Setup(x => x.PrimaryEntityName).Returns(request.PrimaryEntityName);

            //Create service instances
            var orgService = new Moq.Mock<IOrganizationService>();
            var orgServiceAdmin = new Moq.Mock<IOrganizationService>();

            //Create factory instance
            var factory = new Moq.Mock<IOrganizationServiceFactory>();
            factory.Setup(x => x.CreateOrganizationService(It.Is<Guid>(y => y == request.UserId))).Returns(orgService.Object);
            factory.Setup(x => x.CreateOrganizationService(It.Is<Guid>(y => y == Guid.Empty))).Returns(orgServiceAdmin.Object);
            
            //Create trace instance
            var trace = new Moq.Mock<ITracingService>();

            //Link them together
            var serviceProvider = new Moq.Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IPluginExecutionContext))).Returns(pluginContext.Object);
            serviceProvider.Setup(x => x.GetService(typeof(IOrganizationServiceFactory))).Returns(factory.Object);
            serviceProvider.Setup(x => x.GetService(typeof(ITracingService))).Returns(trace.Object);
            return serviceProvider.Object;
        }
    }

    /// <summary>
    /// Used to generate a service provider instance.
    /// </summary>
    public class PluginServiceProviderRequest
    {
        /// <summary>
        /// Gets or sets the Id to set as the User Id.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the Id to set as the Initiating User Id.
        /// </summary>
        public Guid InitiatingUserId { get; set; }

        /// <summary>
        /// Gets or sets the stage for the plugin.
        /// </summary>
        public PluginStage Stage { get; set; }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the primary entity.
        /// </summary>
        public string PrimaryEntityName { get; set; }

        /// <summary>
        /// Gets or sets the Entity Reference to use as the plugin target.
        /// </summary>
        public EntityReference TargetReference { get; set; }

        /// <summary>
        /// Gets or sets the Entity to use as the plugin target.
        /// </summary>
        public Entity Target { get; set; }

        /// <summary>
        /// Gets or sets the collection of additional parameters.
        /// </summary>
        public List<KeyValuePair<string,object>> InputParameters { get; set; }

        /// <summary>
        /// Gets or sets the depth of the plugin.
        /// </summary>
        public int Depth { get; set; }
    }
}