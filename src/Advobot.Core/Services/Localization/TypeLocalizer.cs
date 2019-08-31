using System;
using System.Collections.Generic;
using System.Globalization;

namespace Advobot.Services.Localization
{
	internal sealed class TypeLocalizer : ITypeLocalizer
	{
		private readonly Dictionary<(CultureInfo, Type), string> _Map
			= new Dictionary<(CultureInfo, Type), string>();

		public void Add<T>(string value, bool overwrite = false)
		{
			var culture = CultureInfo.CurrentUICulture;
			var key = (culture, typeof(T));
			if (!_Map.TryGetValue(key, out _))
			{
				_Map.Add(key, value);
			}
			else if (overwrite)
			{
				_Map[key] = value;
			}
		}

		public bool TryGet<T>(out string output)
		{
			var culture = CultureInfo.CurrentUICulture;
			return _Map.TryGetValue((culture, typeof(T)), out output);
		}
	}
}