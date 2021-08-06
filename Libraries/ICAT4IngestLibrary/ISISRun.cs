using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Security.Cryptography;
using Trinet.Core.IO.Ntfs;

namespace ICAT4IngestLibrary
{
    ///<summary>
    /// represents all the files(raw and logs) and the broken down names and codes for that run
    /// </summary>
    public class ISISRun
    {
        #region regular expressions
        //private static Regex rawFileFilter = new System.Text.RegularExpressions.Regex(@"^.+\.(([rR][aA][wW])|([nN][xX][sS]))$");
        //private static Regex devaRe = new Regex(@"^[0-9]{5}");
        //private static Regex musrRe = new Regex(@"^[a-zA-Z]{4}[0-9]{8}");
        //private static Regex emuRe = new Regex(@"^[a-zA-Z]{3}[0-9]{8}");
        //private static Regex enginxRe = new Regex(@"^[a-zA-Z]{6}[0-9]{8}");
        //private static Regex normalRe = new Regex(@"^[a-zA-Z]{3}[0-9]{5}");
        //private static Regex rawFormatRe = new Regex(@"^[a-zA-Z]{3,4}[0-9]{5,8}\.[rR][aA][wW]");
        #endregion
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private RawFile rawFile;
        private string instrumentShortCode;
        public string instrumentName{get; set;}
        private string runNumber;
        private string cycleDirectory;
        private List<LogFile> logFiles;

        

        /// <summary>
        /// Empty constructor - nessesary for serialization
        /// </summary>
        public ISISRun()
        {
        }

        public ISISRun(RawFile rawFileIn, ISISInstrumentManager instManager)
        {
            this.initialise();
            rawFile = rawFileIn;
            this.setCodesFromFile(rawFileIn.FilePath, instManager);
        }

        public ISISRun(LogFile logFileIn, ISISInstrumentManager instManager)
        {
            this.initialise();
            logFiles.Add(logFileIn);
            this.setCodesFromFile(logFileIn.FilePath, instManager);
        }

        public ISISRun(XmlReader reader)
        {
            this.initialise();
            //Console.WriteLine("New Run");
            reader.Read();
            String key = reader.GetAttribute("key");
            //Console.WriteLine("Key: " + key);

            while (!reader.EOF)
            {
                reader.Read();
                if (reader.Name == "instrumentname" && reader.NodeType == XmlNodeType.Element)
                {
                    reader.Read();
                    instrumentName = reader.Value;
                    //Console.WriteLine("   instrumentname: " + instrumentName);

                }
                if (reader.Name == "instrumentshortcode" && reader.NodeType == XmlNodeType.Element)
                {
                    reader.Read();
                    instrumentShortCode = reader.Value;
                    //Console.WriteLine("   instrumentshortcode: " + instrumentShortCode);

                }
                else if (reader.Name == "runnumber" && reader.NodeType == XmlNodeType.Element)
                {
                    reader.Read();
                    runNumber = reader.Value;
                    //Console.WriteLine("   runNumber: " + runNumber);
                }
                else if (reader.Name == "RawFile" && reader.NodeType == XmlNodeType.Element)
                {
                    System.Xml.XmlReader rawSubReader = reader.ReadSubtree();
                    RawFile raw = new RawFile(rawSubReader);
                    this.rawFile = raw;
                }
                else if (reader.Name == "LogFiles" && reader.NodeType == XmlNodeType.Element)
                {
                    System.Xml.XmlReader logsSubReader = reader.ReadSubtree();
                    /*logsSubReader is of the form
                      <LogFiles>
                          <LogFile>
                              <FilePath>\\somepath\somedir\ndxabc\ABC00003_1.log</FilePath> 
                              <CreateTime>03/01/2008 11:36:58</CreateTime> 
                              <InICAT>False</InICAT> 
                          </LogFile>
                      </LogFiles>
                      
                     so we need to further break it down into LogFile elements each with a reader
                      
                     */
                    while (!logsSubReader.EOF)
                    {
                        logsSubReader.Read();
                        if (logsSubReader.Name == "LogFile" && logsSubReader.NodeType == XmlNodeType.Element)
                        {
                            System.Xml.XmlReader logSubReader = reader.ReadSubtree();
                            LogFile log = new LogFile(logSubReader);
                            this.logFiles.Add(log);
                        }
                    }
                }
            }
            reader.Close();

        }

