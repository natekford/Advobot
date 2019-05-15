﻿#define DEBUG_MESSAGES
#undef DEBUG_MESSAGES

using Advobot.Classes.Settings;
using Advobot.Enums;
using Advobot.Tests.Mocks;
using Advobot.Tests.Utilities;
using AdvorangesUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using static Discord.MentionUtils;

namespace Advobot.Tests
{
	[TestClass]
	public sealed class SpamPreventionTests
	{
		private readonly MockGuild _Guild = new MockGuild();
		private readonly MockGuildUser _GuildUser;
		private readonly SnowflakeGenerator _Snowflakes = new SnowflakeGenerator(TimeSpan.FromMilliseconds(200));
		private readonly SpamPrev _SpamPrev1 = new SpamPrev
		{
			Type = SpamType.Mention,
			SpamPerMessage = 2,
			SpamInstances = 5,
			Punishment = Punishment.Kick,
			TimeInterval = TimeSpan.FromSeconds(15),
		};

		public SpamPreventionTests()
		{
			_GuildUser = new MockGuildUser(_Guild, 172138437246320640);
		}

		[TestMethod]
		public async Task PunishSpam_Test()
		{
			await _SpamPrev1.EnableAsync(_Guild).CAF();

			for (var i = 0; i < 10; ++i)
			{
				var id = _Snowflakes.Next(_SpamPrev1.TimeInterval / 10);
				var mockUserMessage = new MockUserMessage(_GuildUser, GenerateMentionSpam(), id);
				var punished = await _SpamPrev1.PunishAsync(_GuildUser, mockUserMessage).CAF();
				if (punished)
				{
					return;
				}
			}
			Assert.Fail("This should have been caught as spam.");
		}
		[TestMethod]
		public async Task DontPunishNotSpam_Test()
		{
			await _SpamPrev1.EnableAsync(_Guild).CAF();

			for (var i = 0; i < 10; ++i)
			{
				var id = _Snowflakes.Next(_SpamPrev1.TimeInterval / 10);
				var mockUserMessage = new MockUserMessage(_GuildUser, GenerateNotSpam(), id);
				var punished = await _SpamPrev1.PunishAsync(_GuildUser, mockUserMessage).CAF();
				if (punished)
				{
					Assert.Fail("This should not have been caught as spam.");
				}
			}
		}
		[TestMethod]
		public async Task DontPunishSlowSpam_Test()
		{
			await _SpamPrev1.EnableAsync(_Guild).CAF();

			for (var i = 0; i < 10; ++i)
			{
				var id = _Snowflakes.Next(_SpamPrev1.TimeInterval * 10);
				var mockUserMessage = new MockUserMessage(_GuildUser, GenerateMentionSpam(), id);
				var punished = await _SpamPrev1.PunishAsync(_GuildUser, mockUserMessage).CAF();
				if (punished)
				{
					Assert.Fail("This should not have been caught as spam.");
				}
			}
		}
		[TestMethod]
		public async Task DontPunishSporadicSpam_Test()
		{
			await _SpamPrev1.EnableAsync(_Guild).CAF();

			for (var i = 0; i < 10; ++i)
			{
				var timeMult = i switch
				{
					0 => 1.0,
					1 => 1.0,
					2 => 2.0,
					3 => 2.0,
					4 => 1.0,
					5 => 3.0,
					6 => 1.0,
					7 => 1.0,
					8 => 1.0,
					9 => 4.0,
					_ => 2.0,
				} / (_SpamPrev1.SpamInstances + 1);

				var id = _Snowflakes.Next(_SpamPrev1.TimeInterval * timeMult, false);
				var mockUserMessage = new MockUserMessage(_GuildUser, GenerateMentionSpam(), id);
				var punished = await _SpamPrev1.PunishAsync(_GuildUser, mockUserMessage).CAF();
				if (punished)
				{
					Assert.Fail("This should not have been caught as spam.");
				}
			}
		}
		[TestMethod]
		public async Task StressSpam_Test()
		{
			await _SpamPrev1.EnableAsync(_Guild).CAF();

			var tasks = new Task[100];
			for (var i = 0; i < tasks.Length; ++i)
			{
				tasks[i] = new Task(async userId =>
				{
					var threadUser = new MockGuildUser(_Guild, (ulong)(int)userId);
#if DEBUG_MESSAGES
					Debug.Print($"Created user {userId}.\n");
#endif

					for (var i = 0; i < 1000; ++i)
					{
						var id = _Snowflakes.Next();
						var mockUserMessage = new MockUserMessage(threadUser, GenerateMentionSpam(), id);
						await _SpamPrev1.PunishAsync(_GuildUser, mockUserMessage).CAF();
					}
				}, i);
			}
			foreach (var task in tasks)
			{
				task.Start();
			}
			await Task.WhenAll(tasks).CAF();
		}

		private string GenerateMentionSpam()
			=> $"{MentionUser(1)} {MentionUser(2)} {MentionUser(3)}";
		private string GenerateNotSpam()
			=> "Not spam.";
	}
}
