using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alik_Payment_Exp_To_Payment
{
    public class onCreate_Payment_Exp : IPlugin
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
                Entity payment_exp_entity = (Entity)context.InputParameters["Target"];

                if (payment_exp_entity.LogicalName != "new_payment_out")
                    return;

                Entity payment = new Entity("new_suply_pay");

                invoice_id = payment_exp_entity.Id;
                invoice_name = payment_exp_entity.LogicalName;

                payment["new_n_pay_doc"] = new EntityReference(invoice_name, (Guid)invoice_id);


                DateTime? date_in_ship_store = null;
                if (payment_exp_entity.Contains("new_date"))
                {
                    date_in_ship_store = (DateTime)payment_exp_entity["new_date"];
                    payment["new_date"] = date_in_ship_store;
                }

                EntityReference currency;

                if (payment_exp_entity.Contains("transactioncurrencyid"))
                {
                    currency = (EntityReference)payment_exp_entity["transactioncurrencyid"];
                    payment["transactioncurrencyid"] = new EntityReference(currency.LogicalName, currency.Id);
                }


                if (payment_exp_entity.Contains("new_sum"))
                {
                    Decimal sum = Convert.ToDecimal(((Money)payment_exp_entity["new_sum"]).Value);
                    payment["new_sum_pay"] = new Money(sum);
                }
                

                EntityReference suplier; 
                if (payment_exp_entity.Contains("new_recipient"))
                {
                    suplier = (EntityReference)payment_exp_entity["new_recipient"];
                    payment["new_supplier"] = new EntityReference(suplier.LogicalName,suplier.Id);
                }

                EntityReference invoice;
                if (payment_exp_entity.Contains("new_n_invoice_purchase"))
                {
                    invoice = (EntityReference)payment_exp_entity["new_n_invoice_purchase"];
                    payment["new_n_invoice"] = new EntityReference(invoice.LogicalName, invoice.Id);
                }
                date_in_ship_store = date_in_ship_store.Value.AddHours(6);
                if(payment_exp_entity.Contains("new_name"))
                payment["new_name"] = payment_exp_entity["new_name"] + ", " + date_in_ship_store.Value.Day + "/" +  date_in_ship_store.Value.Month + "/" + date_in_ship_store.Value.Year;

                service.Create(payment);

            }
        }
    }
}
