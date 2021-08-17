using System;
using System.Collections.Generic;
using org.icatproject.isisicat.ICAT;

namespace ICAT4IngestLibrary
{
    public enum ParameterTypeCallingObject { Investigation, Dataset, Datafile, Sample };

    /// <summary>
    /// Connects to an ICAT and caches the main values we will reuse in the ingest process.
    /// Includes things like Facility, InvestigationType etc, to reduce the number of API calls
    /// and increase performance
    /// </summary>
    public class ICATValues
    {
        private facility facility;
        private List<facilityCycle> cycles;
        private List<instrument> instruments;
        private List<investigationType> invTypes;
        private List<parameterType> paramTypes;
        private List<datasetType> datasetTypes;
        private List<datafileFormat> datafileFormats;

        public ICATValues(string sessionId, CATClient service, string facilityName)
        {
            //facility - Facility name is set in the settings file. Everything else is derived from that.
            try
            {  
                facility = (facility)service.search(sessionId, "Facility [name='" + facilityName + "']")[0];
            }
            catch (Exception e) { }

            //cycles
            cycles = new List<facilityCycle>();
            try
            {
                object[] cyclesArray = service.search(sessionId, "FacilityCycle");
                foreach (object o in cyclesArray)
                {
                    cycles.Add((facilityCycle)o);
                }

            }
            catch (Exception e) { }

            //instruments
            instruments = new List<instrument>();
            try
            {
                object[] instrumentsArray = service.search(sessionId, "Instrument");
                foreach (object o in instrumentsArray)
                {
                    instruments.Add((instrument)o);
                }

            }
            catch (Exception e) { }

            //investigation types
            invTypes = new List<investigationType>();
            try
            {
                object[] invTypesArray = service.search(sessionId, "InvestigationType");
                foreach (object o in invTypesArray)
                {
                    invTypes.Add((investigationType)o);
                }

            }
            catch (Exception e) { }

            //parameter types
            paramTypes = new List<parameterType>();
            try
            {
                object[] paramTypesArray = service.search(sessionId, "ParameterType");
                foreach (object o in paramTypesArray)
                {
                    paramTypes.Add((parameterType)o);
                }

            }
            catch (Exception e) { }

            //dataset types
            datasetTypes = new List<datasetType>();
            try
            {
                object[] datasetTypesArray = service.search(sessionId, "DatasetType");
                foreach (object o in datasetTypesArray)
                {
                    datasetTypes.Add((datasetType)o);
                }

            }
            catch (Exception e) { }

            //datafileformats
            datafileFormats = new List<datafileFormat>();
            try
            {
                object[] datafileFormatsArray = service.search(sessionId, "DatafileFormat");
                foreach (object o in datafileFormatsArray)
                {
                    datafileFormats.Add((datafileFormat)o);
                }

            }
            catch (Exception e) { }

        }

        public List<facilityCycle> Cycles
        {
            get
            {
                return cycles;
            }
        }

        public List<instrument> Instruments
        {
            get
            {
                return instruments;
            }
        }

        public List<investigationType> InvTypes
        {
            get
            {
                return invTypes;
            }
        }

        public List<parameterType> ParamTypes
        {
            get
            {
                return paramTypes;
            }
        }

        public List<datasetType> DataSetTypes
        {
            get
            {
                return datasetTypes;
            }
        }

        public List<datafileFormat> DataFileFormats
        {
            get
            {
                return datafileFormats;
            }
        }

        public facility Facility
        {
            get { return facility; }
        }

        public instrument getInstrumentByName(string instName)
        {
            string instrumentName = ISISInstrumentManager.validInstrumentName(instName);
            instrument foundInstrument = instruments.Find(i => i.name.Trim().ToUpper() == instrumentName);
            if (foundInstrument == null)
            {
                foundInstrument = instruments.Find(i => i.fullName.Trim().ToUpper() == instrumentName);
            }

            return foundInstrument;
        }

        public investigationType getInvestigationTypeByName(string investigationType)
        {
            return invTypes.Find(i => i.name == investigationType);
        }

        public facilityCycle getCycleByDate(DateTime date)
        {
            return cycles.Find(i => i.startDate < date && i.endDate > date);
        }

