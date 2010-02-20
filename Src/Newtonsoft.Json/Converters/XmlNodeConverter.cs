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

#if !SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Xml;
#if !NET20
using System.Xml.Linq;
#endif
using Newtonsoft.Json.Utilities;
using System.Linq;

namespace Newtonsoft.Json.Converters
{
  #region Wrappers
  internal class XmlDocumentWrapper : XmlNodeWrapper, IXmlDocument
  {
    private XmlDocument _document;

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
      return new XmlNodeWrapper(_document.CreateXmlDeclaration(version, encoding, standalone));
    }

    public IXmlNode CreateProcessingInstruction(string target, string data)
    {
      return new XmlNodeWrapper(_document.CreateProcessingInstruction(target, data));
    }

    public IXmlElement CreateElement(string elementName)
    {
      return new XmlElementWrapper(_document.CreateElement(elementName));
    }

    public IXmlElement CreateElement(string qualifiedName, string namespaceURI)
    {
      return new XmlElementWrapper(_document.CreateElement(qualifiedName, namespaceURI));
    }

    public IXmlNode CreateAttribute(string name, string value)
    {
      XmlNodeWrapper attribute = new XmlNodeWrapper(_document.CreateAttribute(name));
      attribute.Value = value;

      return attribute;
    }

    public IXmlNode CreateAttribute(string qualifiedName, string namespaceURI, string value)
    {
      XmlNodeWrapper attribute = new XmlNodeWrapper(_document.CreateAttribute(qualifiedName, namespaceURI));
      attribute.Value = value;

      return attribute;
    }

