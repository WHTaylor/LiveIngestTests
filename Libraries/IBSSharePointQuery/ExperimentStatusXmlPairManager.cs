using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBSSharePointQuery
{
    public class ExperimentStatusXmlPairManager : XmlPairManager
    {
        public ExperimentStatusXmlPairManager()
        {
            this.NameOfSharepointList = "ExperimentStatus";

            this.GuidOfSharepointList = @"{6390f6a5-7ad2-43c4-bf14-4f7707d29df8}";

            List<XmlPair> list = new List<XmlPair>();

            List<string> toAdd = new List<string>();

            toAdd.Add("ows_Title");
            toAdd.Add("ows_RBno");
            toAdd.Add("ows_Time_Allocated");
            toAdd.Add("ows_Time_Delivered");
            toAdd.Add("ows_Delivered_Start_Date");
            toAdd.Add("ows_Delivered_Completed_Date");
            toAdd.Add("ows_Instrument_Allocated");
            toAdd.Add("ows_Status");
            toAdd.Add("ows_EU_Accepted");
            toAdd.Add("ows_Part");
            toAdd.Add("ows_NoA_Requested");
            toAdd.Add("ows_Review_Date");
            toAdd.Add("ows_SRAS");
            toAdd.Add("ows_RBpart");
            toAdd.Add("ows_Scheduled_EndDate");
            toAdd.Add("ows_Scheduled_Date");

            foreach (string s in toAdd)
            {
                XmlPair temp = new XmlPair(s);
                list.Add(temp);
            }

            this.pairsTemplate = list;

            this.createNextDictionary();
        }

        public List<ExperimentPart> getAsExperimentParts()
        {
            List<ExperimentPart> parts = new List<ExperimentPart>();

            List<string> partNumbers = this.getAllValuesForKey("Part");
            List<DateTime> scheduledStarts = this.getAllValuesForKeyAsDates("Scheduled_Date");
            List<DateTime> scheduledEnds = this.getAllValuesForKeyAsDates("Scheduled_EndDate");
            List<string> instruments = this.getAllValuesForKey("Instrument_Allocated");

            for (int i = 0; i < partNumbers.Count; i++)
            {
                ExperimentPart part = new ExperimentPart();
                part.visitId = partNumbers[i];
                part.startDate = scheduledStarts[i];
                part.endDate = scheduledEnds[i];
                part.instrument = instruments[i];

                parts.Add(part);
            }
            return parts;
        }
    }
}
