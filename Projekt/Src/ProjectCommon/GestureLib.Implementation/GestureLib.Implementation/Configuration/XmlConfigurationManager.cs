using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using GestureLib;
using System.Xml;
using System.Xml.Linq;

namespace GestureLib
{
    public class XmlConfigurationManager : AbstractConfigurationManager
    {
        public string FileName { get; set; }

        public override void Save()
        {
            //Set some general formatting settings
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.IndentChars = "  ";
            xmlWriterSettings.NewLineChars = Environment.NewLine;
            xmlWriterSettings.Indent = true;

            //Open the xml file
            XmlWriter xmlWriter = XmlTextWriter.Create(FileName, xmlWriterSettings);

            //Write the xml-document header
            xmlWriter.WriteStartDocument();
            
            //Write container-element, which includes elements, which represent the single TrainedGesture-objects
            xmlWriter.WriteStartElement("TrainedGestures");

            //Process every TrainedGesture-object
            foreach (TrainedGesture trainedGesture in GestureLib.TrainedGestures)
            {
                xmlWriter.WriteStartElement("TrainedGesture");
                
                xmlWriter.WriteStartAttribute("Name");
                xmlWriter.WriteValue(trainedGesture.Name);
                xmlWriter.WriteEndAttribute();

                xmlWriter.WriteStartElement("Actions");

                foreach (IGestureAction action in trainedGesture.GestureActions)
                {
                    xmlWriter.WriteStartElement("Action");

                    xmlWriter.WriteStartAttribute("Name");
                    xmlWriter.WriteValue(action.Name);
                    xmlWriter.WriteEndAttribute();

                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();


                xmlWriter.WriteStartElement("Algorithms");

                foreach (IGestureAlgorithm algorithm in trainedGesture.GestureAlgorithms)
                {
                    xmlWriter.WriteStartElement("Algorithm");

                    xmlWriter.WriteStartAttribute("Name");
                    xmlWriter.WriteValue(algorithm.Name);
                    xmlWriter.WriteEndAttribute();

                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
            }

            //Close the opened tags
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndDocument();

            //Close the xml file
            xmlWriter.Close();
        }

        public override void Load()
        {
            //Proof, if the xml file exists
            if (File.Exists(FileName))
            {
                //Clear all existing TrainedGesture-objects from the collection
                GestureLib.TrainedGestures.Clear();

                //Load the xml file
                XDocument xmlDocument = XDocument.Load(FileName);

                //Loop through each TrainedGesture-Element in the xml file
                foreach (XElement trainedGesture in xmlDocument.Element("TrainedGestures").Descendants("TrainedGesture"))
                {
                    //Create a new TrainedGesture-object
                    TrainedGesture newTrainedGesture = new TrainedGesture();

                    //Set the name for this TrainedGesture
                    newTrainedGesture.Name = trainedGesture.Attribute("Name").Value;

                    //Set the algorithms for this TrainedGesture
                    foreach (XElement algorithm in trainedGesture.Element("Algorithms").Descendants("Algorithm"))
                    {
                        //Search the algorithm in the GestureLib.AvailableGestureAlgorithms-collection by the given name
                        string name = algorithm.Attribute("Name").Value;
                        newTrainedGesture.GestureAlgorithms.Add(GetGestureAlgorithmByName(name));
                    }

                    //Set the actions for this TrainedGesture
                    foreach (XElement action in trainedGesture.Element("Actions").Descendants("Action"))
                    {
                        //Search the action in the GestureLib.AvailableGestureActions-collection by the given name
                        string name = action.Attribute("Name").Value;
                        newTrainedGesture.GestureActions.Add(GetGestureActionByName(name));
                    }

                    //Add the created TrainedGesture-object to the TrainedGesture-collection
                    GestureLib.TrainedGestures.Add(newTrainedGesture);
                }
            }
        }
    }
}
