# RainPlannerBot
This bot was inspired by [DeliBot](https://github.com/OfficialWiddin/DeliBot), but because of a lot of issues with that bot, I decided to make my own.

The bots purpose is to simplify raid planning in Pokemon Go, by allowing the users of a discord community to create plans, which others can sign up for.

The bot is written in C# using .NET Core, so in theory it can run on other platforms than Windows, but it hasn't been tested yet.

## Disclaimer

This is the first project I've put up on github, and pretty much the first open source project I am a member of. It's also the first time using .NET Core, and the first time using the [Discord.Net](https://github.com/RogueException/Discord.Net), and lastly, it's the first time I've come into contact with async/await. The project is also quite messy, since it's made in a rush. Because of all this, I'm not surprised if I have done something totaly wrong, and you should not be surprised if things don't go as smootly as you would like :)

## Setup

### Creating a discord application

* Go to https://discordapp.com/developers/applications/me
* Click **New App**
* Enter a cool name below **App name**, this is the name the bot will have on discord
* Click **Create a Bot User**

### Inviting the newly created bot to your server

* Go to https://discordapp.com/developers/applications/me
* Where it says **Client ID:**, copy that id
* Go to the following link, but replace `CLIENT_ID_GOES_HERE` with the id that you copied in the previous step: https://discordapp.com/oauth2/authorize?client_id=CLIENT_ID_GOES_HERE&scope=bot&permissions=1342532672

### Configuring discord emojis

The bot requires 3 emojis to work properly, and if you don't already have them, you need to add them manually.

* In discord, click **Server settings** > **Emoji**
* Press **Upload Emoji**, and select one of the images in the `Emoji`-folder of this project
* Do the same thing with the other two

### Configuring RainPlannerBot

* Go to https://discordapp.com/developers/applications/me
* Where it says **Token:**, click **click to reveal**
* Copy the token, and paste it in the configuration-file of the Bot
* Change other properties in the config to your liking

## Running RainPlannerBot

* Download and install [Visual studio Community 2017](https://www.visualstudio.com/downloads/)
* Start Visual Studio, and open `RaidPlannerBot.sln`
* Press the green play-button
* If the AppConfig.json isn't setup properly, it will tell you how to do so

### If you want to compile your own binary

* Open the **Build**-menu
* Select **Publish RaidPlannerBot**
* Create a new profile, selecting the output-directory you would like
* On **Target Location** > **Settings...**, make sure **Target Runtime** is set to `win10-x64`
* Press **Publish**
* The output-directory you selected now contains an exe-file, and everything else needed

## Using RainPlannerBot

The following command are available:

* `!raid pokemon time location` for creating raid-plans. `!r` is the same thing as `!raid`.
* `!exraid pokemon time day location` for creating ex-raid-plans. `!xr` is the same thing as `!exraid`.
* `!edit id time` for editing the time of a raid.

### Creating a raid plan

Type `!raid pokemon time location` in a channel where the bot is allowed, for example `!raid latias 15:00 the gym at central station`. The plan will automatically be deleted 2 hours after creation (or the number of minutes set in `AppConfig.json`)

### Creating an EX-raid plan

Type `!exraid pokemon time day location` in a channel where the bot is allowed, for example `!raid mewtwo 15:00 23/4 the gym at central station`. The plan will automatically be deleted 10 days after creation (or the number of days set in `AppConfig.json`)

### Attending a raid

If you are going to attend the raid, press the reaction corresponding to your faction. If you bring extra devices, also press the number-reaction that matches the number of extra devices you are bringing.

### Change planned time

To edit the time of an existing plan, type `!edit id time`, where `id` is the id that the bot have assigned to the plan, for example `!edit 7624 16:00`

### Delete raid plan

Only the creator of the plan can delete it, and it is done by pressing the red-cross-reaction. Plans will however be automatically deleted depending on what type it is and the values set in `AppConfig.json`

## Support

I highly recommend that you build your own executable using the code in this repository. If you are not able to, you can use [this build](https://www.dropbox.com/s/ld5juvnw3hxxh33/RaidPlannerBot.zip?dl=0), but it is not guaranteed to be 100% up-to-date with the last commit.

For other questions, tips or comments, join the discord server at https://discord.gg/hSwJNAb!

