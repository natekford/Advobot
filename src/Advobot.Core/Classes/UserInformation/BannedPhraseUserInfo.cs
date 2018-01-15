using Advobot.Core.Utilities;
using Advobot.Core.Enums;
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
		private Dictionary<PunishmentType, int> _PunishmentVals = CreateDictionary();

		public BannedPhraseUserInfo(IGuildUser user) : base(user) { }

		public int this[PunishmentType value]
		{
			get => _PunishmentVals[value];
		}
		public int IncrementValue(PunishmentType value)
		{
			return ++_PunishmentVals[value];
		}
		public void ResetValue(PunishmentType value)
		{
			_PunishmentVals[value] = 0;
		}

		private static Dictionary<PunishmentType, int> CreateDictionary()
		{
			var temp = new Dictionary<PunishmentType, int>();
			foreach (PunishmentType type in Enum.GetValues(typeof(PunishmentType)))
			{
				temp.Add(type, 0);
			}
			return temp;
		}

		public override string ToString()
		{
			return String.Join("/", _PunishmentVals.Select(x => $"{x.Value}{x.Key.EnumName()[0]}"));
		}
	}
}