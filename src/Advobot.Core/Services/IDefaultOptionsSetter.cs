﻿using System.Threading.Tasks;

using Discord.Commands;

namespace Advobot.Services
{
	/// <summary>
	/// Sets something in a command module to a recommended/default value.
	/// </summary>
	public interface IDefaultOptionsSetter
	{
		/// <summary>
		/// Sets some option value.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		Task SetAsync(ICommandContext context);
	}
}