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

#if !(SILVERLIGHT || NETFX_CORE || PORTABLE40 || PORTABLE)
using System;
using System.Collections.Generic;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
  /// <summary>
  ///Represents a strongly typed in-memory cache of data.
  ///</summary>
  [global::System.Serializable()]
  [global::System.ComponentModel.DesignerCategoryAttribute("code")]
  [global::System.ComponentModel.ToolboxItem(true)]
  [global::System.Xml.Serialization.XmlSchemaProviderAttribute("GetTypedDataSetSchema")]
  [global::System.Xml.Serialization.XmlRootAttribute("CustomerDataSet")]
  [global::System.ComponentModel.Design.HelpKeywordAttribute("vs.data.DataSet")]
  public partial class CustomerDataSet : global::System.Data.DataSet
  {

    private CustomersDataTable tableCustomers;

    private global::System.Data.SchemaSerializationMode _schemaSerializationMode = global::System.Data.SchemaSerializationMode.IncludeSchema;

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    public CustomerDataSet()
    {
      this.BeginInit();
      this.InitClass();
      global::System.ComponentModel.CollectionChangeEventHandler schemaChangedHandler = new global::System.ComponentModel.CollectionChangeEventHandler(this.SchemaChanged);
      base.Tables.CollectionChanged += schemaChangedHandler;
      base.Relations.CollectionChanged += schemaChangedHandler;
      this.EndInit();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected CustomerDataSet(global::System.Runtime.Serialization.SerializationInfo info, global::System.Runtime.Serialization.StreamingContext context) :
      base(info, context, false)
    {
      if ((this.IsBinarySerialized(info, context) == true))
      {
        this.InitVars(false);
        global::System.ComponentModel.CollectionChangeEventHandler schemaChangedHandler1 = new global::System.ComponentModel.CollectionChangeEventHandler(this.SchemaChanged);
        this.Tables.CollectionChanged += schemaChangedHandler1;
        this.Relations.CollectionChanged += schemaChangedHandler1;
        return;
      }
      string strSchema = ((string)(info.GetValue("XmlSchema", typeof(string))));
      if ((this.DetermineSchemaSerializationMode(info, context) == global::System.Data.SchemaSerializationMode.IncludeSchema))
      {
        global::System.Data.DataSet ds = new global::System.Data.DataSet();
        ds.ReadXmlSchema(new global::System.Xml.XmlTextReader(new global::System.IO.StringReader(strSchema)));
        if ((ds.Tables["Customers"] != null))
        {
          base.Tables.Add(new CustomersDataTable(ds.Tables["Customers"]));
        }
        this.DataSetName = ds.DataSetName;
        this.Prefix = ds.Prefix;
        this.Namespace = ds.Namespace;
        this.Locale = ds.Locale;
        this.CaseSensitive = ds.CaseSensitive;
        this.EnforceConstraints = ds.EnforceConstraints;
        this.Merge(ds, false, global::System.Data.MissingSchemaAction.Add);
        this.InitVars();
      }
      else
      {
        this.ReadXmlSchema(new global::System.Xml.XmlTextReader(new global::System.IO.StringReader(strSchema)));
      }
      this.GetSerializationData(info, context);
      global::System.ComponentModel.CollectionChangeEventHandler schemaChangedHandler = new global::System.ComponentModel.CollectionChangeEventHandler(this.SchemaChanged);
      base.Tables.CollectionChanged += schemaChangedHandler;
      this.Relations.CollectionChanged += schemaChangedHandler;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    [global::System.ComponentModel.Browsable(false)]
    [global::System.ComponentModel.DesignerSerializationVisibility(global::System.ComponentModel.DesignerSerializationVisibility.Content)]
    public CustomersDataTable Customers
    {
      get
      {
        return this.tableCustomers;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    [global::System.ComponentModel.BrowsableAttribute(true)]
    [global::System.ComponentModel.DesignerSerializationVisibilityAttribute(global::System.ComponentModel.DesignerSerializationVisibility.Visible)]
    public override global::System.Data.SchemaSerializationMode SchemaSerializationMode
    {
      get
      {
        return this._schemaSerializationMode;
      }
      set
      {
        this._schemaSerializationMode = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    [global::System.ComponentModel.DesignerSerializationVisibilityAttribute(global::System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public new global::System.Data.DataTableCollection Tables
    {
      get
      {
        return base.Tables;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    [global::System.ComponentModel.DesignerSerializationVisibilityAttribute(global::System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public new global::System.Data.DataRelationCollection Relations
    {
      get
      {
        return base.Relations;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected override void InitializeDerivedDataSet()
    {
      this.BeginInit();
      this.InitClass();
      this.EndInit();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    public override global::System.Data.DataSet Clone()
    {
      CustomerDataSet cln = ((CustomerDataSet)(base.Clone()));
      cln.InitVars();
      cln.SchemaSerializationMode = this.SchemaSerializationMode;
      return cln;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected override bool ShouldSerializeTables()
    {
      return false;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected override bool ShouldSerializeRelations()
    {
      return false;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected override void ReadXmlSerializable(global::System.Xml.XmlReader reader)
    {
      if ((this.DetermineSchemaSerializationMode(reader) == global::System.Data.SchemaSerializationMode.IncludeSchema))
      {
        this.Reset();
        global::System.Data.DataSet ds = new global::System.Data.DataSet();
        ds.ReadXml(reader);
        if ((ds.Tables["Customers"] != null))
        {
          base.Tables.Add(new CustomersDataTable(ds.Tables["Customers"]));
        }
        this.DataSetName = ds.DataSetName;
        this.Prefix = ds.Prefix;
        this.Namespace = ds.Namespace;
        this.Locale = ds.Locale;
        this.CaseSensitive = ds.CaseSensitive;
        this.EnforceConstraints = ds.EnforceConstraints;
        this.Merge(ds, false, global::System.Data.MissingSchemaAction.Add);
        this.InitVars();
      }
      else
      {
        this.ReadXml(reader);
        this.InitVars();
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    protected override global::System.Xml.Schema.XmlSchema GetSchemaSerializable()
    {
      global::System.IO.MemoryStream stream = new global::System.IO.MemoryStream();
      this.WriteXmlSchema(new global::System.Xml.XmlTextWriter(stream, null));
      stream.Position = 0;
      return global::System.Xml.Schema.XmlSchema.Read(new global::System.Xml.XmlTextReader(stream), null);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    internal void InitVars()
    {
      this.InitVars(true);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    internal void InitVars(bool initTable)
    {
      this.tableCustomers = ((CustomersDataTable)(base.Tables["Customers"]));
      if ((initTable == true))
      {
        if ((this.tableCustomers != null))
        {
          this.tableCustomers.InitVars();
        }
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    private void InitClass()
    {
      this.DataSetName = "CustomerDataSet";
      this.Prefix = "";
      this.EnforceConstraints = true;
      this.SchemaSerializationMode = global::System.Data.SchemaSerializationMode.IncludeSchema;
      this.tableCustomers = new CustomersDataTable();
      base.Tables.Add(this.tableCustomers);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    private bool ShouldSerializeCustomers()
    {
      return false;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    private void SchemaChanged(object sender, global::System.ComponentModel.CollectionChangeEventArgs e)
    {
      if ((e.Action == global::System.ComponentModel.CollectionChangeAction.Remove))
      {
        this.InitVars();
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    public static global::System.Xml.Schema.XmlSchemaComplexType GetTypedDataSetSchema(global::System.Xml.Schema.XmlSchemaSet xs)
    {
      CustomerDataSet ds = new CustomerDataSet();
      global::System.Xml.Schema.XmlSchemaComplexType type = new global::System.Xml.Schema.XmlSchemaComplexType();
      global::System.Xml.Schema.XmlSchemaSequence sequence = new global::System.Xml.Schema.XmlSchemaSequence();
      global::System.Xml.Schema.XmlSchemaAny any = new global::System.Xml.Schema.XmlSchemaAny();
      any.Namespace = ds.Namespace;
      sequence.Items.Add(any);
      type.Particle = sequence;
      global::System.Xml.Schema.XmlSchema dsSchema = ds.GetSchemaSerializable();
      if (xs.Contains(dsSchema.TargetNamespace))
      {
        global::System.IO.MemoryStream s1 = new global::System.IO.MemoryStream();
        global::System.IO.MemoryStream s2 = new global::System.IO.MemoryStream();
        try
        {
          global::System.Xml.Schema.XmlSchema schema = null;
          dsSchema.Write(s1);
          for (global::System.Collections.IEnumerator schemas = xs.Schemas(dsSchema.TargetNamespace).GetEnumerator(); schemas.MoveNext(); )
          {
            schema = ((global::System.Xml.Schema.XmlSchema)(schemas.Current));
            s2.SetLength(0);
            schema.Write(s2);
            if ((s1.Length == s2.Length))
            {
              s1.Position = 0;
              s2.Position = 0;
              for (; ((s1.Position != s1.Length)
                          && (s1.ReadByte() == s2.ReadByte())); )
              {
                ;
              }
              if ((s1.Position == s1.Length))
              {
                return type;
              }
            }
          }
        }
        finally
        {
          if ((s1 != null))
          {
            s1.Close();
          }
          if ((s2 != null))
          {
            s2.Close();
          }
        }
      }
      xs.Add(dsSchema);
      return type;
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    public delegate void CustomersRowChangeEventHandler(object sender, CustomersRowChangeEvent e);

    /// <summary>
    ///Represents the strongly named DataTable class.
    ///</summary>
    [global::System.Serializable()]
    [global::System.Xml.Serialization.XmlSchemaProviderAttribute("GetTypedTableSchema")]
    public partial class CustomersDataTable : global::System.Data.DataTable, global::System.Collections.IEnumerable
    {

      private global::System.Data.DataColumn columnCustomerID;

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public CustomersDataTable()
      {
        this.TableName = "Customers";
        this.BeginInit();
        this.InitClass();
        this.EndInit();
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      internal CustomersDataTable(global::System.Data.DataTable table)
      {
        this.TableName = table.TableName;
        if ((table.CaseSensitive != table.DataSet.CaseSensitive))
        {
          this.CaseSensitive = table.CaseSensitive;
        }
        if ((table.Locale.ToString() != table.DataSet.Locale.ToString()))
        {
          this.Locale = table.Locale;
        }
        if ((table.Namespace != table.DataSet.Namespace))
        {
          this.Namespace = table.Namespace;
        }
        this.Prefix = table.Prefix;
        this.MinimumCapacity = table.MinimumCapacity;
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      protected CustomersDataTable(global::System.Runtime.Serialization.SerializationInfo info, global::System.Runtime.Serialization.StreamingContext context) :
        base(info, context)
      {
        this.InitVars();
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public global::System.Data.DataColumn CustomerIDColumn
      {
        get
        {
          return this.columnCustomerID;
        }
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      [global::System.ComponentModel.Browsable(false)]
      public int Count
      {
        get
        {
          return this.Rows.Count;
        }
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public CustomersRow this[int index]
      {
        get
        {
          return ((CustomersRow)(this.Rows[index]));
        }
      }

      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public event CustomersRowChangeEventHandler CustomersRowChanging;

      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public event CustomersRowChangeEventHandler CustomersRowChanged;

      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public event CustomersRowChangeEventHandler CustomersRowDeleting;

      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public event CustomersRowChangeEventHandler CustomersRowDeleted;

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public void AddCustomersRow(CustomersRow row)
      {
        this.Rows.Add(row);
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public CustomersRow AddCustomersRow(string CustomerID)
      {
        CustomersRow rowCustomersRow = ((CustomersRow)(this.NewRow()));
        object[] columnValuesArray = new object[] {
                        CustomerID};
        rowCustomersRow.ItemArray = columnValuesArray;
        this.Rows.Add(rowCustomersRow);
        return rowCustomersRow;
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public virtual global::System.Collections.IEnumerator GetEnumerator()
      {
        return this.Rows.GetEnumerator();
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public override global::System.Data.DataTable Clone()
      {
        CustomersDataTable cln = ((CustomersDataTable)(base.Clone()));
        cln.InitVars();
        return cln;
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      protected override global::System.Data.DataTable CreateInstance()
      {
        return new CustomersDataTable();
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      internal void InitVars()
      {
        this.columnCustomerID = base.Columns["CustomerID"];
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      private void InitClass()
      {
        this.columnCustomerID = new global::System.Data.DataColumn("CustomerID", typeof(string), null, global::System.Data.MappingType.Element);
        base.Columns.Add(this.columnCustomerID);
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public CustomersRow NewCustomersRow()
      {
        return ((CustomersRow)(this.NewRow()));
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      protected override global::System.Data.DataRow NewRowFromBuilder(global::System.Data.DataRowBuilder builder)
      {
        return new CustomersRow(builder);
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      protected override global::System.Type GetRowType()
      {
        return typeof(CustomersRow);
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      protected override void OnRowChanged(global::System.Data.DataRowChangeEventArgs e)
      {
        base.OnRowChanged(e);
        if ((this.CustomersRowChanged != null))
        {
          this.CustomersRowChanged(this, new CustomersRowChangeEvent(((CustomersRow)(e.Row)), e.Action));
        }
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      protected override void OnRowChanging(global::System.Data.DataRowChangeEventArgs e)
      {
        base.OnRowChanging(e);
        if ((this.CustomersRowChanging != null))
        {
          this.CustomersRowChanging(this, new CustomersRowChangeEvent(((CustomersRow)(e.Row)), e.Action));
        }
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      protected override void OnRowDeleted(global::System.Data.DataRowChangeEventArgs e)
      {
        base.OnRowDeleted(e);
        if ((this.CustomersRowDeleted != null))
        {
          this.CustomersRowDeleted(this, new CustomersRowChangeEvent(((CustomersRow)(e.Row)), e.Action));
        }
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      protected override void OnRowDeleting(global::System.Data.DataRowChangeEventArgs e)
      {
        base.OnRowDeleting(e);
        if ((this.CustomersRowDeleting != null))
        {
          this.CustomersRowDeleting(this, new CustomersRowChangeEvent(((CustomersRow)(e.Row)), e.Action));
        }
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public void RemoveCustomersRow(CustomersRow row)
      {
        this.Rows.Remove(row);
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public static global::System.Xml.Schema.XmlSchemaComplexType GetTypedTableSchema(global::System.Xml.Schema.XmlSchemaSet xs)
      {
        global::System.Xml.Schema.XmlSchemaComplexType type = new global::System.Xml.Schema.XmlSchemaComplexType();
        global::System.Xml.Schema.XmlSchemaSequence sequence = new global::System.Xml.Schema.XmlSchemaSequence();
        CustomerDataSet ds = new CustomerDataSet();
        global::System.Xml.Schema.XmlSchemaAny any1 = new global::System.Xml.Schema.XmlSchemaAny();
        any1.Namespace = "http://www.w3.org/2001/XMLSchema";
        any1.MinOccurs = new decimal(0);
        any1.MaxOccurs = decimal.MaxValue;
        any1.ProcessContents = global::System.Xml.Schema.XmlSchemaContentProcessing.Lax;
        sequence.Items.Add(any1);
        global::System.Xml.Schema.XmlSchemaAny any2 = new global::System.Xml.Schema.XmlSchemaAny();
        any2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
        any2.MinOccurs = new decimal(1);
        any2.ProcessContents = global::System.Xml.Schema.XmlSchemaContentProcessing.Lax;
        sequence.Items.Add(any2);
        global::System.Xml.Schema.XmlSchemaAttribute attribute1 = new global::System.Xml.Schema.XmlSchemaAttribute();
        attribute1.Name = "namespace";
        attribute1.FixedValue = ds.Namespace;
        type.Attributes.Add(attribute1);
        global::System.Xml.Schema.XmlSchemaAttribute attribute2 = new global::System.Xml.Schema.XmlSchemaAttribute();
        attribute2.Name = "tableTypeName";
        attribute2.FixedValue = "CustomersDataTable";
        type.Attributes.Add(attribute2);
        type.Particle = sequence;
        global::System.Xml.Schema.XmlSchema dsSchema = ds.GetSchemaSerializable();
        if (xs.Contains(dsSchema.TargetNamespace))
        {
          global::System.IO.MemoryStream s1 = new global::System.IO.MemoryStream();
          global::System.IO.MemoryStream s2 = new global::System.IO.MemoryStream();
          try
          {
            global::System.Xml.Schema.XmlSchema schema = null;
            dsSchema.Write(s1);
            for (global::System.Collections.IEnumerator schemas = xs.Schemas(dsSchema.TargetNamespace).GetEnumerator(); schemas.MoveNext(); )
            {
              schema = ((global::System.Xml.Schema.XmlSchema)(schemas.Current));
              s2.SetLength(0);
              schema.Write(s2);
              if ((s1.Length == s2.Length))
              {
                s1.Position = 0;
                s2.Position = 0;
                for (; ((s1.Position != s1.Length)
                            && (s1.ReadByte() == s2.ReadByte())); )
                {
                  ;
                }
                if ((s1.Position == s1.Length))
                {
                  return type;
                }
              }
            }
          }
          finally
          {
            if ((s1 != null))
            {
              s1.Close();
            }
            if ((s2 != null))
            {
              s2.Close();
            }
          }
        }
        xs.Add(dsSchema);
        return type;
      }
    }

    /// <summary>
    ///Represents strongly named DataRow class.
    ///</summary>
    public partial class CustomersRow : global::System.Data.DataRow
    {

      private CustomersDataTable tableCustomers;

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      internal CustomersRow(global::System.Data.DataRowBuilder rb) :
        base(rb)
      {
        this.tableCustomers = ((CustomersDataTable)(this.Table));
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public string CustomerID
      {
        get
        {
          try
          {
            return ((string)(this[this.tableCustomers.CustomerIDColumn]));
          }
          catch (global::System.InvalidCastException e)
          {
            throw new global::System.Data.StrongTypingException("The value for column \'CustomerID\' in table \'Customers\' is DBNull.", e);
          }
        }
        set
        {
          this[this.tableCustomers.CustomerIDColumn] = value;
        }
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public bool IsCustomerIDNull()
      {
        return this.IsNull(this.tableCustomers.CustomerIDColumn);
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public void SetCustomerIDNull()
      {
        this[this.tableCustomers.CustomerIDColumn] = global::System.Convert.DBNull;
      }
    }

    /// <summary>
    ///Row event argument class
    ///</summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    public class CustomersRowChangeEvent : global::System.EventArgs
    {

      private CustomersRow eventRow;

      private global::System.Data.DataRowAction eventAction;

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public CustomersRowChangeEvent(CustomersRow row, global::System.Data.DataRowAction action)
      {
        this.eventRow = row;
        this.eventAction = action;
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public CustomersRow Row
      {
        get
        {
          return this.eventRow;
        }
      }

      [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
      public global::System.Data.DataRowAction Action
      {
        get
        {
          return this.eventAction;
        }
      }
    }
  }
}
#endif