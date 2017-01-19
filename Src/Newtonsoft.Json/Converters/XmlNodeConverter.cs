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

#if (HAVE_XML_DOCUMENT || HAVE_XLINQ)

#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Newtonsoft.Json.Serialization;
#if HAVE_XLINQ
using System.Xml.Linq;
#endif
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    #region XmlNodeWrappers
#if HAVE_XML_DOCUMENT
    internal class XmlDocumentWrapper : XmlNodeWrapper, IXmlDocument
    {
        private readonly XmlDocument _document;

        public XmlDocumentWrapper(XmlDocument document)
            : base(document)
        {
            _document = document;
        }

        public IXmlNode CreateComment(string data)
        {
            return new XmlNodeWrapper(_document.CreateComment(data));
        }

        public IXmlNode CreateTextNode(string text)
        {
            return new XmlNodeWrapper(_document.CreateTextNode(text));
        }

        public IXmlNode CreateCDataSection(string data)
        {
            return new XmlNodeWrapper(_document.CreateCDataSection(data));
        }

        public IXmlNode CreateWhitespace(string text)
        {
            return new XmlNodeWrapper(_document.CreateWhitespace(text));
        }

        public IXmlNode CreateSignificantWhitespace(string text)
        {
            return new XmlNodeWrapper(_document.CreateSignificantWhitespace(text));
        }

        public IXmlNode CreateXmlDeclaration(string version, string encoding, string standalone)
        {
            return new XmlDeclarationWrapper(_document.CreateXmlDeclaration(version, encoding, standalone));
        }

        public IXmlNode CreateXmlDocumentType(string name, string publicId, string systemId, string internalSubset)
        {
            return new XmlDocumentTypeWrapper(_document.CreateDocumentType(name, publicId, systemId, null));
        }

        public IXmlNode CreateProcessingInstruction(string target, string data)
        {
            return new XmlNodeWrapper(_document.CreateProcessingInstruction(target, data));
        }

        public IXmlElement CreateElement(string elementName)
        {
            return new XmlElementWrapper(_document.CreateElement(elementName));
        }

        public IXmlElement CreateElement(string qualifiedName, string namespaceUri)
        {
            return new XmlElementWrapper(_document.CreateElement(qualifiedName, namespaceUri));
        }

        public IXmlNode CreateAttribute(string name, string value)
        {
            XmlNodeWrapper attribute = new XmlNodeWrapper(_document.CreateAttribute(name));
            attribute.Value = value;

            return attribute;
        }

        public IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string value)
        {
            XmlNodeWrapper attribute = new XmlNodeWrapper(_document.CreateAttribute(qualifiedName, namespaceUri));
            attribute.Value = value;

            return attribute;
        }

        public IXmlElement DocumentElement
        {
            get
            {
                if (_document.DocumentElement == null)
                {
                    return null;
                }

                return new XmlElementWrapper(_document.DocumentElement);
            }
        }
    }

    internal class XmlElementWrapper : XmlNodeWrapper, IXmlElement
    {
        private readonly XmlElement _element;

        public XmlElementWrapper(XmlElement element)
            : base(element)
        {
            _element = element;
        }

        public void SetAttributeNode(IXmlNode attribute)
        {
            XmlNodeWrapper xmlAttributeWrapper = (XmlNodeWrapper)attribute;

            _element.SetAttributeNode((XmlAttribute)xmlAttributeWrapper.WrappedNode);
        }

        public string GetPrefixOfNamespace(string namespaceUri)
        {
            return _element.GetPrefixOfNamespace(namespaceUri);
        }

        public bool IsEmpty
        {
            get { return _element.IsEmpty; }
        }
    }

    internal class XmlDeclarationWrapper : XmlNodeWrapper, IXmlDeclaration
    {
        private readonly XmlDeclaration _declaration;

        public XmlDeclarationWrapper(XmlDeclaration declaration)
            : base(declaration)
        {
            _declaration = declaration;
        }

        public string Version
        {
            get { return _declaration.Version; }
        }

        public string Encoding
        {
            get { return _declaration.Encoding; }
            set { _declaration.Encoding = value; }
        }

        public string Standalone
        {
            get { return _declaration.Standalone; }
            set { _declaration.Standalone = value; }
        }
    }

    internal class XmlDocumentTypeWrapper : XmlNodeWrapper, IXmlDocumentType
    {
        private readonly XmlDocumentType _documentType;

        public XmlDocumentTypeWrapper(XmlDocumentType documentType)
            : base(documentType)
        {
            _documentType = documentType;
        }

        public string Name
        {
            get { return _documentType.Name; }
        }

        public string System
        {
            get { return _documentType.SystemId; }
        }

        public string Public
        {
            get { return _documentType.PublicId; }
        }

        public string InternalSubset
        {
            get { return _documentType.InternalSubset; }
        }

        public override string LocalName
        {
            get { return "DOCTYPE"; }
        }
    }

    internal class XmlNodeWrapper : IXmlNode
    {
        private readonly XmlNode _node;
        private List<IXmlNode> _childNodes;
        private List<IXmlNode> _attributes;

        public XmlNodeWrapper(XmlNode node)
        {
            _node = node;
        }

        public object WrappedNode
        {
            get { return _node; }
        }

        public XmlNodeType NodeType
        {
            get { return _node.NodeType; }
        }

        public virtual string LocalName
        {
            get { return _node.LocalName; }
        }

        public List<IXmlNode> ChildNodes
        {
            get
            {
                // childnodes is read multiple times
                // cache results to prevent multiple reads which kills perf in large documents
                if (_childNodes == null)
                {
                    if (!_node.HasChildNodes)
                    {
                        _childNodes = XmlNodeConverter.EmptyChildNodes;
                    }
                    else
                    {
                        _childNodes = new List<IXmlNode>(_node.ChildNodes.Count);
                        foreach (XmlNode childNode in _node.ChildNodes)
                        {
                            _childNodes.Add(WrapNode(childNode));
                        }
                    }
                }

                return _childNodes;
            }
        }

        protected virtual bool HasChildNodes
        {
            get { return _node.HasChildNodes; }
        }

        internal static IXmlNode WrapNode(XmlNode node)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    return new XmlElementWrapper((XmlElement)node);
                case XmlNodeType.XmlDeclaration:
                    return new XmlDeclarationWrapper((XmlDeclaration)node);
                case XmlNodeType.DocumentType:
                    return new XmlDocumentTypeWrapper((XmlDocumentType)node);
                default:
                    return new XmlNodeWrapper(node);
            }
        }

        public List<IXmlNode> Attributes
        {
            get
            {
                // attributes is read multiple times
                // cache results to prevent multiple reads which kills perf in large documents
                if (_attributes == null)
                {
                    if (!HasAttributes)
                    {
                        _attributes = XmlNodeConverter.EmptyChildNodes;
                    }
                    else
                    {
                        _attributes = new List<IXmlNode>(_node.Attributes.Count);
                        foreach (XmlAttribute attribute in _node.Attributes)
                        {
                            _attributes.Add(WrapNode(attribute));
                        }
                    }
                }

                return _attributes;
            }
        }

        private bool HasAttributes
        {
            get
            {
                XmlElement element = _node as XmlElement;
                if (element != null)
                {
                    return element.HasAttributes;
                }

                return _node.Attributes?.Count > 0;
            }
        }

        public IXmlNode ParentNode
        {
            get
            {
                XmlAttribute attribute = _node as XmlAttribute;
                XmlNode node = attribute != null ? attribute.OwnerElement : _node.ParentNode;

                if (node == null)
                {
                    return null;
                }

                return WrapNode(node);
            }
        }

        public string Value
        {
            get { return _node.Value; }
            set { _node.Value = value; }
        }

        public IXmlNode AppendChild(IXmlNode newChild)
        {
            XmlNodeWrapper xmlNodeWrapper = (XmlNodeWrapper)newChild;
            _node.AppendChild(xmlNodeWrapper._node);
            _childNodes = null;
            _attributes = null;

            return newChild;
        }

        public string NamespaceUri
        {
            get { return _node.NamespaceURI; }
        }
    }