        public void getXML(ref XmlTextWriter writer)
        {
            /*
             *- <run key="EVS12443">
                <instrumentname>EVS</instrumentname> 
                <instrumentshortcode>EVS</instrumentshortcode> 
                <runnumber>12443</runnumber> 
             */
            writer.WriteStartElement("ISISRun");

            writer.WriteAttributeString("key", this.InstrumentShortCode + this.RunNumber);
            writer.WriteElementString("instrumentname", this.InstrumentName);
            writer.WriteElementString("instrumentshortcode", this.InstrumentShortCode);

            writer.WriteElementString("runnumber", this.RunNumber);


            //get RAW file, then log files
            if (this.rawFile != null)
            {
                this.rawFile.getXml(ref writer);
            }

            writer.WriteStartElement("LogFiles");
            foreach (LogFile lf in logFiles)
            {
                lf.getXml(ref writer);
            }
            writer.WriteEndElement();

            //end ISISRun
            writer.WriteEndElement();


        }

        /// <summary>
        /// Simply initialise the List<LogFile>
        /// </summary>
        private void initialise()
        {
            logFiles = new List<LogFile>();
        }

        /// <summary>
        /// From the file (raw or log) gets the instrument code, instrument name and run number, and sets these
        /// </summary>
        /// <param name="filePath">The raw or log file path</param>
        private void setCodesFromFile(string filePath, ISISInstrumentManager instManager)
        {
            instrumentShortCode = instManager.getInstrumentCode(filePath);
            instrumentName = instManager.getFullNameFromCode(instrumentShortCode);
            runNumber = instManager.getRunNumber(filePath);
            cycleDirectory = instManager.getCycleDirectory(filePath);
        }

