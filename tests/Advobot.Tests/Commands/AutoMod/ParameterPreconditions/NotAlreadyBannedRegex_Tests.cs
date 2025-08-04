using Advobot.AutoMod.ParameterPreconditions;

namespace Advobot.Tests.Commands.AutoMod.ParameterPreconditions;

[TestClass]
public sealed class NotAlreadyBannedRegex_Tests
	: NotAlreadyBannedPhrase_Tests<NotAlreadyBannedRegex>
{
	protected override NotAlreadyBannedRegex Instance { get; } = new();
	protected override bool IsName => false;
	protected override bool IsRegex => true;
	protected override bool IsString => false;
}