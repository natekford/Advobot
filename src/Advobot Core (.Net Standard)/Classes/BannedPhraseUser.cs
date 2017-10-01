using Advobot.Enums;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes.BannedPhrases
{
	/// <summary>
	/// Holds a user and the counts of which punishments they should get.
	/// </summary>
	public class BannedPhraseUser
	{
		public IGuildUser User { get; }
		private Dictionary<PunishmentType, int> _PunishmentVals = new Dictionary<PunishmentType, int>();

		public BannedPhraseUser(IGuildUser user)
		{
			User = user;
			foreach (var type in Enum.GetValues(typeof(PunishmentType)).Cast<PunishmentType>())
			{
				_PunishmentVals.Add(type, 0);
			}
		}

		public int this[PunishmentType value]
		{
			get => _PunishmentVals[value];
		}

		public int IncrementValue(PunishmentType value)
		{
			return ++_PunishmentVals[value];
		}
		public int GetValue(PunishmentType value)
		{
			return _PunishmentVals[value];
		}
		public void ResetValue(PunishmentType value)
		{
			_PunishmentVals[value] = 0;
		}
	}
}