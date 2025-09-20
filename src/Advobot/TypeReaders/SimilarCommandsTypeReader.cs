using Advobot.Similar;
using Advobot.Utilities;

using Microsoft.Extensions.DependencyInjection;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace Advobot.TypeReaders;

/// <summary>
/// Finds help entries with names or aliases similar to the passed in input.
/// </summary>
public sealed class SimilarCommandsTypeReader
	: TypeReader<IReadOnlyList<IImmutableCommand>>
{
	/// <inheritdoc />
	public override ITask<ITypeReaderResult<IReadOnlyList<IImmutableCommand>>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
	{
		var commands = GetCommands(context.Services);

		var joined = Join(context, input);
		var found = Similarity<IImmutableCommand>.Get(
			source: commands.Commands,
			search: joined,
			getNames: x => x.Paths.Select(x => x.Join(" "))
		)
		.Select(x => x.Value)
		.ToArray();

		if (found.Length == 0)
		{
			return TypeReaderResult<IReadOnlyList<IImmutableCommand>>.ParseFailed.Task;
		}
		return Success(found).AsITask();
	}

	[GetServiceMethod]
	private static ICommandService GetCommands(IServiceProvider services)
		=> services.GetRequiredService<ICommandService>();
}