Json.NET
===============
Json.NET is a popular high-performance JSON framework for .NET

Features
---------------

+ Flexible JSON serializer for converting between .NET objects and JSON
+ LINQ to JSON for manually reading and writing JSON
+ High performance, faster than .NET's built-in JSON serializers
+ Write indented, easy to read JSON
+ Convert JSON to and from XML
+ Supports .NET 2, .NET 3.5, .NET 4, Silverlight, Windows Phone and Windows 8. 

The JSON serializer is a good choice when the JSON you are reading or writing maps closely to a .NET class.

LINQ to JSON is good for situations where you are only interested in getting values from JSON,You don't have a class to serialize or deserialize to, or the JSON is radically different from your class and you need to manually read and write from your objects.

Donate
---------------
Json.NET is a personal open source project. Started in 2006, I have put hundreds of hours adding, refining and tuning Json.NET with the goal to make it not just the best JSON serializer for .NET but the best serializer for any computer language. I need your help to achieve this.

[![Click here to lend your support to: Json.NET and make a donation at www.pledgie.com !](https://pledgie.com/campaigns/18941.png?skin_name=chrome)](http://www.pledgie.com/campaigns/18941 "Donate")

                                                                                     | Json.NET   | DataContractJsonSerializer   | JavaScriptSerializer
:----------------------------------------------------------------------------------- | :--------: | :--------------------------: | :-------------------:
Supports JSON                                                                        | ✓          | ✓                            | ✓
Supports BSON                                                                        | ✓          |                              | 
Supports JSON Schema                                                                 | ✓          |                              | 
Supports .NET 2.0                                                                    | ✓          |                              | 
Supports .NET 3.5                                                                    | ✓          | ✓                            | ✓
Supports .NET 4.0                                                                    | ✓          | ✓                            | ✓
Supports .NET 4.5                                                                    | ✓          | ✓                            | ✓
Supports Silverlight                                                                 | ✓          | ✓                            | 
Supports Windows Phone                                                               | ✓          | ✓                            | 
Supports Windows 8                                                                   | ✓          | ✓                            | 
Supports Portable Class Library                                                      | ✓          | ✓                            | 
Open Source                                                                          | ✓          |                              | 
MIT License                                                                          | ✓          |                              | 
LINQ to JSON                                                                         | ✓          |                              | 
Thread Safe                                                                          | ✓          | ✓                            | ✓
XPath-like JSON query syntax                                                         | ✓          |                              | 
Indented JSON support                                                                | ✓          |                              | 
Efficient dictionary serialization                                                   | ✓          |                              | ✓
Nonsensical dictionary serialization                                                 |            | ✓                            | 
Deserializes IList, IEnumerable, ICollection, IDictionary properties                 | ✓          |                              | 
Serializes circular references                                                       | ✓          |                              | 
Supports serializing objects by reference                                            | ✓          |                              | 
Deserializes polymorphic properties and collections                                  | ✓          | ✓                            | ✓
Serializes and deserializes multidimensional arrays                                  | ✓          |                              | 
Supports including type names with JSON                                              | ✓          | ✓                            | ✓
Globally customize serialization process                                             | ✓          | ✓                            | 
Supports excluding null values when serializing                                      | ✓          |                              | 
Supports SerializationBinder                                                         | ✓          |                              | 
Conditional property serialization                                                   | ✓          |                              | 
Includes line number information in errors                                           | ✓          | ✓                            | 
Converts XML to JSON and JSON to XML                                                 | ✓          |                              | 
JSON Schema validation                                                               | ✓          |                              | 
JSON Schema generation from .NET types                                               | ✓          |                              | 
Camel case JSON property names                                                       | ✓          |                              | 
Non-default constructors support                                                     | ✓          |                              | 
Serialization error handling                                                         | ✓          |                              | 
Supports populating an existing object                                               | ✓          |                              | 
Efficiently serializes byte arrays as base64 text                                    | ✓          |                              | 
Handles NaN, Infinity, -Infinity and undefined                                       | ✓          |                              | 
Handles JavaScript constructors                                                      | ✓          |                              | 
Serializes .NET 4.0 dynamic objects                                                  | ✓          |                              | 
Serializes ISerializable objects                                                     | ✓          |                              | 
Supports serializing enums to their text name                                        | ✓          |                              | 
JSON recursion limit support                                                         | ✓          | ✓                            | ✓
Attribute property name customization                                                | ✓          | ✓                            | 
Attribute property order customization                                               | ✓          | ✓                            | 
Attribute property required customization                                            | ✓          | ✓                            | 
Supports ISO8601 dates                                                               | ✓          |                              | 
Supports JavaScript constructor dates                                                | ✓          |                              | 
Supports Microsoft AJAX dates                                                        | ✓          | ✓                            | ✓
Unquoted property names support                                                      | ✓          |                              | 
Raw JSON support                                                                     | ✓          |                              | 
Supports reading and writing comments                                                | ✓          |                              | 
Supports BigInteger                                                                  | ✓          |                              | 
Serializes anonymous types                                                           | ✓          |                              | ✓
Deserializes anonymous types                                                         | ✓          |                              | 
Deserializes read only collections                                                   | ✓          |                              | 
Opt-in mode serialization                                                            | ✓          | ✓                            | 
Opt-out mode serialization                                                           | ✓          |                              | ✓
Field (Serializable) mode serialization                                              | ✓          | ✓                            | 
Efficiently stream reading and writing JSON                                          | ✓          | ✓                            | 
Single or double quote JSON content                                                  | ✓          |                              | 
Supports overriding a type's serialization                                           | ✓          |                              | ✓
Supports OnDeserialized, OnSerializing, OnSerialized and OnDeserializing attributes  | ✓          | ✓                            | 
Supports serializing private properties                                              | ✓          | ✓                            | 
DataMember attribute support                                                         | ✓          | ✓                            | 
MetdataType attribute support                                                        | ✓          |                              | 
DefaultValue attribute support                                                       | ✓          |                              | 
Serializes DataSets and DataTables                                                   | ✓          |                              | 
Serailizes Entity Framework                                                          | ✓          |                              | 
Serializes nHibernate                                                                | ✓          |                              | 
Case-insensitive property deserialization                                            | ✓          |                              | 
Tracing                                                                              | ✓          | ✓                            | 

Performance Comparison
---------------
![Comparison](http://download.codeplex.com/Download?ProjectName=json&DownloadId=669163)

The source code for this benchmark is included in the Json.NET unit tests.

My Blog
---------------
My blog can be found at [http://james.newtonking.com](http://james.newtonking.com) where I post news and updates about Json.NET.

My Twitter
---------------
My twitter account can be found at [@JamesNK](http://twitter.com/JamesNK)
