using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Diagnostics;
using ICAT4IngestLibrary.ActiveMQ;

namespace ICAT4IngestLibrary
{
    /// <summary>
    /// Static class which is responsible for talking to nxingest and writeRaw, 
    /// verifying XML files and sending things to ICAT
    /// </summary>
    /// 
    public class ICATIngestor
    {
        //these are set here, but should be overriden when the constructor is called.
        /// <summary>
        /// The directory we will create the xml files in
        /// </summary>

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool testMode = false;

        private string writeRawProgram = @"C:\ActiveIngest\writeRaw.exe";
        private string nxIngestProgram = @"C:\ActiveIngest\nxingest.exe";
        private string mappingFilesDir = @"C:\ActiveIngest\mapping\";
        private string fileProtocolPath = @"file://S:/";
        private bool trustedValue = true;
        private bool overrideTrusted = false;
        private string currentCycle = @"\CYCLE_15_4";
        private readonly AMQClient amqClient;
        
        ISISInstrumentManager instManager;        

        public string getWriteRawProgram()
        {
            return writeRawProgram;

        }

        public string getNxIngestProgram()
        {

            return nxIngestProgram;

        }

        public string getMappingFilesDir()
        {

            return mappingFilesDir;

        }

        public string getFileProtocolPath()
        {

            return fileProtocolPath;

        }

        public bool getOverrideTrusted()
        {

            return overrideTrusted;

        }

        public bool getTrustedValue()
        {

            return trustedValue;

        }

        public string getCurrentCycle()
        {
            return currentCycle;
        }

        /// <summary>
        /// ICATIngestor extracts metadata from experimental data files into XMLs and notifies the ICAT queue
        /// so the data can be copied into ICAT
        /// </summary>
        /// <param name="xmlFileIn">The location of the xml configuration for the ingestor</param>
        /// <param name="instManager">An instance of ISISInstrumentManager to handle instrument configuration</param>
        /// <param name="amqClient">An instance of AMQClient which must be ProducingTo ICAT, ICATLong, and PreICATError</param>
        public ICATIngestor(string xmlFileIn, ISISInstrumentManager instManager, AMQClient amqClient)
        {
            this.instManager = instManager;
            this.amqClient = amqClient;

            XmlDocument doc = new XmlDocument();

            //open the xml file
            try
            {
                doc.Load(xmlFileIn);
            }
            catch (Exception e)
            {
                logger.Error("Ingestor control XML file does not exist at location : " + xmlFileIn);
                Environment.Exit(-1);
            }

            XmlNodeList nodeList = null;

            try
            {
                nodeList = doc.DocumentElement.ChildNodes;
            }
            catch (Exception e)
            {
                logger.Error("Ingestor control file children nodes not found");
                Environment.Exit(-1);
            }

            foreach (XmlNode node in nodeList)
            {
                switch (node.Name)
                {
                   
                    case "writeRawProgram":
                        logger.Info("writeRawProgram: " + node.InnerText.Trim());
                        writeRawProgram = node.InnerText.Trim();
                        break;
                    case "nxIngestProgram":
                        logger.Info("nxIngestProgram: " + node.InnerText.Trim());
                        nxIngestProgram = node.InnerText.Trim();
                        break;
                    case "mappingFilesDir":
                        logger.Info("mappingFilesDir: " + node.InnerText.Trim());
                        mappingFilesDir = node.InnerText.Trim();
                        break;
                    case "mappingFile":
                        logger.Info("mappingFile: " + node.InnerText.Trim());
                        //mappingFile = node.InnerText.Trim();
                        break;
                    case "fileProtocolPath":
                        logger.Info("fileProtocolPath: " + node.InnerText.Trim());
                        fileProtocolPath = node.InnerText.Trim();
                        break;
                    case "OverrideTrusted":
                        logger.Info("OverrideTrusted: " + node.InnerText.Trim());
                        overrideTrusted = bool.Parse(node.InnerText.ToString());
                        break;
                    case "TrustedValue":
                        logger.Info("TrustedValue: " + node.InnerText.Trim());
                        trustedValue = bool.Parse(node.InnerText.ToString());
                        break;
                    case "CurrentCycle":
                        logger.Info("CurrentCycle: " + node.InnerText.Trim());
                        currentCycle = node.InnerText.Trim();
                        break;
                }
            }
        }

