using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICAT4IngestLibrary
{
    public static class EscapeCharactersHelper
    {
        public static string InsertEscapeCharacters(string theString)
        {
            if (theString == null)
            {
                return theString;
            }
            return theString.Replace("'", "''");
        }
    }
}
