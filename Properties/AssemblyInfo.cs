﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Digitalis.LDTools.DOM")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Digitalis.LDTools.DOM")]
[assembly: AssemblyCopyright("Copyright © Alex Taylor 2009-2012.  All Rights Reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e475bffd-d575-4bc6-9705-c05058bf0079")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0")]
[assembly: AssemblyFileVersion("1.0.2013.0511")]
[assembly: NeutralResourcesLanguageAttribute("en-GB")]

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Digitalis.LDTools.DOM.UnitTests")]

// required so RenderEngine can implement its own private element subclasses
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Digitalis.LDTools.Controls")]
