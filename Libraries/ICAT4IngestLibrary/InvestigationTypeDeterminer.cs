using System;
using System.Collections.Generic;
using System.Linq;
using org.icatproject.isisicat.ICAT;

namespace ICAT4IngestLibrary
{
    /// <summary>
    /// Determine the type of an investigation from its rb number
    /// </summary>
    public class InvestigationTypeDeterminer
    {
        private enum RBType { experiment, commercial_experiment, calibration, commissioning }
        private readonly List<string> calibrationRBs;
        private static readonly List<string> defaultCalibrationRBs =
            new List<string>() { "0", "1000", "-1000", "12345", "99999", "999999" };

        public InvestigationTypeDeterminer() : this(null) {}
        public InvestigationTypeDeterminer(string calibrationRBList)
        {
            var rbsFromParamList = calibrationRBList?.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(rb => rb.Trim())
                .ToList();
            calibrationRBs = rbsFromParamList?.Count > 0 ? rbsFromParamList
                                                         : defaultCalibrationRBs;
        }

        public investigationType determine(string rbNumber, ICATValues values)
        {
            RBType localType = getTypeInteger(rbNumber);
            return values.getInvestigationTypeByName(localType.ToString());
        }

        private RBType getTypeInteger (string rbNumber)
        {
            /*
             * YYXnnnn
             * YY = Year (1 or 2 digit)
             * X = type number
             * nnnn = 4 digit sequence number
             * X types:
             *  0,1,2 = experiment proposals
             *  30 = comissioning
             *  35 = calibration
             *  39 = directors discretion = experiment
             *  5 = commercial
             *  6 = INES = experiment
             *  65 = RIKEN and JPARC = experiment
             *  7,8,9 = Xpress = experiment
             */
            if (rbNumber.Length < 5 || rbNumber.Length > 8)
            {
                //unknown - must be calibration junk
                return RBType.calibration;
            }
            else if (calibrationRBs.Contains(rbNumber))
            {
                return RBType.calibration;
            }
            else //length is 6 or 7
            {
                string middlePart;
                if (rbNumber.Length == 6) //single digit year
                {
                    middlePart = rbNumber.Substring(1, 2);
                }
                else // (rbNumber.Length == 7) //2 digit year
                {
                    middlePart = rbNumber.Substring(2, 2);
                }
                if (middlePart.StartsWith("0") || middlePart.StartsWith("1") || middlePart.StartsWith("2") || middlePart.StartsWith("6")
                    || middlePart.StartsWith("7") || middlePart.StartsWith("8") || middlePart.StartsWith("9") || middlePart.StartsWith("39"))
                {
                    return RBType.experiment;
                }
                else if(middlePart.StartsWith("35"))
                {
                    return RBType.calibration;
                }
                else if (middlePart.StartsWith("30"))
                {
                    return RBType.commissioning;
                }
                else if (middlePart.StartsWith("5"))
                {
                    return RBType.commercial_experiment;
                }
                else
                {
                    return RBType.experiment;
                }
            }
        }
    }
}
