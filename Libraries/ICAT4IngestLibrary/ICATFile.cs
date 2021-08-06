using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ICAT4IngestLibrary
{
    /// <summary>
    /// The (abstract) class which represents a file to ingest into ICAT
    /// </summary>
    [Serializable]
    public abstract class ICATFile
    {
        private string filePath;
        private bool inICAT;
        private DateTime createTime;

        public DateTime CreateTime
        {
            get { return createTime; }
        }

        public ICATFile()
        {
        }

        public ICATFile(string filePathIn, bool inICATin)
        {
            filePath = filePathIn;
            inICAT = inICATin;
            createTime = DateTime.Now;
        }

        public ICATFile(XmlReader fileReader)
        {
            /*- <LogFile>
                      <FilePath>\\somepath\somedir\ndxabc\ABC00002_1.log</FilePath> 
                      <CreateTime>03/01/2008 11:36:58</CreateTime> 
                      <InICAT>False</InICAT> 
                </LogFile>
            
                <RawFile>
                      <FilePath>\\somepath\somedir\ndxabc\ABC00001.raw</FilePath> 
                      <CreateTime>03/01/2008 11:36:58</CreateTime> 
                      <InICAT>False</InICAT> 
                </RawFile>
             */

            while (!fileReader.EOF)
            {
                
                fileReader.Read();
                if (fileReader.Name == "FilePath" && fileReader.NodeType == XmlNodeType.Element)
                {
                    fileReader.Read();
                    filePath = fileReader.Value;
                    //Console.WriteLine("   FilePath: " + filePath);
                }
                else if (fileReader.Name == "CreateTime" && fileReader.NodeType == XmlNodeType.Element)
                {
                    fileReader.Read();
                    createTime = DateTime.Parse(fileReader.Value);
                    //Console.WriteLine("   CreateTime: " + createTime);
                }
                else if (fileReader.Name == "InICAT" && fileReader.NodeType == XmlNodeType.Element)
                {
                    fileReader.Read();
                    inICAT = Boolean.Parse(fileReader.Value);
                    //Console.WriteLine("   InICAT: " + inICAT);
                }
            }
            fileReader.Close();
        }

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        public bool InICAT
        {
            get { return inICAT; }
            set { inICAT = value; }
        }

        public void setAsSentToIcat()
        {
            inICAT = true;
        }

        /// <summary>
        /// Returns an XML node representing the ICAT File
        /// </summary>
        /// <returns></returns>
        internal void getXml(ref XmlTextWriter writer)
        {
            writer.WriteStartElement("FilePath");
            writer.WriteString(this.filePath);
            writer.WriteEndElement();

            writer.WriteStartElement("CreateTime");
            writer.WriteString(this.createTime.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("InICAT");
            writer.WriteString(this.inICAT.ToString());
            writer.WriteEndElement();

        }
    }
}
