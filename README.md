# Dynamics 365 Templates

Base classes for use with plugins and custom workflows.

Keeps things stateless, [as recommended](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/guidance/server/develop-iplugin-implementations-stateless) and has helper methods to keep new, untested code at a minimum.

## Plugins
### Implementing
When implementing a Dynamics 365 plugin, copy/paste the [PluginBase.cs](XRT.Dynamics365.Templates/PluginBase.cs) base class, and then override the abstract Execute method.
An example might look like:

~~~~
public class MyTestPlugin : PluginBase
{
    public override void Execute(PluginWorker worker)
    {
        //Do plugin things here
    }
}
~~~~
The PluginWorker class will provide instances of all common plugin objects.

### Verifying Registrations
A common requirement is to check the plugin message, event, stage and depth. This can be achieved by calling `VerifyRegistration` on the `PluginWorker` instance.
For example, the following verifies the current execution is either the create or update of an Account record, running on the post event stage.
~~~~
public class MyTestPlugin : XRT.Dynamics365.Templates.PluginBase
{
    public override void Execute(PluginWorker worker)
    {
        worker.VerifyRegistration("MyTestPlugin", new List<PluginStepRegistration>
        {
            new PluginStepRegistration { EntityName = "Account", MaximumDepth = 1, Message = "Create", PluginStage = (int)PluginStage.Post},
            new PluginStepRegistration { EntityName = "Account", MaximumDepth = 1, Message = "Update", PluginStage = (int)PluginStage.Post}
        });
    }
}
~~~~
Passing a null collection will pass for all combinations (no verification).

### Plugin Target and Target References
`PluginWorker` exposes the `IPluginExecutionContext` instance for the plugin but for convenience contains `Entity` and `EntityReference` properties (`Target` and `TargetReference`). The `PluginWorker` constructor will populate these based on the plugin event and message.
Similar helper properties are set for `Assign` and `SetState` plugins ([although `SetState` is now deprecated and you should be using an `Update` message](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/developer/org-service/perform-specialized-operations-using-update)).

