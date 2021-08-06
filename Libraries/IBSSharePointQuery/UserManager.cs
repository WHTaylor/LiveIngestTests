using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBSSharePointQuery.isisudb;

namespace IBSSharePointQuery
{
    public class UserManager
    {
        List<isisudb.personDetailsDTO> users;
        isisudb.UserOfficeWebServiceClient service = new UserOfficeWebServiceClient();
        
        
        public UserManager(string apiKey, List<string> userNumbersFromPropInvs, List<string> userNumbersFromExpTeam)
        {
            foreach (string s in userNumbersFromPropInvs) { userNumbersFromExpTeam.Add(s); }

            List<string> userNumbers = new List<string>();
            foreach (string s in userNumbersFromExpTeam)
            {
                string userNumber = ICATUtilities.removeDecimalPlaces(s);

                if (!userNumbers.Contains(userNumber))
                {
                    userNumbers.Add(userNumber);
                }
            }

            users = service.getPeopleDetailsFromUserNumbers(apiKey, userNumbers.ToArray()).ToList();
        }

        public isisudb.personDetailsDTO getUserByUserNumber(string userNumber)
        {
            userNumber = ICATUtilities.removeDecimalPlaces(userNumber);
            int unAsInt = int.Parse(userNumber);
            return this.getUserByUserNumber(unAsInt);
        }

        public isisudb.personDetailsDTO getUserByUserNumber(int userNumber)
        {
            foreach (isisudb.personDetailsDTO temp in users)
            {                
                int dbUserNum = int.Parse(temp.userNumber);
                if (dbUserNum == userNumber)
                {
                    return temp;
                }
            }
            return null;
        }
    }
}