        public Tuple<Boolean,List<string>> ingestFileSet(String fullPath)
        {           
            List<string> runFilePaths = instManager.getOtherRunFilesFromRawOrNexus(fullPath);
            string runNumber = instManager.getRunNumber(fullPath);
            string instrumentCode = instManager.getInstrumentCode(fullPath);
            return handleFile(fullPath, instrumentCode, runNumber, runFilePaths);
        }

        /// <summary>
        /// Takes the xml file path and sends to the ICAT queue which will be recived by the xml file
        /// </summary>
        /// <param name="xmlPath">path to xml file</param>
        private void sendToICAT(string xmlPath)
        {
            try
            {
                logger.Info("Successfuly parsed to ICAT Queue: " + xmlPath);
                amqClient.SendTo(MessageQueue.ICAT, xmlPath);
            }
            catch (Exception e)
            {
                logger.Error("Unable to send to the ICAT queue.", e);
            }
        }

        /// <summary>
        /// Takes the xml file path and sends to the long queue where it will be re-processed later.
        /// </summary>
        /// <param name="filePath">path to nexus/raw file</param>
        private void sendToLong(string filePath)
        {
            try
            {
                amqClient.SendTo(MessageQueue.ICATLong, filePath);
                logger.Info("Successfuly parsed to Long Queue: " + filePath);
            }
            catch (Exception e)
            {
                logger.Error("Unable to send to the Long queue. ", e);
            }
        }

        /// <summary>
        /// Takes the path name of the raw or nxs file and adds it to the error queue.
        /// </summary>
        /// <param name="filePath">path to .raw or .nxs file</param>
        private void sendToError(string filePath)
        {
            try
            {
                logger.Warn("Issue with file: " + filePath + ". Adding to error queue.");
                amqClient.SendTo(MessageQueue.PreICATError, filePath);
            }
            catch (Exception e)
            {
                logger.Error("Unable to send to the Error queue. ", e);
            }
        }

        /// <summary>
        /// Method to take a RAW or NXS file from the consumer time and pass it to the appropriate handler
        /// </summary>
        /// <param name="filePath">The full name and path of the file to parse</param>
        /// <param name="instrumentShortCode">The instrument short code for the instrument</param> 
        /// <param name="runNumber">the run number for the run</param>
        public Tuple<Boolean,List<String>> handleFile(string filePath, string instrumentShortCode, string runNumber, List<string> logFiles)
        {
            Boolean successfulIngestion = true;
            //25/10/2013 - some extra code to handle the case where the nxs file has been set as the raw file for a raw instrument
            if (Path.GetExtension(filePath).ToUpper() == ".NXS")
            {
                if (!instManager.isNexusInstrument(Path.GetFileName(filePath)))
                {
                    logger.Info("Nexus file for a RAW instrument - changing");

                    List<String> logFilesIterate = new List<String>();
                    foreach (string lf in logFiles)
                    {
                        logFilesIterate.Add(lf);
                    }

                    //we have a nexus file for a non-nexus instrument
                    //check if the raw file is in as a log file
                    foreach (string lf in logFilesIterate)
                    {
                        if (Path.GetExtension(lf).ToUpper() == ".RAW")
                        {
                            logger.Info("Nexus file for a RAW instrument - removing raw file from logs");
                            logFiles.Remove(lf);
                        }
                    }
                    //add the nexus file as a log file
                    logger.Info("Nexus file for a RAW instrument - adding nxs as log file");
                    logFiles.Add(filePath);
                    //rename the .nxs 'raw' file to .raw
                    logger.Info("Nexus file for a RAW instrument - setting .raw as raw file");
                    filePath = filePath.Replace(".nxs", ".raw");
                    filePath = filePath.Replace(".NXS", ".RAW");
                }
            }

            //decode which type of file (RAW or NEXUS) and invoke the appropriate handler
            string extension = Path.GetExtension(filePath);
            if (extension.ToUpper().Equals(".RAW"))
            {
                if(!instManager.isNexusInstrument(Path.GetFileName(filePath))){
                    //invoke the raw file handler - remember to ignore 0KB Muon files 
                    successfulIngestion = this.handleRawFile(filePath, instrumentShortCode, runNumber, logFiles, instManager);
                }
            }
            else if (extension.ToUpper().Equals(".NXS"))
            {
                //invoke the nexus handler
                successfulIngestion = this.handleNexusFile(filePath, instrumentShortCode, runNumber, logFiles, instManager);
            }
            return new Tuple<Boolean,List<String>>(successfulIngestion,logFiles);
                        
        }

