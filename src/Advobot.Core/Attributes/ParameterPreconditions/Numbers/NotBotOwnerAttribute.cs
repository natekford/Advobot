using System;
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
		public override string Summary => "Not the bot owner";

		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is ulong num))
			{
				return this.FromOnlySupports(typeof(ulong));
			}

			var application = await context.Client.GetApplicationInfoAsync().CAF();
			if (application.Owner.Id != num)
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError("You can't use the bot owner as an argument.");
		}
	}
}