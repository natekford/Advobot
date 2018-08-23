using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReactiveUI;

namespace Advobot.NetCoreUI.Utils
{
	/// <summary>
	/// Utilities for the .Net Core UI.
	/// </summary>
	public static class NetCoreUIUtils
	{
		/// <summary>
		/// Because <see cref="ReactiveUI.ReactiveCommand"/> implements this method explicitly.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="parameter"></param>
		public static void Execute(this ICommand command, object parameter)
		{
			command.Execute(parameter);
		}
	}
}