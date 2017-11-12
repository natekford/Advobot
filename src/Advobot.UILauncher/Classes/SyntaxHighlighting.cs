using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Reflection;
using System.Xml;

namespace Advobot.UILauncher.Classes
{
	internal static class SyntaxHighlighting
	{
		public static void LoadJSONHighlighting()
			=> LoadSyntaxHighlighting("Advobot.UILauncher.Resources.JSONSyntaxHighlighting.xshd", "JSON", new[] { ".json" });
		public static void LoadSyntaxHighlighting(string loc, string name, string[] extensions)
		{
			using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(loc)
				?? throw new InvalidOperationException($"{loc} is missing."))
			{
				using (var r = new XmlTextReader(s))
				{
					var highlighting = HighlightingLoader.Load(r, HighlightingManager.Instance);
					HighlightingManager.Inst‌​ance.RegisterHighlighting(name, extensions, highlighting);
				}
			}
		}
	}
}
