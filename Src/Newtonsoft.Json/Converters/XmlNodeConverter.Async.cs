#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    internal sealed partial class XmlNodeConverterImpl
    {
        public override Task<object> ReadJsonAsync(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadJsonAsync(reader, objectType, cancellationToken);
        }

        public override Task WriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteJsonAsync(writer, value, cancellationToken);
        }
    }

    public partial class XmlNodeConverter
    {
        private bool SafeAsync
        {
            get { return GetType() == typeof(XmlNodeConverter); }
        }

        /// <summary>
        /// Asynchronously writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteJsonAsync(writer, value, cancellationToken) : base.WriteJsonAsync(writer, value, serializer, cancellationToken);
        }

        internal async Task DoWriteJsonAsync(JsonWriter writer, object value, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IXmlNode node = WrapXml(value);

            XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
            PushParentNamespaces(node, manager);

            if (!OmitRootObject)
            {
                await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);
            }

            await SerializeNodeAsync(writer, node, manager, !OmitRootObject, cancellationToken).ConfigureAwait(false);

            if (!OmitRootObject)
            {
                await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SerializeNodeAsync(JsonWriter writer, IXmlNode node, XmlNamespaceManager manager, bool writePropertyName, CancellationToken cancellationToken)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                    await SerializeGroupedNodesAsync(writer, node, manager, writePropertyName, cancellationToken).ConfigureAwait(false);
                    break;
                case XmlNodeType.Element:
                    if (IsArray(node) && AllSameName(node) && node.ChildNodes.Count > 0)
                    {
                        await SerializeGroupedNodesAsync(writer, node, manager, false, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        manager.PushScope();

                        foreach (IXmlNode attribute in node.Attributes)
                        {
                            if (attribute.NamespaceUri == "http://www.w3.org/2000/xmlns/")
                            {
                                string namespacePrefix = attribute.LocalName != "xmlns" ? XmlConvert.DecodeName(attribute.LocalName) : string.Empty;
                                string namespaceUri = attribute.Value;

                                manager.AddNamespace(namespacePrefix, namespaceUri);
                            }
                        }

                        if (writePropertyName)
                        {
                            await writer.WritePropertyNameAsync(GetPropertyName(node, manager), cancellationToken).ConfigureAwait(false);
                        }

                        if (!ValueAttributes(node.Attributes) && node.ChildNodes.Count == 1 && node.ChildNodes[0].NodeType == XmlNodeType.Text)
                        {
                            // write elements with a single text child as a name value pair
                            await writer.WriteValueAsync(node.ChildNodes[0].Value, cancellationToken).ConfigureAwait(false);
                        }
                        else if (node.ChildNodes.Count == 0 && CollectionUtils.IsNullOrEmpty(node.Attributes))
                        {
                            IXmlElement element = (IXmlElement)node;

                            // empty element
                            if (element.IsEmpty)
                            {
                                await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                await writer.WriteValueAsync(string.Empty, cancellationToken).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);

                            for (int i = 0; i < node.Attributes.Count; i++)
                            {
                                await SerializeNodeAsync(writer, node.Attributes[i], manager, true, cancellationToken).ConfigureAwait(false);
                            }

                            await SerializeGroupedNodesAsync(writer, node, manager, true, cancellationToken).ConfigureAwait(false);

                            await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
                        }

                        manager.PopScope();
                    }

                    break;
                case XmlNodeType.Comment:
                    if (writePropertyName)
                    {
                        await writer.WriteCommentAsync(node.Value, cancellationToken).ConfigureAwait(false);
                    }
                    break;
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    if (node.NamespaceUri == "http://www.w3.org/2000/xmlns/" && node.Value == JsonNamespaceUri)
                    {
                        return;
                    }

                    if (node.NamespaceUri == JsonNamespaceUri)
                    {
                        if (node.LocalName == "Array")
                        {
                            return;
                        }
                    }

                    if (writePropertyName)
                    {
                        await writer.WritePropertyNameAsync(GetPropertyName(node, manager), cancellationToken).ConfigureAwait(false);
                    }
                    await writer.WriteValueAsync(node.Value, cancellationToken).ConfigureAwait(false);
                    break;
                case XmlNodeType.XmlDeclaration:
                    IXmlDeclaration declaration = (IXmlDeclaration)node;
                    await writer.WritePropertyNameAsync(GetPropertyName(node, manager), cancellationToken).ConfigureAwait(false);
                    await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(declaration.Version))
                    {
                        await writer.WritePropertyNameAsync("@version", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(declaration.Version, cancellationToken).ConfigureAwait(false);
                    }
                    if (!string.IsNullOrEmpty(declaration.Encoding))
                    {
                        await writer.WritePropertyNameAsync("@encoding", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(declaration.Encoding, cancellationToken).ConfigureAwait(false);
                    }
                    if (!string.IsNullOrEmpty(declaration.Standalone))
                    {
                        await writer.WritePropertyNameAsync("@standalone", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(declaration.Standalone, cancellationToken).ConfigureAwait(false);
                    }

                    await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
                    break;
                case XmlNodeType.DocumentType:
                    IXmlDocumentType documentType = (IXmlDocumentType)node;
                    await writer.WritePropertyNameAsync(GetPropertyName(node, manager), cancellationToken).ConfigureAwait(false);
                    await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(documentType.Name))
                    {
                        await writer.WritePropertyNameAsync("@name", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(documentType.Name, cancellationToken).ConfigureAwait(false);
                    }
                    if (!string.IsNullOrEmpty(documentType.Public))
                    {
                        await writer.WritePropertyNameAsync("@public", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(documentType.Public, cancellationToken).ConfigureAwait(false);
                    }
                    if (!string.IsNullOrEmpty(documentType.System))
                    {
                        await writer.WritePropertyNameAsync("@system", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(documentType.System, cancellationToken).ConfigureAwait(false);
                    }
                    if (!string.IsNullOrEmpty(documentType.InternalSubset))
                    {
                        await writer.WritePropertyNameAsync("@internalSubset", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(documentType.InternalSubset, cancellationToken).ConfigureAwait(false);
                    }

                    await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new JsonSerializationException("Unexpected XmlNodeType when serializing nodes: " + node.NodeType);
            }
        }

        private async Task SerializeGroupedNodesAsync(JsonWriter writer, IXmlNode node, XmlNamespaceManager manager, bool writePropertyName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // group nodes together by name
            Dictionary<string, List<IXmlNode>> nodesGroupedByName = new Dictionary<string, List<IXmlNode>>();

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                IXmlNode childNode = node.ChildNodes[i];
                string nodeName = GetPropertyName(childNode, manager);

                List<IXmlNode> nodes;
                if (!nodesGroupedByName.TryGetValue(nodeName, out nodes))
                {
                    nodes = new List<IXmlNode>();
                    nodesGroupedByName.Add(nodeName, nodes);
                }

                nodes.Add(childNode);
            }

            // loop through grouped nodes. write single name instances as normal,
            // write multiple names together in an array
            foreach (KeyValuePair<string, List<IXmlNode>> nodeNameGroup in nodesGroupedByName)
            {
                List<IXmlNode> groupedNodes = nodeNameGroup.Value;


                if (groupedNodes.Count == 1 && !IsArray(groupedNodes[0]))
                {
                    await SerializeNodeAsync(writer, groupedNodes[0], manager, writePropertyName, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    if (writePropertyName)
                    {
                        await writer.WritePropertyNameAsync(nodeNameGroup.Key, cancellationToken).ConfigureAwait(false);
                    }

                    await writer.WriteStartArrayAsync(cancellationToken).ConfigureAwait(false);

                    for (int i = 0; i < groupedNodes.Count; i++)
                    {
                        await SerializeNodeAsync(writer, groupedNodes[i], manager, false, cancellationToken).ConfigureAwait(false);
                    }

                    await writer.WriteEndArrayAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Asynchronously reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the Result property is the object read.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<object> ReadJsonAsync(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoReadJsonAsync(reader, objectType, cancellationToken) : base.ReadJsonAsync(reader, objectType, existingValue, serializer, cancellationToken);
        }

        internal Task<object> DoReadJsonAsync(JsonReader reader, Type objectType, CancellationToken cancellationToken)
        {
            return reader.TokenType == JsonToken.Null ? cancellationToken.CancelledOrNullAsync() : ReadJsonNotNullAsync(reader, objectType, cancellationToken);
        }

        private async Task<object> ReadJsonNotNullAsync(JsonReader reader, Type objectType, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
            IXmlDocument document = null;
            IXmlNode rootNode = null;

#if !NET20
            if (typeof(XObject).IsAssignableFrom(objectType))
            {
                if (objectType != typeof(XDocument) && objectType != typeof(XElement))
                {
                    throw new JsonSerializationException("XmlNodeConverter only supports deserializing XDocument or XElement.");
                }

                XDocument d = new XDocument();
                document = new XDocumentWrapper(d);
                rootNode = document;
            }
#endif
#if !(DOTNET || PORTABLE)
            if (typeof(XmlNode).IsAssignableFrom(objectType))
            {
                if (objectType != typeof(XmlDocument))
                {
                    throw new JsonSerializationException("XmlNodeConverter only supports deserializing XmlDocuments");
                }

                XmlDocument d = new XmlDocument();

                // prevent http request when resolving any DTD references
                d.XmlResolver = null;

                document = new XmlDocumentWrapper(d);
                rootNode = document;
            }
#endif

            if (document == null || rootNode == null)
            {
                throw new JsonSerializationException("Unexpected type when converting XML: " + objectType);
            }

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException("XmlNodeConverter can only convert JSON that begins with an object.");
            }

            if (!string.IsNullOrEmpty(DeserializeRootElementName))
            {
                //rootNode = document.CreateElement(DeserializeRootElementName);
                //document.AppendChild(rootNode);
                await ReadElementAsync(reader, document, rootNode, DeserializeRootElementName, manager, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                await DeserializeNodeAsync(reader, document, manager, rootNode, cancellationToken).ConfigureAwait(false);
            }

            if (objectType == typeof(XElement))
            {
                XElement element = (XElement)document.DocumentElement.WrappedNode;
                element.Remove();

                return element;
            }

            return document.WrappedNode;
        }

        private async Task ReadElementAsync(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName, XmlNamespaceManager manager, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw JsonSerializationException.Create(reader, "XmlNodeConverter cannot convert JSON with an empty property name to XML.");
            }

            Dictionary<string, string> attributeNameValues = ReadAttributeElements(reader, manager);

            string elementPrefix = MiscellaneousUtils.GetPrefix(propertyName);

            if (propertyName.StartsWith('@'))
            {
                string attributeName = propertyName.Substring(1);
                string attributePrefix = MiscellaneousUtils.GetPrefix(attributeName);

                AddAttribute(reader, document, currentNode, propertyName, attributeName, manager, attributePrefix);
                return;
            }

            if (propertyName.StartsWith('$'))
            {
                switch (propertyName)
                {
                    case JsonTypeReflector.ArrayValuesPropertyName:
                        propertyName = propertyName.Substring(1);
                        elementPrefix = manager.LookupPrefix(JsonNamespaceUri);
                        await CreateElementAsync(reader, document, currentNode, propertyName, manager, elementPrefix, attributeNameValues, cancellationToken).ConfigureAwait(false);
                        return;
                    case JsonTypeReflector.IdPropertyName:
                    case JsonTypeReflector.RefPropertyName:
                    case JsonTypeReflector.TypePropertyName:
                    case JsonTypeReflector.ValuePropertyName:
                        string attributeName = propertyName.Substring(1);
                        string attributePrefix = manager.LookupPrefix(JsonNamespaceUri);
                        AddAttribute(reader, document, currentNode, propertyName, attributeName, manager, attributePrefix);
                        return;
                }
            }

            await CreateElementAsync(reader, document, currentNode, propertyName, manager, elementPrefix, attributeNameValues, cancellationToken).ConfigureAwait(false);
        }

        private async Task CreateElementAsync(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string elementName, XmlNamespaceManager manager, string elementPrefix, Dictionary<string, string> attributeNameValues, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IXmlElement element = CreateElement(elementName, document, elementPrefix, manager);

            currentNode.AppendChild(element);

            // add attributes to newly created element
            foreach (KeyValuePair<string, string> nameValue in attributeNameValues)
            {
                string encodedName = XmlConvert.EncodeName(nameValue.Key);
                string attributePrefix = MiscellaneousUtils.GetPrefix(nameValue.Key);

                IXmlNode attribute = !string.IsNullOrEmpty(attributePrefix) ? document.CreateAttribute(encodedName, manager.LookupNamespace(attributePrefix) ?? string.Empty, nameValue.Value) : document.CreateAttribute(encodedName, nameValue.Value);

                element.SetAttributeNode(attribute);
            }

            if (reader.TokenType == JsonToken.String || reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Boolean || reader.TokenType == JsonToken.Date)
            {
                string text = ConvertTokenToXmlValue(reader);
                if (text != null)
                {
                    element.AppendChild(document.CreateTextNode(text));
                }
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                // empty element. do nothing
            }
            else
            {
                // finished element will have no children to deserialize
                if (reader.TokenType != JsonToken.EndObject)
                {
                    manager.PushScope();
                    await DeserializeNodeAsync(reader, document, manager, element, cancellationToken).ConfigureAwait(false);
                    manager.PopScope();
                }

                manager.RemoveNamespace(string.Empty, manager.DefaultNamespace);
            }
        }

        private async Task DeserializeNodeAsync(JsonReader reader, IXmlDocument document, XmlNamespaceManager manager, IXmlNode currentNode, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        if (currentNode.NodeType == XmlNodeType.Document && document.DocumentElement != null)
                        {
                            throw JsonSerializationException.Create(reader, "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifing a DeserializeRootElementName.");
                        }

                        string propertyName = reader.Value.ToString();
                        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            int count = 0;
                            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && reader.TokenType != JsonToken.EndArray)
                            {
                                await DeserializeValueAsync(reader, document, manager, propertyName, currentNode, cancellationToken).ConfigureAwait(false);
                                count++;
                            }

                            if (count == 1 && WriteArrayAttribute)
                            {
                                foreach (IXmlNode childNode in currentNode.ChildNodes)
                                {
                                    IXmlElement element = childNode as IXmlElement;
                                    if (element != null && element.LocalName == propertyName)
                                    {
                                        AddJsonArrayAttribute(element, document);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            await DeserializeValueAsync(reader, document, manager, propertyName, currentNode, cancellationToken).ConfigureAwait(false);
                        }

                        break;
                    case JsonToken.StartConstructor:
                        string constructorName = reader.Value.ToString();

                        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && reader.TokenType != JsonToken.EndConstructor)
                        {
                            await DeserializeValueAsync(reader, document, manager, constructorName, currentNode, cancellationToken).ConfigureAwait(false);
                        }

                        break;
                    case JsonToken.Comment:
                        currentNode.AppendChild(document.CreateComment((string)reader.Value));
                        break;
                    case JsonToken.EndObject:
                    case JsonToken.EndArray:
                        return;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected JsonToken when deserializing node: " + reader.TokenType);
                }
            } while (reader.TokenType == JsonToken.PropertyName || await reader.ReadAsync(cancellationToken).ConfigureAwait(false));

            // don't read if current token is a property. token was already read when parsing element attributes
        }

        private async Task DeserializeValueAsync(JsonReader reader, IXmlDocument document, XmlNamespaceManager manager, string propertyName, IXmlNode currentNode, CancellationToken cancellationToken)
        {
            switch (propertyName)
            {
                case TextName:
                    currentNode.AppendChild(document.CreateTextNode(reader.Value.ToString()));
                    break;
                case CDataName:
                    currentNode.AppendChild(document.CreateCDataSection(reader.Value.ToString()));
                    break;
                case WhitespaceName:
                    currentNode.AppendChild(document.CreateWhitespace(reader.Value.ToString()));
                    break;
                case SignificantWhitespaceName:
                    currentNode.AppendChild(document.CreateSignificantWhitespace(reader.Value.ToString()));
                    break;
                default:

                    // processing instructions and the xml declaration start with ?
                    if (!string.IsNullOrEmpty(propertyName) && propertyName[0] == '?')
                    {
                        await CreateInstructionAsync(reader, document, currentNode, propertyName, cancellationToken).ConfigureAwait(false);
                    }
                    else if (string.Equals(propertyName, "!DOCTYPE", StringComparison.OrdinalIgnoreCase))
                    {
                        await CreateDocumentTypeAsync(reader, document, currentNode, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            // handle nested arrays
                            await ReadArrayElementsAsync(reader, document, propertyName, currentNode, manager, cancellationToken).ConfigureAwait(false);
                            return;
                        }

                        // have to wait until attributes have been parsed before creating element
                        // attributes may contain namespace info used by the element
                        await ReadElementAsync(reader, document, currentNode, propertyName, manager, cancellationToken).ConfigureAwait(false);
                    }

                    break;
            }
        }

        private static async Task CreateInstructionAsync(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName, CancellationToken cancellationToken)
        {
            if (propertyName == DeclarationName)
            {
                string version = null;
                string encoding = null;
                string standalone = null;
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && reader.TokenType != JsonToken.EndObject)
                {
                    switch (reader.Value.ToString())
                    {
                        case "@version":
                            await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                            version = reader.Value.ToString();
                            break;
                        case "@encoding":
                            await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                            encoding = reader.Value.ToString();
                            break;
                        case "@standalone":
                            await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                            standalone = reader.Value.ToString();
                            break;
                        default:
                            throw JsonSerializationException.Create(reader, "Unexpected property name encountered while deserializing XmlDeclaration: " + reader.Value);
                    }
                }

                IXmlNode declaration = document.CreateXmlDeclaration(version, encoding, standalone);
                currentNode.AppendChild(declaration);
            }
            else
            {
                IXmlNode instruction = document.CreateProcessingInstruction(propertyName.Substring(1), reader.Value.ToString());
                currentNode.AppendChild(instruction);
            }
        }

        private static async Task CreateDocumentTypeAsync(JsonReader reader, IXmlDocument document, IXmlNode currentNode, CancellationToken cancellationToken)
        {
            string name = null;
            string publicId = null;
            string systemId = null;
            string internalSubset = null;
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.Value.ToString())
                {
                    case "@name":
                        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                        name = reader.Value.ToString();
                        break;
                    case "@public":
                        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                        publicId = reader.Value.ToString();
                        break;
                    case "@system":
                        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                        systemId = reader.Value.ToString();
                        break;
                    case "@internalSubset":
                        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                        internalSubset = reader.Value.ToString();
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected property name encountered while deserializing XmlDeclaration: " + reader.Value);
                }
            }

            IXmlNode documentType = document.CreateXmlDocumentType(name, publicId, systemId, internalSubset);
            currentNode.AppendChild(documentType);
        }

        private async Task ReadArrayElementsAsync(JsonReader reader, IXmlDocument document, string propertyName, IXmlNode currentNode, XmlNamespaceManager manager, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string elementPrefix = MiscellaneousUtils.GetPrefix(propertyName);

            IXmlElement nestedArrayElement = CreateElement(propertyName, document, elementPrefix, manager);

            currentNode.AppendChild(nestedArrayElement);

            int count = 0;
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && reader.TokenType != JsonToken.EndArray)
            {
                await DeserializeValueAsync(reader, document, manager, propertyName, nestedArrayElement, cancellationToken).ConfigureAwait(false);
                count++;
            }

            if (WriteArrayAttribute)
            {
                AddJsonArrayAttribute(nestedArrayElement, document);
            }

            if (count == 1 && WriteArrayAttribute)
            {
                foreach (IXmlNode childNode in nestedArrayElement.ChildNodes)
                {
                    IXmlElement element = childNode as IXmlElement;
                    if (element != null && element.LocalName == propertyName)
                    {
                        AddJsonArrayAttribute(element, document);
                        break;
                    }
                }
            }
        }
    }
}

#endif
