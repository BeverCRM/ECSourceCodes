using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alik_plugin_create_move_entity
{
    public class update_purchase_entity_then_create_move_entity :IPlugin
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
                    Entity purch_entity = service.Retrieve(purchase_entity.LogicalName, purchase_entity.Id, new ColumnSet("new_warehouse", "new_ship_store"));

                    if (purch_entity.Contains("new_ship_store") && purch_entity["new_ship_store"] != null)
                    {


                        QueryExpression _Query_0 = new QueryExpression
                        {
                            EntityName = "new_prod_purchase",
                            ColumnSet = new ColumnSet("new_invoice_n", "new_product_id", "new_qnt", "new_prod"),
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


                            QueryExpression _Query_1 = new QueryExpression
                            {
                                EntityName = "new_move",
                                ColumnSet = new ColumnSet("new_purchase_prod"),
                                Criteria =
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                 new ConditionExpression
                                 {
                                    AttributeName ="new_purchase_prod",
                                    Operator=ConditionOperator.Equal,
                                    Values={ product_purchase_entity.Id}
                                 }

                                }
                            }
                            };


                            EntityCollection _Entities_move = service.RetrieveMultiple(_Query_1);



                            if (_Entities_move.Entities.Count == 0)
                            {


                                Entity move_from_story = new Entity("new_move");

                                if (product_purchase_entity.Contains("new_product_id"))
                                {
                                    move_from_story["new_article"] = product_purchase_entity["new_product_id"].ToString();
                                }

                                if (product_purchase_entity.Contains("new_qnt"))
                                {
                                    move_from_story["new_qnt"] = Convert.ToDecimal(product_purchase_entity["new_qnt"]);
                                }

                                if (product_purchase_entity.Contains("new_prod") && product_purchase_entity["new_prod"] != null)
                                {
                                    move_from_story["new_prod"] = new EntityReference(((EntityReference)product_purchase_entity["new_prod"]).LogicalName, ((EntityReference)product_purchase_entity["new_prod"]).Id);
                                }
                                if (purch_entity.Contains("new_warehouse") && purch_entity["new_warehouse"] != null)
                                {
                                    move_from_story["new_warehouse_to"] = new EntityReference(((EntityReference)purch_entity["new_warehouse"]).LogicalName, ((EntityReference)purch_entity["new_warehouse"]).Id);
                                }

                                move_from_story["new_date"] = (DateTime)purch_entity["new_ship_store"];

                                if (product_purchase_entity.LogicalName != null && product_purchase_entity.Id != null)
                                {
                                    move_from_story["new_purchase_prod"] = new EntityReference(product_purchase_entity.LogicalName, product_purchase_entity.Id);
                                    service.Create(move_from_story);

                                }
                            }

                        }
                    }

                }
                catch(Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
