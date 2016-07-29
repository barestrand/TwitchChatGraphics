Windows 7+ compatible, 100+Mb RAM req, runs on .net framework

NOTE: Program is in early development stage so all feedback and ideas are welcome

How to use:

1. Get your personal login info from Twitch at this link:
http://www.twitchapps.com/tmi/
You need: username, oauth:password

2. Decide what channel to follow and put suitable values in the settings file accompanying the program:

- Personal channel: (eg. officialgetright)

___Example settings___
ip-->irc.twitch.tv
port-->6667
channelname-->officialgetright
username-->derpmaster1337
oauth_password-->oauth:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

- Event channel: (eg. esl_csgo)

Event IP's found at: http://tmi.twitch.tv/servers?channel=esl_csgo
Usually first or second IP shown will work
Port usually works with 6667

___Example settings___
ip-->192.16.64.214
port-->6667
channelname-->esl_csgo
username-->derpmaster1337
oauth_password-->oauth:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

3. Start up program and enjoy

4. (Optional) Customize!

File formats are pretty self explanatory in general for settings, emotes, preambles and exceptions.
These files can be altered to your own liking to improve your own experience.

Note: Picture format has to be 32 bit PNG for best compatibility.

5. FILTERS!

PREAMBLES: SUPRESS & AND are applied to entire messages.
Order of how stuff is applied to message: SUPRESS - OR - AND - exceptions
Examples of current implementations(OR,AND,SUPRESS):

OR(haha,ahah)-->hahah
AND(trump,wall)-->_____TRUMP WALL_____
AND(rip,skins)-->rip skins
SUPRESS(twat)

EXCEPTIONS are applied to single words delimited by spaces in chat and can be used as a filter for unwanted common words.