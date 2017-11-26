using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alik_plugin_create_move_entity
{
    public class onCreate_move_entity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity prod_purchase_entity = (Entity)context.InputParameters["Target"];

                if (prod_purchase_entity.LogicalName != "new_prod_purchase")
                    return;
              
                if (prod_purchase_entity.Contains("new_invoice_n") && prod_purchase_entity["new_invoice_n"] != null)
                {
                    Guid Id = ((EntityReference)prod_purchase_entity["new_invoice_n"]).Id;
                    string logicalName = ((EntityReference)prod_purchase_entity["new_invoice_n"]).LogicalName;
        
                    Entity invoice = service.Retrieve(logicalName, Id, new ColumnSet("new_warehouse", "new_ship_store"));
                    
                    if (invoice.Contains("new_ship_store") && invoice["new_ship_store"] != null)
                    {
                        
                        Entity move_from_story = new Entity("new_move");

                        if (prod_purchase_entity.Contains("new_product_id"))
                        {
                            move_from_story["new_article"] = prod_purchase_entity["new_product_id"].ToString();
                        }

                        if (prod_purchase_entity.Contains("new_qnt"))
                        {
                            move_from_story["new_qnt"] = Convert.ToDecimal(prod_purchase_entity["new_qnt"]);
                        }

                        if (prod_purchase_entity.Contains("new_prod") && prod_purchase_entity["new_prod"] != null)
                        {
                            move_from_story["new_prod"] = new EntityReference(((EntityReference)prod_purchase_entity["new_prod"]).LogicalName, ((EntityReference)prod_purchase_entity["new_prod"]).Id);
                        }
                        if (invoice.Contains("new_warehouse") && invoice["new_warehouse"] != null)
                        {
                            move_from_story["new_warehouse_to"] = new EntityReference(((EntityReference)invoice["new_warehouse"]).LogicalName, ((EntityReference)invoice["new_warehouse"]).Id);
                        }
                        
                             move_from_story["new_date"] = (DateTime)invoice["new_ship_store"];
                       
                        if (prod_purchase_entity.LogicalName != null && prod_purchase_entity.Id != null)
                        {                         
                            move_from_story["new_purchase_prod"] = new EntityReference(prod_purchase_entity.LogicalName, prod_purchase_entity.Id);
                            service.Create(move_from_story);
                        }
                    }
                }
            }
        }
    }
}
