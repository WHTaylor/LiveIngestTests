using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBSSharePointQuery
{
    public class ExperimentTeamXmlPairManager : XmlPairManager
    {
        public ExperimentTeamXmlPairManager()
        {
            this.NameOfSharepointList = "ExperimentTeam";

            this.GuidOfSharepointList = @"{f6133173-5c5c-47eb-a424-2fd393a1387e}";
            
            List<XmlPair> list = new List<XmlPair>();

            List<string> toAdd = new List<string>();

            toAdd.Add("ows_Title");
            toAdd.Add("ows_LinkTitleNoMenu");
            toAdd.Add("ows_LinkTitle");
            toAdd.Add("ows_RBno");
            toAdd.Add("ows_UNno");
            toAdd.Add("ows_Role");
            toAdd.Add("ows_Instrument");
            toAdd.Add("ows_Organisation");
            toAdd.Add("ows_Department");
            toAdd.Add("ows_Start_Date");
            toAdd.Add("ows_Duration");
            toAdd.Add("ows_Finish_Date");
            toAdd.Add("ows_RB");

            foreach (string s in toAdd)
            {
                XmlPair temp = new XmlPair(s);
                list.Add(temp);
            }

            this.pairsTemplate = list;
            this.createNextDictionary();
        }

        public List<Experimenter> getAsExperimenters(UserManager userManager)
        {
            List<Experimenter> experimenters = new List<Experimenter>();

            List<string> userIds = this.getAllValuesForKey("UNno");
            List<string> roles = this.getAllValuesForKey("Role");

            for (int i = 0; i < userIds.Count; i++)
            {
                Experimenter experimenter = new Experimenter();
                FacilityUser facilityUser = new FacilityUser();

                string userNumber = userIds[i];
                
                isisudb.personDetailsDTO user = userManager.getUserByUserNumber(userNumber);


                facilityUser.facilityUserId = user.userNumber;

                //user = userManager.getUserByUserNumber("800.00");
                //facilityUser.facilityUserId = user.UserNumber.ToString();

                facilityUser.fedId = user.fedId;
                facilityUser.initials = user.initials;
                facilityUser.lastName = user.familyName;
                facilityUser.firstName = user.firstNameKnownAs;
                facilityUser.title = user.title;
                

                experimenter.facilityUser = facilityUser;

                if (roles[i] == "PI")
                {
                    experimenter.role = Role.principal_experimenter;
                }
                else
                {
                    experimenter.role = Role.experimenter;
                }
                if (!experimenters.Contains(experimenter))
                {
                    experimenters.Add(experimenter);
                }
            }

            return experimenters;
        }
    }
}
