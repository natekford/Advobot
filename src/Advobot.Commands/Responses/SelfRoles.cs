using Advobot.Classes.Results;
using Advobot.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Advobot.Commands.Responses
{
	public sealed class SelfRoles : CommandResponses
	{
		private SelfRoles() { }

		public static AdvobotResult CreatedGroup(bool created, int number)
			=> Success(Default.FormatInterpolated($"Successfully {GetCreated(created)} self assignable role group {number}."));
	}
}
