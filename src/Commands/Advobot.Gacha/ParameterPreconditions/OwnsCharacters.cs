using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;
using Advobot.Gacha.Database;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.ParameterPreconditions
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class OwnsCharacters : AdvobotParameterPreconditionAttribute
	{
		public override string Summary => "Character is owned by the invoker";
		public override IEnumerable<Type> SupportedTypes { get; } = new[]
		{
			typeof(IReadOnlyCharacter),
		}.ToImmutableArray();

		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is IReadOnlyCharacter character))
			{
				return this.FromOnlySupports(value);
			}

			var db = services.GetRequiredService<IGachaDatabase>();
			var claim = await db.GetClaimAsync(context.Guild.Id, character).CAF();
			if (claim?.UserId == context.User.Id)
			{
				return this.FromSuccess();
			}
			return PreconditionResult.FromError("You do not currently own this character.");
		}
	}
}