//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;

// General Information about the System.Windows.Forms assembly

[assembly: AssemblyTitle ("System.Windows.Forms.dll")]
[assembly: AssemblyDescription ("System.Windows.Forms.dll")]
[assembly: AssemblyDefaultAlias ("System.Windows.Forms.dll")]

[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]
[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]

[assembly: CLSCompliant (true)]
[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: ComVisible (false)]
[assembly: AllowPartiallyTrustedCallers]

//[assembly: AssemblyDelaySign (true)]
//[assembly: AssemblyKeyFile("../msfinal.pub")]

[assembly: AssemblyFileVersion (Consts.FxFileVersion)]
[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
[assembly: Dependency ("System.Drawing,", LoadHint.Always)]
[assembly: Dependency ("System,", LoadHint.Always)]
[assembly: StringFreezing]
[assembly: ComCompatibleVersion (1, 0, 3300, 0)]

[assembly: InternalsVisibleTo("UIAutomationWinforms, PublicKey=00240000048000009400000006020000002400005253413100040000110000004bb98b1af6c1df0df8c02c380e116b7a7f0c8c827aecfccddc6e29b7c754cd608b49dfcef4df9699ad182e50f66afa4e68dabc7b6aeeec0aa4719a5f8e0aae8c193080a706adc3443a8356b1f254142034995532ac176398e12a30f6a74a119a89ac47672c9ae24d7e90de686557166e3b873cd707884431a0451d9d6f7fe795")]
[assembly: InternalsVisibleTo("Mono.WinformsSupport, PublicKey=00240000048000009400000006020000002400005253413100040000110000004bb98b1af6c1df0df8c02c380e116b7a7f0c8c827aecfccddc6e29b7c754cd608b49dfcef4df9699ad182e50f66afa4e68dabc7b6aeeec0aa4719a5f8e0aae8c193080a706adc3443a8356b1f254142034995532ac176398e12a30f6a74a119a89ac47672c9ae24d7e90de686557166e3b873cd707884431a0451d9d6f7fe795")]

[assembly: InternalsVisibleTo("MacBridge, PublicKey=00240000048000009400000006020000002400005253413100040000110000007731a01182155e151614cb45d9579ae724794b01182740e9d141daa64eb2797a05f91f3c81e43d9a964524405afd2b2a9b2874c960d156849ad9ce42bb0164480faaa5c8846d966638ff763d73e01cdb88316c620167337cfb18146b7277688d11fa07d5862030611a715239788a2ebc7fc0d59a29ea385db856e72b778f528f")]
