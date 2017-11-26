using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Profit_Calculator
{
    public class profitCalculator : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity newSaleProduct = (Entity)context.InputParameters["Target"];

                if (newSaleProduct.LogicalName != "new_sale_product")
                    return;

                try
                {
                    if (newSaleProduct.Contains("new_invoice_n"))
                    {
                        Double totalProfit = 0;

                        QueryExpression saleProductQuery = new QueryExpression
                        {
                            EntityName = "new_sale_product",
                            ColumnSet = new ColumnSet("new_new_profitability"),
                            Criteria =
                                {
                                    FilterOperator = LogicalOperator.And,
                                    Conditions =
                                    {
                                        new ConditionExpression("new_invoice_n", ConditionOperator.Equal, ((EntityReference)newSaleProduct["new_invoice_n"]).Id)
                                    }
                                }
                        };

                        EntityCollection saleProductCollection = service.RetrieveMultiple(saleProductQuery);
                        if (saleProductCollection.Entities.Count() > 0)
                        {
                            foreach (Entity saleProduct in saleProductCollection.Entities)
                            {
                                totalProfit += saleProduct.GetAttributeValue<Double>("new_new_profitability");
                            }
                        }
                        EntityReference invoiceSaleRef = (EntityReference)newSaleProduct["new_invoice_n"];
                        Entity invoiceSale = service.Retrieve(invoiceSaleRef.LogicalName, invoiceSaleRef.Id, new ColumnSet("new_profit"));
                        invoiceSale["new_profit"] = new Money(Convert.ToDecimal(totalProfit));
                        service.Update(invoiceSale);
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
