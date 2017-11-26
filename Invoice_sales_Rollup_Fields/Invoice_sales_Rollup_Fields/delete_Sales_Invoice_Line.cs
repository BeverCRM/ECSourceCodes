using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice_sales_Rollup_Fields
{
    public class delete_Sales_Invoice_Line : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.PreEntityImages.Contains("PreImage1"))
            {
                Entity entity = (Entity)context.PreEntityImages["PreImage1"];

                if (entity.Contains("new_invoice_n") && entity["new_invoice_n"] != null)
                {
                    EntityReference invoice_ref = (EntityReference)entity["new_invoice_n"];

                    var request = new OrganizationRequest("new_calculate_sales_invoice_rollup_fields")
                    {
                        ["Target"] = invoice_ref
                    };

                    service.Execute(request);
                }
            }
        }
    }
}
