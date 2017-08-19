using Advobot.Actions;
using Advobot.Enums;
using Advobot.NonSavedClasses;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Advobot
{
	namespace Attributes
	{
		/// <summary>
		/// Checks if the user has all of the permissions supplied for allOfTheListedPerms or if the user has any of the permissions supplied for anyOfTheListedPerms.
		/// </summary>
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
				get { return String.Join(" & ", GetActions.GetPermissionNames(_AllFlags)); }
			}
			public string AnyText
			{
				get { return String.Join(" | ", GetActions.GetPermissionNames(_AnyFlags)); }
			}
		}

		/// <summary>
		/// Checks if a user has any permissions that would generally be needed for a command, if the user is the guild owner, if the user if the bot owner, or if the user is a trusted user.
		/// </summary>
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

			public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
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
							return PreconditionResult.FromSuccess();
						}
					}
					if (guildOwner && cont.Guild.OwnerId == user.Id)
					{
						return PreconditionResult.FromSuccess();
					}
					if (trustedUser && cont.BotSettings.TrustedUsers.Contains(user.Id))
					{
						return PreconditionResult.FromSuccess();
					}
					if (botOwner && (await UserActions.GetBotOwner(cont.Client)).Id == user.Id)
					{
						return PreconditionResult.FromSuccess();
					}
				}
				return PreconditionResult.FromError(Constants.IGNORE_ERROR);
			}
		}

		/// <summary>
		/// Checks to make sure the bot has admin, the bot is loaded, the guild is loaded, the channel isn't ignored from commands, and the command is enabled for the user.
		/// </summary>
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
				var cmd = GetActions.GetCommand(context.GuildSettings, command.Aliases[0].Split(' ')[0]);
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

		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
		public class BrokenCommandAttribute : PreconditionAttribute
		{
			public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
			{
				if (context is MyCommandContext)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context as MyCommandContext, "This command does not work.");
				}
				return PreconditionResult.FromError(Constants.IGNORE_ERROR);
			}
		}

		[AttributeUsage(AttributeTargets.Class)]
		public class DefaultEnabledAttribute : Attribute
		{
			public readonly bool Enabled;

			public DefaultEnabledAttribute(bool enabled)
			{
				Enabled = enabled;
			}
		}

		[AttributeUsage(AttributeTargets.Class)]
		public class UsageAttribute : Attribute
		{
			public readonly string Usage;

			public UsageAttribute(string usage)
			{
				Usage = usage;
			}
		}

		/// <summary>
		/// Verifies the parameter this attribute is targetting fits all of the given conditions. Abstract since _GetResultsDict has to be created by a class inheriting this.
		/// </summary>
		[AttributeUsage(AttributeTargets.Parameter)]
		public abstract class VerifyObjectAttribute : ParameterPreconditionAttribute
		{
			protected Dictionary<Type, Func<ICommandContext, object, Tuple<FailureReason, object>>> _GetResultsDict;
			protected bool _IfNullCheckFromContext;

			public override Task<PreconditionResult> CheckPermissions(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
			{
				//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
				if (value == null && !_IfNullCheckFromContext)
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}

				if (value is System.Collections.IEnumerable)
				{
					foreach (var item in value as System.Collections.IEnumerable)
					{
						//Don't bother trying to go farther if anything is a failure.
						var preconditionResult = GetPreconditionResult(context, item, item.GetType());
						if (!preconditionResult.IsSuccess)
						{
							return Task.FromResult(preconditionResult);
						}
					}
				}
				else
				{
					return Task.FromResult(GetPreconditionResult(context, value, parameter.Type));
				}

				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			protected virtual PreconditionResult GetPreconditionResult(ICommandContext context, object value, Type type)
			{
				//Will provide exceptions if invalid VerifyObject attributes are used on objects. E.G. VerifyChannel used on a role
				var result = _GetResultsDict[type](context, value);
				var failureReason = result.Item1;
				var obj = result.Item2;

				return (failureReason != FailureReason.NotFailure) ? PreconditionResult.FromError(FormattingActions.FormatErrorString(context.Guild, failureReason, obj)) : PreconditionResult.FromSuccess();
			}
		}

		/// <summary>
		/// Uses ChannelVerification enum to verify certain aspects of a channel. Only works on ITextChannel, IVoiceChannel, and IGuildChannel.
		/// </summary>
		[AttributeUsage(AttributeTargets.Parameter)]
		public class VerifyChannelAttribute : VerifyObjectAttribute
		{
			protected ChannelVerification[] _Checks;

			public VerifyChannelAttribute(bool ifNullCheckFromContext, params ChannelVerification[] checks)
			{
				_GetResultsDict = new Dictionary<Type, Func<ICommandContext, object, Tuple<FailureReason, object>>>
				{
					{ typeof(ITextChannel), ITextChannelResult },
					{ typeof(IVoiceChannel), IVoiceChannelResult },
					{ typeof(IGuildChannel), IGuildChannelResult },
				};
				_IfNullCheckFromContext = ifNullCheckFromContext;
				_Checks = checks;
			}

			private Tuple<FailureReason, object> ITextChannelResult(ICommandContext context, object value)
			{
				var returned = ChannelActions.GetChannel(context.Guild, context.User as IGuildUser, _Checks, (value ?? context.Channel) as IGuildChannel);
				return Tuple.Create<FailureReason, object>(returned.Reason, returned.Object);
			}
			private Tuple<FailureReason, object> IVoiceChannelResult(ICommandContext context, object value)
			{
				var returned = ChannelActions.GetChannel(context.Guild, context.User as IGuildUser, _Checks, (value ?? (context.User as IGuildUser).VoiceChannel) as IGuildChannel);
				return Tuple.Create<FailureReason, object>(returned.Reason, returned.Object);
			}
			private Tuple<FailureReason, object> IGuildChannelResult(ICommandContext context, object value)
			{
				var returned = ChannelActions.GetChannel(context.Guild, context.User as IGuildUser, _Checks, value as IGuildChannel);
				return Tuple.Create<FailureReason, object>(returned.Reason, returned.Object);
			}
		}

		/// <summary>
		/// Uses UserVerification enum to verify certain aspects of a user. Only works on IGuildUser and IUser.
		/// </summary>
		[AttributeUsage(AttributeTargets.Parameter)]
		public class VerifyUserAttribute : VerifyObjectAttribute
		{
			protected UserVerification[] _Checks;

			public VerifyUserAttribute(bool ifNullCheckFromContext, params UserVerification[] checks)
			{
				_GetResultsDict = new Dictionary<Type, Func<ICommandContext, object, Tuple<FailureReason, object>>>
				{
					{ typeof(IGuildUser), IGuildUserResult },
					{ typeof(IUser), IUserResult },
				};
				_IfNullCheckFromContext = ifNullCheckFromContext;
				_Checks = checks;
			}

			private Tuple<FailureReason, object> IGuildUserResult(ICommandContext context, object value)
			{
				var returned = UserActions.GetGuildUser(context.Guild, context.User as IGuildUser, _Checks, (value ?? context.User) as IGuildUser);
				return Tuple.Create<FailureReason, object>(returned.Reason, returned.Object);
			}
			private Tuple<FailureReason, object> IUserResult(ICommandContext context, object value)
			{
				//If user cannot be cast as an IGuildUser then they're not on the guild and thus anything can be used on them
				return (value as IGuildUser != null) ? IGuildUserResult(context, value) : Tuple.Create(FailureReason.NotFailure, value);
			}
		}

		/// <summary>
		/// Uses RoleVerification enum to verify certain aspects of a role. Only works on IRole.
		/// </summary>
		[AttributeUsage(AttributeTargets.Parameter)]
		public class VerifyRoleAttribute : VerifyObjectAttribute
		{
			protected RoleVerification[] _Checks;

			public VerifyRoleAttribute(bool ifNullCheckFromContext, params RoleVerification[] checks)
			{
				_GetResultsDict = new Dictionary<Type, Func<ICommandContext, object, Tuple<FailureReason, object>>>
				{
					{ typeof(IRole), IRoleResult },
				};
				_IfNullCheckFromContext = ifNullCheckFromContext;
				_Checks = checks;
			}

			private Tuple<FailureReason, object> IRoleResult(ICommandContext context, object value)
			{
				var returned = RoleActions.GetRole(context.Guild, context.User as IGuildUser, _Checks, value as IRole);
				return Tuple.Create<FailureReason, object>(returned.Reason, returned.Object);
			}
		}

		/// <summary>
		/// Certain objects in Discord have minimum and maximum lengths for the names that can be set for them. This attribute verifies those lengths and provides error stating the min/max if under/over.
		/// </summary>
		[AttributeUsage(AttributeTargets.Parameter)]
		public class VerifyStringLengthAttribute : ParameterPreconditionAttribute
		{
			private static readonly Dictionary<Target, Tuple<int, int, string>> _MinsAndMaxesAndErrors = new Dictionary<Target, Tuple<int, int, string>>
			{
				{ Target.Guild,		Tuple.Create(Constants.MIN_GUILD_NAME_LENGTH,	Constants.MAX_GUILD_NAME_LENGTH,	"guild name") },
				{ Target.Channel,	Tuple.Create(Constants.MIN_CHANNEL_NAME_LENGTH, Constants.MAX_CHANNEL_NAME_LENGTH,	"channel name") },
				{ Target.Role,		Tuple.Create(Constants.MIN_ROLE_NAME_LENGTH,	Constants.MAX_ROLE_NAME_LENGTH,		"role name") },
				{ Target.Name,		Tuple.Create(Constants.MIN_USERNAME_LENGTH,		Constants.MAX_USERNAME_LENGTH,		"username") },
				{ Target.Nickname,	Tuple.Create(Constants.MIN_NICKNAME_LENGTH,		Constants.MAX_NICKNAME_LENGTH,		"nickname") },
				{ Target.Game,		Tuple.Create(Constants.MIN_GAME_LENGTH,			Constants.MAX_GAME_LENGTH,			"game") },
				{ Target.Stream,	Tuple.Create(Constants.MIN_STREAM_LENGTH,		Constants.MAX_STREAM_LENGTH,		"stream name") },
				{ Target.Topic,		Tuple.Create(Constants.MIN_TOPIC_LENGTH,		Constants.MAX_TOPIC_LENGTH,			"channel topic") },
				{ Target.Prefix,	Tuple.Create(Constants.MIN_PREFIX_LENGTH,		Constants.MAX_PREFIX_LENGTH,		"bot prefix") },
			};
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
					throw new NotSupportedException("Supplied enum doesn't have a min and max or error output.");
				}
			}

			public override Task<PreconditionResult> CheckPermissions(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
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
