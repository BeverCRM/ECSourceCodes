
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Count_prod_purchase_fields_Inport
{
    public class onCreate_Count_Prod_purchase_fiels : IPlugin
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


                if (!prod_purchase_entity.Contains("new_prod")) 
                {
                    Guid invoice_id = ((EntityReference)prod_purchase_entity["new_invoice_n"]).Id;
                    string invoice_name = ((EntityReference)prod_purchase_entity["new_invoice_n"]).LogicalName;

                    Entity invoice_entity = service.Retrieve(invoice_name, invoice_id, new ColumnSet("transactioncurrencyid", "new_ratechange", "new_metric_shipment", "new_ndc"));

                    double Cost = 0;
                    double quantity = 0;
                    Guid? _cur_Id = null;
                    Double matric = 0;

                    if (prod_purchase_entity.Contains("new_product_id"))
                    {
                        prod_purchase_entity["new_sales"] = 0.0;
                        string articule = prod_purchase_entity["new_product_id"].ToString();
 
                        QueryExpression _Query_0 = new QueryExpression
                        {
                            EntityName = "product",
                            ColumnSet = new ColumnSet("productid", "name", "new_timelife"),
                            Criteria =
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                 new ConditionExpression
                                 {
                                    AttributeName ="productnumber",
                                    Operator=ConditionOperator.Equal,
                                    Values={ articule }
                                 },

                                }
                            }

                        };

                        EntityCollection product = service.RetrieveMultiple(_Query_0);
                        foreach(Entity prod_entity in product.Entities)
                        {
                            prod_purchase_entity["new_prod"] = new EntityReference(prod_entity.LogicalName,prod_entity.Id);

                           if(prod_entity.Contains("new_timelife") && prod_entity["new_timelife"] != null)
                           {
                                prod_purchase_entity["new_term_of_storage"] = Convert.ToInt32(prod_entity["new_timelife"]);
                                if (prod_purchase_entity.Contains("new_date_of_manufacture") && prod_purchase_entity["new_date_of_manufacture"] != null)
                                {
                                    DateTime date_of_manufacture = (DateTime)prod_purchase_entity["new_date_of_manufacture"];
                                    int month = Convert.ToInt32(prod_entity["new_timelife"]);
                                    date_of_manufacture = date_of_manufacture.AddMonths(24);
                                    prod_purchase_entity["new_best_before"] = date_of_manufacture;
                                }
                           }
                            
                        }
                    }
                    if (invoice_entity.Contains("new_ratechange"))
                    {
                        prod_purchase_entity["new_change_rate"] = Convert.ToDouble(invoice_entity["new_ratechange"]);
                    }

                    if(prod_purchase_entity.Contains("new_ship_cost"))
                        matric  = Convert.ToDouble(prod_purchase_entity["new_ship_cost"]);
                    if (prod_purchase_entity.Contains("new_cost"))
                         Cost = Convert.ToDouble(((Money)prod_purchase_entity["new_cost"]).Value);
                    if (prod_purchase_entity.Contains("new_qnt"))
                    {
                        quantity = Convert.ToDouble(prod_purchase_entity["new_qnt"]);
                        prod_purchase_entity["new_balance"] = quantity;
                    }


                    if (Cost != 0 && quantity != 0)
                    {
                        prod_purchase_entity["new_sum"] = new Money(Convert.ToDecimal(Cost * quantity));
                    }
                    
                    if (prod_purchase_entity.Contains("transactioncurrencyid") && prod_purchase_entity["transactioncurrencyid"] != null)
                    {
                        EntityReference curr_id = (EntityReference)prod_purchase_entity["transactioncurrencyid"];
                         _cur_Id = curr_id.Id;

                        if (_cur_Id.HasValue && _cur_Id.Value.ToString().ToUpper().Replace("{", "").Replace("}", "") != "6B0B2B0A-256D-E611-8537-0050568856DD")
                        {

                            if (invoice_entity.Contains("new_ndc") && invoice_entity["new_ndc"] != null)
                            {
                                if (((OptionSetValue)invoice_entity["new_ndc"]).Value == 100000001
                                    && Cost != 0 && quantity != 0)
                                {
                                    double nds_1 = (Cost * quantity * 20)/100;
                                    prod_purchase_entity["new_vat_amount"] = new Money(Convert.ToDecimal(nds_1));
                                    prod_purchase_entity["new_amount"] = new Money(Convert.ToDecimal((Cost*quantity) + nds_1));
                                }
                                else
                                {
                                    prod_purchase_entity["new_vat_amount"] = new Money(0);
                                    prod_purchase_entity["new_amount"] = new Money(Convert.ToDecimal(Cost*quantity));
                                }
                            }
                            if (Cost != 0)
                            {
                                prod_purchase_entity["new_cost_amd"] = Convert.ToDouble(Cost * Convert.ToDouble(invoice_entity["new_ratechange"]));
                                prod_purchase_entity["new_totalcost_ship"] = Convert.ToDecimal(matric * Cost);
                                prod_purchase_entity["new_cost_price"] = Convert.ToDouble((matric * Cost) + Cost);
                               
                                prod_purchase_entity["new_sum_amd"] = Convert.ToDouble(((matric * Cost) + Cost) * quantity);
                                prod_purchase_entity["new_rest_sum"] = Convert.ToDecimal(((matric * Cost) + Cost) * quantity);
                            }
                        }
                        else
                        {

                            if (invoice_entity.Contains("new_ndc") && invoice_entity["new_ndc"] != null)
                            {
                                if (((OptionSetValue)invoice_entity["new_ndc"]).Value == 100000001
                                    && Cost != 0 && quantity != 0)
                                {
                                    double nds_1 = (Cost * quantity * 20) / 100;
                                    prod_purchase_entity["new_vat_amount"] = new Money(Convert.ToDecimal(nds_1));
                                    prod_purchase_entity["new_amount"] = new Money(Convert.ToDecimal((Cost * quantity) + nds_1));
                                }
                                else
                                {
                                    prod_purchase_entity["new_vat_amount"] = new Money(0);
                                    prod_purchase_entity["new_amount"] = new Money(Convert.ToDecimal(Cost * quantity));
                                }
                            }
                            double cost_price = Convert.ToDouble(Cost)
                                    + Convert.ToDouble((matric * Cost));
                                prod_purchase_entity["new_cost_amd"] = Convert.ToDouble(Cost);
                                prod_purchase_entity["new_totalcost_ship"] = Convert.ToDecimal(matric * Cost);
                                prod_purchase_entity["new_cost_price"] = cost_price;    
                                prod_purchase_entity["new_sum_amd"] = Convert.ToDouble(cost_price * quantity);
                                prod_purchase_entity["new_rest_sum"] = Convert.ToDecimal(cost_price * quantity);

                        }
                    }

                  //  service.Update(prod_purchase_entity);
                }
            }
        }
    }
}
