using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord.WebSocket;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds a user and the counts of which punishments they should get.
	/// </summary>
	public class BannedPhraseUserInfo : UserInfo
	{
		private Dictionary<PunishmentType, StrongBox<int>> _Values = Enum.GetValues(typeof(PunishmentType)).Cast<PunishmentType>()
			.ToDictionary(x => x, x => new StrongBox<int>(0));

		public BannedPhraseUserInfo(SocketGuildUser user) : base(user) { }

		public int this[PunishmentType type]
		{
			get => _Values[type].Value;
		}
		public int IncrementValue(PunishmentType type)
		{
			return Interlocked.Increment(ref _Values[type].Value);
		}
		public void ResetValue(PunishmentType type)
		{
			Interlocked.Exchange(ref _Values[type].Value, 0);
		}

		public override string ToString()
		{
			return String.Join("/", _Values.Select(x => $"{x.Value}{x.Key.ToString()[0]}"));
		}
	}
}