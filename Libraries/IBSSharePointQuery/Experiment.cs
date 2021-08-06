using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IBSSharePointQuery
{
    public class Experiment
    {
        public string facility = "ISIS";
        public bool facilityAquired = true;

        private string RbNumber;
        public string title;
        public string allocatedInstrument;
        public string localContactUN;

        public string invAbstract;

        public string previousExperiment;

        //public string grantId;
            
        public List<Experimenter> experimenters;
        public List<ICATXmlIngest.Sample> samples;
        public List<ExperimentPart> parts;

        private List<string> keywords;
    
        public Experiment ()
        {
        }

        public string rbNumber
        {
            get
            {
                if (RbNumber.Contains('.'))
                {
                    return RbNumber.Substring(0, RbNumber.IndexOf('.'));
                }
                else
                {
                    return RbNumber;
                }
            }

            set
            {
                RbNumber = value;
            }
        }

        public void setKeywords(string keywordsString)
        {
            keywords = new List<string>();

            keywordsString = Regex.Replace(keywordsString, @"[^A-Za-z; ]", "");

            //tokenise string and add to list

            string[] delimiter = {";"};

            keywords.AddRange(keywordsString.Split(delimiter, StringSplitOptions.RemoveEmptyEntries));

        }

        public List<string> Keywords
        {
            get
            {
                return keywords;
            }
        }
    }
}
