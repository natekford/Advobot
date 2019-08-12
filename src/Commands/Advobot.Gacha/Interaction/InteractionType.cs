namespace Advobot.Gacha.Interaction
{
	public enum InteractionType
	{
		[UnicodeRepresentation("\u2764")] //❤
		Claim,
		[UnicodeRepresentation("\u25C0")] //◀
		Left,
		[UnicodeRepresentation("\u25B6")] //▶
		Right,
		[UnicodeRepresentation("\u2705")] //✅
		Confirm,
		[UnicodeRepresentation("\u274C")] //❌
		Deny,
	}
}
