﻿using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes.Attributes.ParameterPreconditions.SettingValidation
{
	/// <summary>
	/// Makes sure the passed in string is a valid command category.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateCommandCategoryAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (!(value is string str))
			{
				throw new ArgumentException(nameof(value));
			}
			return services.GetRequiredService<IHelpEntryService>().GetCategories().CaseInsContains(str)
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError("Invalid category supplied."));
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Valid command category";
	}
}