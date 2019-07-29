﻿using System;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Validates the passed in <see cref="SocketGuildChannel"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateGenericChannelAttribute : ValidateChannelAttribute
	{
		/// <summary>
		/// Cannot check from context for an unspecified channel type.
		/// </summary>
		public override bool FromContext => false;

		/// <summary>
		/// Creates an instance of <see cref="ValidateGenericChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public ValidateGenericChannelAttribute(params ChannelPermission[] permissions)
			: base(permissions) { }
	}
}
