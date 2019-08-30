using System;
using System.Threading.Tasks;

using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the text channel name by making sure it is between 2 and 100 characters and has no spaces.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class TextChannelNameAttribute : ChannelNameAttribute
	{
		/// <inheritdoc />
		public override string StringType => "text channel name with no spaces";

		/// <inheritdoc />
		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			string value,
			IServiceProvider services)
		{
			var result = await base.SingularCheckPermissionsAsync(context, parameter, value, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}

			if (!value.Contains(" "))
			{
				return PreconditionUtils.FromSuccess();
			}
			return PreconditionUtils.FromError("Spaces are not allowed in text channel names.");
		}
	}
}