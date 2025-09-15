using Advobot;
using Advobot.Levels;
using Advobot.Levels.Database;
using Advobot.Levels.Service;
using Advobot.Serilog;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using YACCS.Plugins;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany(Constants.ASSEMBLY_COMPANY)]
[assembly: AssemblyConfiguration(Constants.ASSEMBLY_CONFIGURATION)]
[assembly: AssemblyCopyright(Constants.ASSEMBLY_COPYRIGHT)]
[assembly: AssemblyDescription("Commands for chat XP.")]
[assembly: AssemblyProduct(Constants.ASSEMBLY_PRODUCT)]
[assembly: AssemblyTitle("Advobot.Levels")]
[assembly: NeutralResourcesLanguage(Constants.ASSEMBLY_LANGUAGE)]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("b90908a6-da2d-42d5-9557-9e5003ee0017")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion(Constants.ASSEMBLY_VERSION)]

// Indicates the assembly has commands in it for the bot to use
[assembly: LevelInstantiator(SupportedCultures = ["en-US"])]
[assembly: InternalsVisibleTo("Advobot.Tests")]

namespace Advobot.Levels;

public sealed class LevelInstantiator : PluginAttribute<IServiceCollection>
{
	public override Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSQLiteDatabase<LevelDatabase>("Levels")
			.AddSingletonWithLogger<LevelService>("Levels")
			.AddSingleton<LevelServiceConfig>();

		return Task.CompletedTask;
	}
}