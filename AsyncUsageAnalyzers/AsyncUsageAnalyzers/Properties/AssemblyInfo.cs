// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AsyncUsageAnalyzers")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Tunnel Vision Laboratories, LLC")]
[assembly: AssemblyProduct("AsyncUsageAnalyzers")]
[assembly: AssemblyCopyright("Copyright © Sam Harwell 2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0-dev")]

#if DEVELOPMENT_KEY
[assembly: InternalsVisibleTo("AsyncUsageAnalyzers.CodeFixes, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b149603f70e8d350f7f2145912b4ba208a4937af096b6677387d1980188af7da84d246943a3c7e68a3345dcf5f7b47bdb5d10c3c79bca5055910b8bf06d0ba406f9006a5a45e27781fd69f53397e57fb56e8086d139a3ac6a8bc65475b83e0d3a66240ff6b1361b047fa72c74f4b8140dba5e4bca3ab5a07b405d7da315da1c9")]
#else
[assembly: InternalsVisibleTo("AsyncUsageAnalyzers.CodeFixes, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b951f77495c7e02b9acf9bb596e43893485441a41e2e6c24683d906b20ff3df76014c4844a9ca044b92edd2d7fb518b6e1025f8b030709ea922fcdaabcf949b13e5147a48a8579114d87ec0e0f1dfc073444c4915d26bf829599c4f2a32b26e5804cb4720d21beaecd9bc364ef8c83165bb49b528b553838c9bba44c75e0f0b5")]
#endif
[assembly: InternalsVisibleTo("AsyncUsageAnalyzers.Test, PublicKey=00240000048000009400000006020000002400005253413100040000010001004900a177aa63863e06209caddd95cc3591a1a30ad53d84be6a531e4a89246078d14f2b7769aa47d5a23e6957d6a4985dffe5571dbc5bf176f2b1870fa91f558de0522109db34900d8d190b15f564f636c536bfeb844543f05362c562fe3e64e444a92e5e02a4cdf35a3d934d06b9c300512f8a70cd741d0e484c5940cd79cfbb")]
