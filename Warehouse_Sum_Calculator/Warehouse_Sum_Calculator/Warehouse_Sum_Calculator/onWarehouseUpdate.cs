using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse_Sum_Calculator
{
    public class onWarehouseUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity Entity1 = (Entity)context.InputParameters["Target"];

                if (Entity1.LogicalName != "new_rest_store")
                    return;

                if (context.Depth > 2) { return; }

                try
                {
                    Entity Entity = service.Retrieve(Entity1.LogicalName, Entity1.Id, new ColumnSet("new_purchase_prod", "new_cost_prod", "new_qnt", "new_sum_rest"));
                    if (Entity.Contains("new_qnt") && Entity["new_qnt"] != null)
                    {
                        Entity1["new_sum_rest"] = (Double)Entity["new_cost_prod"] * Convert.ToDouble((Decimal)Entity["new_qnt"]);
                    }
                    service.Update(Entity1);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in the plug-in. " + ex);
                }
            }
        }
    }
}
