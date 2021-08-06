using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBSSharePointQuery
{
    [Serializable()]

    public class XmlPair
    {
        public String Attribute {get; set;}
        public String Value {get; set;}

        public String PrefixedAttibute
        {
            get
            {
                return "ows_" + Attribute;
            }
        }

        public XmlPair(string attribute)
        {
            if (attribute.StartsWith("ows_"))
            {
                Attribute = attribute.Remove(0, 4);
            }
            else
            {
                Attribute = attribute;
            }
        }
    }
}
