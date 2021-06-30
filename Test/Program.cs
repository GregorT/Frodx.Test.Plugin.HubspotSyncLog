using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace Test
{
  internal class Program
  {
    #region Private

    private static void Main(string[] args)
    {
      var sEnvironment = "https://frodxtest1.crm4.dynamics.com";
      var sUser = "api@gregort.onmicrosoft.com";
      var sPass = "demo123.";

      var connString = $" Url = {sEnvironment};AuthType = OAuth;UserName = {sUser}; Password = {sPass};AppId = 51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri = app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto; RequireNewInstance = True";

      using (var service = new CrmServiceClient(connString))
      {
        var query = "<fetch>" +
                    "<entity name='new_hubspotsynclog' >" +
                    "<attribute name='new_dateandtimeofsync' />" +
                    "<link-entity name='contact' from='contactid' to='new_contactid' link-type='inner' >" +
                    "<attribute name='firstname' />" +
                    "<attribute name='lastname' />" +
                    "<attribute name='new_comment' />" +
                    "<attribute name='new_forhotspotintegration' />" +
                    "<filter>" +
                    "<condition attribute='new_forhotspotintegrationname' operator='eq' value='Yes' />" +
                    "</filter>" +
                    "</link-entity>" +
                    "</entity>" +
                    "</fetch>";
        var logResult = service.RetrieveMultiple(new FetchExpression(query));

        foreach (var entity in logResult.Entities)
        {
          var firstName = ((AliasedValue) entity.Attributes["contact1.firstname"]).Value;
          var lastName = ((AliasedValue) entity.Attributes["contact1.lastname"]).Value;
          var comment = ((AliasedValue) entity.Attributes["contact1.new_comment"]).Value;
          var integration = ((AliasedValue) entity.Attributes["contact1.new_forhotspotintegration"]).Value;
          var dateAndTime = entity.Attributes["new_dateandtimeofsync"];
          Console.WriteLine($"{firstName} {lastName} - {comment} - {integration} - {dateAndTime}");
        }
      }

      Console.WriteLine("-------- end of list ---------");
      Console.WriteLine("Press ENTER to finish");
      Console.ReadLine();
    }

    #endregion
  }
}