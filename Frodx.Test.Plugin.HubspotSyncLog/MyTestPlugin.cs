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

      if (context.InputParameters.Contains("Target") == false) return;
      if (context.InputParameters["Target"] is Entity == false) return;
      var isCreating = context.MessageName == "Create";
      var isUpdating = context.MessageName == "Update";

      var entity = (Entity) context.InputParameters["Target"];
      if (entity.LogicalName != "contact") return;
      if (!isCreating && !isUpdating) return;

      var isCommentPresent = entity.Attributes.ContainsKey("new_comment");
      var isIntegrationPresent = entity.Attributes.ContainsKey("new_forhotspotintegration");

      if (isCreating)
      {
        var contactComment = isCommentPresent ? entity.GetAttributeValue<string>("new_comment") : null;
        var contactIntegration = isIntegrationPresent ? entity.GetAttributeValue<OptionSetValue>("new_forhotspotintegration") : null;
        if (!string.IsNullOrEmpty(contactComment) && contactIntegration != null && contactIntegration.Value != 0) return;

        var eLog = new Entity("new_hubspotsynclog");
        eLog.Attributes["new_contactid"] = entity.Id;
        eLog.Attributes["new_dateandtimeofsync"] = DateTime.Now;
        client.Create(eLog);
        return;
      }

      if (!isCommentPresent && !isIntegrationPresent)return;

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
        var logEntry = logResult.Entities.First();
        logEntry["new_contactid"] = entity.Id;
        logEntry["new_dateandtimeofsync"] = DateTime.Now;
        client.Update(logEntry);
      }
      else
      {
        var logEntry = new Entity("new_hubspotsynclog");
        logEntry["new_contactid"] = entity.Id;
        logEntry["new_dateandtimeofsync"] = DateTime.UtcNow;
        client.Create(logEntry);
      }
    }

    #endregion
  }
}