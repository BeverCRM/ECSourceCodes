using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment_Removal
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

                if (EntityRef.LogicalName != "new_payment_out")
                    return;

                try
                {
                    Entity deletedPayment = service.Retrieve(EntityRef.LogicalName, EntityRef.Id, new ColumnSet("new_n_invoice_purchase", "new_sum", "new_pay_details"));

                    if(deletedPayment.Contains("new_n_invoice_purchase") && deletedPayment["new_n_invoice_purchase"] != null && deletedPayment.Contains("new_sum") && deletedPayment["new_sum"] != null)
                    {
                        Money sum = (Money)deletedPayment["new_sum"];
                        EntityReference purchaseInvoiceRef = (EntityReference)deletedPayment["new_n_invoice_purchase"];
                        OptionSetValue payDetails = (OptionSetValue)deletedPayment["new_pay_details"];
                        Entity purchaseInvoiceEntity = service.Retrieve(purchaseInvoiceRef.LogicalName, purchaseInvoiceRef.Id, new ColumnSet("new_pay_actually", "new_rest_sum", "new_pay", "new_pay_actually_tax", "new_rest_vat", "new_tax_pay"));
                        if (payDetails.Value == 100000000)
                        {
                            if (purchaseInvoiceEntity.Contains("new_pay_actually") && purchaseInvoiceEntity["new_pay_actually"] != null)
                            {
                                Money payActually = (Money)purchaseInvoiceEntity["new_pay_actually"];
                                Money restOfSum = (Money)purchaseInvoiceEntity["new_rest_sum"];
                                Double newRestOfSum = Convert.ToDouble(restOfSum.Value + sum.Value);
                                Double newPayActually = Convert.ToDouble(payActually.Value - sum.Value);
                                purchaseInvoiceEntity["new_pay_actually"] = new Money(Convert.ToDecimal(newPayActually));
                                purchaseInvoiceEntity["new_rest_sum"] = new Money(Convert.ToDecimal(newRestOfSum));
                                if (newRestOfSum > 0)
                                {
                                    purchaseInvoiceEntity["new_pay"] = false;
                                }
                                else
                                {
                                    purchaseInvoiceEntity["new_pay"] = true;
                                }
                                service.Update(purchaseInvoiceEntity);
                            }
                        }
                        else
                        {
                            if (payDetails.Value == 100000002)
                            {
                                if (purchaseInvoiceEntity.Contains("new_pay_actually_tax") && purchaseInvoiceEntity["new_pay_actually_tax"] != null)
                                {
                                    Double payActuallyTax = (Double)purchaseInvoiceEntity["new_pay_actually_tax"];
                                    Double restOfSum = (Double)purchaseInvoiceEntity["new_rest_vat"];
                                    Double newRestOfSum = Convert.ToDouble(sum.Value) + restOfSum;
                                    Double newPayActuallyTax = payActuallyTax - Convert.ToDouble(sum.Value);
                                    purchaseInvoiceEntity["new_pay_actually_tax"] = newPayActuallyTax;
                                    purchaseInvoiceEntity["new_rest_vat"] = newRestOfSum;
                                    if (newRestOfSum > 0)
                                    {
                                        purchaseInvoiceEntity["new_tax_pay"] = false;
                                    }
                                    else
                                    {
                                        purchaseInvoiceEntity["new_tax_pay"] = true;
                                    }
                                    service.Update(purchaseInvoiceEntity);
                                }
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
