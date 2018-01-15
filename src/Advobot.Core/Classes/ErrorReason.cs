namespace Advobot.Core.Classes
{
	/// <summary>
	/// Wrapper for an error reason.
	/// </summary>
	public struct ErrorReason
	{
		public string Reason { get; private set; }

		public ErrorReason(string reason)
		{
			Reason = reason;
		}

		public override string ToString()
		{
			return $"**ERROR:** {Reason}";
		}
	}
}
