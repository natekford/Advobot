using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.ParameterPreconditions.SettingValidation
{
	/// <summary>
	/// Makes sure the passed in number isn't currently being used for a self assignable roles group.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotAlreadySelfAssignableRoleGroupAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is int num))
			{
				throw new ArgumentException(nameof(value));
			}

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			if (!settings.SelfAssignableGroups.Any(x => x.Group == num))
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError($"The group number `{num}` is already being used.");
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Group number not already being used";
	}
}
