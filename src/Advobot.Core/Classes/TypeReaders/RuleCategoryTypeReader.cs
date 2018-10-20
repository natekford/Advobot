﻿using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find a rule category in the guild settings.
	/// </summary>
	public sealed class RuleCategoryTypeReader : TypeReader<AdvobotCommandContext>
	{
		/// <summary>
		/// Attempts to find a category with the supplied input as a name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(AdvobotCommandContext context, string input, IServiceProvider services)
		{
			return context.GuildSettings.Rules?.Categories?.ContainsKey(input) ?? false
				? Task.FromResult(TypeReaderResult.FromSuccess(input))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Unable to find a rule category matching `{input}`."));
		}
	}
}
