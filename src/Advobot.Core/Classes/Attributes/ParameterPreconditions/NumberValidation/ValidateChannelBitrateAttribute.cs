using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation
{
	/// <summary>
	/// Validates the channel bitrate allowing 8 to 96 unless the guild is partnered in which the maximum is raised to 128.
	/// </summary>
	public class ValidateChannelBitrateAttribute : ValidateNumberAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateChannelBitrateAttribute"/>.
		/// </summary>
		public ValidateChannelBitrateAttribute() : base(8, 96) { }

		/// <inheritdoc />
		public override int GetEnd(ICommandContext context)
			=> context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) ? 128 : base.GetEnd(context);
	}
}