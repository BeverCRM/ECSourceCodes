using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alik_Invoice_To_Payment_Entity
{
    public class onDelete_Payment_Entity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
            {
                EntityReference invoice_entity = (EntityReference)context.InputParameters["Target"];

                if (invoice_entity.LogicalName != "new_purchase")
                    return;
                try
                {

                    QueryExpression _Query_0 = new QueryExpression
                    {
                        EntityName = "new_suply_pay",
                        ColumnSet = new ColumnSet("new_date", "new_supplier", "transactioncurrencyid", "new_sum_inv"),
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

                    EntityCollection _Entities_0 = service.RetrieveMultiple(_Query_0);

                    if (_Entities_0.Entities.Count > 0)
                    {
                        service.Delete(_Entities_0.Entities[0].LogicalName, _Entities_0.Entities[0].Id);
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
