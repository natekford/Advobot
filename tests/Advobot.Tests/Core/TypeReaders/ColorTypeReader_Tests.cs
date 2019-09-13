using System;
using System.Threading.Tasks;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Utilities;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class ColorTypeReader_Tests
		: TypeReader_TestsBase<ColorTypeReader>
	{
		[TestMethod]
		public async Task Invalid_Test()
		{
			var result = await ReadAsync("asdf").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task ValidEmpty_Test()
		{
			var result = await ReadAsync(null).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
		}

		[TestMethod]
		public async Task ValidHex_Test()
		{
			var result = await ReadAsync(Color.Red.RawValue.ToString("X6")).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
		}

		[TestMethod]
		public async Task ValidName_Test()
		{
			var result = await ReadAsync("Red").CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
		}

		[TestMethod]
		public async Task ValidRGB_Test()
		{
			var result = await ReadAsync("100/100/100").CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(Color));
		}
	}

	[TestClass]
	public sealed class EnumTypeTypeReader_Tests
		: TypeReader_TestsBase<EnumTypeTypeReader>
	{
	}

	[TestClass]
	public sealed class InviteTypeReader_Tests
		: TypeReader_TestsBase<InviteTypeReader>
	{
	}

	[TestClass]
	public sealed class ModerationReasonTypeReader_Tests
		: TypeReader_TestsBase<ModerationReasonTypeReader>
	{
	}

	[TestClass]
	public sealed class PermissionsTypeReader_Tests
		: TypeReader_TestsBase<PermissionsTypeReader<ChannelPermission>>
	{
	}

	[TestClass]
	public sealed class SelfAssignableRolesTypeReader_Tests
		: TypeReader_TestsBase<SelfAssignableRolesTypeReader>
	{
	}

	[TestClass]
	public sealed class SelfAssignableRoleTypeReader_Tests
		: TypeReader_TestsBase<SelfAssignableRoleTypeReader>
	{
	}

	[TestClass]
	public sealed class VoiceRegionTypeReader_Tests
		: TypeReader_TestsBase<VoiceRegionTypeReader>
	{
	}

	[TestClass]
	public sealed class WebhookTypeReader_Tests
		: TypeReader_TestsBase<WebhookTypeReader>
	{
	}
}