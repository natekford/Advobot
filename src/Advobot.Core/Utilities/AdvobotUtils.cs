using System;
using System.IO;
using System.Linq;
using Advobot.Interfaces;
using AdvorangesSettingParser;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Utilities
{
	/// <summary>
	/// Random utilities.
	/// </summary>
	public static class AdvobotUtils
	{
		/// <summary>
		/// Gets the file inside the bot directory.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(this IBotDirectoryAccessor accessor, string fileName)
			=> new FileInfo(Path.Combine(accessor.BaseBotDirectory.FullName, fileName));
		/// <summary>
		/// Gets the path of the object which implements both <see cref="IBotDirectoryAccessor"/> and <see cref="ISettingsBase"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static FileInfo GetFile<T>(this T obj) where T : IBotDirectoryAccessor, ISettingsBase
			=> obj.GetFile(obj);
		/// <summary>
		/// Saves the settings of the object which implements both <see cref="IBotDirectoryAccessor"/> and <see cref="ISettingsBase"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		public static void SaveSettings<T>(this T obj) where T : IBotDirectoryAccessor, ISettingsBase
			=> obj.SaveSettings(obj);
		/// <summary>
		/// Creates a provider and initializes all of its singletons.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceProvider CreateProvider(this IServiceCollection services)
		{
			var provider = services.BuildServiceProvider();
			foreach (var service in services.Where(x => x.Lifetime == ServiceLifetime.Singleton))
			{
				provider.GetRequiredService(service.ServiceType);
			}
			return provider;
		}
		/// <summary>
		/// Regular <see cref="Enum.TryParse{TEnum}(string, out TEnum)"/> except case insenstive.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryParseCaseIns<T>(string s, out T value) where T : struct, Enum
			=> Enum.TryParse(s, true, out value);
		/// <summary>
		/// Acts as an empty <see cref="TryParseDelegate{T}"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[Obsolete("Only intended to be temporary until the tryparses are set up.")]
		public static bool EmptyTryParse<T>(string s, out T value)
		{
			value = default;
			return true;
		}
	}
}