# Advobot
The console or .Net Core UI versions run on any OS that supports .Net Core 2.1. The .Net Framework UI version only runs on Windows.

When wanting to run more than one bot at a time, supply the command line argument "-instance [number]" where the number is not 1 or a number supplied for any other currently running bots.

**Regular Features**
* **Help:** Help command lists a short description of a command and how to use it.
* **Servers:** Change names, regions, afk channels, default message notification, verification, and icons. Set guild specific prefixes.
* **Channels:** Create, delete, softdelete, list positions, and list permissions; change positions, permission overwrites, names, topics, bitrates, and user limits.
* **Roles:** Create, delete, softdelete, give, take, list positions, and list permissions; change positions, permissions, names, colors, mentionability, and hoisted statuses.
* **Users:** Text mute, voice mute, deafen, move, ban, softban, unban, kick, remove messages, prune members, list current bans, do an action on all users with a role.
* **Nicknames:** Change nicknames, remove all nicknames, replace words in nicknames and usernames.
* **Invites:** Create, delete, delete multiple with given variable, list all.
* **Information:** Get ID of servers, channels, roles, and users. Get information about the bot, users, emojis, and invites. Get users with specified names and with specified roles. Get a list of users who have joined, membercount, and users who joined at a given position.
* **Miscellaneous:** User made embeds, mention unmentionable roles.
* **Named Arguments:** Some commands support a format of 'name:argument value' for input.

I've tried to make a lot of the bot as configurable as possible meaning there are a lot of settings. Some settings can only be changed via certain commands so things can be labyrinthian at times.

**Settings**
* **Customizable Prefix:** Set it to whatever you want in your server between 1 and 10 characters long.
* **Command Configuration:** Enable/disable commands on servers and channels.
* **Welcome/Goodbye Messages:** Have the bot say something in a designated channel when users join/leave.
* **Self Assignable Roles:** Assign roles to groups giving them exclusivity and the ability for regular users to assign them to themselves.
* **Persistent Roles:** Give a user a role so hard they'll be unable to remove it even if they leave and rejoin the server.
* **Bot Users:** Give permissions to users via the bot instead of on Discord itself.
* **Server/Mod/Image Log:** Set channels to be their respective log. Ignore channels and specific logging actions.
* **Mute Role:** Set the mute role yourself, or let the bot create it. 
* **Banned Phrases:** Ban a specific word or you can try your luck at writing a RegEx and hope it doesn't delete every message.
* **Slowmode:** Set slowmodes, variable times, variable message counts, exempt roles.
* **Spam Prevention:** Prevent message, long message, links, images, and mention spam; prevent raid spam.
* **Quotes:** Save a quote to be recalled with a keyword.
* **Rule Formatting:** Save rules via the bot with specific formatting instructions so you can easily reprint them if lost.

**Owner Only Features**
* **Servers:** Create, delete.
* **Bot User:** Change icon, game, stream, name.
* **Bot Client:** Disconnect, restart, list servers.
* **Bot Settings:** Trusted users, ban users from commands, custom prefix, game, stream, log level.

**UI Features**
* **Stats:** Lists stats about various things. Latency, memory usage, thread usage, bot actions on users.
* **Files:** Searching, editing, copying, deleting. (Because who doesn't need a really bad text editor in their Discord bot?)
* **Colors:** Fully customizable colors. Want a fully #FF0000FF program? You can have it for as long as your eyes can stand it.
* **Themes:** Two themes if you don't want to spend time killing your eyes. Light and dark.
* **Settings:** Setting menu so you can change global bot settings easily.
* **A useless text box:** It used to have a use for command input, but then those uses got put into actual menus.

**+ many more features.**
