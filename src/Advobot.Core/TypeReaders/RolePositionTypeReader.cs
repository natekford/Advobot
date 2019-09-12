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
		protected override Task<IReadOnlyList<IRole>> GetObjectsWithPositionAsync(
			ICommandContext context,
			int position)
		{
			var items = context.Guild.Roles.Where(x => x.Position == position).ToArray();
			return Task.FromResult<IReadOnlyList<IRole>>(items);
		}
	}
}