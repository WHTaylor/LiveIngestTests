using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICAT4IngestLibrary
{
    public static class FileLocationFixer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This is ISIS specific - it corrects a potentially machine specific filepath to a DFS path
        /// If the path does not look ISIS-y it it returned unchanged, so the method is reasonably 'safe' for use elsewhere
        /// </summary>
        /// <param name="oldFileLocation"></param>
        /// <returns></returns>
        public static string GetCorrectFileLocation(string oldFileLocation)
        {
            log.Info("Correcting file location: " + oldFileLocation);
            try
            {
                string correctLocation = GetNewLocation(oldFileLocation);
                return correctLocation;
            }
            catch (Exception e)
            {
                log.Error("Error correcting location", e);
                return oldFileLocation;
            }
        }

        private static string GetNewLocation(string oldFileLocation)
        {
            string[] words = oldFileLocation.Split(new string[2] { @"/", @"\" }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length < 3)
            {
                log.Error("File location too short: " + oldFileLocation);
                return oldFileLocation;
            }

            string instrumentWord = null;
            string cycleWord = null;
            string fileWord = null;
            foreach (string word in words)
            {
                string upperWord = word.ToUpper();
                if (upperWord.StartsWith("CYCLE_"))
                {
                    cycleWord = word;
                }
                if (upperWord.StartsWith("NDX"))
                {
                    instrumentWord = word;
                }
                if (upperWord.StartsWith("EMU") || upperWord.StartsWith("HIFI") || upperWord.StartsWith("MUSR"))
                {
                    fileWord = word;
                }
            }

            if (fileWord == null)
            {
                //last word
                fileWord = words[words.Length - 1];
            }

            if (fileWord == null)
            {
                log.Error("File word is null");
            }
            else if (instrumentWord == null)
            {
                log.Error("Instrument word is null");
            }
            else if(cycleWord == null)
            {
                log.Error("Cycle word is null");
            }
            if(fileWord == null || instrumentWord == null || cycleWord == null)
            {
                return oldFileLocation;
            }

            cycleWord = cycleWord.Replace("$", "");

            return @"\\isis\inst$\" + instrumentWord + @"\Instrument\data\" + cycleWord + @"\" + fileWord;
        }
    }
}
