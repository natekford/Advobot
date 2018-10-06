using System;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Gets a voice region from a string.
	/// </summary>
	[TypeReaderTargetType(typeof(IVoiceRegion))]
	public class VoiceRegionTypeReader : TypeReader
	{
		/// <inheritdoc />
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var regions = await context.Guild.GetVoiceRegionsAsync().CAF();
			return regions.TryGetSingle(x => x.Name.CaseInsEquals(input), out var region)
				? TypeReaderResult.FromSuccess(region)
				: TypeReaderResult.FromError(CommandError.ParseFailed, "Unable to find a matching voice region.");
		}
	}
}