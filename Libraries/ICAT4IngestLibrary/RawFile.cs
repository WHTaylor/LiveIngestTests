using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ICAT4IngestLibrary
{
    /// <summary>
    /// Represents a Raw file to igest into ICAT
    /// </summary>
    public class RawFile : ICATFile
    {
        public RawFile(string filePathIn, bool inICATin)
            : base(filePathIn, inICATin)
        {
        }
        public RawFile()
            : base()
        {
        }
        public RawFile(XmlReader fileReader)
            : base(fileReader)
        {
        }

        /// <summary>
        /// Returns an XML node representing the ICAT File
        /// </summary>
        /// <returns></returns>
        internal new void getXml(ref XmlTextWriter writer)
        {
            writer.WriteStartElement("RawFile");
            base.getXml(ref writer);
            writer.WriteEndElement();
        }
    }
}
