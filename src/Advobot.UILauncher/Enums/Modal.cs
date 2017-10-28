using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.UILauncher.Enums
{
	[Flags]
    internal enum Modal : uint
    {
		FileSearch = (1U << 0),
		OutputSearch = (1U << 1),
    }
}
