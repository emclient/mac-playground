using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// AssemblyTitle, AssemblyProduct and AssemblyCopyright are in BrandingUtils.cs

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: InternalsVisibleTo("MailClient.UITest")]

[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("93f6b425-2131-4be7-9efe-6bd0161c9c71")]

// NGEN optimization
[assembly: Dependency("MailClient", LoadHint.Always)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("7.1.0.0")]
[assembly: AssemblyFileVersion("7.1.0.0")]
