using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Describes the reason for an error.
	/// </summary>
	public class Error
	{
		public string Reason { get; }

		public Error(Exception e)
		{
			Reason = e.Message;
		}
		public Error(string reason)
		{
			Reason = reason;
		}
	}
}
