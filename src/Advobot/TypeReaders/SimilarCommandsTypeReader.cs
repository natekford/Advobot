using Advobot.Similar;
using Advobot.Utilities;

using Microsoft.Extensions.DependencyInjection;

using MorseCode.ITask;

using System.Collections.Concurrent;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace Advobot.TypeReaders;

/// <summary>
/// Finds help entries with names or aliases similar to the passed in input.
/// </summary>
public sealed class SimilarCommandsTypeReader
	: TypeReader<IReadOnlyList<SimilarCommands>>
{
	/// <inheritdoc />
	public override ITask<ITypeReaderResult<IReadOnlyList<SimilarCommands>>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
	{
		var commands = GetCommands(context.Services);
		var joined = Join(context, input);

		var dict = new ConcurrentDictionary<string, List<IImmutableCommand>>(1, commands.Commands.Count * 2);
		foreach (var command in commands.Commands)
		{
			foreach (var path in command.Paths.Select(x => x.Join(" ")))
			{
				dict.GetOrAdd(path, _ => []).Add(command);
			}
		}
		var found = Similarity<KeyValuePair<string, List<IImmutableCommand>>>.Get(
			source: dict,
			search: joined,
			getName: x => x.Key
		)
		.Select(x => new SimilarCommands(x.Value.Key, x.Value.Value))
		.ToArray();

		if (found.Length == 0)
		{
			return TypeReaderResult<IReadOnlyList<SimilarCommands>>.ParseFailed.Task;
		}
		return Success(found).AsITask();
	}

	[GetServiceMethod]
	private static ICommandService GetCommands(IServiceProvider services)
		=> services.GetRequiredService<ICommandService>();
}

/// <summary>
/// Holds the path these commands were found from.
/// </summary>
/// <param name="Path"></param>
/// <param name="Commands"></param>
public sealed record SimilarCommands(
	string Path,
	IReadOnlyList<IImmutableCommand> Commands
);