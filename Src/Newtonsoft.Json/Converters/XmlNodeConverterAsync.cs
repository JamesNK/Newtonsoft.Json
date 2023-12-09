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
#if HAVE_ASYNC

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using Newtonsoft.Json.Utilities;
using System.Xml.Linq;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Converters
{
    public partial class XmlNodeConverter
    {

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public override async Task WriteJsonAsync(JsonWriter writer, object? value, JsonSerializer serializer, CancellationToken cancellationToken = default)
        {
            if (value == null)
            {
                await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

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

        private async Task SerializeGroupedNodesAsync(JsonWriter writer, IXmlNode node, XmlNamespaceManager manager, bool writePropertyName, CancellationToken cancellationToken)
        {
            switch (node.ChildNodes.Count)
            {
                case 0:
                    {
                        // nothing to serialize
                        break;
                    }
                case 1:
                    {
                        // avoid grouping when there is only one node
                        string nodeName = GetPropertyName(node.ChildNodes[0], manager);
                        await WriteGroupedNodesAsync(writer, manager, writePropertyName, node.ChildNodes, nodeName, cancellationToken).ConfigureAwait(false);
                        break;
                    }
                default:
                    {
                        // check whether nodes have the same name
                        // if they don't then group into dictionary together by name

                        // value of dictionary will be a single IXmlNode when there is one for a name,
                        // or a List<IXmlNode> when there are multiple
                        Dictionary<string, object>? nodesGroupedByName = null;

                        string? nodeName = null;

                        for (int i = 0; i < node.ChildNodes.Count; i++)
                        {
                            IXmlNode childNode = node.ChildNodes[i];
                            string currentNodeName = GetPropertyName(childNode, manager);

                            if (nodesGroupedByName == null)
                            {
                                if (nodeName == null)
                                {
                                    nodeName = currentNodeName;
                                }
                                else if (currentNodeName == nodeName)
                                {
                                    // current node name matches others
                                }
                                else
                                {
                                    nodesGroupedByName = new Dictionary<string, object>();
                                    if (i > 1)
                                    {
                                        List<IXmlNode> nodes = new List<IXmlNode>(i);
                                        for (int j = 0; j < i; j++)
                                        {
                                            nodes.Add(node.ChildNodes[j]);
                                        }
                                        nodesGroupedByName.Add(nodeName, nodes);
                                    }
                                    else
                                    {
                                        nodesGroupedByName.Add(nodeName, node.ChildNodes[0]);
                                    }
                                    nodesGroupedByName.Add(currentNodeName, childNode);
                                }
                            }
                            else
                            {
                                if (!nodesGroupedByName.TryGetValue(currentNodeName, out object? value))
                                {
                                    nodesGroupedByName.Add(currentNodeName, childNode);
                                }
                                else
                                {
                                    if (!(value is List<IXmlNode> nodes))
                                    {
                                        nodes = new List<IXmlNode> { (IXmlNode)value! };
                                        nodesGroupedByName[currentNodeName] = nodes;
                                    }

                                    nodes.Add(childNode);
                                }
                            }
                        }

                        if (nodesGroupedByName == null)
                        {
                            await WriteGroupedNodesAsync(writer, manager, writePropertyName, node.ChildNodes, nodeName!, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            // loop through grouped nodes. write single name instances as normal,
                            // write multiple names together in an array
                            foreach (KeyValuePair<string, object> nodeNameGroup in nodesGroupedByName)
                            {
                                if (nodeNameGroup.Value is List<IXmlNode> nodes)
                                {
                                    await WriteGroupedNodesAsync(writer, manager, writePropertyName, nodes, nodeNameGroup.Key, cancellationToken).ConfigureAwait(false);
                                }
                                else
                                {
                                    await WriteGroupedNodesAsync(writer, manager, writePropertyName, (IXmlNode)nodeNameGroup.Value, nodeNameGroup.Key, cancellationToken).ConfigureAwait(false);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        private async Task WriteGroupedNodesAsync(JsonWriter writer, XmlNamespaceManager manager, bool writePropertyName, List<IXmlNode> groupedNodes, string elementNames, CancellationToken cancellationToken)
        {
            bool writeArray = groupedNodes.Count != 1 || IsArray(groupedNodes[0]);

            if (!writeArray)
            {
                await SerializeNodeAsync(writer, groupedNodes[0], manager, writePropertyName, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (writePropertyName)
                {
                    await writer.WritePropertyNameAsync(elementNames, cancellationToken).ConfigureAwait(false);
                }

                await writer.WriteStartArrayAsync(cancellationToken).ConfigureAwait(false);

                for (int i = 0; i < groupedNodes.Count; i++)
                {
                    await SerializeNodeAsync(writer, groupedNodes[i], manager, false, cancellationToken).ConfigureAwait(false);
                }

                await writer.WriteEndArrayAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task WriteGroupedNodesAsync(JsonWriter writer, XmlNamespaceManager manager, bool writePropertyName, IXmlNode node, string elementNames, CancellationToken cancellationToken)
        {
            bool writeArray = IsArray(node);

            if (!writeArray)
            {
                await SerializeNodeAsync(writer, node, manager, writePropertyName, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (writePropertyName)
                {
                    await writer.WritePropertyNameAsync(elementNames, cancellationToken).ConfigureAwait(false);
                }

                await writer.WriteStartArrayAsync(cancellationToken).ConfigureAwait(false);

                await SerializeNodeAsync(writer, node, manager, false, cancellationToken).ConfigureAwait(false);

                await writer.WriteEndArrayAsync(cancellationToken).ConfigureAwait(false);
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
                                string namespacePrefix = (attribute.LocalName != "xmlns")
                                    ? XmlConvert.DecodeName(attribute.LocalName)!
                                    : string.Empty;
                                string? namespaceUri = attribute.Value;
                                if (namespaceUri == null)
                                {
                                    throw new JsonSerializationException("Namespace attribute must have a value.");
                                }

                                manager.AddNamespace(namespacePrefix, namespaceUri);
                            }
                        }

                        if (writePropertyName)
                        {
                            await writer.WritePropertyNameAsync(GetPropertyName(node, manager), cancellationToken).ConfigureAwait(false);
                        }

                        if (!ValueAttributes(node.Attributes) && node.ChildNodes.Count == 1
                            && node.ChildNodes[0].NodeType == XmlNodeType.Text)
                        {
                            // write elements with a single text child as a name value pair
                            await writer.WriteValueAsync(node.ChildNodes[0].Value).ConfigureAwait(false);
                        }
                        else if (node.ChildNodes.Count == 0 && node.Attributes.Count == 0)
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

                    if (!StringUtils.IsNullOrEmpty(declaration.Version))
                    {
                        await writer.WritePropertyNameAsync("@version", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(declaration.Version, cancellationToken).ConfigureAwait(false);
                    }
                    if (!StringUtils.IsNullOrEmpty(declaration.Encoding))
                    {
                        await writer.WritePropertyNameAsync("@encoding", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(declaration.Encoding, cancellationToken).ConfigureAwait(false);
                    }
                    if (!StringUtils.IsNullOrEmpty(declaration.Standalone))
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

                    if (!StringUtils.IsNullOrEmpty(documentType.Name))
                    {
                        await writer.WritePropertyNameAsync("@name", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(documentType.Name, cancellationToken).ConfigureAwait(false);
                    }
                    if (!StringUtils.IsNullOrEmpty(documentType.Public))
                    {
                        await writer.WritePropertyNameAsync("@public", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(documentType.Public, cancellationToken).ConfigureAwait(false);
                    }
                    if (!StringUtils.IsNullOrEmpty(documentType.System))
                    {
                        await writer.WritePropertyNameAsync("@system", cancellationToken).ConfigureAwait(false);
                        await writer.WriteValueAsync(documentType.System, cancellationToken).ConfigureAwait(false);
                    }
                    if (!StringUtils.IsNullOrEmpty(documentType.InternalSubset))
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



        #region Reading
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The object value.</returns>
        public override async Task<object?> ReadJsonAsync(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.StartObject:
                    break;
                default:
                    throw JsonSerializationException.Create(reader, "XmlNodeConverter can only convert JSON that begins with an object.");
            }

            XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
            IXmlDocument? document = null;
            IXmlNode? rootNode = null;

#if HAVE_XLINQ
            if (typeof(XObject).IsAssignableFrom(objectType))
            {
                if (objectType != typeof(XContainer)
                    && objectType != typeof(XDocument)
                    && objectType != typeof(XElement)
                    && objectType != typeof(XNode)
                    && objectType != typeof(XObject))
                {
                    throw JsonSerializationException.Create(reader, "XmlNodeConverter only supports deserializing XDocument, XElement, XContainer, XNode or XObject.");
                }

                XDocument d = new XDocument();
                document = new XDocumentWrapper(d);
                rootNode = document;
            }
#endif
#if HAVE_XML_DOCUMENT
            if (typeof(XmlNode).IsAssignableFrom(objectType))
            {
                if (objectType != typeof(XmlDocument)
                    && objectType != typeof(XmlElement)
                    && objectType != typeof(XmlNode))
                {
                    throw JsonSerializationException.Create(reader, "XmlNodeConverter only supports deserializing XmlDocument, XmlElement or XmlNode.");
                }

                XmlDocument d = new XmlDocument();
#if HAVE_XML_DOCUMENT_TYPE
                // prevent http request when resolving any DTD references
                d.XmlResolver = null;
#endif

                document = new XmlDocumentWrapper(d);
                rootNode = document;
            }
#endif

            if (document == null || rootNode == null)
            {
                throw JsonSerializationException.Create(reader, "Unexpected type when converting XML: " + objectType);
            }

            if (!StringUtils.IsNullOrEmpty(DeserializeRootElementName))
            {
                await ReadElementAsync(reader, document, rootNode, DeserializeRootElementName, manager, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                await DeserializeNodeAsync(reader, document, manager, rootNode, cancellationToken).ConfigureAwait(false);
            }

#if HAVE_XLINQ
            if (objectType == typeof(XElement))
            {
                XElement element = (XElement)document.DocumentElement!.WrappedNode!;
                element.Remove();

                return element;
            }
#endif
#if HAVE_XML_DOCUMENT
            if (objectType == typeof(XmlElement))
            {
                return document.DocumentElement!.WrappedNode;
            }
#endif

            return document.WrappedNode;
        }

        private async Task DeserializeValueAsync(JsonReader reader, IXmlDocument document, XmlNamespaceManager manager, string propertyName, IXmlNode currentNode, CancellationToken cancellationToken)
        {
            if (!EncodeSpecialCharacters)
            {
                switch (propertyName)
                {
                    case TextName:
                        currentNode.AppendChild(document.CreateTextNode(ConvertTokenToXmlValue(reader)));
                        return;
                    case CDataName:
                        currentNode.AppendChild(document.CreateCDataSection(ConvertTokenToXmlValue(reader)));
                        return;
                    case WhitespaceName:
                        currentNode.AppendChild(document.CreateWhitespace(ConvertTokenToXmlValue(reader)));
                        return;
                    case SignificantWhitespaceName:
                        currentNode.AppendChild(document.CreateSignificantWhitespace(ConvertTokenToXmlValue(reader)));
                        return;
                    default:
                        // processing instructions and the xml declaration start with ?
                        if (!StringUtils.IsNullOrEmpty(propertyName) && propertyName[0] == '?')
                        {
                            await CreateInstructionAsync(reader, document, currentNode, propertyName, cancellationToken).ConfigureAwait(false);
                            return;
                        }
#if HAVE_XML_DOCUMENT_TYPE
                        else if (string.Equals(propertyName, "!DOCTYPE", StringComparison.OrdinalIgnoreCase))
                        {
                            await CreateDocumentTypeAsync(reader, document, currentNode, cancellationToken).ConfigureAwait(false);
                            return;
                        }
#endif
                        break;
                }
            }

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

        private async Task ReadElementAsync(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName, XmlNamespaceManager manager, CancellationToken cancellationToken)
        {
            if (StringUtils.IsNullOrEmpty(propertyName))
            {
                throw JsonSerializationException.Create(reader, "XmlNodeConverter cannot convert JSON with an empty property name to XML.");
            }

            Dictionary<string, string?>? attributeNameValues = null;
            string? elementPrefix = null;

            if (!EncodeSpecialCharacters)
            {
                attributeNameValues = ShouldReadInto(reader)
                    ? await ReadAttributeElementsAsync(reader, manager, cancellationToken).ConfigureAwait(false)
                    : null;
                elementPrefix = MiscellaneousUtils.GetPrefix(propertyName);

                if (propertyName.StartsWith('@'))
                {
                    string attributeName = propertyName.Substring(1);
                    string? attributePrefix = MiscellaneousUtils.GetPrefix(attributeName);

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
                            string? attributePrefix = manager.LookupPrefix(JsonNamespaceUri);
                            AddAttribute(reader, document, currentNode, propertyName, attributeName, manager, attributePrefix);
                            return;
                    }
                }
            }
            else
            {
                if (ShouldReadInto(reader))
                {
                    await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            await CreateElementAsync(reader, document, currentNode, propertyName, manager, elementPrefix, attributeNameValues, cancellationToken).ConfigureAwait(false);
        }

        private async Task CreateElementAsync(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string elementName, XmlNamespaceManager manager, string? elementPrefix, Dictionary<string, string?>? attributeNameValues, CancellationToken cancellationToken)
        {
            IXmlElement element = CreateElement(elementName, document, elementPrefix, manager);

            currentNode.AppendChild(element);

            if (attributeNameValues != null)
            {
                // add attributes to newly created element
                foreach (KeyValuePair<string, string?> nameValue in attributeNameValues)
                {
                    string encodedName = XmlConvert.EncodeName(nameValue.Key);
                    string? attributePrefix = MiscellaneousUtils.GetPrefix(nameValue.Key);

                    IXmlNode attribute = (!StringUtils.IsNullOrEmpty(attributePrefix))
                        ? document.CreateAttribute(encodedName, manager.LookupNamespace(attributePrefix) ?? string.Empty, nameValue.Value!)
                        : document.CreateAttribute(encodedName, nameValue.Value!);

                    element.SetAttributeNode(attribute);
                }
            }

            switch (reader.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Boolean:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    string? text = ConvertTokenToXmlValue(reader);
                    if (text != null)
                    {
                        element.AppendChild(document.CreateTextNode(text));
                    }
                    break;
                case JsonToken.Null:

                    // empty element. do nothing
                    break;
                case JsonToken.EndObject:

                    // finished element will have no children to deserialize
                    manager.RemoveNamespace(string.Empty, manager.DefaultNamespace);
                    break;
                default:
                    manager.PushScope();
                    await DeserializeNodeAsync(reader, document, manager, element, cancellationToken).ConfigureAwait(false);
                    manager.PopScope();
                    manager.RemoveNamespace(string.Empty, manager.DefaultNamespace);
                    break;
            }
        }        
        
        private async Task ReadArrayElementsAsync(JsonReader reader, IXmlDocument document, string propertyName, IXmlNode currentNode, XmlNamespaceManager manager, CancellationToken cancellationToken)
        {
            string? elementPrefix = MiscellaneousUtils.GetPrefix(propertyName);

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
                    if (childNode is IXmlElement element && element.LocalName == propertyName)
                    {
                        AddJsonArrayAttribute(element, document);
                        break;
                    }
                }
            }
        }
        

        private async Task<Dictionary<string, string?>?> ReadAttributeElementsAsync(JsonReader reader, XmlNamespaceManager manager, CancellationToken cancellationToken)
        {
            Dictionary<string, string?>? attributeNameValues = null;
            bool finished = false;

            // read properties until first non-attribute is encountered
            while (!finished && await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string attributeName = reader.Value!.ToString()!;

                        if (!StringUtils.IsNullOrEmpty(attributeName))
                        {
                            char firstChar = attributeName[0];
                            string? attributeValue;

                            switch (firstChar)
                            {
                                case '@':
                                    if (attributeNameValues == null)
                                    {
                                        attributeNameValues = new Dictionary<string, string?>();
                                    }

                                    attributeName = attributeName.Substring(1);
                                    await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                                    attributeValue = ConvertTokenToXmlValue(reader);
                                    attributeNameValues.Add(attributeName, attributeValue);

                                    if (IsNamespaceAttribute(attributeName, out string? namespacePrefix))
                                    {
                                        manager.AddNamespace(namespacePrefix, attributeValue!);
                                    }
                                    break;
                                case '$':
                                    switch (attributeName)
                                    {
                                        case JsonTypeReflector.ArrayValuesPropertyName:
                                        case JsonTypeReflector.IdPropertyName:
                                        case JsonTypeReflector.RefPropertyName:
                                        case JsonTypeReflector.TypePropertyName:
                                        case JsonTypeReflector.ValuePropertyName:
                                            // check that JsonNamespaceUri is in scope
                                            // if it isn't then add it to document and namespace manager
                                            string? jsonPrefix = manager.LookupPrefix(JsonNamespaceUri);
                                            if (jsonPrefix == null)
                                            {
                                                if (attributeNameValues == null)
                                                {
                                                    attributeNameValues = new Dictionary<string, string?>();
                                                }

                                                // ensure that the prefix used is free
                                                int? i = null;
                                                while (manager.LookupNamespace("json" + i) != null)
                                                {
                                                    i = i.GetValueOrDefault() + 1;
                                                }
                                                jsonPrefix = "json" + i;

                                                attributeNameValues.Add("xmlns:" + jsonPrefix, JsonNamespaceUri);
                                                manager.AddNamespace(jsonPrefix, JsonNamespaceUri);
                                            }

                                            // special case $values, it will have a non-primitive value
                                            if (attributeName == JsonTypeReflector.ArrayValuesPropertyName)
                                            {
                                                finished = true;
                                                break;
                                            }

                                            attributeName = attributeName.Substring(1);
                                            await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

                                            if (!JsonTokenUtils.IsPrimitiveToken(reader.TokenType))
                                            {
                                                throw JsonSerializationException.Create(reader, "Unexpected JsonToken: " + reader.TokenType);
                                            }

                                            if (attributeNameValues == null)
                                            {
                                                attributeNameValues = new Dictionary<string, string?>();
                                            }

                                            attributeValue = reader.Value?.ToString();
                                            attributeNameValues.Add(jsonPrefix + ":" + attributeName, attributeValue);
                                            break;
                                        default:
                                            finished = true;
                                            break;
                                    }
                                    break;
                                default:
                                    finished = true;
                                    break;
                            }
                        }
                        else
                        {
                            finished = true;
                        }

                        break;
                    case JsonToken.EndObject:
                    case JsonToken.Comment:
                        finished = true;
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected JsonToken: " + reader.TokenType);
                }
            }

            return attributeNameValues;
        }

        private async Task CreateInstructionAsync(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName, CancellationToken cancellationToken)
        {
            if (propertyName == DeclarationName)
            {
                string? version = null;
                string? encoding = null;
                string? standalone = null;
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && reader.TokenType != JsonToken.EndObject)
                {
                    switch (reader.Value?.ToString())
                    {
                        case "@version":
                            await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                            version = ConvertTokenToXmlValue(reader);
                            break;
                        case "@encoding":
                            await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                            encoding = ConvertTokenToXmlValue(reader);
                            break;
                        case "@standalone":
                            await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                            standalone = ConvertTokenToXmlValue(reader);
                            break;
                        default:
                            throw JsonSerializationException.Create(reader, "Unexpected property name encountered while deserializing XmlDeclaration: " + reader.Value);
                    }
                }

                if (version == null)
                {
                    throw JsonSerializationException.Create(reader, "Version not specified for XML declaration.");
                }

                IXmlNode declaration = document.CreateXmlDeclaration(version, encoding, standalone);
                currentNode.AppendChild(declaration);
            }
            else
            {
                IXmlNode instruction = document.CreateProcessingInstruction(propertyName.Substring(1), ConvertTokenToXmlValue(reader)!);
                currentNode.AppendChild(instruction);
            }
        }

#if HAVE_XML_DOCUMENT_TYPE
        private async Task CreateDocumentTypeAsync(JsonReader reader, IXmlDocument document, IXmlNode currentNode, CancellationToken cancellationToken)
        {
            string? name = null;
            string? publicId = null;
            string? systemId = null;
            string? internalSubset = null;
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.Value?.ToString())
                {
                    case "@name":
                        await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                        name = ConvertTokenToXmlValue(reader);
                        break;
                    case "@public":
                        await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                        publicId = ConvertTokenToXmlValue(reader);
                        break;
                    case "@system":
                        await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                        systemId = ConvertTokenToXmlValue(reader);
                        break;
                    case "@internalSubset":
                        await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                        internalSubset = ConvertTokenToXmlValue(reader);
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected property name encountered while deserializing XmlDeclaration: " + reader.Value);
                }
            }

            if (name == null)
            {
                throw JsonSerializationException.Create(reader, "Name not specified for XML document type.");
            }

            IXmlNode documentType = document.CreateXmlDocumentType(name, publicId, systemId, internalSubset);
            currentNode.AppendChild(documentType);
        }
#endif

        private async Task DeserializeNodeAsync(JsonReader reader, IXmlDocument document, XmlNamespaceManager manager, IXmlNode currentNode, CancellationToken cancellationToken)
        {
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        if (currentNode.NodeType == XmlNodeType.Document && document.DocumentElement != null)
                        {
                            throw JsonSerializationException.Create(reader, "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifying a DeserializeRootElementName.");
                        }

                        string propertyName = reader.Value!.ToString()!;
                        await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

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
                                MiscellaneousUtils.GetQualifiedNameParts(propertyName, out string? elementPrefix, out string localName);
                                string? ns = StringUtils.IsNullOrEmpty(elementPrefix) ? manager.DefaultNamespace : manager.LookupNamespace(elementPrefix);

                                foreach (IXmlNode childNode in currentNode.ChildNodes)
                                {
                                    if (childNode is IXmlElement element && element.LocalName == localName && element.NamespaceUri == ns)
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
                        continue;
                    case JsonToken.StartConstructor:
                        string constructorName = reader.Value!.ToString()!;

                        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && reader.TokenType != JsonToken.EndConstructor)
                        {
                            await DeserializeValueAsync(reader, document, manager, constructorName, currentNode, cancellationToken).ConfigureAwait(false);
                        }
                        break;
                    case JsonToken.Comment:
                        currentNode.AppendChild(document.CreateComment((string)reader.Value!));
                        break;
                    case JsonToken.EndObject:
                    case JsonToken.EndArray:
                        return;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected JsonToken when deserializing node: " + reader.TokenType);
                }
            } while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false));
            // don't read if current token is a property. token was already read when parsing element attributes
        }
        
        #endregion
    }

}


#endif