        /// <summary>
        /// Method to invoke writeRaw on a raw file, call the validator, then pass it and any log files to ICAT
        /// </summary>
        /// <param name="e">The RAW File</param>
        private Boolean handleRawFile(string filePath, string instrumentShortCode, string runNumber, List<string> logFiles, ISISInstrumentManager instManager)
        {

            
            //writeRaw Usage: Program Directory InstrumentID rawFileName
            string directory = Path.GetDirectoryName(filePath) + "\\";
            string destinationDir = Path.GetDirectoryName(writeRawProgram) + "\\xmls\\";
            //check the instrument id is not a muon instrument

            //get the file numbers to use
            
            //get the instrument
            if (!filePath.Contains(runNumber))
            {
                logger.Info("Long Run number is not in filename - cutting down to short");
                runNumber = runNumber.Substring(3);
            }
            
            //make sure we are using the correct short code for the RAW file
            string instrumentShortCodeToUse = "";
            foreach (string shortCode in instManager.getAllInstrumentCodes(instrumentShortCode))
            {
                string pathPart = shortCode + runNumber;
                if (filePath.ToUpper().Contains(pathPart.ToUpper()))
                {
                    instrumentShortCodeToUse = shortCode;
                }
            }

            if (instrumentShortCodeToUse == "")
            {
                logger.Info("Error getting correct short code to use: shortCode: " + instrumentShortCode + " file " + filePath);
                sendToError(filePath);
            }


            string arguments = " " + directory + " " + instrumentShortCodeToUse + " " + runNumber + " " + runNumber;
            //logWriter.WriteLine();
            //logWriter.WriteLine(writeRawProgram + " " + arguments);
            //logWriter.WriteLine();

            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = destinationDir;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = writeRawProgram;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;

            logger.Info("Arguments: " + arguments);

            try
            {
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception e)
            {
                logger.Error("missing writeRaw executable at path : " + writeRawProgram);
                sendToError(filePath);
                return false;
            }

            if (proc.ExitCode != 0)
            {
                string error = proc.StandardError.ReadToEnd();
                error = error.Replace(",", "-");
                error = error.Replace("\n", "");
                error = error.Trim();
                string output = proc.StandardOutput.ReadToEnd();
                output = output.Replace(",", "-");
                output = output.Replace("\n", "");
                output = output.Trim();


                logger.Info("Doing file: " + filePath);
                logger.Info("Exit code: " + proc.ExitCode);
                logger.Info(error);
                logger.Info(output);
                //logWriter.Close();
                sendToError(filePath);
                return false;
            }
            else
            {
                string outputFile = null;
                try
                {
                    outputFile = destinationDir + "\\" + instrumentShortCodeToUse + runNumber + ".xml";

                    //modify the file name to match what it really is - writeRaw populates this with the RAW file, which may not
                    //give the right answer - it is locked to ABC12345, so breaks for ENGINX12345678 and any other new instruments

                    string realFileName = Path.GetFileName(filePath);
                    GC.Collect();
                    modifyDataFileName(outputFile, realFileName);

                    //some instruments have changed names and the files need to be re-written with this new name
                    //for example EVS is now VESUVIO, but the files still say EVS.
                    if (instrumentShortCodeToUse == "EVS")
                    {
                        //set it to VESUVIO
                        modifyInstrumentName(outputFile, "EVS", "VESUVIO");
                    }

                    //if there are any logs files, then wrap them up too
                    addLogFilesToXML(outputFile, logFiles, false);

                    //do schema validation and send to ICAT

                    if (validateXMLFile(outputFile) != ICATError.OK)
                    {
                        logger.Error("Validation has failed for: " + outputFile);
                        sendToError(filePath);
                        return false;
                    }
                   
                }
                catch (Exception e)
                {
                    logger.Error("Issue with file " + outputFile+" the following error has occured: "+e.Message);
                    logger.Info("Sending to error queue.");
                    sendToError(filePath);
                    return false;
                }

                //upload to ICAT

                sendToICAT(outputFile);

                return true;

            }
        }

