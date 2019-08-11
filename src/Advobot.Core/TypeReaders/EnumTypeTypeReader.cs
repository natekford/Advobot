using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Parses an enum type from its name.
	/// </summary>
	public class EnumTypeTypeReader : TypeReader
	{
		/// <summary>
		/// All of the enums from the Discord API wrapper and the core of this bot.
		/// </summary>
		public static readonly ImmutableArray<Type> Enums = AppDomain.CurrentDomain.GetAssemblies()
			.Where(x => x.FullName.CaseInsContains("Discord") || x.FullName.CaseInsContains("Advobot"))
			.SelectMany(x => x.GetTypes())
			.Where(x => x.IsEnum && x.IsPublic)
			.Distinct()
			.ToImmutableArray();

		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			var matches = Enums.Where(x => x.Name.CaseInsEquals(input)).ToArray();
			return this.SingleValidResultAsync(matches, "enums", input);
		}
	}
}