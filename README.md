# RainPlannerBot
This bot was inspired by [DeliBot](https://github.com/OfficialWiddin/DeliBot), but because of a lot of issues with that bot, I decided to make my own.

The bots purpose is to simplify raid planning in Pokemon Go, by allowing the users of a discord community to create plans, which others can sign up.

The bot is written in C# using .NET Core, so in theory it can run on other platforms than Windows, but it hasn't been tested yet.

## Disclaimer

This is the first project I've put up on github, and pretty much the first open source project I am a member of. It's also the first time using .NET Core, and the first time using the [Discord.Net](https://github.com/RogueException/Discord.Net), and lastly, it's the first time I've come into contact with async/await. The project is also quite messy, since it's made in a rush. Because of all this, I'm not surprised if I have done something totaly wrong, and you should not be surprised if things don't go as smootly as you would like :)

## Setup

### Creating a discord application

* Go to https://discordapp.com/developers/applications/me
* Click "New App"
* Enter a cool name below "App name", this is the name the bot will have on discord
* Click "Create a Bot User"

### Inviting the newly created bot to your server

* Go to https://discordapp.com/developers/applications/me
* Where it says "Client ID:", copy that id
* Go to the following link, but replace `CLIENT_ID_GOES_HERE` with the id that you copied in the previous step: https://discordapp.com/oauth2/authorize?client_id=CLIENT_ID_GOES_HERE&scope=bot&permissions=1342532672

### Configuring RainPlannerBot

* Go to https://discordapp.com/developers/applications/me
* Where it says "Token:", click "click to reveal"
* Copy the token, and paste it in the configuration-file of the Bot
* Change other properties in the config to your liking

## Running RainPlannerBot

Compile the source and run the executable.

### Creating a raid plan

Type `!raid pokemon time location` in a channel where the bot is allowed, for example `!raid latias 15:00 the gym at central station`.

### Attending a raid

If you are going to attend the raid, press the reaction corresponding to your faction. If you bring extra devices, also press the number-reaction that matches the number of extra devices you are bringing.

### Change planned time

To edit the time of an existing plan, type `!edit id time`, where `id` is the id that the bot have assigned to the plan, for example `!edit 7624 16:00`

### Delete raid plan

Only the creator of the plan can delete it, and it is done by pressing the red-cross-reaction.
