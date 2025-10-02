using Advobot.Services.Commands;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class CloseHelpEntryTypeReader_Tests
	: TypeReader_Tests<SimilarCommandsTypeReader>
{
	protected override SimilarCommandsTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task Valid_Test()
	{
		var commands = Context.Services.GetRequiredService<AdvobotCommandService>();
		foreach (var name in new[]
		{
			"dog",
			"bog",
			"pneumonoultramicroscopicsilicovolcanoconiosis"
		})
		{
			commands.Commands.Add(new DelegateCommand(() => { }, [[name]]).ToImmutable());
		}

		var result = await ReadAsync("hog").ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<IReadOnlyList<SimilarCommands>>(result.Value);
		Assert.HasCount(2, (IReadOnlyList<SimilarCommands>)result.Value);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services.AddSingleton<AdvobotCommandService>()
			.AddSingleton<CommandService>(x => x.GetRequiredService<AdvobotCommandService>())
			.AddSingleton<ICommandService>(x => x.GetRequiredService<AdvobotCommandService>());
	}
}