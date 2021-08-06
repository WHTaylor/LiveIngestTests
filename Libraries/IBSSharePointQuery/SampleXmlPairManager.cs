using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBSSharePointQuery
{
    public class SampleXmlPairManager : XmlPairManager
    {
        public SampleXmlPairManager()
        {
            this.NameOfSharepointList = "SamplesFullData";

            this.GuidOfSharepointList = @"{fbbfdbc4-d729-4f73-9989-6aedab41c70e}";

            List<XmlPair> list = new List<XmlPair>();

            List<string> toAdd = new List<string>();

            toAdd.Add("ows_Title");
            toAdd.Add("ows__ID");
            toAdd.Add("ows_LinkTitleNoMenu");
            toAdd.Add("ows_LinkTitle");
            toAdd.Add("ows_RB");
            toAdd.Add("ows_Instrument_Requested");
            toAdd.Add("ows_Time_Requested");
            toAdd.Add("ows_Sample_Name");
            toAdd.Add("ows_Sample_Formula");
            toAdd.Add("ows_VOLUME");
            toAdd.Add("ows_WEIGHT");
            toAdd.Add("ows_SAMPLE_HOLDER_DESCRIPTION");
            toAdd.Add("ows_TEMPERATURE_UPPER");
            toAdd.Add("ows_TEMPERATURE_LOWER");
            toAdd.Add("ows_PRESSURE_UPPER");
            toAdd.Add("ows_PRESSURE_LOWER");
            toAdd.Add("ows_MAGNETIC_FIELD_UPPER");
            toAdd.Add("ows_MAGNETIC_FIELD_LOWER");
            toAdd.Add("ows_SENSITIVE_TO_AIR");
            toAdd.Add("ows_SENSITIVE_TO_VAPOUR");
            toAdd.Add("ows_Other_Special_Equipment");
            toAdd.Add("ows_PREP_LAB_REQUIRED");
            toAdd.Add("ows_HAZARD_DETAILS");
            toAdd.Add("ows_Disposal");
            toAdd.Add("ows_SE_Requested");
            toAdd.Add("ows_Temperature_Units");
            toAdd.Add("ows_Pressure_Units");
            toAdd.Add("ows_Magnetic_Field_Units");
            toAdd.Add("ows_RBno");
            toAdd.Add("ows_Sample_Type");
            toAdd.Add("ows_Special_Requirements");   
            toAdd.Add("ows_Other_Sample_Prep_Hazards");
            toAdd.Add("ows_Other_Equip_Hazards");
            toAdd.Add("ows_Hazard_Categories");
            toAdd.Add("ows_Other_Hazards");

            foreach (string s in toAdd)
            {
                XmlPair temp = new XmlPair(s);
                list.Add(temp);
            }

            this.pairsTemplate = list;
            this.createNextDictionary();
        }

        public List<ICATXmlIngest.Sample> getAsSampleList()
        {
            List<ICATXmlIngest.Sample> samples = new List<ICATXmlIngest.Sample>();

            List<string> sampleNames = this.getAllValuesForKey("Sample_Name");
            List<string> chemicalFormula = this.getAllValuesForKey("Sample_Formula");
            List<string> hazards = this.getAllValuesForKey("HAZARD_DETAILS");

            //sample type
            List<string> sampleTypes = this.getAllValuesForKey("Sample_Type");

            //volume
            List<string> volumes = this.getAllValuesForKey("VOLUME");

            //weight
            List<string> weights = this.getAllValuesForKey("WEIGHT");

            //temperature
            List<string> tempUppers = this.getAllValuesForKey("TEMPERATURE_UPPER");
            List<string> tempLowers = this.getAllValuesForKey("TEMPERATURE_LOWER");
            List<string> tempUnits = this.getAllValuesForKey("Temperature_Units");

            //pressure
            List<string> pressureUppers = this.getAllValuesForKey("PRESSURE_UPPER");
            List<string> pressureLowers = this.getAllValuesForKey("PRESSURE_LOWER");
            List<string> pressureUnits = this.getAllValuesForKey("Pressure_Units");

            //magnetic fields
            List<string> magFUpper = this.getAllValuesForKey("MAGNETIC_FIELD_UPPER");
            List<string> magFLower = this.getAllValuesForKey("MAGNETIC_FIELD_LOWER");
            List<string> magFUnits = this.getAllValuesForKey("Magnetic_Field_Units");

            //sensitivities
            List<string> sensToAirs = this.getAllValuesForKey("SENSITIVE_TO_AIR");
            List<string> sensToVapours = this.getAllValuesForKey("SENSITIVE_TO_VAPOUR");

            //sample holder and environment and prep lab
            List<string> sampleHolders = this.getAllValuesForKey("SAMPLE_HOLDER_DESCRIPTION");
            List<string> seRequests = this.getAllValuesForKey("SE_Requested");
            List<string> prepLabs = this.getAllValuesForKey("PREP_LAB_REQUIRED");
            List<string> specialReqs = this.getAllValuesForKey("Special_Requirements");
            List<string> specialEquips = this.getAllValuesForKey("Other_Special_Equipment");

            //hazards and disposal
            List<string> hazardsDetails = this.getAllValuesForKey("HAZARD_DETAILS");
            List<string> disposals = this.getAllValuesForKey("Disposal");
            List<string> otherSamplePrepHazards = this.getAllValuesForKey("Other_Sample_Prep_Hazards");
            List<string> otherEquipHazards = this.getAllValuesForKey("Other_Equip_Hazards");
            List<string> hazardCategories = this.getAllValuesForKey("Hazard_Categories");
            List<string> otherHazards = this.getAllValuesForKey("Other_Hazards");

            for (int i = 0; i < sampleNames.Count; i++)
            {
                ICATXmlIngest.Sample sample = new ICATXmlIngest.Sample();
                sample.name = sampleNames[i];
                sample.chemical_formula = chemicalFormula[i];
                sample.safety_information = hazards[i];
                if (sample.safety_information == null)
                {
                    sample.safety_information = "None";
                }

                //do the parameters
                List<ICATXmlIngest.Parameter> parameters = new List<ICATXmlIngest.Parameter>();

                //sample type
                ICATXmlIngest.Parameter sampleTypeParam = new ICATXmlIngest.Parameter();
                sampleTypeParam.name = "Sample Type";
                sampleTypeParam.Item = sampleTypes[i];
                sampleTypeParam.units = "N/A";
                parameters.Add(sampleTypeParam);
                               

                //volume
                if(volumes[i] != null && volumes[i].Length >0)
                {
                    string volString = volumes[i].Trim();
                    string valuePart;
                    string unitsPart;
                    if(volString.Contains("cc"))
                    {
                        valuePart = volString.Replace("cc", "").Trim();
                        unitsPart = "cc";
                    }
                    else if(volString.Contains("ml"))
                    {
                        valuePart = volString.Replace("ml", "").Trim();
                        unitsPart = "ml";
                    }
                    else
                    {
                        //Not much we can do
                        valuePart = volString;
                        unitsPart = "N/A";                        
                    }

                    if (valuePart.Length > 0)
                    {

                        ICATXmlIngest.Parameter volumeParam = new ICATXmlIngest.Parameter();
                        volumeParam.name = "Volume";
                        double valueValue;
                        if (double.TryParse(valuePart, out valueValue))
                        {
                            volumeParam.Item = valueValue;
                        }
                        else
                        {
                            volumeParam.Item = valuePart;
                        }
                        volumeParam.units = unitsPart;

                        parameters.Add(volumeParam);
                    }
 
                }
                
                //weight
                if (weights[i] != null && weights[i].Length > 0)
                {
                    string weightString = weights[i].Trim();
                    string valuePart;
                    string unitsPart;
                    if (weightString.Contains("g"))
                    {
                        valuePart = weightString.Replace("g", "").Trim();
                        unitsPart = "g";
                    }
                    else if (weightString.Contains("mg"))
                    {
                        valuePart = weightString.Replace("mg", "").Trim();
                        unitsPart = "mg";
                    }
                    else
                    {
                        //Not much we can do
                        valuePart = weightString;
                        unitsPart = "N/A";                        
                    }

                    if (valuePart.Length > 0)
                    {

                        ICATXmlIngest.Parameter weightParam = new ICATXmlIngest.Parameter();
                        weightParam.name = "Weight";
                        double valueValue;
                        if (double.TryParse(valuePart, out valueValue))
                        {
                            weightParam.Item = valueValue;
                        }
                        else
                        {
                            weightParam.Item = valuePart;
                        }
                        weightParam.units = unitsPart;

                        parameters.Add(weightParam);
                    }
 
                }
                

                //temperature
                ICATXmlIngest.Parameter tempParam = new ICATXmlIngest.Parameter();
                bool saveThis = false;
                if (tempLowers[i] != null && tempLowers[i].Trim().Length >0)
                {
                    tempParam.range_bottom = tempLowers[i].Trim();
                    saveThis = true;
                }
                if (tempUppers[i] != null && tempUppers[i].Trim().Length > 0)
                {
                    tempParam.range_top = tempUppers[i].Trim();
                    saveThis = true;
                }
                if (tempUnits[i] != null && tempUnits[i].Trim().Length > 0)
                {
                    tempParam.units = tempUnits[i].Trim();
                    //saveThis = true;
                }
                else
                {
                    tempParam.units = "N/A";
                }
                if (saveThis)
                {
                    if ((tempParam.range_top != null && tempParam.range_top.Length > 0) && (tempParam.range_bottom != null && tempParam.range_top.Length > 0))
                    {
                        //they are both set
                        //the value can be them concatenated
                        tempParam.Item = tempParam.range_bottom + " - " + tempParam.range_top;
                    }
                    else if (tempParam.range_top != null && tempParam.range_top.Length > 0)
                    {
                        //just the top is set
                        //forget the range stuff and set the value to be the top
                        double tempParamValueAsDouble;
                        if (double.TryParse(tempParam.range_top, out tempParamValueAsDouble))
                        {
                            tempParam.Item = tempParamValueAsDouble;
                        }
                        else
                        {
                            tempParam.Item = tempParam.range_top;
                        }
                    }
                    else if (tempParam.range_bottom != null && tempParam.range_bottom.Length > 0)
                    {
                        //just the bottom is set
                        //forget the range stuff and set the value to be the bottom
                        double tempParamValueAsDouble;
                        if (double.TryParse(tempParam.range_bottom, out tempParamValueAsDouble))
                        {
                            tempParam.Item = tempParamValueAsDouble;
                        }
                        else
                        {
                            tempParam.Item = tempParam.range_bottom;
                        }
                    }
                    tempParam.name = "Temperature";
                    parameters.Add(tempParam);
                }
                
                //pressure
                ICATXmlIngest.Parameter pressureParam = new ICATXmlIngest.Parameter();
                saveThis = false;
                if (pressureLowers[i] != null && pressureLowers[i].Trim().Length > 0)
                {
                    pressureParam.range_bottom = pressureLowers[i].Trim();
                    saveThis = true;
                }
                if (pressureUppers[i] != null && pressureUppers[i].Trim().Length > 0)
                {
                    pressureParam.range_top = pressureUppers[i].Trim();
                    saveThis = true;
                }
                if (pressureUnits[i] != null && pressureUnits[i].Trim().Length > 0)
                {
                    pressureParam.units = pressureUnits[i].Trim();
                }
                else
                {
                    pressureParam.units = "N/A";
                }
                if (saveThis)
                {
                    if ((pressureParam.range_top != null && pressureParam.range_top.Length > 0) && (pressureParam.range_bottom != null && pressureParam.range_top.Length > 0))
                    {
                        //they are both set
                        //the value can be them concatenated
                        pressureParam.Item = pressureParam.range_bottom + " - " + pressureParam.range_top;
                    }
                    else if (pressureParam.range_top != null && pressureParam.range_top.Length > 0)
                    {
                        //just the top is set
                        //forget the range stuff and set the value to be the top
                        double pressureParamValueAsDouble;
                        if (double.TryParse(pressureParam.range_top, out pressureParamValueAsDouble))
                        {
                            pressureParam.Item = pressureParamValueAsDouble;
                        }
                        else
                        {
                            pressureParam.Item = pressureParam.range_top;
                        }
                    }
                    else if (pressureParam.range_bottom != null && pressureParam.range_bottom.Length > 0)
                    {
                        //just the bottom is set
                        //forget the range stuff and set the value to be the bottom
                        double pressureParamValueAsDouble;
                        if (double.TryParse(pressureParam.range_bottom, out pressureParamValueAsDouble))
                        {
                            pressureParam.Item = pressureParamValueAsDouble;
                        }
                        else
                        {
                            pressureParam.Item = pressureParam.range_bottom;
                        }
                    }

                    pressureParam.name = "Pressure";
                    parameters.Add(pressureParam);
                }
 
                //magnetic fields
                ICATXmlIngest.Parameter magParam = new ICATXmlIngest.Parameter();
                saveThis = false;
                if (magFLower[i] != null && magFLower[i].Trim().Length >0)
                {
                    magParam.range_bottom = magFLower[i].Trim();
                    saveThis = true;
                }
                if (magFUpper[i] != null && magFUpper[i].Trim().Length > 0)
                {
                    magParam.range_top = magFUpper[i].Trim();
                    saveThis = true;
                }
                if (magFUnits[i] != null && magFUnits[i].Trim().Length > 0)
                {
                    magParam.units = magFUnits[i].Trim();
                }
                else
                {
                    magParam.units = "N/A";
                }
                if (saveThis)
                {
                    if ((magParam.range_top != null && magParam.range_top.Length > 0) && (magParam.range_bottom != null && magParam.range_top.Length > 0))
                    {
                        //they are both set
                        //the value can be them concatenated
                        magParam.Item = magParam.range_bottom + " - " + magParam.range_top;
                    }
                    else if (magParam.range_top != null && magParam.range_top.Length > 0)
                    {
                        //just the top is set
                        //forget the range stuff and set the value to be the top
                        double magParamValueAsDouble;
                        if (double.TryParse(magParam.range_top, out magParamValueAsDouble))
                        {
                            magParam.Item = magParamValueAsDouble;
                        }
                        else
                        {
                            magParam.Item = magParam.range_top;
                        }
                    }
                    else if (magParam.range_bottom != null && magParam.range_bottom.Length > 0)
                    {
                        //just the bottom is set
                        //forget the range stuff and set the value to be the bottom
                        double magParamValueAsDouble;
                        if (double.TryParse(magParam.range_bottom, out magParamValueAsDouble))
                        {
                            magParam.Item = magParamValueAsDouble;
                        }
                        else
                        {
                            magParam.Item = magParam.range_bottom;
                        }
                    }

                    magParam.name = "Magnetic Field";
                    parameters.Add(magParam);
                }

                //sensitivities
                if (sensToAirs[i] != null && sensToAirs[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter sensToAirParam = new ICATXmlIngest.Parameter();
                    sensToAirParam.name = "Sensitive to Air";
                    sensToAirParam.Item = sensToAirs[i].Trim();
                    sensToAirParam.units = "N/A";
                    parameters.Add(sensToAirParam);
                }
                if (sensToVapours[i] != null && sensToVapours[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter sensToVapourParam = new ICATXmlIngest.Parameter();
                    sensToVapourParam.name = "Sensitive to Vapour";
                    sensToVapourParam.Item = sensToVapours[i].Trim();
                    sensToVapourParam.units = "N/A";
                    parameters.Add(sensToVapourParam);
                }

                //sample holder and environment
                if (sampleHolders[i] != null && sampleHolders[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter sampleHolderParam = new ICATXmlIngest.Parameter();
                    sampleHolderParam.name = "Sample Holder Description";
                    sampleHolderParam.Item = sampleHolders[i].Trim();
                    sampleHolderParam.units = "N/A";
                    parameters.Add(sampleHolderParam);
                }

                if (seRequests[i] != null && seRequests[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter seRequestParam = new ICATXmlIngest.Parameter();
                    seRequestParam.name = "Sample Environment Requested";
                    seRequestParam.Item = seRequests[i].Trim();
                    seRequestParam.units = "N/A";
                    parameters.Add(seRequestParam);
                }

                //prep labs
                if (prepLabs[i] != null && prepLabs[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter prepLabParam = new ICATXmlIngest.Parameter();
                    prepLabParam.name = "Sample Prep Lab";
                    prepLabParam.Item = prepLabs[i].Trim();
                    prepLabParam.units = "N/A";
                    parameters.Add(prepLabParam);
                }

                //special requirements and equipments
                if (specialReqs[i] != null && specialReqs[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter specialReqsParam = new ICATXmlIngest.Parameter();
                    specialReqsParam.name = "Special Requirements";
                    specialReqsParam.Item = specialReqs[i].Trim();
                    specialReqsParam.units = "N/A";
                    parameters.Add(specialReqsParam);
                }

                if (specialEquips[i] != null && specialEquips[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter specialEquipsParam = new ICATXmlIngest.Parameter();
                    specialEquipsParam.name = "Special Equipment";
                    specialEquipsParam.Item = specialEquips[i].Trim();
                    specialEquipsParam.units = "N/A";
                    parameters.Add(specialEquipsParam);
                }

                //hazards
                if (hazardsDetails[i] != null && hazardsDetails[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter hazardDetailsParam = new ICATXmlIngest.Parameter();
                    hazardDetailsParam.name = "Hazard Details";
                    hazardDetailsParam.Item = hazardsDetails[i].Trim();
                    hazardDetailsParam.units = "N/A";
                    parameters.Add(hazardDetailsParam);
                }

                //disposal
                if (disposals[i] != null && disposals[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter disposalsParam = new ICATXmlIngest.Parameter();
                    disposalsParam.name = "Disposal Method";
                    disposalsParam.Item = disposals[i].Trim();
                    disposalsParam.units = "N/A";
                    parameters.Add(disposalsParam);
                }
                
                //More hazards
                if (otherSamplePrepHazards[i] != null && otherSamplePrepHazards[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter otherSamplePrepHazardsParam = new ICATXmlIngest.Parameter();
                    otherSamplePrepHazardsParam.name = "Sample Prep Hazards";
                    otherSamplePrepHazardsParam.Item = otherSamplePrepHazards[i].Trim();
                    otherSamplePrepHazardsParam.units = "N/A";
                    parameters.Add(otherSamplePrepHazardsParam);
                }

                if (otherEquipHazards[i] != null && otherEquipHazards[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter otherEquipHazardsParam = new ICATXmlIngest.Parameter();
                    otherEquipHazardsParam.name = "Other Equipment Hazards";
                    otherEquipHazardsParam.Item = otherEquipHazards[i].Trim();
                    otherEquipHazardsParam.units = "N/A";
                    parameters.Add(otherEquipHazardsParam);
                }

                if (hazardCategories[i] != null && hazardCategories[i].Trim().Length > 0)
                {
                    //break the hazard catagories up by '-'
                    string[] hazardCategoriesBrokenUp = hazardCategories[i].Split(new char[1] { '-' });

                    foreach (string hazardCategory in hazardCategoriesBrokenUp)
                    {
                        ICATXmlIngest.Parameter hazardCategoriesParam = new ICATXmlIngest.Parameter();
                        hazardCategoriesParam.name = "Hazard Catagory";
                        hazardCategoriesParam.Item = hazardCategory.Trim();
                        hazardCategoriesParam.units = "N/A";
                        parameters.Add(hazardCategoriesParam);
                    }
                }

                if (otherHazards[i] != null && otherHazards[i].Trim().Length > 0)
                {
                    ICATXmlIngest.Parameter otherHazardsParam = new ICATXmlIngest.Parameter();
                    otherHazardsParam.name = "Other Hazards";
                    otherHazardsParam.Item = otherHazards[i].Trim();
                    otherHazardsParam.units = "N/A";
                    parameters.Add(otherHazardsParam);
                }

                sample.parameter = parameters.ToArray();
                samples.Add(sample);
            }
            return samples;
        }
    }
}
