using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Indicates after the command has executed guild settings should be saved.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class SaveGuildSettingsAttribute : Attribute { }
	/// <summary>
	/// Indicates after the command has executed bot settings should be saved.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class SaveBotSettingsAttribute : Attribute { }
	/// <summary>
	/// Indicates after the command has executed low level configuration should be saved.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class SaveLowLevelConfigAttribute : Attribute { }
}