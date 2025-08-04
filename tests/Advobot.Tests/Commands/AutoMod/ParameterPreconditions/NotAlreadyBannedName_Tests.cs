using Advobot.AutoMod.ParameterPreconditions;

namespace Advobot.Tests.Commands.AutoMod.ParameterPreconditions;

[TestClass]
public sealed class NotAlreadyBannedName_Tests
	: NotAlreadyBannedPhrase_Tests<NotAlreadyBannedName>
{
	protected override NotAlreadyBannedName Instance { get; } = new();
	protected override bool IsName => true;
	protected override bool IsRegex => false;
	protected override bool IsString => false;
}