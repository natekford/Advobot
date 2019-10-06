﻿using Advobot.Levels.Service;
using Advobot.Modules;

namespace Advobot.Levels
{
	public abstract class LevelModuleBase : AdvobotModuleBase
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public ILevelService Levels { get; set; }
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
	}
}