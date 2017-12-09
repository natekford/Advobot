namespace Advobot.Core.Classes
{
	/// <summary>
	/// Wrapper for an error reason.
	/// </summary>
	public class ErrorReason
	{
		public string Reason { get; private set; }

		public ErrorReason(string reason)
		{
			this.Reason = reason;
		}

		public override string ToString() => $"**ERROR:** {this.Reason}";
	}
}
