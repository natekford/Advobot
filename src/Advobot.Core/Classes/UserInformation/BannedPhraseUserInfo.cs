using Advobot.Core.Enums;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds a user and the counts of which punishments they should get.
	/// </summary>
	public class BannedPhraseUserInfo : UserInfo
	{
		/// <summary>
		/// The amount of messages that gave them a kick punishment.
		/// </summary>
		public int Kick => _Kick;
		/// <summary>
		/// The amount of messages that gave them a ban punishment.
		/// </summary>
		public int Ban => _Ban;
		/// <summary>
		/// The amount of messages that gave them a deafen punishment.
		/// </summary>
		public int Deafen => _Deafen;
		/// <summary>
		/// The amount of messages that gave them a voice mute punishment.
		/// </summary>
		public int VoiceMute => _VoiceMute;
		/// <summary>
		/// The amount of messages that gave them a soft ban punishment.
		/// </summary>
		public int Softban => _Softban;
		/// <summary>
		/// The amount of messages that gave them a role mute punishment.
		/// </summary>
		public int RoleMute => _RoleMute;

		private int _Kick;
		private int _Ban;
		private int _Deafen;
		private int _VoiceMute;
		private int _Softban;
		private int _RoleMute;

		/// <summary>
		/// Creates an instance of bannedphraseuserinfo.
		/// </summary>
		/// <param name="user"></param>
		public BannedPhraseUserInfo(SocketGuildUser user) : base(user) { }

		/// <summary>
		/// Increases the banned phrase count for that punishment by one.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public int Increment(Punishment type)
		{
			switch (type)
			{
				case Punishment.Kick:
					return Interlocked.Increment(ref _Kick);
				case Punishment.Ban:
					return Interlocked.Increment(ref _Ban);
				case Punishment.Deafen:
					return Interlocked.Increment(ref _Deafen);
				case Punishment.VoiceMute:
					return Interlocked.Increment(ref _VoiceMute);
				case Punishment.Softban:
					return Interlocked.Increment(ref _Softban);
				case Punishment.RoleMute:
					return Interlocked.Increment(ref _RoleMute);
				default:
					throw new ArgumentException("Invalid punishment type provided.", nameof(type));
			}
		}
		/// <summary>
		/// Sets the banned phrase count for that punishment back to zero.
		/// </summary>
		/// <param name="type"></param>
		public void Reset(Punishment type)
		{
			switch (type)
			{
				case Punishment.Kick:
					Interlocked.Exchange(ref _Kick, 0);
					return;
				case Punishment.Ban:
					Interlocked.Exchange(ref _Ban, 0);
					return;
				case Punishment.Deafen:
					Interlocked.Exchange(ref _Deafen, 0);
					return;
				case Punishment.VoiceMute:
					Interlocked.Exchange(ref _VoiceMute, 0);
					return;
				case Punishment.Softban:
					Interlocked.Exchange(ref _Softban, 0);
					return;
				case Punishment.RoleMute:
					Interlocked.Exchange(ref _RoleMute, 0);
					return;
				default:
					throw new ArgumentException("Invalid punishment type provided.", nameof(type));
			}
		}
		/// <inheritdoc />
		public override void Reset()
		{
			Interlocked.Exchange(ref _Kick, 0);
			Interlocked.Exchange(ref _Ban, 0);
			Interlocked.Exchange(ref _Deafen, 0);
			Interlocked.Exchange(ref _VoiceMute, 0);
			Interlocked.Exchange(ref _Softban, 0);
			Interlocked.Exchange(ref _RoleMute, 0);
		}

		/// <summary>
		/// Returns the value of the specified punishment.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public int this[Punishment type]
		{
			get
			{
				switch (type)
				{
					case Punishment.Kick:
						return _Kick;
					case Punishment.Ban:
						return _Ban;
					case Punishment.Deafen:
						return _Deafen;
					case Punishment.VoiceMute:
						return _VoiceMute;
					case Punishment.Softban:
						return _Softban;
					case Punishment.RoleMute:
						return _RoleMute;
					default:
						throw new ArgumentException("Invalid punishment type provided.", nameof(type));
				}
			}
		}

		/// <summary>
		/// Returns the count of each punishment type.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Join("/", GetType().GetProperties().Select(x => $"{x.Name[0]}{x.GetValue(this)}"));
		}
	}
}