using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace IBSSharePointQuery
{
    public abstract class XmlPairManager
    {
        protected List<Dictionary<string, XmlPair>> xmlPairDictList;
        public string GuidOfSharepointList {get;set;}
        public string NameOfSharepointList { get; set; }
        protected List<XmlPair> pairsTemplate;
        protected Dictionary<string, XmlPair> currentDictionary;

        public XmlPairManager()
        {
            xmlPairDictList = new List<Dictionary<string, XmlPair>>();
        }

        public void setValue(string attributeIn, string valueIn)
        {
            if (currentDictionary.Keys.Contains(attributeIn))
            {
                currentDictionary[attributeIn].Value = valueIn;
            }
        }

        public void createNextDictionary()
        {
            if (currentDictionary != null)
            {
                MemoryStream memStream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memStream, currentDictionary);
                memStream.Flush();
                memStream.Position = 0;
                Dictionary<string, XmlPair> tempDictionary = (Dictionary<string, XmlPair>)formatter.Deserialize(memStream);
                xmlPairDictList.Add(tempDictionary);                
            }
            currentDictionary = new Dictionary<string, XmlPair>();
            
            foreach(XmlPair pair in pairsTemplate)
            {
                currentDictionary.Add(pair.PrefixedAttibute, pair);
            }
        }

        public String WriteOutContents()
        {
            string toReturn = "New List : " + NameOfSharepointList + "\n\n";
            foreach (Dictionary<string, XmlPair> dict in xmlPairDictList)
            {
                foreach (String key in dict.Keys)
                {
                    toReturn = toReturn + "\n" + dict[key].Attribute + "\t:\t" + dict[key].Value;
                }
                toReturn += "End Item\n\n";
            }
            toReturn = toReturn + "\n\n\n\n";
            return toReturn;
        }

        public List<string> getAllValuesForKey(string key)
        {
            List<string> values = new List<string>();
            foreach (Dictionary<string, XmlPair> dict in xmlPairDictList)
            {
                try
                {
                    values.Add(dict["ows_" + key].Value);
                }
                catch (KeyNotFoundException)
                {
                    values.Add(null);
                }
            }
            return values;
        }
        public List<DateTime> getAllValuesForKeyAsDates(string key)
        {
            List<DateTime> values = new List<DateTime>();
            foreach (Dictionary<string, XmlPair> dict in xmlPairDictList)
            {
                try
                {
                    string dateAsString = dict["ows_" + key].Value;
                    DateTime asDate = DateTime.Parse(dateAsString);
                    values.Add(asDate);
                }
                catch (KeyNotFoundException)
                {

                }
                catch (Exception)
                {
                    values.Add(new DateTime());
                }
            }
            return values;
        }
    }
}
