using System.Linq;
using System.Reflection;
using System.Threading;

using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;

namespace Advobot.Services.GuildSettings.UserInformation
{
	/// <summary>
	/// Holds a user and the counts of which punishments they should get.
	/// </summary>
	public sealed class BannedPhraseUserInfo : UserInfo
	{
		private static readonly PropertyInfo[] _Properties = typeof(BannedPhraseUserInfo).GetProperties();

		private int _Ban;
		private int _Deafen;
		private int _Kick;
		private int _RoleMute;
		private int _Softban;
		private int _VoiceMute;

		/// <summary>
		/// The amount of messages that gave them a ban punishment.
		/// </summary>
		public int Ban => _Ban;

		/// <summary>
		/// The amount of messages that gave them a deafen punishment.
		/// </summary>
		public int Deafen => _Deafen;

		/// <summary>
		/// The amount of messages that gave them a kick punishment.
		/// </summary>
		public int Kick => _Kick;

		/// <summary>
		/// The amount of messages that gave them a role mute punishment.
		/// </summary>
		public int RoleMute => _RoleMute;

		/// <summary>
		/// The amount of messages that gave them a soft ban punishment.
		/// </summary>
		public int Softban => _Softban;

		/// <summary>
		/// The amount of messages that gave them a voice mute punishment.
		/// </summary>
		public int VoiceMute => _VoiceMute;

		/// <summary>
		/// Returns the value of the specified punishment.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public int this[Punishment type] => type switch
		{
			Punishment.Kick => _Kick,
			Punishment.Ban => _Ban,
			Punishment.Deafen => _Deafen,
			Punishment.VoiceMute => _VoiceMute,
			Punishment.Softban => _Softban,
			Punishment.RoleMute => _RoleMute,
			_ => -1,
		};

		/// <summary>
		/// Creates an instance of bannedphraseuserinfo.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="user"></param>
		public BannedPhraseUserInfo(ITime time, IGuildUser user) : base(time, user) { }

		/// <summary>
		/// Increases the banned phrase count for that punishment by one.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public int Increment(Punishment type) => type switch
		{
			Punishment.Kick => Interlocked.Increment(ref _Kick),
			Punishment.Ban => Interlocked.Increment(ref _Ban),
			Punishment.Deafen => Interlocked.Increment(ref _Deafen),
			Punishment.VoiceMute => Interlocked.Increment(ref _VoiceMute),
			Punishment.Softban => Interlocked.Increment(ref _Softban),
			Punishment.RoleMute => Interlocked.Increment(ref _RoleMute),
			_ => -1,
		};

		/// <summary>
		/// Sets the banned phrase count for that punishment back to zero.
		/// </summary>
		/// <param name="type"></param>
		public int Reset(Punishment type) => type switch
		{
			Punishment.Kick => Interlocked.Exchange(ref _Kick, 0),
			Punishment.Ban => Interlocked.Exchange(ref _Ban, 0),
			Punishment.Deafen => Interlocked.Exchange(ref _Deafen, 0),
			Punishment.VoiceMute => Interlocked.Exchange(ref _VoiceMute, 0),
			Punishment.Softban => Interlocked.Exchange(ref _Softban, 0),
			Punishment.RoleMute => Interlocked.Exchange(ref _RoleMute, 0),
			_ => -1,
		};

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
		/// Returns the count of each punishment type.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> _Properties.Join(x => $"{x.Name[0]}{x.GetValue(this)}", "/");
	}
}