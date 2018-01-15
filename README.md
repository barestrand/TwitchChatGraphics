#TwitchChatGraphics
##Transparent overlay application, graphically illustrating word occurrence in any Twitch channel chat.

Perfect for watching tournament streams on fullscreen without need for chat window open, get a feel for the chat reactions!

Windows 7+ compatible, 100+Mb RAM req, runs on .net framework

NOTE: Program is in early development stage so all feedback and ideas are welcome

##How to use:
###1. Get the latest release build of the application [here](https://github.com/barestrand/TwitchChatGraphics/releases)

###2. Get your personal IRC login info(username, oauth:password) from Twitch at  
http://www.twitchapps.com/tmi/  

###3. Decide what channel to follow and put suitable values in the settings file accompanying the program:  
####Example settings:

	ip-->irc.twitch.tv  
	port-->6667  
	channelname-->officialgetright  
	username-->derpmaster1337  
	oauth_password-->oauth:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx  

###4. Start up program and enjoy

###5. AIDS filters!  
####Examples of current implementations inside preambles.csv:

	OR(haha,ahah)-->hahah  
	AND(trump,wall)-->__TRUMP WALL__  
	AND(rip,skins)-->rip skins  
	SUPRESS(twat)  

(SUPRESS,AND) keywords are applied to entire messages.  
Order of application: SUPRESS > OR > AND > exceptions  
EXCEPTIONS are applied to single words delimited by spaces in chat and can be used as a filter for unwanted common words.

###6. (Optional) Customize!  
File formats are pretty self explanatory in general for settings, emotes, preambles and exceptions.  
These files can be altered to your own liking to improve your own experience.  
Note: Picture format has to be 32 bit PNG for best compatibility.  

#####Keywords: Twitch, Chat, Graphics, Tournament, Reactions
