using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

#if SILVERLIGHT
[assembly: AssemblyTitle("Newtonsoft Json.NET Tests Silverlight")]
#elif PocketPC
[assembly: AssemblyTitle("Newtonsoft Json.NET Tests Compact")]
#else
[assembly: AssemblyTitle("Newtonsoft Json.NET Tests")]
#endif

[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Newtonsoft")]
[assembly: AssemblyProduct("Newtonsoft Json.NET Tests")]
[assembly: AssemblyCopyright("Copyright © Newtonsoft 2008")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("0be3d72b-d2ef-409c-985c-d3ede89a25f1")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("4.0.2.0")]
#if !PocketPC
[assembly: AssemblyFileVersion("4.0.2.13707")]
#endif
