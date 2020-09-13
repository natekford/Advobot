using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	/// <summary>
	/// Makes sure the passed in <see cref="ulong"/> is not already banned.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotBannedAttribute
		: AdvobotParameterPreconditionAttribute, IExistenceParameterPrecondition
	{
		/// <inheritdoc />
		public ExistenceStatus Status => ExistenceStatus.MustNotExist;
		/// <inheritdoc />
		public override string Summary => "not already banned";
		/// <inheritdoc />
		public override IEnumerable<Type> SupportedTypes { get; } = new[]
		{
			typeof(ulong),
		}.ToImmutableArray();

		/// <inheritdoc />
		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is ulong id))
			{
				return this.FromOnlySupports(value);
			}

			var bans = await context.Guild.GetBansAsync().CAF();
			var exists = bans.Select(x => x.User.Id).Contains(id);
			return this.FromExistence(exists, value, "ban");
		}
	}
}