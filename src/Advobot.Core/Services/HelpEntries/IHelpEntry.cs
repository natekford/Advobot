using System;
using System.Collections.Generic;
using Advobot.Interfaces;
using Advobot.Services.GuildSettings;
using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Abstraction for information about a module.
	/// </summary>
	public interface IHelpEntry : IFormattable, INameable
	{
		/// <summary>
		/// Whether or not the command can be toggled.
		/// </summary>
		bool AbleToBeToggled { get; }
		/// <summary>
		/// Whether or not the command is on by default.
		/// </summary>
		bool DefaultEnabled { get; }
		/// <summary>
		/// Describes what the command does.
		/// </summary>
		string Description { get; }
		/// <summary>
		/// The category the command is in.
		/// </summary>
		string? Category { get; }
		/// <summary>
		/// Other names to invoke the command.
		/// </summary>
		IReadOnlyCollection<string> Aliases { get; }
		/// <summary>
		/// The base permissions to use the command.
		/// </summary>
		IReadOnlyCollection<PreconditionAttribute> BasePerms { get; }

		/// <summary>
		/// Returns a string with all the information about the command and whether it's currently enabled.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		string ToString(IGuildSettings? settings, IFormatProvider? formatProvider);
		/// <summary>
		/// Returns a string with all the information about the command and whether it's currently enabled.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="commandIndex"></param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		string ToString(IGuildSettings? settings, IFormatProvider? formatProvider, int commandIndex);
	}
}