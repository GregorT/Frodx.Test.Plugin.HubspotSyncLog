using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Frodx.Test.Plugin.HubspotSyncLog
{
  public class MyTestPlugin : IPlugin
  {
    #region Public

    public void Execute(IServiceProvider serviceProvider)
    {
      var context = (IPluginExecutionContext) serviceProvider.GetService(typeof(IPluginExecutionContext));
      var servicefactory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof(IOrganizationServiceFactory));
      var client = servicefactory.CreateOrganizationService(context.UserId);
      ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

      if (context.InputParameters.Contains("Target") == false)
      {
        tracingService.Trace("Input parameters does not contains Target - exiting");
        return;
      }

      if (context.InputParameters["Target"] is Entity == false)
      {
        tracingService.Trace("Input parameter Target is not an entity - exiting");
        return;
      }
      var isCreating = context.MessageName == "Create";
      if (isCreating) tracingService.Trace("Message contains Create");
      var isUpdating = context.MessageName == "Update";
      if (isUpdating) tracingService.Trace("Message contains Update");
      if (!isCreating && !isUpdating)
      {
        tracingService.Trace("Message does not contain Create or Update - exiting");
        return;
      }

      var entity = (Entity) context.InputParameters["Target"];
      if (entity.LogicalName != "contact")
      {
        tracingService.Trace("Entity is not contact - exiting");
        return;
      }

      var isCommentPresent = entity.Attributes.ContainsKey("new_comment");
      if (isCommentPresent) tracingService.Trace("new_comment is present");
      var isIntegrationPresent = entity.Attributes.ContainsKey("new_forhotspotintegration");
      if (isIntegrationPresent)tracingService.Trace("new_forhotspotintegration is present");

      if (isCreating)
      {
        tracingService.Trace("Entering CREATE subprocess");
        var contactComment = isCommentPresent ? entity.GetAttributeValue<string>("new_comment") : null;
        var contactIntegration = isIntegrationPresent ? entity.GetAttributeValue<OptionSetValue>("new_forhotspotintegration") : null;
        if (!string.IsNullOrEmpty(contactComment) && contactIntegration != null && contactIntegration.Value != 0) return;

        var eLog = new Entity("new_hubspotsynclog");
        eLog.Attributes["new_contactid"] = new EntityReference("contact", entity.Id);
        eLog.Attributes["new_dateandtimeofsync"] = DateTime.Now;
        client.Create(eLog);
        return;
      }

      if (!isCommentPresent && !isIntegrationPresent)return;
      tracingService.Trace("Entering UPDATE subprocess");

      var query = "<fetch top='1'>" +
                  "<entity name='new_hubspotsynclog'>" +
                  "<filter>" +
                  "<condition attribute='new_contactid' operator='eq' " +
                  $"value='{entity.Id}' uitype='contact' />" +
                  "</filter>" +
                  "</entity>" +
                  "</fetch>";
      var logResult = client.RetrieveMultiple(new FetchExpression(query));
      var logExists = logResult.Entities.Count != 0;

      if (logExists)
      {
        tracingService.Trace("Log does exists");
        var logEntry = logResult.Entities.First();
        logEntry["new_dateandtimeofsync"] = DateTime.Now;
        client.Update(logEntry);
      }
      else
      {
        tracingService.Trace("Log does not exists, creating new entry");
        var logEntry = new Entity("new_hubspotsynclog");
        logEntry["new_contactid"] = new EntityReference("contact", entity.Id);
        logEntry["new_dateandtimeofsync"] = DateTime.UtcNow;
        client.Create(logEntry);
      }
    }

    #endregion
  }
}