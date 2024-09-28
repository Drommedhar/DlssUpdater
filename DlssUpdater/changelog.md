# 1.0.4.0
* Add GOG library support

# 1.0.3.0
* Streamline games ui (no unnecessary popups, easier readable buttons)
* Make libraries configurable and allow manually setting the installation directory
* Change behaviour if more than one instance of the same dll is found in game directory. It will selected the last one found. This might result in the wrong one being used, so make sure that not multiple instances are inside a game folder.

# 1.0.2.0
* Add notification icon if DLSS updates are available
	* If a specific type has no version installed, no notification will be shown, as not everyone will use Ray Reconstruction or Frame Gen
* Show information if a game has an update available (in Navigation view and game itself)
* Automatically detect changes to dlls from installed games
* Show installed DLSS versions on Game box on mouse hover

# 1.0.1.0
* Add logging to narrow down common problems. These can be found in 'logs' directory next to the executable
* Window now stores if it was previously maximized or not
* Will now correctly always use dark theme no matter what the windows setting is set to

# v1.0.0.2
* Reduce archive footprint by not bundling the .NET Framework with the application archive

# v1.0.0.1
* Add changelog window, which will automatically be shown after update

# v1.0.0.0
* Initial release