using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Advobot;
using Advobot.CommandAssemblies;
using Advobot.Invites;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany("Advorange")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCopyright("Copyright © 2019")]
[assembly: AssemblyDescription("A Discord invite aggregator.")]
[assembly: AssemblyProduct("Advobot")]
[assembly: AssemblyTitle("Advobot.Invites")]
[assembly: NeutralResourcesLanguage("en")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a7f7a3a2-4e32-4426-a314-2551f7bf8753")]

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
[assembly: CommandAssembly("en-US", InstantiatorType = typeof(InviteInstantiator))]
[assembly: InternalsVisibleTo("Advobot.Tests")]