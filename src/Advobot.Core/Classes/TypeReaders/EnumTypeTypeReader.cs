using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
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
			.Distinct().OrderBy(x => x.Name)
			.ToImmutableArray();

		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var matchingNames = Enums.Where(x => x.Name.CaseInsEquals(input)).ToArray();
			if (matchingNames.Length == 1)
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(matchingNames[0]));
			}
			if (matchingNames.Length > 1)
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.MultipleMatches, $"Too many enums have the name `{input}`."));
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"No enum has the name `{input}`."));
		}
	}
}