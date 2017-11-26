using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_plugin
{
    public class add_total : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
          
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity purchase_entity = (Entity)context.InputParameters["Target"];


                if (purchase_entity.LogicalName != "new_purchase")
                    return;
                try
                {
                    Entity purch_entity = service.Retrieve(purchase_entity.LogicalName, purchase_entity.Id, new ColumnSet("new_date_billing"));

                    if (purch_entity.Contains("new_date_billing") && purch_entity["new_date_billing"] != null)
                    {
                        QueryExpression _Query_0 = new QueryExpression
                        {
                            EntityName = "new_prod_purchase",
                            ColumnSet = new ColumnSet("new_sum", "new_vat_amount", "new_amount"),
                            Criteria =
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                 new ConditionExpression
                                 {
                                    AttributeName ="new_invoice_n",
                                    Operator=ConditionOperator.Equal,
                                    Values={ purchase_entity.Id}
                                 }

                                }
                            }
                        };


                        EntityCollection _Entities_prod_purchase = service.RetrieveMultiple(_Query_0);
  


                        foreach (Entity product_purchase_entity in _Entities_prod_purchase.Entities)
                        {
                            if (product_purchase_entity.Contains("new_vat_amount") && product_purchase_entity.Contains("new_sum"))
                            {
                                product_purchase_entity["new_amount"] = new Money(((Money)product_purchase_entity["new_sum"]).Value + ((Money)product_purchase_entity["new_vat_amount"]).Value);
                            }
                            else
                            {
                                product_purchase_entity["new_amount"] = new Money(((Money)product_purchase_entity["new_sum"]).Value);
                                product_purchase_entity["new_vat_amount"] = new Money(0);
                            }

                            service.Update(product_purchase_entity);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
               
            }
        }
    }
}