    public IXmlElement DocumentElement
    {
      get
      {
        if (_document.DocumentElement == null)
          return null;

        return new XmlElementWrapper(_document.DocumentElement);
      }
    }
  }

  internal class XmlElementWrapper : XmlNodeWrapper, IXmlElement
  {
    private XmlElement _element;

    public XmlElementWrapper(XmlElement element)
      : base(element)
    {
      _element = element;
    }

    public void SetAttributeNode(IXmlNode attribute)
    {
      XmlNodeWrapper xmlAttributeWrapper = (XmlNodeWrapper)attribute;

      _element.SetAttributeNode((XmlAttribute) xmlAttributeWrapper.WrappedNode);
    }
  }

  internal class XmlDeclarationWrapper : XmlNodeWrapper, IXmlDeclaration
  {
    private XmlDeclaration _declaration;

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

  internal class XmlNodeWrapper : IXmlNode
  {
    private readonly XmlNode _node;

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

    public string Name
    {
      get { return _node.Name; }
    }

    public string LocalName
    {
      get { return _node.LocalName; }
    }

    public IList<IXmlNode> ChildNodes
    {
      get { return _node.ChildNodes.Cast<XmlNode>().Select(n => WrapNode(n)).ToList(); }
    }

    private IXmlNode WrapNode(XmlNode node)
    {
      switch (node.NodeType)
      {
        case XmlNodeType.Element:
          return new XmlElementWrapper((XmlElement) node);
        case XmlNodeType.XmlDeclaration:
          return new XmlDeclarationWrapper((XmlDeclaration) node);
        default:
          return new XmlNodeWrapper(node);
      }
    }

    public IList<IXmlNode> Attributes
    {
      get
      {
        if (_node.Attributes == null)
          return null;

        return _node.Attributes.Cast<XmlAttribute>().Select(a => WrapNode(a)).ToList();
      }
    }

    public IXmlNode ParentNode
    {
      get
      {
        XmlNode node = (_node is XmlAttribute)
                         ? ((XmlAttribute) _node).OwnerElement
                         : _node.ParentNode;
        
        if (node == null)
          return null;

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
      XmlNodeWrapper xmlNodeWrapper = (XmlNodeWrapper) newChild;
      _node.AppendChild(xmlNodeWrapper._node);

      return newChild;
    }

    public string Prefix
    {
      get { return _node.Prefix; }
    }

    public string NamespaceURI
    {
      get { return _node.NamespaceURI; }
    }
  }

  internal interface IXmlDocument : IXmlNode
  {
    IXmlNode CreateComment(string text);
    IXmlNode CreateTextNode(string text);
    IXmlNode CreateCDataSection(string data);
    IXmlNode CreateWhitespace(string text);
    IXmlNode CreateSignificantWhitespace(string text);
    IXmlNode CreateXmlDeclaration(string version, string encoding, string standalone);
    IXmlNode CreateProcessingInstruction(string target, string data);
    IXmlElement CreateElement(string elementName);
    IXmlElement CreateElement(string qualifiedName, string namespaceURI);
    IXmlNode CreateAttribute(string name, string value);
    IXmlNode CreateAttribute(string qualifiedName, string namespaceURI, string value);

    IXmlElement DocumentElement { get; }
  }

  internal interface IXmlDeclaration : IXmlNode
  {
    string Version { get; }
    string Encoding { get; set; }
    string Standalone { get; set; }
  }

  internal interface IXmlElement : IXmlNode
  {
    void SetAttributeNode(IXmlNode attribute);
  }

  internal interface IXmlNode
  {
    XmlNodeType NodeType { get; }
    string LocalName { get; }
    IList<IXmlNode> ChildNodes { get; }
    IList<IXmlNode> Attributes { get; }
    IXmlNode ParentNode { get; }
    string Value { get; set; }
    IXmlNode AppendChild(IXmlNode newChild);
    string NamespaceURI { get; }
    object WrappedNode { get; }
  }

  #if !NET20
  internal class XDeclarationWrapper : XObjectWrapper, IXmlDeclaration
  {
    internal readonly XDeclaration _declaration;

    public XDeclarationWrapper(XDeclaration declaration)
      : base(null)
    {
      _declaration = declaration;
    }

    public override XmlNodeType NodeType
    {
      get { return XmlNodeType.XmlDeclaration; }
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

    public override IList<IXmlNode> ChildNodes
    {
      get
      {
        IList<IXmlNode> childNodes = base.ChildNodes;

        if (Document.Declaration != null)
          childNodes.Insert(0, new XDeclarationWrapper(Document.Declaration));

        return childNodes;
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

    public IXmlNode CreateProcessingInstruction(string target, string data)
    {
      return new XProcessingInstructionWrapper(new XProcessingInstruction(target, data));
    }

    public IXmlElement CreateElement(string elementName)
    {
      return new XElementWrapper(new XElement(elementName));
    }

    public IXmlElement CreateElement(string qualifiedName, string namespaceURI)
    {
      string localName = MiscellaneousUtils.GetLocalName(qualifiedName);
      return new XElementWrapper(new XElement(XName.Get(localName, namespaceURI)));
    }

    public IXmlNode CreateAttribute(string name, string value)
    {
      return new XAttributeWrapper(new XAttribute(name, value));
    }

    public IXmlNode CreateAttribute(string qualifiedName, string namespaceURI, string value)
    {
      string localName = MiscellaneousUtils.GetLocalName(qualifiedName);
      return new XAttributeWrapper(new XAttribute(XName.Get(localName, namespaceURI), value));
    }

    public IXmlElement DocumentElement
    {
      get
      {
        if (Document.Root == null)
          return null;

        return new XElementWrapper(Document.Root);
      }
    }

    public override IXmlNode AppendChild(IXmlNode newChild)
    {
      XDeclarationWrapper declarationWrapper = newChild as XDeclarationWrapper;
      if (declarationWrapper != null)
      {
        Document.Declaration = declarationWrapper._declaration;
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
          return null;
        
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
          return null;

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
    private XContainer Container
    {
      get { return (XContainer)WrappedNode; }
    }

    public XContainerWrapper(XContainer container)
      : base(container)
    {
    }

    public override IList<IXmlNode> ChildNodes
    {
      get { return Container.Nodes().Select(n => WrapNode(n)).ToList(); }
    }

    public override IXmlNode ParentNode
    {
      get
      {
        if (Container.Parent == null)
          return null;
        
        return WrapNode(Container.Parent);
      }
    }

    internal static IXmlNode WrapNode(XObject node)
    {
      if (node is XDocument)
        return new XDocumentWrapper((XDocument)node);
      else if (node is XElement)
        return new XElementWrapper((XElement)node);
      else if (node is XContainer)
        return new XContainerWrapper((XContainer)node);
      else if (node is XProcessingInstruction)
        return new XProcessingInstructionWrapper((XProcessingInstruction)node);
      else if (node is XText)
        return new XTextWrapper((XText)node);
      else if (node is XComment)
        return new XCommentWrapper((XComment)node);
      else if (node is XAttribute)
        return new XAttributeWrapper((XAttribute) node);
      else
        return new XObjectWrapper(node);
    }

    public override IXmlNode AppendChild(IXmlNode newChild)
    {
      Container.Add(newChild.WrappedNode);
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

    public virtual IList<IXmlNode> ChildNodes
    {
      get { return new List<IXmlNode>(); }
    }

    public virtual IList<IXmlNode> Attributes
    {
      get { return null; }
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

    public virtual string NamespaceURI
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

    public override string NamespaceURI
    {
      get { return Attribute.Name.NamespaceName; }
    }

    public override IXmlNode ParentNode
    {
      get
      {
        if (Attribute.Parent == null)
          return null;

        return XContainerWrapper.WrapNode(Attribute.Parent);
      }
    }
  }

  internal class XElementWrapper : XContainerWrapper, IXmlElement
  {
    private XElement Element
    {
      get { return (XElement) WrappedNode; }
    }

    public XElementWrapper(XElement element)
      : base(element)
    {
    }

    public void SetAttributeNode(IXmlNode attribute)
    {
      XObjectWrapper wrapper = (XObjectWrapper)attribute;
      Element.Add(wrapper.WrappedNode);
    }

    public override IList<IXmlNode> Attributes
    {
      get { return Element.Attributes().Select(a => new XAttributeWrapper(a)).Cast<IXmlNode>().ToList(); }
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

    public override string NamespaceURI
    {
      get { return Element.Name.NamespaceName; }
    }
  }
#endif
  #endregion

  /// <summary>
  /// Converts an <see cref="XmlNode"/> to and from JSON.
  /// </summary>
  public class XmlNodeConverter : JsonConverter
  {
    private const string TextName = "#text";
    private const string CommentName = "#comment";
    private const string CDataName = "#cdata-section";
    private const string WhitespaceName = "#whitespace";
    private const string SignificantWhitespaceName = "#significant-whitespace";
    private const string DeclarationName = "?xml";
    private const string JsonNamespaceUri = "http://james.newtonking.com/projects/json";

    /// <summary>
    /// Gets or sets the name of the root element to insert when deserializing to XML if the JSON structure has produces multiple root elements.
    /// </summary>
    /// <value>The name of the deserialize root element.</value>
    public string DeserializeRootElementName { get; set; }

    #region Writing
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <param name="value">The value.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      IXmlNode node;

      if (value is XmlNode)
        node = new XmlNodeWrapper((XmlNode)value);
#if !NET20
      else if (value is XObject)
        node = XContainerWrapper.WrapNode((XObject)value);
#endif
      else
        throw new ArgumentException("Value must be an XmlNode", "value");

      XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
      PushParentNamespaces(node, manager);

      writer.WriteStartObject();
      SerializeNode(writer, node, manager, true);
      writer.WriteEndObject();
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
            parentElements = new List<IXmlNode>();

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
            if (attribute.NamespaceURI == "http://www.w3.org/2000/xmlns/" && attribute.LocalName != "xmlns")
              manager.AddNamespace(attribute.LocalName, attribute.Value);
          }
        }
      }
    }

    private string ResolveFullName(IXmlNode node, XmlNamespaceManager manager)
    {
      string prefix = (node.NamespaceURI == null || (node.LocalName == "xmlns" && node.NamespaceURI == "http://www.w3.org/2000/xmlns/"))
                        ? null
                        : manager.LookupPrefix(node.NamespaceURI);

      if (!string.IsNullOrEmpty(prefix))
        return prefix + ":" + node.LocalName;
      else
        return node.LocalName;
    }

    private string GetPropertyName(IXmlNode node, XmlNamespaceManager manager)
    {
      switch (node.NodeType)
      {
        case XmlNodeType.Attribute:
          return "@" + ResolveFullName(node, manager);
        case XmlNodeType.CDATA:
          return CDataName;
        case XmlNodeType.Comment:
          return CommentName;
        case XmlNodeType.Element:
          return ResolveFullName(node, manager);
        case XmlNodeType.ProcessingInstruction:
          return "?" + ResolveFullName(node, manager);
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

    private void SerializeGroupedNodes(JsonWriter writer, IXmlNode node, XmlNamespaceManager manager)
    {
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
        bool writeArray;

        if (groupedNodes.Count == 1)
        {
          IXmlNode singleNode = groupedNodes[0];
          IXmlNode jsonArrayAttribute = (singleNode.Attributes != null)
            ? singleNode.Attributes.SingleOrDefault(a => a.LocalName == "Array" && a.NamespaceURI == JsonNamespaceUri)
            : null;
          if (jsonArrayAttribute != null)
            writeArray = XmlConvert.ToBoolean(jsonArrayAttribute.Value);
          else
            writeArray = false;
        }
        else
        {
          writeArray = true;
        }

        if (!writeArray)
        {
          SerializeNode(writer, groupedNodes[0], manager, true);
        }
        else
        {
          string elementNames = nodeNameGroup.Key;
          writer.WritePropertyName(elementNames);
          writer.WriteStartArray();

          for (int i = 0; i < groupedNodes.Count; i++)
          {
            SerializeNode(writer, groupedNodes[i], manager, false);
          }

          writer.WriteEndArray();
        }
      }
    }

    private void SerializeNode(JsonWriter writer, IXmlNode node, XmlNamespaceManager manager, bool writePropertyName)
    {
      switch (node.NodeType)
      {
        case XmlNodeType.Document:
        case XmlNodeType.DocumentFragment:
          SerializeGroupedNodes(writer, node, manager);
          break;
        case XmlNodeType.Element:
          foreach (IXmlNode attribute in node.Attributes)
          {
            if (attribute.NamespaceURI == "http://www.w3.org/2000/xmlns/")
            {
              string prefix = (attribute.LocalName != "xmlns")
                                ? attribute.LocalName
                                : string.Empty;

              manager.AddNamespace(prefix, attribute.Value);
            }
          }

          if (writePropertyName)
            writer.WritePropertyName(GetPropertyName(node, manager));

          if (ValueAttributes(node.Attributes).Count() == 0 && node.ChildNodes.Count == 1
                  && node.ChildNodes[0].NodeType == XmlNodeType.Text)
          {
            // write elements with a single text child as a name value pair
            writer.WriteValue(node.ChildNodes[0].Value);
          }
          else if (node.ChildNodes.Count == 0 && CollectionUtils.IsNullOrEmpty(node.Attributes))
          {
            // empty element
            writer.WriteNull();
          }
          else
          {
            writer.WriteStartObject();

            for (int i = 0; i < node.Attributes.Count; i++)
            {
              SerializeNode(writer, node.Attributes[i], manager, true);
            }

            SerializeGroupedNodes(writer, node, manager);

            writer.WriteEndObject();
          }

          break;
        case XmlNodeType.Comment:
          if (writePropertyName)
            writer.WriteComment(node.Value);
          break;
        case XmlNodeType.Attribute:
        case XmlNodeType.Text:
        case XmlNodeType.CDATA:
        case XmlNodeType.ProcessingInstruction:
        case XmlNodeType.Whitespace:
        case XmlNodeType.SignificantWhitespace:
          if (node.NamespaceURI == "http://www.w3.org/2000/xmlns/" && node.Value == JsonNamespaceUri)
            break;
          else if (node.NamespaceURI == JsonNamespaceUri)
            break;

          if (writePropertyName)
            writer.WritePropertyName(GetPropertyName(node, manager));
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
        default:
          throw new JsonSerializationException("Unexpected XmlNodeType when serializing nodes: " + node.NodeType);
      }
    }
    #endregion

    #region Reading
    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, JsonSerializer serializer)
    {
      XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
      IXmlDocument document;
      IXmlNode rootNode;

      if (typeof (XmlNode).IsAssignableFrom(objectType))
      {
        if (objectType != typeof (XmlDocument))
          throw new JsonSerializationException("XmlNodeConverter only supports deserializing XmlDocuments");

        XmlDocument d = new XmlDocument();
        document = new XmlDocumentWrapper(d);
        rootNode = document;
      }
#if !NET20
      else if (typeof(XObject).IsAssignableFrom(objectType))
      {
        if (objectType != typeof (XDocument) && objectType != typeof (XElement))
          throw new JsonSerializationException("XmlNodeConverter only supports deserializing XDocument or XElement.");

        XDocument d = new XDocument();
        document = new XDocumentWrapper(d);
        rootNode = document;
      }
#endif
      else
      {
        throw new JsonSerializationException("Unexpected type when converting XML: " + objectType);
      }

      if (!string.IsNullOrEmpty(DeserializeRootElementName))
      {
        rootNode = document.CreateElement(DeserializeRootElementName);
        document.AppendChild(rootNode);
      }

      if (reader.TokenType != JsonToken.StartObject)
        throw new JsonSerializationException("XmlNodeConverter can only convert JSON that begins with an object.");

      reader.Read();

      DeserializeNode(reader, document, manager, rootNode);

#if !NET20
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
                    version = reader.Value.ToString();
                    break;
                  case "@encoding":
                    reader.Read();
                    encoding = reader.Value.ToString();
                    break;
                  case "@standalone":
                    reader.Read();
                    standalone = reader.Value.ToString();
                    break;
                  default:
                    throw new JsonSerializationException("Unexpected property name encountered while deserializing XmlDeclaration: " + reader.Value);
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
          else
          {
            // deserialize xml element
            bool finishedAttributes = false;
            bool finishedElement = false;
            string elementPrefix = MiscellaneousUtils.GetPrefix(propertyName);

            Dictionary<string, string> attributeNameValues = new Dictionary<string, string>();

            if (reader.TokenType == JsonToken.StartArray)
            {
              IXmlElement nestedArrayElement = CreateElement(propertyName, document, elementPrefix, manager);

              currentNode.AppendChild(nestedArrayElement);

              while (reader.Read() && reader.TokenType != JsonToken.EndArray)
              {
                DeserializeValue(reader, document, manager, propertyName, nestedArrayElement);
              }

              return;
            }

            // a string token means the element only has a single text child
            if (reader.TokenType != JsonToken.String
              && reader.TokenType != JsonToken.Null
              && reader.TokenType != JsonToken.Boolean
              && reader.TokenType != JsonToken.Integer
              && reader.TokenType != JsonToken.Float
              && reader.TokenType != JsonToken.Date
              && reader.TokenType != JsonToken.StartConstructor)
            {
              // read properties until first non-attribute is encountered
              while (!finishedAttributes && !finishedElement && reader.Read())
              {
                switch (reader.TokenType)
                {
                  case JsonToken.PropertyName:
                    string attributeName = reader.Value.ToString();

                    if (attributeName[0] == '@')
                    {
                      attributeName = attributeName.Substring(1);
                      reader.Read();
                      string attributeValue = reader.Value.ToString();
                      attributeNameValues.Add(attributeName, attributeValue);

                      string namespacePrefix;

                      if (IsNamespaceAttribute(attributeName, out namespacePrefix))
                      {
                        manager.AddNamespace(namespacePrefix, attributeValue);
                      }
                    }
                    else
                    {
                      finishedAttributes = true;
                    }
                    break;
                  case JsonToken.EndObject:
                    finishedElement = true;
                    break;
                  default:
                    throw new JsonSerializationException("Unexpected JsonToken: " + reader.TokenType);
                }
              }
            }

            // have to wait until attributes have been parsed before creating element
            // attributes may contain namespace info used by the element
            IXmlElement element = CreateElement(propertyName, document, elementPrefix, manager);

            currentNode.AppendChild(element);

            // add attributes to newly created element
            foreach (KeyValuePair<string, string> nameValue in attributeNameValues)
            {
              string attributePrefix = MiscellaneousUtils.GetPrefix(nameValue.Key);

              IXmlNode attribute = (!string.IsNullOrEmpty(attributePrefix))
                      ? document.CreateAttribute(nameValue.Key, manager.LookupNamespace(attributePrefix), nameValue.Value)
                      : document.CreateAttribute(nameValue.Key, nameValue.Value);

              element.SetAttributeNode(attribute);
            }

            if (reader.TokenType == JsonToken.String)
            {
              element.AppendChild(document.CreateTextNode(reader.Value.ToString()));
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
              element.AppendChild(document.CreateTextNode(XmlConvert.ToString((long)reader.Value)));
            }
            else if (reader.TokenType == JsonToken.Float)
            {
              element.AppendChild(document.CreateTextNode(XmlConvert.ToString((double)reader.Value)));
            }
            else if (reader.TokenType == JsonToken.Boolean)
            {
              element.AppendChild(document.CreateTextNode(XmlConvert.ToString((bool)reader.Value)));
            }
            else if (reader.TokenType == JsonToken.Date)
            {
              DateTime d = (DateTime)reader.Value;
              element.AppendChild(document.CreateTextNode(XmlConvert.ToString(d, DateTimeUtils.ToSerializationMode(d.Kind))));
            }
            else if (reader.TokenType == JsonToken.Null)
            {
              // empty element. do nothing
            }
            else
            {
              // finished element will have no children to deserialize
              if (!finishedElement)
              {
                manager.PushScope();

                DeserializeNode(reader, document, manager, element);

                manager.PopScope();
              }
            }
          }
          break;
      }
    }

    private IXmlElement CreateElement(string elementName, IXmlDocument document, string elementPrefix, XmlNamespaceManager manager)
    {
      return (!string.IsNullOrEmpty(elementPrefix))
               ? document.CreateElement(elementName, manager.LookupNamespace(elementPrefix))
               : document.CreateElement(elementName);
    }

    private void DeserializeNode(JsonReader reader, IXmlDocument document, XmlNamespaceManager manager, IXmlNode currentNode)
    {
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            if (currentNode.NodeType == XmlNodeType.Document && document.DocumentElement != null)
              throw new JsonSerializationException("JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifing a DeserializeRootElementName.");

            string propertyName = reader.Value.ToString();
            reader.Read();

            if (reader.TokenType == JsonToken.StartArray)
            {
              while (reader.Read() && reader.TokenType != JsonToken.EndArray)
              {
                DeserializeValue(reader, document, manager, propertyName, currentNode);
              }
            }
            else
            {
              DeserializeValue(reader, document, manager, propertyName, currentNode);
            }
            break;
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
            throw new JsonSerializationException("Unexpected JsonToken when deserializing node: " + reader.TokenType);
        }
      } while (reader.TokenType == JsonToken.PropertyName || reader.Read());
      // don't read if current token is a property. token was already read when parsing element attributes
    }

    /// <summary>
    /// Checks if the attributeName is a namespace attribute.
    /// </summary>
    /// <param name="attributeName">Attribute name to test.</param>
    /// <param name="prefix">The attribute name prefix if it has one, otherwise an empty string.</param>
    /// <returns>True if attribute name is for a namespace attribute, otherwise false.</returns>
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

    private IEnumerable<IXmlNode> ValueAttributes(IEnumerable<IXmlNode> c)
    {
      return c.Where(a => a.NamespaceURI != JsonNamespaceUri);
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
      if (typeof(XmlNode).IsAssignableFrom(valueType))
        return true;
#if !NET20
      if (typeof(XObject).IsAssignableFrom(valueType))
        return true;
#endif

      return false;
    }
  }
}
#endif