        /// <summary>
        /// Checks if all the log files and all the raw files for this run are in ICAT
        /// </summary>
        /// <returns></returns>
        public bool allSent()
        {
            if (!rawFile.InICAT)
            {
                return false;
            }
            foreach (LogFile lf in logFiles)
            {
                if (!lf.InICAT)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Method to do a quick scan of directory with the log/raw file in to check for any log files we missed the trigger for
        /// </summary>
        public void updateWithAnyMissedLogFiles(string currentCycle, ISISInstrumentManager instManager)
        {
            try
            {
                string searchPath = getDirectoryForThisRun();
                if (searchPath == "")
                {
                    logger.Info("Error running updateWithAnyMissedLogFiles: seachPath is null");
                    return;
                }
               

                string baseName = this.InstrumentShortCode + this.RunNumber + "*";

                string[] files = Directory.GetFiles(searchPath, baseName);
                foreach (string file in files)
                {
                    //check it's not a rubbish file
                    
                    if (instManager.isRubbishFile(file))
                    {
                        //if it is the raw file, see if we have a raw file
                        string extension = Path.GetExtension(file).ToUpper();
                        if (extension.Equals(".RAW") || extension.Equals(".NXS"))
                        {
                            if (this.rawFile == null)
                            {
                                this.rawFile = new RawFile(file, false);
                            }
                        }
                        else
                        {
                            //must be a log file. see if it is already in the list
                            bool isInList = false;
                            foreach (LogFile lf in this.LogFiles)
                            {
                                if (lf.FilePath == file)
                                {
                                    isInList = true;
                                }
                            }
                            if (!isInList)
                            {
                                //new log file - add it to the list
                                logger.Info("Adding missed log file: " + file + " to " + this.instrumentShortCode + this.runNumber);
                                this.LogFiles.Add(new LogFile(file, false));
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                logger.Info("Error updateWithAnyMissedLogFiles: " + exp.Message);
                return;
            }
        }

        /// <summary>
        /// Method to get the directory the files for this run are in
        /// </summary>
        /// <returns>The directory containing the files for this run</returns>
        public string getDirectoryForThisRun()
        {
            string searchPath = "";
            if (this.rawFile != null)
            {
                searchPath = Path.GetDirectoryName(this.rawFile.FilePath);
            }
            else
            {
                searchPath = Path.GetDirectoryName(this.LogFiles[0].FilePath);
            }
            if (searchPath == "")
            {
                logger.Info("Error getting seachPath : is null");
            }
            return searchPath;
        }

        public RawFile RawFile
        {
            
            get { return rawFile; }
            set { rawFile = value; }
        }

        public string InstrumentShortCode
        {
            get { return instrumentShortCode; }
        }

        public string InstrumentName
        {
            get { return instrumentName; }
        }

        public string RunNumber
        {
            get { return runNumber; }
        }

        public string CycleDirectory
        {
            get { return cycleDirectory; }
        }

        public List<LogFile> LogFiles
        {
            get { return logFiles; }
            set { logFiles = value; }
        }

        public string getParentRefName()
        {
            //CSP-00079000
            int paddingZeros = 8 - runNumber.Length;
            string paddedNumber = "";
            for (int i = 0; i < paddingZeros; i++)
            {
                paddedNumber = paddedNumber + "0";
            }
            paddedNumber = paddedNumber + runNumber;
            string thousand = paddedNumber.Substring(0, paddedNumber.Length - 3) + "000";
            return instrumentShortCode + "-" + thousand;
        }

        /// <summary>
        /// Reads the checksum values of the files in the run from the alternate data stream of the .RAW file.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> getChecksums()
        {
            string name = "";

            if (Path.GetExtension(this.RawFile.FilePath).ToUpper().Equals(".RAW")) //Rawfile has .raw extension
            {
                name = this.RawFile.FilePath;
            }
            else // rawfile has .nxs extension
            {
                foreach (LogFile f in this.LogFiles) //find .raw file for run
                {
                    if (Path.GetExtension(f.FilePath).ToUpper().Equals(".RAW"))
                    {
                        name = f.FilePath;
                        break;
                    }
                }
            }

            if (name.Equals(""))
            {
                logger.Error(".Raw file not found for run " + this.InstrumentName + this.RunNumber + "- checksum comparison cannot be made");
                return new Dictionary<string,string>();
            }

            Dictionary<string, string> checksumStrings = new Dictionary<string, string>();

            FileInfo file = new FileInfo(name);

            if (file.AlternateDataStreamExists("checksum")) //Raw file has checksum data stream
            {
                AlternateDataStreamInfo s = file.GetAlternateDataStream("checksum", FileMode.Open);
                using (TextReader reader = s.OpenText())
                {
                    while (reader.Peek() != -1) //not end of stream
                    {
                        /**
                         * Line is data stream will take the format :
                         * [file checksum] *[file name]
                         **/
                        string temp = reader.ReadLine();
                        string[] fileAndChecksum = temp.Split(' ');
                        string fileName = fileAndChecksum[1].Substring(1).ToUpper();
                        string checksum = fileAndChecksum[0].ToUpper();

                        if (fileAndChecksum.Length == 2)
                        {
                            if (!checksumStrings.ContainsKey(fileName))
                            {
                                checksumStrings.Add(fileName, checksum);
                            }
                            else
                            {
                                if (checksumStrings[fileName].Equals(checksum))
                                {
                                    //Checksums match, ignore duplicates
                                }
                                else
                                {
                                    logger.Warn("Multiple checksum values for file: " + fileName + " " + checksumStrings[fileName] + " & " + checksum);
                                    logger.Warn("Making a guess and using first checksum value");
                                }
                            }
                        }
                        else
                        {
                            logger.Error("Unrecognised data stream " + fileAndChecksum);
                        }
                    }
                }
            }
            else
            {
                logger.Error("Checksum alternate data stream not found for " + file.FullName);
            }

            return checksumStrings;
        }
    }
}