        /// <summary>
        /// NOTICE: parameterValueType in arguments is not used in method?
        /// </summary>
        /// <param name="pvt"></param>
        /// <param name="paramName"></param>
        /// <param name="paramUnits"></param>
        /// <returns></returns>
        public parameterType getParameterTypeByNameAndUnits(parameterValueType pvt, string paramName, string paramUnits)
        {
            parameterType ptype = paramTypes.Find(i => i.name == paramName && i.units == paramUnits);
            if (ptype == null)
            {
                ptype = paramTypes.Find(i => i.name == paramName && i.unitsFullName == paramUnits);
            }
            return ptype;
        }
        public datafileFormat getDatafileFormat(string name, string version)
        {
            if (name  == null && version == null)
            {
                return null;
            }
            else
            {
                return datafileFormats.Find(i => i.name.ToUpper() == name.ToUpper() && i.version.ToUpper() == version.ToUpper());
            }
        }
        public datasetType getDataSetType(string name)
        {
            return datasetTypes.Find(i => i.name.ToUpper() == name.ToUpper());
        }

        public datasetType getRawDatasetType()
        {
            return getDataSetType("experiment_raw");
        }

        public int getDaysToPublicRelease()
        {
            return facility.daysUntilRelease;
        }

        /// <summary>
        /// Sets the release date of the investigation according to the data policy and Facility.daysuntilrelesae atttribute
        /// </summary>
        /// <param name="inv"></param>
        /// <returns>The date the investigation should be released, or null for commercial experiments</returns>
        public DateTime? calculateReleaseDate(investigation inv)
        {
            if (inv.type == getInvestigationTypeByName("commercial_experiment"))
            {
                return null;
            }

            /*
             * 3.3.3 Access to raw data and the associated metadata obtained from an experiment is restricted to the 
             * experimental team for a period of three years after the end of the experiment. Thereafter, it will 
             * become publicly accessible. Any PI that wishes their data to remain ‘restricted access’ for a longer 
             * period will be required to make a special case to the Director of ISIS.
             */
            Tuple<DateTime, DateTime> startAndEnd = getInvestigationStartAndEndDates(inv);

            DateTime endOfExperiment = startAndEnd.Item2;

            TimeSpan embargoPeriod = new TimeSpan(facility.daysUntilRelease, 0, 0, 0);

            DateTime releaseDate = endOfExperiment + embargoPeriod;

            return releaseDate;
        }

        public Tuple<DateTime, DateTime> getInvestigationStartAndEndDates(investigation inv)
        {
            DateTime initEarliestFileTime = new DateTime(9999, 12, 31);
            DateTime initLatestFileTime = new DateTime(1, 1, 1);

            DateTime earliestFileTime = initEarliestFileTime;
            DateTime latestFileTime = initLatestFileTime;

            if (inv.startDate != null)
            {
                earliestFileTime = inv.startDate;
            }
            if (inv.endDate != null)
            {
                latestFileTime = inv.endDate;
            }
            if (inv.datasets != null)
            {
                foreach (dataset ds in inv.datasets)
                {
                    if (ds.datafiles != null)
                    {
                        foreach (datafile df in ds.datafiles)
                        {
                            if (df.datafileCreateTime < earliestFileTime)
                            {
                                earliestFileTime = df.datafileCreateTime;
                            }
                            if (df.datafileCreateTime > latestFileTime)
                            {
                                latestFileTime = df.datafileCreateTime;
                            }
                        }
                    }
                }
            }

            if (earliestFileTime == initLatestFileTime)
            {
                earliestFileTime = DateTime.Now;
            }
            if (latestFileTime == initLatestFileTime)
            {
                latestFileTime = DateTime.Now;
            }

            return new Tuple<DateTime, DateTime>(earliestFileTime, latestFileTime);
        }

       

        public static parameterValueType GetParameterValueType(object item)
        {
            //see what type we have
            parameterValueType pvt;
            if(item.GetType() == typeof(string))
            {
                pvt = parameterValueType.STRING;
            }
            else if(item.GetType() == typeof(DateTime))
            {
                pvt = parameterValueType.DATE_AND_TIME;
            }
            else
            {
                pvt = parameterValueType.NUMERIC;
            }
            return pvt;
        }
    }
}
