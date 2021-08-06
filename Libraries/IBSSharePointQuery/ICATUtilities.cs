using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBSSharePointQuery
{
    public class ICATUtilities
    {
        public static string removeDecimalPlaces(string sIn)
        {
            string toReturn;
            if (sIn.Contains("."))
            {
                toReturn = sIn.Substring(0, sIn.IndexOf("."));
            }
            else
            {
                toReturn = sIn;
            }
            return toReturn;
        }

        public static string[] breakIntoKeywords(string toBreak)
        {
            if (toBreak == null || toBreak.Length == 0)
            {
                return new string[0];
            }
            else
            {
                return toBreak.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string correctInstrumentName(string instrument)
        {
            string correct = instrument.ToUpper();
            if (correct.Equals("HET_MERLIN"))
            {
                correct = "HET";
            }
            else if (correct.Equals("ENGIN_X"))
            {
                correct = "ENGINX";
            }
            return correct;
        }

        public static ICATXmlIngest.Keyword[] createKeywords(List<string> keywordStrings)
        {
            List<string> uniqueKeywords = new List<string>();
            foreach (string s in keywordStrings)
            {
                if (!uniqueKeywords.Contains(s)) { uniqueKeywords.Add(s); }
            }
            ICATXmlIngest.Keyword[] toReturn = new ICATXmlIngest.Keyword[uniqueKeywords.Count];

            for (int i = 0; i < toReturn.Length; i++)
            {
                ICATXmlIngest.Keyword keyword = new ICATXmlIngest.Keyword();
                keyword.name = uniqueKeywords[i];
                toReturn[i] = keyword;
            }
            return toReturn;
        }

        public static bool sampleAlreadyContains(ref List<ICATXmlIngest.Sample> investigationSamples, ICATXmlIngest.Sample  invSample)
        {
            foreach (ICATXmlIngest.Sample s in investigationSamples)
            {
                if ((s.name == invSample.name) && (s.chemical_formula == invSample.chemical_formula) && (s.safety_information == invSample.safety_information))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
