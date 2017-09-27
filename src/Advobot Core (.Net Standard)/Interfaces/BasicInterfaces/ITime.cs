using System;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Signifies the object can return a <see cref="DateTime"/>.
	/// </summary>
	public interface IHasTime
	{
		DateTime GetTime();
	}
}