        /// <summary>
        /// Method to invoke nxIngest on a nexus file, call the validator, then pass it and any log files to ICAT
        /// </summary>
        /// <param name="e">The Nexus File</param>
        private Boolean handleNexusFile(string filePath, string instrumentShortCode, string runNumber, List<string> logFiles, ISISInstrumentManager instManager)
        {
            //usage is: nxingest.exe  mapping_file  nexus_file  output_xml_file
            string destinationDir = Path.GetDirectoryName(nxIngestProgram);
            string mappingFileToUse = mappingFilesDir + instManager.getMappingFile(instrumentShortCode); ;
            string nexusFile = filePath;
            string fileName = Path.GetFileName(filePath);
            string xmlFileName = fileName.Substring(0, fileName.Length - 4);
            string outputFile = destinationDir + "\\xmls\\" + xmlFileName + ".xml";

            string arguments = mappingFileToUse + " " + "\"" + nexusFile + "\" " + outputFile;
            logger.Info("Arguments: " + arguments);

            Process proc = new Process();
            
          try
            {
                proc.StartInfo.WorkingDirectory = destinationDir;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.FileName = nxIngestProgram;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;

                proc.Start();
                GC.Collect();
                if (!proc.WaitForExit(60000))
                {
                    proc.Kill();                    
                    logger.Warn("Nxingest has taken too long. Sending to long queue to be re-processed at a later date.");
                    sendToLong(filePath);
                    //Do not want to continue as it has been sent.
                    return false;      
                }
            }
            catch (Exception e)
            {
                
                logger.Error("Nxingest error : " +e.Message);
                proc.Kill();
                return false;
               
            }
            


            if (proc.ExitCode != 0)
            {
                string error = proc.StandardError.ReadToEnd();
                error = error.Replace(",", "-");
                error = error.Replace("\n", "");
                error = error.Trim();
                string output = proc.StandardOutput.ReadToEnd();
                output = output.Replace(",", "-");
                output = output.Replace("\n", "");
                output = output.Trim();


                logger.Info("Doing file: " + filePath);
                logger.Info("Exit code: " + proc.ExitCode);
                logger.Info(error);
                logger.Info(output);
                //logWriter.Close();
                sendToError(filePath);
                return false;
                
            }     
            else
            {
                try
                {
                    //if there are any logs files, then wrap them up too
                    addLogFilesToXML(outputFile, logFiles, true);
                }
                catch (Exception e)
                {
                    logger.Error("Error occurred when adding log files. Sending to error queue " + e.Message);
                    sendToError(filePath);
                    return false;
                }
                
                //do schema validation then send it to ICAT
                if (validateXMLFile(outputFile) != ICATError.OK)
                {
                    logger.Info("Validation has failed for: " + outputFile + " trying to rectify the error...");
                    rectifyFile(outputFile);
                    if (validateXMLFile(outputFile) != ICATError.OK)
                    {
                        logger.Error("Validation has failed for: " + outputFile);
                        sendToError(filePath);
                        return false;
                    }
                    else
                    {
                        logger.Info("Succesffully rectified the problems with the file. Sending to ICAT");
                        sendToICAT(outputFile);
                    }
                }
                else
                {
                    sendToICAT(outputFile);
                }
            }

            return true;
        }

        private void rectifyFile(string outputFile)
        {
            // This is usually triggered for very old files where certain attributes aren't set correctly
            // Find out if the file has a run number. If it doesn't, we will need to add it later
            string contents = File.ReadAllText(outputFile);
            Boolean isRunNumber = false;
            if (contents.Contains("<name>run_number</name>"))
            {
                isRunNumber = true;
            }
            using (StreamReader sre = new StreamReader(outputFile))
            {
                using (StreamWriter sw = new StreamWriter(outputFile + ".temp"))
                {
                    while (!sre.EndOfStream)
                    {
                        string line = sre.ReadLine();
                        // Set the investigation number to be the default (0) if it doesn't already exist 
                        if (line.Contains("<investigation trusted="))
                        {
                            string nextLine = sre.ReadLine();
                            if (!nextLine.Contains("<inv_number>"))
                            {
                                line = line + "\r\n" + "<inv_number>0</inv_number>" + "\r\n" + nextLine;
                            }
                            else
                            {
                                line = line + "\r\n" + nextLine;
                            }
                        }
                        // Solves an issue with MUSR files when the standard deviation can't be calculated
                        if (line.Contains("<name>temperature_std</name>"))
                        {
                            string nextLine = sre.ReadLine();
                            if (nextLine.Contains("#IND"))
                            {
                                string replacement = "<numeric_value>1</numeric_value>";
                                line = line + "\r\n" + replacement;
                            }
                            else
                            {
                                line = line + "\r\n" + nextLine;
                            }
                        }
                        // Add the run number if not already in the file
                        if (!isRunNumber && line.Contains("<file_size>"))
                        {
                            string run = Path.GetFileNameWithoutExtension(outputFile);
                            run = Regex.Replace(run, "[^0-9.]", "");
                            run = run.TrimStart(new Char[] { '0' });
                            if (run.Length == 0)
                            {
                                run = "0";
                            }
                            line = line + "\r\n" + "<parameter>" + "\r\n" + "<name>run_number</name>" + "\r\n" + "<numeric_value>" + run + "</numeric_value>" + "\r\n" + "<units>N/A</units>" + "\r\n" + "</parameter>";
                        }
                        sw.WriteLine(line);
                    }
                    sw.Close();
                    sre.Close();
                    File.Delete(outputFile);
                    // Write the information to the correct file
                    File.Move(outputFile + ".temp", outputFile);
                }
            }
        }

