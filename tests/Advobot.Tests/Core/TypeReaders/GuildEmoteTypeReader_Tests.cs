using System.Threading.Tasks;

using Advobot.Tests.Utilities;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class GuildEmoteTypeReader_Tests
		: TypeReader_TestsBase<GuildEmoteTypeReader>
	{
		private readonly GuildEmote _Emote;
		private readonly GuildEmote _NotFromThisGuildEmote;

		public GuildEmoteTypeReader_Tests()
		{
			_Emote = new EmoteCreationArgs
			{
				Id = 73UL,
				Name = "emote name",
			}.Build();
			_NotFromThisGuildEmote = new EmoteCreationArgs
			{
				Id = 69UL,
				Name = "not on this guild",
			}.Build();
			Context.Guild.Emotes.Add(_Emote);
		}

		[TestMethod]
		public async Task Invalid_Test()
		{
			var result = await ReadAsync("asdf").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvalidNotOnThisGuildId_Test()
		{
			var result = await ReadAsync(_NotFromThisGuildEmote.Id.ToString()).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvalidNotOnThisGuildName_Test()
		{
			var result = await ReadAsync(_NotFromThisGuildEmote.Name).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task ValidId_Test()
		{
			var result = await ReadAsync(_Emote.Id.ToString()).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(Emote));
		}

		[TestMethod]
		public async Task ValidName_Test()
		{
			var result = await ReadAsync(_Emote.Name).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(Emote));
		}
	}
}