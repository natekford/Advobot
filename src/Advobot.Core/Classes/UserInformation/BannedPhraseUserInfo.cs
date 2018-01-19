using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds a user and the counts of which punishments they should get.
	/// </summary>
	public class BannedPhraseUserInfo : UserInfo
	{
		private Dictionary<PunishmentType, int> _Values = Enum.GetValues(typeof(PunishmentType)).Cast<PunishmentType>().ToDictionary(x => x, x => 0);

		public BannedPhraseUserInfo(IGuildUser user) : base(user) { }

		public int this[PunishmentType type]
		{
			get => _Values[type];
		}
		public int IncrementValue(PunishmentType type)
		{
			return ++_Values[type];
		}
		public void ResetValue(PunishmentType type)
		{
			_Values[type] = 0;
		}

		public override string ToString()
		{
			return String.Join("/", _Values.Select(x => $"{x.Value}{x.Key.EnumName()[0]}"));
		}
	}
}