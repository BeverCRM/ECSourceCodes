﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alik_Warehouse_Management_Purchase_EC
{
    public class onCreate_rest_of_story_after_move : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity move_entity = (Entity)context.InputParameters["Target"];

                if (move_entity.LogicalName != "new_move")
                    return;
                try
                {
                    if (move_entity.Contains("new_purchase_prod"))
                    {
                        Guid id_product_purchase = ((EntityReference)move_entity["new_purchase_prod"]).Id;
                        string name_product_purchase = ((EntityReference)move_entity["new_purchase_prod"]).LogicalName;

                        if (move_entity.Contains("new_warehouse_from") && move_entity.Contains("new_warehouse_to"))
                        {

                            Guid id_warwhouse = ((EntityReference)move_entity["new_warehouse_from"]).Id;
                            string name_warwhouse = ((EntityReference)move_entity["new_warehouse_from"]).LogicalName;




                            QueryExpression _Query_0 = new QueryExpression
                            {
                                EntityName = "new_rest_store",
                                ColumnSet = new ColumnSet("new_warehouse", "new_purchase_prod", "new_qnt"),
                                Criteria =
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                 new ConditionExpression
                                 {
                                    AttributeName ="new_warehouse",
                                    Operator=ConditionOperator.Equal,
                                    Values={ id_warwhouse }
                                 },
                                  new ConditionExpression
                                  {
                                    AttributeName ="new_purchase_prod",
                                    Operator=ConditionOperator.Equal,
                                    Values={ id_product_purchase }
                                  }


                                 }
                            }

                            };

                            EntityCollection _Entities = service.RetrieveMultiple(_Query_0);
                            if (_Entities.Entities.Count > 0)
                            {
                                foreach (Entity rest_of_story in _Entities.Entities)
                                {
                                    decimal quantity = Convert.ToDecimal(rest_of_story["new_qnt"]);
                                    if (Convert.ToDecimal(move_entity["new_qnt"]) > quantity)
                                    {
                                        throw new EmployeeListNotFoundException();
                                    }
                                    quantity -= Convert.ToDecimal(move_entity["new_qnt"]);
                                    rest_of_story["new_qnt"] = quantity;
                                    service.Update(rest_of_story);

                                }
                            }
                            else
                            {

                                Entity rest_store_from = new Entity("new_rest_store");

                                if (move_entity.Contains("new_article"))
                                {
                                    rest_store_from["new_article"] = move_entity["new_article"].ToString();
                                }

                                if (move_entity.Contains("new_qnt"))
                                {
                                    rest_store_from["new_qnt"] = Convert.ToDecimal(move_entity["new_qnt"]);
                                }


                                if (move_entity.Contains("new_prod") && move_entity["new_prod"] != null)
                                {
                                    rest_store_from["new_prod"] = new EntityReference(((EntityReference)move_entity["new_prod"]).LogicalName, ((EntityReference)move_entity["new_prod"]).Id);
                                }
                                if (move_entity.Contains("new_warehouse_from"))
                                    rest_store_from["new_warehouse"] = new EntityReference(((EntityReference)move_entity["new_warehouse_from"]).LogicalName, ((EntityReference)move_entity["new_warehouse_from"]).Id);


                                if (move_entity.Contains("new_purchase_prod"))
                                {
                                    rest_store_from["new_purchase_prod"] = new EntityReference(((EntityReference)move_entity["new_purchase_prod"]).LogicalName, ((EntityReference)move_entity["new_purchase_prod"]).Id);
                                    service.Create(rest_store_from);
                                }
                            }


                            Guid id_warwhouse_to = ((EntityReference)move_entity["new_warehouse_to"]).Id;
                            string name_warwhouse_to = ((EntityReference)move_entity["new_warehouse_to"]).LogicalName;


                            QueryExpression _Query_1 = new QueryExpression
                            {
                                EntityName = "new_rest_store",
                                ColumnSet = new ColumnSet("new_warehouse", "new_purchase_prod", "new_qnt"),
                                Criteria =
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                 new ConditionExpression
                                 {
                                    AttributeName ="new_warehouse",
                                    Operator=ConditionOperator.Equal,
                                    Values={ id_warwhouse_to }
                                 },
                                  new ConditionExpression
                                  {
                                    AttributeName ="new_purchase_prod",
                                    Operator=ConditionOperator.Equal,
                                    Values={ id_product_purchase }
                                  }
                                 }
                            }

                            };


                            EntityCollection _Entities_to = service.RetrieveMultiple(_Query_1);
                            if (_Entities_to.Entities.Count > 0)
                            {
                                foreach (Entity rest_of_story in _Entities_to.Entities)
                                {
                                    decimal quantity = Convert.ToDecimal(rest_of_story["new_qnt"]);
                                    if (Convert.ToDecimal(move_entity["new_qnt"]) > quantity)
                                    {
                                        throw new EmployeeListNotFoundException();
                                    }
                                    quantity += Convert.ToDecimal(move_entity["new_qnt"]);
                                    rest_of_story["new_qnt"] = quantity;
                                    service.Update(rest_of_story);

                                }
                            }
                            else
                            {

                                Entity rest_store_from = new Entity("new_rest_store");

                                if (move_entity.Contains("new_article"))
                                {
                                    rest_store_from["new_article"] = move_entity["new_article"].ToString();
                                }

                                if (move_entity.Contains("new_qnt"))
                                {
                                    rest_store_from["new_qnt"] = Convert.ToDecimal(move_entity["new_qnt"]);
                                }


                                if (move_entity.Contains("new_prod") && move_entity["new_prod"] != null)
                                {
                                    rest_store_from["new_prod"] = new EntityReference(((EntityReference)move_entity["new_prod"]).LogicalName, ((EntityReference)move_entity["new_prod"]).Id);
                                }
                                if (move_entity.Contains("new_warehouse_to"))
                                    rest_store_from["new_warehouse"] = new EntityReference(((EntityReference)move_entity["new_warehouse_to"]).LogicalName, ((EntityReference)move_entity["new_warehouse_to"]).Id);


                                if (move_entity.Contains("new_purchase_prod"))
                                {
                                    rest_store_from["new_purchase_prod"] = new EntityReference(((EntityReference)move_entity["new_purchase_prod"]).LogicalName, ((EntityReference)move_entity["new_purchase_prod"]).Id);

                                    service.Create(rest_store_from);
                                }
                            }
                        }
                        else
                        {

                            if (move_entity.Contains("new_warehouse_to"))
                            {

                                Guid id_warwhouse_to = ((EntityReference)move_entity["new_warehouse_to"]).Id;
                                string name_warwhouse_to = ((EntityReference)move_entity["new_warehouse_to"]).LogicalName;

                                QueryExpression _Query_1 = new QueryExpression
                                {
                                    EntityName = "new_rest_store",
                                    ColumnSet = new ColumnSet("new_warehouse", "new_purchase_prod", "new_qnt"),
                                    Criteria =
                                    {
                                        FilterOperator = LogicalOperator.And,
                                        Conditions =
                                        {
                                         new ConditionExpression
                                         {
                                            AttributeName ="new_warehouse",
                                            Operator=ConditionOperator.Equal,
                                            Values={ id_warwhouse_to }
                                         },
                                          new ConditionExpression
                                          {
                                            AttributeName ="new_purchase_prod",
                                            Operator=ConditionOperator.Equal,
                                            Values={ id_product_purchase }
                                          }
                                         }
                                    }

                                };

                                EntityCollection _Entities_to = service.RetrieveMultiple(_Query_1);
                                if (_Entities_to.Entities.Count > 0)
                                {
                                    foreach (Entity rest_of_story in _Entities_to.Entities)
                                    {
                                        decimal quantity = Convert.ToDecimal(rest_of_story["new_qnt"]);
                                        if (Convert.ToDecimal(move_entity["new_qnt"]) > quantity)
                                        {
                                            throw new EmployeeListNotFoundException();
                                        }
                                        quantity += Convert.ToDecimal(move_entity["new_qnt"]);
                                        rest_of_story["new_qnt"] = quantity;
                                        service.Update(rest_of_story);

                                    }
                                }
                                else
                                {
                                    Entity rest_store = new Entity("new_rest_store");

                                    if (move_entity.Contains("new_article"))
                                    {
                                        rest_store["new_article"] = move_entity["new_article"].ToString();
                                    }

                                    if (move_entity.Contains("new_qnt"))
                                    {
                                        rest_store["new_qnt"] = Convert.ToDecimal(move_entity["new_qnt"]);
                                    }


                                    if (move_entity.Contains("new_prod") && move_entity["new_prod"] != null)
                                    {
                                        rest_store["new_prod"] = new EntityReference(((EntityReference)move_entity["new_prod"]).LogicalName, ((EntityReference)move_entity["new_prod"]).Id);
                                    }
                                    if (move_entity.Contains("new_warehouse_to"))
                                        rest_store["new_warehouse"] = new EntityReference(((EntityReference)move_entity["new_warehouse_to"]).LogicalName, ((EntityReference)move_entity["new_warehouse_to"]).Id);


                                    if (move_entity.Contains("new_purchase_prod"))
                                    {
                                        rest_store["new_purchase_prod"] = new EntityReference(((EntityReference)move_entity["new_purchase_prod"]).LogicalName, ((EntityReference)move_entity["new_purchase_prod"]).Id);
                                        service.Create(rest_store);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(
                        @"<img height='16px' src='http://emojione.com/wp-content/uploads/assets/emojis/1f644.svg'> <h2><strong>Oh snap!</strong><h2>
                        It seems the record can not be saved in its current state. Not enough quantity of product \n");
                }
            }
        }
    }
}
