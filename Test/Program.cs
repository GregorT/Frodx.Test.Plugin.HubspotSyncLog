using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Tooling.Connector;

namespace Test
{
  class Program
  {
    static void Main(string[] args)
    {


      var aaa = new CrmServiceClient("admin",CrmServiceClient.MakeSecureString("Gt09602k."),"","Frodxtest1",false,true,null,true);
      var prxy = aaa.OrganizationServiceProxy;

      CrmServiceClient crmSvc = new CrmServiceClient("admin", CrmServiceClient.MakeSecureString("Gt09602k."), "NorthAmerica", "UniqueOrgName", isOffice365: true);

      
    }
  }
}
