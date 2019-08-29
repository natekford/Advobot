using System;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.Numbers
{
	/// <summary>
	/// Validates the channel bitrate allowing 8 to 96 unless the guild is partnered or has a premium tier in which the maximum is raised to 128.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class ChannelBitrateAttribute : IntParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string NumberType => "channel bitrate";

		/// <summary>
		/// Creates an instance of <see cref="ChannelBitrateAttribute"/>.
		/// </summary>
		public ChannelBitrateAttribute() : base(8, 96) { }

		/// <inheritdoc />
		protected override NumberCollection<int> GetNumbers(
			ICommandContext context,
			ParameterInfo parameter,
			IServiceProvider services)
		{
			var end = context.Guild.PremiumTier switch
			{
				PremiumTier.Tier3 => 384,
				PremiumTier.Tier2 => 256,
				PremiumTier.Tier1 => 128,
				_ when context.Guild.Features.CaseInsContains("VIP_REGIONS") => 128,
				_ => 96,
			};
			return new NumberCollection<int>(8, end);
		}
	}
}