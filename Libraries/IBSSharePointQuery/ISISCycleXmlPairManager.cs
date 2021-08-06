using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBSSharePointQuery
{
    public class ISISCycleXmlPairManager : XmlPairManager
    {
        public ISISCycleXmlPairManager()
        {
            this.NameOfSharepointList = "ISISCycleDates";

            this.GuidOfSharepointList = @"{44D65DBF-351F-4539-840E-FD14CBA77293}";

            List<XmlPair> list = new List<XmlPair>();

            List<string> toAdd = new List<string>();

            toAdd.Add("ows_Title");
            toAdd.Add("ows_Start_Date");
            toAdd.Add("ows_Finish_Date");
            
            foreach (string s in toAdd)
            {
                XmlPair temp = new XmlPair(s);
                list.Add(temp);
            }

            this.pairsTemplate = list;

            this.createNextDictionary();
        }

        public List<ISISCycle> getAsCycles()
        {
            List<ISISCycle> cycles = new List<ISISCycle>();

            List<string> cycleNames = this.getAllValuesForKey("Title");
            List<DateTime> startDates = this.getAllValuesForKeyAsDates("Start_Date");
            List<DateTime> endDates = this.getAllValuesForKeyAsDates("Finish_Date");

            for (int i = 0; i < cycleNames.Count; i++)
            {
                ISISCycle cycle = new ISISCycle();
                cycle.cycleName = cycleNames[i];
                cycle.startDate = startDates[i];
                cycle.endDate = endDates[i];
                cycles.Add(cycle);
            }
            return cycles;
        }
    }
}
