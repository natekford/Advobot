using System.Windows.Input;

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