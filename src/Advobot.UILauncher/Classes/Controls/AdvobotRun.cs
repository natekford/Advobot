using Advobot.UILauncher.Interfaces;
using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// Because you can't use \n, \r, \t etc. in XAML without them being escaped.
	/// </summary>
	internal class AdvobotRun : Run, IUnescapedText
	{
		/// <summary>
		/// Sets <see cref="Run.Text"/> to the supplied value but unescaped.
		/// </summary>
		public string UnescapedText
		{
			set => base.Text = Regex.Unescape(value);
		}
	}
}
