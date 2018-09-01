using System;
using Advobot.Interfaces;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Indicates after the command has executed guild settings should be saved.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class SaveGuildSettingsAttribute : RequiredServices
	{
		/// <summary>
		/// Creates an instance of <see cref="SaveGuildSettingsAttribute"/>.
		/// </summary>
		public SaveGuildSettingsAttribute() : base(typeof(IGuildSettings), typeof(IBotSettings)) { }
	}

	/// <summary>
	/// Indicates after the command has executed bot settings should be saved.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class SaveBotSettingsAttribute : RequiredServices
	{
		/// <summary>
		/// Creates an instance of <see cref="SaveBotSettingsAttribute"/>.
		/// </summary>
		public SaveBotSettingsAttribute() : base(typeof(IBotSettings)) { }
	}
}