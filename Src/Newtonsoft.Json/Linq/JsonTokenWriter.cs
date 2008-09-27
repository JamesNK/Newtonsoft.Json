using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents a writer that provides a fast, non-cached, forward-only way of generating Json data.
  /// </summary>
  public class JsonTokenWriter : JsonWriter
  {
    private JContainer _token;
    private JContainer _parent;

    /// <summary>
    /// Gets the token being writen.
    /// </summary>
    /// <value>The token being writen.</value>
    public JContainer Token
    {
      get { return _token; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonTokenWriter"/> class writing to the given <see cref="JContainer"/>.
    /// </summary>
    /// <param name="container">The container being written to.</param>
    public JsonTokenWriter(JContainer container)
    {
      ValidationUtils.ArgumentNotNull(container, "container");

      _token = container;
      _parent = container;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonTokenWriter"/> class.
    /// </summary>
    public JsonTokenWriter()
    {
    }

    /// <summary>
    /// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
    /// </summary>
    public override void Flush()
    {
    }

    /// <summary>
    /// Closes this stream and the underlying stream.
    /// </summary>
    public override void Close()
    {
      base.Close();
    }

    /// <summary>
    /// Writes the beginning of a Json object.
    /// </summary>
    public override void WriteStartObject()
    {
      base.WriteStartObject();

      AddParent(new JObject());
    }

    private void AddParent(JContainer container)
    {
      if (_parent == null)
        _token = container;
      else
        _parent.Add(container);

      _parent = container;
    }

    private void RemoveParent()
    {
      _parent = _parent.Parent;

      if (_parent != null && _parent.Type == JsonTokenType.Property)
        _parent = _parent.Parent;
    }

    /// <summary>
    /// Writes the beginning of a Json array.
    /// </summary>
    public override void WriteStartArray()
    {
      base.WriteStartArray();

      AddParent(new JArray());
    }

    /// <summary>
    /// Writes the start of a constructor with the given name.
    /// </summary>
    /// <param name="name">The name of the constructor.</param>
    public override void WriteStartConstructor(string name)
    {
      base.WriteStartConstructor(name);

      AddParent(new JConstructor(name));
    }

    /// <summary>
    /// Writes the end.
    /// </summary>
    /// <param name="token">The token.</param>
    protected override void WriteEnd(JsonToken token)
    {
      RemoveParent();
    }

    /// <summary>
    /// Writes the property name of a name/value pair on a Json object.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    public override void WritePropertyName(string name)
    {
      base.WritePropertyName(name);

      AddParent(new JProperty(name));
    }

    private void AddValue(object value, JsonToken token)
    {
      AddValue(new JValue(value), token);
    }

    private void AddValue(JValue value, JsonToken token)
    {
      _parent.Add(value);

      if (_parent.Type == JsonTokenType.Property)
        _parent = _parent.Parent;
    }

    #region WriteValue methods
    /// <summary>
    /// Writes a null value.
    /// </summary>
    public override void WriteNull()
    {
      base.WriteNull();
      AddValue(null, JsonToken.Null);
    }

    /// <summary>
    /// Writes an undefined value.
    /// </summary>
    public override void WriteUndefined()
    {
      base.WriteUndefined();
      AddValue(null, JsonToken.Undefined);
    }

    /// <summary>
    /// Writes raw JSON.
    /// </summary>
    /// <param name="json">The raw JSON to write.</param>
    public override void WriteRaw(string json)
    {
      base.WriteRaw(json);
      // hack
      AddValue(JValue.CreateRaw(json), JsonToken.Raw);
    }

    /// <summary>
    /// Writes a <see cref="String"/> value.
    /// </summary>
    /// <param name="value">The <see cref="String"/> value to write.</param>
    public override void WriteValue(string value)
    {
      base.WriteValue(value);
      AddValue(value ?? string.Empty, JsonToken.String);
    }

    /// <summary>
    /// Writes a <see cref="Int32"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Int32"/> value to write.</param>
    public override void WriteValue(int value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="UInt32"/> value.
    /// </summary>
    /// <param name="value">The <see cref="UInt32"/> value to write.</param>
    public override void WriteValue(uint value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Int64"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Int64"/> value to write.</param>
    public override void WriteValue(long value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="UInt64"/> value.
    /// </summary>
    /// <param name="value">The <see cref="UInt64"/> value to write.</param>
    public override void WriteValue(ulong value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Single"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Single"/> value to write.</param>
    public override void WriteValue(float value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="Double"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Double"/> value to write.</param>
    public override void WriteValue(double value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="Boolean"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Boolean"/> value to write.</param>
    public override void WriteValue(bool value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Boolean);
    }

    /// <summary>
    /// Writes a <see cref="Int16"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Int16"/> value to write.</param>
    public override void WriteValue(short value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="UInt16"/> value.
    /// </summary>
    /// <param name="value">The <see cref="UInt16"/> value to write.</param>
    public override void WriteValue(ushort value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Char"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Char"/> value to write.</param>
    public override void WriteValue(char value)
    {
      base.WriteValue(value);
      AddValue(value.ToString(), JsonToken.String);
    }

    /// <summary>
    /// Writes a <see cref="Byte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Byte"/> value to write.</param>
    public override void WriteValue(byte value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="SByte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="SByte"/> value to write.</param>
    public override void WriteValue(sbyte value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Decimal"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Decimal"/> value to write.</param>
    public override void WriteValue(decimal value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="DateTime"/> value.
    /// </summary>
    /// <param name="value">The <see cref="DateTime"/> value to write.</param>
    public override void WriteValue(DateTime value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Date);
    }

    /// <summary>
    /// Writes a <see cref="DateTimeOffset"/> value.
    /// </summary>
    /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
    public override void WriteValue(DateTimeOffset value)
    {
      base.WriteValue(value);
      AddValue(value, JsonToken.Date);
    }
    #endregion
  }
}
