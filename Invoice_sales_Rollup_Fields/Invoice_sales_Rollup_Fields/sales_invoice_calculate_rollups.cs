using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice_sales_Rollup_Fields
{
    public class sales_invoice_calculate_rollups : CodeActivity
    {

        double amouny_without_vat = 0;
        double vat = 0;
        double total = 0;

        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            // Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();

            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();

            // Create the Organiztion service
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            IExecutionContext crmContext = executionContext.GetExtension<IExecutionContext>();

            EntityReference invoice_reference = (EntityReference)crmContext.InputParameters["Target"];

            Entity invoice_sales_entity = service.Retrieve(invoice_reference.LogicalName, invoice_reference.Id, new ColumnSet(null));

            //tracingService.Trace("1");
            try
            {
                QueryExpression _Query_0 = new QueryExpression
                {
                    EntityName = "new_sale_product",
                    ColumnSet = new ColumnSet("new_sum_without_vat", "new_vat_sum", "new_sum_with_vat"),
                    Criteria =
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                     new ConditionExpression
                                     {
                                        AttributeName ="new_invoice_n",
                                        Operator=ConditionOperator.Equal,
                                        Values={ invoice_sales_entity.Id}
                                     }
                                 }
                            }
                };

               // tracingService.Trace("2");

                EntityCollection _Entities = service.RetrieveMultiple(_Query_0);

                foreach (Entity invoiceLines in _Entities.Entities)
                {
                    if (invoiceLines.Contains("new_sum_without_vat") && invoiceLines["new_sum_without_vat"] != null)
                        amouny_without_vat += Convert.ToDouble(((Money)(invoiceLines["new_sum_without_vat"])).Value);

                    if (invoiceLines.Contains("new_vat_sum") && invoiceLines["new_vat_sum"] != null)
                        vat += Convert.ToDouble(((Money)(invoiceLines["new_vat_sum"])).Value);


                    if (invoiceLines.Contains("new_sum_with_vat") && invoiceLines["new_sum_with_vat"] != null)
                        total += Convert.ToDouble(((Money)(invoiceLines["new_sum_with_vat"])).Value);
                }

               // tracingService.Trace("3");
                invoice_sales_entity["new_total_cost"] = new Money(Convert.ToDecimal(amouny_without_vat));
                invoice_sales_entity["new_sum_vat"] = new Money(Convert.ToDecimal(vat));
                invoice_sales_entity["new_total_sum"] = new Money(Convert.ToDecimal(total));
               // tracingService.Trace("4");
                service.Update(invoice_sales_entity);
                //tracingService.Trace("5");
            }
            catch(Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in the plug-in. " + ex);
            }

        }
    }
}
