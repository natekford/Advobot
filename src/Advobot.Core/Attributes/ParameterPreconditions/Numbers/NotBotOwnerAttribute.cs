using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.Numbers
{
	/// <summary>
	/// Makes sure the passed in number is not the owner.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotBotOwnerAttribute
		: AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary => "Not the bot owner";
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
			if (!(value is ulong num))
			{
				return this.FromOnlySupports(value);
			}

			var application = await context.Client.GetApplicationInfoAsync().CAF();
			if (application.Owner.Id != num)
			{
				return this.FromSuccess();
			}
			return PreconditionResult.FromError("You can't use the bot owner as an argument.");
		}
	}
}