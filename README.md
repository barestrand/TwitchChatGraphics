#TwitchChatGraphics
##Transparent overlay application, graphically illustrating word occurrence in any Twitch channel chat.

Perfect for watching tournament streams on fullscreen without need for chat window open to get a feel for the chat reactions!

![alt text](https://github.com/barestrand/TwitchChatGraphics/blob/master/pansy.PNG "Showcase 1")

Windows 7+ compatible, 100+Mb RAM req, runs on .net framework

NOTE: Program is in early development stage so all feedback and ideas are welcome

##How to use:
###1. Get your personal IRC login info(username, oauth:password) from Twitch at  
  http://www.twitchapps.com/tmi/  

###2. Decide what channel to follow and put suitable values in the settings file accompanying the program:  
     ####Example settings:  
     ip-->irc.twitch.tv  
     port-->6667  
     channelname-->officialgetright  
     username-->derpmaster1337  
     oauth_password-->oauth:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx  

###3. Start up program and enjoy

###4. Aids filters!  
     ####Examples of current implementations inside preambles.csv(OR,AND,SUPRESS):  
     OR(haha,ahah)-->hahah  
     AND(trump,wall)-->\_\_TRUMP WALL\_\_  
     AND(rip,skins)-->rip skins  
     SUPRESS(twat)  

     (SUPRESS,AND) keywords are applied to entire messages.
     Order of application: SUPRESS > OR > AND > exceptions
     EXCEPTIONS are applied to single words delimited by spaces in chat and can be used as a filter for unwanted common words.

###5. (Optional) Customize!  
     File formats are pretty self explanatory in general for settings, emotes, preambles and exceptions.
     These files can be altered to your own liking to improve your own experience.
     Note: Picture format has to be 32 bit PNG for best compatibility.
