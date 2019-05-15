using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Finds a role based on position.
	/// </summary>
	public class RolePositionTypeReader : PositionTypeReader<IRole>
	{
		/// <inheritdoc />
		public override string ObjectType => "role";

		/// <inheritdoc />
		public override Task<IReadOnlyCollection<IRole>> GetObjectsWithPosition(ICommandContext context, int position)
		{
			var roles = context.Guild.Roles.Where(x => x.Position == position).ToArray();
			return Task.FromResult<IReadOnlyCollection<IRole>>(roles);
		}
	}
}