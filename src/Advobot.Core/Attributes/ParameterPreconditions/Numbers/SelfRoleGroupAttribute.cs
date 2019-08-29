using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.ParameterPreconditions.Numbers
{
	/// <summary>
	/// Makes sure the passed in number isn't currently being used for a self assignable roles group.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class SelfRoleGroupAttribute
		: PositiveAttribute, IExistenceParameterPrecondition
	{
		/// <inheritdoc />
		public override string NumberType => "self role group";
		/// <inheritdoc />
		public ExistenceStatus Status => ExistenceStatus.MustNotExist;

		/// <inheritdoc />
		public override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			int value,
			IServiceProvider services)
		{
			var result = await base.SingularCheckPermissionsAsync(context, parameter, value, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var exists = settings.SelfAssignableGroups.Any(x => x.Group == value);
			return this.FromExistence(exists, value, NumberType);
		}
	}
}
