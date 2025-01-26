** This application is now longer mentained. Please switch to DlssSwapper which basically now has the same functionality. **

![Dlss Updater logo](docs/images/DLSS_Updater_Logo.png)

# Download
[<img src="docs/images/msstore.png">](https://apps.microsoft.com/detail/9P1NDFBRS95L)

# Introduction

Dlss Updater is an open source software written to make updating any of the DLSS dlls available from nVidia (Upscaling, Ray Reconstruction, Frame Generation) easier for the end user. It allows you to update either the base DLSS dll, the ray reconstruction dll as well as the frame generation dll. This can be used to improve the visuals of games that may use an older version of DLSS and have not been updated to the latest version.
It also allows the user to easily switch back to an older version if desired. It uses TechPowerUp as a source for all available dlls, from which the user could also download the files separately. With Dlss Updater this task is unified in a specific application designed for such a task.

# Build requirements
To build Dlss Updater on your machine, you first need to check out the git repository (or download a zip archive). Dlss Updater was developed using **Visual Studio 2022** and uses the **WPF** ui framework.  In addition, several third-party nuget extensions are used to improve aspects of the software.
At the time of writing, these nugets include:
* [AdonisUI](https://github.com/benruehl/adonis-ui)
* [WpfBindingErrors](https://github.com/bblanchon/WpfBindingErrors)

With this you should be able to compile Dlss Updater.

# Features

## Auto detection
Dlss Updater features automatic game detection from popular game libraries. Currently the following libraries are supported:
* Steam
* Ubisoft Connect
* GOG
* Epic Games
* Xbox

Other libraries may be added in the future.

## AntiCheat detection
Dlss Updater tries to detect common anti-cheat software in games using several methods. This is by no means perfect and may result in invalid detections or none at all. This feature can be configured in the settings page.

Dlss Updater will not prevent you from updating any DLSS dll for such games, but keep in mind that doing so may lead to bans. 

# License
Dlss Updater is free and open source software licensed under the MIT License. You can use it in both private and commercial projects. But you must include a copy of the license in your project.

# Discord
If you have further question feel free to join the official discord [here](https://discord.gg/WShdqSDSvu)

# Known issues
* Multiple instances of the the dll in a game folder can lead to strange behaviour. Make sure that there is only 1 instance in a game.

# Code Signing Policy
> Free code signing provided by [SignPath.io](https://signpath.io), certificate by [SignPath Foundation](https://signpath.org)
