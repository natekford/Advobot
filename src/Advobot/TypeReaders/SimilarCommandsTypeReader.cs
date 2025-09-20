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
		var found = new SimilarCommands(commands.Commands)
			.FindSimilar(joined)
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

	private sealed class SimilarCommands(IEnumerable<IImmutableCommand> source)
		: Similar<IImmutableCommand>(source)
	{
		protected override Similarity<IImmutableCommand> FindSimilarity(
			string search,
			IImmutableCommand item)
		{
			var name = "";
			var distance = int.MaxValue;
			foreach (var path in item.Paths.Select(x => x.Join(" ")))
			{
				var cDistance = FindCloseness(path, search, Threshold);
				if (distance > cDistance)
				{
					name = path;
					distance = cDistance;
				}
			}
			return new(name, search, distance, item);
		}
	}
}