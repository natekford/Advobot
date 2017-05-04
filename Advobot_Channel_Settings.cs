using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Channel_Settings")]
	public class Advobot_Channel_Settings : ModuleBase
	{
		[Command("onlyimages")]
		[Alias("oi")]
		[Usage("<#Channel>")]
		[Summary("Makes the bot delete any message sent on a channel which is not an image or has an embed. No input channel means it applies to the current channel.")]
		[UserHasAPermission]
		[DefaultEnabled(false)]
		public async Task ImagesOnly([Optional, Remainder] string input)
		{
			
		}
	}
}
