using Advobot.AutoMod.ParameterPreconditions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.ParameterPreconditions;

[TestClass]
public sealed class NotAlreadyBannedString_Tests
	: NotAlreadyBannedPhrase_Tests<NotAlreadyBannedString>
{
	protected override NotAlreadyBannedString Instance { get; } = new();
	protected override bool IsName => false;
	protected override bool IsRegex => false;
	protected override bool IsString => true;
}