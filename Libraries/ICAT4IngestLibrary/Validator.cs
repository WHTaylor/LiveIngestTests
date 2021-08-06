using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace ICAT4IngestLibrary
{
    /// <summary>
    /// Simple class to run a XML validating reader over a given XML file.
    /// </summary>
    public class Validator
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Empty constructor
        /// </summary>
        public Validator()
        {

        }

        private bool isValid;

        /// <summary>
        /// Runs the the xml validator against an xml file, returning the error string
        /// </summary>
        /// <param name="filenameAndPath"></param>
        /// <returns></returns>
#pragma warning disable 618
        public string runValidator(string filenameAndPath)
        {
            
            string errors = "";
            XmlValidatingReader validator = null;
            isValid = true;

            string filename = filenameAndPath;
            //string filename = @"C:\Programming\cpp\WriteRaw\runfromhere\Xml\MUSR00005323.xml";

            try
            {
                // open the file and wrap a validating reader around it
                XmlTextReader reader = new XmlTextReader(filename);
                reader.WhitespaceHandling = WhitespaceHandling.None;
                validator = new XmlValidatingReader(reader);

                // select the validation type

                validator.ValidationType = ValidationType.Schema;


                // add the handler for the errors
                validator.ValidationEventHandler += new ValidationEventHandler(validator_ValidationEventHandler);

                // parse and validate
                while (validator.Read()) ;
            }
            catch (Exception e) //NOTICE : Is currently returning valid when xml file is not found, made change to correct this.
            {
                errors = errors + "\n" + e.Message;
                isValid = false;
            }
            finally
            {
                if (validator != null)
                    validator.Close();

            }// end finally
            if (isValid)
            {
                //Console.WriteLine("Valid XML file");
                return "Valid";
            }

            return errors;
        }
#pragma warning restore 618

        /// <summary>
        /// Handler called when validation fails. Sets the boolean 'valid' to false, and logs the nature of the validation error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void validator_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            isValid = false;
            //Console.WriteLine(e.Message);
            logger.Info("XML Validation Error: " + e.Message);
            logger.Info("XML Validation Error: " + e.Severity);
            logger.Info("XML Validation Error on Line: " + e.Exception.LineNumber);
            logger.Info("XML Validation Error on Line: " + e.Exception.InnerException);
        }

    }
}
