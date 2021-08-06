using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBSSharePointQuery
{
    public class ExperimentPart
    {
        public string facilityCycle;
        public DateTime startDate;
        public DateTime endDate;
        private string VisitId;
        public string instrument;

        public ExperimentPart()
        {
        }

        public string visitId
        {
            get
            {
                if (VisitId.Contains('.'))
                {
                    return VisitId.Substring(0, VisitId.IndexOf('.'));
                }
                else
                {
                    return VisitId;
                }
            }

            set
            {
                VisitId = value;
            }
        }
    }
}