#endif
    #endregion

    #region Interfaces
    internal interface IXmlDocument : IXmlNode
    {
        IXmlNode CreateComment(string text);
        IXmlNode CreateTextNode(string text);
        IXmlNode CreateCDataSection(string data);
        IXmlNode CreateWhitespace(string text);
        IXmlNode CreateSignificantWhitespace(string text);
        IXmlNode CreateXmlDeclaration(string version, string encoding, string standalone);
        IXmlNode CreateXmlDocumentType(string name, string publicId, string systemId, string internalSubset);
        IXmlNode CreateProcessingInstruction(string target, string data);
        IXmlElement CreateElement(string elementName);
        IXmlElement CreateElement(string qualifiedName, string namespaceUri);
        IXmlNode CreateAttribute(string name, string value);
        IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string value);

        IXmlElement DocumentElement { get; }
    }

    internal interface IXmlDeclaration : IXmlNode
    {
        string Version { get; }
        string Encoding { get; set; }
        string Standalone { get; set; }
    }

    internal interface IXmlDocumentType : IXmlNode
    {
        string Name { get; }
        string System { get; }
        string Public { get; }
        string InternalSubset { get; }
    }

    internal interface IXmlElement : IXmlNode
    {
        void SetAttributeNode(IXmlNode attribute);
        string GetPrefixOfNamespace(string namespaceUri);
        bool IsEmpty { get; }
    }

    internal interface IXmlNode
    {
        XmlNodeType NodeType { get; }
        string LocalName { get; }
        List<IXmlNode> ChildNodes { get; }
        List<IXmlNode> Attributes { get; }
        IXmlNode ParentNode { get; }
        string Value { get; set; }
        IXmlNode AppendChild(IXmlNode newChild);
        string NamespaceUri { get; }
        object WrappedNode { get; }
    }
    #endregion

    #region XNodeWrappers
