using System;
using System.Collections.Generic;
using System.Text;

namespace Advobot.Settings.GenerateResetValues
{
	internal sealed class Null : IGenerateResetValue
	{
		public object? GenerateResetValue(object currentValue)
			=> null;
	}
}
