using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Finds a role based on position.
	/// </summary>
	public class RolePositionTypeReader : PositionTypeReader<IRole>
	{
		/// <inheritdoc />
		public override string ObjectTypeName => "roles";

		/// <inheritdoc />
		protected override Task<IEnumerable<IRole>> GetObjectsWithPositionAsync(
			ICommandContext context,
			int position)
			=> Task.FromResult(context.Guild.Roles.Where(x => x.Position == position));
	}
}