using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse_Management_EC
{
    public class onCreate_Product_In_Sales : IPlugin
    {
        Guid? productId = null;
        public void Execute(IServiceProvider serviceProvider)
        {

            Guid? productId = null;
            Guid? stock_id = null;
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {

                Entity prod_sale_entity = (Entity)context.InputParameters["Target"];

                if (prod_sale_entity.LogicalName != "new_sale_product")
                    return;

                if (prod_sale_entity.Contains("new_product") && prod_sale_entity["new_product"] != null)
                {
                    EntityReference product_entity = (EntityReference)prod_sale_entity["new_product"];
                    productId = product_entity.Id;
                }
                if (prod_sale_entity.Contains("new_invoice_n") && prod_sale_entity["new_invoice_n"] != null)
                {
                    EntityReference invoice_link = (EntityReference)prod_sale_entity["new_invoice_n"];

                    Entity invoice = service.Retrieve(invoice_link.LogicalName, invoice_link.Id, new ColumnSet("new_stock"));
                    if (invoice.Contains("new_stock") && invoice["new_stock"] != null)
                        stock_id = ((EntityReference)invoice["new_stock"]).Id;

                }

                if (stock_id != null)
                {
                    QueryExpression _Query_0 = new QueryExpression
                    {
                        EntityName = "new_rest_store",
                        ColumnSet = new ColumnSet("new_name", "new_purchase_prod", "new_rest_storeid", "new_qnt", "new_warehouse"),
                        Criteria =
                                    {
                                        FilterOperator = LogicalOperator.And,
                                        Conditions =
                                        {
                                          new ConditionExpression
                                          {
                                             AttributeName ="new_prod",
                                             Operator=ConditionOperator.Equal,
                                              Values={ productId }
                                          },
                                          new ConditionExpression
                                          {
                                            AttributeName ="new_qnt",
                                            Operator=ConditionOperator.NotEqual,
                                            Values={ Convert.ToDecimal(0) }
                                          },
                                          new ConditionExpression
                                          {
                                            AttributeName ="new_warehouse",
                                            Operator=ConditionOperator.Equal,
                                            Values={ stock_id }
                                          }


                                        }
                                    }

                    };

                    EntityCollection _Entities_rest_store = service.RetrieveMultiple(_Query_0);

                    Entity from_warehouse = new Entity("new_extract_from_warehouse");


                    var date_map = new Dictionary<Guid, DateTime>();

                    foreach (Entity restStoreLines in _Entities_rest_store.Entities)
                    {
                        if (restStoreLines.Contains("new_purchase_prod") && restStoreLines["new_purchase_prod"] != null)
                        {
                            Guid prod_purchase_id = ((EntityReference)restStoreLines["new_purchase_prod"]).Id;
                            string prod_purchase_name = ((EntityReference)restStoreLines["new_purchase_prod"]).LogicalName;

                            Entity invoice_purchace_link = service.Retrieve(prod_purchase_name, prod_purchase_id, new ColumnSet("new_invoice_n"));

                            if (invoice_purchace_link.Contains("new_invoice_n"))
                            {
                                Guid invoice_purchase_id = ((EntityReference)invoice_purchace_link["new_invoice_n"]).Id;
                                string invoice_purchase_name = ((EntityReference)invoice_purchace_link["new_invoice_n"]).LogicalName;

                                if (invoice_purchase_id != null && prod_purchase_id != null)
                                {
                                    Entity invoice_purchase = service.Retrieve(invoice_purchase_name, invoice_purchase_id, new ColumnSet("new_ship_store"));

                                    Guid rest_of_store_id = (Guid)restStoreLines["new_rest_storeid"];
                                    if (invoice_purchase.Contains("new_ship_store") && invoice_purchase["new_ship_store"] != null)
                                    {

                                        date_map.Add(rest_of_store_id, (DateTime)invoice_purchase["new_ship_store"]);
                                    }
                                }
                            }
                        }
                    }

                    if (date_map != null)
                    {
      
                        var result = date_map.OrderBy(d => d.Value).Select(d => new
                        {
                            id = d.Key,
                            date = d.Value
                        });


                        int prod_length = 1;
                        double balance_all = 0;

                        for (int i = 0; i < prod_length; i++)
                        {
                            Guid ID = result.ElementAt(i).id;

                            Entity rest_of_store_entity = service.Retrieve("new_rest_store", ID, new ColumnSet("new_qnt"));

                            if (rest_of_store_entity.Contains("new_qnt"))
                            {
                                double balance = Convert.ToDouble(rest_of_store_entity["new_qnt"]);
                                balance_all += balance;
                                double quantity_1 = Convert.ToDouble(prod_sale_entity["new_quantity"]);
                                if (quantity_1 > balance_all)
                                {
                                    prod_length++;
                                }
                            }
                        }

                        List<double> balance_list = new List<double>();

                        double quantity = Convert.ToDouble(prod_sale_entity["new_quantity"]);

                        double balance_quantity = quantity;

                        for (int i = 0; i < prod_length; i++)
                        {

                            Guid ID = result.ElementAt(i).id;

                            Entity in_entity_rest_of_story = service.Retrieve("new_rest_store", ID, new ColumnSet("new_purchase_prod", "new_prod", "new_qnt", "new_warehouse"));

                            Entity in_entity_product_purchase = service.Retrieve(((EntityReference)(in_entity_rest_of_story["new_purchase_prod"])).LogicalName, ((EntityReference)(in_entity_rest_of_story["new_purchase_prod"])).Id,
                                 new ColumnSet("new_cost_price", "new_balance", "new_sales", "new_rest_sum", "new_sale_sum", "new_sale_sum", "new_profitability"));

                            double balance = Convert.ToDouble(in_entity_rest_of_story["new_qnt"]);

                            balance_list.Add(balance);
                            if (in_entity_rest_of_story.Contains("new_purchase_prod") && in_entity_rest_of_story["new_purchase_prod"] != null)
                                from_warehouse["new_product_purchase"] = new EntityReference(((EntityReference)(in_entity_rest_of_story["new_purchase_prod"])).LogicalName, ((EntityReference)(in_entity_rest_of_story["new_purchase_prod"])).Id);//Product in purchase
                            from_warehouse["new_product_in_sales_id"] = new EntityReference(prod_sale_entity.LogicalName, prod_sale_entity.Id);//product in sales

                            from_warehouse["new_warehouse_prod"] = new EntityReference(in_entity_rest_of_story.LogicalName, in_entity_rest_of_story.Id);

                            //set invoice sale in extract from  warehouse entity
                            if (prod_sale_entity.Contains("new_invoice_n") && prod_sale_entity["new_invoice_n"] != null)
                            {

                                EntityReference invoice_sale = (EntityReference)prod_sale_entity["new_invoice_n"];
                                from_warehouse["new_invoice_sale"] = new EntityReference(invoice_sale.LogicalName, invoice_sale.Id);

                                // set date in warehouse from invoice sale 
                                Entity invoice_sale_entity = service.Retrieve(invoice_sale.LogicalName, invoice_sale.Id, new ColumnSet("new_account_date"));
                                if (invoice_sale_entity.Contains("new_account_date") && invoice_sale_entity["new_account_date"] != null)
                                    from_warehouse["new_date"] = (DateTime)invoice_sale_entity["new_account_date"];

                            }
                            //set price with vat 
                            if (prod_sale_entity.Contains("new_price_without_vat"))
                            {
                                from_warehouse["new_sale_price"] = Convert.ToDouble(((Money)prod_sale_entity["new_price_without_vat"]).Value);
                            }
                            if (prod_sale_entity.Contains("new_account_date") && prod_sale_entity["new_account_date"] != null)
                            {
                                from_warehouse["new_date"] = (DateTime)prod_sale_entity["new_account_date"];
                            }
                            if (in_entity_rest_of_story.Contains("new_purchase_prod") && in_entity_rest_of_story["new_purchase_prod"] != null)
                            {

                                if (in_entity_product_purchase.Contains("new_cost_price"))
                                {
                                    from_warehouse["new_cost_price"] = Convert.ToDouble(in_entity_product_purchase["new_cost_price"]);
                                }

                                if (prod_length == 1)
                                {
                                    from_warehouse["new_amount"] = Convert.ToInt32(prod_sale_entity["new_quantity"]);
                                }
                                else
                                {
                                    if (in_entity_product_purchase.Contains("new_balance"))
                                    {
                                        if (balance < balance_quantity)
                                        {
                                            from_warehouse["new_amount"] = balance_list[i];
                                        }
                                        else
                                        {
                                            from_warehouse["new_amount"] = balance_quantity;
                                        }
                                    }

                                }

                            }

                            if (in_entity_product_purchase.Contains("new_cost_price") && prod_sale_entity.Contains("new_price_without_vat"))
                            {
                                double profitability = (Convert.ToDouble(((Money)prod_sale_entity["new_price_without_vat"]).Value)
                                    - Convert.ToDouble(in_entity_product_purchase["new_cost_price"])) * quantity;


                                if (in_entity_product_purchase.Contains("new_profitability") && in_entity_product_purchase["new_profitability"] != null)
                                {
                                    if (prod_length == 1)
                                    {
                                        double profitability_purchase = Convert.ToDouble(in_entity_product_purchase["new_profitability"]);
                                        in_entity_product_purchase["new_profitability"] = profitability_purchase + profitability;
                                        from_warehouse["new_profitability"] = profitability;
                                        prod_sale_entity["new_new_profitability"] = profitability;

                                    }
                                    else
                                    {
                                        double profitability_product_purchase = 0;

                                        if (balance < balance_quantity)
                                        {
                                            profitability_product_purchase = (Convert.ToDouble(((Money)prod_sale_entity["new_price_without_vat"]).Value)
                                                  - Convert.ToDouble(in_entity_product_purchase["new_cost_price"])) * balance_list[i];
                                        }
                                        else
                                        {
                                            profitability_product_purchase = (Convert.ToDouble(((Money)prod_sale_entity["new_price_without_vat"]).Value)
                                                - Convert.ToDouble(in_entity_product_purchase["new_cost_price"])) * balance_quantity;
                                        }

                                        double profitability_purchase = Convert.ToDouble(in_entity_product_purchase["new_profitability"]);
                                        in_entity_product_purchase["new_profitability"] = profitability_purchase + profitability_product_purchase;

                                        from_warehouse["new_profitability"] = profitability_product_purchase;

                                        if (prod_sale_entity.Contains("new_new_profitability"))
                                        {
                                            double profibility_2 = Convert.ToDouble(prod_sale_entity["new_new_profitability"]);
                                            prod_sale_entity["new_new_profitability"] = profibility_2 + profitability_product_purchase;
                                        }

                                    }
                                }
                                else
                                {
                                    in_entity_product_purchase["new_profitability"] = profitability;
                                }

                            }

                            double cost_price = Convert.ToDouble(in_entity_product_purchase["new_cost_price"]);


                            if (in_entity_product_purchase["new_balance"] != null)
                            {
                                double bal = Convert.ToDouble(in_entity_product_purchase["new_balance"]);
                                decimal rest_quantity = Convert.ToDecimal(in_entity_rest_of_story["new_qnt"]);

                                if (bal <= balance_quantity)
                                {
                                    in_entity_product_purchase["new_balance"] = Convert.ToDouble(0);
                                    in_entity_product_purchase["new_rest_sum"] = Convert.ToDouble(0);
                                    in_entity_rest_of_story["new_qnt"] = Convert.ToDecimal(0);
                                    in_entity_product_purchase["new_sales"] = bal;
                                    in_entity_product_purchase["new_sale_sum"] = Convert.ToDouble(cost_price * (Convert.ToDouble(in_entity_product_purchase["new_sales"])));
                                    balance_quantity -= bal;
                                }
                                else
                                {
                                    if (prod_length != 1)
                                    {
                                        in_entity_product_purchase["new_balance"] = bal - balance_quantity;
                                        in_entity_rest_of_story["new_qnt"] = rest_quantity - Convert.ToDecimal(balance_quantity);
                                        in_entity_product_purchase["new_rest_sum"] = (cost_price * (bal - balance_quantity));
                                        if (in_entity_product_purchase.Contains("new_sales") && in_entity_product_purchase["new_sales"] != null)
                                            in_entity_product_purchase["new_sales"] = Convert.ToDouble(in_entity_product_purchase["new_sales"]) + balance_quantity;
                                        else
                                            in_entity_product_purchase["new sales"] = balance_quantity;
                                        in_entity_product_purchase["new_sale_sum"] = Convert.ToDouble(cost_price * (Convert.ToDouble(in_entity_product_purchase["new_sales"])));
                                        balance_quantity = 0;

                                    }
                                    else if (prod_length == 1)
                                    {
                                        in_entity_product_purchase["new_balance"] = bal - quantity;
                                        in_entity_rest_of_story["new_qnt"] = Convert.ToDecimal(balance - quantity);
                                        in_entity_product_purchase["new_rest_sum"] = Convert.ToDouble(cost_price * (bal - quantity));

                                        if (in_entity_product_purchase["new_sales"] == null)
                                        {

                                            in_entity_product_purchase["new_sales"] = quantity;
                                            in_entity_product_purchase["new_sale_sum"] = Convert.ToDouble(cost_price * quantity);
                                        }
                                        else
                                        {
                                            double sales = Convert.ToDouble(in_entity_product_purchase["new_sales"]);
                                            in_entity_product_purchase["new_sales"] = sales + quantity;
                                            in_entity_product_purchase["new_sale_sum"] = Convert.ToDouble((sales + quantity) * cost_price);
                                        }
                                    }

                                }

                                if (bal != 0)
                                {
                                    service.Update(in_entity_rest_of_story);
                                    service.Create(from_warehouse);
                                    service.Update(in_entity_product_purchase);
                                    service.Update(prod_sale_entity);

                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
