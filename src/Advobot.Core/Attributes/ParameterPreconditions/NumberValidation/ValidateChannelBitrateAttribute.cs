using System;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.NumberValidation
{
	/// <summary>
	/// Validates the channel bitrate allowing 8 to 96 unless the guild is partnered or has a premium tier in which the maximum is raised to 128.
	/// </summary>
	public class ValidateChannelBitrateAttribute : ValidateNumberAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateChannelBitrateAttribute"/>.
		/// </summary>
		public ValidateChannelBitrateAttribute() : base(8, 96) { }

		/// <inheritdoc />
		public override int GetEnd(
			ICommandContext context,
			ParameterInfo parameter,
			IServiceProvider services)
		{
			return context.Guild.PremiumTier switch
			{
				PremiumTier.Tier3 => 384,
				PremiumTier.Tier2 => 256,
				PremiumTier.Tier1 => 128,
				_ when context.Guild.Features.CaseInsContains("VIP_REGIONS") => 128,
				_ => base.GetEnd(context, parameter, services),
			};
		}
	}
}