using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBSSharePointQuery
{
    public class ProposalListXmlPairManager : XmlPairManager
    {
        public ProposalListXmlPairManager()
        {
            this.NameOfSharepointList = "ProposalList";

            this.GuidOfSharepointList = @"{fda3987f-a947-4645-b0c3-c609349dd003}";

            List<XmlPair> list = new List<XmlPair>();

            List<string> toAdd = new List<string>();
            toAdd.Add("ows_Title0");
            toAdd.Add("ows_Abstract");
            toAdd.Add("ows_RBno");
            toAdd.Add("ows_Time_Allocated");
            toAdd.Add("ows_Time_Delivered");
            toAdd.Add("ows_Delivered_Start_Date");
            toAdd.Add("ows_Delivered_Completed_Date");
            toAdd.Add("ows_Instrument_Allocated");
            toAdd.Add("ows_Status");
            toAdd.Add("ows_Scheduled_Date");
            toAdd.Add("ows_Part");
            toAdd.Add("ows_Previous_Experiment");
            toAdd.Add("ows_NoA_Requested");
            toAdd.Add("ows_Review_Date");
            toAdd.Add("ows_RBpart");
            toAdd.Add("ows_Local_Contact_UN");
            toAdd.Add("ows_Scheduled_EndDate");
            toAdd.Add("ows_Modified");
            toAdd.Add("ows_GUID");
            toAdd.Add("ows_EncodedAbsUrl");
            toAdd.Add("ows_ScienceProgramme");

            foreach (string s in toAdd)
            {
                XmlPair temp = new XmlPair(s);
                list.Add(temp);
            }

            this.pairsTemplate = list;

            this.createNextDictionary();
        }

        public Experiment getAsExperiment()
        {
            Experiment experiment = new Experiment();

            experiment.title = this.getAllValuesForKey("Title0")[0];
            experiment.invAbstract = this.getAllValuesForKey("Abstract")[0];
            experiment.rbNumber = this.getAllValuesForKey("RBno")[0];
            experiment.previousExperiment = this.getAllValuesForKey("Previous_Experiment")[0];
            experiment.allocatedInstrument = this.getAllValuesForKey("Instrument_Allocated")[0];
            experiment.localContactUN = this.getAllValuesForKey("Local_Contact_UN")[0];
            experiment.setKeywords(this.getAllValuesForKey("ScienceProgramme")[0]);
            
            return experiment;
        }
    }
}
