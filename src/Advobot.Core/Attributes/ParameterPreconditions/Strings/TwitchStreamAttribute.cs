using System;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the Twitch stream name by making sure it is between 4 and 25 characters and matches a Regex for Twitch usernames.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class TwitchStreamAttribute : StringParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string StringType => "Twitch stream name";

		/// <summary>
		/// Creates an instance of <see cref="TwitchStreamAttribute"/>.
		/// </summary>
		public TwitchStreamAttribute() : base(4, 25) { }

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

			if (RegexUtils.IsValidTwitchName(value))
			{
				return PreconditionUtils.FromSuccess();
			}
			return PreconditionUtils.FromError("Invalid Twitch username supplied.");
		}
	}
}