        /// <summary>
        /// Checks the XML file is valid according to the schema
        /// </summary>
        /// <param name="xmlFilePath">The xml file to validate</param>
        /// <returns>"Valid" of valid, errors if not</returns>
        private ICATError validateXMLFile(string xmlFilePath)
        {
            Validator v = new Validator();
            string response = v.runValidator(xmlFilePath);
            if (response.ToUpper() != "VALID")
            {
                return ICATError.XMLValidationError;
            }
            else
            {
                return ICATError.OK;
            }
        }

        /// <summary>
        /// Adds the log files passed as an argument to the xml file as datafiles
        /// </summary>
        /// <param name="xmlFilePath">The xml file to change</param>
        /// <param name="logFiles">the logs files to add as datafiles</param>
        private void addLogFilesToXML(string xmlFilePath, List<string> logFiles, bool nexusFile)
        {
            using (StreamReader sre = new StreamReader(xmlFilePath))
            {

                using (StreamWriter sw = new StreamWriter(xmlFilePath + ".temp"))
                {

                    StringBuilder newXML = new StringBuilder();

                    foreach (string s in logFiles)
                    {
                        //get the create date and modify date, and the size
                        /*
                            <datafile_create_time>2006-11-10T04:48:00</datafile_create_time> 
                            <datafile_modify_time>2006-11-10T05:14:42</datafile_modify_time> 
                            <file_size>5267456</file_size> 
                            */
                        FileInfo fi = new FileInfo(s);
                        string createTime = fi.CreationTime.Date.ToString("yyyy-MM-ddT") + fi.CreationTime.Hour.ToString("00") + fi.CreationTime.Minute.ToString(":00") + fi.CreationTime.Second.ToString(":00.0");
                        string modifyTime = fi.LastWriteTime.Date.ToString("yyyy-MM-ddT") + fi.CreationTime.Hour.ToString("00") + fi.CreationTime.Minute.ToString(":00") + fi.CreationTime.Second.ToString(":00.0");
                        string fileSize = fi.Length.ToString();

                        newXML.AppendLine("<datafile>");
                        newXML.AppendLine("<name>" + Path.GetFileName(s) + "</name>");
                        newXML.AppendLine("<location>" + getLinuxMountPath(s) + "</location>");
                        newXML.AppendLine("<datafile_create_time>" + createTime + "</datafile_create_time>");
                        newXML.AppendLine("<datafile_modify_time>" + modifyTime + "</datafile_modify_time>");
                        newXML.AppendLine("<file_size>" + fileSize + "</file_size>");
                        newXML.AppendLine("</datafile>");
                    }

                    string line;

                    while (!sre.EndOfStream)
                    {
                        line = sre.ReadLine();
                        string printableLine = "";
                        bool removed = false;
                        foreach (char c in line)
                        {
                            if (c > 127 || c < 32)
                            {
                                logger.Info("Error Non printable character in line: " + line + " of: " + xmlFilePath);
                                //removed = true;
                                printableLine = printableLine + c;
                            }
                            else
                            {
                                printableLine = printableLine + c;
                            }

                        }
                        line = printableLine;
                        if (removed && line.Length == 0)
                        {
                            line = "Non_ascii";
                        }

                        if (overrideTrusted)
                        {
                            if (line.Contains("<investigation trusted="))
                            {
                                //replace datafile with "</datafile> + ALL OUR NEW XML"
                                if (trustedValue)
                                { //is true
                                    string newLine = line.Replace("<investigation trusted=\"false\">", "<investigation trusted=\"true\">");
                                    line = newLine;
                                }
                                else
                                { //is false
                                    string newLine = line.Replace("<investigation trusted=\"true\">", "<investigation trusted=\"false\">");
                                    line = newLine;
                                }

                            }
                        }

                        if (line.Contains(@"</datafile>"))
                        {
                            //replace datafile with "</datafile> + ALL OUR NEW XML"

                            string newLine = line.Replace("</datafile>", "</datafile>" + newXML);
                            line = newLine;
                        }
                        if (line.Contains(@"<location>"))
                        {
                            line = getLinuxMountPath(line);
                        }

                        //just write it out
                        sw.WriteLine(line);
                    }
                    sw.Close();
                    sre.Close();
                    File.Delete(xmlFilePath);
                    File.Move(xmlFilePath + ".temp", xmlFilePath);
                }
            }
        }

