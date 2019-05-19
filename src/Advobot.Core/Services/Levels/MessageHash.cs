using System;
using System.Security.Cryptography;
using System.Text;
using Discord;

namespace Advobot.Services.Levels
{
	/// <summary>
	/// Holds the id of the message and the hash of the message's content.
	/// </summary>
	internal sealed class MessageHash
	{
		/// <summary>
		/// The id of the guild the message was sent on.
		/// </summary>
		public ulong GuildId { get; set; }
		/// <summary>
		/// The id of the channel the message was sent on.
		/// </summary>
		public ulong ChannelId { get; set; }
		/// <summary>
		/// The id of the message.
		/// </summary>
		public ulong MessageId { get; set; }
		/// <summary>
		/// The message's content hashed to prevent any sensitive information being in the database.
		/// </summary>
		public string Hash { get; set; } = "";
		/// <summary>
		/// The amount of experience given for the message.
		/// </summary>
		public int ExperienceGiven { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="MessageHash"/>.
		/// </summary>
		public MessageHash() { }
		/// <summary>
		/// Creates an instance of <see cref="MessageHash"/>.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="xp"></param>
		public MessageHash(IUserMessage message, int xp)
		{
			GuildId = ((ITextChannel)message.Channel).Guild.Id;
			ChannelId = message.Channel.Id;
			MessageId = message.Id;
			using (var md5 = MD5.Create())
			{
				Hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(message.Content))).Replace("-", "").ToLower();
			}
			ExperienceGiven = xp;
		}

		/// <summary>
		/// Returns the hash and message id.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> $"{Hash} ({MessageId})";
	}
}