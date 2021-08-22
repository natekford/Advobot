using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Advobot;
using Advobot.CommandAssemblies;
using Advobot.Gacha;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany(Constants.ASSEMBLY_COMPANY)]
#if DEBUG
[assembly: AssemblyConfiguration(Constants.AC_DEB)]
#else
[assembly: AssemblyConfiguration(Constants.AC_REL)]
#endif
[assembly: AssemblyCopyright(Constants.ASSEMBLY_COPYRIGHT)]
[assembly: AssemblyDescription("Commands for a gacha game.")]
[assembly: AssemblyProduct(Constants.ASSEMBLY_PRODUCT)]
[assembly: AssemblyTitle("Advobot.Gacha")]
[assembly: NeutralResourcesLanguage(Constants.ASSEMBLY_LANGUAGE)]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c2fce325-6db2-46bc-a4e8-5771f7e49c9f")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion(Constants.ASSEMBLY_VERSION)]

// Indicates the assembly has commands in it for the bot to use
[assembly: CommandAssembly("en-US", InstantiatorType = typeof(GachaInstantiation))]
[assembly: InternalsVisibleTo("Advobot.Tests")]