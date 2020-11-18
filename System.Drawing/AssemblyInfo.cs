using System.Reflection;
using System.Runtime.CompilerServices;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle ("System.Drawing.dll")]
[assembly: AssemblyDescription ("System.Drawing.dll")]
[assembly: AssemblyDefaultAlias ("System.Drawing.dll")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("Jimmy")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion("4.0.*")]

// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile("../mono/mcs/class/msfinal.pub")]

[assembly: InternalsVisibleTo("MacBridge, PublicKey=00240000048000009400000006020000002400005253413100040000110000007731a01182155e151614cb45d9579ae724794b01182740e9d141daa64eb2797a05f91f3c81e43d9a964524405afd2b2a9b2874c960d156849ad9ce42bb0164480faaa5c8846d966638ff763d73e01cdb88316c620167337cfb18146b7277688d11fa07d5862030611a715239788a2ebc7fc0d59a29ea385db856e72b778f528f")]
