using Advobot;
using Advobot.Services;
using Advobot.Services.GuildSettings;
using Advobot.Settings;
using Advobot.Settings.Database;
using Advobot.Settings.Service;
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
[assembly: AssemblyDescription("Commands for changing the settings of Advobot.")]
[assembly: AssemblyProduct(Constants.ASSEMBLY_PRODUCT)]
[assembly: AssemblyTitle("Advobot.Settings")]
[assembly: NeutralResourcesLanguage(Constants.ASSEMBLY_LANGUAGE)]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e440bc7d-07ae-4d6c-b24b-e32369520075")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion(Constants.ASSEMBLY_VERSION)]

// Indicates the assembly has commands in it for the bot to use
[assembly: SettingsInstantiator(SupportedCultures = ["en-US"])]
[assembly: InternalsVisibleTo("Advobot.Tests")]

namespace Advobot.Settings;

public sealed class SettingsInstantiator : PluginAttribute<IServiceCollection>
{
	public override Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSQLiteDatabase<SettingsDatabase>("GuildSettings")
			.ReplaceAllWithSingleton<IGuildSettingsService, GuildSettingsService>()
			.AddSingleton<ICommandValidator, CommandValidator>();

		return Task.CompletedTask;
	}
}