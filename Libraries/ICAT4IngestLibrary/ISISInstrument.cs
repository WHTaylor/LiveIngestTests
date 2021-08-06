using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ICAT4IngestLibrary
{
    public class ISISInstrument
    {
        private List<string> shortCodes;
        private string longName;
        private List<int> fileNumbers;
        private string directory;
        private bool nexusInstrument;
        private bool on;
        private string mappingFile;
        private bool autoreductionOn;
        private bool analysisRequired;
        private bool isisCompute;
        private bool activeDirectory;
        private bool calibrationInstrument;
        private List<string> instrumentScientists;


        public ISISInstrument(XmlNode node)
        {
            shortCodes = new List<string>();
            fileNumbers = new List<int>();
            autoreductionOn = false;
            analysisRequired = false;
            isisCompute = false;
            activeDirectory = false;
            calibrationInstrument = false;
            instrumentScientists = new List<string>();

            if (node.Name != "instrument")
            {
                throw new Exception("Invalid XML to ISISInstrument");
            }
            XmlNodeList children = node.ChildNodes;
            foreach(XmlNode child in children)
            {
                string text = child.InnerText;
                if (child.Name == "shortcode")
                {
                    shortCodes.Add(child.InnerText);
                    shortCodes.Sort(delegate(string s1, string s2) { return s2.Length.CompareTo(s1.Length); });
                }
                else if (child.Name == "longname")
                {
                    longName = child.InnerText;
                }
                else if (child.Name == "filenumbers")
                {
                    string temp = child.InnerText;
                    fileNumbers.Add(int.Parse(temp));
                    //filenumbers must to sorted highest first
                    fileNumbers.Sort();
                    fileNumbers.Reverse();
                }
                else if (child.Name == "directory")
                {
                    directory = child.InnerText;
                }
                else if (child.Name == "nexus")
                {
                    string temp = child.InnerText;
                    nexusInstrument = bool.Parse(temp);
                }
                else if (child.Name == "on")
                {
                    string temp = child.InnerText;
                    on = bool.Parse(temp);
                }
                else if (child.Name == "mappingfile")
                {
                    mappingFile = child.InnerText;
                }
                else if (child.Name == "autoreduce")
                {
                    string temp = child.InnerText;
                    autoreductionOn = bool.Parse(temp);
                }
                else if (child.Name == "analysis")
                {
                    string temp = child.InnerText;
                    analysisRequired = bool.Parse(temp);
                }
                else if (child.Name == "isiscompute")
                {
                    isisCompute = bool.Parse(text);
                }
                else if (child.Name == "activedirectory")
                {
                    activeDirectory = bool.Parse(text);
                }
                else if (child.Name == "instrumentscientist")
                {
                    instrumentScientists.Add(child.InnerText);
                }
                else if (child.Name == "calibrationinstrument")
                {
                    string temp = child.InnerText;
                    calibrationInstrument = bool.Parse(temp);
                }
            }
        }


        public List<string> ShortCodes
        {
            get { return shortCodes; }
        }

        public string LongName
        {
            get { return longName; }
        }
        public List<int> FileNumbers
        {
            get { return fileNumbers; }
        }
        public string Directory
        {
            get { return directory; }
        }
        public bool NexusInstrument
        {
            get { return nexusInstrument; }
        }
        public bool On
        {
            get { return on; }
        }
        public string MappingFile
        {
            get { return mappingFile; }
        }
        public bool Autoreduction
        {
            get { return autoreductionOn; }
        }
        public bool AnalysisRequired
        {
            get { return analysisRequired; }
        }
        public bool ISISCompute
        {
            get { return isisCompute; }
        }
        public bool ActiveDirectory
        {
            get { return activeDirectory; }
        }
        public bool CalibrationInstrument
        {
            get { return calibrationInstrument; }
        }
        public List<String> InstrumentScientists
        {
            get { return instrumentScientists; }
        }
    }
}
