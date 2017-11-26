using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse_Management_EC
{
    public class Delete_Product_In_Sales : IPlugin
    {
        Guid? productId = null;
        Guid? prod_purchase_id = null;
        public void Execute(IServiceProvider serviceProvider)
        {

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                {
                    EntityReference invoice_entity_ref = (EntityReference)context.InputParameters["Target"];

                    Entity product_sales_entity = service.Retrieve(invoice_entity_ref.LogicalName,invoice_entity_ref.Id,new ColumnSet("new_product", "new_quantity"));

                        if (product_sales_entity.LogicalName != "new_sale_product")
                            return;
                    try
                    {
                    if (product_sales_entity.Contains("new_product") && product_sales_entity["new_product"] != null)
                    {
                            EntityReference product_entity = (EntityReference)product_sales_entity["new_product"];
                            productId = product_entity.Id;
                    }

                    QueryExpression _Query_0 = new QueryExpression
                    {
                            EntityName = "new_extract_from_warehouse",
                            ColumnSet = new ColumnSet("new_amount", "new_profitability", "new_product_purchase", "new_product_in_sales_id", "new_warehouse_prod"),         
                            Criteria =
                                {
                                    FilterOperator = LogicalOperator.And,
                                    Conditions =
                                    {
                                      new ConditionExpression("new_product_in_sales_id", ConditionOperator.Equal, invoice_entity_ref.Id)
                                    }                            
                                }
                    };

                    EntityCollection _Entities = service.RetrieveMultiple(_Query_0);

                    double extract_quantity = 0 , profitability = 0;

                    foreach (Entity extractLines in _Entities.Entities)
                    {   
                        if (extractLines.Contains("new_amount"))
                        {
                            extract_quantity = Convert.ToDouble(extractLines["new_amount"]);
                        }
                        if (extractLines.Contains("new_profitability"))
                        {
                            profitability = Convert.ToDouble(extractLines["new_profitability"]);
                        }
                        Guid purchase_product_id = ((EntityReference)extractLines["new_product_purchase"]).Id;

                        Guid rest_of_store_id = ((EntityReference)extractLines["new_warehouse_prod"]).Id;
                        string logical_name = ((EntityReference)extractLines["new_warehouse_prod"]).LogicalName;


                        Entity prod_purchase = service.Retrieve("new_prod_purchase", purchase_product_id, new ColumnSet("new_sale_sum", "new_rest_sum", "new_prod", "new_sales", "new_balance", "new_profitability", "new_cost_price"));//From Here
                            
                            if(prod_purchase.Contains("new_sales") && Convert.ToDouble(prod_purchase["new_sales"]) != 0)
                            {
                                prod_purchase["new_sales"] = Convert.ToDouble(prod_purchase["new_sales"]) - extract_quantity;
                            }
                            if (prod_purchase.Contains("new_balance"))
                            {
                                prod_purchase["new_balance"] = Convert.ToDouble(prod_purchase["new_balance"]) + extract_quantity;
                            }

                        if (Convert.ToDecimal(prod_purchase["new_sale_sum"]) != 0)
                        {
                            prod_purchase["new_sale_sum"] = Convert.ToDecimal(prod_purchase["new_sale_sum"]) - (Convert.ToDecimal(prod_purchase["new_cost_price"]) * Convert.ToDecimal(extractLines["new_amount"]));
                        }

                        prod_purchase["new_rest_sum"] = Convert.ToDecimal(prod_purchase["new_rest_sum"]) + (Convert.ToDecimal(prod_purchase["new_cost_price"]) * Convert.ToDecimal(extractLines["new_amount"]));

                        if (prod_purchase.Contains("new_profitability") && Convert.ToDouble(prod_purchase["new_profitability"]) != 0)
                                prod_purchase["new_profitability"] = Convert.ToDouble(prod_purchase["new_profitability"]) - profitability;

                        Entity rest_of_story  = service.Retrieve(logical_name, rest_of_store_id, new ColumnSet("new_qnt"));

                        if (rest_of_story.Contains("new_qnt"))
                        {
                            rest_of_story["new_qnt"] = Convert.ToDouble(rest_of_story["new_qnt"]) + extract_quantity;
                        }
                        if (prod_purchase.Contains("new_prod"))
                        {
                            if (((EntityReference)prod_purchase["new_prod"]).Id == productId)
                            {
                                service.Update(prod_purchase);
                                service.Update(rest_of_story);
                                service.Delete(extractLines.LogicalName, extractLines.Id);
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
