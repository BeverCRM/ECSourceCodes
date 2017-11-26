using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alik_Payment_Exp_To_Payment
{
    public class onUpdate_Payment_Exp : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity payment_exp_entity = (Entity)context.InputParameters["Target"];


                if (payment_exp_entity.LogicalName != "new_payment_out")
                    return;


                try
                {

                    QueryExpression _Query_0 = new QueryExpression
                    {
                        EntityName = "new_suply_pay",
                        ColumnSet = new ColumnSet("new_date", "new_recipient", "transactioncurrencyid", "new_n_invoice_purchase"),
                        Criteria =
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                 new ConditionExpression
                                 {
                                    AttributeName ="new_n_pay_doc",
                                    Operator=ConditionOperator.Equal,
                                    Values={ payment_exp_entity.Id.ToString()}
                                 }


                                 }
                            }
                    };

                    DateTime? date_in_ship_store = null;
                    EntityReference suplier_entity;
                    EntityReference currency;

                    EntityCollection _Entities = service.RetrieveMultiple(_Query_0);

                    foreach (Entity payment in _Entities.Entities)
                    {

                        if (payment_exp_entity.Contains("new_date"))
                        {
                            date_in_ship_store = (DateTime)payment_exp_entity["new_date"];
                            payment["new_date"] = date_in_ship_store;
                        }

                        if (payment_exp_entity.Contains("new_recipient"))
                        {
                            suplier_entity = (EntityReference)payment_exp_entity["new_recipient"];
                            payment["new_supplier"] = new EntityReference(suplier_entity.LogicalName, suplier_entity.Id);
                        }

                        if (payment_exp_entity.Contains("transactioncurrencyid"))
                        {
                            currency = (EntityReference)payment_exp_entity["transactioncurrencyid"];
                            payment["transactioncurrencyid"] = new EntityReference(currency.LogicalName, currency.Id);
                        }

                        EntityReference invoice;
                        if (payment_exp_entity.Contains("new_n_invoice_purchase"))
                        {
                            invoice = (EntityReference)payment_exp_entity["new_n_invoice_purchase"];
                            payment["new_n_invoice"] = new EntityReference(invoice.LogicalName, invoice.Id);
                        }


                        if (payment_exp_entity.Contains("new_sum"))
                       {
                           double sum = Convert.ToDouble(((Money)payment_exp_entity["new_sum"]).Value);
                          payment["new_sum_pay"] = new Money(Convert.ToDecimal(sum));
                       }


                        date_in_ship_store = date_in_ship_store.Value.AddHours(6);
                        if (payment_exp_entity.Contains("new_name"))
                            payment["new_name"] = payment_exp_entity["new_name"] + ", " + date_in_ship_store.Value.Day + "/" + date_in_ship_store.Value.Month + "/" + date_in_ship_store.Value.Year;
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
