using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.WebSocket;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Advobot
{
	public static class ExtensionMethods
	{
		public static String GetCommandName(this MethodInfo instance)
		{
			var attrType = typeof(CommandAttribute);
			var prop = (CommandAttribute)instance.GetCustomAttribute(attrType);
			var text = prop.Text;
			return text;
		}
		public static String[] GetCommandAliases(this MethodInfo instance)
		{
			var attrType = typeof(AliasAttribute);
			var prop = (AliasAttribute)instance.GetCustomAttribute(attrType);
			var text = prop.Aliases;
			return text;
		}
	}

	public class MyCommand : ModuleBase
	{
		public String getCommandName(String methodName)
		{
			MethodInfo method = GetType().GetMethod(methodName);
			return method.GetCommandName();
		}
	}

	public class Miscellaneous_Commands : MyCommand
	{
		[Command("serverid")]
		[Alias("sid")]
		[Summary("Shows the ID of the server.")]
		public async Task Say()
		{
			//var cn = getCommandName("Say");
			await Context.Channel.SendMessageAsync(String.Format("This server has the ID `{0}`.", Context.Guild.Id) + " ");
		}
	}
}
