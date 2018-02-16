using Advobot.Core.Enums;
using Discord.WebSocket;
using LiteDB;
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
		private int _Kick;
		private int _Ban;
		private int _Deafen;
		private int _VoiceMute;
		private int _Softban;
		private int _RoleMute;

		/// <summary>
		/// The id of the object for LiteDB.
		/// </summary>
		public ObjectId Id { get; set; }
		/// <summary>
		/// The amount of messages that gave them a kick punishment.
		/// </summary>
		public int Kick
		{
			get => _Kick;
			set => _Kick = value;
		}
		/// <summary>
		/// The amount of messages that gave them a ban punishment.
		/// </summary>
		public int Ban
		{
			get => _Ban;
			set => _Ban = value;
		}
		/// <summary>
		/// The amount of messages that gave them a deafen punishment.
		/// </summary>
		public int Deafen
		{
			get => _Deafen;
			set => _Deafen = value;
		}
		/// <summary>
		/// The amount of messages that gave them a voice mute punishment.
		/// </summary>
		public int VoiceMute
		{
			get => _VoiceMute;
			set => _VoiceMute = value;
		}
		/// <summary>
		/// The amount of messages that gave them a soft ban punishment.
		/// </summary>
		public int Softban
		{
			get => _Softban;
			set => _Softban = value;
		}
		/// <summary>
		/// The amount of messages that gave them a role mute punishment.
		/// </summary>
		public int RoleMute
		{
			get => _RoleMute;
			set => _RoleMute = value;
		}

		public BannedPhraseUserInfo() { }
		public BannedPhraseUserInfo(SocketGuildUser user) : base(user) { }

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
		public int IncrementValue(Punishment type)
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
		public int ResetValue(Punishment type)
		{
			switch (type)
			{
				case Punishment.Kick:
					return Interlocked.Exchange(ref _Kick, 0);
				case Punishment.Ban:
					return Interlocked.Exchange(ref _Ban, 0);
				case Punishment.Deafen:
					return Interlocked.Exchange(ref _Deafen, 0);
				case Punishment.VoiceMute:
					return Interlocked.Exchange(ref _VoiceMute, 0);
				case Punishment.Softban:
					return Interlocked.Exchange(ref _Softban, 0);
				case Punishment.RoleMute:
					return Interlocked.Exchange(ref _RoleMute, 0);
				default:
					throw new ArgumentException("Invalid punishment type provided.", nameof(type));
			}
		}

		public override string ToString()
		{
			return String.Join("/", GetType().GetProperties().Select(x => $"{x.Name[0]}{x.GetValue(this)}"));
		}
	}
}