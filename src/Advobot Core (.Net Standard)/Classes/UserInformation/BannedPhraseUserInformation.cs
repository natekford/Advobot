using Advobot.Enums;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes.UserInformation
{
	/// <summary>
	/// Holds a user and the counts of which punishments they should get.
	/// </summary>
	public class BannedPhraseUserInformation : UserInfo
	{
		private Dictionary<PunishmentType, int> _PunishmentVals = new Dictionary<PunishmentType, int>();

		public BannedPhraseUserInformation(IGuildUser user) : base(user)
		{
			foreach (PunishmentType type in Enum.GetValues(typeof(PunishmentType)))
			{
				_PunishmentVals.Add(type, 0);
			}
		}

		public int this[PunishmentType value]
		{
			get => _PunishmentVals[value];
		}

		public int GetValue(PunishmentType value)
		{
			return _PunishmentVals[value];
		}
		public int IncrementValue(PunishmentType value)
		{
			return ++_PunishmentVals[value];
		}
		public void ResetValue(PunishmentType value)
		{
			_PunishmentVals[value] = 0;
		}

		public override string ToString()
		{
			return String.Join("/", _PunishmentVals.Select(x => $"{x.Value}{x.Key.EnumName()[0]}"));
		}
	}
}