#if HAVE_XLINQ
    internal class XDeclarationWrapper : XObjectWrapper, IXmlDeclaration
    {
        internal XDeclaration Declaration { get; private set; }

        public XDeclarationWrapper(XDeclaration declaration)
            : base(null)
        {
            Declaration = declaration;
        }

        public override XmlNodeType NodeType
        {
            get { return XmlNodeType.XmlDeclaration; }
        }

        public string Version
        {
            get { return Declaration.Version; }
        }

        public string Encoding
        {
            get { return Declaration.Encoding; }
            set { Declaration.Encoding = value; }
        }

        public string Standalone
        {
            get { return Declaration.Standalone; }
            set { Declaration.Standalone = value; }
        }
    }

    internal class XDocumentTypeWrapper : XObjectWrapper, IXmlDocumentType
    {
        private readonly XDocumentType _documentType;

        public XDocumentTypeWrapper(XDocumentType documentType)
            : base(documentType)
        {
            _documentType = documentType;
        }

        public string Name
        {
            get { return _documentType.Name; }
        }

        public string System
        {
            get { return _documentType.SystemId; }
        }

        public string Public
        {
            get { return _documentType.PublicId; }
        }

        public string InternalSubset
        {
            get { return _documentType.InternalSubset; }
        }

        public override string LocalName
        {
            get { return "DOCTYPE"; }
        }
    }

    internal class XDocumentWrapper : XContainerWrapper, IXmlDocument
    {
        private XDocument Document
        {
            get { return (XDocument)WrappedNode; }
        }

        public XDocumentWrapper(XDocument document)
            : base(document)
        {
        }

        public override List<IXmlNode> ChildNodes
        {
            get
            {
                List<IXmlNode> childNodes = base.ChildNodes;
                if (Document.Declaration != null && (childNodes.Count == 0 || childNodes[0].NodeType != XmlNodeType.XmlDeclaration))
                {
                    childNodes.Insert(0, new XDeclarationWrapper(Document.Declaration));
                }

                return childNodes;
            }
        }

        protected override bool HasChildNodes
        {
            get
            {
                if (base.HasChildNodes)
                {
                    return true;
                }

                return Document.Declaration != null;
            }
        }

        public IXmlNode CreateComment(string text)
        {
            return new XObjectWrapper(new XComment(text));
        }

        public IXmlNode CreateTextNode(string text)
        {
            return new XObjectWrapper(new XText(text));
        }

        public IXmlNode CreateCDataSection(string data)
        {
            return new XObjectWrapper(new XCData(data));
        }

        public IXmlNode CreateWhitespace(string text)
        {
            return new XObjectWrapper(new XText(text));
        }

        public IXmlNode CreateSignificantWhitespace(string text)
        {
            return new XObjectWrapper(new XText(text));
        }

        public IXmlNode CreateXmlDeclaration(string version, string encoding, string standalone)
        {
            return new XDeclarationWrapper(new XDeclaration(version, encoding, standalone));
        }

        public IXmlNode CreateXmlDocumentType(string name, string publicId, string systemId, string internalSubset)
        {
            return new XDocumentTypeWrapper(new XDocumentType(name, publicId, systemId, internalSubset));
        }

        public IXmlNode CreateProcessingInstruction(string target, string data)
        {
            return new XProcessingInstructionWrapper(new XProcessingInstruction(target, data));
        }

        public IXmlElement CreateElement(string elementName)
        {
            return new XElementWrapper(new XElement(elementName));
        }

        public IXmlElement CreateElement(string qualifiedName, string namespaceUri)
        {
            string localName = MiscellaneousUtils.GetLocalName(qualifiedName);
            return new XElementWrapper(new XElement(XName.Get(localName, namespaceUri)));
        }

        public IXmlNode CreateAttribute(string name, string value)
        {
            return new XAttributeWrapper(new XAttribute(name, value));
        }

        public IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string value)
        {
            string localName = MiscellaneousUtils.GetLocalName(qualifiedName);
            return new XAttributeWrapper(new XAttribute(XName.Get(localName, namespaceUri), value));
        }

        public IXmlElement DocumentElement
        {
            get
            {
                if (Document.Root == null)
                {
                    return null;
                }

                return new XElementWrapper(Document.Root);
            }
        }

        public override IXmlNode AppendChild(IXmlNode newChild)
        {
            XDeclarationWrapper declarationWrapper = newChild as XDeclarationWrapper;
            if (declarationWrapper != null)
            {
                Document.Declaration = declarationWrapper.Declaration;
                return declarationWrapper;
            }
            else
            {
                return base.AppendChild(newChild);
            }
        }
    }

    internal class XTextWrapper : XObjectWrapper
    {
        private XText Text
        {
            get { return (XText)WrappedNode; }
        }

        public XTextWrapper(XText text)
            : base(text)
        {
        }

        public override string Value
        {
            get { return Text.Value; }
            set { Text.Value = value; }
        }

        public override IXmlNode ParentNode
        {
            get
            {
                if (Text.Parent == null)
                {
                    return null;
                }

                return XContainerWrapper.WrapNode(Text.Parent);
            }
        }
    }

    internal class XCommentWrapper : XObjectWrapper
    {
        private XComment Text
        {
            get { return (XComment)WrappedNode; }
        }

        public XCommentWrapper(XComment text)
            : base(text)
        {
        }

        public override string Value
        {
            get { return Text.Value; }
            set { Text.Value = value; }
        }

        public override IXmlNode ParentNode
        {
            get
            {
                if (Text.Parent == null)
                {
                    return null;
                }

                return XContainerWrapper.WrapNode(Text.Parent);
            }
        }
    }

    internal class XProcessingInstructionWrapper : XObjectWrapper
    {
        private XProcessingInstruction ProcessingInstruction
        {
            get { return (XProcessingInstruction)WrappedNode; }
        }

        public XProcessingInstructionWrapper(XProcessingInstruction processingInstruction)
            : base(processingInstruction)
        {
        }

        public override string LocalName
        {
            get { return ProcessingInstruction.Target; }
        }

        public override string Value
        {
            get { return ProcessingInstruction.Data; }
            set { ProcessingInstruction.Data = value; }
        }
    }

    internal class XContainerWrapper : XObjectWrapper
    {
        private List<IXmlNode> _childNodes;

        private XContainer Container
        {
            get { return (XContainer)WrappedNode; }
        }

        public XContainerWrapper(XContainer container)
            : base(container)
        {
        }

        public override List<IXmlNode> ChildNodes
        {
            get
            {
                // childnodes is read multiple times
                // cache results to prevent multiple reads which kills perf in large documents
                if (_childNodes == null)
                {
                    if (!HasChildNodes)
                    {
                        _childNodes = XmlNodeConverter.EmptyChildNodes;
                    }
                    else
                    {
                        _childNodes = new List<IXmlNode>();
                        foreach (XNode node in Container.Nodes())
                        {
                            _childNodes.Add(WrapNode(node));
                        }
                    }
                }

                return _childNodes;
            }
        }

        protected virtual bool HasChildNodes
        {
            get
            {
                // use last node for performance
                // container linked list starts with lastnode
                return Container.LastNode != null;
            }
        }

        public override IXmlNode ParentNode
        {
            get
            {
                if (Container.Parent == null)
                {
                    return null;
                }

                return WrapNode(Container.Parent);
            }
        }

        internal static IXmlNode WrapNode(XObject node)
        {
            XDocument document = node as XDocument;
            if (document != null)
            {
                return new XDocumentWrapper(document);
            }

            XElement element = node as XElement;
            if (element != null)
            {
                return new XElementWrapper(element);
            }

            XContainer container = node as XContainer;
            if (container != null)
            {
                return new XContainerWrapper(container);
            }

            XProcessingInstruction pi = node as XProcessingInstruction;
            if (pi != null)
            {
                return new XProcessingInstructionWrapper(pi);
            }

            XText text = node as XText;
            if (text != null)
            {
                return new XTextWrapper(text);
            }

            XComment comment = node as XComment;
            if (comment != null)
            {
                return new XCommentWrapper(comment);
            }

            XAttribute attribute = node as XAttribute;
            if (attribute != null)
            {
                return new XAttributeWrapper(attribute);
            }

            XDocumentType type = node as XDocumentType;
            if (type != null)
            {
                return new XDocumentTypeWrapper(type);
            }

            return new XObjectWrapper(node);
        }

        public override IXmlNode AppendChild(IXmlNode newChild)
        {
            Container.Add(newChild.WrappedNode);
            _childNodes = null;

            return newChild;
        }
    }

    internal class XObjectWrapper : IXmlNode
    {
        private readonly XObject _xmlObject;

        public XObjectWrapper(XObject xmlObject)
        {
            _xmlObject = xmlObject;
        }

        public object WrappedNode
        {
            get { return _xmlObject; }
        }

        public virtual XmlNodeType NodeType
        {
            get { return _xmlObject.NodeType; }
        }

        public virtual string LocalName
        {
            get { return null; }
        }

        public virtual List<IXmlNode> ChildNodes
        {
            get { return XmlNodeConverter.EmptyChildNodes; }
        }

        public virtual List<IXmlNode> Attributes
        {
            get { return XmlNodeConverter.EmptyChildNodes; }
        }

        public virtual IXmlNode ParentNode
        {
            get { return null; }
        }

        public virtual string Value
        {
            get { return null; }
            set { throw new InvalidOperationException(); }
        }

        public virtual IXmlNode AppendChild(IXmlNode newChild)
        {
            throw new InvalidOperationException();
        }

        public virtual string NamespaceUri
        {
            get { return null; }
        }
    }

    internal class XAttributeWrapper : XObjectWrapper
    {
        private XAttribute Attribute
        {
            get { return (XAttribute)WrappedNode; }
        }

        public XAttributeWrapper(XAttribute attribute)
            : base(attribute)
        {
        }

        public override string Value
        {
            get { return Attribute.Value; }
            set { Attribute.Value = value; }
        }

        public override string LocalName
        {
            get { return Attribute.Name.LocalName; }
        }

        public override string NamespaceUri
        {
            get { return Attribute.Name.NamespaceName; }
        }

        public override IXmlNode ParentNode
        {
            get
            {
                if (Attribute.Parent == null)
                {
                    return null;
                }

                return XContainerWrapper.WrapNode(Attribute.Parent);
            }
        }
    }

    internal class XElementWrapper : XContainerWrapper, IXmlElement
    {
        private List<IXmlNode> _attributes;

        private XElement Element
        {
            get { return (XElement)WrappedNode; }
        }

        public XElementWrapper(XElement element)
            : base(element)
        {
        }

        public void SetAttributeNode(IXmlNode attribute)
        {
            XObjectWrapper wrapper = (XObjectWrapper)attribute;
            Element.Add(wrapper.WrappedNode);
            _attributes = null;
        }

        public override List<IXmlNode> Attributes
        {
            get
            {
                // attributes is read multiple times
                // cache results to prevent multiple reads which kills perf in large documents
                if (_attributes == null)
                {
                    if (!Element.HasAttributes && !HasImplicitNamespaceAttribute(NamespaceUri))
                    {
                        _attributes = XmlNodeConverter.EmptyChildNodes;
                    }
                    else
                    {
                        _attributes = new List<IXmlNode>();
                        foreach (XAttribute attribute in Element.Attributes())
                        {
                            _attributes.Add(new XAttributeWrapper(attribute));
                        }

                        // ensure elements created with a namespace but no namespace attribute are converted correctly
                        // e.g. new XElement("{http://example.com}MyElement");
                        string namespaceUri = NamespaceUri;
                        if (HasImplicitNamespaceAttribute(namespaceUri))
                        {
                            _attributes.Insert(0, new XAttributeWrapper(new XAttribute("xmlns", namespaceUri)));
                        }
                    }
                }

                return _attributes;
            }
        }

        private bool HasImplicitNamespaceAttribute(string namespaceUri)
        {
            if (!string.IsNullOrEmpty(namespaceUri) && namespaceUri != ParentNode?.NamespaceUri)
            {
                if (string.IsNullOrEmpty(GetPrefixOfNamespace(namespaceUri)))
                {
                    bool namespaceDeclared = false;

                    if (Element.HasAttributes)
                    {
                        foreach (XAttribute attribute in Element.Attributes())
                        {
                            if (attribute.Name.LocalName == "xmlns" && string.IsNullOrEmpty(attribute.Name.NamespaceName) && attribute.Value == namespaceUri)
                            {
                                namespaceDeclared = true;
                            }
                        }
                    }

                    if (!namespaceDeclared)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override IXmlNode AppendChild(IXmlNode newChild)
        {
            IXmlNode result = base.AppendChild(newChild);
            _attributes = null;
            return result;
        }

        public override string Value
        {
            get { return Element.Value; }
            set { Element.Value = value; }
        }

        public override string LocalName
        {
            get { return Element.Name.LocalName; }
        }

        public override string NamespaceUri
        {
            get { return Element.Name.NamespaceName; }
        }

        public string GetPrefixOfNamespace(string namespaceUri)
        {
            return Element.GetPrefixOfNamespace(namespaceUri);
        }

        public bool IsEmpty
        {
            get { return Element.IsEmpty; }
        }
    }
#endif
    #endregion

    /// <summary>
    /// Converts XML to and from JSON.
    /// </summary>
    public class XmlNodeConverter : JsonConverter
    {
        internal static readonly List<IXmlNode> EmptyChildNodes = new List<IXmlNode>();

        private const string TextName = "#text";
        private const string CommentName = "#comment";
        private const string CDataName = "#cdata-section";
        private const string WhitespaceName = "#whitespace";
        private const string SignificantWhitespaceName = "#significant-whitespace";
        private const string DeclarationName = "?xml";
        private const string JsonNamespaceUri = "http://james.newtonking.com/projects/json";

        /// <summary>
        /// Gets or sets the name of the root element to insert when deserializing to XML if the JSON structure has produced multiple root elements.
        /// </summary>
        /// <value>The name of the deserialized root element.</value>
        public string DeserializeRootElementName { get; set; }

        /// <summary>
        /// Gets or sets a flag to indicate whether to write the Json.NET array attribute.
        /// This attribute helps preserve arrays when converting the written XML back to JSON.
        /// </summary>
        /// <value><c>true</c> if the array attribute is written to the XML; otherwise, <c>false</c>.</value>
        public bool WriteArrayAttribute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to write the root JSON object.
        /// </summary>
        /// <value><c>true</c> if the JSON root object is omitted; otherwise, <c>false</c>.</value>
        public bool OmitRootObject { get; set; }

        #region Writing
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="value">The value.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IXmlNode node = WrapXml(value);

            XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
            PushParentNamespaces(node, manager);

            if (!OmitRootObject)
            {
                writer.WriteStartObject();
            }

            SerializeNode(writer, node, manager, !OmitRootObject);

            if (!OmitRootObject)
            {
                writer.WriteEndObject();
            }
        }

        private IXmlNode WrapXml(object value)
        {
#if HAVE_XLINQ
            XObject xObject = value as XObject;
            if (xObject != null)
            {
                return XContainerWrapper.WrapNode(xObject);
            }
#endif
#if HAVE_XML_DOCUMENT
            XmlNode node = value as XmlNode;
            if (node != null)
            {
                return XmlNodeWrapper.WrapNode(node);
            }
#endif

            throw new ArgumentException("Value must be an XML object.", nameof(value));
        }

        private void PushParentNamespaces(IXmlNode node, XmlNamespaceManager manager)
        {
            List<IXmlNode> parentElements = null;

            IXmlNode parent = node;
            while ((parent = parent.ParentNode) != null)
            {
                if (parent.NodeType == XmlNodeType.Element)
                {
                    if (parentElements == null)
                    {
                        parentElements = new List<IXmlNode>();
                    }

                    parentElements.Add(parent);
                }
            }

            if (parentElements != null)
            {
                parentElements.Reverse();

                foreach (IXmlNode parentElement in parentElements)
                {
                    manager.PushScope();
                    foreach (IXmlNode attribute in parentElement.Attributes)
                    {
                        if (attribute.NamespaceUri == "http://www.w3.org/2000/xmlns/" && attribute.LocalName != "xmlns")
                        {
                            manager.AddNamespace(attribute.LocalName, attribute.Value);
                        }
                    }
                }
            }
        }

        private string ResolveFullName(IXmlNode node, XmlNamespaceManager manager)
        {
            string prefix = (node.NamespaceUri == null || (node.LocalName == "xmlns" && node.NamespaceUri == "http://www.w3.org/2000/xmlns/"))
                ? null
                : manager.LookupPrefix(node.NamespaceUri);

            if (!string.IsNullOrEmpty(prefix))
            {
                return prefix + ":" + XmlConvert.DecodeName(node.LocalName);
            }
            else
            {
                return XmlConvert.DecodeName(node.LocalName);
            }
        }

        private string GetPropertyName(IXmlNode node, XmlNamespaceManager manager)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Attribute:
                    if (node.NamespaceUri == JsonNamespaceUri)
                    {
                        return "$" + node.LocalName;
                    }
                    else
                    {
                        return "@" + ResolveFullName(node, manager);
                    }
                case XmlNodeType.CDATA:
                    return CDataName;
                case XmlNodeType.Comment:
                    return CommentName;
                case XmlNodeType.Element:
                    if (node.NamespaceUri == JsonNamespaceUri)
                    {
                        return "$" + node.LocalName;
                    }
                    else
                    {
                        return ResolveFullName(node, manager);
                    }
                case XmlNodeType.ProcessingInstruction:
                    return "?" + ResolveFullName(node, manager);
                case XmlNodeType.DocumentType:
                    return "!" + ResolveFullName(node, manager);
                case XmlNodeType.XmlDeclaration:
                    return DeclarationName;
                case XmlNodeType.SignificantWhitespace:
                    return SignificantWhitespaceName;
                case XmlNodeType.Text:
                    return TextName;
                case XmlNodeType.Whitespace:
                    return WhitespaceName;
                default:
                    throw new JsonSerializationException("Unexpected XmlNodeType when getting node name: " + node.NodeType);
            }
        }

        private bool IsArray(IXmlNode node)
        {
            foreach (IXmlNode attribute in node.Attributes)
            {
                if (attribute.LocalName == "Array" && attribute.NamespaceUri == JsonNamespaceUri)
                {
                    return XmlConvert.ToBoolean(attribute.Value);
                }
            }

            return false;
        }

        private void SerializeGroupedNodes(JsonWriter writer, IXmlNode node, XmlNamespaceManager manager, bool writePropertyName)
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
                    WriteGroupedNodes(writer, manager, writePropertyName, node.ChildNodes, nodeName);
                    break;
                }
                default:
                {
                    // check whether nodes have the same name
                    // if they don't then group into dictionary together by name

                    // value of dictionary will be a single IXmlNode when there is one for a name,
                    // or a List<IXmlNode> when there are multiple
                    Dictionary<string, object> nodesGroupedByName = null;

                    string nodeName = null;

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
                            object value;
                            if (!nodesGroupedByName.TryGetValue(currentNodeName, out value))
                            {
                                nodesGroupedByName.Add(currentNodeName, childNode);
                            }
                            else
                            {
                                List<IXmlNode> nodes = value as List<IXmlNode>;
                                if (nodes == null)
                                {
                                    nodes = new List<IXmlNode> {(IXmlNode)value};
                                    nodesGroupedByName[currentNodeName] = nodes;
                                }

                                nodes.Add(childNode);
                            }
                        }
                    }

                    if (nodesGroupedByName == null)
                    {
                        WriteGroupedNodes(writer, manager, writePropertyName, node.ChildNodes, nodeName);
                    }
                    else
                    {
                        // loop through grouped nodes. write single name instances as normal,
                        // write multiple names together in an array
                        foreach (KeyValuePair<string, object> nodeNameGroup in nodesGroupedByName)
                        {
                            List<IXmlNode> nodes = nodeNameGroup.Value as List<IXmlNode>;
                            if (nodes != null)
                            {
                                WriteGroupedNodes(writer, manager, writePropertyName, nodes, nodeNameGroup.Key);
                            }
                            else
                            {
                                WriteGroupedNodes(writer, manager, writePropertyName, (IXmlNode)nodeNameGroup.Value, nodeNameGroup.Key);
                            }
                        }
                    }
                    break;
                }
            }
        }

        private void WriteGroupedNodes(JsonWriter writer, XmlNamespaceManager manager, bool writePropertyName, List<IXmlNode> groupedNodes, string elementNames)
        {
            bool writeArray;

            if (groupedNodes.Count == 1)
            {
                writeArray = IsArray(groupedNodes[0]);
            }
            else
            {
                writeArray = true;
            }

            if (!writeArray)
            {
                SerializeNode(writer, groupedNodes[0], manager, writePropertyName);
            }
            else
            {
                if (writePropertyName)
                {
                    writer.WritePropertyName(elementNames);
                }

                writer.WriteStartArray();

                for (int i = 0; i < groupedNodes.Count; i++)
                {
                    SerializeNode(writer, groupedNodes[i], manager, false);
                }

                writer.WriteEndArray();
            }
        }

        private void WriteGroupedNodes(JsonWriter writer, XmlNamespaceManager manager, bool writePropertyName, IXmlNode node, string elementNames)
        {
            bool writeArray = IsArray(node);

            if (!writeArray)
            {
                SerializeNode(writer, node, manager, writePropertyName);
            }
            else
            {
                if (writePropertyName)
                {
                    writer.WritePropertyName(elementNames);
                }

                writer.WriteStartArray();

                SerializeNode(writer, node, manager, false);

                writer.WriteEndArray();
            }
        }

        private void SerializeNode(JsonWriter writer, IXmlNode node, XmlNamespaceManager manager, bool writePropertyName)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                    SerializeGroupedNodes(writer, node, manager, writePropertyName);
                    break;
                case XmlNodeType.Element:
                    if (IsArray(node) && AllSameName(node) && node.ChildNodes.Count > 0)
                    {
                        SerializeGroupedNodes(writer, node, manager, false);
                    }
                    else
                    {
                        manager.PushScope();

                        foreach (IXmlNode attribute in node.Attributes)
                        {
                            if (attribute.NamespaceUri == "http://www.w3.org/2000/xmlns/")
                            {
                                string namespacePrefix = (attribute.LocalName != "xmlns")
                                    ? XmlConvert.DecodeName(attribute.LocalName)
                                    : string.Empty;
                                string namespaceUri = attribute.Value;

                                manager.AddNamespace(namespacePrefix, namespaceUri);
                            }
                        }

                        if (writePropertyName)
                        {
                            writer.WritePropertyName(GetPropertyName(node, manager));
                        }

                        if (!ValueAttributes(node.Attributes) && node.ChildNodes.Count == 1
                            && node.ChildNodes[0].NodeType == XmlNodeType.Text)
                        {
                            // write elements with a single text child as a name value pair
                            writer.WriteValue(node.ChildNodes[0].Value);
                        }
                        else if (node.ChildNodes.Count == 0 && node.Attributes.Count == 0)
                        {
                            IXmlElement element = (IXmlElement)node;

                            // empty element
                            if (element.IsEmpty)
                            {
                                writer.WriteNull();
                            }
                            else
                            {
                                writer.WriteValue(string.Empty);
                            }
                        }
                        else
                        {
                            writer.WriteStartObject();

                            for (int i = 0; i < node.Attributes.Count; i++)
                            {
                                SerializeNode(writer, node.Attributes[i], manager, true);
                            }

                            SerializeGroupedNodes(writer, node, manager, true);

                            writer.WriteEndObject();
                        }

                        manager.PopScope();
                    }

                    break;
                case XmlNodeType.Comment:
                    if (writePropertyName)
                    {
                        writer.WriteComment(node.Value);
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
                        writer.WritePropertyName(GetPropertyName(node, manager));
                    }
                    writer.WriteValue(node.Value);
                    break;
                case XmlNodeType.XmlDeclaration:
                    IXmlDeclaration declaration = (IXmlDeclaration)node;
                    writer.WritePropertyName(GetPropertyName(node, manager));
                    writer.WriteStartObject();

                    if (!string.IsNullOrEmpty(declaration.Version))
                    {
                        writer.WritePropertyName("@version");
                        writer.WriteValue(declaration.Version);
                    }
                    if (!string.IsNullOrEmpty(declaration.Encoding))
                    {
                        writer.WritePropertyName("@encoding");
                        writer.WriteValue(declaration.Encoding);
                    }
                    if (!string.IsNullOrEmpty(declaration.Standalone))
                    {
                        writer.WritePropertyName("@standalone");
                        writer.WriteValue(declaration.Standalone);
                    }

                    writer.WriteEndObject();
                    break;
                case XmlNodeType.DocumentType:
                    IXmlDocumentType documentType = (IXmlDocumentType)node;
                    writer.WritePropertyName(GetPropertyName(node, manager));
                    writer.WriteStartObject();

                    if (!string.IsNullOrEmpty(documentType.Name))
                    {
                        writer.WritePropertyName("@name");
                        writer.WriteValue(documentType.Name);
                    }
                    if (!string.IsNullOrEmpty(documentType.Public))
                    {
                        writer.WritePropertyName("@public");
                        writer.WriteValue(documentType.Public);
                    }
                    if (!string.IsNullOrEmpty(documentType.System))
                    {
                        writer.WritePropertyName("@system");
                        writer.WriteValue(documentType.System);
                    }
                    if (!string.IsNullOrEmpty(documentType.InternalSubset))
                    {
                        writer.WritePropertyName("@internalSubset");
                        writer.WriteValue(documentType.InternalSubset);
                    }

                    writer.WriteEndObject();
                    break;
                default:
                    throw new JsonSerializationException("Unexpected XmlNodeType when serializing nodes: " + node.NodeType);
            }
        }

        private static bool AllSameName(IXmlNode node)
        {
            foreach (IXmlNode childNode in node.ChildNodes)
            {
                if (childNode.LocalName != node.LocalName)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Reading
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.StartObject:
                    break;
                default:
                    throw new JsonSerializationException("XmlNodeConverter can only convert JSON that begins with an object.");
            }

            XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
            IXmlDocument document = null;
            IXmlNode rootNode = null;

#if HAVE_XLINQ
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
#if HAVE_XML_DOCUMENT
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

            if (!string.IsNullOrEmpty(DeserializeRootElementName))
            {
                ReadElement(reader, document, rootNode, DeserializeRootElementName, manager);
            }
            else
            {
                reader.Read();
                DeserializeNode(reader, document, manager, rootNode);
            }

#if HAVE_XLINQ
            if (objectType == typeof(XElement))
            {
                XElement element = (XElement)document.DocumentElement.WrappedNode;
                element.Remove();

                return element;
            }
#endif

            return document.WrappedNode;
        }

        private void DeserializeValue(JsonReader reader, IXmlDocument document, XmlNamespaceManager manager, string propertyName, IXmlNode currentNode)
        {
            switch (propertyName)
            {
                case TextName:
                    currentNode.AppendChild(document.CreateTextNode(ConvertTokenToXmlValue(reader)));
                    break;
                case CDataName:
                    currentNode.AppendChild(document.CreateCDataSection(ConvertTokenToXmlValue(reader)));
                    break;
                case WhitespaceName:
                    currentNode.AppendChild(document.CreateWhitespace(ConvertTokenToXmlValue(reader)));
                    break;
                case SignificantWhitespaceName:
                    currentNode.AppendChild(document.CreateSignificantWhitespace(ConvertTokenToXmlValue(reader)));
                    break;
                default:
                    // processing instructions and the xml declaration start with ?
                    if (!string.IsNullOrEmpty(propertyName) && propertyName[0] == '?')
                    {
                        CreateInstruction(reader, document, currentNode, propertyName);
                    }
                    else if (string.Equals(propertyName, "!DOCTYPE", StringComparison.OrdinalIgnoreCase))
                    {
                        CreateDocumentType(reader, document, currentNode);
                    }
                    else
                    {
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            // handle nested arrays
                            ReadArrayElements(reader, document, propertyName, currentNode, manager);
                            return;
                        }

                        // have to wait until attributes have been parsed before creating element
                        // attributes may contain namespace info used by the element
                        ReadElement(reader, document, currentNode, propertyName, manager);
                    }
                    break;
            }
        }

        private void ReadElement(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName, XmlNamespaceManager manager)
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
                        CreateElement(reader, document, currentNode, propertyName, manager, elementPrefix, attributeNameValues);
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

            CreateElement(reader, document, currentNode, propertyName, manager, elementPrefix, attributeNameValues);
        }

        private void CreateElement(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string elementName, XmlNamespaceManager manager, string elementPrefix, Dictionary<string, string> attributeNameValues)
        {
            IXmlElement element = CreateElement(elementName, document, elementPrefix, manager);

            currentNode.AppendChild(element);

            if (attributeNameValues != null)
            {
                // add attributes to newly created element
                foreach (KeyValuePair<string, string> nameValue in attributeNameValues)
                {
                    string encodedName = XmlConvert.EncodeName(nameValue.Key);
                    string attributePrefix = MiscellaneousUtils.GetPrefix(nameValue.Key);

                    IXmlNode attribute = (!string.IsNullOrEmpty(attributePrefix)) ? document.CreateAttribute(encodedName, manager.LookupNamespace(attributePrefix) ?? string.Empty, nameValue.Value) : document.CreateAttribute(encodedName, nameValue.Value);

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
                    string text = ConvertTokenToXmlValue(reader);
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
                    DeserializeNode(reader, document, manager, element);
                    manager.PopScope();
                    manager.RemoveNamespace(string.Empty, manager.DefaultNamespace);
                    break;
            }
        }

        private static void AddAttribute(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName, string attributeName, XmlNamespaceManager manager, string attributePrefix)
        {
            if (currentNode.NodeType == XmlNodeType.Document)
            {
                throw JsonSerializationException.Create(reader, "JSON root object has property '{0}' that will be converted to an attribute. A root object cannot have any attribute properties. Consider specifying a DeserializeRootElementName.".FormatWith(CultureInfo.InvariantCulture, propertyName));
            }

            string encodedName = XmlConvert.EncodeName(attributeName);
            string attributeValue = reader.Value.ToString();

            IXmlNode attribute = (!string.IsNullOrEmpty(attributePrefix))
                ? document.CreateAttribute(encodedName, manager.LookupNamespace(attributePrefix), attributeValue)
                : document.CreateAttribute(encodedName, attributeValue);

            ((IXmlElement)currentNode).SetAttributeNode(attribute);
        }

        private string ConvertTokenToXmlValue(JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    return reader.Value?.ToString();
                case JsonToken.Integer:
#if HAVE_BIG_INTEGER
                    if (reader.Value is BigInteger)
                    {
                        return ((BigInteger)reader.Value).ToString(CultureInfo.InvariantCulture);
                    }
#endif
                    return XmlConvert.ToString(Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture));
                case JsonToken.Float:
                    if (reader.Value is decimal)
                    {
                        return XmlConvert.ToString((decimal)reader.Value);
                    }

                    if (reader.Value is float)
                    {
                        return XmlConvert.ToString((float)reader.Value);
                    }

                    return XmlConvert.ToString(Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture));
                case JsonToken.Boolean:
                    return XmlConvert.ToString(Convert.ToBoolean(reader.Value, CultureInfo.InvariantCulture));
                case JsonToken.Date:
