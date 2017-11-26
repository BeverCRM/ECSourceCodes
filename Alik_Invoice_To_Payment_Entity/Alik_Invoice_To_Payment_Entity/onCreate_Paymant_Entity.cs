using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alik_Invoice_To_Payment_Entity
{
    public class onCreate_Paymant_Entity : IPlugin
    {
        Guid? invoice_id = null;
        String invoice_name = "";
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

                Entity payment = new Entity("new_suply_pay");

                invoice_id = invoice_entity.Id;
                invoice_name = invoice_entity.LogicalName;

                payment["new_n_invoice"] = new EntityReference(invoice_name,(Guid)invoice_id);


                DateTime? date_in_ship_store  = null;
                if (invoice_entity.Contains("new_ship_store"))
                {
                    date_in_ship_store = (DateTime)invoice_entity["new_ship_store"];
                    date_in_ship_store.Value.AddHours(6);
                    payment["new_date"] = date_in_ship_store;
                }
                EntityReference suplier_entity;
                if (invoice_entity.Contains("new_supplier"))
                {
                    suplier_entity = (EntityReference)invoice_entity["new_supplier"];
                    payment["new_supplier"] = new EntityReference(suplier_entity.LogicalName,suplier_entity.Id);
                }
                EntityReference currency;

                if (invoice_entity.Contains("transactioncurrencyid"))
                {
                    currency = (EntityReference)invoice_entity["transactioncurrencyid"];
                    payment["transactioncurrencyid"] = new EntityReference(currency.LogicalName,currency.Id);
                }
                if (invoice_entity.Contains("new_sum"))
                {
                    Decimal sum = Convert.ToDecimal(((Money)(invoice_entity["new_sum"])).Value);
                    payment["new_sum_inv"] = new Money(sum);
                }

                if (invoice_entity.Contains("new_name") && date_in_ship_store != null)
                {
                    date_in_ship_store = date_in_ship_store.Value.AddHours(6);
                    payment["new_name"] = invoice_entity["new_name"] + ", " + date_in_ship_store.Value.Day + "/" + date_in_ship_store.Value.Month + "/" + date_in_ship_store.Value.Year;
                }
                else
                {
                    payment["new_name"] = invoice_entity["new_name"];
                }
                service.Create(payment);

            }
        }
    }
}
