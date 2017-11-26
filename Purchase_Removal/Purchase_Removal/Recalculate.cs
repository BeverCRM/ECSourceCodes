using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purchase_Removal
{
    public class Recalculate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
            {
                EntityReference EntityRef = (EntityReference)context.InputParameters["Target"];

                if (EntityRef.LogicalName != "new_payment")
                    return;
                try
                {
                    Entity deletedPayment = service.Retrieve(EntityRef.LogicalName, EntityRef.Id, new ColumnSet("new_invoice_sale", "new_amount"));

                    if (deletedPayment.Contains("new_invoice_sale") && deletedPayment["new_invoice_sale"] != null && deletedPayment.Contains("new_amount") && deletedPayment["new_amount"] != null)
                    {
                        Money sum = (Money)deletedPayment["new_amount"];
                        EntityReference purchaseInvoiceRef = (EntityReference)deletedPayment["new_invoice_sale"];
                        Entity purchaseInvoiceEntity = service.Retrieve(purchaseInvoiceRef.LogicalName, purchaseInvoiceRef.Id, new ColumnSet("new_pay_actually", "new_rest_to_pay", "new_stage_pay"));
                        if (purchaseInvoiceEntity.Contains("new_pay_actually") && purchaseInvoiceEntity["new_pay_actually"] != null)
                        {
                            Money payActually = (Money)purchaseInvoiceEntity["new_pay_actually"];
                            Money restToPay = (Money)purchaseInvoiceEntity["new_rest_to_pay"];
                            Double newRestToPay = Convert.ToDouble(restToPay.Value + sum.Value);
                            Double newPayActually = Convert.ToDouble(payActually.Value - sum.Value);
                            purchaseInvoiceEntity["new_pay_actually"] = new Money(Convert.ToDecimal(newPayActually));
                            purchaseInvoiceEntity["new_rest_to_pay"] = new Money(Convert.ToDecimal(newRestToPay));
                            if (newRestToPay > 0)
                            {
                                purchaseInvoiceEntity["new_stage_pay"] = false;
                            }
                            else
                            {
                                purchaseInvoiceEntity["new_stage_pay"] = true;
                            }
                            service.Update(purchaseInvoiceEntity);
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
