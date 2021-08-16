#define DEBUG_MESSAGES
#undef DEBUG_MESSAGES

using System;
using System.Collections.Generic;

using Advobot.AutoMod;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.Utils;
using Advobot.Punishments;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.Utilities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Discord.MentionUtils;

namespace Advobot.Tests.Commands.AutoMod.Service
{
	[TestClass]
	public sealed class SpamPrevention_Tests
	{
		private readonly FakeTextChannel _Channel;
		private readonly FakeGuild _Guild;
		private readonly SpamPrevention _Prevention = new()
		{
			SpamType = SpamType.Mention,
			Size = 2,
			Instances = 5,
			PunishmentType = PunishmentType.Kick,
			IntervalTicks = TimeSpan.FromSeconds(15).Ticks,
			Enabled = true,
		};
		private readonly SnowflakeGenerator _Snowflakes = new(TimeSpan.FromMilliseconds(200));
		private readonly FakeGuildUser _User;

		public SpamPrevention_Tests()
		{
			_Guild = new(new());
			_Channel = new(_Guild);
			_User = new(_Guild)
			{
				Id = 172138437246320640,
			};
		}

		[TestMethod]
		public void DontPunishNotSpam_Test()
		{
			for (var i = 0; i < 10; ++i)
			{
				var msg = new FakeUserMessage(_Channel, _User, GenerateNotSpam())
				{
					Id = _Snowflakes.Next(_Prevention.Interval / 10),
				};
				if (_Prevention.IsSpam(msg))
				{
					Assert.Fail("This should not have been caught as spam.");
				}
			}
		}

		[TestMethod]
		public void DontPunishSlowSpam_Test()
		{
			var spam = new List<ulong>();
			for (var i = 0; i < 10; ++i)
			{
				var msg = new FakeUserMessage(_Channel, _User, GenerateMentionSpam())
				{
					Id = _Snowflakes.Next(_Prevention.Interval * 10),
				};
				if (_Prevention.IsSpam(msg))
				{
					spam.Add(msg.Id);
				}
				if (_Prevention.ShouldPunish(spam))
				{
					Assert.Fail("This should not have been caught as spam.");
				}
			}
		}

		[TestMethod]
		public void DontPunishSporadicSpam_Test()
		{
			var timeMults = new[]
			{
				1.0,
				1.0,
				2.0,
				2.0,
				1.0,
				3.0,
				1.0,
				1.0,
				1.0,
				4.0,
				2.0,
			};

			var spam = new List<ulong>();
			foreach (var timeMult in timeMults)
			{
				var normalizedTimeMult = timeMult / (_Prevention.Instances + 1);
				var msg = new FakeUserMessage(_Channel, _User, GenerateMentionSpam())
				{
					Id = _Snowflakes.Next(_Prevention.Interval * normalizedTimeMult, false),
				};
				if (_Prevention.IsSpam(msg))
				{
					spam.Add(msg.Id);
				}
				if (_Prevention.ShouldPunish(spam))
				{
					Assert.Fail("This should not have been caught as spam.");
				}
			}
		}

		[TestMethod]
		public void PunishSpam_Test()
		{
			var spam = new List<ulong>();
			for (var i = 0; i < 10; ++i)
			{
				var msg = new FakeUserMessage(_Channel, _User, GenerateMentionSpam())
				{
					Id = _Snowflakes.Next(_Prevention.Interval / 10),
				};
				if (_Prevention.IsSpam(msg))
				{
					spam.Add(msg.Id);
				}
				if (_Prevention.ShouldPunish(spam))
				{
					return;
				}
			}
			Assert.Fail("This should have been caught as spam.");
		}

		private string GenerateMentionSpam()
			=> $"{MentionUser(1)} {MentionUser(2)} {MentionUser(3)}";

		private string GenerateNotSpam()
			=> "Not spam.";
	}
}