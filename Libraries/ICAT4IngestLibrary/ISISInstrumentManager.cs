using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;


namespace ICAT4IngestLibrary
{
    public class ISISInstrumentManager
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //List<ISISInstrument> instruments;
        public Dictionary<List<string>, ISISInstrument> instruments;

        public ISISInstrumentManager(string xmlFilePath)
         {
            instruments = new Dictionary<List<string>, ISISInstrument>();

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(xmlFilePath);
            }
            catch (Exception e)
            {
                logger.Error("Instruments xml file not found at " + xmlFilePath, e);
                Environment.Exit(-1);
            }

            XmlNodeList nodes = null;

            try
            {
                nodes = doc.SelectNodes("//ISISFiles/instrument");
            }
            catch (Exception e)
            {
                logger.Error("Instruments xml file at " + xmlFilePath + "has an invalid layout");
                Environment.Exit(-1);
            }

            string mappingFile = null;
            foreach (XmlNode node in nodes)
            {
                ISISInstrument inst = new ISISInstrument(node);
                mappingFile = inst.MappingFile;
                instruments.Add(inst.ShortCodes, inst);
            }
        }

        /// <summary>
        /// Get an instrument by name
        /// </summary>
        /// <param name="instName">Instrument name</param>
        /// <returns>ISIS Instrument</returns>
        public ISISInstrument getInstrumentByName(string instrumentName)
        {
            string name = validInstrumentName(instrumentName);
            foreach (ISISInstrument ins in instruments.Values)
            {
                // Check the long name
                if (name.Equals(ins.LongName.ToUpper().Trim()))
                {
                    return ins;
                }
                // Check the set of short names
                foreach (string sCode in ins.ShortCodes)
                {
                    if (name.Equals(sCode.ToUpper().Trim()))
                    {
                        return ins;
                    }
                }
            }
            logger.Error("Unable to find instrument by name: " + name);
            return null;
        }

        /// <summary>
        /// Method to decide if a given file is of interest to ICAT: throws away rubbish like JOURNAL.TXT and Copy Of...
        /// </summary>
        /// <param name="e">The File to judge</param>
        /// <returns>True if the file is rubbish (i.e. not of interest) false if the file is to go to ICAT</returns>
        public bool isRubbishFile(string filepath)
        {
            //see if the file matches any of the formats spedifed in the ISISInstruments xml file
            //first see if the path is one we are intersted in
            string xmlStartString = "<?xml version=\"1.0\" encoding=\"utf-8\"?><string>";
            string xmlEndString = "</string>";
            filepath = filepath.Replace(xmlStartString, "");
            filepath = filepath.Replace(xmlEndString, "");
            string fileName = Path.GetFileName(filepath);

            //Second see if it ends with .raw, .nsx, .log or .raw
            string extension = Path.GetExtension(fileName).ToUpper();
            if (extension.Equals(".RAW") || extension.Equals(".NXS") || extension.Equals(".TXT") || extension.Equals(".LOG") || extension.StartsWith(".S") ||extension.StartsWith(".ZIP") ||extension.StartsWith(".zip"))
            {
                //do nothing
            }
            else
            {
                //logger.Info("Ignoring " + filepath + " due to its extension");
                return true;
            }

            //Third, check it is not a COPY of or EXCEPTION log
            string nameUpper = fileName.ToUpper();
            if ((nameUpper.StartsWith("COPY")) || (nameUpper.StartsWith("EXCEPTION")))
            {
                logger.Info("Ignoring " + filepath + " as it starts COPY of EXCEPTION");
                return true;
            }

            if (filepath.Contains("invalid files"))
            {
                logger.Info("Ignoring file in and 'invalid files' directory");
                return true;
            }

            //four: check it matches the format of at least one of the instruments in the list
            bool validFile = false;

            foreach (ISISInstrument instrument in instruments.Values)
            {
                if (instrument.On)
                {
                    string numbersRegExPart = "";
                    foreach (int number in instrument.FileNumbers)
                    {
                        string numberString = "";
                        for (int i = 0; i < number; i++)
                        {
                            numberString = numberString + @"[0-9]";
                        }
                        if (numbersRegExPart.Length < 1)
                        {
                            numbersRegExPart = numberString;
                        }
                        else
                        {
                            numbersRegExPart = numbersRegExPart + "|" + numberString;
                        }
                    }

                    string instrumentShortCodeRegExPart = "";
                    foreach (string shortCodePart in instrument.ShortCodes)
                    {
                        if (instrumentShortCodeRegExPart.Length < 1)
                        {
                            instrumentShortCodeRegExPart = shortCodePart;
                        }
                        else
                        {
                            instrumentShortCodeRegExPart = instrumentShortCodeRegExPart + "|" + shortCodePart;
                        }
                    }


                    //see if filename matches the format of this instrument
                    string reString = @"^" + "(" + instrumentShortCodeRegExPart + ")" + "(" + numbersRegExPart + ")";
                    var regex = new Regex(reString, RegexOptions.IgnoreCase);
                    if (regex.IsMatch(fileName))
                    {
                        validFile = true;
                        break;
                    }
                }
            }

            if (!validFile)
            {
                logger.Warn("Ignoring " + filepath + " as it does not match an instrument");
                return true;
            }

            /*
            //Fifth, and finally, check that if it is a nexus instrument, we ignore the raw file and vice versa
            if (isNexusInstrument(fileName))
            {
                //any .raw file is to be ignore
                if (extension.Equals(".RAW"))
                {
                    //logger.Info("Ignoring " + filepath + " as it is a RAW file for a Nexus instrument");
                    return true;
                }
            }
            else
            {
                //any .nxs file is to be ignored
                if (extension.Equals(".NXS"))
                {
                    //logger.Info("Ignoring " + filepath + " as it is a NXS file for a RAW instrument");
                    //return true;
                    //logger.Info("we are now ingesting " + filepath + " as it is a NXS file for a RAW instrument");                    
                }
            }*/

            return false;
        }

        public static string validInstrumentName(string instName)
        {
            string newName = instName.Trim().ToUpper();
            if (newName == "ENGIN_X" || newName == "ENGIN-X")
            {
                newName = "ENGINX";
            }
            else if (newName == "PORT-1" || newName == "PORT-2")
            {
                newName = "ARGUS";
            }
            else if (newName == "PORT-3" || newName == "PORT-4")
            {
                newName = "CHRONUS";
            }

            return newName;
        }

        /// <summary>
        /// Test to see if the instrument is a a calibration instrument
        /// </summary>
        /// <param name="name">THe name of the instrument</param>
        /// <returns>If it is a caliration instrument or not</returns>
        public bool isCalibrationInstrument(string instName)
        {
            string name = validInstrumentName(instName);
            foreach (ISISInstrument inst in instruments.Values)
            {
                if (name.ToUpper().Equals(inst.LongName.ToUpper()))
                {
                    return inst.CalibrationInstrument;
                }

                foreach (string shortCode in inst.ShortCodes)
                {
                    string startsWith = shortCode;
                    if (name.ToUpper().StartsWith(startsWith.ToUpper()))
                    {
                        return inst.CalibrationInstrument;
                    }
                }
            }
            logger.Error("Error checking if Calibration Instrument: did not recognise instrument: " + name);
            return false;

        }

        /// <summary>
        /// Tests if a given filename belongs to a Muon instrument
        /// </summary>
        /// <param name="name">The file name to test</param>
        /// <returns>True if the file belongs to  a muon instrument, otherwise false</returns>
        public bool isNexusInstrument(string name)
        {
            foreach (ISISInstrument inst in instruments.Values)
            {
                if (name.ToUpper().Equals(inst.LongName.ToUpper()))
                {
                    return inst.NexusInstrument;
                }

                foreach (string shortCode in inst.ShortCodes)
                {
                    string startsWith = shortCode;
                    if (name.ToUpper().StartsWith(startsWith.ToUpper()))
                    {
                        return inst.NexusInstrument;
                    }
                }
            }
            logger.Error("Error checking is Nexus Instrument: did not recognise instrument: " + name);
            return false;
        }

        /// <summary>
        /// Returns true if the instrument is registered on Isis compute
        /// </summary>
        /// <param name="name">Instrument name</param>
        public bool isISISCompute(string name)
        {
            return getInstrumentByName(name).ISISCompute;
        }

        /// <summary>
        /// Returns true if the instrument is registered on active directory
        /// </summary>
        /// <param name="name">Instrument name</param>
        public bool isActiveDirectory(string name)
        {
            return getInstrumentByName(name).ActiveDirectory;
        }

        /// <summary>
        /// Returns the instrument scientists for a given instrument
        /// </summary>
        /// <param name="name">The file name to test (not full path)</param>
        /// <returns>True if the file belongs to  an autoreduction instrument, otherwise false</returns>
        public List<String> getInstrumentScientistsForInstrument(string name)
        {
            ISISInstrument inst = getInstrumentByName(name);
            if (inst == null)
            {
                logger.Error("Error getting Instrument Scientists: did not recognise instrument: " + name);
                return new List<string>();
            }
            return inst.InstrumentScientists;
        }

        /// <summary>
        /// Returns the full instrument name for a given 3/4 letter code
        /// </summary>
        /// <param name="code">The 3/4 letter code</param>
        /// <returns>The full name</returns>
        public string getFullNameFromCode(string code)
        {
            if (code.Equals("SANSD"))
            {
                code = "SANS2D";
            } 

            foreach (ISISInstrument instrument in instruments.Values)
            {
                if (instrument.ShortCodes.Contains(code))
                {
                    return instrument.LongName;
                }
            }

            logger.Error("Error getting full name from code: unrecognised code - " + code);
            return code.ToUpper();
        }

        /// <summary>
        /// Parses the fileName and returns a two element array of instrument code and run number
        /// </summary>
        /// <param name="fileNameAndPath">The filename to parse</param>
        /// <returns>A two element array: [0] is the instrument code, [1] the run number</returns>
        private string[] getCodeAndRunNumberPair(string fileNameAndPath)
        {
            string[] pair = new string[2];
            string fileName = Path.GetFileName(fileNameAndPath).ToUpper();
            //first check for obvious rubbish
            if (fileName.Length < 5)
            {
                //too short, so must be a rubbish file
                pair[0] = "error";
                pair[1] = "0";
                return pair;
            }

            foreach (ISISInstrument inst in instruments.Values)
            {
                foreach (string shortCode in inst.ShortCodes)
                {
                    if (fileName.StartsWith(shortCode.ToUpper(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //it is for this instrument
                        //pair[0] = inst.ShortCodes[0]; NOTICE : Not sure why its taking one it doesn't match?????
                        pair[0] = shortCode;

                        //determining the run number is now slightly harder
                        //we must check of each of the options in inst.FileNumbersList, which one works
                        string minusName = fileName.Substring(shortCode.Length);
                        foreach (int fileNumberOption in inst.FileNumbers)
                        {
                            try
                            {
                                string possibleNumbersBit = minusName.Substring(0, fileNumberOption);
                                //try to parse that to an integer
                                int numbersBit;
                                if (int.TryParse(possibleNumbersBit, out numbersBit))
                                {
                                    //works, so numberBit is the run number

                                    //always pad this out to be the longest possible for this instrument
                                    int highest = 0;
                                    foreach (int temp in inst.FileNumbers)
                                    { if (temp > highest) { highest = temp; } }

                                    if (possibleNumbersBit.Length < highest)
                                    {
                                        //recast it to the right length
                                        string formatString = "";
                                        for (int i = 0; i < highest; i++)
                                        { formatString = formatString + "0"; }
                                        pair[1] = numbersBit.ToString(formatString);
                                    }
                                    else
                                    {
                                        pair[1] = possibleNumbersBit;
                                    }

                                    return pair;
                                }
                            }
                            catch (Exception)
                            { }
                        }
                    }
                }
            }

            //get instrument name from file path
            string[] directories = fileNameAndPath.Split('\\');

            foreach (string directory in directories)
            {
                if (directory.Contains("NDX"))
                {
                    string ins = Regex.Replace(directory, "[^a-zA-Z]", "");
                    ins = ins.Substring(3, ins.Length - 3);

                    string runNumber = "";

                    for (int i = 0; i < fileName.Length; i++ )
                    {
                        if (!Char.IsLetter(fileName[i]))
                        {
                            string runNumberTemp = fileName.Substring(i, fileName.Length - i);

                            for (int j = 0; j < runNumberTemp.Length; j++)
                            {
                                if (!Char.IsDigit(runNumberTemp[j])) 
                                {
                                    runNumber = runNumberTemp.Substring(j, runNumberTemp.Length - j);
                                }
                            }
                        }
                    }

                    pair[0] = ins;
                    pair[1] = runNumber;
                    return pair;
                }
            }

            string tempNowEtc = fileNameAndPath;
            logger.Error("Error getting code and run number pair: unrecognised instrument");
            pair[0] = "error";
            pair[1] = "0";
            return pair;
        }

        /// <summary>
        /// Method to get the instrument code
        /// </summary>
        /// <param name="fileName">The file name to extract the code from</param>
        /// <returns>The instrument code (3 or 4 letters)</returns>
        public string getInstrumentCode(string filePath)
        {
            string[] pair = getCodeAndRunNumberPair(filePath);
            return pair[0];
        }

        /// <summary>
        /// Method to get the Run ID (instrument code (3 or 4 or 6letters) plus run number (5 or 8 digits)
        /// </summary>
        /// <param name="fileName">The file name to extract the run Id from</param>
        /// <returns>The Run ID (instrument code (3 or 4 letters) plus run number (5 or 8 digits)</returns>
        public string getRunNumber(string fileName)
        {
            string[] pair = getCodeAndRunNumberPair(fileName);
            return pair[1];
        }

        /// <summary>
        /// Method to get the cycle directory from the file path
        /// </summary>
        /// <param name="filePath">The file path to extract the cycle directory from</param>
        /// <returns>The cycle directory - e.g. "cycle_05_1"</returns>
        public string getCycleDirectory(string filePath)
        {
            string[] bits = filePath.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string bit in bits)
            {
                if (bit.ToUpper().StartsWith("CYCLE_"))
                {
                    return bit;
                }
            }

            //not found it
            logger.Error("Error getting cycle directory for " + filePath);
            return "NO_CYCLE";
        }

        /// <summary>
        /// Given a path to a nxs or raw file, returns all of the other files for the run from the same directory
        /// </summary>
        /// <param name="filePath">the path to an nxs or raw file</param>
        /// <returns>All the other files in the directory for the same run, or null if filePath was not an nxs or raw file</returns>
        public List<string> getOtherRunFilesFromRawOrNexus(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            extension = Regex.Replace(extension, "[^a-zA-Z]", "");

            if (extension.ToUpper().Contains("RAW") || extension.ToUpper().Contains("NXS"))
            {
                string noExtension = Path.GetFileNameWithoutExtension(filePath);
                string directory = Path.GetDirectoryName(filePath);
                string[] allFiles = Directory.GetFiles(directory, noExtension + "*.*", SearchOption.TopDirectoryOnly);
                return allFiles
                    .Where(f => f.ToUpper() != filePath.ToUpper())
                    .ToList();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the mapping file for the instrument
        /// </summary>
        /// <param name="instrumentShortCode">The short code for the instrument</param>
        /// <returns>the name of the mapping file</returns>
        public string getMappingFile(string instrumentShortCode)
        {
            try
            {
                string mappingFile = null;
                foreach (ISISInstrument instrument in instruments.Values)
                {
                    foreach (string shortCode in instrument.ShortCodes)
                    {
                        if (shortCode == instrumentShortCode)
                        {
                            mappingFile = instrument.MappingFile;
                        }
                    }
                }


                if (mappingFile == null)
                {
                    logger.Error("Error getting mapping file from code: no mapping file: " + instrumentShortCode);
                    return "no_mapping_file";
                }
                else
                {
                    return mappingFile;
                }
            }
            catch (Exception)
            {
                logger.Error("Error getting mapping file from code: unrecognised code: " + instrumentShortCode);
                return "no_mapping_file";
            }
        }

        /// <summary>
        /// Gets all runs from all on instruments in current cycle
        /// </summary>
        /// <param name="currentCycle"></param>
        /// <returns>Dictionary of runs where the key is the instrument short code, run number and cycle directory</returns>
        public Dictionary<string, ISISRun> getAllRunsForOnInstruments(string currentCycle)
        {
            //currentCycle = "";
            Dictionary<string, ISISRun> runs = new Dictionary<string, ISISRun>();
            List<string> interestingFiles = new List<string>();

            try
            {
                foreach (ISISInstrument inst in instruments.Values)
                {

                    if (inst.On)
                    {
                        string directory = inst.Directory.Replace("CURRCYCLE", currentCycle);
                        string[] files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
                        logger.Info("Found " + files.Length + " files for " + inst.LongName);
                        foreach (string file in files)
                        {
                            if (!isRubbishFile(file))
                            {
                                interestingFiles.Add(file);
                            }
                        }
                    }

                    StreamWriter sw = new StreamWriter(inst.ShortCodes[0] + "_files.txt");
                    foreach (string s in interestingFiles)
                    {
                        sw.WriteLine(s);
                    }
                    sw.Flush();
                    sw.Close();
                    interestingFiles = new List<string>();

                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }


            foreach (ISISInstrument inst in instruments.Values)
            {
                if (inst.On)
                {
                    try
                    {
                        StreamReader sr = new StreamReader(inst.ShortCodes[0] + "_files.txt");
                        while (!sr.EndOfStream)
                        {
                            interestingFiles.Add(sr.ReadLine());
                        }
                        sr.Close();
                        File.Delete(inst.ShortCodes[0] + "_files.txt");
                    }
                    catch (Exception)
                    { }
                }
            }

            //interesting file contains all files which should be in ICAT. bundle them into runs
            foreach (string fileAndPath in interestingFiles)
            {
                string extension = Path.GetExtension(fileAndPath).ToUpper();
                if (extension == ".RAW" || extension == ".NXS") 
                {
                    RawFile rawFile = new RawFile(fileAndPath, false);
                    ISISRun tempRun = new ISISRun(rawFile, this);
                    string key = tempRun.InstrumentShortCode + tempRun.RunNumber + tempRun.CycleDirectory;

                    if (runs.ContainsKey(key))
                    {
                        //add the raw file to that entry
                        if (runs[key].RawFile == null)
                        {
                            runs[key].RawFile = rawFile;
                        }
                        else
                        {
                            //logger.Info("Duplicate RAW file for: " + key + " it is at " +  fileAndPath + " and " + runs[key].RawFile.FilePath);

                            /*duplicate may be because the 'raw' file is a nexus file
                            if ( ( runs[key].RawFile.FilePath.ToUpper().EndsWith("NXS") && !isNexusInstrument(runs[key].InstrumentName) ) || 
                                (runs[key].RawFile.FilePath.ToUpper().EndsWith("RAW") && isNexusInstrument(runs[key].InstrumentName)) )
                            {
                             * */

                            bool isNexus = isNexusInstrument(runs[key].InstrumentName);

                            if((isNexus && extension == ".NXS") || !isNexus && extension == ".RAW") {
                                //thing in the key is the NXS file, move this to the logs and set the raw as the raw!
                                LogFile logFile = new LogFile(runs[key].RawFile.FilePath, false);
                                runs[key].LogFiles.Add(logFile);
                                RawFile newRawFile = new RawFile(fileAndPath, false);
                                runs[key].RawFile = newRawFile;
                            }
                            else
                            {
                                //add this nexus file as a 'log' file
                                LogFile logFile = new LogFile(fileAndPath, false);
                                runs[key].LogFiles.Add(logFile);
                            }

                        }
                    }
                    else
                    {
                        //create a new entry
                        runs.Add(key, tempRun);
                    }
                }
                else
                {
                    LogFile logFile = new LogFile(fileAndPath, false);
                    ISISRun tempRun = new ISISRun(logFile, this);
                    string key = tempRun.InstrumentShortCode + tempRun.RunNumber + tempRun.CycleDirectory;
                    if (runs.ContainsKey(key))
                    {
                        //add the log file to that entry
                        if (runs[key].LogFiles.Contains(logFile))
                        {
                            logger.Info("Duplicate log file for: " + fileAndPath);
                            Console.Write("Press Enter to continue");
                            Console.ReadLine();
                        }
                        else
                        {
                            runs[key].LogFiles.Add(logFile);
                        }
                    }
                    else
                    {
                        //create a new entry
                        runs.Add(key, tempRun);
                    }
                }
            }

            return runs;

        }

        /// <summary>
        /// Gets all name variations for an instrument
        /// </summary>
        /// <param name="instrumentCode">One code that the instrument is referred to by</param>
        /// <returns>List of names</returns>
        public List<string> getAllInstrumentCodes(string instrumentCode)
        {
            foreach (ISISInstrument instrument in instruments.Values)
            {
                foreach (string shortCode in instrument.ShortCodes)
                {
                    if (instrument.LongName == instrumentCode || shortCode == instrumentCode)
                    {
                        return instrument.ShortCodes;
                    }
                }
            }
            List<string> temp = new List<string>();
            temp.Add(instrumentCode);
            return temp;
        }

        /// <summary>
        /// Gets all name variations for all instruments.
        /// </summary>
        /// <returns>List of instrument codes.</returns>
        public List<string> getAllInstrumentCodesRaw()
        {
            List<string> allCodes = new List<string>();
            foreach (ISISInstrument instrument in instruments.Values)
            {
                allCodes.AddRange(instrument.ShortCodes);
            }
            return allCodes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allSummaries"></param>
        /// <returns></returns>
        public string checkContiguousness(List<ISISRunSummary> allSummaries)
        {
            string mainErrorString = "";
            List<string> instSumms = new List<string>();
            foreach (ISISRunSummary summary in allSummaries)
            {
                if (!instSumms.Contains(summary.instrument)) { instSumms.Add(summary.instrument); }
            }

            foreach (string inst in instSumms)
            {
                List<int> instRuns = new List<int>();
                foreach (ISISRunSummary summary in allSummaries)
                {
                    if (summary.instrument == inst) { instRuns.Add(summary.runNumber); }
                }
                //can now check contiguosity for this instrument
                string smallErrorString = checkContNumbers(instRuns);
                if (smallErrorString.Length > 1)
                {
                    mainErrorString = mainErrorString + "Possible missing runs for " + inst + ":\n" + smallErrorString + "\n";
                }
            }
            return mainErrorString;
        }

        /// <summary>
        /// Checks whether a set of runs are continuous
        /// </summary>
        /// <param name="numbers">List of run numbers</param>
        /// <returns>Error message</returns>
        private string checkContNumbers(List<int> numbers)
        {
            string errorString = "";
            //sort the list
            numbers.Sort();
            int startingNumber = 0;// numbers[0];
            for (int i = 1; i < numbers.Count; i++)
            {
                int nextNumber = numbers[i];
                int diff = nextNumber - startingNumber;
                if (diff > 1)
                {
                    errorString = errorString + "Missing runs between: " + startingNumber + " and " + nextNumber + "\n";
                }
                startingNumber = nextNumber;
            }
            return errorString;
        }

        /// <summary>
        /// Checks whether a given code belongs to an instrument.
        /// </summary>
        /// <param name="code">instrument code to be verified.</param>
        /// <returns>True or false depending on whether to code belongs to instrument.</returns>
        public bool isValidInstrumentCode(string code)
        {
            foreach (ISISInstrument ins in instruments.Values)
            {
                if (code.ToUpper().Trim().Equals(ins.LongName.ToUpper().Trim()))
                {
                    return true;
                }
                foreach (string sCode in ins.ShortCodes)
                {
                    if (code.ToUpper().Trim().Equals(sCode.ToUpper().Trim()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