        /// <summary>
        /// Modifies the XML file from writeRaw to correct the datafile name
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <param name="realDataFileName"></param>
        private void modifyDataFileName(string xmlFilePath, string realDataFileName)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFilePath);
                XmlNode node = doc.SelectSingleNode("//icat/study/investigation/dataset/datafile/name");
                node.InnerXml = realDataFileName;
                doc.Save(xmlFilePath);
                doc = null;
                GC.Collect();
            }
            catch (Exception e)
            {
                logger.Info("Error loading XML file: " + xmlFilePath + " : " + e.Message);
            }
        }

        /// <summary>
        /// Modifies the XML to reset the instrument name
        /// </summary>
        /// <param name="xmlFilePath">the file to modify</param>
        /// <param name="oldInstrumentName">the instrument's old name</param>
        /// <param name="neewInstrumentName">the instrument's new name</param>
        private void modifyInstrumentName(String xmlFilePath, string oldInstrumentName, string newInstrumentName)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFilePath);
                XmlNode node = doc.SelectSingleNode("//icat/study/investigation/instrument");
                if (node.InnerXml == oldInstrumentName)
                {
                    node.InnerXml = newInstrumentName;
                    doc.Save(xmlFilePath);
                }

                else
                {
                    logger.Info("Error renaming instrument in XML file: " + xmlFilePath + " : expected " + oldInstrumentName + " but found " + node.InnerXml);
                }
                 doc = null;
                 GC.Collect();
            }
            catch (Exception e)
            {
                logger.Info("Error loading XML file: " + xmlFilePath + " : " + e.Message);
            }
        }


        /// <summary>
        /// Turns a DSF (\\isis\inst$\NDX....) path into a minux mount path (file:///mnt/isidata/NDX...)
        /// </summary>
        /// <param name="filePath">The path to change</param>
        /// <returns>The new path</returns>
        private string getLinuxMountPath(string filePath)
        {
            //this style is no longer wanted - we want \\isis\inst$\Instruments\NDX......\
            /*
            //filePath will be like \\isis\inst$\NDXALF\Instrument\data\cycle_07_1\ALF13810.RAW
            //want it to look like: file:///mnt/isisdata/NDXALF/Instrument/data/cycle_07_1/ALF13810.RAW
            //1) replace \ with /
            filePath = filePath.Replace('\\', '/');
            //2) replace //isis/inst$ with file:///mnt/isisdata (in a variable called fileProtocolPath)
            filePath = filePath.Replace(@"//isisdatar55/inst$/", fileProtocolPath);
            filePath = filePath.Replace(@"C:/", fileProtocolPath);
            return filePath;
             */



            /*new style: take 
            \\isisdatar55\Cycles$\cycle_10_3\NDXHIFI\somefile.raw and turn it into
            \\isis\inst$\Instruments$\NDXHIFI\Instrument\data\cycle_10_3\somefile.raw
            
            */

            filePath = filePath.Replace(@"\/", "\\");
            return filePath;

            //NOTICE: Code is unreachable?
            try
            {

                string oldPath = @"\\isisdatar55\Cycles$\cycle_10_3\";
                string newPath = @"\\isis\inst$\Instruments$\";

                string newFilePath = filePath.Replace('/', '\\');

                newFilePath = newFilePath.Replace(oldPath, newPath);

                //now looks like \\isis\inst$\Instruments$\NDXHIFI\somefile.raw

                int lastSlash = newFilePath.LastIndexOf(@"\");

                newFilePath.Insert(lastSlash, @"\" + currentCycle + @"\");

                return newFilePath;
            }
            catch (Exception exp)
            {
                logger.Info("Error changing file path: " + filePath + exp.Message);
            }
            return filePath;

        }

    }

}
