using Advobot.Actions;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Attributes
	{
		[AttributeUsage(AttributeTargets.Class)]
		public class PermissionRequirementAttribute : PreconditionAttribute
		{
			private uint _AllFlags;
			private uint _AnyFlags;

			//This doesn't have default values for the parameters since that makes it harder to potentially provide the wrong permissions
			public PermissionRequirementAttribute(GuildPermission[] anyOfTheListedPerms, GuildPermission[] allOfTheListedPerms)
			{
				_AnyFlags |= (1U << (int)GuildPermission.Administrator);
				foreach (var perm in anyOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
				{
					_AnyFlags |= (1U << (int)perm);
				}
				foreach (var perm in allOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
				{
					_AllFlags |= (1U << (int)perm);
				}
			}
			/* For when/if GuildPermission values get put as bits
			public PermissionRequirementAttribute(GuildPermission anyOfTheListedPerms, GuildPermission allOfTheListedPerms)
			{
				_AnyFlags |= GuildPermission.Administrator;
				foreach (var perm in anyOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
				{
					_AnyFlags |= perm;
				}
				foreach (var perm in allOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
				{
					_AllFlags |= perm;
				}
			}
			*/

			public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
			{
				if (context is MyCommandContext)
				{
					var cont = context as MyCommandContext;
					var user = context.User as IGuildUser;

					var guildBits = user.GuildPermissions.RawValue;
					var botBits = cont.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id)?.Permissions ?? 0;

					var userPerms = guildBits | botBits;
					if ((userPerms & _AllFlags) == _AllFlags || (userPerms & _AnyFlags) != 0)
					{
						return Task.FromResult(PreconditionResult.FromSuccess());
					}
				}
				return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
			}

			public string AllText
			{
				get { return String.Join(" & ", Gets.GetPermissionNames(_AllFlags)); }
			}
			public string AnyText
			{
				get { return String.Join(" | ", Gets.GetPermissionNames(_AnyFlags)); }
			}
		}

		[AttributeUsage(AttributeTargets.Class)]
		public class OtherRequirementAttribute : PreconditionAttribute
		{
			private const uint PERMISSION_BITS = 0
				| (1U << (int)GuildPermission.Administrator)
				| (1U << (int)GuildPermission.BanMembers)
				| (1U << (int)GuildPermission.DeafenMembers)
				| (1U << (int)GuildPermission.KickMembers)
				| (1U << (int)GuildPermission.ManageChannels)
				| (1U << (int)GuildPermission.ManageEmojis)
				| (1U << (int)GuildPermission.ManageGuild)
				| (1U << (int)GuildPermission.ManageMessages)
				| (1U << (int)GuildPermission.ManageNicknames)
				| (1U << (int)GuildPermission.ManageRoles)
				| (1U << (int)GuildPermission.ManageWebhooks)
				| (1U << (int)GuildPermission.MoveMembers)
				| (1U << (int)GuildPermission.MuteMembers);
			public Precondition Requirements { get; }

			public OtherRequirementAttribute(Precondition requirements)
			{
				Requirements = requirements;
			}

			public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
			{
				if (context is MyCommandContext)
				{
					var cont = context as MyCommandContext;
					var user = context.User as IGuildUser;

					var permissions = (Requirements & Precondition.UserHasAPerm) != 0;
					var guildOwner = (Requirements & Precondition.GuildOwner) != 0;
					var trustedUser = (Requirements & Precondition.TrustedUser) != 0;
					var botOwner = (Requirements & Precondition.BotOwner) != 0;

					if (permissions)
					{
						var guildBits = user.GuildPermissions.RawValue;
						var botBits = cont.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id)?.Permissions ?? 0;

						var userPerms = guildBits | botBits;
						if ((userPerms & PERMISSION_BITS) != 0)
						{
							return Task.FromResult(PreconditionResult.FromSuccess());
						}
					}
					if (guildOwner && cont.Guild.OwnerId == user.Id)
					{
						return Task.FromResult(PreconditionResult.FromSuccess());
					}
					if (trustedUser && cont.BotSettings.TrustedUsers.Contains(user.Id))
					{
						return Task.FromResult(PreconditionResult.FromSuccess());
					}
					if (botOwner && cont.BotSettings.BotOwnerId == user.Id)
					{
						return Task.FromResult(PreconditionResult.FromSuccess());
					}
				}
				return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
			}
		}

		[AttributeUsage(AttributeTargets.Class)]
		public class CommandRequirementsAttribute : PreconditionAttribute
		{
			public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
			{
				if (context is MyCommandContext)
				{
					var cont = context as MyCommandContext;
					var user = context.User as IGuildUser;

					if (!(await cont.Guild.GetCurrentUserAsync()).GuildPermissions.Administrator)
					{
						return PreconditionResult.FromError("This bot will not function without the `Administrator` permission.");
					}
					else if (!cont.BotSettings.Loaded)
					{
						return PreconditionResult.FromError("Wait until the bot is loaded.");
					}
					if (!cont.GuildSettings.Loaded)
					{
						return PreconditionResult.FromError("Wait until the guild is loaded.");
					}
					else if (cont.GuildSettings.IgnoredCommandChannels.Contains(context.Channel.Id) || !CheckIfCommandIsEnabled(cont, command, user))
					{
						return PreconditionResult.FromError(Constants.IGNORE_ERROR);
					}
					else
					{
						return PreconditionResult.FromSuccess();
					}
				}
				return PreconditionResult.FromError(Constants.IGNORE_ERROR);
			}

			private bool CheckIfCommandIsEnabled(MyCommandContext context, CommandInfo command, IGuildUser user)
			{
				//Use the first alias since that's what group gets set as (could use any alias since GetCommand works for aliases too)
				//Doing a split since subcommands (in this bot's case) are simply easy to use options on a single command
				var cmd = Gets.GetCommand(context.GuildSettings, command.Aliases[0].Split(' ')[0]);
				if (!cmd.ValAsBoolean)
				{
					return false;
				}

				/* If user is set, use user setting
				 * Else if any roles are set, use the highest role setting
				 * Else if channel is set, use channel setting
				 */

				var userOverrides = context.GuildSettings.CommandsDisabledOnUser;
				var userOverride = userOverrides.FirstOrDefault(x => x.Id == context.User.Id && cmd.Name.CaseInsEquals(x.Name));
				if (userOverride != null)
				{
					return userOverride.Enabled;
				}

				var roleOverrides = context.GuildSettings.CommandsDisabledOnRole;
				var roleOverride = roleOverrides.Where(x => user.RoleIds.Contains(x.Id) && cmd.Name.CaseInsEquals(x.Name)).OrderBy(x => context.Guild.GetRole(x.Id).Position).LastOrDefault();
				if (roleOverride != null)
				{
					return roleOverride.Enabled;
				}

				var channelOverrides = context.GuildSettings.CommandsDisabledOnChannel;
				var channelOverride = channelOverrides.FirstOrDefault(x => x.Id == context.Channel.Id && cmd.Name.CaseInsEquals(x.Name));
				if (channelOverride != null)
				{
					return channelOverride.Enabled;
				}

				return true;
			}
		}

		[AttributeUsage(AttributeTargets.Class)]
		public class DefaultEnabledAttribute : Attribute
		{
			public bool Enabled { get; }

			public DefaultEnabledAttribute(bool enabled)
			{
				Enabled = enabled;
			}
		}

		[AttributeUsage(AttributeTargets.Class)]
		public class UsageAttribute : Attribute
		{
			public string Usage { get; }

			public UsageAttribute(string usage)
			{
				Usage = usage;
			}
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class DiscordObjectTargetAttribute : Attribute
		{
			public Target Target { get; }

			public DiscordObjectTargetAttribute(Target target)
			{
				Target = target;
			}
		}

		[AttributeUsage(AttributeTargets.Parameter)]
		public class VerifyObjectAttribute : ParameterPreconditionAttribute
		{
			private readonly bool _IfNullDrawFromContext;
			private readonly ObjectVerification[] _Checks;

			public VerifyObjectAttribute(bool ifNullDrawFromContext, params ObjectVerification[] checks)
			{
				_IfNullDrawFromContext = ifNullDrawFromContext;
				_Checks = checks;
			}

			public override Task<PreconditionResult> CheckPermissions(ICommandContext context, Discord.Commands.ParameterInfo parameter, object value, IServiceProvider services)
			{
				//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
				if (value == null && !_IfNullDrawFromContext)
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}

				return Task.FromResult(GetPreconditionResult(context, value));
			}

			private PreconditionResult GetPreconditionResult(ICommandContext context, System.Collections.IEnumerable list)
			{
				foreach (var item in list)
				{
					var preconditionResult = GetPreconditionResult(context, item);
					if (!preconditionResult.IsSuccess)
					{
						return preconditionResult;
					}
				}

				return PreconditionResult.FromSuccess();
			}
			private PreconditionResult GetPreconditionResult(ICommandContext context, object value)
			{
				FailureReason failureReason = default(FailureReason);
				object obj = null;
				if (value is ITextChannel)
				{
					var returned = Channels.GetChannel(context.Guild, context.User as IGuildUser, _Checks, (value ?? context.Channel) as IGuildChannel);
					failureReason = returned.Reason;
					obj = returned.Object;
				}
				else if (value is IVoiceChannel)
				{
					var returned = Channels.GetChannel(context.Guild, context.User as IGuildUser, _Checks, (value ?? (context.User as IGuildUser).VoiceChannel) as IGuildChannel);
					failureReason = returned.Reason;
					obj = returned.Object;
				}
				else if (value is IGuildUser)
				{
					var returned = Users.GetGuildUser(context.Guild, context.User as IGuildUser, _Checks, (value ?? context.User) as IGuildUser);
					failureReason = returned.Reason;
					obj = returned.Object;
				}
				else if (value is IRole)
				{
					var returned = Roles.GetRole(context.Guild, context.User as IGuildUser, _Checks, value as IRole);
					failureReason = returned.Reason;
					obj = returned.Object;
				}

				if (failureReason != FailureReason.NotFailure)
				{
					return PreconditionResult.FromError(Actions.Formatting.FormatErrorString(context.Guild, failureReason, obj));
				}
				else
				{
					return PreconditionResult.FromSuccess();
				}
			}
		}

		[AttributeUsage(AttributeTargets.Parameter)]
		public class VerifyEnumAttribute : ParameterPreconditionAttribute
		{
			private readonly uint _Allowed;
			private readonly uint _Disallowed;

			public VerifyEnumAttribute(uint allowed = 0, uint disallowed = 0)
			{
				_Allowed = allowed;
				_Disallowed = disallowed;
			}

			public override Task<PreconditionResult> CheckPermissions(ICommandContext context, Discord.Commands.ParameterInfo parameter, object value, IServiceProvider services)
			{
				var enumVal = (uint)value;
				if (_Allowed != 0 && ((_Allowed & enumVal) == 0))
				{
					return Task.FromResult(PreconditionResult.FromError(String.Format("The option `{0}` is not allowed for the current command overload.", value)));
				}
				else if (_Disallowed != 0 && ((_Disallowed & enumVal) != 0))
				{
					return Task.FromResult(PreconditionResult.FromError(String.Format("The option `{0}` is not allowed for the current command overload.", value)));
				}
				else
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
		}

		[AttributeUsage(AttributeTargets.Parameter)]
		public class VerifyStringAttribute : ParameterPreconditionAttribute
		{
			private readonly string[] _ValidStrings;

			public VerifyStringAttribute(params string[] validStrings)
			{
				_ValidStrings = validStrings;
			}

			public override Task<PreconditionResult> CheckPermissions(ICommandContext context, Discord.Commands.ParameterInfo parameter, object value, IServiceProvider services)
			{
				//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
				if (value == null)
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}

				return _ValidStrings.CaseInsContains(value.ToString()) ? Task.FromResult(PreconditionResult.FromSuccess()) : Task.FromResult(PreconditionResult.FromError("Invalid string provided."));
			}
		}

		[AttributeUsage(AttributeTargets.Parameter)]
		public class VerifyStringLengthAttribute : ParameterPreconditionAttribute
		{
			private readonly ReadOnlyDictionary<Target, Tuple<int, int, string>> _MinsAndMaxesAndErrors = new ReadOnlyDictionary<Target, Tuple<int, int, string>>(new Dictionary<Target, Tuple<int, int, string>>
		{
			{ Target.Guild, new Tuple<int, int, string>(Constants.MIN_GUILD_NAME_LENGTH, Constants.MAX_GUILD_NAME_LENGTH, "guild name") },
			{ Target.Channel, new Tuple<int, int, string>(Constants.MIN_CHANNEL_NAME_LENGTH, Constants.MAX_CHANNEL_NAME_LENGTH, "channel name") },
			{ Target.Role, new Tuple<int, int, string>(Constants.MIN_ROLE_NAME_LENGTH, Constants.MAX_ROLE_NAME_LENGTH, "role name") },
			{ Target.Name, new Tuple<int, int, string>(Constants.MIN_USERNAME_LENGTH, Constants.MAX_USERNAME_LENGTH, "username") },
			{ Target.Nickname, new Tuple<int, int, string>(Constants.MIN_NICKNAME_LENGTH, Constants.MAX_NICKNAME_LENGTH, "nickname") },
			{ Target.Game, new Tuple<int, int, string>(Constants.MIN_GAME_LENGTH, Constants.MAX_GAME_LENGTH, "game") },
			{ Target.Stream, new Tuple<int, int, string>(Constants.MIN_STREAM_LENGTH, Constants.MAX_STREAM_LENGTH, "stream name") },
			{ Target.Topic, new Tuple<int, int, string>(Constants.MIN_TOPIC_LENGTH, Constants.MAX_TOPIC_LENGTH, "channel topic") },
		});
			private int _Min;
			private int _Max;
			private string _TooShort;
			private string _TooLong;

			public VerifyStringLengthAttribute(Target target)
			{
				if (_MinsAndMaxesAndErrors.TryGetValue(target, out var minAndMaxAndError))
				{
					_Min = minAndMaxAndError.Item1;
					_Max = minAndMaxAndError.Item2;
					_TooShort = String.Format("A {0} must be at least `{1}` characters long.", minAndMaxAndError.Item3, _Min);
					_TooLong = String.Format("A {0} must be at most `{1}` characters long.", minAndMaxAndError.Item3, _Max);
				}
				else
				{
					throw new NotSupportedException("Inputted enum doesn't have a min and max or error output.");
				}
			}

			public override Task<PreconditionResult> CheckPermissions(ICommandContext context, Discord.Commands.ParameterInfo parameter, object value, IServiceProvider services)
			{
				//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
				if (value == null)
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}

				if (value.GetType() == typeof(string))
				{
					var str = value.ToString();
					if (str.Length < _Min)
					{
						return Task.FromResult(PreconditionResult.FromError(_TooShort));
					}
					else if (str.Length > _Max)
					{
						return Task.FromResult(PreconditionResult.FromError(_TooLong));
					}
					else
					{
						return Task.FromResult(PreconditionResult.FromSuccess());
					}
				}
				else
				{
					throw new NotSupportedException(String.Format("{0} only supports strings.", nameof(VerifyStringLengthAttribute)));
				}
			}
		}
	}
}
