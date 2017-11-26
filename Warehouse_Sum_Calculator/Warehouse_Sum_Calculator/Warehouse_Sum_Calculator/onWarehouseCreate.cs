using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse_Sum_Calculator
{
    public class onWarehouseCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity Entity = (Entity)context.InputParameters["Target"];

                if (Entity.LogicalName != "new_rest_store")
                    return;

                try
                {
                    if (Entity.Contains("new_purchase_prod") && Entity["new_purchase_prod"] != null)
                    {
                        EntityReference purchaseProdEntityRef = (EntityReference)Entity["new_purchase_prod"];
                        Entity purchaseProdEntity = service.Retrieve(purchaseProdEntityRef.LogicalName, purchaseProdEntityRef.Id, new ColumnSet("new_cost_price"));
                        if (purchaseProdEntity.Contains("new_cost_price") && purchaseProdEntity["new_cost_price"] != null)
                        {
                            Entity["new_cost_prod"] = purchaseProdEntity["new_cost_price"];
                            if (Entity.Contains("new_qnt") && Entity["new_qnt"] != null)
                            {
                                Entity["new_sum_rest"] = (Double)Entity["new_cost_prod"] * Convert.ToDouble((Decimal)Entity["new_qnt"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in the plug-in. " + ex);
                }
            }
        }
    }
}
