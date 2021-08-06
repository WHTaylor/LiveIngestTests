using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;


namespace IBSSharePointQuery
{
    public class SharepointLink
    {
        private System.Net.ICredentials credentials;
        private string serverURL = null;

        //uses the default server 
        public SharepointLink(System.Net.ICredentials credentialsIn)
        {
            credentials = credentialsIn;
        }

        //manually set a server
        public SharepointLink(System.Net.ICredentials credentialsIn, string serverURLAndPort)
        {
            //https://www.facilities.rl.ac.uk/isis/experiments/_vti_bin/Lists.asmx
            credentials = credentialsIn;
            serverURL = serverURLAndPort + @"/isis/experiments/_vti_bin/Lists.asmx";
        }

        /// <summary>
        /// Get all experiments (KeyValuePairs) scheduled to run between two dates
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="pairManager"></param>
        /// <returns></returns>

        public XmlPairManager getKeyValuePairsScheduledBetweenDates(DateTime from, DateTime to, XmlPairManager pairManager)
        {
            string queryXml = @"<Where><And><Geq><FieldRef Name='Scheduled_Date' /><Value IncludeTimeValue='TRUE' Type='DateTime'>==FROMDATE==</Value></Geq><Leq><FieldRef Name='Scheduled_Date' /><Value IncludeTimeValue='TRUE' Type='DateTime'>==TODATE==</Value></Leq></And></Where>";
            queryXml = queryXml.Replace("==FROMDATE==", from.ToString("u"));
            queryXml = queryXml.Replace("==TODATE==", to.ToString("u"));
            return getKeyValuePairsForSearchTerm(queryXml, pairManager);
        }

        /// <summary>
        /// Get all experiments (KeyValuePairs) which have SharePoint modified dates newer than changesSince
        /// </summary>
        /// <param name="changesSince"></param>
        /// <param name="pairManager"></param>
        /// <returns></returns>
        public XmlPairManager getKeyValuePairsByModifiedDate(DateTime changesSince, XmlPairManager pairManager)
        {
            string queryXml = @"<Where><Geq><FieldRef Name='Modified' /><Value IncludeTimeValue='TRUE' Type='DateTime'>==DATE==</Value></Geq></Where>";
            queryXml = queryXml.Replace("==DATE==", changesSince.ToString("u"));
            return getKeyValuePairsForSearchTerm(queryXml, pairManager);
        }

        /// <summary>
        /// Get all experiments (KeyValuePairs) scheduled to run in a given cycle
        /// </summary>
        /// <param name="cycle"></param>
        /// <param name="pairManager"></param>
        /// <returns></returns>
        public XmlPairManager getKeyValuePairsByCycleDates(ISISCycle cycle, XmlPairManager pairManager)
        {
            String queryXml = @"<Where><Or><And><Geq><FieldRef Name='Delivered_Start_Date' /><Value IncludeTimeValue='TRUE' Type='DateTime'>==STARTDATE==</Value></Geq><Leq><FieldRef Name='Delivered_Start_Date' /><Value IncludeTimeValue='TRUE' Type='DateTime'>==ENDDATE==</Value></Leq></And><And><IsNull><FieldRef Name='Delivered_Start_Date' /></IsNull><And><Geq><FieldRef Name='Scheduled_Date' /><Value IncludeTimeValue='TRUE' Type='DateTime'>==STARTDATE==</Value></Geq><Lt><FieldRef Name='Scheduled_Date' /><Value IncludeTimeValue='TRUE' Type='DateTime'>==ENDDATE==</Value></Lt></And></And></Or></Where>";
            queryXml = queryXml.Replace("==STARTDATE==", cycle.startDate.ToString("u"));
            queryXml = queryXml.Replace("==ENDDATE==", cycle.endDate.ToString("u"));

            return getKeyValuePairsForSearchTerm(queryXml, pairManager);
        }

        /// <summary>
        /// Get all cycles
        /// </summary>
        /// <param name="cycleName"></param>
        /// <param name="pairManager"></param>
        /// <returns></returns>
        public XmlPairManager getKeyValuePairsForCycleName(string cycleName, XmlPairManager pairManager)
        {
            String queryXml = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\'Text\'>" + cycleName + "</Value></Eq></Where>";
            return getKeyValuePairsForSearchTerm(queryXml, pairManager);
        }

        /// <summary>
        /// Get everything related to an RB number. XmlPairManager means this is a very generic method which will work on
        /// any list where RB number is the key
        /// </summary>
        /// <param name="rbNumber"></param>
        /// <param name="pairManager"></param>
        /// <returns></returns>
        public XmlPairManager getKeyValuePairsForRBNumbers(string rbNumber, XmlPairManager pairManager)
        {
            String queryXml = "<Where><Eq><FieldRef Name=\'RBno\'/><Value Type=\'Number\'>" + rbNumber + "</Value></Eq></Where>";
            return getKeyValuePairsForSearchTerm(queryXml, pairManager);
        }

        /// <summary>
        /// Implementation of the search
        /// </summary>
        /// <param name="queryXml"></param>
        /// <param name="pairManager"></param>
        /// <returns></returns>
        private XmlPairManager getKeyValuePairsForSearchTerm(string queryXml, XmlPairManager pairManager)
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(System.Security.Principal.PrincipalPolicy.WindowsPrincipal);

            //set up XML pairs for what we are interest in
            IBSSharePointQuery.ListsService.Lists service = new IBSSharePointQuery.ListsService.Lists();

            //if the server URL has been manually set then override it
            if (serverURL != null && serverURL.Length > 0) { service.Url = serverURL; }

            //must provide authentication
            service.Credentials = credentials;

            XmlDocument camlDocument = new XmlDocument();
            XmlNode queryNode = camlDocument.CreateElement("Query");
            queryNode.InnerXml = queryXml;
            XmlNode viewFieldsNode = camlDocument.CreateElement("ViewFields");
            XmlNode queryOptionsNode = camlDocument.CreateElement("QueryOptions");
            XmlNode result = service.GetListItems(pairManager.GuidOfSharepointList, "", queryNode, viewFieldsNode, "1000", queryOptionsNode, "");

            //drill through the returned XML to extract the values and put them in the PairsDictionary
            foreach (XmlNode x in result.ChildNodes)
            {
                if (x.Name == "rs:data")
                {
                    //drill down a bit more
                    foreach (XmlNode xx in x.ChildNodes)
                    {
                        if (xx.Name == "z:row")
                        {
                            foreach (XmlAttribute att in xx.Attributes)
                            {
                                pairManager.setValue(att.Name, att.Value);
                            }
                            pairManager.createNextDictionary();
                        }
                    }
                }
            }
            return pairManager;

        }
    }
}
