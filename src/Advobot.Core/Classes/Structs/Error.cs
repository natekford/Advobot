using Advobot.Core.Interfaces;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Wrapper for an error reason.
	/// </summary>
	public struct Error : IError
	{
		public string Reason { get; }

		public Error(string reason)
		{
			Reason = reason;
		}
	}
}
