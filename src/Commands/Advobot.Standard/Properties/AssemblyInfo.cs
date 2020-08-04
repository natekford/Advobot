using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Advobot;
using Advobot.CommandAssemblies;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany("Advorange")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCopyright("Copyright © 2020")]
[assembly: AssemblyDescription("Standard commands for interacting with Discord via Advobot.")]
[assembly: AssemblyProduct("Advobot")]
[assembly: AssemblyTitle("Advobot.Standard")]
[assembly: NeutralResourcesLanguage("en")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("de73f2ef-eb0c-409b-8762-b164dbcbab30")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion(Constants.BOT_VERSION)]
[assembly: AssemblyFileVersion(Constants.BOT_VERSION)]
[assembly: AssemblyInformationalVersion(Constants.BOT_VERSION)]

// Indicates the assembly has commands in it for the bot to use
[assembly: CommandAssembly("en-US")]
[assembly: InternalsVisibleTo("Advobot.Tests")]