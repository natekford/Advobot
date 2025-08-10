using Advobot.Services.Help;
using Advobot.Utilities;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.TypeReaders;

/// <summary>
/// Finds a category from the help entry service.
/// </summary>
[TypeReaderTargetType(typeof(Category))]
public class CategoryTypeReader : TypeReader
{
	/// <inheritdoc />
	public override Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var helpEntries = services.GetRequiredService<IHelpService>();
		foreach (var category in helpEntries.GetCategories())
		{
			if (category.CaseInsEquals(input))
			{
				return TypeReaderResult.FromSuccess(new Category(category)).AsTask();
			}
		}
		return TypeReaderUtils.NotFoundResult("category", input).AsTask();
	}
}