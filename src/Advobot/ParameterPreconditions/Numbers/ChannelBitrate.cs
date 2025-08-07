using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.Numbers;

/// <summary>
/// Validates the channel bitrate allowing 8 to 96 unless the guild is partnered or has a premium tier in which the maximum is raised to 128.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class ChannelBitrate : NumberParameterPrecondition
{
	/// <inheritdoc />
	public override string NumberType => "channel bitrate";

	/// <summary>
	/// Creates an instance of <see cref="ChannelBitrate"/>.
	/// </summary>
	public ChannelBitrate() : base(8, 96) { }

	/// <inheritdoc />
	protected override ValidateNumber<int> GetRange(
		ICommandContext context,
		ParameterInfo parameter,
		IServiceProvider services)
	{
		var end = context.Guild.PremiumTier switch
		{
			PremiumTier.Tier3 => 384,
			PremiumTier.Tier2 => 256,
			PremiumTier.Tier1 => 128,
			// this probably lets you go to 384 at this point but idk
			_ when (context.Guild.Features.Value & GuildFeature.VIPRegions) != 0 => 128,
			_ => 96,
		};
		return new(8, end);
	}
}