#if HAVE_DATE_TIME_OFFSET
                    if (reader.Value is DateTimeOffset)
                    {
                        return XmlConvert.ToString((DateTimeOffset)reader.Value);
                    }

#endif
                    DateTime d = Convert.ToDateTime(reader.Value, CultureInfo.InvariantCulture);
#if !PORTABLE
                    return XmlConvert.ToString(d, DateTimeUtils.ToSerializationMode(d.Kind));
#else
                    return XmlConvert.ToString(d, DateTimeUtils.ToDateTimeFormat(d.Kind));
#endif
                case JsonToken.Null:
                    return null;
                default:
                    throw JsonSerializationException.Create(reader, "Cannot get an XML string value from token type '{0}'.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }
        }

        private void ReadArrayElements(JsonReader reader, IXmlDocument document, string propertyName, IXmlNode currentNode, XmlNamespaceManager manager)
        {
            string elementPrefix = MiscellaneousUtils.GetPrefix(propertyName);

            IXmlElement nestedArrayElement = CreateElement(propertyName, document, elementPrefix, manager);

            currentNode.AppendChild(nestedArrayElement);

            int count = 0;
            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
            {
                DeserializeValue(reader, document, manager, propertyName, nestedArrayElement);
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

        private void AddJsonArrayAttribute(IXmlElement element, IXmlDocument document)
        {
            element.SetAttributeNode(document.CreateAttribute("json:Array", JsonNamespaceUri, "true"));

#if HAVE_XLINQ
            // linq to xml doesn't automatically include prefixes via the namespace manager
            if (element is XElementWrapper)
            {
                if (element.GetPrefixOfNamespace(JsonNamespaceUri) == null)
                {
                    element.SetAttributeNode(document.CreateAttribute("xmlns:json", "http://www.w3.org/2000/xmlns/", JsonNamespaceUri));
                }
            }
#endif
        }

        private Dictionary<string, string> ReadAttributeElements(JsonReader reader, XmlNamespaceManager manager)
        {
            // a string token means the element only has a single text child
            switch (reader.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Null:
                case JsonToken.Boolean:
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Date:
                case JsonToken.StartConstructor:
                    return null;
            }

            Dictionary<string, string> attributeNameValues = null;
            bool finished = false;

            // read properties until first non-attribute is encountered
            while (!finished && reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string attributeName = reader.Value.ToString();

                        if (!string.IsNullOrEmpty(attributeName))
                        {
                            char firstChar = attributeName[0];
                            string attributeValue;

                            switch (firstChar)
                            {
                                case '@':
                                    if (attributeNameValues == null)
                                    {
                                        attributeNameValues = new Dictionary<string, string>();
                                    }

                                    attributeName = attributeName.Substring(1);
                                    reader.Read();
                                    attributeValue = ConvertTokenToXmlValue(reader);
                                    attributeNameValues.Add(attributeName, attributeValue);

                                    string namespacePrefix;
                                    if (IsNamespaceAttribute(attributeName, out namespacePrefix))
                                    {
                                        manager.AddNamespace(namespacePrefix, attributeValue);
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
                                            string jsonPrefix = manager.LookupPrefix(JsonNamespaceUri);
                                            if (jsonPrefix == null)
                                            {
                                                if (attributeNameValues == null)
                                                {
                                                    attributeNameValues = new Dictionary<string, string>();
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
                                            reader.Read();

                                            if (!JsonTokenUtils.IsPrimitiveToken(reader.TokenType))
                                            {
                                                throw JsonSerializationException.Create(reader, "Unexpected JsonToken: " + reader.TokenType);
                                            }

                                            if (attributeNameValues == null)
                                            {
                                                attributeNameValues = new Dictionary<string, string>();
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

        private void CreateInstruction(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName)
        {
            if (propertyName == DeclarationName)
            {
                string version = null;
                string encoding = null;
                string standalone = null;
                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                {
                    switch (reader.Value.ToString())
                    {
                        case "@version":
                            reader.Read();
                            version = ConvertTokenToXmlValue(reader);
                            break;
                        case "@encoding":
                            reader.Read();
                            encoding = ConvertTokenToXmlValue(reader);
                            break;
                        case "@standalone":
                            reader.Read();
                            standalone = ConvertTokenToXmlValue(reader);
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
                IXmlNode instruction = document.CreateProcessingInstruction(propertyName.Substring(1), ConvertTokenToXmlValue(reader));
                currentNode.AppendChild(instruction);
            }
        }

        private void CreateDocumentType(JsonReader reader, IXmlDocument document, IXmlNode currentNode)
        {
            string name = null;
            string publicId = null;
            string systemId = null;
            string internalSubset = null;
            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.Value.ToString())
                {
                    case "@name":
                        reader.Read();
                        name = ConvertTokenToXmlValue(reader);
                        break;
                    case "@public":
                        reader.Read();
                        publicId = ConvertTokenToXmlValue(reader);
                        break;
                    case "@system":
                        reader.Read();
                        systemId = ConvertTokenToXmlValue(reader);
                        break;
                    case "@internalSubset":
                        reader.Read();
                        internalSubset = ConvertTokenToXmlValue(reader);
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected property name encountered while deserializing XmlDeclaration: " + reader.Value);
                }
            }

            IXmlNode documentType = document.CreateXmlDocumentType(name, publicId, systemId, internalSubset);
            currentNode.AppendChild(documentType);
        }

        private IXmlElement CreateElement(string elementName, IXmlDocument document, string elementPrefix, XmlNamespaceManager manager)
        {
            string encodeName = XmlConvert.EncodeName(elementName);
            string ns = string.IsNullOrEmpty(elementPrefix) ? manager.DefaultNamespace : manager.LookupNamespace(elementPrefix);

            IXmlElement element = (!string.IsNullOrEmpty(ns)) ? document.CreateElement(encodeName, ns) : document.CreateElement(encodeName);

            return element;
        }

        private void DeserializeNode(JsonReader reader, IXmlDocument document, XmlNamespaceManager manager, IXmlNode currentNode)
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

                        string propertyName = reader.Value.ToString();
                        reader.Read();

                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            int count = 0;
                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                DeserializeValue(reader, document, manager, propertyName, currentNode);
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
                            DeserializeValue(reader, document, manager, propertyName, currentNode);
                        }
                        continue;
                    case JsonToken.StartConstructor:
                        string constructorName = reader.Value.ToString();

                        while (reader.Read() && reader.TokenType != JsonToken.EndConstructor)
                        {
                            DeserializeValue(reader, document, manager, constructorName, currentNode);
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
            } while (reader.Read());
            // don't read if current token is a property. token was already read when parsing element attributes
        }

        /// <summary>
        /// Checks if the <paramref name="attributeName"/> is a namespace attribute.
        /// </summary>
        /// <param name="attributeName">Attribute name to test.</param>
        /// <param name="prefix">The attribute name prefix if it has one, otherwise an empty string.</param>
        /// <returns><c>true</c> if attribute name is for a namespace attribute, otherwise <c>false</c>.</returns>
        private bool IsNamespaceAttribute(string attributeName, out string prefix)
        {
            if (attributeName.StartsWith("xmlns", StringComparison.Ordinal))
            {
                if (attributeName.Length == 5)
                {
                    prefix = string.Empty;
                    return true;
                }
                else if (attributeName[5] == ':')
                {
                    prefix = attributeName.Substring(6, attributeName.Length - 6);
                    return true;
                }
            }
            prefix = null;
            return false;
        }

        private bool ValueAttributes(List<IXmlNode> c)
        {
            foreach (IXmlNode xmlNode in c)
            {
                if (xmlNode.NamespaceUri != JsonNamespaceUri)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        /// <summary>
        /// Determines whether this instance can convert the specified value type.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type valueType)
        {
#if HAVE_XLINQ
            if (typeof(XObject).IsAssignableFrom(valueType))
            {
                return true;
            }
#endif
#if HAVE_XML_DOCUMENT
            if (typeof(XmlNode).IsAssignableFrom(valueType))
            {
                return true;
            }
#endif

            return false;
        }
    }
}

#endif