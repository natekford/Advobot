using Advobot.Core.Utilities;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	public class IconResizer : ImageResizer<IconResizerArguments>
	{
		public string IconType { get; }

		public IconResizer(string iconType, int threads) : base(threads)
		{
			IconType = iconType.ToLower();
		}

		protected override async Task Create(AdvobotSocketCommandContext context, IconResizerArguments args, Uri uri, RequestOptions options, string nameForEmote)
		{
			using (var resp = await ImageUtils.ResizeImageAsync(uri, context, args).CAF())
			{
				if (resp.IsSuccess)
				{
					await context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(resp.Stream), options).CAF();
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(context, $"Successfully updated the {IconType} icon.").CAF();
					return;
				}
				await MessageUtils.SendErrorMessageAsync(context, new Error($"Failed to update the {IconType} icon. Reason: {resp.Error}.")).CAF();
			}
		}
	}

}
