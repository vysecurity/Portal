using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xaml;
using System.Xml;
using System.Xml.Linq;
using ClassLibrary;
using Azure;

namespace PortalService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PortalService : IPortalService
    {

        private Exception theException = null;

        private List<IExpression> BPFGeneralExpressionList = new List<IExpression>();

        public String GetDataTrailer()
        {
            String _connectionString = String.Format("Server={0};Database={1};User ID={2};Password={3};Encrypt={4};",
                                             "sql-eus.database.windows.net",
                                             "xRMLink",
                                             "genbil",
                                             "GeN81190BiL",
                                             "True");

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;            // <== lacking
                    command.CommandType = CommandType.Text;
                    command.CommandText = "insert into [dbo].[p4crm_portalroleBase](p4crm_portalroleId,p4crm_portalid,p4crm_entitylogicalname,OwnerId,statecode)values('B482C7C9-5F6F-E511-80FA-3863BB341A14','94691716-9A51-E511-80EA-FC15B428D6EC','yaheyya','6AC605AB-D45E-E511-80EF-3863BB340A80',0)";


                    try
                    {
                        connection.Open();
                        int recordsAffected = command.ExecuteNonQuery();
                    }
                    catch (SqlException Ex)
                    {
                        throw new InvalidPluginExecutionException(Ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            return "yaheyya";
        }

        public String GetExternalWidgetHTML(String URL)
        {
            String s = String.Empty;

            using (WebClient client = new WebClient())
            {
                s = client.DownloadString(URL);
            }

            return s;
        }

        public CrmServiceInformation GetPortalIdAndCreateSourceCrmService(String DomainURL)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();
            CrmServiceInformation returnserviceinfo = Azure.GetCrmConnectionInformation(DomainURL);
            return returnserviceinfo;
        }

        public CrmServiceInformation GetConfigurationInfo(String ConfigurationId)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();
            CrmServiceInformation returnserviceinfo = Azure.GetCrmConnectionInformationWithConfigurationId(ConfigurationId);
            return returnserviceinfo;
        }

        public String GetDynamicScriptForEntity(String Id)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();
            String ReturnScripts = Azure.GetDynamicScriptForEntity(Id);
            return ReturnScripts;
        }

        public LoginInformation GetLoginInfo(CrmServiceInformation serviceinfo)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();
            LoginInformation returnlogin = Azure.GetLoginInfo(serviceinfo);
            return returnlogin;
        }

        public List<Object> AttemptLogin(CrmServiceInformation serviceinfo, String PortalId, LoginInformation LoginInformation, Login LoginValues)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);
            List<Object> Object = new List<Object>();

            QueryExpression expression = new QueryExpression();
            expression.EntityName = LoginInformation.EntityName;
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition(new ConditionExpression(LoginInformation.UserNameField, ConditionOperator.Equal, LoginValues.UserNameValue));
            expression.Criteria.AddCondition(new ConditionExpression(LoginInformation.PasswordField, ConditionOperator.Equal, LoginValues.PasswordValue));

            EntityCollection collect = service.RetrieveMultiple(expression);

            if (collect.Entities.Count > 0)
            {
                Object.Add(true);
                Object.Add(collect.Entities[0].Id);

            }
            else
            {
                Object.Add(false);
                Object.Add(Guid.Empty);
            }
            return Object;


        }

        public Byte[] GetPicture(CrmServiceInformation serviceinfo, String PortalId, String EntityLogicalName, String PrimaryField, String LoginUser)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);


            Byte[] returnbyte = new Byte[] { };

            try
            {

                String query = String.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                     <entity name='{0}'>
                                                     <attribute name='entityimage' />
                                                     <filter type='and'>
                                                    <condition attribute='{1}' operator='eq'  value='{2}' />
                                            </filter>
                                          </entity>
                                        </fetch>", EntityLogicalName, PrimaryField, LoginUser);

                EntityCollection collect = service.RetrieveMultiple(new FetchExpression(query));

                if (collect.Entities.Count > 0)
                {
                    if (collect.Entities[0].Attributes.Contains("entityimage"))
                        returnbyte = collect.Entities[0]["entityimage"] as Byte[];
                }
            }
            catch
            {
                return returnbyte;
            }

            return returnbyte;
        }

        public Page GetNavigationPage(CrmServiceInformation serviceinfo, String PortalId, String NavigationId, String PortalEntity)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();
            Page p = Azure.GetNavigationPage(NavigationId);
            return p;
        }

        public Page GetPage(String PageId)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();
            Page p = Azure.GetPage(PageId);
            return p;
        }

        public List<WidgetData> GetWidgetData(CrmServiceInformation serviceinfo, String PortalId, List<WidgetParameters> WidgetParameters, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField, String UseCache)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();

            List<WidgetProperties> WidgetProperties = Azure.GetPageWidgetEntities(WidgetParameters);

            IOrganizationService dataservice = CreateDestinationCrmService(PortalId, serviceinfo, UseCache, false);

            List<WidgetData> returndata = new List<WidgetData>();

            #region Operations



            Thread[] workerThreads = new Thread[WidgetProperties.Count];

            int threadcounters = 0;
            foreach (var PWidget in WidgetProperties)
            {
                #region Iterate

                string threadName = "WorkerThread " + threadcounters.ToString();

                HttpContext httpcontext = HttpContext.Current;
                workerThreads[threadcounters] = new Thread(new ThreadStart(() =>
                {
                    HttpContext.Current = httpcontext;
                    GetWidgetDataWithThreads(PWidget, WidgetParameters, dataservice, serviceinfo, PortalId, PortalEntity, PortalEntityUserField, PortalEntityUserValue, ref returndata);
                }));

                workerThreads[threadcounters].Name = threadName;
                workerThreads[threadcounters].Start();

                threadcounters++;
                #endregion
            }

            for (int threadIndex = 0; threadIndex < workerThreads.Length; threadIndex++)
            {
                workerThreads[threadIndex].Join();
            }
            if (theException != null)
                throw new Exception(theException.Message);

            #endregion


            return returndata;
        }

        public String GetHtmlWidgetContent(CrmServiceInformation serviceinfo, String PortalId, String WidgetGuid, String EntityId, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();

            IOrganizationService dataservice = CreateDestinationCrmService(PortalId, serviceinfo);
            WidgetProperties Widget = Azure.GetWidgetHTMLContent(WidgetGuid);

            String html = Widget.HTMLSource;
            String EntityName = Widget.EntityLogicalName;
            ConvertHTML(dataservice, ref html, "@@session", "session", EntityName, EntityId, PortalEntity, PortalEntityUserField, PortalEntityUserValue);
            ConvertHTML(dataservice, ref html, "@@entity", "entity", EntityName, EntityId, PortalEntity, PortalEntityUserField, PortalEntityUserValue);
            return html;

        }

        public FormLayout GetEditFormLayout(CrmServiceInformation serviceinfo, String PortalId, String FormId, String EntityId, List<SubGridModel> SubGridModel = null)
        {
            IOrganizationService service = null;
            SqlAzureConnection Azure = new SqlAzureConnection();

            var result = Azure.CheckPortalIsMultiLanguage(PortalId);
            if (result == true)
            {
                service = CreateDestinationCrmService(PortalId, serviceinfo, false.ToString(), true);
            }
            else
                service = CreateDestinationCrmService(PortalId, serviceinfo, "false", false);

            Entity SystemForm = service.Retrieve("systemform", new Guid(FormId), new ColumnSet("formxml", "name", "objecttypecode"));

            List<Tabs> tabs = ParseFormXml(service, SystemForm["formxml"].ToString(), SystemForm["objecttypecode"].ToString(), EntityId, PortalId, serviceinfo, SubGridModel);

            FormLayout layout = new FormLayout();
            layout.EntityId = EntityId;
            layout.Tabs = tabs;
            layout.FormName = SystemForm["name"].ToString();

            return layout;
        }

        public List<SubGridData> GetSubGridData(CrmServiceInformation serviceinfo, String PortalId, String ViewId)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);
            List<SubGridData> returndata = new List<SubGridData>();

            QueryExpression savedquery = new QueryExpression();
            savedquery.EntityName = "savedquery";
            savedquery.ColumnSet = new ColumnSet(true);
            savedquery.Criteria.AddCondition(new ConditionExpression("savedqueryid", ConditionOperator.Equal, new Guid(ViewId)));

            EntityCollection savedquerycollect = service.RetrieveMultiple(savedquery);

            if (savedquerycollect.Entities.Count > 0)
            {
                String EntityName = Convert.ToString(savedquerycollect.Entities[0]["returnedtypecode"]);
                EntityMetadata EntityMetadata = GetAndCheckMetaDataFromCache(service, serviceinfo.ConfigurationId, EntityName);

                AttributeMetadata[] metadata = EntityMetadata.Attributes;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Convert.ToString(savedquerycollect.Entities[0]["layoutxml"]));
                XmlNodeList nodelist = doc.SelectNodes("//cell");

                int counter = 0;
                foreach (var items in nodelist)
                {
                    String columns = ((System.Xml.XmlElement)(items)).Attributes["name"].Value;
                    AttributeMetadata meta = metadata.FirstOrDefault(x => x.LogicalName.Equals(columns));

                    SubGridData subgriddata = new SubGridData();
                    subgriddata.ColumnName = columns;
                    if (meta != null)
                    {
                        subgriddata.Counter = counter;
                        subgriddata.DisplayName = meta.DisplayName.UserLocalizedLabel.Label;
                        counter++;
                        returndata.Add(subgriddata);
                    }

                }
            }
            return returndata;
        }

        public List<GridRowData> GetRelatedSubGridRecords(CrmServiceInformation serviceinfo, String PortalId, String SubGridViewId, String RelationShipName, String ParentId)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);
            String FetchXml = String.Empty;
            List<GridRowData> returndata = new List<GridRowData>();

            Entity SavedQuery = service.Retrieve("savedquery", new Guid(SubGridViewId), new ColumnSet(true));
            FetchXml = Convert.ToString(SavedQuery["fetchxml"]);

            int p = 1;
            String EntityName = Convert.ToString(SavedQuery["returnedtypecode"]);

            EntityMetadata EntityMetadata = GetAndCheckMetaDataFromCache(service, serviceinfo.ConfigurationId, EntityName);

            FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = FetchXml

            };
            var response = (FetchXmlToQueryExpressionResponse)service.Execute(req);

            QueryExpression exp = response.Query;


            RetrieveRelationshipRequest retrieveOneToManyRequest = new RetrieveRelationshipRequest
            {
                Name = RelationShipName,
            };
            RetrieveRelationshipResponse relationshipResponse = (RetrieveRelationshipResponse)service.Execute(retrieveOneToManyRequest);
            var referencedattribute = ((Microsoft.Xrm.Sdk.Metadata.OneToManyRelationshipMetadata)(relationshipResponse.RelationshipMetadata)).ReferencingAttribute;
            var referenceentity = ((Microsoft.Xrm.Sdk.Metadata.OneToManyRelationshipMetadata)(relationshipResponse.RelationshipMetadata)).ReferencedEntity;

            exp.Criteria.AddCondition(new ConditionExpression(referencedattribute, ConditionOperator.Equal, new Guid(ParentId)));
            EntityCollection results = service.RetrieveMultiple(exp);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Convert.ToString(SavedQuery["layoutxml"]));
            XmlNodeList nodelist = doc.SelectNodes("//cell");

            foreach (var item in results.Entities)
            {
                PrepareGridData(serviceinfo, service, PortalId, item, nodelist, EntityMetadata.Attributes, FetchXml, p, ref returndata, EntityMetadata.PrimaryNameAttribute, String.Empty, String.Empty, String.Empty);
                p++;
            }
            return returndata;
        }

        public List<SubGridRecords> GetSubGridRecords(CrmServiceInformation serviceinfo, String PortalId, String EntityName, String EntityId, List<String> SubGridColumns)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);
            List<SubGridRecords> returnsubgrid = new List<SubGridRecords>();

            Entity Entity = service.Retrieve(EntityName, new Guid(EntityId), new ColumnSet(SubGridColumns.ToArray()));

            foreach (var item in SubGridColumns)
            {
                SubGridRecords s = new SubGridRecords();
                s.LogicalName = item;

                if (!Entity.Attributes.Contains(item))
                {
                    s.Value = String.Empty;
                }
                else
                {
                    if (Entity[item] is EntityReference)
                    {
                        s.Value = ((EntityReference)Entity[item]).Name.ToString();
                    }
                    else if (Entity[item] is OptionSetValue)
                    {
                        String V = GetoptionsetTextOnValue(service, PortalId, EntityName, item, ((OptionSetValue)Entity[item]).Value, serviceinfo.ConfigurationId);
                        s.Value = V;
                    }
                    else if (Entity[item] is Money)
                    {
                        s.Value = ((Money)Entity[item]).Value.ToString();
                    }
                    else
                    {
                        s.Value = Entity[item].ToString();
                    }
                }
                returnsubgrid.Add(s);
            }
            return returnsubgrid;
        }

        public CreatedData CreateData(CrmServiceInformation serviceinfo, String PortalId, String EntityName, List<FormData> FormData, String Signature, String OwnerShip, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField, String RelationShipName, String ParentId)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);
            SqlAzureConnection SqlAzure = new SqlAzureConnection();

            CreatedData data = new CreatedData();


            Entity createentity = new Entity(EntityName);
            foreach (var item in FormData)
            {
                if (item.type == AttributeTypeCode.String.ToString().ToLower())
                {
                    createentity[item.logicalname] = Convert.ToString(item.value);
                }
                else if (item.type == AttributeTypeCode.Lookup.ToString().ToLower())
                {
                    createentity[item.logicalname] = new EntityReference(item.entityname, new Guid(item.value));
                }
                else if (item.type == AttributeTypeCode.Customer.ToString().ToLower())
                {
                    createentity[item.logicalname] = new EntityReference(item.entityname, new Guid(item.value));
                }
                else if (item.type == AttributeTypeCode.Money.ToString().ToLower())
                {
                    Decimal newd = Decimal.MinValue;
                    Decimal.TryParse(item.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newd);
                    if (newd != Decimal.MinValue)
                        createentity[item.logicalname] = new Money(newd);
                }
                else if (item.type == AttributeTypeCode.Decimal.ToString().ToLower())
                {
                    Decimal newd = Decimal.MinValue;
                    Decimal.TryParse(item.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newd);
                    if (newd != Decimal.MinValue)
                        createentity[item.logicalname] = newd;
                }
                else if (item.type == AttributeTypeCode.Double.ToString().ToLower())
                {
                    Double newd = Double.MinValue;
                    Double.TryParse(item.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newd);
                    if (newd != Double.MinValue)
                        createentity[item.logicalname] = newd;
                }
                else if (item.type == AttributeTypeCode.DateTime.ToString().ToLower())
                {
                    DateTime dt1 = DateTime.MaxValue;
                    if (item.datetimepicker == "dateonly")
                    {
                        dt1 = DateTime.ParseExact(item.value, item.dateformat, CultureInfo.InvariantCulture);
                    }
                    else if (item.datetimepicker == "dateandtime")
                    {
                        dt1 = DateTime.ParseExact(item.value, item.dateformat + " " + item.timeformat, CultureInfo.InvariantCulture);
                    }

                    var x = dt1.ToString("s");
                    createentity[item.logicalname] = Convert.ToDateTime(x);

                }
                else if (item.type == AttributeTypeCode.Picklist.ToString().ToLower())
                {
                    createentity[item.logicalname] = new OptionSetValue(Convert.ToInt32(item.value));
                }
                else if (item.type == AttributeTypeCode.Memo.ToString().ToLower())
                {
                    createentity[item.logicalname] = Convert.ToString(item.value);
                }
                else if (item.type == AttributeTypeCode.Boolean.ToString().ToLower())
                {
                    createentity[item.logicalname] = Convert.ToBoolean(Convert.ToInt32(item.value) == 1 ? true : false);
                }
                else if (item.type == "ınteger" || item.type == "integer")
                {
                    createentity[item.logicalname] = Convert.ToInt32(item.value);
                }
                else if (item.type == "guid")
                {
                    createentity[item.logicalname] = new Guid(item.value);
                }
            }
            try
            {
                if (OwnerShip == "1")
                {
                    PortalMembers Portal = SqlAzure.GetPortalOwnerFields(PortalId);
                    if (Portal.CreatedBy == null || Portal.ModifiedBy == null)
                    {
                        throw new Exception("Portal Ownership definitions is missing!");
                    }

                    QueryByAttribute q = new QueryByAttribute();
                    q.EntityName = PortalEntity;
                    q.ColumnSet = new ColumnSet(true);
                    q.AddAttributeValue(PortalEntityUserField, PortalEntityUserValue);
                    EntityCollection collect = service.RetrieveMultiple(q);

                    Entity PortalUser = collect.Entities.FirstOrDefault();

                    createentity[Portal.CreatedBy] = new EntityReference(PortalUser.LogicalName, PortalUser.Id);
                    createentity[Portal.ModifiedBy] = new EntityReference(PortalUser.LogicalName, PortalUser.Id);
                }

                if (RelationShipName != String.Empty)
                {
                    RetrieveRelationshipRequest retrieveOneToManyRequest = new RetrieveRelationshipRequest
                    {
                        Name = RelationShipName,
                    };
                    RetrieveRelationshipResponse response = (RetrieveRelationshipResponse)service.Execute(retrieveOneToManyRequest);
                    var referencedattribute = ((Microsoft.Xrm.Sdk.Metadata.OneToManyRelationshipMetadata)(response.RelationshipMetadata)).ReferencingAttribute;
                    var referenceentity = ((Microsoft.Xrm.Sdk.Metadata.OneToManyRelationshipMetadata)(response.RelationshipMetadata)).ReferencedEntity;
                    createentity[referencedattribute] = new EntityReference(referenceentity, new Guid(ParentId));
                }
                Guid id = service.Create(createentity);

                data.Id = id.ToString();
                data.ErrorMessage = String.Empty;
                //attach note

                if (Signature != String.Empty)
                {
                    Entity annotation = new Entity("annotation");
                    annotation.Attributes.Add("subject", "Signature");

                    annotation.Attributes.Add("filename", EntityName + "-Signature.png");
                    annotation.Attributes.Add("documentbody", Signature);
                    annotation.Attributes.Add("mimetype", "image/png");
                    annotation.Attributes.Add("objectid", new EntityReference(EntityName, id));

                    Guid annotationId = service.Create(annotation);
                }
            }
            catch (Exception ex)
            {
                data.Id = String.Empty;
                data.ErrorMessage = ex.Message;
                return data;
            }

            return data;
        }

        public String UpdateData(CrmServiceInformation serviceinfo, String PortalId, String EntityName, List<FormData> FormData, String Id, String OwnerShip, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);
            SqlAzureConnection SqlAzure = new SqlAzureConnection();


            Entity updateEntity = new Entity(EntityName);
            foreach (var item in FormData)
            {
                if (item.type == AttributeTypeCode.String.ToString().ToLower())
                {
                    updateEntity[item.logicalname] = Convert.ToString(item.value);
                }
                else if (item.type == AttributeTypeCode.Lookup.ToString().ToLower())
                {
                    updateEntity[item.logicalname] = new EntityReference(item.entityname, new Guid(item.value));
                }
                else if (item.type == AttributeTypeCode.Customer.ToString().ToLower())
                {
                    updateEntity[item.logicalname] = new EntityReference(item.entityname, new Guid(item.value));
                }
                else if (item.type == AttributeTypeCode.Money.ToString().ToLower())
                {
                    Decimal newd = Decimal.MinValue;
                    Decimal.TryParse(item.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newd);
                    if (newd != Decimal.MinValue)
                        updateEntity[item.logicalname] = new Money(newd);
                }
                else if (item.type == AttributeTypeCode.Decimal.ToString().ToLower())
                {
                    Decimal newd = Decimal.MinValue;
                    Decimal.TryParse(item.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newd);
                    if (newd != Decimal.MinValue)
                        updateEntity[item.logicalname] = newd;
                }
                else if (item.type == AttributeTypeCode.Double.ToString().ToLower())
                {
                    Double newd = Double.MinValue;
                    Double.TryParse(item.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newd);
                    if (newd != Double.MinValue)
                        updateEntity[item.logicalname] = newd;
                }
                else if (item.type == AttributeTypeCode.DateTime.ToString().ToLower())
                {
                    DateTime dt1 = DateTime.MaxValue;
                    if (item.datetimepicker == "dateonly")
                    {
                        dt1 = DateTime.ParseExact(item.value, item.dateformat, CultureInfo.InvariantCulture);
                    }
                    else if (item.datetimepicker == "dateandtime")
                    {
                        dt1 = DateTime.ParseExact(item.value, item.dateformat + " " + item.timeformat, CultureInfo.InvariantCulture);
                    }

                    var x = dt1.ToString("s");
                    updateEntity[item.logicalname] = Convert.ToDateTime(x);
                }
                else if (item.type == AttributeTypeCode.Picklist.ToString().ToLower())
                {
                    if (item.value == "null")
                        updateEntity[item.logicalname] = null;
                    else if (item.value == null)
                        updateEntity[item.logicalname] = null;
                    else
                        updateEntity[item.logicalname] = new OptionSetValue(Convert.ToInt32(item.value));
                }
                else if (item.type == AttributeTypeCode.Memo.ToString().ToLower())
                {
                    updateEntity[item.logicalname] = Convert.ToString(item.value);
                }
                else if (item.type == AttributeTypeCode.Boolean.ToString().ToLower())
                {
                    updateEntity[item.logicalname] = Convert.ToBoolean(Convert.ToInt32(item.value) == 1 ? true : false);
                }
                else if (item.type == "ınteger" || item.type == "integer")
                {
                    updateEntity[item.logicalname] = Convert.ToInt32(item.value);
                }
                else if (item.type == "guid")
                {
                    updateEntity[item.logicalname] = new Guid(item.value);
                }




            }
            try
            {
                if (OwnerShip == "1")
                {
                    PortalMembers Portal = SqlAzure.GetPortalOwnerFields(PortalId);
                    if (Portal.ModifiedBy == null)
                    {
                        throw new Exception("Portal Ownership definitions is missing!");
                    }

                    QueryByAttribute q = new QueryByAttribute();
                    q.EntityName = PortalEntity;
                    q.ColumnSet = new ColumnSet(true);
                    q.AddAttributeValue(PortalEntityUserField, PortalEntityUserValue);
                    EntityCollection collect = service.RetrieveMultiple(q);

                    Entity PortalUser = collect.Entities.FirstOrDefault();
                    if (PortalUser.Id != new Guid(Id))
                        updateEntity[Portal.ModifiedBy] = new EntityReference(PortalUser.LogicalName, PortalUser.Id);
                }

                updateEntity.Id = new Guid(Id);
                service.Update(updateEntity);
            }
            catch (Exception ex)
            {
                return ex.Message;

            }

            return String.Empty;
        }

        public String GetGridWidgetId(CrmServiceInformation serviceinfo, String PortalId, String WidgetGuid)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);

            Entity entity = service.Retrieve("p4crm_widget", new Guid(WidgetGuid), new ColumnSet(true));
            return entity["p4crm_widgetuniqueid"].ToString();
        }

        public List<GridRowData> GetGridDataPerPage(CrmServiceInformation serviceinfo, String PortalId, List<WidgetParameters> WidgetParameters, String WidgetId, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField, String Page, String RecordCount, String SearchValue = "")
        {
            IOrganizationService dataservice = CreateDestinationCrmService(PortalId, serviceinfo);
            SqlAzureConnection Azure = new SqlAzureConnection();

            WidgetProperties Widget = Azure.GetPageWidgetWithPageWidgetName(WidgetId);
            List<GridRowData> returdata = new List<GridRowData>();
            List<Filters> filters = new List<Filters>();

            if (WidgetParameters.Where(p => p.PageWidgetId.Equals(WidgetId)).ToList().FirstOrDefault() != null)
            {
                filters = WidgetParameters.Where(p => p.PageWidgetId.Equals(WidgetId)).ToList().FirstOrDefault().filters;
            }
            String ActionsButton = WidgetParameters.Where(p => p.PageWidgetId.Equals(WidgetId)).ToList().FirstOrDefault().ActionsCount;
            String PercentageofTotalWidth = WidgetParameters.Where(p => p.PageWidgetId.Equals(WidgetId)).ToList().FirstOrDefault().PercentageofTotalWidth;
            //if not a custom filter!
            if (Widget.CustomFilter == null)
                returdata = GetGridData(dataservice, serviceinfo, PortalId, Widget.ViewId, Widget, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue, RecordCount, Page, SearchValue, ActionsButton, PercentageofTotalWidth);
            else
                returdata = GetGridDataCustom(dataservice, serviceinfo, PortalId, Widget.CustomFilter, Widget, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue, RecordCount, Page, SearchValue, ActionsButton, PercentageofTotalWidth);

            return returdata;

        }

        public List<GridRowData> GetLookupValues(CrmServiceInformation serviceinfo, String PortalId, String LogicalName, String Page, String SearchValue, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField, List<LookupFilter> lookupfilters, String UseCache)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo, UseCache);
            Boolean IsCustomFetch = false;
            List<GridRowData> returndata = new List<GridRowData>();
            XmlNodeList nodelist = null;

            #region Check CustomFetch Wiil be Rendered

            if (lookupfilters.Count > 0)
            {
                if (lookupfilters[0].IsCustom == "1")
                {
                    IsCustomFetch = true;

                }
            }
            #endregion

            #region Get Metadata From Cache

            EntityMetadata metadata = GetAndCheckMetaDataFromCache(service, serviceinfo.ConfigurationId, LogicalName);

            #endregion

            String fetchxml = String.Empty, LayoutXml = String.Empty;
            if (IsCustomFetch == false)
            {
                #region Get Lookup View For That Entity
                QueryExpression savedquery = new QueryExpression();
                savedquery.EntityName = "savedquery";
                savedquery.ColumnSet = new ColumnSet(true);
                savedquery.Criteria.AddCondition(new ConditionExpression("returnedtypecode", ConditionOperator.Equal, metadata.ObjectTypeCode));
                savedquery.Criteria.AddCondition(new ConditionExpression("querytype", ConditionOperator.Equal, 64));//means lookup view
                EntityCollection collect = service.RetrieveMultiple(savedquery);
                #endregion

                //Set Fetch and Layout Xml's
                fetchxml = collect.Entities[0]["fetchxml"].ToString();
                LayoutXml = collect.Entities[0]["layoutxml"].ToString();
            }
            else
            {
                String internalfetch = String.Empty;
                //Multi Lookup Filter , We need to check for entitylogicalname
                var CheckLookupFilter = lookupfilters.Where(p => p.EntityName.Equals(metadata.LogicalName)).ToList();
                if (CheckLookupFilter.Count == 0)
                {
                    #region Get Lookup View For That Entity
                    QueryExpression savedquery = new QueryExpression();
                    savedquery.EntityName = "savedquery";
                    savedquery.ColumnSet = new ColumnSet(true);
                    savedquery.Criteria.AddCondition(new ConditionExpression("returnedtypecode", ConditionOperator.Equal, metadata.ObjectTypeCode));
                    savedquery.Criteria.AddCondition(new ConditionExpression("querytype", ConditionOperator.Equal, 64));//means lookup view
                    EntityCollection collect = service.RetrieveMultiple(savedquery);
                    #endregion

                    internalfetch = collect.Entities[0]["fetchxml"].ToString();
                }
                else
                {
                    //Set Fetch and Layout Xml's
                    internalfetch = CheckLookupFilter[0].FetchXml;
                }
                ChangeFetchXmlDynamicValuesForLookup(service, ref internalfetch, PortalEntity, PortalEntityUserField, PortalEntityUserValue);
                fetchxml = internalfetch;
                LayoutXml = internalfetch;
            }

            FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = fetchxml

            };
            var fetchresponse = (FetchXmlToQueryExpressionResponse)service.Execute(req);
            QueryExpression exp = fetchresponse.Query;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(LayoutXml);
            //Build Columns. If we do not have custom fetchxml then get the columns from lookupview
            if (IsCustomFetch == false)
            {
                nodelist = doc.SelectNodes("//cell");
            }
            else
            {
                nodelist = doc.SelectNodes("//attribute");
            }
            //set the default record count per page
            exp.PageInfo.Count = 11;

            if (Page != String.Empty)
                exp.PageInfo.PageNumber = Convert.ToInt32(Page);

            exp.ColumnSet = new ColumnSet(true);

            if (SearchValue != String.Empty)
            {
                #region If User Want to search something search it in  Quick View
                QueryExpression savedqueryquickfind = new QueryExpression();
                savedqueryquickfind.EntityName = "savedquery";
                savedqueryquickfind.ColumnSet = new ColumnSet(true);
                savedqueryquickfind.Criteria.AddCondition(new ConditionExpression("returnedtypecode", ConditionOperator.Equal, metadata.ObjectTypeCode.Value));
                savedqueryquickfind.Criteria.AddCondition(new ConditionExpression("querytype", ConditionOperator.Equal, 4));

                EntityCollection quickfindcollect = service.RetrieveMultiple(savedqueryquickfind);
                XmlDocument quicfinddoc = new XmlDocument();
                quicfinddoc.LoadXml(Convert.ToString(quickfindcollect.Entities[0]["fetchxml"]));
                XmlNodeList quickfindnodelist = quicfinddoc.SelectNodes("//filter[@isquickfindfields='1']//condition");

                AddSearchFiltersToGrid(service, ref exp, quickfindnodelist, metadata.Attributes, SearchValue);

                #endregion
            }

            //If ıt is not CustomFetch
            if (IsCustomFetch == false)
            {
                AddLookupFilters(service, ref exp, lookupfilters, metadata.Attributes, PortalEntity, PortalEntityUserField, PortalEntityUserValue, LogicalName);

            }
            EntityCollection results = service.RetrieveMultiple(exp);
            int counter = 0;

            if (results.Entities.Count == 0)
            {
                #region If there is no value in grid

                List<GridData> ldata = new List<GridData>();
                foreach (var items in nodelist)
                {
                    if (((System.Xml.XmlElement)(items)).Attributes["ishidden"] == null)
                    {
                        String columns = ((System.Xml.XmlElement)(items)).Attributes["name"].Value;
                        AttributeMetadata meta = metadata.Attributes.FirstOrDefault(x => x.LogicalName.Equals(columns));
                        GridData data = new GridData();
                        data.ColumnName = columns;
                        if (meta != null)
                            data.DisplayName = meta.DisplayName.UserLocalizedLabel.Label;
                        else
                        {
                            //link-entity information
                            XElement form = XElement.Parse(fetchxml);
                            EntityMetadata linkmetadata = GetLinkedEntityMetaData(service, PortalId, serviceinfo.ConfigurationId, form, (XmlNode)items);
                            data.DisplayName = linkmetadata.Attributes.Where(p => p.LogicalName.Equals(columns.Contains('.') ? columns.Split('.')[1] : columns)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;
                        }
                        ldata.Add(data);

                    }
                    GridRowData gridrowdata = new GridRowData();
                    gridrowdata.Data = ldata;
                    gridrowdata.IsEmptyGrid = "1";
                    gridrowdata.RowNumber = "0";
                    returndata.Add(gridrowdata);
                }

                #endregion
            }

            else
            {
                #region Get Total Records
                String TotalRecords = String.Empty;

                if (results.Entities.Count == 11)
                {
                    //means comes from multi entity lookup 
                    if (exp.EntityName != metadata.LogicalName)
                    {
                        String CountXml = String.Format(@"<fetch distinct='false' no-lock='false' mapping='logical' aggregate='true'>
                                            <entity name='{0}'>
                                                
                                                <attribute name='{1}' aggregate='count' alias='alias' />
                                            </entity>
                                        </fetch>", metadata.LogicalName, metadata.PrimaryIdAttribute);
                        EntityCollection collectTotal = service.RetrieveMultiple(new FetchExpression(CountXml));
                        TotalRecords = ((Microsoft.Xrm.Sdk.AliasedValue)(collectTotal.Entities[0].Attributes["alias"])).Value.ToString();
                    }
                    else
                    {
                        QueryExpressionToFetchXmlRequest querytofetchreq = new QueryExpressionToFetchXmlRequest()
                        {
                            Query = exp
                        };
                        QueryExpressionToFetchXmlResponse querytofetchres = (QueryExpressionToFetchXmlResponse)service.Execute(querytofetchreq);
                        XmlDocument docTotal = new XmlDocument();
                        docTotal.LoadXml(querytofetchres.FetchXml);
                        XmlNodeList totalnodes = docTotal.SelectNodes("fetch/entity/attribute");
                        for (int i = totalnodes.Count - 1; i >= 0; i--)
                        {
                            totalnodes[i].ParentNode.RemoveChild(totalnodes[i]);
                        }
                        totalnodes = docTotal.SelectNodes("fetch/entity/order");
                        for (int i = totalnodes.Count - 1; i >= 0; i--)
                        {
                            totalnodes[i].ParentNode.RemoveChild(totalnodes[i]);
                        }

                        totalnodes = docTotal.SelectNodes("fetch/entity/link-entity/attribute");
                        for (int i = totalnodes.Count - 1; i >= 0; i--)
                        {
                            totalnodes[i].ParentNode.RemoveChild(totalnodes[i]);
                        }

                        String aggregetanode = "<attribute name='" + metadata.PrimaryIdAttribute + "' aggregate='count' alias='alias'/>";
                        XmlDocumentFragment xfrag = docTotal.CreateDocumentFragment();
                        xfrag.InnerXml = aggregetanode;

                        XmlNode totalentitynode = docTotal.SelectSingleNode("fetch/entity");
                        totalentitynode.AppendChild(xfrag);

                        XmlAttribute xKey = docTotal.CreateAttribute("aggregate");
                        xKey.Value = "true";
                        totalentitynode = docTotal.SelectSingleNode("fetch");
                        totalentitynode.Attributes.Append(xKey);

                        EntityCollection collectTotal = service.RetrieveMultiple(new FetchExpression(docTotal.OuterXml.ToString()));
                        TotalRecords = ((Microsoft.Xrm.Sdk.AliasedValue)(collectTotal.Entities[0].Attributes["alias"])).Value.ToString();
                    }
                }
                else
                {
                    TotalRecords = results.Entities.Count.ToString();
                }

                #endregion

                #region Build the data

                if (IsCustomFetch == false)
                {
                    bool flag = false;
                    foreach (var item in results.Entities)
                    {
                        counter = counter++;

                        List<GridData> ldata = new List<GridData>();

                        #region Prepare Data

                        foreach (var nodes in nodelist)
                        {
                            if (((System.Xml.XmlElement)(nodes)).Attributes["ishidden"] == null)
                            {
                                String column = ((System.Xml.XmlElement)(nodes)).Attributes["name"].Value;
                                GridData data = new GridData();

                                AttributeMetadata meta = metadata.Attributes.SingleOrDefault(x => x.LogicalName.Equals(column));
                                if (item.Attributes.Contains(column))
                                {
                                    var value = CheckAndReturnCrmTypes(item[column], ref flag, meta, service, column, PortalId, serviceinfo);
                                    data.Value = value[0];
                                }
                                else
                                {
                                    data.Value = String.Empty;
                                }
                                if (column == metadata.PrimaryNameAttribute)
                                    data.NameAttributeValue = item.Attributes.Contains(column) ? item[column].ToString() : String.Empty;
                                data.ColumnName = column;
                                data.RecordId = item.Id.ToString();
                                if (meta != null)
                                    data.DisplayName = metadata.Attributes.Where(p => p.LogicalName.Equals(column)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;
                                else
                                {
                                    XElement form = XElement.Parse(fetchxml);
                                    GetLinkedEntityData(service, serviceinfo, PortalId, item, column.Contains('.') ? column.Split('.')[1] : column, form, (XmlNode)nodes, flag, ref data);


                                }

                                ldata.Add(data);
                            }
                        }

                        GridRowData rowdata = new GridRowData();
                        rowdata.Data = ldata;
                        rowdata.RowNumber = counter.ToString();
                        rowdata.TotalRecord = TotalRecords;

                        returndata.Add(rowdata);
                        #endregion
                    }
                }
                else
                {
                    foreach (var item in results.Entities)
                    {
                        PrepareGridData(serviceinfo, service, PortalId, item, nodelist, metadata.Attributes, fetchxml, counter, ref returndata, metadata.PrimaryNameAttribute, String.Empty, String.Empty, TotalRecords);
                        counter++;
                    }
                }

                #endregion
            }

            return returndata;

        }

        public PersonelInformationForm GetPersonelInformation(CrmServiceInformation serviceinfo, String PortalId, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();
            IOrganizationService dataservice = CreateDestinationCrmService(PortalId, serviceinfo);

            WidgetProperties Widget = Azure.GetProfileWidget(PortalId);
            String fetchxml = Widget.Profile;

            EntityCollection collect = GetUpdateFormData(dataservice, fetchxml, PortalEntity, PortalEntityUserValue, PortalEntityUserField, null);

            PersonelInformationForm form = new PersonelInformationForm();
            form.EntityId = collect[0].Id.ToString();
            form.FormId = Widget.FormId;
            form.WidgetGuid = Widget.WidgetId;
            form.WidgetId = Widget.WidgetUniqueId;
            return form;
        }

        public List<Roles> GetLoginUserRoles(CrmServiceInformation serviceinfo, String PortalId, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();
            IOrganizationService dataservice = CreateDestinationCrmService(PortalId, serviceinfo);
            List<Roles> returnroles = new List<Roles>();

            List<UserRoles> roles = Azure.GetUserRoles(PortalId);

            QueryExpression portalentity = new QueryExpression(PortalEntity);
            portalentity.ColumnSet = new ColumnSet(true);
            portalentity.Criteria.AddCondition(new ConditionExpression(PortalEntityUserField, ConditionOperator.Equal, PortalEntityUserValue));

            foreach (var item in roles)
            {
                List<UserRoleConditions> conditions = Azure.GetUserRolesConditions(item.PortalRoleId);
                //and conditions
                var andconditions = conditions.Where(p => p.FilterType != null && p.FilterType.Value.Equals(0)).ToList();
                FilterExpression filterexp = new FilterExpression();
                foreach (var itemcondition in andconditions)
                {
                    filterexp.FilterOperator = LogicalOperator.And;
                    filterexp.Conditions.Add(new ConditionExpression(itemcondition.AttributeLogicalName, ConditionOperator.Equal, itemcondition.AttributeValue));
                }

                var orconditions = conditions.Where(p => p.FilterType != null && p.FilterType.Value.Equals(2)).ToList();
                foreach (var itemcondition in orconditions)
                {
                    filterexp.FilterOperator = LogicalOperator.Or;
                    filterexp.Conditions.Add(new ConditionExpression(itemcondition.AttributeLogicalName, ConditionOperator.Equal, itemcondition.AttributeValue));
                }
                portalentity.Criteria.AddFilter(filterexp);

                EntityCollection portalcollect = new EntityCollection();
                //this is DashboardController first method in MVC.Check DataService is Still Alive!
                try
                {
                    portalcollect = dataservice.RetrieveMultiple(portalentity);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                if (portalcollect.Entities.Count > 0)
                {
                    Roles r = new Roles();
                    r.Name = item.PortalRoleName;
                    r.Id = item.PortalRoleId;
                    returnroles.Add(r);
                }
                portalentity.Criteria.Filters.Clear();
            }


            return returnroles;
        }

        public List<Navigation> GetExternalNavigation(String PortalId)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();

            List<Navigation> returnnavigation = new List<Navigation>();

            List<Navigation> navigations = Azure.GetExternalNavigation(PortalId);
            foreach (var item in navigations)
            {
                var uniqueitem = returnnavigation.Where(p => p.NavigationId.Equals(item.NavigationId)).ToList();
                if (uniqueitem.Count == 0)
                {
                    Navigation n = new Navigation();
                    n.PageId = item.PageId;
                    n.Name = item.Name;
                    n.NavigationId = item.NavigationId;
                    n.Order = item.Order;
                    n.ParentNavigationName = item.ParentNavigationName; ;
                    n.ParentNavigationId = item.ParentNavigationId;
                    n.ExternalLink = item.ExternalLink;
                    n.UrlName = item.UrlName;
                    n.UniqueId = item.UniqueId;
                    returnnavigation.Add(n);
                }
            }
            return returnnavigation;
        }

        public List<Navigation> GetNavigationOfUser(CrmServiceInformation serviceinfo, String PortalId, List<Roles> roles)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();

            List<Navigation> returnnavigation = new List<Navigation>();

            String[] RoleIds = roles.Select(p => p.Id).ToArray();

            String[] navigationIds = Azure.GetRelationOfNavigationAndPortalRole(RoleIds);

            if (navigationIds.Length == 0)
            {
                return returnnavigation;
            }
            List<Navigation> navigations = Azure.GetNavigationOfUser(navigationIds);

            foreach (var item in navigations)
            {
                var uniqueitem = returnnavigation.Where(p => p.NavigationId.Equals(item.NavigationId)).ToList();
                if (uniqueitem.Count == 0)
                {
                    Navigation n = new Navigation();
                    n.PageId = item.PageId;
                    n.Name = item.Name;
                    n.NavigationId = item.NavigationId;
                    n.Order = item.Order;
                    n.ParentNavigationName = item.ParentNavigationName; ;
                    n.ParentNavigationId = item.ParentNavigationId;
                    n.ExternalLink = item.ExternalLink;
                    n.UrlName = item.UrlName;
                    n.UniqueId = item.UniqueId;
                    returnnavigation.Add(n);
                }
            }
            return returnnavigation;

        }

        public List<GridRowData> GetCalculatedFieldRecords(CrmServiceInformation serviceinfo, String PortalId, String CalculatedWidgetId, String GridWidgetId, List<Filters> filters, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField, String Page, String SearchValue, String RecordCount)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();
            IOrganizationService dataservice = CreateDestinationCrmService(PortalId, serviceinfo);

            List<GridRowData> returndata = new List<GridRowData>();
            String GridXml = String.Empty, FetchXml = String.Empty;

            #region Getting Attributes

            WidgetProperties GridWidget = Azure.GetWidgetWithWidgetId(GridWidgetId);

            if (GridWidget.CustomFilter != null)
            {
                if (GridWidget.UseCustomFilter == true)
                {
                    GridXml = GridWidget.CustomFilter;

                }
                else if (GridWidget.UseCustomFilter == false)
                {
                    GridXml = ReturnFetchXmlOfView(dataservice, GridWidget.ViewId);

                }
            }
            else
            {
                GridXml = ReturnFetchXmlOfView(dataservice, GridWidget.ViewId);
            }
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(GridXml);
            XmlNodeList nodelist = doc.SelectNodes("//attribute");
            #endregion


            WidgetProperties PWidget = Azure.GetPageWidgetWithPageWidgetName(CalculatedWidgetId);

            if (PWidget.CustomFilter == null)
            {
                #region Not Custom Filter
                FetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                FetchXml += "<entity name='" + PWidget.EntityLogicalName + "'>";

                foreach (var items in nodelist)
                {
                    if ((((System.Xml.XmlElement)(items)).ParentNode).Name == "link-entity")
                        continue;

                    if (((System.Xml.XmlElement)(items)).Attributes["name"].Value == PWidget.EntityLogicalName + "id")
                        continue;

                    FetchXml += "<attribute name='" + ((System.Xml.XmlElement)(items)).Attributes["name"].Value + "' />";

                }
                if (filters.Count > 0)
                {
                    String FilterStr = AddFilter(dataservice, PortalId, serviceinfo.ConfigurationId, PWidget, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue);
                    FetchXml += FilterStr;
                }

                FetchXml += "</entity>";
                FetchXml += "</fetch>";
                #endregion
            }
            else
            {
                #region CustomFilter

                XElement form = XElement.Parse(PWidget.CustomFilter);
                AddCustomFiltersToFetchXml(dataservice, ref form, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                if (filters.Count > 0)
                {
                    String FilterStr = AddFilter(dataservice, PortalId, serviceinfo.ConfigurationId, PWidget, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(form.ToString());

                    XmlDocumentFragment xfrag = xdoc.CreateDocumentFragment();
                    xfrag.InnerXml = FilterStr;
                    xdoc.SelectSingleNode("//entity").AppendChild(xfrag);

                    FetchXml = xdoc.OuterXml;
                }
                else
                {
                    FetchXml = form.ToString();
                }
                XmlDocument xdoc1 = new XmlDocument();
                xdoc1.LoadXml(FetchXml);
                XmlNode node = xdoc1.SelectSingleNode("//entity//attribute");
                XmlNode node1 = xdoc1.SelectSingleNode("//entity");
                node1.RemoveChild(node);

                XmlElement root = xdoc1.DocumentElement;
                root.RemoveAttribute("aggregate");


                foreach (var items in nodelist)
                {
                    if ((((System.Xml.XmlElement)(items)).ParentNode).Name == "link-entity")
                        continue;

                    if (((System.Xml.XmlElement)(items)).Attributes["name"].Value == PWidget.EntityLogicalName + "id")
                        continue;

                    String s = "<attribute name='" + ((System.Xml.XmlElement)(items)).Attributes["name"].Value + "' />";
                    XmlDocumentFragment xfrag = xdoc1.CreateDocumentFragment();
                    xfrag.InnerXml = s;
                    xdoc1.SelectSingleNode("//entity").AppendChild(xfrag);

                }
                FetchXml = xdoc1.InnerXml;

                #endregion
            }

            FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = FetchXml

            };
            var response = (FetchXmlToQueryExpressionResponse)dataservice.Execute(req);

            QueryExpression exp = response.Query;
            exp.PageInfo.Count = Convert.ToInt32(RecordCount) + 1;

            if (Page != String.Empty)
                exp.PageInfo.PageNumber = Convert.ToInt32(Page);

            EntityMetadata metadata = GetAndCheckMetaDataFromCache(dataservice, serviceinfo.ConfigurationId, PWidget.EntityLogicalName);

            if (SearchValue != String.Empty)
            {
                QueryExpression savedqueryquickfind = new QueryExpression();
                savedqueryquickfind.EntityName = "savedquery";
                savedqueryquickfind.ColumnSet = new ColumnSet(true);
                savedqueryquickfind.Criteria.AddCondition(new ConditionExpression("returnedtypecode", ConditionOperator.Equal, metadata.ObjectTypeCode.Value));
                savedqueryquickfind.Criteria.AddCondition(new ConditionExpression("querytype", ConditionOperator.Equal, 4));

                EntityCollection quickfindcollect = dataservice.RetrieveMultiple(savedqueryquickfind);
                XmlDocument quicfinddoc = new XmlDocument();
                quicfinddoc.LoadXml(Convert.ToString(quickfindcollect.Entities[0]["fetchxml"]));
                XmlNodeList quickfindnodelist = quicfinddoc.SelectNodes("//filter[@isquickfindfields='1']//condition");

                AddSearchFiltersToGrid(dataservice, ref exp, quickfindnodelist, metadata.Attributes, SearchValue);
            }
            EntityCollection collection = dataservice.RetrieveMultiple(exp);


            int p = 0;
            foreach (var item in collection.Entities)
            {
                PrepareGridData(serviceinfo, dataservice, PortalId, item, nodelist, metadata.Attributes, FetchXml, p, ref returndata, metadata.PrimaryNameAttribute, String.Empty, String.Empty, String.Empty);
                p++;
            }

            return returndata;
        }

        public List<InitialValues> GetAndChangeDynamicInitialValues(CrmServiceInformation serviceinfo, String PortalId, List<InitialValues> InitialValues, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);

            List<InitialValues> ReturnIntialValues = new List<InitialValues>();

            QueryByAttribute q = new QueryByAttribute();
            q.EntityName = PortalEntity;
            q.ColumnSet = new ColumnSet(true);
            q.AddAttributeValue(PortalEntityUserField, PortalEntityUserValue);

            Entity Portal = service.RetrieveMultiple(q).Entities.FirstOrDefault();
            String EntityName = Portal.LogicalName;

            foreach (var item in InitialValues)
            {
                if (item.Static == "false")
                {

                    if (item.InitialValue.Split('.').Length == 1)
                    {
                        continue;
                    }

                    String Attr = Convert.ToString(item.InitialValue.Split('.')[1]);

                    if (Attr == EntityName + "id")
                    {
                        EntityMetadata metadata = GetAndCheckMetaDataFromCache(service, serviceinfo.ConfigurationId, EntityName);
                        item.InitialValue = Portal[Attr].ToString();
                        item.LookupNameValue = Portal[metadata.PrimaryNameAttribute].ToString();

                    }
                    else if (Portal.Contains(Attr) ? Portal[Attr] is EntityReference : false)
                    {
                        item.InitialValue = ((EntityReference)Portal[Attr]).Id.ToString();
                        item.LookupNameValue = ((EntityReference)Portal[Attr]).Name.ToString();
                    }
                    else if (Portal.Contains(Attr) ? Portal[Attr] is OptionSetValue : false)
                    {
                        item.InitialValue = ((OptionSetValue)Portal[Attr]).Value.ToString();
                    }
                    else if (Portal.Contains(Attr) ? Portal[Attr] is Money : false)
                    {
                        item.InitialValue = ((Money)Portal[Attr]).Value.ToString();
                    }
                    else
                    {
                        item.InitialValue = Portal.Contains(Attr) ? Portal[Attr].ToString() : string.Empty;
                    }
                    ReturnIntialValues.Add(item);
                }
                else
                {
                    if (item.LookupLogicalName != String.Empty)
                    {
                        EntityMetadata metadata = GetAndCheckMetaDataFromCache(service, serviceinfo.ConfigurationId, item.LookupLogicalName);
                        //for static lookup values

                        Entity LookUp = service.Retrieve(item.LookupLogicalName, new Guid(item.InitialValue), new ColumnSet(true));

                        item.LookupNameValue = LookUp[metadata.PrimaryNameAttribute].ToString();

                        ReturnIntialValues.Add(item);
                    }
                }
            }
            return ReturnIntialValues;
        }

        public String UpdateEntityImage(CrmServiceInformation serviceinfo, String PortalId, String Image, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);

            QueryByAttribute q = new QueryByAttribute();
            q.EntityName = PortalEntity;
            q.ColumnSet = new ColumnSet(true);
            q.AddAttributeValue(PortalEntityUserField, PortalEntityUserValue);

            Entity Portal = service.RetrieveMultiple(q).Entities.FirstOrDefault();
            try
            {
                Portal["entityimage"] = Convert.FromBase64String(Image.Replace("data:image/jpeg;base64,", ""));
                service.Update(Portal);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return String.Empty;
        }

        public AttachmentReturn AddNotesToRelatedRecord(CrmServiceInformation serviceinfo, String PortalId, ClassLibrary.Attachment Attachment)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);
            AttachmentReturn r = new AttachmentReturn();
            Guid Id = Guid.Empty;

            try
            {
                Entity entity = new Entity();
                entity.LogicalName = "annotation";
                entity["objectid"] = new EntityReference(Attachment.EntityName, new Guid(Attachment.RecordId));
                entity["mimetype"] = Attachment.MimeType;
                entity["documentbody"] = Attachment.DocumentBody;
                entity["filename"] = Attachment.FileName;
                entity["notetext"] = Attachment.Subject;
                Id = service.Create(entity);
            }
            catch (Exception ex)
            {
                r.Status = "error";
                r.Content = ex.Message;
                return r;
            }
            r.Status = "success";
            r.Content = Id.ToString();

            return r;
        }

        public String DeleteNote(CrmServiceInformation serviceinfo, String PortalId, String AttachmentId)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);

            try
            {
                service.Delete("annotation", new Guid(AttachmentId));

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return String.Empty;
        }

        public String ExecuteCustomActions(CrmServiceInformation serviceinfo, String PortalId, String EntityId, String WorkflowId)
        {

            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);

            try
            {
                ExecuteWorkflowRequest Request = new ExecuteWorkflowRequest
                {
                    EntityId = new Guid(EntityId),
                    WorkflowId = new Guid(WorkflowId)
                };

                ExecuteWorkflowResponse Response = (ExecuteWorkflowResponse)service.Execute(Request);

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return String.Empty;
        }

        public List<CrmEntities> GetCrmEntityList(String ConfigurationId, CrmServiceInformation information)
        {
            IOrganizationService service = CheckConfigurationService(ConfigurationId, information);

            List<CrmEntities> ReturnEntities = new List<CrmEntities>();

            RetrieveAllEntitiesRequest req = new RetrieveAllEntitiesRequest()
            {
                RetrieveAsIfPublished = true

            };
            RetrieveAllEntitiesResponse res = (RetrieveAllEntitiesResponse)service.Execute(req);

            EntityMetadata[] metadata = (EntityMetadata[])(res.Results.Values.ElementAt(0));
            metadata = metadata.Where(p => p.IsIntersect.Value.Equals(false) && p.IsCustomizable.Value.Equals(true)).ToArray();
            foreach (var item in metadata)
            {
                if (item.DisplayName.LocalizedLabels != null)
                {
                    CrmEntities c = new CrmEntities();
                    c.Id = item.LogicalName;
                    c.LogicalName = item.LogicalName;
                    c.Label = item.DisplayName.UserLocalizedLabel.Label;
                    ReturnEntities.Add(c);
                }
            }
            ReturnEntities = ReturnEntities.OrderBy(p => p.Label).ToList();
            return ReturnEntities;

        }

        public List<CrmAttributes> GetEntityAttributes(String ConfigurationId, String EntityName, CrmServiceInformation information, String calculatedfieldtype, String DateTime, String Lookup)
        {
            IOrganizationService service = CheckConfigurationService(ConfigurationId, information);
            List<CrmAttributes> ReturnAttributes = new List<CrmAttributes>();

            RetrieveEntityRequest req = new RetrieveEntityRequest
            {
                LogicalName = EntityName,
                RetrieveAsIfPublished = true,
                EntityFilters = EntityFilters.All
            };

            RetrieveEntityResponse res = (RetrieveEntityResponse)service.Execute(req);

            AttributeMetadata[] metadata = res.EntityMetadata.Attributes;

            foreach (var item in metadata)
            {
                if (item.DisplayName != null && item.DisplayName.LocalizedLabels != null && item.DisplayName.UserLocalizedLabel != null)
                {
                    CrmAttributes c = new CrmAttributes();
                    c.Id = item.LogicalName;
                    c.Label = item.DisplayName.UserLocalizedLabel.Label;
                    c.LogicalName = item.LogicalName;
                    c.Type = item.AttributeType.Value.ToString();
                    if (item.AttributeType.Value == AttributeTypeCode.Lookup || item.AttributeType.Value == AttributeTypeCode.Customer)
                    {
                        c.LookupLogicalName = ((Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata)(item)).Targets[0];
                    }
                    else
                    {
                        c.LookupLogicalName = String.Empty;

                    }
                    ReturnAttributes.Add(c);
                }
            }
            if (calculatedfieldtype != null)
            {
                if (calculatedfieldtype == "1" || calculatedfieldtype == "2")
                {
                    ReturnAttributes = ReturnAttributes.Where(p => p.Type.Equals(AttributeTypeCode.BigInt.ToString()) ||
                                                 p.Type.Equals(AttributeTypeCode.Decimal.ToString()) ||
                                                 p.Type.Equals(AttributeTypeCode.Double.ToString()) ||
                                                 p.Type.Equals(AttributeTypeCode.Integer.ToString()) ||
                                                 p.Type.Equals(AttributeTypeCode.Money.ToString())).ToList();
                }
                // Picture widget
                else if (calculatedfieldtype == "9")
                {
                    ReturnAttributes = ReturnAttributes.Where(p => p.Type.Equals(AttributeTypeCode.String.ToString())).ToList();
                }
            }
            if (DateTime != null)
            {
                ReturnAttributes = ReturnAttributes.Where(p => p.Type.Equals(AttributeTypeCode.DateTime.ToString())).ToList();
            }
            if (!String.IsNullOrEmpty(Lookup))
            {
                ReturnAttributes = ReturnAttributes.Where(p => p.Type.Equals(AttributeTypeCode.Lookup.ToString()) || p.Type.Equals(AttributeTypeCode.Customer.ToString())).ToList();
            }
            ReturnAttributes = ReturnAttributes.OrderBy(p => p.Label).ToList();
            return ReturnAttributes;

        }

        public List<CrmViews> GetEntityViews(String ConfigurationId, String EntityName, CrmServiceInformation information)
        {
            IOrganizationService service = CheckConfigurationService(ConfigurationId, information);
            List<CrmViews> ReturnViews = new List<CrmViews>();

            QueryExpression expression = new QueryExpression();
            expression.EntityName = "savedquery";
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("returnedtypecode", ConditionOperator.Equal, EntityName);

            EntityCollection collect = service.RetrieveMultiple(expression);

            foreach (var item in collect.Entities)
            {
                CrmViews v = new CrmViews();
                v.Label = Convert.ToString(item["name"]);
                v.LogicalName = item.Id.ToString();
                v.Id = item.Id.ToString();
                ReturnViews.Add(v);
            }

            return ReturnViews;
        }

        public List<CrmForms> GetEntityForms(String ConfigurationId, String EntityName, CrmServiceInformation information)
        {
            IOrganizationService service = CheckConfigurationService(ConfigurationId, information);
            List<CrmForms> ReturnForms = new List<CrmForms>();

            QueryExpression expression = new QueryExpression();
            expression.EntityName = "systemform";
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, EntityName);

            EntityCollection collect = service.RetrieveMultiple(expression);

            foreach (var item in collect.Entities)
            {
                CrmForms v = new CrmForms();
                v.Label = Convert.ToString(item["name"]);
                v.LogicalName = item.Id.ToString();
                v.Id = item.Id.ToString();
                ReturnForms.Add(v);
            }

            return ReturnForms;
        }

        public List<SubGrid> GetFormSubGrids(String ConfigurationId, String EntityName, String FormId, String PortalId, CrmServiceInformation information)
        {
            IOrganizationService service = CheckConfigurationService(ConfigurationId, information);
            List<SubGrid> returnSubGrid = new List<SubGrid>();

            Entity SystemForm = service.Retrieve("systemform", new Guid(FormId), new ColumnSet("formxml", "name", "objecttypecode"));

            List<Tabs> tabs = ParseFormXml(service, SystemForm["formxml"].ToString(), SystemForm["objecttypecode"].ToString(), String.Empty, PortalId, information);

            foreach (var item in tabs)
            {
                foreach (var columnitem in item.Columns)
                {
                    foreach (var sectionitem in columnitem.Sections)
                    {
                        foreach (var rowitem in sectionitem.Rows)
                        {
                            if (rowitem.ElementType == "subgrid")
                            {
                                SubGrid subgrid = new SubGrid();
                                subgrid.Name = rowitem.Label;
                                subgrid.Id = rowitem.SubGridId;
                                subgrid.SubGridId = rowitem.SubGridId;
                                subgrid.EntityName = rowitem.SubGridTargetEntity;
                                returnSubGrid.Add(subgrid);
                            }
                        }
                    }
                }
            }
            return returnSubGrid;
        }

        public List<FormLookups> GetFormLookups(String ConfigurationId, String EntityName, String FormId, String PortalId, CrmServiceInformation information)
        {
            IOrganizationService service = CheckConfigurationService(ConfigurationId, information);
            List<FormLookups> returnFormLookups = new List<FormLookups>();

            Entity SystemForm = service.Retrieve("systemform", new Guid(FormId), new ColumnSet("formxml", "name", "objecttypecode"));

            List<Tabs> tabs = ParseFormXml(service, SystemForm["formxml"].ToString(), SystemForm["objecttypecode"].ToString(), String.Empty, PortalId, information);

            foreach (var item in tabs)
            {
                foreach (var columnitem in item.Columns)
                {
                    foreach (var sectionitem in columnitem.Sections)
                    {
                        foreach (var rowitem in sectionitem.Rows)
                        {
                            if (rowitem.Type == "lookup")
                            {
                                FormLookups lookups = new FormLookups();
                                lookups.Name = rowitem.DisplayName;
                                lookups.Id = rowitem.LogicalName;
                                lookups.LogicalName = rowitem.LogicalName;
                                lookups.LookupLogicalName = rowitem.LookupLogicalName;
                                returnFormLookups.Add(lookups);
                            }
                        }
                    }
                }
            }
            return returnFormLookups;
        }

        public List<Language> GetLanguages(String ConfigurationId, CrmServiceInformation information)
        {
            IOrganizationService service = CheckConfigurationService(ConfigurationId, information);
            List<Language> returnLanguage = new List<Language>();

            RetrieveAvailableLanguagesRequest Request = new RetrieveAvailableLanguagesRequest();
            RetrieveAvailableLanguagesResponse Response = (RetrieveAvailableLanguagesResponse)service.Execute(Request);

            var x = Response.Results.FirstOrDefault().Value;

            var Langs = (int[])x;
            for (int i = 0; i < Langs.Length; i++)
            {
                Language L = new Language();
                L.LangId = Langs[i].ToString();

                var c = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList();
                var c1 = c.Where(p => p.LCID.Equals(Langs[i])).FirstOrDefault();
                L.Label = c1.DisplayName.Substring(0, c1.DisplayName.IndexOf("(") - 1);
                L.NativeName = c1.Name;
                returnLanguage.Add(L);
            }
            return returnLanguage;
        }

        public List<Formats> GetFormats(String LangId, String FormatType)
        {
            List<Formats> returnFormats = new List<Formats>();

            var Cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList();

            var SpecificCulture = Cultures.Where(p => p.LCID.Equals(Convert.ToInt32(LangId))).FirstOrDefault();

            string[] x1 = SpecificCulture.DateTimeFormat.GetAllDateTimePatterns(Convert.ToChar(FormatType));

            foreach (var item in x1)
            {
                if (x1.Contains("MMM") != true)
                {
                    Formats F = new Formats();
                    F.Value = item;
                    returnFormats.Add(F);
                }
            }
            return returnFormats;

        }

        public List<ClassLibrary.TimeZone> GetTimeZones()
        {
            var TimeZones = TimeZoneInfo.GetSystemTimeZones();
            List<ClassLibrary.TimeZone> returnTimeZones = new List<ClassLibrary.TimeZone>();

            foreach (var item in TimeZones)
            {
                ClassLibrary.TimeZone zone = new ClassLibrary.TimeZone();
                zone.TotalMinutes = item.BaseUtcOffset.TotalMinutes.ToString();
                zone.DisplayName = item.DisplayName;
                returnTimeZones.Add(zone);
            }
            return returnTimeZones;
        }

        public List<Language> GetPortalLanguages(String PortalId)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();
            List<Language> Languages = Azure.GetPortalLanguages(PortalId);

            return Languages;
        }

        public String GetPortalBaseLanguage(String ConfigurationId, CrmServiceInformation information)
        {
            IOrganizationService service = CheckConfigurationService(ConfigurationId, information);
            QueryExpression ex = new QueryExpression("organization");
            ex.ColumnSet = new ColumnSet(new String[] { "languagecode" });
            var Organization = service.RetrieveMultiple(ex).Entities.FirstOrDefault();

            var BaseLangId = Organization.GetAttributeValue<Int32>("languagecode");
            return BaseLangId.ToString();

        }

        public String GetPortalMainLanguage(String PortalId, CrmServiceInformation information)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, information);

            QueryByAttribute q = new QueryByAttribute();
            q.EntityName = "systemuser";
            q.ColumnSet = new ColumnSet(true);
            if (information.CrmType == "2")
            {
                q.AddAttributeValue("domainname", information.UserName);
            }
            else
            {
                if (information.CrmType == "1")
                {
                    q.AddAttributeValue("domainname", information.Domain + "\\" + information.UserName);
                }
                else
                {
                    q.AddAttributeValue("domainname", information.UserName);
                }
            }

            EntityCollection collect = service.RetrieveMultiple(q);

            Entity User = collect.Entities.FirstOrDefault();

            RetrieveUserSettingsSystemUserRequest req = new RetrieveUserSettingsSystemUserRequest
            {
                ColumnSet = new ColumnSet() { AllColumns = true },
                EntityId = User.Id
            };

            RetrieveUserSettingsSystemUserResponse userResponse = (RetrieveUserSettingsSystemUserResponse)service.Execute(req);
            return userResponse.Entity["uilanguageid"].ToString();

        }

        public List<Language> GetLanguagesForPageWidget(CrmServiceInformation information)
        {
            List<Language> returnValue = new List<Language>();

            List<Language> languageList = this.GetLanguages(information.ConfigurationId, information);

            string mainlanguage = this.GetPortalMainLanguage(string.Empty, information);
            foreach (var item in languageList)
            {
                if (item.LangId.Equals(mainlanguage))
                {
                    item.IsMain = "true";
                }
            }

            returnValue = languageList;


            return returnValue;
        }

        public String ResetPassword(String PortalId, String Value, String Template, CrmServiceInformation information)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, information);
            try
            {
                SqlAzureConnection Azure = new SqlAzureConnection();
                ResetPassword ResetPassword = Azure.GetPortalInformationForPasswordReset(PortalId);

                QueryByAttribute q = new QueryByAttribute();
                q.EntityName = ResetPassword.EntityName;
                q.ColumnSet = new ColumnSet(true);
                q.AddAttributeValue(ResetPassword.UserName, Value);
                EntityCollection collect = service.RetrieveMultiple(q);

                if (collect.Entities.Count == 0)
                {
                    QueryByAttribute q1 = new QueryByAttribute();
                    q1.EntityName = ResetPassword.EntityName;
                    q1.ColumnSet = new ColumnSet(true);
                    q1.AddAttributeValue(ResetPassword.EmailValue, Value);
                    collect = service.RetrieveMultiple(q1);

                    if (collect.Entities.Count == 0)
                    {
                        return int.MaxValue.ToString();
                    }
                    else
                    {
                        SendMail(service, PortalId, collect.Entities.FirstOrDefault(), ResetPassword.AdminAddress, ResetPassword.EmailAlias, Value, Template);
                    }
                }
                else
                {
                    SendMail(service, PortalId, collect.Entities.FirstOrDefault(), ResetPassword.AdminAddress, ResetPassword.EmailAlias, collect[0][ResetPassword.EmailValue].ToString(), Template);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return String.Empty;
        }

        public List<CrmWorkFlows> GetWorkFlows(String ConfigurationId, String EntityName, CrmServiceInformation information)
        {
            IOrganizationService service = CheckConfigurationService(ConfigurationId, information);
            List<CrmWorkFlows> ReturnWorkFlows = new List<CrmWorkFlows>();

            QueryExpression expression = new QueryExpression();
            expression.EntityName = "workflow";
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("primaryentity", ConditionOperator.Equal, EntityName);
            expression.Criteria.AddCondition("type", ConditionOperator.Equal, 1);

            EntityCollection collect = service.RetrieveMultiple(expression);

            foreach (var item in collect.Entities)
            {

                CrmWorkFlows v = new CrmWorkFlows();
                v.Label = Convert.ToString(item["name"]);
                v.Id = item.Id.ToString();
                v.SolutionId = Convert.ToString(item["solutionid"]);
                ReturnWorkFlows.Add(v);

            }
            ReturnWorkFlows = ReturnWorkFlows.OrderBy(p => p.Label).ToList();
            return ReturnWorkFlows;
        }

        private String ReturnFetchXmlOfView(IOrganizationService service, String ViewId)
        {
            QueryExpression savedquery = new QueryExpression();
            savedquery.EntityName = "savedquery";
            savedquery.ColumnSet = new ColumnSet(true);

            ConditionExpression condt = new ConditionExpression();
            condt.AttributeName = "savedqueryid";
            condt.Operator = ConditionOperator.Equal;
            condt.Values.Add(new Guid(ViewId));

            savedquery.Criteria.AddCondition(condt);

            EntityCollection savedquerycollect = service.RetrieveMultiple(savedquery);

            return Convert.ToString(savedquerycollect.Entities[0]["fetchxml"]);
        }

        private EntityCollection GetUpdateFormData(IOrganizationService service, String FetchXml, String PortalEntity, String PortalEntityUserValue, String PortalEntityUserField, WidgetParameters Parameters)
        {
            XElement Xml = XElement.Parse(FetchXml);
            AddCustomFiltersToFetchXml(service, ref Xml, PortalEntity, PortalEntityUserField, PortalEntityUserValue);
            if (Parameters != null)
                ChangeQueryStrings(ref Xml, Parameters);

            FetchXmlToQueryExpressionRequest fetchxmlreq = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = Xml.ToString()
            };

            FetchXmlToQueryExpressionResponse fetchxmlres = (FetchXmlToQueryExpressionResponse)service.Execute(fetchxmlreq);

            fetchxmlres.Query.PageInfo.Count = 1;
            fetchxmlres.Query.PageInfo.PageNumber = 1;

            EntityCollection collect = service.RetrieveMultiple(fetchxmlres.Query);

            return collect;
        }

        private void AddLookupFilters(IOrganizationService service, ref QueryExpression exp, List<LookupFilter> lookupfilters, AttributeMetadata[] metadata, String PortalEntity, String PortalEntityUserField, String PortalEntityUserValue, String EntityName)
        {
            if (PortalEntity != String.Empty)
            {
                QueryByAttribute q = new QueryByAttribute();
                q.EntityName = PortalEntity;
                q.ColumnSet = new ColumnSet(true);
                q.AddAttributeValue(PortalEntityUserField, PortalEntityUserValue);

                EntityCollection collect = service.RetrieveMultiple(q);

                Entity Portal = collect.Entities.FirstOrDefault();

                var OrFilters = lookupfilters.Where(p => p.FilterType.Equals("or")).ToList();
                FilterExpression filterexpor = new FilterExpression();
                filterexpor.FilterOperator = LogicalOperator.Or;

                AddLookupConditionsToFilterEx(OrFilters, Portal, ref filterexpor, EntityName);

                var AndFilters = lookupfilters.Where(p => p.FilterType.Equals("and")).ToList();
                FilterExpression filterexpand = new FilterExpression();
                filterexpand.FilterOperator = LogicalOperator.And;

                AddLookupConditionsToFilterEx(AndFilters, Portal, ref filterexpand, EntityName);


                if (filterexpor.Conditions.Count > 0)
                {
                    exp.Criteria.AddFilter(filterexpor);
                }

                if (filterexpand.Conditions.Count > 0)
                {
                    exp.Criteria.AddFilter(filterexpand);
                }
            }

        }

        private String AddFilter(IOrganizationService service, String PortalId, String ConfigurationId, WidgetProperties Widget, List<Filters> filters, String PortalEntity, String PortalEntityUserField, String PortalEntityUserValue)
        {
            String FilterString = "<filter type='and'>";

            foreach (var item in filters)
            {
                #region Iterate Filters

                FilterString += "<filter type='" + item.type + "'>";
                foreach (var filter in item.filter)
                {
                    if (filter.isstatic == "0")
                    {
                        #region Not Static Filters
                        if (filter.condition.ToLower() == "equalscurrentuser")
                        {
                            EntityMetadata EntityMetadata = GetAndCheckMetaDataFromCache(service, ConfigurationId, PortalEntity);

                            QueryByAttribute q = new QueryByAttribute();
                            q.EntityName = PortalEntity;
                            q.ColumnSet = new ColumnSet(true);
                            q.AddAttributeValue(PortalEntityUserField, PortalEntityUserValue);

                            EntityCollection collect = service.RetrieveMultiple(q);

                            if (EntityMetadata.LogicalName == Widget.EntityLogicalName)
                            {
                                FilterString += "<condition attribute='" + EntityMetadata.PrimaryIdAttribute + "' uitype='" + PortalEntity + "' operator='eq' value='" + collect[0].Id + "' />";
                            }
                            else
                            {
                                OneToManyRelationshipMetadata[] onetomany = EntityMetadata.OneToManyRelationships;

                                OneToManyRelationshipMetadata[] specificentity = onetomany.Where(p => p.ReferencingEntity.Equals(Widget.EntityLogicalName)).ToArray();

                                if (specificentity.Length > 0)
                                {
                                    FilterString += "<condition attribute='" + specificentity[0].ReferencingAttribute + "' uitype='" + PortalEntity + "' operator='eq' value='" + collect[0].Id + "' />";
                                }
                            }

                        }
                        #endregion

                    }
                    else if (filter.isstatic == "1")
                    {
                        #region Static Filters
                        if (!String.IsNullOrEmpty(filter.uitype))
                            FilterString += "<condition attribute='" + filter.attributtename + "' uitype='" + filter.uitype + "' operator='" + filter.Operator + "' value='" + filter.value + "' />";
                        else
                            FilterString += "<condition attribute='" + filter.attributtename + "' operator='" + filter.Operator + "' value='" + filter.value + "' />";
                        #endregion

                    }
                }
                FilterString += "</filter>";
                #endregion
            }
            FilterString += "</filter>";

            return FilterString;
        }

        private void AddLookupConditionsToFilterEx(List<LookupFilter> lookupfilters, Entity Portal, ref FilterExpression FilterExp, String EntityName)
        {
            foreach (var item in lookupfilters)
            {
                if (item.EntityName != EntityName)
                {
                    continue;
                }
                ConditionOperator Operator = new ConditionOperator();

                if (item.Operator == "eq")
                {
                    Operator = ConditionOperator.Equal;
                }
                else if (item.Operator == "ne")
                {
                    Operator = ConditionOperator.NotEqual;
                }
                else if (item.Operator == "like")
                {
                    Operator = ConditionOperator.Like;
                }
                else if (item.Operator == "not-like")
                {
                    Operator = ConditionOperator.NotLike;
                }
                else if (item.Operator == "not-null")
                {
                    Operator = ConditionOperator.NotNull;
                }
                else if (item.Operator == "null")
                {
                    Operator = ConditionOperator.Null;

                }
                else
                {
                    Operator = ConditionOperator.Equal;
                }

                if (item.IsStatic == "0")
                {
                    if (item.Value.Contains("@portaluser"))
                    {
                        if (item.Value.Split('.').Length > 1)
                        {
                            String attributename = item.Value.Split('.')[1], Value = String.Empty;

                            if (Portal[attributename] is EntityReference)
                            {
                                Value = ((EntityReference)Portal[attributename]).Id.ToString();
                            }
                            else if (Portal[attributename] is OptionSetValue)
                            {
                                Value = ((OptionSetValue)Portal[attributename]).Value.ToString();
                            }
                            else if (Portal[attributename] is Money)
                            {
                                Value = ((Money)Portal[attributename]).Value.ToString();
                            }
                            else
                            {
                                Value = Portal[attributename].ToString();
                            }

                            FilterExp.Conditions.Add(new ConditionExpression(item.ValueLogicalName, Operator, Value));
                        }
                        else
                        {
                            FilterExp.Conditions.Add(new ConditionExpression(item.ValueLogicalName, Operator, Portal.Id));
                        }
                    }
                }
                else
                {
                    if (item.ValueLogicalName == "statecode")
                    {
                        FilterExp.Conditions.Add(new ConditionExpression(item.ValueLogicalName, Operator, Convert.ToInt32(item.Value)));
                    }
                    else if (item.ValueLogicalName == "statuscode")
                    {
                        FilterExp.Conditions.Add(new ConditionExpression(item.ValueLogicalName, Operator, Convert.ToInt32(item.Value)));
                    }
                    else
                    {
                        if (Operator == ConditionOperator.Null || Operator == ConditionOperator.NotNull)
                        {
                            FilterExp.Conditions.Add(new ConditionExpression(item.ValueLogicalName, Operator));
                        }
                        else
                        {
                            FilterExp.Conditions.Add(new ConditionExpression(item.ValueLogicalName, Operator, item.Value));
                        }
                    }
                }
            }
        }

        private void AddSearchFiltersToGrid(IOrganizationService service, ref QueryExpression exp, XmlNodeList nodelist, AttributeMetadata[] metadata, String SearchValue)
        {
            FilterExpression filterexp = new FilterExpression();
            filterexp.FilterOperator = LogicalOperator.Or;

            foreach (var items in nodelist)
            {
                String columns = ((System.Xml.XmlElement)(items)).Attributes["attribute"].Value;
                AttributeMetadata meta = metadata.SingleOrDefault(x => x.LogicalName.Equals(columns));
                if (meta != null)
                {
                    if (meta.AttributeType.Value == AttributeTypeCode.Lookup)
                    {
                        filterexp.Conditions.Add(new ConditionExpression(meta.LogicalName + "name", ConditionOperator.Like, "%" + SearchValue + "%"));
                    }
                    else if (meta.AttributeType.Value == AttributeTypeCode.Customer)
                    {
                        filterexp.Conditions.Add(new ConditionExpression(meta.LogicalName + "name", ConditionOperator.Like, "%" + SearchValue + "%"));
                    }
                    else if (meta.AttributeType.Value == AttributeTypeCode.Lookup)
                    {
                        filterexp.Conditions.Add(new ConditionExpression(meta.LogicalName + "name", ConditionOperator.Like, "%" + SearchValue + "%"));
                    }
                    else
                    {
                        filterexp.Conditions.Add(new ConditionExpression(meta.LogicalName, ConditionOperator.Like, "%" + SearchValue + "%"));
                    }
                }
            }
            exp.Criteria.AddFilter(filterexp);
        }

        private void AddFilterToGrid(IOrganizationService service, String PortalId, String ConfigurationId, WidgetProperties Widget, ref QueryExpression exp, List<Filters> filters, String PortalEntity, String PortalEntityUserField, String PortalEntityUserValue)
        {
            String FilterType = String.Empty;
            FilterExpression filterexp = new FilterExpression();

            foreach (var item in filters)
            {
                #region Iterate Filters
                if (item.type != FilterType)
                {
                    filterexp = new FilterExpression();
                    if (item.type == "and")
                        filterexp.FilterOperator = LogicalOperator.And;
                    else if (item.type == "or")
                        filterexp.FilterOperator = LogicalOperator.Or;
                }
                foreach (var filter in item.filter)
                {
                    if (filter.isstatic == "0")
                    {
                        #region Not Static Filters
                        if (filter.condition.ToLower() == "equalscurrentuser")
                        {
                            EntityMetadata EntityMetadata = GetAndCheckMetaDataFromCache(service, ConfigurationId, PortalEntity);

                            OneToManyRelationshipMetadata[] onetomany = EntityMetadata.OneToManyRelationships;

                            OneToManyRelationshipMetadata[] specificentity = onetomany.Where(p => p.ReferencingEntity.Equals(Widget.EntityLogicalName)).ToArray();

                            if (specificentity.Length > 0)
                            {
                                QueryByAttribute q = new QueryByAttribute();
                                q.EntityName = PortalEntity;
                                q.ColumnSet = new ColumnSet(true);
                                q.AddAttributeValue(PortalEntityUserField, PortalEntityUserValue);

                                EntityCollection collect = service.RetrieveMultiple(q);

                                filterexp.Conditions.Add(new ConditionExpression(specificentity[0].ReferencingAttribute, ConditionOperator.Equal, collect[0].Id));

                            }

                        }
                        #endregion
                    }
                    else if (filter.isstatic == "1")
                    {
                        #region Static Filters

                        ConditionOperator op = ConditionOperator.Equal;
                        if (filter.Operator == "eq")
                        {
                            op = ConditionOperator.Equal;
                        }
                        else if (filter.Operator == "like")
                        {
                            op = ConditionOperator.Contains;
                        }

                        filterexp.Conditions.Add(new ConditionExpression(filter.attributtename, op, filter.value));

                        #endregion
                    }

                }
                //ilk seferinde her zaman eklesin!
                if (FilterType == String.Empty)
                {
                    exp.Criteria.AddFilter(filterexp);
                    FilterType = item.type;
                }

                else if (FilterType != item.type)
                {
                    exp.Criteria.AddFilter(filterexp);
                }
                #endregion
            }
        }

        private void ChangeFetchXmlDynamicValuesForLookup(IOrganizationService service, ref String FetchXml, String PortalEntity, String PortalEntityUserField, String PortalEntityUserValue)
        {
            XElement form = XElement.Parse(FetchXml);

            var portalfieldfilters = (from el in form.Descendants("condition")
                                      where el.Attribute("value") != null && el.Attribute("value").ToString().Contains("@portaluser.")
                                      select el).ToList();
            if (portalfieldfilters.Count > 0)
            {
                QueryByAttribute q = new QueryByAttribute();
                q.EntityName = PortalEntity;
                q.ColumnSet = new ColumnSet(true);
                q.AddAttributeValue(PortalEntityUserField, PortalEntityUserValue);

                EntityCollection collect = service.RetrieveMultiple(q);

                Entity Portal = collect.Entities.SingleOrDefault();

                foreach (var item in portalfieldfilters)
                {
                    String Value = String.Empty, Attr = String.Empty;
                    Attr = item.Attribute("value").Value.Split('.')[1];

                    if (Portal.Attributes.Contains(Attr))
                    {
                        if (Portal[Attr] is EntityReference)
                        {
                            Value = ((EntityReference)Portal[Attr]).Id.ToString();
                        }
                        else if (Portal[Attr] is OptionSetValue)
                        {
                            Value = ((OptionSetValue)Portal[Attr]).Value.ToString();
                        }
                        else if (Portal[Attr] is Money)
                        {
                            Value = ((Money)Portal[Attr]).Value.ToString();
                        }
                        else
                        {
                            Value = Portal[Attr].ToString();
                        }
                        item.Attribute("value").Value = Value;
                        item.ReplaceWith(item);
                    }
                    else
                    {
                        item.Attribute("operator").Value = "null";
                        item.Attribute("value").Remove();
                        item.ReplaceWith(item);
                    }
                }
            }
            FetchXml = form.ToString();
        }

        private List<GridRowData> GetGridData(IOrganizationService service, CrmServiceInformation serviceinfo, String PortalId, String ViewId,
                                                WidgetProperties Widget, List<Filters> filters, String PortalEntity, String PortalEntityUserField,
                                                       String PortalEntityUserValue, String RecordCount, String Page, String SearchValue, String ActionsButtonCount, String PercentageofTotalWidth)
        {
            List<GridRowData> returndata = new List<GridRowData>();

            QueryExpression savedquery = new QueryExpression();
            savedquery.EntityName = "savedquery";
            savedquery.ColumnSet = new ColumnSet(true);

            ConditionExpression condt = new ConditionExpression();
            condt.AttributeName = "savedqueryid";
            condt.Operator = ConditionOperator.Equal;
            condt.Values.Add(new Guid(ViewId));

            savedquery.Criteria.AddCondition(condt);
            EntityCollection savedquerycollect = service.RetrieveMultiple(savedquery);

            if (savedquerycollect.Entities.Count > 0)
            {
                FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest
                {
                    FetchXml = Convert.ToString(savedquerycollect.Entities[0]["fetchxml"])

                };
                var response = (FetchXmlToQueryExpressionResponse)service.Execute(req);

                QueryExpression exp = response.Query;

                int p = 1;
                String EntityName = Convert.ToString(savedquerycollect.Entities[0]["returnedtypecode"]);
                EntityMetadata metadata = GetAndCheckMetaDataFromCache(service, serviceinfo.ConfigurationId, EntityName);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Convert.ToString(savedquerycollect.Entities[0]["layoutxml"]));
                XmlNodeList nodelist = doc.SelectNodes("//cell");

                if (filters.Count > 0)
                    AddFilterToGrid(service, PortalId, serviceinfo.ConfigurationId, Widget, ref exp, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                if (SearchValue != String.Empty)
                {
                    QueryExpression savedqueryquickfind = new QueryExpression();
                    savedqueryquickfind.EntityName = "savedquery";
                    savedqueryquickfind.ColumnSet = new ColumnSet(true);
                    savedqueryquickfind.Criteria.AddCondition(new ConditionExpression("returnedtypecode", ConditionOperator.Equal, metadata.ObjectTypeCode.Value));
                    savedqueryquickfind.Criteria.AddCondition(new ConditionExpression("querytype", ConditionOperator.Equal, 4));

                    EntityCollection quickfindcollect = service.RetrieveMultiple(savedqueryquickfind);
                    XmlDocument quicfinddoc = new XmlDocument();
                    quicfinddoc.LoadXml(Convert.ToString(quickfindcollect.Entities[0]["fetchxml"]));
                    XmlNodeList quickfindnodelist = quicfinddoc.SelectNodes("//filter[@isquickfindfields='1']//condition");

                    AddSearchFiltersToGrid(service, ref exp, quickfindnodelist, metadata.Attributes, SearchValue);
                }

                exp.PageInfo.Count = Convert.ToInt32(RecordCount) + 1;

                if (Page != String.Empty)
                    exp.PageInfo.PageNumber = Convert.ToInt32(Page);

                EntityCollection collect = service.RetrieveMultiple(exp);
                if (collect.Entities.Count == 0)
                {
                    #region If there is no value in grid

                    List<GridData> ldata = new List<GridData>();
                    foreach (var items in nodelist)
                    {
                        String columns = ((System.Xml.XmlElement)(items)).Attributes["name"].Value;
                        AttributeMetadata meta = metadata.Attributes.SingleOrDefault(x => x.LogicalName.Equals(columns));
                        GridData data = new GridData();
                        data.ColumnName = columns;
                        if (meta != null)
                            data.DisplayName = meta.DisplayName.UserLocalizedLabel.Label;
                        else
                        {
                            //link-entity information
                            XElement form = XElement.Parse(Convert.ToString(savedquerycollect.Entities[0]["fetchxml"]));
                            EntityMetadata linkmetadata = GetLinkedEntityMetaData(service, PortalId, serviceinfo.ConfigurationId, form, (XmlNode)items);
                            data.DisplayName = linkmetadata.Attributes.Where(z => z.LogicalName.Equals(columns.Contains('.') ? columns.Split('.')[1] : columns)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;

                        }
                        ldata.Add(data);

                    }
                    GridRowData gridrowdata = new GridRowData();
                    gridrowdata.Data = ldata;
                    gridrowdata.IsEmptyGrid = "1";
                    gridrowdata.RowNumber = "0";
                    returndata.Add(gridrowdata);
                    #endregion
                }
                else
                {
                    foreach (var item in collect.Entities)
                    {
                        PrepareGridData(serviceinfo, service, PortalId, item, nodelist, metadata.Attributes, Convert.ToString(savedquerycollect.Entities[0]["fetchxml"]), p, ref returndata, metadata.PrimaryNameAttribute, ActionsButtonCount, PercentageofTotalWidth, String.Empty);
                    }
                }

            }

            return returndata;
        }


        private List<PictureData> GetPictureData(IOrganizationService service, CrmServiceInformation serviceinfo, String PortalId, String ViewId,
                                                WidgetProperties Widget, String PortalEntity, String PortalEntityUserField,
                                                       String PortalEntityUserValue, String PictureUrlAttribute, String CustomFilterXml, String pictureHeight)
        {

            List<PictureData> returndata = new List<PictureData>();
            EntityCollection collect = new EntityCollection();

            if (string.IsNullOrEmpty(CustomFilterXml))
            {
                QueryExpression savedquery = new QueryExpression();
                savedquery.EntityName = "savedquery";
                savedquery.ColumnSet = new ColumnSet(true);

                ConditionExpression condt = new ConditionExpression();
                condt.AttributeName = "savedqueryid";
                condt.Operator = ConditionOperator.Equal;
                condt.Values.Add(new Guid(ViewId));

                savedquery.Criteria.AddCondition(condt);
                EntityCollection savedquerycollect = service.RetrieveMultiple(savedquery);

                if (savedquerycollect.Entities.Count > 0)
                {
                    FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest
                    {
                        FetchXml = Convert.ToString(savedquerycollect.Entities[0]["fetchxml"])
                    };

                    var response = (FetchXmlToQueryExpressionResponse)service.Execute(req);

                    QueryExpression exp = response.Query;
                    exp.ColumnSet.AddColumn(PictureUrlAttribute);

                    collect = service.RetrieveMultiple(exp);
                }

            }
            else
            {
                XElement xml = XElement.Parse(CustomFilterXml);
                AddCustomFiltersToFetchXml(service, ref xml, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest
                {
                    FetchXml = xml.ToString()
                };

                var response = (FetchXmlToQueryExpressionResponse)service.Execute(req);

                QueryExpression exp = response.Query;
                exp.ColumnSet.AddColumn(PictureUrlAttribute);

                collect = service.RetrieveMultiple(exp);
            }

            if (collect.Entities.Count > 0)
            {
                foreach (var entity in collect.Entities)
                {
                    PictureData pd = new PictureData();
                    pd.PictureList = new List<PictureInfo>();

                    QueryExpression expression = new QueryExpression();
                    expression.EntityName = "annotation";
                    expression.ColumnSet = new ColumnSet(true);

                    FilterExpression fGeneral = new FilterExpression();

                    FilterExpression fAnd = new FilterExpression();
                    fAnd.FilterOperator = LogicalOperator.And;
                    fAnd.AddCondition(new ConditionExpression("objectid", ConditionOperator.Equal, entity.Id));

                    FilterExpression fOr = new FilterExpression();
                    fOr.FilterOperator = LogicalOperator.Or;
                    fOr.AddCondition(new ConditionExpression("mimetype", ConditionOperator.Like, "%jpeg%"));
                    fOr.AddCondition(new ConditionExpression("mimetype", ConditionOperator.Like, "%jpg%"));
                    fOr.AddCondition(new ConditionExpression("mimetype", ConditionOperator.Like, "%png%"));
                    fOr.AddCondition(new ConditionExpression("mimetype", ConditionOperator.Like, "%gif%"));

                    fGeneral.AddFilter(fAnd);
                    fGeneral.AddFilter(fOr);
                    expression.Criteria = fGeneral;

                    EntityCollection AttachmentCollect = service.RetrieveMultiple(expression);

                    foreach (var attachmentitem in AttachmentCollect.Entities)
                    {
                        PictureInfo pi = new PictureInfo
                        {
                            Name = attachmentitem.Attributes.Contains("filename") ? Convert.ToString(attachmentitem["filename"]) : String.Empty,
                            PictureBase64List = attachmentitem.Attributes.Contains("documentbody") ? Convert.ToString(attachmentitem["documentbody"]) : String.Empty,
                            MimeType = attachmentitem["mimetype"].ToString()
                        };

                        pd.PictureList.Add(pi);
                        pd.PictureUrlAddress = entity.Attributes.ContainsKey(PictureUrlAttribute) ? entity.Attributes[PictureUrlAttribute].ToString() : string.Empty;
                        pd.PictureHeight = pictureHeight;
                    }

                    if (AttachmentCollect.Entities.Count > 0)
                    {
                        returndata.Add(pd);
                    }


                }


            }



            return returndata;
        }

        private FieldInfo GetFieldWidgetData(IOrganizationService service, CrmServiceInformation serviceinfo, String PortalId, String ViewId, String PortalEntity, String PortalEntityUserField, String PortalEntityUserValue,
                                        String entityField1, String entityField2, String entityField3, String CustomFilterXml, EntityMetadata metadata)
        {

            FieldInfo returndata = new FieldInfo();
            EntityCollection collect = new EntityCollection();

            if (string.IsNullOrEmpty(CustomFilterXml))
            {
                QueryExpression savedquery = new QueryExpression();
                savedquery.EntityName = "savedquery";
                savedquery.ColumnSet = new ColumnSet(true);

                ConditionExpression condt = new ConditionExpression();
                condt.AttributeName = "savedqueryid";
                condt.Operator = ConditionOperator.Equal;
                condt.Values.Add(new Guid(ViewId));

                savedquery.Criteria.AddCondition(condt);
                EntityCollection savedquerycollect = service.RetrieveMultiple(savedquery);

                if (savedquerycollect.Entities.Count > 0)
                {
                    FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest
                    {
                        FetchXml = Convert.ToString(savedquerycollect.Entities[0]["fetchxml"])
                    };

                    var response = (FetchXmlToQueryExpressionResponse)service.Execute(req);

                    QueryExpression exp = response.Query;
                    exp.ColumnSet.AddColumn(entityField1);
                    exp.ColumnSet.AddColumn(entityField2);
                    exp.ColumnSet.AddColumn(entityField3);

                    collect = service.RetrieveMultiple(exp);
                }

            }
            else
            {
                XElement form = XElement.Parse(CustomFilterXml);
                AddCustomFiltersToFetchXml(service, ref form, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest
                {
                    FetchXml = form.ToString()
                };

                var response = (FetchXmlToQueryExpressionResponse)service.Execute(req);

                QueryExpression exp = response.Query;
                exp.ColumnSet.AddColumn(entityField1);
                exp.ColumnSet.AddColumn(entityField2);
                exp.ColumnSet.AddColumn(entityField3);

                collect = service.RetrieveMultiple(exp);
            }

            if (collect.Entities != null && collect.Entities.Any())
            {
                Entity lastEntity = collect.Entities.LastOrDefault();

                var fieldAttributeMetadata1 = metadata.Attributes.Where(z => z.LogicalName.Equals(entityField1)).FirstOrDefault();
                returndata.Field1Text = metadata.Attributes.Where(p => p.LogicalName.Equals(entityField1)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;

                if (lastEntity.Attributes.ContainsKey(entityField1))
                {
                    var data = GetFieldValue(service, PortalId, fieldAttributeMetadata1.EntityLogicalName, fieldAttributeMetadata1.LogicalName, serviceinfo.ConfigurationId, lastEntity.Attributes[entityField1], fieldAttributeMetadata1, serviceinfo);
                    returndata.Field1Value = data.Item1;
                }
                else
                {
                    returndata.Field1Value = String.Empty;
                }

                var fieldAttributeMetadata2 = metadata.Attributes.Where(z => z.LogicalName.Equals(entityField2)).FirstOrDefault();
                returndata.Field2Text = metadata.Attributes.Where(p => p.LogicalName.Equals(entityField2)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;

                if (lastEntity.Attributes.ContainsKey(entityField2))
                {
                    var data = GetFieldValue(service, PortalId, fieldAttributeMetadata2.EntityLogicalName, fieldAttributeMetadata2.LogicalName, serviceinfo.ConfigurationId, lastEntity.Attributes[entityField2], fieldAttributeMetadata2, serviceinfo);
                    returndata.Field2Value = data.Item1;
                }
                else
                {
                    returndata.Field2Value = String.Empty;
                }

                var fieldAttributeMetadata3 = metadata.Attributes.Where(z => z.LogicalName.Equals(entityField3)).FirstOrDefault();
                returndata.Field3Text = metadata.Attributes.Where(p => p.LogicalName.Equals(entityField3)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;

                if (lastEntity.Attributes.ContainsKey(entityField3))
                {
                    var data = GetFieldValue(service, PortalId, fieldAttributeMetadata3.EntityLogicalName, fieldAttributeMetadata3.LogicalName, serviceinfo.ConfigurationId, lastEntity.Attributes[entityField3], fieldAttributeMetadata3, serviceinfo);
                    returndata.Field3Value = data.Item1;
                }
                else
                {
                    returndata.Field3Value = String.Empty;
                }


            }

            return returndata;
        }

        private List<String> GetNotificationWidgetData(IOrganizationService service, CrmServiceInformation serviceinfo, String PortalId, String ViewId,
                                        String PortalEntity, String PortalEntityUserField, String PortalEntityUserValue, String entityField, String CustomFilterXml)
        {

            List<String> returndata = new List<String>();
            EntityCollection collect = new EntityCollection();

            if (string.IsNullOrEmpty(CustomFilterXml))
            {
                QueryExpression savedquery = new QueryExpression();
                savedquery.EntityName = "savedquery";
                savedquery.ColumnSet = new ColumnSet(true);

                ConditionExpression condt = new ConditionExpression();
                condt.AttributeName = "savedqueryid";
                condt.Operator = ConditionOperator.Equal;
                condt.Values.Add(new Guid(ViewId));

                savedquery.Criteria.AddCondition(condt);
                EntityCollection savedquerycollect = service.RetrieveMultiple(savedquery);

                if (savedquerycollect.Entities.Count > 0)
                {
                    FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest
                    {
                        FetchXml = Convert.ToString(savedquerycollect.Entities[0]["fetchxml"])
                    };

                    var response = (FetchXmlToQueryExpressionResponse)service.Execute(req);

                    QueryExpression exp = response.Query;
                    exp.ColumnSet.AddColumn(entityField);

                    collect = service.RetrieveMultiple(exp);
                }

            }
            else
            {

                XElement xml = XElement.Parse(CustomFilterXml);
                AddCustomFiltersToFetchXml(service, ref xml, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest
                {
                    FetchXml = xml.ToString()
                };

                var response = (FetchXmlToQueryExpressionResponse)service.Execute(req);

                QueryExpression exp = response.Query;
                exp.ColumnSet.AddColumn(entityField);

                collect = service.RetrieveMultiple(exp);
            }

            if (collect.Entities != null && collect.Entities.Any())
            {
                returndata = collect.Entities.Where(z => z.Attributes.ContainsKey(entityField)).Select(z => z.Attributes[entityField].ToString()).ToList();
            }

            return returndata;
        }

        private List<GridRowData> GetGridDataCustom(IOrganizationService service, CrmServiceInformation serviceinfo, String PortalId, String FetchXml, WidgetProperties Widget, List<Filters> filters, String PortalEntity, String PortalEntityUserField, String PortalEntityUserValue, String RecordCount, String Page, String SearchValue, String ActionsCount, String PercentageofTotalWidth)
        {

            List<String> vals = new List<String>();
            List<GridRowData> returndata = new List<GridRowData>();
            try
            {
                XElement form = XElement.Parse(FetchXml);

                AddCustomFiltersToFetchXml(service, ref form, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest
                {
                    FetchXml = Convert.ToString(form.ToString())

                };
                var response = (FetchXmlToQueryExpressionResponse)service.Execute(req);

                QueryExpression exp = response.Query;

                String EntityName = form.Element("entity").Attribute("name").Value;
                EntityMetadata metadata = GetAndCheckMetaDataFromCache(service, serviceinfo.ConfigurationId, EntityName);

                int p = 0;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(FetchXml);
                XmlNodeList nodelist = doc.SelectNodes("//attribute");

                if (filters.Count > 0)
                    AddFilterToGrid(service, PortalId, serviceinfo.ConfigurationId, Widget, ref exp, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                if (SearchValue != String.Empty)
                {
                    QueryExpression savedqueryquickfind = new QueryExpression();

                    savedqueryquickfind.EntityName = "savedquery";
                    savedqueryquickfind.ColumnSet = new ColumnSet(true);
                    savedqueryquickfind.Criteria.AddCondition(new ConditionExpression("returnedtypecode", ConditionOperator.Equal, metadata.ObjectTypeCode.Value));
                    savedqueryquickfind.Criteria.AddCondition(new ConditionExpression("querytype", ConditionOperator.Equal, 4));

                    EntityCollection quickfindcollect = service.RetrieveMultiple(savedqueryquickfind);
                    XmlDocument quicfinddoc = new XmlDocument();
                    quicfinddoc.LoadXml(Convert.ToString(quickfindcollect.Entities[0]["fetchxml"]));
                    XmlNodeList quickfindnodelist = quicfinddoc.SelectNodes("//filter[@isquickfindfields='1']//condition");

                    AddSearchFiltersToGrid(service, ref exp, quickfindnodelist, metadata.Attributes, SearchValue);

                }
                exp.PageInfo.Count = Convert.ToInt32(RecordCount) + 1;
                if (Page != String.Empty)
                    exp.PageInfo.PageNumber = Convert.ToInt32(Page);

                exp.ColumnSet = new ColumnSet(true);
                EntityCollection GridEntities = service.RetrieveMultiple(exp);

                if (GridEntities.Entities.Count == 0)
                {
                    #region If there is no value in grid

                    List<GridData> ldata = new List<GridData>();
                    foreach (var items in nodelist)
                    {

                        String columns = ((System.Xml.XmlElement)(items)).Attributes["name"].Value;
                        AttributeMetadata meta = metadata.Attributes.FirstOrDefault(x => x.LogicalName.Equals(columns));
                        GridData data = new GridData();
                        data.ColumnName = columns;
                        if (meta != null)
                            data.DisplayName = meta.DisplayName.UserLocalizedLabel.Label;
                        else
                        {
                            //link-entity information                        
                            EntityMetadata linkmetadata = GetLinkedEntityMetaData(service, PortalId, serviceinfo.ConfigurationId, form, (XmlNode)items);
                            data.DisplayName = linkmetadata.Attributes.Where(z => z.LogicalName.Equals(columns.Contains('.') ? columns.Split('.')[1] : columns)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;

                        }
                        ldata.Add(data);

                    }
                    GridRowData gridrowdata = new GridRowData();
                    gridrowdata.Data = ldata;
                    gridrowdata.IsEmptyGrid = "1";
                    gridrowdata.RowNumber = "0";
                    returndata.Add(gridrowdata);
                    #endregion
                }
                foreach (var item in GridEntities.Entities)
                {
                    PrepareGridData(serviceinfo, service, PortalId, item, nodelist, metadata.Attributes, form.ToString(), p, ref returndata, metadata.PrimaryNameAttribute, ActionsCount, PercentageofTotalWidth, String.Empty);
                    p++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return returndata;
        }

        private void AddCustomFiltersToFetchXml(IOrganizationService service, ref XElement form, String PortalEntity, String PortalEntityUserField, String PortalEntityUserValue)
        {

            var portalfieldfilters = (from el in form.Descendants("condition")
                                      where el.Attribute("value") != null && el.Attribute("value").ToString().Contains("@portaluser.")
                                      select el).ToList();

            var portalfields = (from el in form.Descendants("condition")
                                where el.Attribute("value") != null && el.Attribute("value").Value.ToString().Equals("@portaluser")
                                select el).ToList();

            if (portalfieldfilters.Count > 0 || portalfields.Count > 0)
            {
                QueryByAttribute q = new QueryByAttribute();
                q.EntityName = PortalEntity;
                q.ColumnSet = new ColumnSet(true);
                q.AddAttributeValue(PortalEntityUserField, PortalEntityUserValue);

                EntityCollection collect = service.RetrieveMultiple(q);

                Entity Portal = collect.Entities.FirstOrDefault();

                foreach (var item in portalfields)
                {
                    item.Attribute("value").Value = Portal.Id.ToString();
                    item.ReplaceWith(item);
                }
                foreach (var item in portalfieldfilters)
                {
                    String Value = String.Empty, Attr = String.Empty;
                    Attr = item.Attribute("value").Value.Split('.')[1];

                    if (Portal.Attributes.Contains(Attr))
                    {
                        if (Portal[Attr] is EntityReference)
                        {
                            Value = ((EntityReference)Portal[Attr]).Id.ToString();
                        }
                        else if (Portal[Attr] is OptionSetValue)
                        {
                            Value = ((OptionSetValue)Portal[Attr]).Value.ToString();
                        }
                        else if (Portal[Attr] is Money)
                        {
                            Value = ((Money)Portal[Attr]).Value.ToString();
                        }
                        else
                        {
                            Value = Portal[Attr].ToString();
                        }
                        item.Attribute("value").Value = Value;
                        item.ReplaceWith(item);
                    }
                    else
                    {
                        item.Attribute("operator").Value = "null";
                        item.Attribute("value").Remove();
                        item.ReplaceWith(item);
                    }
                }
            }
        }

        private void ChangeQueryStrings(ref XElement form, WidgetParameters Parameters)
        {
            var QueryStrings = (from el in form.Descendants("condition")
                                where el.Attribute("value") != null && el.Attribute("value").ToString().Contains("@Querystring.")
                                select el).ToList();
            foreach (var item in QueryStrings)
            {
                var attributename = item.Attribute("attribute").Value;
                foreach (var querystrings in Parameters.QueryStrings)
                {
                    if (querystrings.Key == item.Attribute("value").Value.Split('.')[1])
                    {
                        item.Attribute("value").Value = querystrings.Value;
                        item.ReplaceWith(item);
                    }
                }
            }

        }

        private void PrepareGridData(CrmServiceInformation serviceinfo, IOrganizationService service, String PortalId, Entity item,
                                        XmlNodeList nodelist, AttributeMetadata[] metadata, String Xml, int p, ref List<GridRowData> returndata,
                                           String NameAttribute, String Actionscount, String PercentageofWidth, String TotalRecord = "")
        {
            #region İterate Retrieve Multiple

            GridRowData gridrowdata = new GridRowData();
            List<GridData> listgriddata = new List<GridData>();
            List<String> vals = new List<string>();
            XElement form = XElement.Parse(Xml);

            int totalwidth = 0;

            foreach (var items in nodelist)
            {

                if (((System.Xml.XmlElement)(items)).Attributes["name"].Value == item.LogicalName + "id")
                    continue;

                if (((System.Xml.XmlElement)(items)).Attributes["width"] != null)
                {
                    totalwidth += Convert.ToInt32(((System.Xml.XmlElement)(items)).Attributes["width"].Value.Replace("px", ""));
                }
                else
                {
                    totalwidth += 100;
                }

            }
            PercentageofWidth = String.IsNullOrEmpty(PercentageofWidth) == true ? "10" : PercentageofWidth;
            //get width for the buttons
            if (!String.IsNullOrEmpty(Actionscount))
                totalwidth = ((totalwidth * Convert.ToInt32(Actionscount) * Convert.ToInt32(PercentageofWidth)) / 100) + totalwidth;

            foreach (var items in nodelist)
            {

                if (((System.Xml.XmlElement)(items)).Attributes["name"].Value == item.LogicalName + "id")
                    continue;

                String columns = ((System.Xml.XmlElement)(items)).Attributes["name"].Value;
                AttributeMetadata meta = metadata.SingleOrDefault(x => x.LogicalName.Equals(columns));
                GridData data = new GridData();

                if (((System.Xml.XmlElement)(items)).Attributes["width"] != null)
                {
                    data.Width = Convert.ToString((Convert.ToInt32(((System.Xml.XmlElement)(items)).Attributes["width"].Value.Replace("px", "")) * 100) / totalwidth);
                }
                else
                {
                    data.Width = Convert.ToString((100 * 100) / totalwidth);
                }
                bool flag = true;
                if (item.Attributes.Contains(columns))
                {
                    #region CrmTypes
                    if (meta == null)
                    {
                        vals = CheckAndReturnCrmTypes(item[columns], ref flag, meta, service, columns, PortalId, serviceinfo);

                        data.Value = vals[0];
                    }
                    else if (meta.AttributeType.Value != AttributeTypeCode.Uniqueidentifier)
                    {

                        vals = CheckAndReturnCrmTypes(item[columns], ref flag, meta, service, columns, PortalId, serviceinfo);

                        data.Value = vals[0];
                    }
                    #endregion
                }
                else
                {
                    if ((((System.Xml.XmlElement)(items)).ParentNode).Name != "link-entity")
                    {
                        data.Value = String.Empty;
                    }

                }
                if (flag == true)
                {
                    if (item.Attributes.Contains(NameAttribute))
                        data.NameAttributeValue = item[NameAttribute].ToString();

                    data.RecordId = item.Id.ToString();
                    data.ColumnName = columns;

                    if (meta != null)
                    {
                        data.DisplayName = meta.DisplayName.UserLocalizedLabel.Label;
                    }
                    else
                    {
                        if (vals.Count > 1)
                            data.DisplayName = vals[1];
                        else
                        {
                            //buraya geldiyse linkentity'den geliyordur
                            GetLinkedEntityData(service, serviceinfo, PortalId, item, columns.Contains('.') ? columns.Split('.')[1] : columns, form, (XmlNode)items, flag, ref data);

                        }
                    }
                    listgriddata.Add(data);
                    flag = true;
                }
                flag = true;
            }
            gridrowdata.Data = listgriddata;
            gridrowdata.RowNumber = p.ToString();
            gridrowdata.TotalRecord = TotalRecord;
            if (!String.IsNullOrEmpty(Actionscount))
                gridrowdata.HasAction = "true";
            else
                gridrowdata.HasAction = "false";

            returndata.Add(gridrowdata);
            p = p + 1;
            #endregion
        }

        private List<String> CheckAndReturnCrmTypes(Object checkattribute, ref bool flag, AttributeMetadata meta, IOrganizationService service, String Columns, String PortalId, CrmServiceInformation serviceinfo)
        {
            List<String> Value = new List<String>();

            var v = GetFieldValue(service, PortalId, meta.EntityLogicalName, meta.LogicalName, serviceinfo.ConfigurationId, checkattribute, meta, serviceinfo);

            Value.Add(v.Item1);
            if (v.Item2 != String.Empty)
            {
                Value.Add(v.Item2);
            }

            return Value;
        }

        private List<Tabs> ParseFormXml(IOrganizationService service, String FormXml, String ObjectTypeCode, String EntityId, String PortalId, CrmServiceInformation serviceinfo, List<SubGridModel> SubGridModel = null, String FormID = null)
        {
            XElement form = XElement.Parse(FormXml);
            List<FormDataDetail> FormList = new List<FormDataDetail>();
            RetrieveUserSettingsSystemUserResponse usersettings = CreateUserSettings(service, PortalId, serviceinfo);
            var LanguageCode = usersettings.Entity["uilanguageid"].ToString();

            EntityMetadata EntityMetadata = GetAndCheckMetaDataFromCache(service, serviceinfo.ConfigurationId, ObjectTypeCode);

            Entity targetEntity = null;
            string processid = string.Empty, stageid = string.Empty, traversedpath = string.Empty;

            if (EntityId != null && EntityId != String.Empty)
            {
                targetEntity = service.Retrieve(EntityMetadata.LogicalName, new Guid(EntityId), new ColumnSet(true));

                if (targetEntity.Attributes.Contains("stageid"))
                {
                    stageid = targetEntity.Attributes["stageid"].ToString();
                }
                if (targetEntity.Attributes.Contains("processid"))
                {
                    processid = targetEntity.Attributes["processid"].ToString();
                }
                if (targetEntity.Attributes.Contains("traversedpath"))
                {
                    traversedpath = targetEntity.Attributes["traversedpath"].ToString();
                }
            }


            var tabs = (from elem in form.Descendants("tabs")
                               .Descendants("tab")
                        select elem).ToList();

            List<Tabs> tablist = new List<Tabs>();

            //Get business process flow by entity name
            List<BusinessProcessFlow> businessProcessFlowList = GetBusinessProcessFlowListByEntityName(serviceinfo, PortalId, ObjectTypeCode, EntityMetadata, usersettings, stageid, processid, traversedpath);

            if (businessProcessFlowList != null && businessProcessFlowList.Any())
            {
                Tabs tab = new Tabs();
                tab.BusinessProcessFlowList = businessProcessFlowList;
                tab.IsBpf = true;
                tab.Visible = "true";
                tab.EntityName = EntityMetadata.LogicalName;
                tab.Columns = new List<Columns>();
                tab.FormData = new List<FormDataDetail>();
                tablist.Add(tab);
            }

            // Get Business rules by entity name
            BusinessRuleContainer businessRules = GetBusinessRules(serviceinfo, PortalId, ObjectTypeCode, usersettings, FormID);
            if (businessRules != null && businessRules.BusinessRules.Any())
            {
                Tabs tab = new Tabs();
                tab.BusinessRules = businessRules;
                tab.IsBr = true;
                tab.Visible = "true";
                tab.EntityName = EntityMetadata.LogicalName;
                tab.Columns = new List<Columns>();
                tab.FormData = new List<FormDataDetail>();
                tablist.Add(tab);
            }


            foreach (var item in tabs)
            {
                //Dont get the unvisible tabs.
                if (item.Attribute("visible") != null && item.Attribute("visible").Value == "false")
                    continue;

                Tabs t = new Tabs();
                t.EntityName = EntityMetadata.LogicalName;
                t.Expanded = item.Attribute("expanded") == null ? "true" : item.Attribute("expanded").Value;
                t.Visible = item.Attribute("visible") == null ? "true" : item.Attribute("visible").Value;
                t.Name = item.Attribute("name") == null ? "tabname" : item.Attribute("name").Value;
                t.Label = item.Elements("labels").Elements("label").FirstOrDefault().Attribute("description").Value;
                t.ShowLabel = item.Attribute("showlabel") == null ? "true" : item.Attribute("showlabel").Value;
                var columns = item.Descendants("column").ToList();

                List<Columns> listColumns = new List<Columns>();
                foreach (var citem in columns)
                {
                    Columns c = new Columns();
                    c.Width = citem.Attribute("width").Value;

                    var sections = citem.Descendants("section").ToList();

                    List<Sections> listsections = new List<Sections>();

                    foreach (var sitem in sections)
                    {
                        Sections s = new Sections();
                        s.ShowLabel = sitem.Attribute("showlabel").Value;
                        s.Visible = sitem.Attribute("visible") == null ? "true" : sitem.Attribute("visible").Value;
                        s.ColumnLength = sitem.Attribute("columns") == null ? "1" : sitem.Attribute("columns").Value;
                        s.Name = sitem.Descendants("label").ToList()[0].Attribute("description").Value;
                        List<Rows> listrows = new List<Rows>();
                        var rows = sitem.Descendants("row").Descendants("cell").Where(p => p.Attribute("id") != null).ToList();
                        foreach (var ritem in rows)
                        {
                            Rows r = new Rows();
                            r.Visible = ritem.Attribute("visible") == null ? "true" : ritem.Attribute("visible").Value;
                            FormDataDetail data = new FormDataDetail();
                            if (ritem.Descendants("control").Descendants("parameters").Descendants("ViewId").FirstOrDefault() == null && (ritem.Descendants("control").Descendants("parameters").Descendants("QuickForms").FirstOrDefault() == null))
                            {

                                r.ElementType = "formcontrol";
                                if (ritem.Descendants("control").FirstOrDefault() != null)
                                {
                                    String logical = ritem.Descendants("control").FirstOrDefault().Attribute("id").Value;
                                    var attribute = EntityMetadata.Attributes.Where(p => p.LogicalName.Equals(logical)).FirstOrDefault();
                                    //new fields add into form , we need to renew the cache
                                    if (attribute == null && logical != "notescontrol")
                                    {
                                        if (logical != "ProductSuggestions_LinkControl" && logical != "DynamicPropertiesList_LinkControl")
                                        {
                                            EntityMetadata = GetAndCheckMetaDataFromCache(service, serviceinfo.ConfigurationId, ObjectTypeCode, "true");
                                            attribute = EntityMetadata.Attributes.Where(p => p.LogicalName.Equals(logical)).FirstOrDefault();
                                        }

                                    }
                                    if (attribute != null || logical == "notescontrol")
                                    {

                                        if (ritem.Descendants("label").ToList().Count > 1)
                                        {
                                            r.DisplayName = ritem.Descendants("label").ToList().Where(p => p.Attribute("languagecode").Value.Equals(LanguageCode)).ToList().Count > 0
                                            ? ritem.Descendants("label").ToList().Where(p => p.Attribute("languagecode").Value.Equals(LanguageCode)).ToList().FirstOrDefault().Attribute("description").Value
                                            : ritem.Descendants("label").ToList().FirstOrDefault().Attribute("description").Value;
                                        }
                                        else
                                        {
                                            r.DisplayName = ritem.Descendants("label").ToList().FirstOrDefault().Attribute("description").Value;
                                        }

                                        r.Type = logical != "notescontrol" ? attribute.AttributeType.Value.ToString().ToLower() : "notescontrol";
                                        r.RequiredLevel = attribute != null ? attribute.RequiredLevel.Value.ToString() : "None";
                                        r.ClassId = ritem.Descendants("control").SingleOrDefault().Attribute("classid") == null ? String.Empty : ritem.Descendants("control").FirstOrDefault().Attribute("classid").Value;

                                        #region Crm Fields Controls
                                        if (r.Type == "lookup")
                                        {
                                            r.LookupLogicalName = ((Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata)(attribute)).Targets[0];

                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = ((EntityReference)targetEntity[logical]).Id.ToString();
                                                    data.LookUpValueName = ((EntityReference)targetEntity[logical]).Name.ToString();
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                    data.LookUpValueName = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }
                                        else if (r.Type == "notescontrol")
                                        {
                                            if (targetEntity != null)
                                            {
                                                QueryExpression expression = new QueryExpression();
                                                expression.EntityName = "annotation";
                                                expression.ColumnSet = new ColumnSet(true);
                                                expression.Criteria.AddCondition(new ConditionExpression("objectid", ConditionOperator.Equal, targetEntity.Id));

                                                EntityCollection AttachmentCollect = service.RetrieveMultiple(expression);

                                                List<ClassLibrary.Attachment> AttachmentList = new List<ClassLibrary.Attachment>();
                                                foreach (var attachmentitem in AttachmentCollect.Entities)
                                                {
                                                    ClassLibrary.Attachment a = new ClassLibrary.Attachment();
                                                    a.AttachmentId = attachmentitem.Id.ToString();
                                                    a.RecordId = ((EntityReference)attachmentitem["objectid"]).Id.ToString();
                                                    a.MimeType = attachmentitem.Attributes.Contains("mimetype") ? Convert.ToString(attachmentitem["mimetype"]) : String.Empty;
                                                    a.DocumentBody = attachmentitem.Attributes.Contains("documentbody") ? Convert.ToString(attachmentitem["documentbody"]) : String.Empty;
                                                    a.FileName = attachmentitem.Attributes.Contains("filename") ? Convert.ToString(attachmentitem["filename"]) : String.Empty;
                                                    a.Subject = attachmentitem.Attributes.Contains("notetext") ? Convert.ToString(attachmentitem["notetext"]) : String.Empty;
                                                    AttachmentList.Add(a);
                                                }
                                                r.Attachments = AttachmentList;
                                            }
                                        }
                                        else if (r.Type == "customer")
                                        {
                                            List<String> targets = new List<String>();
                                            targets.Add(((Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata)(attribute)).Targets[0]);
                                            targets.Add(((Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata)(attribute)).Targets[1]);
                                            r.CustomerLogicalName = targets;

                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = ((EntityReference)targetEntity[logical]).Id.ToString();
                                                    data.LookUpValueName = ((EntityReference)targetEntity[logical]).Name.ToString();
                                                    data.LookupLogicalName = ((EntityReference)targetEntity[logical]).LogicalName.ToString();
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                    data.LookUpValueName = String.Empty;
                                                    data.LookupLogicalName = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }
                                        else if (r.Type == "string")
                                        {
                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = targetEntity[logical].ToString();
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }

                                        else if (r.Type == "memo")
                                        {
                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = targetEntity[logical].ToString();
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }

                                        else if (r.Type == "ınteger" || r.Type == "integer")
                                        {
                                            r.MaxValue = ((Microsoft.Xrm.Sdk.Metadata.IntegerAttributeMetadata)(attribute)).MaxValue.ToString();
                                            r.MinValue = ((Microsoft.Xrm.Sdk.Metadata.IntegerAttributeMetadata)(attribute)).MinValue.ToString();

                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = targetEntity[logical].ToString();
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }
                                        else if (r.Type == "money")
                                        {
                                            r.MaxValue = ((Microsoft.Xrm.Sdk.Metadata.MoneyAttributeMetadata)(attribute)).MaxValue.ToString();
                                            r.MinValue = ((Microsoft.Xrm.Sdk.Metadata.MoneyAttributeMetadata)(attribute)).MinValue.ToString();
                                            r.Precision = ((Microsoft.Xrm.Sdk.Metadata.MoneyAttributeMetadata)(attribute)).Precision.ToString();

                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = ((Money)targetEntity[logical]).Value.ToString();
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }
                                        else if (r.Type == "double")
                                        {
                                            r.MaxValue = ((Microsoft.Xrm.Sdk.Metadata.DoubleAttributeMetadata)(attribute)).MaxValue.ToString();
                                            r.MinValue = ((Microsoft.Xrm.Sdk.Metadata.DoubleAttributeMetadata)(attribute)).MinValue.ToString();
                                            r.Precision = ((Microsoft.Xrm.Sdk.Metadata.DoubleAttributeMetadata)(attribute)).Precision.ToString();

                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = targetEntity[logical].ToString();
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }
                                        else if (r.Type == "decimal")
                                        {
                                            r.Precision = ((Microsoft.Xrm.Sdk.Metadata.DecimalAttributeMetadata)(attribute)).Precision.ToString();

                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = targetEntity[logical].ToString();
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }
                                        else if (r.Type == "datetime")
                                        {

                                            r.DatePart = ((Microsoft.Xrm.Sdk.Metadata.DateTimeAttributeMetadata)(attribute)).Format.Value.ToString().ToLower();
                                            if (usersettings != null)
                                            {
                                                r.BeforeDateFormat = usersettings.Entity["dateformatstring"].ToString();
                                                r.BeforeTimeFormat = usersettings.Entity["timeformatstring"].ToString();

                                                r.DateFormat = usersettings.Entity["dateformatstring"].ToString().Replace("M", "m").Replace("MM", "m").Replace("mm", "m").Replace("dd", "d").Replace("yyyy", "Y");
                                                //convert crm time format to datetime picker format
                                                if (usersettings.Entity["timeformatstring"].ToString().Contains("tt"))//means has am
                                                {
                                                    r.TimeFormat = "h:i a";
                                                }
                                                else
                                                {
                                                    r.TimeFormat = "H:i";
                                                }
                                            }

                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = FormatDates(service, usersettings, targetEntity[logical].ToString(), r.DatePart);
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }
                                        else if (r.Type == "boolean")
                                        {
                                            List<Picklist> picklist = new List<Picklist>();
                                            Picklist p = new Picklist();
                                            p.Label = ((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute)).OptionSet.FalseOption.Label.UserLocalizedLabel.Label;
                                            p.Value = ((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute)).OptionSet.FalseOption.Value.ToString();
                                            p.DefaultValue = (((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute))).DefaultValue.Value.ToString();
                                            picklist.Add(p);

                                            p = new Picklist();
                                            p.Label = ((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute)).OptionSet.TrueOption.Label.UserLocalizedLabel.Label;
                                            p.Value = ((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute)).OptionSet.TrueOption.Value.ToString();
                                            p.DefaultValue = (((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute))).DefaultValue.Value.ToString();
                                            picklist.Add(p);
                                            r.PicklistValues = picklist;

                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = targetEntity[logical].ToString();
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }
                                        else if (r.Type == "picklist")
                                        {
                                            var opt = ((Microsoft.Xrm.Sdk.Metadata.EnumAttributeMetadata)(((Microsoft.Xrm.Sdk.Metadata.PicklistAttributeMetadata)(attribute)))).OptionSet.Options;
                                            List<Picklist> picklist = new List<Picklist>();

                                            foreach (var optitem in opt)
                                            {
                                                Picklist p = new Picklist();
                                                p.Label = optitem.Label.UserLocalizedLabel.Label;
                                                p.Value = optitem.Value.ToString();
                                                picklist.Add(p);
                                            }
                                            r.PicklistValues = picklist;

                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = ((OptionSetValue)targetEntity[logical]).Value.ToString();
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }
                                        else if (r.Type == "status")
                                        {
                                            var opt = ((Microsoft.Xrm.Sdk.Metadata.EnumAttributeMetadata)(((Microsoft.Xrm.Sdk.Metadata.StatusAttributeMetadata)(attribute)))).OptionSet.Options;
                                            List<Picklist> picklist = new List<Picklist>();

                                            foreach (var optitem in opt)
                                            {
                                                Picklist p = new Picklist();
                                                p.Label = optitem.Label.UserLocalizedLabel.Label;
                                                p.Value = optitem.Value.ToString();
                                                picklist.Add(p);
                                            }
                                            r.PicklistValues = picklist;

                                            if (targetEntity != null)
                                            {
                                                if (targetEntity.Attributes.Contains(logical))
                                                {
                                                    data.Value = ((OptionSetValue)targetEntity[logical]).Value.ToString();
                                                }
                                                else
                                                {
                                                    data.Value = String.Empty;
                                                }
                                                data.LogicalName = logical;
                                                data.Type = r.Type;
                                            }
                                        }

                                        #endregion

                                        r.LogicalName = logical;
                                        r.ColSpan = ritem.Attribute("colspan") != null ? ritem.Attribute("colspan").Value : String.Empty;
                                        r.RowSpan = ritem.Attribute("rowspan") != null ? ritem.Attribute("rowspan").Value : String.Empty;
                                        r.Disabled = ritem.Descendants("control").FirstOrDefault().Attribute("disabled") == null ? String.Empty : ritem.Descendants("control").FirstOrDefault().Attribute("disabled").Value;

                                        listrows.Add(r);

                                        if (targetEntity != null)
                                        {
                                            FormList.Add(data);
                                        }
                                    }
                                }
                                else if (ritem.Attribute("userspacer") != null)
                                {
                                    r.UserSpacer = ritem.Attribute("userspacer").Value;
                                    listrows.Add(r);
                                }
                                else
                                {
                                    r.IsSpace = "true";
                                    listrows.Add(r);
                                }
                            }

                            else if (ritem.Descendants("control").Descendants("parameters").Descendants("ViewId").FirstOrDefault() != null)
                            {
                                if (SubGridModel == null)
                                {
                                    r.ViewId = ritem.Descendants("control").Descendants("parameters").Descendants("ViewId").FirstOrDefault().Value;
                                    SubGridModel internalSubGrid = new SubGridModel();
                                    internalSubGrid.NewFormId = String.Empty;
                                    internalSubGrid.UpdateFormId = String.Empty;
                                    internalSubGrid.SubGridLogicalName = String.Empty;


                                    List<SubGridModel> lSubGrid = new List<SubGridModel>();
                                    lSubGrid.Add(internalSubGrid);

                                    r.SubGrids = lSubGrid;
                                }
                                else
                                {
                                    //
                                    SubGridModel customview = SubGridModel.Where(p => p.SubGridId.Equals(ritem.Descendants("control").Attributes("id").FirstOrDefault().Value)).FirstOrDefault();
                                    if (customview == null)
                                    {
                                        r.ViewId = ritem.Descendants("control").Descendants("parameters").Descendants("ViewId").FirstOrDefault().Value;
                                        SubGridModel internalSubGrid = new SubGridModel();
                                        internalSubGrid.NewFormId = String.Empty;
                                        internalSubGrid.UpdateFormId = String.Empty;
                                        internalSubGrid.SubGridLogicalName = String.Empty;


                                        List<SubGridModel> lSubGrid = new List<SubGridModel>();
                                        lSubGrid.Add(internalSubGrid);

                                        r.SubGrids = lSubGrid;
                                    }
                                    else
                                    {
                                        r.ViewId = customview.SubGridViewId == String.Empty ? ritem.Descendants("control").Descendants("parameters").Descendants("ViewId").FirstOrDefault().Value : customview.SubGridViewId;
                                        SubGridModel internalSubGrid = new SubGridModel();

                                        internalSubGrid.NewFormId = String.IsNullOrEmpty(customview.NewFormId) == true ? String.Empty : customview.NewFormId;
                                        internalSubGrid.UpdateFormId = String.IsNullOrEmpty(customview.UpdateFormId) == true ? String.Empty : customview.UpdateFormId;
                                        internalSubGrid.SubGridLogicalName = String.IsNullOrEmpty(customview.SubGridLogicalName) == true ? String.Empty : customview.SubGridLogicalName;
                                        List<SubGridModel> lSubGrid = new List<SubGridModel>();
                                        lSubGrid.Add(internalSubGrid);

                                        r.SubGrids = lSubGrid;

                                    }
                                }
                                r.SubGridId = ritem.Descendants("control").Attributes("id").FirstOrDefault().Value;
                                r.RelationShipName = ritem.Descendants("control").Descendants("parameters").Descendants("RelationshipName").FirstOrDefault().Value;
                                r.SubGridTargetEntity = ritem.Descendants("control").Descendants("parameters").Descendants("TargetEntityType").FirstOrDefault().Value;
                                r.ElementType = "subgrid";
                                r.Label = ritem.Elements("labels").Elements("label").FirstOrDefault().Attribute("description").Value;
                                r.ShowLabel = ritem.Attribute("showlabel").Value;
                                listrows.Add(r);
                            }
                        }
                        s.Rows = listrows;
                        listsections.Add(s);
                    }
                    c.Sections = listsections;
                    listColumns.Add(c);
                }
                t.Columns = listColumns;
                t.FormData = FormList;
                tablist.Add(t);
            }

            return tablist;
        }

        private String MakePrecision(int? Precision)
        {
            return "N" + Precision;
        }

        private void GetLinkedEntityData(IOrganizationService service, CrmServiceInformation serviceinfo, String PortalId, Entity item, String columns, XElement form, XmlNode node, Boolean flag, ref GridData data)
        {
            String AliasName = (((System.Xml.XmlElement)(node)).ParentNode).Attributes["alias"] != null ? (((System.Xml.XmlElement)(node)).ParentNode).Attributes["alias"].Value : (((System.Xml.XmlElement)(node))).Attributes["name"].Value.Split('.')[0];
            String LinkEntityName = (from elem in form.Descendants("link-entity") where elem.Attribute("alias").Value.Equals(AliasName) select elem.Attribute("name").Value).FirstOrDefault();
            String linkedattribute = (from elem in form.Descendants("link-entity") where elem.Attribute("alias").Value.Equals(AliasName) select elem.Attribute("to").Value).FirstOrDefault();

            EntityReference LinkedEntityValue = null;

            if (item.Attributes.Contains(linkedattribute))
            {
                LinkedEntityValue = (EntityReference)item[linkedattribute];
            }
            else
            {
                LinkedEntityValue = null;
                data.Value = String.Empty;
            }
            EntityMetadata EntityMetadata = GetAndCheckMetaDataFromCache(service, serviceinfo.ConfigurationId, LinkEntityName);

            data.DisplayName = EntityMetadata.Attributes.Where(a => a.LogicalName.Equals(columns)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;

            if (LinkedEntityValue != null)
            {
                try
                {
                    //customer gibi multi lookup alanlar için try catch bloguna konuldu.
                    Entity linkitem = service.Retrieve(LinkEntityName, LinkedEntityValue.Id, new ColumnSet(true));
                    AttributeMetadata m = EntityMetadata.Attributes.Where(a => a.LogicalName.Equals(columns)).FirstOrDefault();
                    if (linkitem.Attributes.Contains(m.LogicalName))
                    {
                        data.Value = CheckAndReturnCrmTypes(linkitem[columns], ref flag, m, service, columns, PortalId, serviceinfo)[0];
                    }
                    else
                    {
                        data.Value = String.Empty;
                    }
                }
                catch
                {
                    data.Value = String.Empty;
                }

            }
        }

        private EntityMetadata GetLinkedEntityMetaData(IOrganizationService service, String PortalId, String ConfigurationId, XElement form, XmlNode node)
        {
            String AliasName = (((System.Xml.XmlElement)(node)).ParentNode).Attributes["alias"] != null ? (((System.Xml.XmlElement)(node)).ParentNode).Attributes["alias"].Value : (((System.Xml.XmlElement)(node))).Attributes["name"].Value.Split('.')[0];
            String LinkEntityName = (from elem in form.Descendants("link-entity") where elem.Attribute("alias").Value.Equals(AliasName) select elem.Attribute("name").Value).FirstOrDefault();
            EntityMetadata EntityMetadata = GetAndCheckMetaDataFromCache(service, ConfigurationId, LinkEntityName);

            return EntityMetadata;
        }

        private void ConvertHTML(IOrganizationService service, ref String HTML, String Pattern, String Type, String EntityName, String EntityId, String PortalEntity, String PortalEntityUserField, String PortalEntityUserValue)
        {
            #region Check

            //get the entity records
            try
            {
                Entity ValueEntity = new Entity();
                if (Type == "entity")
                {
                    if (EntityName != String.Empty)
                    {
                        ValueEntity = service.Retrieve(EntityName, new Guid(EntityId), new ColumnSet(true));
                    }
                }
                else if (Type == "session")
                {
                    QueryByAttribute q = new QueryByAttribute();
                    q.EntityName = PortalEntity;
                    q.ColumnSet = new ColumnSet(true);
                    q.AddAttributeValue(PortalEntityUserField, PortalEntityUserValue);

                    EntityCollection collect = service.RetrieveMultiple(q);

                    ValueEntity = collect.Entities.FirstOrDefault();
                }
                else
                {
                    return;
                }

                ReplacePattern(service, Pattern, ref HTML, ValueEntity);

            }
            catch
            {
                //continue if get error
            }

            #endregion
        }

        private void ReplacePattern(IOrganizationService service, String Pattern, ref String HTML, Entity ValueEntity)
        {
            MatchCollection matches = Regex.Matches(HTML, Pattern);

            List<int> IndexElements = new List<int>();
            #region Find The Matches

            foreach (Match match in matches)
            {
                foreach (Capture capture in match.Captures)
                {
                    IndexElements.Add(capture.Index);

                }
            }

            #endregion

            List<ChangableValues> vals = new List<ChangableValues>();

            #region Find Change Value Indexes And Values
            foreach (var item in IndexElements)
            {
                String Attr = String.Empty;
                int leng = item + Pattern.Length + 1;
                for (int i = leng; i < HTML.Length; i++)
                {

                    if (HTML[i] == ']')
                    {
                        ChangableValues ca = new ChangableValues();
                        String NewValue = String.Empty;

                        if (ValueEntity.Attributes.Contains(Attr))
                        {
                            if (ValueEntity[Attr] is EntityReference)
                            {
                                NewValue = ((EntityReference)ValueEntity[Attr]).Name.ToString();
                            }
                            else if (ValueEntity[Attr] is OptionSetValue)
                            {
                                #region OptionSets
                                RetrieveAttributeRequest req = new RetrieveAttributeRequest
                                {
                                    EntityLogicalName = ValueEntity.LogicalName,
                                    LogicalName = Attr
                                };
                                RetrieveAttributeResponse res = (RetrieveAttributeResponse)service.Execute(req);

                                if (Attr == "statecode")
                                {
                                    StateAttributeMetadata pickisttext = res.AttributeMetadata as StateAttributeMetadata;

                                    IList<OptionMetadata> OptionsList = (from o in pickisttext.OptionSet.Options
                                                                         where o.Value.Value == ((OptionSetValue)ValueEntity[Attr]).Value
                                                                         select o).ToList();
                                    NewValue = (OptionsList.First()).Label.UserLocalizedLabel.Label;
                                }
                                if (Attr == "statuscode")
                                {
                                    StatusAttributeMetadata pickisttext = res.AttributeMetadata as StatusAttributeMetadata;

                                    IList<OptionMetadata> OptionsList = (from o in pickisttext.OptionSet.Options
                                                                         where o.Value.Value == ((OptionSetValue)ValueEntity[Attr]).Value
                                                                         select o).ToList();
                                    NewValue = (OptionsList.First()).Label.UserLocalizedLabel.Label;
                                }
                                else
                                {
                                    PicklistAttributeMetadata pickisttext = res.AttributeMetadata as PicklistAttributeMetadata;

                                    IList<OptionMetadata> OptionsList = (from o in pickisttext.OptionSet.Options
                                                                         where o.Value.Value == ((OptionSetValue)ValueEntity[Attr]).Value
                                                                         select o).ToList();
                                    NewValue = (OptionsList.First()).Label.UserLocalizedLabel.Label;
                                }
                                #endregion
                            }
                            else if (ValueEntity[Attr] is Money)
                            {
                                NewValue = ((Money)ValueEntity[Attr]).Value.ToString();
                            }
                            else
                            {
                                NewValue = ValueEntity[Attr].ToString();
                            }
                        }
                        ca.NewValue = NewValue;
                        ca.OldValue = Pattern + "[" + Attr + "]";
                        vals.Add(ca);
                        break;

                    }
                    Attr += HTML[i];
                }

            }
            #endregion

            #region Change The Values

            foreach (var item in vals)
            {
                HTML = HTML.Replace(item.OldValue, item.NewValue);
            }

            #endregion
        }

        private DateTime? ConvertUTCTimeToLocalTime(IOrganizationService service, String PortalId, String UTCTime, CrmServiceInformation serviceinfo)
        {
            RetrieveUserSettingsSystemUserResponse usersettings = CreateUserSettings(service, PortalId, serviceinfo);

            int timezonecode = (int)usersettings.Entity["timezonecode"];

            TimeZoneInfo timezoneinfo = GetTimeZone(service, timezonecode);

            DateTime? d = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(UTCTime), timezoneinfo);

            return d;
        }

        private static TimeZoneInfo GetTimeZone(IOrganizationService service, int crmTimeZoneCode)
        {
            var qe = new QueryExpression("timezonedefinition");
            qe.ColumnSet = new ColumnSet("standardname");
            qe.Criteria.AddCondition("timezonecode", ConditionOperator.Equal, crmTimeZoneCode);
            return TimeZoneInfo.FindSystemTimeZoneById(service.RetrieveMultiple(qe).Entities.FirstOrDefault().Attributes["standardname"].ToString());
        }

        private IOrganizationService CheckConfigurationService(String ConfigurationId, CrmServiceInformation serviceinfo)
        {

            DestinationService Service = new DestinationService(ConfigurationId,
                                                                serviceinfo.UserName,
                                                                serviceinfo.Password,
                                                                serviceinfo.Domain,
                                                                serviceinfo.OrganizationUri,
                                                                serviceinfo.DiscoveryUri,
                                                                serviceinfo.Source,
                                                                serviceinfo.CrmType,
                                                                serviceinfo.OrganizationName,
                                                                serviceinfo.Region,
                                                                serviceinfo.UseSSL,
                                                                serviceinfo.IsOffice365,
                                                                ConfigurationId,
                                                                "false",
                                                                true);
            return Service.IOrganizationService;

        }

        private String GetoptionsetTextOnValue(IOrganizationService service, String PortalId, String entityName, String attributeName, int selectedValue, String ConfigurationId)
        {
            Microsoft.Xrm.Sdk.Metadata.EntityMetadata metadata = GetAndCheckMetaDataFromCache(service, ConfigurationId, entityName);
            Microsoft.Xrm.Sdk.Metadata.OptionSetMetadata options = new OptionSetMetadata();

            var attributemetadata = metadata.Attributes.Where(p => p.LogicalName.Equals(attributeName)).FirstOrDefault();
            if (attributeName == "statuscode")
            {
                Microsoft.Xrm.Sdk.Metadata.StatusAttributeMetadata statusmetadata = attributemetadata as Microsoft.Xrm.Sdk.Metadata.StatusAttributeMetadata;
                options = statusmetadata.OptionSet;
            }
            else if (attributeName == "statecode")
            {
                Microsoft.Xrm.Sdk.Metadata.StateAttributeMetadata statemetadata = attributemetadata as Microsoft.Xrm.Sdk.Metadata.StateAttributeMetadata;
                options = statemetadata.OptionSet;
            }
            else
            {
                Microsoft.Xrm.Sdk.Metadata.PicklistAttributeMetadata picklistMetadata = attributemetadata as Microsoft.Xrm.Sdk.Metadata.PicklistAttributeMetadata;
                options = picklistMetadata.OptionSet;
            }


            IList<OptionMetadata> OptionsList = (from o in options.Options
                                                 where o.Value.Value == selectedValue
                                                 select o).ToList();
            string optionsetLabel = (OptionsList.First()).Label.UserLocalizedLabel.Label;
            return optionsetLabel;
        }

        internal IOrganizationService CreateDestinationCrmService(String PortalId, CrmServiceInformation serviceinfo, String UseCache = "false", Boolean IsUniqueInstansce = false)
        {
            DestinationService Service = new DestinationService(PortalId,
                                                                serviceinfo.UserName,
                                                                serviceinfo.Password,
                                                                serviceinfo.Domain,
                                                                serviceinfo.OrganizationUri,
                                                                serviceinfo.DiscoveryUri,
                                                                serviceinfo.Source,
                                                                serviceinfo.CrmType,
                                                                serviceinfo.OrganizationName,
                                                                serviceinfo.Region,
                                                                serviceinfo.UseSSL,
                                                                serviceinfo.IsOffice365,
                                                                serviceinfo.ConfigurationId,
                                                                UseCache,
                                                                IsUniqueInstansce);


            return Service.IOrganizationService;
        }

        private RetrieveUserSettingsSystemUserResponse CreateUserSettings(IOrganizationService service, String PortalId, CrmServiceInformation serviceinfo)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();

            RetrieveUserSettingsSystemUserResponse returnResponse = null;

            string userSettingID = string.Format("{0}_{1}_usersettings", PortalId, serviceinfo.ConfigurationId);

            returnResponse = (RetrieveUserSettingsSystemUserResponse)Deserialize(Azure.GetUserSettingFromAzureSQL(userSettingID), typeof(RetrieveUserSettingsSystemUserResponse));

            return returnResponse;

        }

        private void GetWidgetDataWithThreads(WidgetProperties Widget, List<WidgetParameters> WidgetParameters, IOrganizationService dataservice, CrmServiceInformation serviceinfo, String PortalId, String PortalEntity, String PortalEntityUserField, String PortalEntityUserValue, ref List<WidgetData> returndata)
        {
            try
            {
                if (Widget.WidgetType == 1)
                {
                    #region Grid
                    WidgetData data = new WidgetData();
                    List<Filters> filters = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().filters;
                    List<SubGridModel> subgrids = new List<SubGridModel>();

                    if (WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault() != null)
                        subgrids = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().subgrids;

                    String RecordPerPage = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().GridPerPage;
                    String ActionsCount = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().ActionsCount;
                    String PercentageofTotalWidth = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().PercentageofTotalWidth;
                    //if not a custom filter!

                    if (Widget.CustomFilter == null)
                    {
                        data.GridData = GetGridData(dataservice, serviceinfo, PortalId, Widget.ViewId, Widget, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue, RecordPerPage, String.Empty, String.Empty, ActionsCount, PercentageofTotalWidth);

                    }
                    else
                    {
                        data.GridData = GetGridDataCustom(dataservice, serviceinfo, PortalId, Widget.CustomFilter, Widget, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue, RecordPerPage, String.Empty, String.Empty, ActionsCount, PercentageofTotalWidth);
                    }


                    data.WidgetId = Widget.WidgetUniqueId;
                    data.WidgetType = "grid";
                    data.name = Widget.WidgetName;
                    data.PageWidgetId = Widget.PageWidgetName;
                    data.Order = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().Order;
                    returndata.Add(data);
                    #endregion
                }

                else if (Widget.WidgetType == 2)
                {
                    EntityMetadata metdadata = GetAndCheckMetaDataFromCache(dataservice, serviceinfo.ConfigurationId, Widget.EntityLogicalName);

                    WidgetData data = new WidgetData();
                    List<ChartData> ChartList = new List<ChartData>();

                    #region Chart Operations

                    XElement form = XElement.Parse(Widget.CustomFilter);
                    XAttribute aggregateFetch = new XAttribute("aggregate", true);
                    form.DescendantsAndSelf().FirstOrDefault().Add(aggregateFetch);
                    var list = form.Descendants("attribute").ToList();
                    //first remove the attributes of Entity
                    foreach (var attribute in list)
                    {
                        if (attribute.Parent.Name == "entity")
                            attribute.Remove();
                    }
                    //Column Chart
                    if (Widget.ChartType.Value == 2)
                    {
                        #region BarChart

                        #region Series - 0

                        if (!String.IsNullOrEmpty(Widget.Series0LogicalName))
                        {
                            XElement element = new XElement("attribute");

                            XAttribute attrName = new XAttribute("name", Widget.Series0LogicalName);
                            XAttribute attrAggregate = new XAttribute("aggregate", GetAggregateOperator(Widget.Series0Aggregate));
                            XAttribute attrAlias = new XAttribute("alias", "alias");
                            element.Add(attrName);
                            element.Add(attrAggregate);
                            element.Add(attrAlias);
                            form.Element("entity").Add(element);
                        }
                        #endregion

                        #region Series - 1
                        if (!String.IsNullOrEmpty(Widget.Series1LogicalName))
                        {
                            XElement element = new XElement("attribute");

                            XAttribute attrName = new XAttribute("name", Widget.Series1LogicalName);
                            XAttribute attrAggregate = new XAttribute("aggregate", GetAggregateOperator(Widget.Series1Aggregate));
                            XAttribute attrAlias = new XAttribute("alias", "alias1");
                            element.Add(attrName);
                            element.Add(attrAggregate);
                            element.Add(attrAlias);
                            form.Element("entity").Add(element);
                        }
                        #endregion

                        #region Series - 2
                        if (!String.IsNullOrEmpty(Widget.Series2LogicalName))
                        {
                            XElement element = new XElement("attribute");

                            XAttribute attrName = new XAttribute("name", Widget.Series2LogicalName);
                            XAttribute attrAggregate = new XAttribute("aggregate", GetAggregateOperator(Widget.Series2Aggregate));
                            XAttribute attrAlias = new XAttribute("alias", "alias2");
                            element.Add(attrName);
                            element.Add(attrAggregate);
                            element.Add(attrAlias);
                            form.Element("entity").Add(element);
                        }
                        #endregion

                        #region Series - 3
                        if (!String.IsNullOrEmpty(Widget.Series3LogicalName))
                        {
                            XElement element = new XElement("attribute");

                            XAttribute attrName = new XAttribute("name", Widget.Series3LogicalName);
                            XAttribute attrAggregate = new XAttribute("aggregate", GetAggregateOperator(Widget.Series3Aggregate));
                            XAttribute attrAlias = new XAttribute("alias", "alias3");
                            element.Add(attrName);
                            element.Add(attrAggregate);
                            element.Add(attrAlias);
                            form.Element("entity").Add(element);
                        }
                        #endregion

                        #region Series - 4
                        if (!String.IsNullOrEmpty(Widget.Series4LogicalName))
                        {
                            XElement element = new XElement("attribute");

                            XAttribute attrName = new XAttribute("name", Widget.Series4LogicalName);
                            XAttribute attrAggregate = new XAttribute("aggregate", GetAggregateOperator(Widget.Series4Aggregate));
                            XAttribute attrAlias = new XAttribute("alias", "alias4");
                            element.Add(attrName);
                            element.Add(attrAggregate);
                            element.Add(attrAlias);
                            form.Element("entity").Add(element);
                        }
                        #endregion

                        #region Horizontal
                        if (!String.IsNullOrEmpty(Widget.HorizontalLogicalName))
                        {
                            XElement element = new XElement("attribute");

                            XAttribute attrName = new XAttribute("name", Widget.HorizontalLogicalName);
                            XAttribute attrAggregate = new XAttribute("groupby", true);
                            XAttribute attrAlias = new XAttribute("alias", "grouper");
                            element.Add(attrName);
                            element.Add(attrAggregate);
                            element.Add(attrAlias);
                            form.Element("entity").Add(element);
                        }
                        #endregion

                        // add filters!
                        AddCustomFiltersToFetchXml(dataservice, ref form, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                        EntityCollection returncollect = dataservice.RetrieveMultiple(new FetchExpression(form.ToString()));

                        foreach (var item in returncollect.Entities)
                        {

                            ChartData c = new ChartData();

                            c.Series1 = !String.IsNullOrEmpty(Widget.Series0LogicalName) ? item.GetAttributeValue<AliasedValue>("alias") == null ? "0" : Convert.ToString(item.GetAttributeValue<AliasedValue>("alias").Value) : "999999";
                            c.Series1Name = item.GetAttributeValue<AliasedValue>("alias") == null ? "unknowattribute" : metdadata.Attributes.Where(p => p.LogicalName.Equals(item.GetAttributeValue<AliasedValue>("alias").AttributeLogicalName)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;

                            c.Series2 = !String.IsNullOrEmpty(Widget.Series1LogicalName) ? item.GetAttributeValue<AliasedValue>("alias1") == null ? "0" : Convert.ToString(item.GetAttributeValue<AliasedValue>("alias1").Value) : "999999";
                            c.Series2Name = item.GetAttributeValue<AliasedValue>("alias1") == null ? "unknowattribute" : metdadata.Attributes.Where(p => p.LogicalName.Equals(item.GetAttributeValue<AliasedValue>("alias1").AttributeLogicalName)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;

                            c.Series3 = !String.IsNullOrEmpty(Widget.Series2LogicalName) ? item.GetAttributeValue<AliasedValue>("alias2") == null ? "0" : Convert.ToString(item.GetAttributeValue<AliasedValue>("alias2").Value) : "999999";
                            c.Series3Name = item.GetAttributeValue<AliasedValue>("alias2") == null ? "unknowattribute" : metdadata.Attributes.Where(p => p.LogicalName.Equals(item.GetAttributeValue<AliasedValue>("alias2").AttributeLogicalName)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;

                            c.Series4 = !String.IsNullOrEmpty(Widget.Series3LogicalName) ? item.GetAttributeValue<AliasedValue>("alias3") == null ? "0" : Convert.ToString(item.GetAttributeValue<AliasedValue>("alias3").Value) : "999999";
                            c.Series4Name = item.GetAttributeValue<AliasedValue>("alias3") == null ? "unknowattribute" : metdadata.Attributes.Where(p => p.LogicalName.Equals(item.GetAttributeValue<AliasedValue>("alias3").AttributeLogicalName)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;

                            c.Series5 = !String.IsNullOrEmpty(Widget.Series4LogicalName) ? item.GetAttributeValue<AliasedValue>("alias4") == null ? "0" : Convert.ToString(item.GetAttributeValue<AliasedValue>("alias4").Value) : "999999";
                            c.Series5Name = item.GetAttributeValue<AliasedValue>("alias4") == null ? "unknowattribute" : metdadata.Attributes.Where(p => p.LogicalName.Equals(item.GetAttributeValue<AliasedValue>("alias4").AttributeLogicalName)).FirstOrDefault().DisplayName.UserLocalizedLabel.Label;

                            c.Horizontal = item.GetAttributeValue<AliasedValue>("grouper") == null ? "_blank" : this.GetFieldValue(dataservice, PortalId, Widget.EntityLogicalName, Widget.HorizontalLogicalName, serviceinfo.ConfigurationId, item.GetAttributeValue<AliasedValue>("grouper").Value, metdadata.Attributes.Where(p => p.LogicalName.Equals(item.GetAttributeValue<AliasedValue>("alias2").AttributeLogicalName)).FirstOrDefault(), serviceinfo).Item1;

                            c.SeriesColor1 = Widget.LegendColor0;
                            c.SeriesColor2 = Widget.LegendColor1;
                            c.SeriesColor3 = Widget.LegendColor2;
                            c.SeriesColor4 = Widget.LegendColor3;
                            c.SeriesColor5 = Widget.LegendColor4;
                            ChartList.Add(c);
                        }
                        #endregion

                    }
                    else if (Widget.ChartType.Value == 1 || Widget.ChartType.Value == 3)
                    {
                        #region PieChart && funnel Chart

                        #region Horizontal
                        if (!String.IsNullOrEmpty(Widget.HorizontalLogicalName))
                        {
                            XElement element = new XElement("attribute");

                            XAttribute attrName = new XAttribute("name", Widget.HorizontalLogicalName);
                            XAttribute attrAggregate = new XAttribute("groupby", true);
                            XAttribute attrAlias = new XAttribute("alias", "grouper");
                            element.Add(attrName);
                            element.Add(attrAggregate);
                            element.Add(attrAlias);
                            form.Element("entity").Add(element);
                        }
                        #endregion

                        #region Series

                        if (!String.IsNullOrEmpty(Widget.SeriesLogicalName))
                        {
                            XElement element = new XElement("attribute");

                            XAttribute attrName = new XAttribute("name", Widget.SeriesLogicalName);
                            XAttribute attrAggregate = new XAttribute("aggregate", "count");
                            XAttribute attrAlias = new XAttribute("alias", "alias");
                            element.Add(attrName);
                            element.Add(attrAggregate);
                            element.Add(attrAlias);
                            form.Element("entity").Add(element);
                        }
                        #endregion

                        // add filters!
                        AddCustomFiltersToFetchXml(dataservice, ref form, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                        EntityCollection returncollect = dataservice.RetrieveMultiple(new FetchExpression(form.ToString()));
                        foreach (var item in returncollect.Entities)
                        {
                            ChartData c = new ChartData();
                            c.Series = !String.IsNullOrEmpty(Widget.SeriesLogicalName) ? item.GetAttributeValue<AliasedValue>("alias") == null ? "0" : Convert.ToString(item.GetAttributeValue<AliasedValue>("alias").Value) : "999999";
                            c.Horizontal = item.GetAttributeValue<AliasedValue>("grouper") == null ? "_blank" : this.GetFieldValue(dataservice, PortalId, Widget.EntityLogicalName, Widget.HorizontalLogicalName, serviceinfo.ConfigurationId, item.GetAttributeValue<AliasedValue>("grouper").Value, metdadata.Attributes.Where(p => p.LogicalName.Equals(item.GetAttributeValue<AliasedValue>("grouper").AttributeLogicalName)).FirstOrDefault(), serviceinfo).Item1;
                            ChartList.Add(c);
                        }

                        #endregion
                    }
                    data.ChartType = Widget.ChartType.Value.ToString();
                    data.WidgetType = "chart";
                    data.name = Widget.WidgetName;
                    data.WidgetId = Widget.WidgetUniqueId;
                    data.PageWidgetId = Widget.PageWidgetName;
                    data.Order = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().Order;
                    data.ChartData = ChartList;
                    returndata.Add(data);
                    #endregion

                }

                else if (Widget.WidgetType == 3)
                {
                    String FetchXml = String.Empty;
                    WidgetData data = new WidgetData();

                    #region Calculated Field

                    if (Widget.CustomFilter == null)
                    {

                        String aggregate = Widget.CalculatedWidgetType == 1 ? "sum" : Widget.CalculatedWidgetType == 2 ? "avg" : "count";
                        FetchXml = "<fetch distinct='false' mapping='logical' aggregate='true'> ";
                        FetchXml += "<entity name='" + Widget.EntityLogicalName + "'>";
                        FetchXml += "<attribute name='" + Widget.CalculatedFieldLogicalName + "' aggregate='" + aggregate + "' alias='alias'/> ";

                        List<Filters> filters = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().filters;
                        if (filters.Count > 0)
                        {
                            String FilterStr = AddFilter(dataservice, PortalId, serviceinfo.ConfigurationId, Widget, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                            FetchXml += FilterStr;
                        }
                        FetchXml += "</entity>";
                        FetchXml += "</fetch>";
                    }
                    else
                    {
                        XElement form = XElement.Parse(Widget.CustomFilter);
                        AddCustomFiltersToFetchXml(dataservice, ref form, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                        List<Filters> filters = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().filters;
                        if (filters.Count > 0)
                        {
                            String FilterStr = AddFilter(dataservice, PortalId, serviceinfo.ConfigurationId, Widget, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                            XmlDocument xdoc = new XmlDocument();
                            xdoc.LoadXml(form.ToString());

                            XmlDocumentFragment xfrag = xdoc.CreateDocumentFragment();
                            xfrag.InnerXml = FilterStr;
                            xdoc.SelectSingleNode("//entity").AppendChild(xfrag);

                            FetchXml = xdoc.OuterXml;
                        }
                        else
                        {
                            FetchXml = form.ToString();
                        }
                    }
                    EntityCollection returncollect = dataservice.RetrieveMultiple(new FetchExpression(FetchXml));
                    var e = returncollect.Entities.FirstOrDefault();

                    if (e != null)
                    {
                        if (e.Attributes.Contains("alias"))
                        {
                            if (((AliasedValue)e.Attributes["alias"]).Value is Money)
                                data.Count = ((AliasedValue)e.Attributes["alias"]).Value == null ? "0" : ((Money)((AliasedValue)e.Attributes["alias"]).Value).Value.ToString("#.##");
                            else
                                data.Count = ((AliasedValue)e.Attributes["alias"]).Value == null ? "0" :
                                             Convert.ToDecimal(((AliasedValue)e.Attributes["alias"]).Value) == Decimal.Zero ? "0" : String.Format("{0:#.##}", ((AliasedValue)e.Attributes["alias"]).Value);
                        }
                        else
                            data.Count = "0";
                    }
                    else
                        data.Count = "0";

                    data.WidgetType = "calculatedfield";
                    data.name = Widget.WidgetName;
                    data.WidgetId = Widget.WidgetUniqueId;
                    data.PageWidgetId = Widget.PageWidgetName;
                    data.Order = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().Order;
                    returndata.Add(data);

                    #endregion
                }

                else if (Widget.WidgetType == 4)//Calendar Field
                {
                    String FetchXml = String.Empty;
                    WidgetData data = new WidgetData();

                    #region Calendar

                    if (Widget.CustomFilter == null)
                    {
                        FetchXml = "<fetch distinct='false' mapping='logical'> ";
                        FetchXml += "<entity name='" + Widget.EntityLogicalName + "'>";
                        FetchXml += "<attribute name='" + Widget.CalendarLogicalName + "'" + "/> ";
                        FetchXml += "<attribute name='" + Widget.CalendarStartDateLogicalName + "'" + "/> ";

                        if (Widget.CalendarEndDateLogicalName != null)
                            FetchXml += "<attribute name='" + Widget.CalendarEndDateLogicalName + "'" + "/> ";
                        List<Filters> filters = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().filters;
                        if (filters.Count > 0)
                        {
                            String FilterStr = AddFilter(dataservice, PortalId, serviceinfo.ConfigurationId, Widget, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue);
                            FetchXml += FilterStr;
                        }
                        FetchXml += "</entity>";
                        FetchXml += "</fetch>";
                    }
                    else
                    {
                        XElement form = XElement.Parse(Widget.CustomFilter);
                        AddCustomFiltersToFetchXml(dataservice, ref form, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                        List<Filters> filters = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().filters;
                        if (filters.Count > 0)
                        {
                            String FilterStr = AddFilter(dataservice, PortalId, serviceinfo.ConfigurationId, Widget, filters, PortalEntity, PortalEntityUserField, PortalEntityUserValue);

                            XmlDocument xdoc = new XmlDocument();
                            xdoc.LoadXml(form.ToString());

                            XmlDocumentFragment xfrag = xdoc.CreateDocumentFragment();
                            xfrag.InnerXml = FilterStr;
                            xdoc.SelectSingleNode("//entity").AppendChild(xfrag);

                            FetchXml = xdoc.OuterXml;
                        }
                        else
                        {
                            FetchXml = form.ToString();
                        }

                    }
                    EntityCollection returncollect = dataservice.RetrieveMultiple(new FetchExpression(FetchXml));

                    List<CalendarData> datalist = new List<CalendarData>();
                    WidgetData widgetdata = new WidgetData();


                    for (int i = 0; i < returncollect.Entities.Count; i++)
                    {
                        CalendarData Cdata = new CalendarData();
                        Entity ent = returncollect.Entities[i];

                        DateTime? StartDateLocal = null, EndDateLocal = null;


                        if (Widget.CustomFilter == null)
                        {
                            //Cdata.Value = ent.Attributes.Contains(Widget.CalendarLogicalName) == true ? ent[Widget.CalendarLogicalName].ToString() : String.Empty;
                            Cdata.StartDateValue = ent.Attributes.Contains(Widget.CalendarStartDateLogicalName) == true ?
                                                  ConvertUTCTimeToLocalTime(dataservice, PortalId, ent[Widget.CalendarStartDateLogicalName].ToString(), serviceinfo) :
                                                  null;
                            Cdata.EndDateValue = ent.Attributes.Contains(Widget.CalendarEndDateLogicalName) == true ?
                                                 ConvertUTCTimeToLocalTime(dataservice, PortalId, ent[Widget.CalendarEndDateLogicalName].ToString(), serviceinfo) :
                                                 null;

                            var title = ent.Attributes.Contains(Widget.CalendarLogicalName) == true ? ent[Widget.CalendarLogicalName].ToString() : String.Empty;
                            title = title + "-" + Cdata.StartDateValue + "-" + Cdata.EndDateValue;
                            Cdata.Value = title;

                        }
                        else
                        {
                            XElement form = XElement.Parse(Widget.CustomFilter);
                            String Entity = form.Descendants("attribute").Where(p => p.Attribute("namevalue") != null && p.Attribute("namevalue").Value == "1").Select(p => p.Attribute("name").Value).SingleOrDefault();
                            String StartDate = form.Descendants("attribute").Where(p => p.Attribute("start") != null && p.Attribute("start").Value == "1").Select(p => p.Attribute("name").Value).SingleOrDefault();
                            String EndDate = form.Descendants("attribute").Where(p => p.Attribute("end") != null && p.Attribute("end").Value == "1").Select(p => p.Attribute("name").Value).SingleOrDefault();
                            //Cdata.Value = ent.Attributes.Contains(Entity) == true ? ent[Entity].ToString() : String.Empty;

                            if (ent.Attributes.Contains(StartDate))
                            {
                                StartDateLocal = ConvertUTCTimeToLocalTime(dataservice, PortalId, ent[StartDate].ToString(), serviceinfo);
                            }

                            if (ent.Attributes.Contains(EndDate))
                            {
                                EndDateLocal = ConvertUTCTimeToLocalTime(dataservice, PortalId, ent[EndDate].ToString(), serviceinfo);
                            }

                            Cdata.StartDateValue = StartDateLocal;

                            Cdata.EndDateValue = EndDateLocal;

                            var title = ent.Attributes.Contains(Entity) == true ? ent[Entity].ToString() : String.Empty;
                            title = title + "-" + Cdata.StartDateValue + "-" + Cdata.EndDateValue;
                            Cdata.Value = title;
                        }
                        datalist.Add(Cdata);

                    }
                    widgetdata.WidgetType = "calendar";
                    widgetdata.name = Widget.WidgetName;
                    widgetdata.WidgetId = Widget.WidgetUniqueId;
                    widgetdata.PageWidgetId = Widget.PageWidgetName;
                    widgetdata.Values = datalist;
                    widgetdata.Order = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().SingleOrDefault().Order;
                    returndata.Add(widgetdata);
                    #endregion
                }
                else if (Widget.WidgetType == 5 || Widget.WidgetType == 6)//Form Field
                {
                    //need to create unique instansce true get the exact language of the form
                    SqlAzureConnection Azure = new SqlAzureConnection();
                    var result = Azure.CheckPortalIsMultiLanguage(PortalId);
                    if (result == true)
                    {
                        dataservice = CreateDestinationCrmService(PortalId, serviceinfo, false.ToString(), true);
                    }
                    #region FormXml
                    WidgetData data = new WidgetData();
                    Entity SystemForm = dataservice.Retrieve("systemform", new Guid(Convert.ToString(Widget.FormId)), new ColumnSet("formxml", "name", "objecttypecode"));
                    String EntityId = String.Empty;
                    List<SubGridModel> subgrids = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().subgrids;

                    List<Tabs> tabs = new List<Tabs>();
                    if (Widget.WidgetType == 5)
                        tabs = ParseFormXml(dataservice, SystemForm["formxml"].ToString(), SystemForm["objecttypecode"].ToString(), String.Empty, PortalId, serviceinfo, subgrids, SystemForm["formid"].ToString());

                    else if (Widget.WidgetType == 6)
                    {
                        String fetchxml = Widget.UpdateFormFetchXML;

                        EntityCollection collect = GetUpdateFormData(dataservice, fetchxml, PortalEntity, PortalEntityUserValue, PortalEntityUserField, WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault());

                        if (collect.Entities.Count > 0)
                        {
                            EntityId = collect.Entities[0].Id.ToString();
                            tabs = ParseFormXml(dataservice, SystemForm["formxml"].ToString(), SystemForm["objecttypecode"].ToString(), collect.Entities[0].Id.ToString(), PortalId, serviceinfo, subgrids, SystemForm["formid"].ToString());
                        }
                    }
                    if (Widget.WidgetType == 5)
                        data.WidgetType = "form";
                    if (Widget.WidgetType == 6)
                    {
                        data.WidgetType = "updateform";

                    }
                    data.WidgetGuid = Widget.WidgetId;
                    data.FormId = Widget.FormId;
                    data.name = Widget.WidgetName;
                    data.WidgetId = Widget.WidgetUniqueId;
                    data.PageWidgetId = Widget.PageWidgetName;
                    data.Order = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().Order;
                    data.IsSignature = Widget.SignatureEnabled != null ? Convert.ToString(Widget.SignatureEnabled) : String.Empty;
                    data.WidgetGuid = Widget.WidgetId.ToString();

                    FormLayout layout = new FormLayout();
                    layout.EntityId = EntityId;
                    layout.Tabs = tabs;
                    layout.FormName = SystemForm["name"].ToString();
                    data.FormLayout = layout;
                    returndata.Add(data);
                    #endregion
                }
                else if (Widget.WidgetType == 7)//Login Widget
                {
                    #region Login
                    WidgetData data = new WidgetData();
                    data.WidgetId = Widget.WidgetUniqueId;
                    data.PageWidgetId = Widget.PageWidgetName;
                    data.WidgetType = "login";
                    data.name = Widget.WidgetName;
                    data.Order = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().Order;
                    returndata.Add(data);
                    #endregion
                }
                else if (Widget.WidgetType == 8)//HTML Widget
                {
                    #region HTML Widget
                    WidgetData data = new WidgetData();
                    data.WidgetId = Widget.WidgetUniqueId; ;
                    data.PageWidgetId = Widget.PageWidgetName;
                    data.WidgetType = "htmlwidget";
                    data.name = Widget.WidgetName;
                    data.Order = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().Order;
                    String html = Widget.HTMLSource;
                    //if HTML Widget use in a page we cant pass entity values into html widget.Just session
                    ConvertHTML(dataservice, ref html, "@@session", "session", String.Empty, String.Empty, PortalEntity, PortalEntityUserField, PortalEntityUserValue);
                    ConvertHTML(dataservice, ref html, "@@entity", "entity", String.Empty, String.Empty, PortalEntity, PortalEntityUserField, PortalEntityUserValue);
                    data.HTML = html;
                    returndata.Add(data);

                    #endregion
                }
                else if (Widget.WidgetType == 9)
                {
                    #region Picture Widget
                    WidgetData data = new WidgetData();

                    String pictureHeight = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().PictureHeight;
                    //if use a custom filter!
                    if (Widget.UseCustomFilter)
                    {
                        data.PictureData = GetPictureData(dataservice, serviceinfo, PortalId, Widget.ViewId, Widget, PortalEntity, PortalEntityUserField, PortalEntityUserValue, Widget.UrlAttributeLogicalname, Widget.CustomFilter, pictureHeight);
                    }
                    else
                    {
                        data.PictureData = GetPictureData(dataservice, serviceinfo, PortalId, Widget.ViewId, Widget, PortalEntity, PortalEntityUserField, PortalEntityUserValue, Widget.UrlAttributeLogicalname, string.Empty, pictureHeight);
                    }


                    data.WidgetId = Widget.WidgetUniqueId;
                    data.WidgetType = "picture";
                    data.name = Widget.WidgetName;
                    data.PageWidgetId = Widget.PageWidgetName;
                    data.Order = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().Order;
                    returndata.Add(data);
                    #endregion
                }
                else if (Widget.WidgetType == 10)
                {

                    #region Link Widget
                    WidgetData data = new WidgetData();

                    data.WidgetId = Widget.WidgetUniqueId;
                    data.WidgetType = "link";
                    data.name = Widget.WidgetName;
                    data.PageWidgetId = Widget.PageWidgetName;
                    data.Order = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().Order;
                    returndata.Add(data);
                    #endregion

                }
                else if (Widget.WidgetType == 11)
                {

                    WidgetData data = new WidgetData();

                    EntityMetadata metdadata = GetAndCheckMetaDataFromCache(dataservice, serviceinfo.ConfigurationId, Widget.EntityLogicalName);

                    data.FieldInfo = GetFieldWidgetData(dataservice, serviceinfo, PortalId, Widget.ViewId, PortalEntity, PortalEntityUserField, PortalEntityUserValue, Widget.Field1, Widget.Field2, Widget.Field3, Widget.CustomFilter, metdadata);

                    data.WidgetId = Widget.WidgetUniqueId;
                    data.WidgetType = "field";
                    data.name = Widget.WidgetName;
                    data.PageWidgetId = Widget.PageWidgetName;
                    data.Order = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().Order;
                    returndata.Add(data);

                }
                else if (Widget.WidgetType == 12)
                {
                    #region Notification Widget
                    WidgetData data = new WidgetData();

                    data.NotificationList = GetNotificationWidgetData(dataservice, serviceinfo, PortalId, Widget.ViewId, PortalEntity, PortalEntityUserField, PortalEntityUserValue, Widget.NotificationAttributeName, Widget.CustomFilter);

                    data.WidgetId = Widget.WidgetUniqueId;
                    data.WidgetType = "notification";
                    data.name = Widget.WidgetName;
                    data.PageWidgetId = Widget.PageWidgetName;
                    data.Order = WidgetParameters.Where(p => p.PageWidgetId.Equals(Widget.PageWidgetName)).ToList().FirstOrDefault().Order;
                    returndata.Add(data);
                    #endregion
                }

            }
            catch (Exception ex)
            {
                theException = ex; // save it
            }
        }

        public static Object Deserialize(string xml, Type toType)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(toType, null, int.MaxValue, false, false, null, new KnownTypesResolver());
                return deserializer.ReadObject(stream);
            }

        }

        public static EntityMetadata GetAndCheckMetaDataFromCache(IOrganizationService service, String ConfigurationId, String EntityLogicalName, String OverrideCache = null)
        {
            SqlAzureConnection Azure = new SqlAzureConnection();

            EntityMetadata ReturnEntityMetaData = null;

            string metadataID = string.Format("{0}_{1}_metadatacache", ConfigurationId, EntityLogicalName);

            ReturnEntityMetaData = (EntityMetadata)Deserialize(Azure.GetMetadataFromAzureSQL(metadataID), typeof(EntityMetadata));

            return ReturnEntityMetaData;
        }

        private String GetAggregateOperator(String val)
        {
            String Aggregate = String.Empty;
            if (val == "2")
            {
                Aggregate = "count";
            }
            else if (val == "1")
            {
                Aggregate = "sum";
            }
            return Aggregate;
        }

        private Tuple<String, String> GetFieldValue(IOrganizationService service, String PortalId, String EntityName, String AttributeName, String ConfigurationId, Object Value, AttributeMetadata meta, CrmServiceInformation serviceinfo)
        {
            String returnVal = String.Empty;
            Tuple<String, String> returnTuple = new Tuple<String, String>(String.Empty, String.Empty);

            if (Value is EntityReference)
            {
                returnVal = ((EntityReference)Value).Name.ToString();
            }
            else if (Value is OptionSetValue)
            {
                returnVal = GetoptionsetTextOnValue(service, PortalId, EntityName, AttributeName, (Value as OptionSetValue).Value, ConfigurationId);
            }
            else if (Value is Money)
            {
                int? precision = ((Microsoft.Xrm.Sdk.Metadata.MoneyAttributeMetadata)(meta)).Precision;

                returnVal = ((Money)Value).Value.ToString(MakePrecision(precision));
            }
            else if (Value is Decimal)
            {
                int? precision = ((Microsoft.Xrm.Sdk.Metadata.DecimalAttributeMetadata)(meta)).Precision;
                if (Value != null)
                    returnVal = Convert.ToDecimal(Value).ToString(MakePrecision(precision));
                else
                    returnVal = String.Empty;
            }
            else if (Value is Int32)
            {
                returnVal = Convert.ToString(Value);
            }
            else if (Value is DateTime)
            {
                RetrieveUserSettingsSystemUserResponse usersettings = CreateUserSettings(service, PortalId, serviceinfo);
                Value = FormatDates(service, usersettings, Value.ToString(), ((Microsoft.Xrm.Sdk.Metadata.DateTimeAttributeMetadata)(meta)).Format.Value.ToString().ToLower());

                var dateformat = usersettings.Entity["dateformatstring"].ToString();
                var timeformat = usersettings.Entity["timeformatstring"].ToString();
                returnVal = Convert.ToString(Value);
            }
            else if (Value is String)
            {
                returnVal = Convert.ToString(Value);
            }
            else if (Value is float)
            {
                int? precision = ((Microsoft.Xrm.Sdk.Metadata.DoubleAttributeMetadata)(meta)).Precision;
                if (Value != null)
                    returnVal = Convert.ToDouble(Value).ToString(MakePrecision(precision));
                else
                    returnVal = String.Empty;
            }
            else if (Value is Boolean)
            {
                returnVal = Convert.ToString(Value);
            }
            else if (Value is AliasedValue)
            {

                if (((AliasedValue)(Value)).Value is EntityReference == true)
                {
                    returnVal = ((EntityReference)((AliasedValue)(Value)).Value).Name;
                }
                else if (((AliasedValue)(Value)).Value is Money == true)
                {
                    returnVal = ((Money)((AliasedValue)(Value)).Value).Value.ToString();
                }
                else if (((AliasedValue)(Value)).Value is OptionSetValue == true)
                {
                    RetrieveAttributeRequest req = new RetrieveAttributeRequest();
                    req.EntityLogicalName = ((AliasedValue)(Value)).EntityLogicalName;
                    req.LogicalName = ((AliasedValue)(Value)).AttributeLogicalName;
                    RetrieveAttributeResponse respnewDicOption = (RetrieveAttributeResponse)service.Execute(req);

                    PicklistAttributeMetadata pickisttext = respnewDicOption.AttributeMetadata as PicklistAttributeMetadata;
                    IList<OptionMetadata> OptionsList = (from o in pickisttext.OptionSet.Options
                                                         where o.Value.Value == ((OptionSetValue)((AliasedValue)(Value)).Value).Value
                                                         select o).ToList();
                    string optionsetLabel = (OptionsList.First()).Label.UserLocalizedLabel.Label;

                    return new Tuple<string, string>(optionsetLabel, respnewDicOption.AttributeMetadata.DisplayName.UserLocalizedLabel.Label);
                }
                else
                {
                    returnVal = ((AliasedValue)(Value)).Value.ToString();
                }
            }
            else
            {
                returnVal = Convert.ToString(Value.ToString());
            }
            returnTuple = new Tuple<string, string>(returnVal, String.Empty);

            return returnTuple;
        }

        private void SendMail(IOrganizationService service, String PortalId, Entity ValueEntity, String AdminAdress, String AdminAlias, String EmailAdress, String Template)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(AdminAdress, AdminAlias);
            message.To.Add(EmailAdress);
            message.Subject = Template.IndexOf("<title>") == -1 ? String.Empty : Template.Substring(Template.IndexOf("<title>") + 7, Template.IndexOf("</title>") - Template.IndexOf("<title>") - 7);
            message.IsBodyHtml = true;
            if (!String.IsNullOrEmpty(Template))
            {
                ReplacePattern(service, "@portaluser", ref Template, ValueEntity);
            }
            message.Body = Template;

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.UseDefaultCredentials = true;

            EmailConfiguration Conf = ParseEmailXML(System.Web.Hosting.HostingEnvironment.MapPath("~/Helpers/EmailConfiguration.xml"), PortalId);

            smtpClient.Host = Conf.Host;
            smtpClient.Port = Convert.ToInt32(Conf.Port);
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new System.Net.NetworkCredential(Conf.Userame, Conf.Password);
            smtpClient.Send(message);
        }

        private String FormatDates(IOrganizationService service, RetrieveUserSettingsSystemUserResponse usersettings, String Value, String DatePart)
        {
            var separator = usersettings.Entity["dateformatstring"].ToString().Replace("/", "'/'");
            TimeZoneInfo TimeZoneInfo = GetTimeZone(service, Convert.ToInt32(usersettings.Entity["timezonecode"].ToString()));
            String ReturnValue = String.Empty;
            DateTime dt1 = Convert.ToDateTime(Value).AddMinutes(TimeZoneInfo.BaseUtcOffset.TotalMinutes);

            if (DatePart == "dateandtime")
                ReturnValue = String.Format("{0:" + separator + " " + usersettings.Entity["timeformatstring"] + "}", dt1);
            else if (DatePart == "dateonly")
                ReturnValue = String.Format("{0:" + separator + "}", dt1);

            var hour = dt1.Hour;
            if (usersettings.Entity["timeformatstring"].ToString().Contains("tt"))//means has am
            {
                if (DatePart == "dateandtime")
                {
                    if (!ReturnValue.Contains("am") && !ReturnValue.Contains("AM") && !ReturnValue.Contains("PM") && !ReturnValue.Contains("pm"))
                    {
                        var suffix = hour >= 12 ? "pm" : "am";
                        ReturnValue = ReturnValue + suffix;
                    }
                }
            }
            return ReturnValue;
        }

        private EmailConfiguration ParseEmailXML(String XML, String PortalId)
        {
            EmailConfiguration Configuration = new EmailConfiguration();

            XElement form = XElement.Load(XML);

            Configuration = (from elem in form.Descendants("Configuration")
                             where (elem.Attribute("portalid").Value.Equals(PortalId))
                             select new EmailConfiguration
                             {
                                 Userame = elem.Element("SMTPUserName").Value,
                                 Password = elem.Element("SMTPPassword").Value,
                                 Host = elem.Element("SMTPHost").Value,
                                 Port = elem.Element("SMTPPort").Value
                             }).FirstOrDefault();

            if (Configuration == null)
            {
                Configuration = (from elem in form.Descendants("Configuration")
                                 where (elem.Attribute("portalid").Value.Equals("default"))
                                 select new EmailConfiguration
                                 {
                                     Userame = elem.Element("SMTPUserName").Value,
                                     Password = elem.Element("SMTPPassword").Value,
                                     Host = elem.Element("SMTPHost").Value,
                                     Port = elem.Element("SMTPPort").Value
                                 }).FirstOrDefault();
            }
            return Configuration;
        }

        private List<BusinessProcessFlow> GetBusinessProcessFlowListByEntityName(CrmServiceInformation serviceinfo, String PortalId, String primaryEntityName, EntityMetadata entityMetaData, RetrieveUserSettingsSystemUserResponse usersettings, String stageid, String processid, String traversedpath)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);

            QueryExpression expression = new QueryExpression();
            expression.ColumnSet = new ColumnSet(true);
            expression.EntityName = "workflow";

            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(new ConditionExpression("category", ConditionOperator.Equal, 4));
            filter.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 2));
            filter.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 1));
            filter.AddCondition(new ConditionExpression("type", ConditionOperator.Equal, 1));
            expression.Criteria.AddFilter(filter);

            EntityCollection collection = service.RetrieveMultiple(expression);

            IEnumerable<string> idList = collection.Entities.Where(z => z.Attributes["primaryentity"].ToString() == primaryEntityName).Select(z => z.Attributes["workflowid"].ToString());

            List<BusinessProcessFlow> businessProcessFlowList = new List<BusinessProcessFlow>();
            foreach (var id in idList)
            {
                Entity entity = service.Retrieve("workflow", new Guid(id), new ColumnSet(true));
                JsonConverter[] converters = { new MyConverter() };

                BusinessProcessFlow bpfObj = new BusinessProcessFlow();
                bpfObj = GetBusinessProcessFlowItem(entity.Attributes["clientdata"].ToString(), entityMetaData, usersettings, entity.Attributes["workflowid"].ToString(), stageid, processid, traversedpath);
                if (bpfObj.StageList != null && bpfObj.StageList.Any())
                {
                    businessProcessFlowList.Add(bpfObj);
                }
            }

            return businessProcessFlowList;
        }

        private BusinessProcessFlow GetBusinessProcessFlowItem(string jsonData, EntityMetadata entityMetaData, RetrieveUserSettingsSystemUserResponse usersettings, string workflowid, String stageid, String processid, String traversedpath)
        {
            JsonConverter[] converters = { new MyConverter() };
            //Custom deserialize classımızla BPF verilerini kendi modellerine çeviriyoruz.(Bu nesneler üzerinden sorgulama işlemleri yapılıyor.)
            WorkflowStep workflowStep = JsonConvert.DeserializeObject<WorkflowStep>(jsonData, new JsonSerializerSettings() { Converters = converters });

            BusinessProcessFlow businessProcessFlow = new BusinessProcessFlow();

            businessProcessFlow.ID = workflowStep.id;
            businessProcessFlow.ProcessID = workflowid;
            businessProcessFlow.IsCrmUIWorkflow = workflowStep.isCrmUIWorkflow;
            businessProcessFlow.Mode = workflowStep.mode;
            businessProcessFlow.Name = workflowStep.name;
            businessProcessFlow.NextStepIndex = workflowStep.nextStepIndex;
            businessProcessFlow.PrimaryEntityName = workflowStep.primaryEntityName;
            businessProcessFlow.Title = workflowStep.title;
            businessProcessFlow.Description = workflowStep.description;
            businessProcessFlow.SelectedProcessID = processid;
            businessProcessFlow.SelectedStageID = stageid;
            businessProcessFlow.SelectedTraversedPath = traversedpath;


            businessProcessFlow.StageList = new List<StageItem>();

            //Entity step, stage'in bağlı olduğu entity bilgileriyle beraber stage'i döndürür.(Deploy, qualify, propose birer stage categorisidir.)
            var entityStepList = workflowStep.steps.list.OfType<EntityStep>().Where(z => z != null).ToList();

            foreach (EntityStep entityItem in entityStepList)
            {
                //Her entitystep nesnesi içerisinde stagestep listesi bulundurur.(Ekranda gördüğümüz her bir sekme bir stage'e denk geliyor.)
                var stageList = entityItem.steps.list.OfType<StageStep>();

                //Stageler ekleniyor
                foreach (StageStep stageItem in stageList)
                {
                    StageItem newStage = new StageItem();
                    newStage.Description = stageItem.description;
                    newStage.ID = stageItem.id;
                    newStage.Name = stageItem.name;
                    newStage.NextStageID = stageItem.nextStageId.HasValue ? stageItem.nextStageId.Value.ToString() : string.Empty;
                    newStage.StageID = stageItem.stageId.HasValue ? stageItem.stageId.Value.ToString() : string.Empty;
                    newStage.StageCategory = stageItem.stageCategory.ToString(); //Deploy, qualify, propose vs.

                    //Stage bilgilerini localize bilgilerine göre getiriyor.
                    if (stageItem.stepLabels.list != null)
                    {
                        StepLabelItem stepLabel = stageItem.stepLabels.list.FirstOrDefault();
                        newStage.LanguageCode = stepLabel.languageCode.ToString();
                        newStage.LabelID = stepLabel.labelId.ToString();
                        newStage.LabelDescription = stepLabel.description;
                    }

                    newStage.StepList = new List<StepItem>();

                    //Stage içerisinde liste objesi dinamik data içeriyor.Bu aşama ekranda göstereceğimiz field alanlarını çekiyoruz.
                    //Stage içerisindeki her bir input objesi bir stepstep objesidir.
                    if (stageItem.steps.list.Where(z => z.GetType() == typeof(StepStep)).Any())
                    {
                        var stepList = stageItem.steps.list.OfType<StepStep>();

                        //Stepler ekleniyor
                        foreach (StepStep stepItem in stepList)
                        {
                            StepItem newStep = new StepItem();
                            newStep.Description = stepItem.description;
                            newStep.ID = stepItem.id;
                            newStep.IsProcessRequired = stepItem.isProcessRequired;
                            newStep.Name = stepItem.name;
                            newStep.StepID = stepItem.stepStepId.HasValue ? stepItem.stepStepId.Value.ToString() : string.Empty;

                            //Step bilgilerini localize bilgilerine göre getiriyor.
                            if (stepItem.stepLabels.list != null)
                            {
                                StepLabelItem stepLabel = stepItem.stepLabels.list.FirstOrDefault();
                                newStep.LanguageCode = stepLabel.languageCode.ToString();
                                newStep.LabelID = stepLabel.labelId.ToString();
                                newStep.LabelDescription = stepLabel.description;
                            }

                            //Step içerisindeki list objesi dinamik data içeriyor.Bu liste içerisinden form elementimizin bilgilerini çekiyoruz.
                            ControlStep controlStepItem = stepItem.steps.list.OfType<ControlStep>().FirstOrDefault();

                            //Control Element ekleniyor (form üzerindenki her bir fielde[input vs.] karşılık geliyor.)
                            newStep.ControlElement = new ControlItem
                            {
                                ControlDisplayName = controlStepItem.controlDisplayName,
                                ControlID = controlStepItem.controlId,
                                DataFieldName = controlStepItem.dataFieldName,
                                Description = controlStepItem.description,
                                ID = controlStepItem.id,
                                IsSystemControl = controlStepItem.isSystemControl,
                                Name = controlStepItem.name,
                                Parameters = controlStepItem.parameters,
                                SystemStepType = controlStepItem.SystemStepType,
                                ControlProperty = PrepareControlProperty(controlStepItem.dataFieldName, entityMetaData, usersettings)
                            };

                            newStage.StepList.Add(newStep);
                        }
                    }


                    //Stagelerimizin şartlarını set ediyoruz.
                    newStage.ConditionList = new List<ConditionItem>();

                    //Stage içerisinde liste objesi dinamik data içeriyor.Bu aşama ekranda göstereceğimiz field alanlarının şartlarını çekiyoruz.
                    //Stage içerisindeki şartları controlstep türünde bir objede tutuyor.
                    //Her stagede controlstep olma zorunluluğu bulunmuyor.
                    if (stageItem.steps.list.Where(z => z.GetType() == typeof(ConditionStep)).Any())
                    {
                        //Her bir şartı conditionbranchstep olarak ayrı objelerde tutuyor.Örn. if - else if - else if - else 4 adet conditionbranch objesi içerisnde tutuluyor.
                        var conditionList = stageItem.steps.list.OfType<ConditionStep>().FirstOrDefault().steps.list.OfType<ConditionBranchStep>();

                        foreach (ConditionBranchStep conditionItem in conditionList)
                        {
                            ConditionItem newCondition = new ConditionItem();

                            //Şartları gruplamak için conditionbranc id kullanıyoruz.
                            newCondition.ID = conditionItem.id;
                            newCondition.ContainsElsebranch = stageItem.steps.list.OfType<ConditionStep>().FirstOrDefault().containsElsebranch ? "true" : "false";

                            //Şartlar sağlandığında hangi stage geçeceğini belirtiyoruz.
                            if (conditionItem.steps.list != null)
                            {
                                SetNextStageStep setNextStageStepItem = conditionItem.steps.list.OfType<SetNextStageStep>().FirstOrDefault();

                                newCondition.NextStageID = setNextStageStepItem.stageId.HasValue ? setNextStageStepItem.stageId.Value.ToString() : string.Empty;
                                newCondition.ParentStageID = setNextStageStepItem.parentStageId.HasValue ? setNextStageStepItem.parentStageId.Value.ToString() : string.Empty;
                            }

                            //else kısmında bir şart belirlenmezse conditionexpression oluşturulmuyor. Sadece nextstage idsi geliyor.
                            //aşağıdaki koşul if ve ifelse şartları üzerinde işlem yapıyor.
                            if (conditionItem.conditionExpression != null)
                            {
                                //Şartların and yada or lamı bağlandığını set ediyoruz.
                                // 2 and 3 or operatorune karşılık geliyor
                                //koşulumuzda birden fazla şart varsa Örn if(x > 5 && x < 10) aşağıdaki nesne ile operatoru alıyoruz.
                                if (conditionItem.conditionExpression.conditionOperatoroperator.Equals("2") ||
                                    conditionItem.conditionExpression.conditionOperatoroperator.Equals("3"))
                                {
                                    newCondition.AndOr = conditionItem.conditionExpression.conditionOperatoroperator;
                                }

                                //Bu method recursive bir method.Crm şartları verirken bir ağaç yapısı şeklinde şartları dallandıyor.
                                //Tüm şartlar ulaşmak için bu methodu kullanıyoruz.
                                ParseCondition(conditionItem.conditionExpression);

                                //Genel şartlarımızın tutulduğu liste üzerinden şartlarımızı yeni sınıfımıza aktatıyoruz.
                                foreach (IExpression expression in BPFGeneralExpressionList)
                                {
                                    ConditionItem newConditionForEach = new ConditionItem();

                                    newConditionForEach.ID = newCondition.ID;
                                    newConditionForEach.NextStageID = newCondition.NextStageID;
                                    newConditionForEach.ParentStageID = newCondition.ParentStageID;
                                    newConditionForEach.AndOr = newCondition.AndOr;
                                    newConditionForEach.ContainsElsebranch = newCondition.ContainsElsebranch;

                                    if (expression.GetType() == typeof(BinaryExpression))
                                    {
                                        //Binary classını şartlarımızın base classı olarak düşünebilirsiniz.
                                        //Şartımızın operatörü bu obje içerisinde tutuluyor( beginwith, equal vs. -> binaryExpression.conditionOperatoroperator )
                                        BinaryExpression binaryExpression = (BinaryExpression)expression;

                                        //Binary classımız içerisindeki left objesi soyut bilgileri ele alıyor. Hangi attribute üzerinde çalışacağımızı buradan seçiyoruz.
                                        EntityAttributeExpression entityAttributeExpression = (EntityAttributeExpression)binaryExpression.left;

                                        newConditionForEach.EntityName = entityAttributeExpression.entity.entityName;
                                        newConditionForEach.AttributeName = entityAttributeExpression.attributeName;
                                        newConditionForEach.ConditionOperator = binaryExpression.conditionOperatoroperator;

                                        //Binary objesinin somut kısmı right objesinde tutuluyor.Bu obje sayesinde elimizdeki değeri bir sabit bir değerle (abc, 123 vs.) 
                                        //veya bir fieldın değeriyle karşılaştırabiliriz.
                                        if (binaryExpression.right.FirstOrDefault().GetType() == typeof(PrimitiveExpression))
                                        {
                                            newConditionForEach.Value = ((PrimitiveExpression)binaryExpression.right.FirstOrDefault()).primitiveValue;
                                            newConditionForEach.ConditionType = "primitive";
                                        }
                                        else if (binaryExpression.right.FirstOrDefault().GetType() == typeof(EntityAttributeExpression))
                                        {
                                            newConditionForEach.Value = ((EntityAttributeExpression)binaryExpression.right.FirstOrDefault()).attributeName;
                                            newConditionForEach.ConditionType = "entityattribute";
                                        }
                                        //Yukarıdaki newConditionForEach.ConditionType objesi şartlar kontol edilirken gerekli.
                                        //Primitive bir karşılaştırma mı yoksa fielda göremi bir karşılaştırma yapacağımızı belirliyoruz.

                                    }
                                    //UnaryExpression karşılaştırmanın somut tarafına ihtiyaç duymayan şartlar için.Örn inputun içi dolumu veya boşmu.
                                    else if (expression.GetType() == typeof(UnaryExpression))
                                    {
                                        UnaryExpression unaryExpression = ((UnaryExpression)expression);

                                        newConditionForEach.EntityName = unaryExpression.operand.entity.entityName;
                                        newConditionForEach.AttributeName = unaryExpression.operand.attributeName;
                                        newConditionForEach.ConditionOperator = unaryExpression.conditionOperatoroperator;
                                        newConditionForEach.ConditionType = "containstype";
                                    }
                                    newStage.ConditionList.Add(newConditionForEach);
                                }
                                //Şartları ekledikten sonra diğer ConditionBranch için listeyi yeniliyoruz.
                                BPFGeneralExpressionList = new List<IExpression>();
                            }
                            else
                            {
                                //Şartın else kısmında bir koşul belirtilmediği için sadece nextstageid bilgisini alarak listeye ekliyoruz.
                                newStage.ConditionList.Add(newCondition);
                            }

                        }
                    }
                    //Oluşan stage bilgisini ana objemiz içerisindeki stage listesine ekliyoruz.
                    businessProcessFlow.StageList.Add(newStage);
                }
            }


            BPFStageIsMain(businessProcessFlow, businessProcessFlow.StageList.FirstOrDefault());

            return businessProcessFlow;
        }

        private void BPFStageIsMain(BusinessProcessFlow bpf, StageItem stage)
        {
            if (stage != null)
            {
                if (string.IsNullOrEmpty(stage.NextStageID))
                {
                    bpf.StageList.FirstOrDefault(z => z.StageID == stage.StageID).IsMainStage = "true";
                }

                var exist = bpf.StageList.Where(z => z.StageID == stage.NextStageID).FirstOrDefault();

                if (exist != null)
                {
                    bpf.StageList.FirstOrDefault(z => z.StageID == stage.StageID).IsMainStage = "true";
                    BPFStageIsMain(bpf, exist);
                }
            }

        }

        private ControlPropertyItem PrepareControlProperty(string logicalName, EntityMetadata entityMetaData, RetrieveUserSettingsSystemUserResponse usersettings)
        {
            ControlPropertyItem controlPropertyItem = new ControlPropertyItem();

            var attribute = entityMetaData.Attributes.Where(p => p.LogicalName.Equals(logicalName)).FirstOrDefault();

            if (attribute == null)
            {
                return new ControlPropertyItem();
            }

            if (attribute.AttributeType == AttributeTypeCode.DateTime)
            {

                controlPropertyItem.DatePart = ((Microsoft.Xrm.Sdk.Metadata.DateTimeAttributeMetadata)(attribute)).Format.Value.ToString().ToLower();
                if (usersettings != null)
                {
                    controlPropertyItem.BeforeDateFormat = usersettings.Entity["dateformatstring"].ToString();
                    controlPropertyItem.BeforeTimeFormat = usersettings.Entity["timeformatstring"].ToString();

                    controlPropertyItem.DateFormat = usersettings.Entity["dateformatstring"].ToString().Replace("M", "m").Replace("MM", "m").Replace("mm", "m").Replace("dd", "d").Replace("yyyy", "Y");
                    //convert crm time format to datetime picker format
                    if (usersettings.Entity["timeformatstring"].ToString().Contains("tt"))//means has am
                    {
                        controlPropertyItem.TimeFormat = "h:i a";
                    }
                    else
                    {
                        controlPropertyItem.TimeFormat = "H:i";
                    }
                }


                controlPropertyItem.ElementType = "datetime";
                controlPropertyItem.LogicalName = attribute.LogicalName;
            }

            if (attribute.AttributeType == AttributeTypeCode.Customer)
            {
                controlPropertyItem.ElementType = "customer";
                controlPropertyItem.LogicalName = attribute.LogicalName;
            }

            if (attribute.AttributeType == AttributeTypeCode.Lookup)
            {
                controlPropertyItem.ElementType = "lookup";
                controlPropertyItem.LogicalName = ((Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata)(attribute)).Targets.FirstOrDefault();
            }


            if (attribute.AttributeType == AttributeTypeCode.BigInt)
            {
                controlPropertyItem.ElementType = "bigint";
                controlPropertyItem.LogicalName = attribute.LogicalName;
            }

            if (attribute.AttributeType == AttributeTypeCode.Integer)
            {
                controlPropertyItem.ElementType = "integer";
                controlPropertyItem.LogicalName = attribute.LogicalName;
            }

            if (attribute.AttributeType == AttributeTypeCode.Decimal)
            {
                controlPropertyItem.Precision = ((Microsoft.Xrm.Sdk.Metadata.DecimalAttributeMetadata)(attribute)).Precision.ToString();

                controlPropertyItem.ElementType = "decimal";
                controlPropertyItem.LogicalName = attribute.LogicalName;
            }

            if (attribute.AttributeType == AttributeTypeCode.Double)
            {
                controlPropertyItem.Precision = ((Microsoft.Xrm.Sdk.Metadata.DoubleAttributeMetadata)(attribute)).Precision.ToString();

                controlPropertyItem.ElementType = "double";
                controlPropertyItem.LogicalName = attribute.LogicalName;
            }

            if (attribute.AttributeType == AttributeTypeCode.Memo)
            {
                controlPropertyItem.ElementType = "memo";
                controlPropertyItem.LogicalName = attribute.LogicalName;
            }

            if (attribute.AttributeType == AttributeTypeCode.Money)
            {
                controlPropertyItem.Precision = ((Microsoft.Xrm.Sdk.Metadata.MoneyAttributeMetadata)(attribute)).Precision.ToString();

                controlPropertyItem.ElementType = "money";
                controlPropertyItem.LogicalName = attribute.LogicalName;
            }

            if (attribute.AttributeType == AttributeTypeCode.String)
            {
                controlPropertyItem.ElementType = "string";
                controlPropertyItem.LogicalName = attribute.LogicalName;
            }

            if (attribute.AttributeType == AttributeTypeCode.Status)
            {
                var opt = ((Microsoft.Xrm.Sdk.Metadata.EnumAttributeMetadata)(((Microsoft.Xrm.Sdk.Metadata.StateAttributeMetadata)(attribute)))).OptionSet.Options;
                List<Picklist> picklist = new List<Picklist>();

                foreach (var optitem in opt)
                {
                    Picklist p = new Picklist();
                    p.Label = optitem.Label.UserLocalizedLabel.Label;
                    p.Value = optitem.Value.ToString();
                    picklist.Add(p);
                }

                controlPropertyItem.ElementType = "status";
                controlPropertyItem.LogicalName = attribute.LogicalName;
                controlPropertyItem.PicklistValues = picklist;
            }

            if (attribute.AttributeType == AttributeTypeCode.Boolean)
            {
                List<Picklist> picklist = new List<Picklist>();
                Picklist p = new Picklist();
                p.Label = ((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute)).OptionSet.FalseOption.Label.UserLocalizedLabel.Label;
                p.Value = ((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute)).OptionSet.FalseOption.Value.ToString();
                p.DefaultValue = (((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute))).DefaultValue.Value.ToString();
                picklist.Add(p);

                p = new Picklist();
                p.Label = ((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute)).OptionSet.TrueOption.Label.UserLocalizedLabel.Label;
                p.Value = ((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute)).OptionSet.TrueOption.Value.ToString();
                p.DefaultValue = (((Microsoft.Xrm.Sdk.Metadata.BooleanAttributeMetadata)(attribute))).DefaultValue.Value.ToString();
                picklist.Add(p);

                controlPropertyItem.ElementType = "boolean";
                controlPropertyItem.LogicalName = attribute.LogicalName;
                controlPropertyItem.PicklistValues = picklist;
            }

            if (attribute.AttributeType == AttributeTypeCode.Picklist)
            {
                var opt = ((Microsoft.Xrm.Sdk.Metadata.EnumAttributeMetadata)(((Microsoft.Xrm.Sdk.Metadata.PicklistAttributeMetadata)(attribute)))).OptionSet.Options;
                List<Picklist> picklist = new List<Picklist>();

                foreach (var optitem in opt)
                {
                    Picklist p = new Picklist();
                    p.Label = optitem.Label.UserLocalizedLabel.Label;
                    p.Value = optitem.Value.ToString();
                    picklist.Add(p);
                }

                controlPropertyItem.ElementType = "picklist";
                controlPropertyItem.LogicalName = attribute.LogicalName;
                controlPropertyItem.PicklistValues = picklist;
            }

            return controlPropertyItem;
        }

        private void ParseCondition(IExpression expression)
        {
            if (expression.GetType() == typeof(BinaryExpression))
            {
                BinaryExpression binaryExpression = ((BinaryExpression)expression);

                if (!binaryExpression.conditionOperatoroperator.Equals("2") && !binaryExpression.conditionOperatoroperator.Equals("3"))
                {
                    BPFGeneralExpressionList.Add(binaryExpression);
                }
                else
                {
                    BPFGeneralExpressionList.Add(binaryExpression.right.FirstOrDefault());
                    ParseCondition(binaryExpression.left);
                }
            }

        }

        private BusinessRuleContainer GetBusinessRules(CrmServiceInformation serviceinfo, String PortalId, String primaryEntityName, RetrieveUserSettingsSystemUserResponse usersettings, String formID = null)
        {
            IOrganizationService service = CreateDestinationCrmService(PortalId, serviceinfo);

            QueryExpression expression = new QueryExpression();
            expression.ColumnSet = new ColumnSet(true);
            expression.EntityName = "workflow";

            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(new ConditionExpression("category", ConditionOperator.Equal, 2));
            filter.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 2));
            filter.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 1));
            filter.AddCondition(new ConditionExpression("primaryentity", ConditionOperator.Equal, primaryEntityName));
            filter.AddCondition(new ConditionExpression("type", ConditionOperator.Equal, 1));
            expression.Criteria.AddFilter(filter);

            EntityCollection collection = service.RetrieveMultiple(expression);

            List<string> workflowIDList = collection.Entities.Select(z => z.Attributes["workflowid"].ToString()).ToList();
            List<string> filteredWorkflowIDList = new List<string>();

            foreach (string workflowIDItem in workflowIDList)
            {
                QueryExpression e = new QueryExpression();
                e.ColumnSet = new ColumnSet(true);
                e.EntityName = "processtrigger";

                FilterExpression f = new FilterExpression(LogicalOperator.And);
                f.AddCondition(new ConditionExpression("processid", ConditionOperator.Equal, new Guid(workflowIDItem)));
                e.Criteria.AddFilter(f);

                EntityCollection c = service.RetrieveMultiple(e);
                if (c.Entities != null && c.Entities.Any())
                {
                    Entity entity = c.Entities.FirstOrDefault();

                    if (!entity.Attributes.ContainsKey("formid"))
                    {
                        filteredWorkflowIDList.Add(((EntityReference)entity["processid"]).Id.ToString());
                    }
                    else if (((EntityReference)entity.Attributes["formid"]).Id.ToString().Equals(formID))
                    {
                        filteredWorkflowIDList.Add(((EntityReference)entity["processid"]).Id.ToString());
                    }
                }
            }

            List<string> xamlList = new List<string>();

            foreach (var filteredWorkflowIDItem in filteredWorkflowIDList)
            {
                xamlList.Add(collection.Entities.Where(z => z.Attributes["workflowid"].ToString().Equals(filteredWorkflowIDItem)).FirstOrDefault().Attributes["xaml"].ToString());
            }

            BusinessRuleContainer businessRules = new BusinessRuleContainer();
            businessRules.BusinessRules = new List<BusinessRuleList>();
            foreach (var xaml in xamlList)
            {
                businessRules.BusinessRules.Add(ParseForBusinessRule(xaml));
            }

            return businessRules;
        }

        private Boolean CreateServiceWithUniqueInstanceIsWidgetIsFormAndJustItSelf(List<WidgetParameters> WidgetParameters, WidgetProperties properties)
        {
            if (WidgetParameters.Count == 1 && (properties.WidgetType == 5 || properties.WidgetType == 6))
            {
                return true;
            }
            return false;
        }
        public static BusinessRuleList ParseForBusinessRule(string xamlSource)
        {
            ActivityBuilder ab = XamlServices.Load(ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(new StringReader(xamlSource)))) as ActivityBuilder;

            var list = ((ab.Implementation as Microsoft.Xrm.Sdk.Workflow.Activities.Workflow).Activities.FirstOrDefault() as Microsoft.Xrm.Sdk.Workflow.Activities.ActivityReference).Properties;

            var activities = list["Activities"];

            BusinessRuleList businessRuleList = new BusinessRuleList();

            #region Parse Operation

            businessRuleList.BRList = new List<BRBranch>();
            BRBranch branch = new BRBranch();
            branch.ConditionList = new List<BRCondition>();
            BRCondition brCondition = new BRCondition();
            branch.ActionList = new List<BRAction>();
            BRAction brAction = new BRAction();

            var activityList = (activities as ICollection<System.Activities.Activity>).ToList();

            string lastActivityType = string.Empty;

            foreach (var item in activityList)
            {

                #region ## Set left side value
                if (item.DisplayName.Equals("GetEntityProperty"))
                {
                    var getEntityProperty = item as Microsoft.Xrm.Sdk.Workflow.Activities.GetEntityProperty;


                    if (lastActivityType == item.DisplayName.ToString().ToLower())
                    {
                        brCondition.Value = getEntityProperty.Attribute.Expression.ToString();
                        brCondition.ValueType = "entityattribute";
                    }
                    else
                    {
                        brCondition.AttributeName = getEntityProperty.Attribute.Expression.ToString();
                        brCondition.EntityName = getEntityProperty.EntityName.Expression.ToString();
                    }
                }
                #endregion

                #region ## Set right side value
                if (item.DisplayName.Equals("EvaluateExpression"))
                {
                    var evaluateExpression = item as Microsoft.Xrm.Sdk.Workflow.Activities.ActivityReference;

                    brCondition.Value = (evaluateExpression.Arguments["Parameters"].Expression as Microsoft.VisualBasic.Activities.VisualBasicValue<object[]>).ExpressionText.Split(',')[1].Trim().Trim('"');
                    brCondition.ValueType = "primitive";
                }
                #endregion

                #region ## Set condition operator contains equal etc..
                if (item.DisplayName.Equals("EvaluateCondition"))
                {
                    var evaluateCondition = item as Microsoft.Xrm.Sdk.Workflow.Activities.ActivityReference;

                    brCondition.Operator = evaluateCondition.Arguments["ConditionOperator"].Expression.ToString().ToLower();

                    branch.ConditionList.Add(brCondition);
                    brCondition = new BRCondition();
                }
                #endregion

                #region ## Set condition logical operator and | or
                if (item.DisplayName.Equals("EvaluateLogicalCondition"))
                {
                    var evaluateCondition = item as Microsoft.Xrm.Sdk.Workflow.Activities.ActivityReference;

                    branch.LogicalOperator = evaluateCondition.Arguments["LogicalOperator"].Expression.ToString().ToLower();
                }
                #endregion

                #region ## Set actions lock,visiblity etc..
                if (item.DisplayName.Contains("ConditionBranchStep"))
                {
                    var conditionBranchStep = item as Microsoft.Xrm.Sdk.Workflow.Activities.ActivityReference;
                    var condActivity = ((conditionBranchStep.Properties["Then"] as Microsoft.Xrm.Sdk.Workflow.Activities.ActivityReference).Properties["Activities"] as ICollection<System.Activities.Activity>).ToList();

                    foreach (var condActivityItem in condActivity)
                    {

                        #region  ## Set value action - primitive / entityattribute
                        if (condActivityItem.DisplayName.Contains("SetAttributeValueStep"))
                        {
                            brAction = new BRAction();
                            var sequemce = condActivityItem as System.Activities.Statements.Sequence;
                            var activitySequance = (sequemce.Activities as ICollection<System.Activities.Activity>).ToList();

                            var setEntityProperty = activitySequance.FirstOrDefault(z => z.DisplayName.Equals("SetEntityProperty"));
                            brAction.AttributeName = (setEntityProperty as Microsoft.Xrm.Sdk.Workflow.Activities.SetEntityProperty).Attribute.Expression.ToString();
                            brAction.EntityName = (setEntityProperty as Microsoft.Xrm.Sdk.Workflow.Activities.SetEntityProperty).EntityName.Expression.ToString();

                            if (activitySequance.Any(z => z.DisplayName.Equals("EvaluateExpression")))
                            {
                                var evaluateExpression = activitySequance.FirstOrDefault(z => z.DisplayName.Equals("EvaluateExpression"));
                                brAction.Value = (((evaluateExpression as Microsoft.Xrm.Sdk.Workflow.Activities.ActivityReference)
                                                    .Arguments["Parameters"] as System.Activities.InArgument<object[]>)
                                                    .Expression as Microsoft.VisualBasic.Activities.VisualBasicValue<object[]>)
                                                    .ExpressionText;

                                var arr = brAction.Value.Split(',');

                                if (arr.Length > 2)
                                {
                                    brAction.Value = arr[1].Trim().Trim('"');
                                }
                                else
                                {
                                    brAction.Value = arr[1].Replace("}", "").Replace('"', ' ').Trim();
                                }

                                brAction.ValueType = "primitive";
                            }
                            else if (activitySequance.Any(z => z.DisplayName.Equals("GetEntityProperty")))
                            {
                                var getEntityProperty = activitySequance.FirstOrDefault(z => z.DisplayName.Equals("GetEntityProperty"));
                                brAction.Value = (getEntityProperty as Microsoft.Xrm.Sdk.Workflow.Activities.GetEntityProperty).Attribute.Expression.ToString();
                                brAction.ValueType = "entityattribute";
                            }

                            if (activitySequance.Count(z => z.DisplayName.Equals("EvaluateExpression")) > 1)
                            {
                                var evaluateExpression = activitySequance.Where(z => z.DisplayName.Equals("EvaluateExpression")).Skip(1).Take(1).FirstOrDefault();
                                var lookaptext = (((evaluateExpression as Microsoft.Xrm.Sdk.Workflow.Activities.ActivityReference)
                                                    .Arguments["Parameters"] as System.Activities.InArgument<object[]>)
                                                    .Expression as Microsoft.VisualBasic.Activities.VisualBasicValue<object[]>)
                                                    .ExpressionText;

                                brAction.Value = string.Format("{0}&{1}", brAction.Value, lookaptext.Split(',')[2].Replace('"', ' ').Trim());
                            }

                            brAction.Operation = "setvalue";

                            branch.ActionList.Add(brAction);
                        }
                        #endregion

                        #region ## Set error message show action
                        if (condActivityItem.DisplayName.Contains("SetMessageStep"))
                        {
                            brAction = new BRAction();
                            var sequemce = condActivityItem as System.Activities.Statements.Sequence;
                            var activitySequance = (sequemce.Activities as ICollection<System.Activities.Activity>).ToList().FirstOrDefault();
                            var setMessage = (activitySequance as Microsoft.Crm.Workflow.ClientActivities.SetMessage);

                            brAction.AttributeName = setMessage.ControlId.Expression.ToString();
                            brAction.Value = setMessage.StepLabels.FirstOrDefault().Description;

                            brAction.Operation = "showerrormessage";
                            branch.ActionList.Add(brAction);
                        }
                        #endregion

                        #region ## Set business required action
                        if (condActivityItem.DisplayName.Contains("SetFieldRequiredLevelStep"))
                        {
                            brAction = new BRAction();
                            var sequemce = condActivityItem as System.Activities.Statements.Sequence;
                            var activitySequance = (sequemce.Activities as ICollection<System.Activities.Activity>).ToList().FirstOrDefault(z => z.DisplayName.Contains("SetFieldRequiredLevelStep"));
                            var setFieldRequiredLevel = activitySequance as Microsoft.Crm.Workflow.ClientActivities.SetFieldRequiredLevel;

                            brAction.AttributeName = setFieldRequiredLevel.ControlId.Expression.ToString();
                            brAction.EntityName = setFieldRequiredLevel.EntityName.Expression.ToString();
                            brAction.Value = setFieldRequiredLevel.RequiredLevel.Expression.ToString().ToLower();

                            brAction.Operation = "setbusinessrequired";
                            branch.ActionList.Add(brAction);
                        }
                        #endregion

                        #region ## Set visibility action
                        if (condActivityItem.DisplayName.Contains("SetVisibilityStep"))
                        {
                            brAction = new BRAction();
                            var sequemce = condActivityItem as System.Activities.Statements.Sequence;
                            var activitySequance = (sequemce.Activities as ICollection<System.Activities.Activity>).ToList().FirstOrDefault(z => z.DisplayName.Contains("SetVisibility"));
                            var setVisibility = activitySequance as Microsoft.Crm.Workflow.ClientActivities.SetVisibility;

                            brAction.AttributeName = setVisibility.ControlId.Expression.ToString();
                            brAction.Value = setVisibility.IsVisible.Expression.ToString().ToLower();

                            brAction.Operation = "setvisiblity";
                            branch.ActionList.Add(brAction);
                        }
                        #endregion

                        #region ## Set default value action - primitive / entityattribute
                        if (condActivityItem.DisplayName.Contains("SetDefaultValueStep"))
                        {
                            brAction = new BRAction();
                            var sequemce = condActivityItem as System.Activities.Statements.Sequence;
                            var activitySequance = (sequemce.Activities as ICollection<System.Activities.Activity>).ToList();

                            var setEntityProperty = activitySequance.FirstOrDefault(z => z.DisplayName.Equals("SetEntityProperty"));
                            brAction.AttributeName = (setEntityProperty as Microsoft.Xrm.Sdk.Workflow.Activities.SetEntityProperty).Attribute.Expression.ToString();
                            brAction.EntityName = (setEntityProperty as Microsoft.Xrm.Sdk.Workflow.Activities.SetEntityProperty).EntityName.Expression.ToString();

                            if (activitySequance.Any(z => z.DisplayName.Equals("EvaluateExpression")))
                            {
                                var evaluateExpression = activitySequance.FirstOrDefault(z => z.DisplayName.Equals("EvaluateExpression"));
                                brAction.Value = (((evaluateExpression as Microsoft.Xrm.Sdk.Workflow.Activities.ActivityReference)
                                                    .Arguments["Parameters"] as System.Activities.InArgument<object[]>)
                                                    .Expression as Microsoft.VisualBasic.Activities.VisualBasicValue<object[]>)
                                                    .ExpressionText;

                                var arr = brAction.Value.Split(',');

                                if (arr.Length > 2)
                                {
                                    brAction.Value = arr[1].Trim().Trim('"');
                                }
                                else
                                {
                                    brAction.Value = arr[1].Replace("}", "").Replace('"', ' ').Trim();
                                }

                                brAction.ValueType = "primitive";
                            }
                            else if (activitySequance.Any(z => z.DisplayName.Equals("GetEntityProperty")))
                            {
                                var getEntityProperty = activitySequance.FirstOrDefault(z => z.DisplayName.Equals("GetEntityProperty"));
                                brAction.Value = (getEntityProperty as Microsoft.Xrm.Sdk.Workflow.Activities.GetEntityProperty).Attribute.Expression.ToString();
                                brAction.ValueType = "entityattribute";
                            }

                            if (activitySequance.Count(z => z.DisplayName.Equals("EvaluateExpression")) > 1)
                            {
                                var evaluateExpression = activitySequance.Where(z => z.DisplayName.Equals("EvaluateExpression")).Skip(1).Take(1).FirstOrDefault();
                                var lookaptext = (((evaluateExpression as Microsoft.Xrm.Sdk.Workflow.Activities.ActivityReference)
                                                    .Arguments["Parameters"] as System.Activities.InArgument<object[]>)
                                                    .Expression as Microsoft.VisualBasic.Activities.VisualBasicValue<object[]>)
                                                    .ExpressionText;

                                brAction.Value = string.Format("{0}&{1}", brAction.Value, lookaptext.Split(',')[2].Replace('"', ' ').Trim());
                            }

                            brAction.Operation = "setdefaultvalue";

                            branch.ActionList.Add(brAction);
                        }
                        #endregion

                        #region ## Set lock mode action
                        if (condActivityItem.DisplayName.Contains("SetDisplayModeStep"))
                        {
                            brAction = new BRAction();
                            var sequemce = condActivityItem as System.Activities.Statements.Sequence;
                            var activitySequance = (sequemce.Activities as ICollection<System.Activities.Activity>).ToList().FirstOrDefault(z => z.DisplayName.Contains("SetDisplayMode"));
                            var setDisplayMode = activitySequance as Microsoft.Crm.Workflow.ClientActivities.SetDisplayMode;

                            brAction.AttributeName = setDisplayMode.ControlId.Expression.ToString();
                            brAction.Value = setDisplayMode.IsReadOnly.Expression.ToString().ToLower();

                            brAction.Operation = "setlockmode";
                            branch.ActionList.Add(brAction);
                        }
                        #endregion

                    }

                    branch.BranchID = item.DisplayName.ToString().ToLower();

                    businessRuleList.BRList.Add(branch);

                    branch = new BRBranch();
                    branch.ConditionList = new List<BRCondition>();
                    branch.ActionList = new List<BRAction>();
                }
                #endregion

                lastActivityType = item.DisplayName.ToString().ToLower();
            }
            #endregion

            return businessRuleList;
        }

    }
    public class ChangableValues
    {
        public String OldValue { get; set; }
        public String NewValue { get; set; }
    }
    public class EmailConfiguration
    {
        public String Userame { get; set; }
        public String Password { get; set; }
        public String Host { get; set; }
        public String Port { get; set; }
    }

    public class MyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ISteps) || objectType == typeof(IExpression));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            if (FindClassName(jo["__class"].Value<string>(), "WorkflowStep"))
                return jo.ToObject<WorkflowStep>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "EntityStep"))
                return jo.ToObject<EntityStep>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "StageStep"))
                return jo.ToObject<StageStep>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "StepStep"))
                return jo.ToObject<StepStep>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "ConditionStep"))
                return jo.ToObject<ConditionStep>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "CustomJavaScriptStep"))
                return jo.ToObject<CustomJavaScriptStep>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "PrimaryEntity"))
                return jo.ToObject<PrimaryEntity>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "ConditionBranchStep"))
                return jo.ToObject<ConditionBranchStep>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "BinaryExpression"))
                return jo.ToObject<BinaryExpression>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "PrimitiveExpression"))
                return jo.ToObject<PrimitiveExpression>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "EntityAttributeExpression"))
                return jo.ToObject<EntityAttributeExpression>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "UnaryExpression"))
                return jo.ToObject<UnaryExpression>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "ControlStep"))
                return jo.ToObject<ControlStep>(serializer);

            if (FindClassName(jo["__class"].Value<string>(), "SetNextStageStep"))
                return jo.ToObject<SetNextStageStep>(serializer);


            return null;
        }

        public static bool FindClassName(string jObjectString, string className)
        {
            bool existClassName = jObjectString.Split(':').First().Equals(className);

            return existClassName;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }


    }



}
