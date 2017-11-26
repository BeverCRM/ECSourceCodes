using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice_sales_Rollup_Fields
{
   public  class update_Sales_Invoice_Line : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {

                Entity entity = (Entity)context.InputParameters["Target"];

                Entity invocie2 = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("new_invoice_n"));

                EntityReference invoice_ref = (EntityReference)invocie2["new_invoice_n"];

                var request = new OrganizationRequest("new_calculate_sales_invoice_rollup_fields")
                {
                    ["Target"] = invoice_ref
                };

                service.Execute(request);
            }
        }
    }
}
