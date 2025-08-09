using Advobot.Attributes;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders;

/// <summary>
/// Gets a voice region from a string.
/// </summary>
[TypeReaderTargetType(typeof(IVoiceRegion))]
public class VoiceRegionTypeReader : TypeReader
{
	/// <inheritdoc />
	public override async Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var regions = await context.Guild.GetVoiceRegionsAsync().ConfigureAwait(false);
		var matches = regions.Where(x => x.Name.CaseInsEquals(input)).ToArray();
		return TypeReaderUtils.SingleValidResult(matches, "voice regions", input);
	}
}