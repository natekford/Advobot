using System.Text.RegularExpressions;
using System.Windows.Documents;
using Advobot.Windows.Interfaces;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// Because you can't use \n, \r, \t etc. in XAML without them being escaped.
	/// </summary>
	public class AdvobotRun : Run, IUnescapedText
	{
		/// <summary>
		/// Sets <see cref="Run.Text"/> to the supplied value but unescaped.
		/// </summary>
		public string UnescapedText
		{
			get => Text;
			set => Text = Regex.Unescape(value);
		}
	}
}
