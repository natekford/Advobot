using System;
using System.Collections.Generic;

namespace Advobot.Services.Localization
{
	public interface IEnumLocalizer
	{
		void Add<T>(IDictionary<T, string> values, bool overwrite = false) where T : Enum;

		bool TryGet<T>(out IReadOnlyCollection<string>? output) where T : Enum;

		bool TryGet<T>(T value, out string? output) where T : Enum;
	}
}