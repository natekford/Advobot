using Advobot.Core.Utilities;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	public class EmoteResizer : ImageResizer<EmoteResizerArguments>
	{
		public EmoteResizer(int threads) : base(threads) { }

		protected override async Task Create(AdvobotSocketCommandContext context, EmoteResizerArguments args, Uri uri, RequestOptions options, string nameForEmote)
		{
			using (var resp = await ImageUtils.ResizeImageAsync(uri, context, args).CAF())
			{
				if (resp.IsSuccess)
				{
					var emote = await context.Guild.CreateEmoteAsync(nameForEmote, new Image(resp.Stream), default, options).CAF();
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(context, $"Successfully created the emote {emote}.").CAF();
					return;
				}
				await MessageUtils.SendErrorMessageAsync(context, new Error($"Failed to create the emote `{nameForEmote}`. Reason: {resp.Error}.")).CAF();
			}
		}
	}
}
