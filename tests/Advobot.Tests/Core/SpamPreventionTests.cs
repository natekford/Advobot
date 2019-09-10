#define DEBUG_MESSAGES
#undef DEBUG_MESSAGES

using System;
using System.Threading.Tasks;

using Advobot.Services.GuildSettings.Settings;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.Utilities;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Discord.MentionUtils;

namespace Advobot.Tests.Core
{
	[TestClass]
	public sealed class SpamPreventionTests
	{
		private readonly FakeGuild _Guild;
		private readonly FakeTextChannel _Channel;
		private readonly FakeGuildUser _User;
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
			_Guild = new FakeGuild(new FakeClient());
			_Channel = new FakeTextChannel(_Guild);
			_User = new FakeGuildUser(_Guild)
			{
				Id = 172138437246320640,
			};
		}

		[TestMethod]
		public async Task PunishSpam_Test()
		{
			await _SpamPrev1.EnableAsync(_Guild).CAF();

			for (var i = 0; i < 10; ++i)
			{
				var msg = new FakeUserMessage(_Channel, _User, GenerateMentionSpam())
				{
					Id = _Snowflakes.Next(_SpamPrev1.TimeInterval / 10),
				};
				var punished = await _SpamPrev1.PunishAsync(msg).CAF();
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
				var msg = new FakeUserMessage(_Channel, _User, GenerateNotSpam())
				{
					Id = _Snowflakes.Next(_SpamPrev1.TimeInterval / 10),
				};
				var punished = await _SpamPrev1.PunishAsync(msg).CAF();
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
				var msg = new FakeUserMessage(_Channel, _User, GenerateMentionSpam())
				{
					Id = _Snowflakes.Next(_SpamPrev1.TimeInterval * 10),
				};
				var punished = await _SpamPrev1.PunishAsync(msg).CAF();
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

			foreach (var timeMult in timeMults)
			{
				var normalizedTimeMult = timeMult / (_SpamPrev1.SpamInstances + 1);
				var msg = new FakeUserMessage(_Channel, _User, GenerateMentionSpam())
				{
					Id = _Snowflakes.Next(_SpamPrev1.TimeInterval * normalizedTimeMult, false),
				};
				var punished = await _SpamPrev1.PunishAsync(msg).CAF();
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
					var threadUser = new FakeGuildUser(_Guild)
					{
						Id = (ulong)(int)userId,
					};
#if DEBUG_MESSAGES
					Debug.Print($"Created user {userId}.\n");
#endif

					for (var c = 0; c < 1000; ++c)
					{
						var msg = new FakeUserMessage(_Channel, threadUser, GenerateMentionSpam())
						{
							Id = _Snowflakes.Next(),
						};
						await _SpamPrev1.PunishAsync(msg).CAF();
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