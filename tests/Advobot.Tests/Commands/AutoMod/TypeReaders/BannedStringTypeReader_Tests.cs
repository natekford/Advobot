﻿using Advobot.AutoMod.TypeReaders;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders.BannedPhraseTypeReaders;

[TestClass]
public sealed class BannedStringTypeReader_Tests
	: BannedPhraseTypeReader_Tests<BannedStringTypeReader>
{
	protected override BannedStringTypeReader Instance { get; } = new();
	protected override bool IsName => false;
	protected override bool IsRegex => false;
	protected override bool IsString => true;
}