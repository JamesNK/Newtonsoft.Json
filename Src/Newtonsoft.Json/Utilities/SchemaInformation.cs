using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;

namespace Newtonsoft.Json.Utilities
{
    /// <summary>
    /// Create object of SchemaInformation class
    /// </summary>
    public static class SchemaInformationFactory
    {
        private static Dictionary<string, SchemaInformation> schemaInformationObjects = new Dictionary<string, SchemaInformation>();

        /// <summary>
        /// Create a new object of SchemaInformation class or returns an exting one if already present
        /// </summary>
        /// <value>namespace uri string</value>
        public static SchemaInformation CreateOrGet(XmlSchemaSet schemaSet)
        {
            if (null == schemaSet || schemaSet.Count == 0)
            {
                SchemaInformation nullSchemaInformation = new SchemaInformation(schemaSet);
                return nullSchemaInformation;
            }

            if (!schemaSet.IsCompiled)
            {
                schemaSet.Compile();
            }

            string schemaId = null;

            foreach (XmlSchema firstSchema in schemaSet.Schemas())
            {
                IEnumerator elementEnumerator = firstSchema.Elements.Values.GetEnumerator();
                elementEnumerator.MoveNext();
                XmlSchemaElement rootElement = (XmlSchemaElement)elementEnumerator.Current;

                schemaId = firstSchema.TargetNamespace + "/" + rootElement.Name;
                break;              
            }
            
            SchemaInformation schemaInformation;

            if (schemaInformationObjects.TryGetValue(schemaId, out schemaInformation))
            {
                return schemaInformation;
            }
            else
            {
                lock (schemaInformationObjects)
                {
                    if (schemaInformationObjects.TryGetValue(schemaId, out schemaInformation))
                    {
                        return schemaInformation;
                    }
                    else
                    {
                        schemaInformation = new SchemaInformation(schemaSet);
                        schemaInformationObjects.Add(schemaId, schemaInformation);
                        return schemaInformation;
                    }
                }
            }

        }
    }

    /// <summary>
    /// Stores serialization properties, data type and array information from an xsd to a dictionary
    /// </summary>
    public class SchemaInformation : Dictionary<string, ElementInformation>
    {
        public SchemaInformation(XmlSchemaSet schemaSet)
        {
            if (null != schemaSet && schemaSet.Count > 0)
            {
                this.TraverseSchema(schemaSet);
            }
        }

        private void TraverseSchema(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                foreach (XmlSchemaElement element in schema.Elements.Values)
                {
                    this.ProcessElement(element);
                }

                foreach (XmlSchemaComplexType type in schema.SchemaTypes.Values)
                {
                    ProcessSchemaObject(type.ContentTypeParticle);
                }
            }
        }

        private void ProcessElement(XmlSchemaElement element)
        {
            ElementInformation elementInformation = new ElementInformation();

            if (element.MaxOccurs > 1)
            {
                elementInformation.IsArray = true;
            }

            if (element.ElementSchemaType is XmlSchemaSimpleType && null != element.ElementSchemaType.Datatype)
            {
                elementInformation.DataType = element.ElementSchemaType.Datatype.ValueType;
            }

            if (!this.ContainsKey(element.Name.ToString()))
            {
                this.Add(element.Name.ToString(), elementInformation);
            }

            if (element.ElementSchemaType is XmlSchemaComplexType)
            {
                XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;

                this.ProcessSchemaObject(complexType.ContentTypeParticle);
            }
        }

        private void ProcessSequence(XmlSchemaSequence sequence)
        {
            this.ProcessItemCollection(sequence.Items);
        }

        private void ProcessChoice(XmlSchemaChoice choice)
        {
            this.ProcessItemCollection(choice.Items);
        }

        private void ProcessItemCollection(XmlSchemaObjectCollection objs)
        {
            foreach (XmlSchemaObject obj in objs)
                this.ProcessSchemaObject(obj);
        }

        private void ProcessSchemaObject(XmlSchemaObject obj)
        {
            if (obj is XmlSchemaElement)
                this.ProcessElement(obj as XmlSchemaElement);
            if (obj is XmlSchemaChoice)
                this.ProcessChoice(obj as XmlSchemaChoice);
            if (obj is XmlSchemaSequence)
                this.ProcessSequence(obj as XmlSchemaSequence);
        }
    }

    /// <summary>
    /// Class to store Data type and Array information for an xml element
    /// </summary>
    public class ElementInformation
    {
        /// <summary>
        /// Stores Data type information for an xml element
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// Stores Array information for an xml element
        /// </summary>
        public bool IsArray { get; set; }
    }
}
