using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace XRT.Dynamics365.Templates
{
    /// <summary>
    /// Base plugin for Dynamics365. Requires Execute to be implemented in a derived class.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        /// <summary>
        /// Initialises the PluginWorker helper.
        /// </summary>
        /// <param name="serviceProvider">The implementation of IServiceProvider.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var worker = new PluginWorker(serviceProvider);
            Execute(worker);
        }

        /// <summary>
        /// Implements the plugin business logic.
        /// </summary>
        /// <param name="worker">PluginWorker with the plugin instance properties.</param>
        public abstract void Execute(PluginWorker worker);
    }

    /// <summary>
    /// Helper class containing shortcut methods and common plugin properties.
    /// </summary>
    public class PluginWorker
    {
        //Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="serviceProvider">Implementation of IServiceProvider to use to initialise all properties.</param>
        public PluginWorker(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            //Set properties
            Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            Factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            Service = Factory.CreateOrganizationService(Context.UserId);
            AdminService = Factory.CreateOrganizationService(Guid.Empty);
            Trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //Unpack various combinations of parameters that are sent for various plugin events
            if (Context.InputParameters.Contains("Target") && Context.InputParameters["Target"] is Entity target)
            {
                Target = target;
                TargetReference = target.ToEntityReference();
            }
            else if (Context.InputParameters.Contains("Target") && Context.InputParameters["Target"] is EntityReference targetRef)
            {
                TargetReference = targetRef;
            }
            else if (Context.InputParameters.Contains("EntityMoniker") && Context.InputParameters["EntityMoniker"] is EntityReference moniker)
            {
                TargetReference = moniker;
            }
            else if (Context.InputParameters.Contains("EmailId") && Context.InputParameters["EmailId"] is Guid)
            {
                TargetReference = new EntityReference("email", (Guid)Context.InputParameters["EmailId"]);
            }

            //Additional common properties
            if (Context.InputParameters.Contains("Assignee") && Context.InputParameters["Assignee"] is EntityReference er)
            {
                Assignee = er;
            }
            if (Context.InputParameters.Contains("State") && Context.InputParameters["State"] is OptionSetValue state)
            {
                State = state;
            }
            if (Context.InputParameters.Contains("Status") && Context.InputParameters["Status"] is OptionSetValue status)
            {
                Status = status;
            }

            //Instantiate helper
            ServiceHelper = new OrgServiceHelper(Service, TraceMessage);
            AdminServiceHelper = new OrgServiceHelper(AdminService, TraceMessage);
        }

        //Properties

        /// <summary>
        /// Gets or sets the implementation of IOrganizationServiceFactory.
        /// </summary>
        public IOrganizationServiceFactory Factory { get; set; }

        /// <summary>
        /// Gets or sets the implementation of IOrganizationService running as the user.
        /// </summary>
        public IOrganizationService Service { get; set; }

        /// <summary>
        /// Gets or sets the implementation of IOrganizationService running as the SYSTEM user.
        /// </summary>
        public IOrganizationService AdminService { get; set; }

        /// <summary>
        /// Gets or sets the implementation of ITracingService.
        /// </summary>
        public ITracingService Trace { get; set; }

        /// <summary>
        /// Gets or sets the implementation of IPluginExecutionContext.
        /// </summary>
        public IPluginExecutionContext Context { get; set; }

        /// <summary>
        /// Gets or sets the target of this plugin.
        /// For some operations, this will be null.
        /// </summary>
        public Entity Target { get; set; }

        /// <summary>
        /// Gets or sets a reference to the target of this plugin.
        /// For some operations, this will be null.
        /// </summary>
        public EntityReference TargetReference { get; set; }

        /// <summary>
        /// Gets the message name for this plugin instance.
        /// </summary>
        public string Message
        {
            get
            {
                return Context.MessageName;
            }
        }

        /// <summary>
        /// Gets the depth of this plugin instance.
        /// </summary>
        public int Depth
        {
            get
            {
                return Context.Depth;
            }
        }

        /// <summary>
        /// Gets the Id of the user for this plugin instance.
        /// </summary>
        public Guid UserId
        {
            get
            {
                return Context.UserId;
            }
        }

        /// <summary>
        /// Gets the Id of the initiating user for this plugin instance.
        /// </summary>
        public Guid InitiatingUserId
        {
            get
            {
                return Context.InitiatingUserId;
            }
        }

        /// <summary>
        /// Gets or sets prefix for any trace messages.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the assignee of the plugin.
        /// Only available for Assign plugins.
        /// </summary>
        public EntityReference Assignee { get; set; }

        /// <summary>
        /// Gets or sets the state of a SetState plugin.
        /// Only available in a SetState plugin.
        /// </summary>
        public OptionSetValue State { get; set; }

        /// <summary>
        /// Gets or sets the status of a SetState plugin.
        /// Only available in a SetState plugin.
        /// </summary>
        public OptionSetValue Status { get; set; }

        /// <summary>
        /// Gets or sets the helper for the user service.
        /// </summary>
        public OrgServiceHelper ServiceHelper { get; set; }

        /// <summary>
        /// Gets or sets the helper for the admin service.
        /// </summary>
        public OrgServiceHelper AdminServiceHelper { get; set; }

        //Methods

        /// <summary>
        /// Validates this plugin instance against a collection of allowed registrations.
        /// Pass null to avoid this check.
        /// </summary>
        /// <param name="pluginName">The name of this plugin, used for any error messages.</param>
        /// <param name="allowedRegistrations">Collection of allowed registrations.</param>
        public void VerifyRegistration(string pluginName, List<PluginStepRegistration> allowedRegistrations)
        {
            //If null, assume all registrations are allowed/no verification
            if (allowedRegistrations == null)
            {
                return;
            }

            //Find a match
            var match = allowedRegistrations.FirstOrDefault(x =>
                (x.EntityName == string.Empty || x.EntityName.Equals(Context.PrimaryEntityName, StringComparison.CurrentCultureIgnoreCase)) &&
                (x.Message == string.Empty || x.Message.Equals(Context.MessageName, StringComparison.CurrentCultureIgnoreCase)) &&
                (x.PluginStage == -1 || x.PluginStage == Context.Stage) &&
                (x.MaximumDepth == -1 || x.MaximumDepth >= Context.Depth));
            if (match == null)
            {
                throw new InvalidPluginExecutionException(
                    $"{pluginName} has been registered incorrectly. Found -\r\n" +
                    $"Entity: {Context.PrimaryEntityName}\r\n" +
                    $"Message: {Context.MessageName}\r\n" +
                    $"Stage: {Context.Stage}\r\n" + 
                    $"Depth: {Context.Depth}\r\n");
            }
        }

        /// <summary>
        /// Outputs a trace message with an optional prefix.
        /// </summary>
        /// <param name="msg"></param>
        public void TraceMessage(string msg)
        {
            Trace.Trace($"{Prefix} {msg}");
        }
    }

    /// <summary>
    /// Stages for a plugin to be executed.
    /// </summary>
    public enum PluginStage
    {
        PreValidation = 10,
        Pre = 20,
        Post = 40
    }

    /// <summary>
    /// Represents a plugin step registration.
    /// </summary>
    public class PluginStepRegistration
    {
        /// <summary>
        /// Gets or sets the allowed stages for the plugin.
        /// -1 to ignore this check.
        /// </summary>
        public int PluginStage { get; set; }

        /// <summary>
        /// Gets or sets the allowed message for the plugin.
        /// Empty string to ignore this check.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the allowed entity logical name for the plugin.
        /// Empty string to ignore this check.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed depth for the plugin.
        /// -1 to ignore this check.
        /// </summary>
        public int MaximumDepth { get; set; }
    }

    /// <summary>
    /// Providers shortcuts to common Organisation Service methods.
    /// </summary>
    public class OrgServiceHelper
    {
        //Properties

        private bool DoTrace { get; set; }
        private IOrganizationService Service { get; set; }
        private Action<string> Trace { get; set; }

        //Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="service">Implementation of IOrganizationService to use to connect to Dynamics 365.</param>
        /// <param name="doTrace">Whether to trace information for every method called.</param>
        /// <param name="trace">The method to use to trace out information.</param>
        public OrgServiceHelper(IOrganizationService service, Action<string> trace, bool doTrace = false)
        {
            Service = service;
            DoTrace = DoTrace;
            Trace = trace;
        }

        //Methods

        /// <summary>
        /// Associates an Entity to a collection of other Entities.
        /// </summary>
        /// <param name="relationshipName">The the name of the relationship to use to connect the records.</param>
        /// <param name="target">The target Entity to be connected from.</param>
        /// <param name="children">The collection of Entities to be connected to.</param>
        public void Associate(string relationshipName, EntityReference target, EntityReferenceCollection children)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target", "Associate method has a null parameter.");
            }

            if (children == null)
            {
                throw new ArgumentNullException("children", "Associate method has a null parameter.");
            }

            if (string.IsNullOrEmpty(relationshipName))
            {
                throw new ArgumentNullException("relationshipName", "Associate method has a null parameter.");
            }

            PerformAction(
                $"Associate [{relationshipName} / {target.Id} / {target.LogicalName}]",
                () => 
                {
                    var associateRequest = new AssociateRequest()
                    {
                        Relationship = new Relationship(relationshipName),
                        Target = new EntityReference(target.LogicalName, target.Id),
                        RelatedEntities = children
                    };
                    Service.Execute(associateRequest);
                });
        }

        /// <summary>
        /// Disassociates an Entity from a collection of other Entities.
        /// </summary>
        /// <param name="relationshipName">The the name of the relationship to use to disconnect the records.</param>
        /// <param name="target">The target Entity to be disconnected from.</param>
        /// <param name="children">The collection of Entities to be disconnected from.</param>
        public void Disassociate(string relationshipName, EntityReference target, EntityReferenceCollection children)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target", "Disassociate method has a null parameter.");
            }

            if (children == null)
            {
                throw new ArgumentNullException("children", "Disassociate method has a null parameter.");
            }

            if (string.IsNullOrEmpty(relationshipName))
            {
                throw new ArgumentNullException("relationshipName", "Disassociate method has a null parameter.");
            }

            PerformAction(
                $"Associate [{relationshipName} / {target.Id} / {target.LogicalName}]",
                () =>
                {
                    var disassociateRequest = new DisassociateRequest()
                    {
                        Relationship = new Relationship(relationshipName),
                        Target = new EntityReference(target.LogicalName, target.Id),
                        RelatedEntities = children
                    };
                    Service.Execute(disassociateRequest);
                });
        }

        /// <summary>
        /// Queries for an Entity using the details provided.
        /// </summary>
        /// <param name="targetEntityName">The name of the Entity to retreive.</param>
        /// <param name="columnSet">The set of properties to retrieve for the Entity.</param>
        /// <param name="attributeName">The name of the attribute to filter by.</param>
        /// <param name="attributeValue">The value of the attribute to filter by.</param>
        /// <returns>A collection of Entities that match the provided details.</returns>
        public EntityCollection QueryByValue(string targetEntityName, ColumnSet columnSet, string attributeName, object attributeValue)
        {
            return QueryByValue(targetEntityName, columnSet, new Dictionary<string, object> { { attributeName, attributeValue } });
        }

        /// <summary>
        /// Queries for an Entity using the details provided.
        /// </summary>
        /// <param name="targetEntityName">The name of the Entity to retreive.</param>
        /// <param name="columnSet">The set of properties to retrieve for the Entity.</param>
        /// <param name="filters">The collection of the attribute to filter results by.</param>
        /// <returns>A collection of Entities that match the provided details.</returns>
        public EntityCollection QueryByValue(string targetEntityName, ColumnSet columnSet, Dictionary<string, object> filters)
        {
            if (string.IsNullOrEmpty(targetEntityName))
            {
                throw new ArgumentNullException("targetEntityName", "QueryByValue method has a null parameter.");
            }

            if (filters == null)
            {
                throw new ArgumentNullException("filters", "QueryByValue method has a null parameter.");
            }

            var collection = new EntityCollection();
            PerformAction(
                $"QueryByValue [{targetEntityName} ({string.Join(", ", filters.Select(x => $"{x.Key} / {ConvertObjectToString(x.Value)}"))})]",
                () =>
                {
                    var query = new QueryByAttribute(targetEntityName)
                    {
                        ColumnSet = columnSet
                    };
                    foreach (var prop in filters)
                    {
                        query.AddAttributeValue(prop.Key, prop.Value);
                    }
                    collection = Service.RetrieveMultiple(query);
                });
            return collection;
        }

        //Helpers

        private void PerformAction(string caller, Action a)
        {
            var sw = new Stopwatch();
            try
            {
                sw.Start();
                a.Invoke();
            }
            catch (Exception ex)
            {
                Trace.Invoke($"{ex.ToString()}");
                throw;
            }
            finally
            {
                sw.Stop();
                if (DoTrace)
                {
                    Trace.Invoke($"{caller}: {sw.ElapsedMilliseconds}ms");
                }
            }
        }

        private string ConvertObjectToString(object obj)
        {
            if (obj is null)
            {
                return "(null)";
            }
            else if (obj is Money moneyObj)
            {
                return $"{moneyObj.Value}";
            }
            else if (obj is OptionSetValue osObj)
            {
                return $"{osObj.Value}";
            }
            else if (obj is EntityReference erObj)
            {
                return $"{erObj.LogicalName} / {erObj.Id}";
            }
            else
            {
                return $"{obj}";
            }
        }
    }
}