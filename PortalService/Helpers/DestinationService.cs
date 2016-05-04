using Azure;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Web;

namespace PortalService
{
    public class DestinationService
    {
        private IOrganizationService _service;
        private CrmServiceClient _connectionClient;
        private Dictionary<string, string> RegionList = new Dictionary<string, string>();

        public IOrganizationService IOrganizationService
        {
            get { return _service; }
        }

        public CrmServiceClient ConnectionClient
        {
            get { return _connectionClient; }
        }

        public DestinationService(String PortalId, String UserName, String Password, String Domain, String OrganizationUrl, String DiscoveryUrl, String Source, String Type, String OrganizationName, String Region,
            String UseSSL, String IsOffice365, String ConfigurationId, String UseCache,Boolean UseUniqeInstansce = false)
        {

            String PortalKey = "!xRMLink?PoRtA1";
            Password = Helpers.DecryptRijndael(Password, PortalKey);
            //second depth
            String ConfID = ConfigurationId.ToUpper().Replace("{", "").Replace("}", "");
            Password = Helpers.DecryptRijndael(Password, ConfID);

            bool useSSL = UseSSL == null ? true : UseSSL.ToLower().Equals("true") ? true : false;
            bool isoffice365 = IsOffice365 == null ? false : IsOffice365.ToLower().Equals("true") ? true : false;
            string region = GetRegion(Region);

            //_service = new CrmServiceClient(new System.Net.NetworkCredential(UserName, Password, Domain), AuthenticationType.AD, OrganizationUrl, useSSL ? "443" : null, OrganizationName, useSsl: useSSL).OrganizationServiceProxy as IOrganizationService;

            //ADFS - On-premise
            if (Type.Equals("3"))
            {
                //throw new Exception(UserName + "_" + Password + "_" + Domain + "_" + Convert.ToString(useSSL) + "_" + OrganizationName + "_" + OrganizationUrl);
                _connectionClient = new CrmServiceClient(new System.Net.NetworkCredential(UserName, Password, Domain), AuthenticationType.IFD, OrganizationUrl, useSSL ? "443" : null, OrganizationName, useSsl: useSSL, useUniqueInstance: UseUniqeInstansce);                
                _service = (IOrganizationService)_connectionClient.OrganizationServiceProxy;
            }
            // On-premise
            else if (Type.Equals("1"))
            {
                _connectionClient = new CrmServiceClient(new System.Net.NetworkCredential(UserName, Password, Domain), AuthenticationType.AD, OrganizationUrl, useSSL ? "443" : null, OrganizationName, useSsl: useSSL, useUniqueInstance: UseUniqeInstansce);
                _service = (IOrganizationService)_connectionClient.OrganizationServiceProxy;

            }
            // Online
            else if (Type.Equals("2"))
            {
                _connectionClient = new CrmServiceClient(UserName, CrmServiceClient.MakeSecureString(Password), region, OrganizationName, useSsl: useSSL, isOffice365: isoffice365, useUniqueInstance: UseUniqeInstansce);
                _service = (IOrganizationService)_connectionClient.OrganizationServiceProxy;
            }



            #region Depreceted
            //Decrpty Password First
            //String X = "Url=" + OrganizationUrl + ";";
            //X += " Domain=" + Domain + "; Username=" + UserName + "; Password=" + Password + "";

            //if (Convert.ToBoolean(UseCache))
            //{
            //    _service = CrmConfigurationManager.CreateService(CrmConnection.Parse(X), "mycachedservice") as CachedOrganizationService;
            //   // ((CachedOrganizationService)_service).Cache.Mode = OrganizationServiceCacheMode.LookupAndInsert;
            //}
            //else
            //_service = CrmConfigurationManager.CreateService(CrmConnection.Parse(X), "myservice") as IOrganizationService;
            #endregion

        }

        private string GetRegion(string region)
        {
            if (String.IsNullOrEmpty(region))
            {
                return string.Empty;
            }

            RegionList.Add("1", "North America");
            RegionList.Add("2", "South America");
            RegionList.Add("3", "Canada");
            RegionList.Add("4", "EMEA");
            RegionList.Add("5", "APAC");
            RegionList.Add("6", "Australia");
            RegionList.Add("7", "Japan");
            RegionList.Add("8", "India");
            RegionList.Add("9", "North America");

            return RegionList[region];
        }


    }
}