using Advobot.Core.Actions;
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
	public class BannedPhraseUserInformation : UserInfo
	{
		private Dictionary<PunishmentType, int> _PunishmentVals = new Dictionary<PunishmentType, int>();

		public BannedPhraseUserInformation(IGuildUser user) : base(user)
		{
			foreach (PunishmentType type in Enum.GetValues(typeof(PunishmentType)))
			{
				this._PunishmentVals.Add(type, 0);
			}
		}

		public int this[PunishmentType value]
		{
			get => _PunishmentVals[value];
		}

		public int GetValue(PunishmentType value) => this._PunishmentVals[value];
		public int IncrementValue(PunishmentType value) => ++this._PunishmentVals[value];
		public void ResetValue(PunishmentType value) => this._PunishmentVals[value] = 0;

		public override string ToString() => String.Join("/", this._PunishmentVals.Select(x => $"{x.Value}{x.Key.EnumName()[0]}"));
	}
}