using Advobot.Punishments;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class ModerationReasonTypeReader_Tests
	: TypeReader_Tests<ModerationReasonTypeReader>
{
	protected override ModerationReasonTypeReader Instance { get; } = new();
	protected override string? NotExisting => null;

	[TestMethod]
	public async Task Valid_Test()
	{
		var result = await ReadAsync("asdf").ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<ModerationReason>(result.Value);
	}

	[TestMethod]
	public async Task ValidWithTime_Test()
	{
		var result = await ReadAsync("asdf time:5 kapow").ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<ModerationReason>(result.Value);
		var cast = (ModerationReason)result.Value;
		Assert.AreEqual(TimeSpan.FromMinutes(5), cast.Time);
	}
}