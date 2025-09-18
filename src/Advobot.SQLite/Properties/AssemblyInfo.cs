using Advobot;

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany(Constants.ASSEMBLY_COMPANY)]
[assembly: AssemblyConfiguration(Constants.ASSEMBLY_CONFIGURATION)]
[assembly: AssemblyCopyright(Constants.ASSEMBLY_COPYRIGHT)]
[assembly: AssemblyDescription("SQLite database access for Advobot.")]
[assembly: AssemblyProduct(Constants.ASSEMBLY_PRODUCT)]
[assembly: AssemblyTitle("Advobot.SQLite")]
[assembly: NeutralResourcesLanguage(Constants.ASSEMBLY_LANGUAGE)]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a3917caf-9fe4-4314-9ecf-7b6e1b55a006")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion(Constants.ASSEMBLY_VERSION)]
[assembly: InternalsVisibleTo("Advobot.Tests")]