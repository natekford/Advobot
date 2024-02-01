using System.Globalization;

namespace Advobot.Services.Localization;

internal sealed class TypeLocalizer : ITypeLocalizer
{
	private readonly Dictionary<(CultureInfo, Type), string> _Dict = [];

	public void Add<T>(string value, bool overwrite = false)
	{
		var culture = CultureInfo.CurrentUICulture;
		var key = (culture, typeof(T));
		if (!_Dict.TryGetValue(key, out _))
		{
			_Dict.Add(key, value);
		}
		else if (overwrite)
		{
			_Dict[key] = value;
		}
	}

	public bool TryGet<T>(out string output)
	{
		var culture = CultureInfo.CurrentUICulture;
		return _Dict.TryGetValue((culture, typeof(T)), out output);
	}
}