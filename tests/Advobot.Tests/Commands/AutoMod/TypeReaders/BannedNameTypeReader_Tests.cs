using Advobot.AutoMod.TypeReaders;

namespace Advobot.Tests.Core.TypeReaders.BannedPhraseTypeReaders;

[TestClass]
public sealed class BannedNameTypeReader_Tests
	: BannedPhraseTypeReader_Tests<BannedNameTypeReader>
{
	protected override BannedNameTypeReader Instance { get; } = new();
	protected override bool IsName => true;
	protected override bool IsRegex => false;
	protected override bool IsString => false;
}