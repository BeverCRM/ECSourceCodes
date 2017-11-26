using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alik_Invoice_To_Payment_Entity
{
    public class OnUpdate_Payment_Entity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity invoice_entity = (Entity)context.InputParameters["Target"];


                if (invoice_entity.LogicalName != "new_purchase")
                    return;


                try
                {
                    Entity invoice_2 = service.Retrieve(invoice_entity.LogicalName,invoice_entity.Id,new ColumnSet("new_name", "new_ship_store"));

                    QueryExpression _Query_0 = new QueryExpression
                    {
                        EntityName = "new_suply_pay",
                        ColumnSet = new ColumnSet("new_date", "new_supplier", "transactioncurrencyid", "new_sum_inv", "new_name"),
                        Criteria =
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                 new ConditionExpression
                                 {
                                    AttributeName ="new_n_invoice",
                                    Operator=ConditionOperator.Equal,
                                    Values={ invoice_entity.Id.ToString()}
                                 }


                                 }
                            }
                    };

                    DateTime? date_in_ship_store = null;
                    DateTime? date_invoice_2 = null;
                    EntityReference suplier_entity;
                    EntityReference currency;

                    EntityCollection _Entities = service.RetrieveMultiple(_Query_0);

                    foreach (Entity payment in _Entities.Entities)
                    {

                        if (invoice_entity.Contains("new_ship_store") && invoice_entity["new_ship_store"] != null)
                        {
                            date_in_ship_store = (DateTime)invoice_entity["new_ship_store"];
                            date_in_ship_store = date_in_ship_store.Value.AddHours(6);
                            payment["new_date"] = date_in_ship_store;
                        }
                        else
                        {
                            if (invoice_2.Contains("new_ship_store"))
                            {
                                date_invoice_2 = (DateTime)invoice_2["new_ship_store"];
                                tracingService.Trace(date_invoice_2.ToString());
                                date_invoice_2 = date_invoice_2.Value.AddHours(6);
                                tracingService.Trace(date_invoice_2.ToString());
                                if (date_invoice_2 != null)
                                    payment["new_date"] = date_invoice_2;
                            }
                            else
                            {
                                payment["new_date"] = null;
                            }
                        }
                        if (invoice_entity.Contains("new_supplier") && invoice_entity["new_supplier"] != null)
                        {
                            suplier_entity = (EntityReference)invoice_entity["new_supplier"];
                            payment["new_supplier"] = new EntityReference(suplier_entity.LogicalName, suplier_entity.Id);
                        }
                        if (invoice_entity.Contains("transactioncurrencyid") && invoice_entity["transactioncurrencyid"] != null)
                        {
                            currency = (EntityReference)invoice_entity["transactioncurrencyid"];
                            payment["transactioncurrencyid"] = new EntityReference(currency.LogicalName, currency.Id);
                        }
                        
                        if (invoice_entity.Contains("new_sum") && invoice_entity["new_sum"] != null)
                        {
                            double sum = Convert.ToDouble(((Money)(invoice_entity["new_sum"])).Value);
                            payment["new_sum_inv"] = new Money(Convert.ToDecimal(sum));
                        }

                        if (invoice_2.Contains("new_name") && date_in_ship_store != null)
                        {
                            date_in_ship_store = date_in_ship_store.Value.AddHours(6);
                            payment["new_name"] = invoice_2["new_name"] + ", " + date_in_ship_store.Value.Day + "/" + date_in_ship_store.Value.Month + "/" + date_in_ship_store.Value.Year;
                        }
                        else 
                        {
                            if (invoice_2.Contains("new_ship_store") && invoice_2["new_ship_store"] != null && invoice_2.Contains("new_name"))
                            {
                                date_invoice_2 = (DateTime)invoice_2["new_ship_store"];
                                tracingService.Trace(date_invoice_2.ToString());
                                if (date_invoice_2 != null)
                                {
                                    date_invoice_2 =  date_invoice_2.Value.AddHours(6);
                                    tracingService.Trace(date_invoice_2.ToString());
                                    payment["new_name"] = invoice_2["new_name"] + ", " + date_invoice_2.Value.Day + "/" + date_invoice_2.Value.Month + "/" + date_invoice_2.Value.Year;
                                }
                            }
                            else if(invoice_2.Contains("new_name"))
                            {
                                payment["new_name"] = invoice_2["new_name"];
                            }
                        }
                        
                        service.Update(payment);
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
