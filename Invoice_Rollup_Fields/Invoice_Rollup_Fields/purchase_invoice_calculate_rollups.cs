using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice_Rollup_Fields
{
    public class purchase_invoice_calculate_rollups : CodeActivity
    {
       double nettoAmount = 0;
       double vat = 0;
       double Amount = 0;
       Guid? currencyId = null;
        double amountRub = 0;
        double rate = 0;
        int procent = 0;


        protected override void Execute(CodeActivityContext executionContext)
        {

            // Create the tracing service
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            // Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();

            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();

            // Create the Organiztion service
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            IExecutionContext crmContext = executionContext.GetExtension<IExecutionContext>();


            EntityReference invoice_reference = (EntityReference)crmContext.InputParameters["Target"];

            Entity invoice_entity = service.Retrieve(invoice_reference.LogicalName, invoice_reference.Id, new ColumnSet("transactioncurrencyid", "new_ratechange", "new_ship_cost", "new_ship_arm", "new_sum_amd", "new_supplier", "new_sum", "new_ndc"));
            
            if (invoice_entity.LogicalName != "new_purchase")
                return;
            //get Currency entity id
            if (invoice_entity.Contains("transactioncurrencyid") && invoice_entity["transactioncurrencyid"] != null) {
                EntityReference currency_entity = (EntityReference)invoice_entity["transactioncurrencyid"];
                currencyId = currency_entity.Id;
            }


            try
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
                                    Values={ invoice_entity.Id}
                                 }


                                 }
                            }
                };

                EntityCollection _Entities = service.RetrieveMultiple(_Query_0);
                
                foreach (Entity invoiceLines in _Entities.Entities)
                {
                    if(invoiceLines.Contains("new_sum") && invoiceLines["new_sum"] !=null)
                        nettoAmount += Convert.ToDouble(((Money)(invoiceLines["new_sum"])).Value);

                    if (invoiceLines.Contains("new_vat_amount") && invoiceLines["new_vat_amount"] != null)
                        vat += Convert.ToDouble(((Money)(invoiceLines["new_vat_amount"])).Value);


                    if (invoiceLines.Contains("new_amount") && invoiceLines["new_amount"] != null)
                        Amount += Convert.ToDouble(((Money)(invoiceLines["new_amount"])).Value);                
                   
                }
                invoice_entity["new_sum_without_nds"] = Convert.ToDecimal(nettoAmount);

                if (currencyId.HasValue && currencyId.Value.ToString().ToUpper().Replace("{", "").Replace("}", "") != "6B0B2B0A-256D-E611-8537-0050568856DD")
                {
                    //set Amount value
                    invoice_entity["new_sum"] = new Money(Convert.ToDecimal(Amount)); //Convert.ToDecimal(Amount);

                    //Amount * rate = Amount(AMD)
                    if (invoice_entity.Contains("new_ratechange") && invoice_entity.Contains("new_sum"))
                    {
                        amountRub = Convert.ToDouble(((Money)(invoice_entity["new_sum"])).Value);
                        rate = Convert.ToDouble(invoice_entity["new_ratechange"]);
                        invoice_entity["new_sum_amd"] = Convert.ToDecimal(rate * amountRub);
                    }

                    //set amount to border value
                    double am = rate * amountRub;
                    if (invoice_entity.Contains("new_ship_cost"))//&& invoice_entity["new_ship_cost"] != null
                    {

                        double ship_ost = Convert.ToDouble(invoice_entity["new_ship_cost"]);
                        invoice_entity["new_sum_to_border"] = Convert.ToDecimal(am + ship_ost);
                    }
                    else
                    {
                        invoice_entity["new_sum_to_border"] = Convert.ToDecimal(am);
                    }

                    //set value total in purchase

                    if (invoice_entity.Contains("new_ship_arm") && invoice_entity.Contains("new_sum_to_border")) //&& invoice_entity["new_ship_arm"] != null && invoice_entity["new_sum_to_border"] != null
                    {
                        invoice_entity["new_total_amd"] = Convert.ToDouble(invoice_entity["new_ship_arm"]) + Convert.ToDouble(invoice_entity["new_sum_to_border"]);
                    }
                    else
                    {
                        invoice_entity["new_total_amd"] = Convert.ToDouble(invoice_entity["new_sum_to_border"]);
                    }


                    //set metric shipment

                    if (invoice_entity.Contains("new_sum_amd") && Convert.ToDouble(invoice_entity["new_sum_amd"]) != 0
                        && invoice_entity.Contains("new_ship_arm") && invoice_entity.Contains("new_ship_cost"))
                    {
                        invoice_entity["new_metric_shipment"] = (Convert.ToDouble(invoice_entity["new_ship_cost"]) + Convert.ToDouble(invoice_entity["new_ship_arm"])) / Convert.ToDouble(invoice_entity["new_sum_amd"]);
                    }
                    else
                        invoice_entity["new_metric_shipment"] = 0.0;
                    
                }
                else if (currencyId.HasValue && currencyId.Value.ToString().ToUpper().Replace("{", "").Replace("}", "") == "6B0B2B0A-256D-E611-8537-0050568856DD")
                {
                    if (nettoAmount != null)
                    {
                         invoice_entity["new_sum_without_nds"] = nettoAmount; // sum withaout vat
                        decimal sum_amd = 0;

                        if (vat != null)
                        {
                            sum_amd = Convert.ToDecimal(vat) + Convert.ToDecimal(nettoAmount);
                            invoice_entity["new_sum_amd"] = sum_amd;
                         }
                        if (invoice_entity.Contains("new_ship_arm"))
                        {
                            if(sum_amd != 0)
                               invoice_entity["new_metric_shipment"] = Convert.ToDouble(invoice_entity["new_ship_arm"]) / Convert.ToDouble(sum_amd);
                            else
                            {
                                invoice_entity["new_metric_shipment"] = 0.0;
                                invoice_entity["new_rest_of_sum"] = new Money(0); ;
                            }
                                

                            invoice_entity["new_total_amd"] = Convert.ToDouble(sum_amd)  + Convert.ToDouble(invoice_entity["new_ship_arm"]);
                            
                        }
                          
                    }

                }

                //set vat value dependence from currency
                if (currencyId.HasValue && currencyId.Value.ToString().ToUpper().Replace("{", "").Replace("}", "") == "6B0B2B0A-256D-E611-8537-0050568856DD")
                {
                    if (invoice_entity.Contains("new_ndc") && ((OptionSetValue)invoice_entity["new_ndc"]).Value == 100000001)
                        invoice_entity["new_vat_credit"] = Convert.ToDecimal(vat);
                    else
                        invoice_entity["new_vat_credit"] = 0.0;
                }
                else if (invoice_entity.Contains("new_sum_to_border"))
                {
                    invoice_entity["new_tax"] = (Convert.ToDecimal(invoice_entity["new_sum_to_border"]) * 20) / 100;
                }


                //get Organization entity id
                if (invoice_entity.Contains("new_supplier") && invoice_entity["new_supplier"] != null)
                {
                    EntityReference suplier_entitys = (EntityReference)invoice_entity["new_supplier"];

                    Entity supliar_entity = service.Retrieve(suplier_entitys.LogicalName, suplier_entitys.Id, new ColumnSet("new_suply_advance"));

                    if (invoice_entity.Contains("new_sum_amd") && invoice_entity["new_sum_amd"] != null && currencyId.HasValue
                        && currencyId.Value.ToString().ToUpper().Replace("{", "").Replace("}", "") == "6B0B2B0A-256D-E611-8537-0050568856DD")
                    {
                        double sum_amd = Convert.ToDouble(invoice_entity["new_sum_amd"]);

                        //set new_advance_sum and advancet_sum_value
                        double advancet_sum = (sum_amd * Convert.ToInt32(supliar_entity["new_suply_advance"])) / 100;
                        invoice_entity["new_advance_sum"] = new Money(Convert.ToDecimal(advancet_sum));

                        //set rest_of_sum and advancet_sum_value
                        double rest_of_amount = sum_amd - advancet_sum;
                        invoice_entity["new_rest_of_sum"] = new Money(Convert.ToDecimal(rest_of_amount));

                        double rest_sum = advancet_sum + rest_of_amount;
                        invoice_entity["new_rest_sum"] = new Money(Convert.ToDecimal(rest_sum));

                    }
                    else
                    {
                        
                        double sum_rub = Convert.ToDouble(((Money)(invoice_entity["new_sum"])).Value);

                        //set new_advance_sum and advancet_sum_value
                        double advancet_sum_rub = (sum_rub * Convert.ToInt32(supliar_entity["new_suply_advance"])) / 100;
                        invoice_entity["new_advance_sum"] = new Money(Convert.ToDecimal(advancet_sum_rub));

                        //set rest_of_sum and advancet_sum_value
                        double rest_of_amount_rub = sum_rub - advancet_sum_rub;
                        invoice_entity["new_rest_of_sum"] = new Money(Convert.ToDecimal(rest_of_amount_rub));

                        double rest_sum_rub = advancet_sum_rub + rest_of_amount_rub;
                        invoice_entity["new_rest_sum"] = new Money(Convert.ToDecimal(rest_sum_rub));
                    }
                }






                service.Update(invoice_entity);


                //Uppdate Products after product save

                QueryExpression _Query_1 = new QueryExpression
                {
                    EntityName = "new_prod_purchase",
                    ColumnSet = new ColumnSet("new_cost_amd", "new_qnt"),
                    Criteria =
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                 new ConditionExpression
                                 {
                                    AttributeName ="new_invoice_n",
                                    Operator=ConditionOperator.Equal,
                                    Values={ invoice_entity.Id}
                                 }


                                 }
                            }
                };

                EntityCollection _Entities_product = service.RetrieveMultiple(_Query_1);

                foreach (Entity productLines in _Entities_product.Entities)
                {
                    if (productLines.Contains("new_cost_amd") &&  invoice_entity.Contains("new_metric_shipment") && productLines.Contains("new_qnt"))
                    {
                        double totalCost = Convert.ToDouble(productLines["new_cost_amd"]) * Convert.ToDouble(invoice_entity["new_metric_shipment"]);
                        productLines["new_totalcost_ship"] = totalCost;
                        double costPrice = totalCost + Convert.ToDouble(productLines["new_cost_amd"]);
                        productLines["new_cost_price"] = costPrice;
                        double sum_amd = costPrice * Convert.ToDouble(productLines["new_qnt"]);
                        productLines["new_sum_amd"] = sum_amd;
                        productLines["new_rest_sum"] = sum_amd;

                        if (invoice_entity.Contains("new_ndc") && ((OptionSetValue)invoice_entity["new_ndc"]).Value == 100000001)
                        {
                            double vat = (sum_amd * 20) / 100;
                            productLines["new_vat_amd"] = Convert.ToDecimal(vat);
                            double total = sum_amd + vat;
                            productLines["new_total_amd"] = Convert.ToDecimal(total);
                        }
                        else
                        {
                            productLines["new_vat_amd"] = 0.0;
                            productLines["new_total_amd"] = Convert.ToDecimal(sum_amd);
                        }
                        productLines["new_ship_cost"] = Convert.ToDouble(invoice_entity["new_metric_shipment"]);

                        service.Update(productLines);
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
