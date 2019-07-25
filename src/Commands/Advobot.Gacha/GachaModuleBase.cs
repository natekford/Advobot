using Advobot.Classes.Modules;
using Advobot.Gacha.Database;
using Advobot.Gacha.Displays;
using Advobot.Gacha.Models;
using Advobot.Utilities;
using AdvorangesUtils;
using System.Threading.Tasks;

namespace Advobot.Gacha
{
	public abstract class GachaModuleBase : AdvobotModuleBase
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public GachaDatabase Database { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

		protected async Task<CharacterDisplay> CreateCharacterDisplayAsync(Character character, bool fireAndForget = true)
		{
			var marriage = await Database.GetMarriageAsync(Context.Guild, character).CAF();
			var metadata = await Database.GetCharacterMetadataAsync(character).CAF();
			var display = new CharacterDisplay(Context.Client, Database, metadata, marriage);
			return await FireAndForget(display, fireAndForget).CAF();
		}
		protected async Task<HaremDisplay> CreateHaremDisplayAsync(User user, bool fireAndForget = true)
		{
			var marriages = await Database.GetClaimsAsync(user).CAF();
			var display = new HaremDisplay(Context.Client, Database, marriages);
			return await FireAndForget(display, fireAndForget).CAF();
		}
		protected async Task<RollDisplay> CreateRollDisplayAsync(bool fireAndForget = true)
		{
			var character = await Database.GetRandomCharacterAsync(Context.Guild).CAF();
			var wishes = await Database.GetWishesAsync(Context.Guild, character).CAF();
			var display = new RollDisplay(Context.Client, Database, character, wishes);
			return await FireAndForget(display, fireAndForget).CAF();
		}
		protected async Task<SourceDisplay> CreateSourceDisplayAsync(Source source, bool fireAndForget = true)
		{
			var display = new SourceDisplay(Context.Client, Database, source);
			return await FireAndForget(display, fireAndForget).CAF();
		}
		private async Task<T> FireAndForget<T>(T display, bool fireAndForget) where T : Display
		{
			if (fireAndForget)
			{
				await display.SendAsync(Context.Channel).CAF();
			}
			return display;
		}
	}
}
