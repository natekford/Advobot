First, to get the really shitty part of the bot out of the way:
0.	I am too lazy to type out .ConfigureAwait(false) on every await I do and I don't really know what it does so I don't use it.

1.	A lot of guild settings return a readonly collection because that forces whoever is messing with them to reassign instead of just using .add
	This forces the setter to be used, which saves the list, thus keeping any changes made.
 
2.	I didn't know about Discord.Net's arg parsing until about 7 months into this project. That's why some parts may look like my own custom arg parsing.

3.	ILogModule goes into MyCommandContext to be used in exactly one command. The getinfo bot command.

4.	I use String.Format over string interpolation; fight me.

5.	Could possibly have forgotten saving IGuildSettings in a spot or two over the past three revisions of that interface.
	Regular class with only properties and a gross way of saving ->
	Class with an overuse of reflection and enums with a grosser way of saving ->
	Finally an interface and having MyGuildSettings save each time a setter is user ->
	Save as before except removed the setter saving and am using AfterExecute to save after commands edit settings.

To get some of the better parts out there:
0.	I'm slowly making each module completely optional (aside from IBotSettings and IGuildSettingsModule)
	ILogModule can be removed completely and only gives six easy to comment out errors.
	ITimersModule shouldn't be too hard to code out (just null check a lot of places).
	IInviteListModule will be easy to replace when I get around to creating it.