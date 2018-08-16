using System;

namespace Advobot.Classes
{
	/// <summary>
	/// Describes the reason for an error.
	/// </summary>
	public class Error
	{
		/// <summary>
		/// The reason for the error.
		/// </summary>
		public string Reason { get; }

		/// <summary>
		/// Gets the reason from an exception.
		/// </summary>
		/// <param name="e"></param>
		public Error(Exception e)
		{
			Reason = e.Message;
		}
		/// <summary>
		/// Gets the reason from a string.
		/// </summary>
		/// <param name="reason"></param>
		public Error(string reason)
		{
			Reason = reason;
		}
	}
}
