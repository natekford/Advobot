using Advobot.Core.Interfaces;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Wrapper for an error reason.
	/// </summary>
	public struct Error : IError
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
