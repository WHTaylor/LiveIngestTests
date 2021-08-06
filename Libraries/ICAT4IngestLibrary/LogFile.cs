using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ICAT4IngestLibrary
{
    /// <summary>
    /// Represents a log file to igest into ICAT
    /// </summary>
    public class LogFile : ICATFile
    {
        public LogFile(string filePathIn, bool inICATin)
            : base(filePathIn, inICATin)
        {
        }

        public LogFile()
            : base()
        {
        }

        public LogFile(XmlReader fileReader)
            : base(fileReader)
        {
        }


        internal new void getXml(ref XmlTextWriter writer)
        {
            writer.WriteStartElement("LogFile");
            base.getXml(ref writer);
            writer.WriteEndElement();
        }

    }
}
