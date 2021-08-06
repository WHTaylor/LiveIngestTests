using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBSSharePointQuery
{
    public class InvestigatorsXmlPairManager : XmlPairManager
    {
        public InvestigatorsXmlPairManager()
        {
            this.NameOfSharepointList = "PropsalInvestigators";

            this.GuidOfSharepointList = @"{b9150e95-779f-4cf4-ac39-a6b5de490054}";

            List<XmlPair> list = new List<XmlPair>();

            List<string> toAdd = new List<string>();

            toAdd.Add("ows_Title");
            toAdd.Add("ows_ID");
            toAdd.Add("ows_RBno");
            toAdd.Add("ows_Round");
            toAdd.Add("ows_Investigator");
            toAdd.Add("ows_Instrument");
            toAdd.Add("ows_Requested_time");
            toAdd.Add("ows_Role");
            toAdd.Add("ows_Organisation");
            toAdd.Add("ows_Department");
            toAdd.Add("ows_Country");
            toAdd.Add("ows_UN");
            toAdd.Add("ows_User_Number");
            toAdd.Add("ows_Allocated_Time");
            toAdd.Add("ows_RB");
            toAdd.Add("ows_Proposal");
            toAdd.Add("ows_Status");

            foreach (string s in toAdd)
            {
                XmlPair temp = new XmlPair(s);
                list.Add(temp);
            }

            this.pairsTemplate = list;
            this.createNextDictionary();
        }

        public Experimenter getLocalContact(UserManager userManager, string localContactUN)
        {
            Experimenter experimenter = new Experimenter();

            FacilityUser facilityUser = new FacilityUser();            

            isisudb.personDetailsDTO user = userManager.getUserByUserNumber(localContactUN);

            facilityUser.facilityUserId = user.userNumber;

            facilityUser.fedId = user.fedId;
            facilityUser.initials = user.initials;
            facilityUser.lastName = user.familyName;
            facilityUser.firstName = user.firstNameKnownAs;
            facilityUser.title = user.title;
            facilityUser.emailAddress = user.email;

            experimenter.facilityUser = facilityUser;

            experimenter.role = Role.local_contact;


            return experimenter;
        }
        public List<Experimenter> getAsExperimenters(UserManager userManager)
        {
            List<Experimenter> experimenters = new List<Experimenter>();

            List<string> userIds = this.getAllValuesForKey("User_Number");
            List<string> roles = this.getAllValuesForKey("Role");

            

            for (int i = 0; i < userIds.Count; i++)
            {
                Experimenter experimenter = new Experimenter();
                FacilityUser facilityUser = new FacilityUser();
                string userNumber = userIds[i];

                isisudb.personDetailsDTO user = userManager.getUserByUserNumber(userNumber);

                facilityUser.facilityUserId = user.userNumber;

                facilityUser.fedId = user.fedId;
                facilityUser.initials = user.initials;
                facilityUser.lastName = user.familyName;
                facilityUser.firstName = user.firstNameKnownAs;
                facilityUser.title = user.title;
                facilityUser.emailAddress = user.email;

                experimenter.facilityUser = facilityUser;

                if ((roles[i] == "Principal Investigator") || (roles[i] == "Principle Investigator"))
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
