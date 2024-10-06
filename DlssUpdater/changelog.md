# 2.0.0.0
* Completely redesigned user interface
* Several bugfixes while creating new user interface
* New option "Show notifications" to hide notification bubbles
* Several settings removed that not really served a purpose but induced bugs
* Dev Note: 4 days of "free time" invested into the new user interface, hope you like it as much as I do :D

# 1.0.6.1
* Fix incorrect SHA256 check

# 1.0.6.0
* Add saving of default dlls if applying new version and a backup wasn't done
* Fix File Watcher running on startup while gathering all games leading to race conditions
* Switch MD5 check to SHA256 check
* Better check if downloaded DLL is properly signed

# 1.0.5.1
* Fix incorrectly enabled ComboBox for DLL types in configuration view
* Fix AsyncFileWatcher not correctly removing removed games

# 1.0.5.0
* Add support for hiding games from main list with right mouse button
* Add filter to games page
* Validate MD5 hash and signature of downloaded DLL
* Fix crash if image for manually added game is no longer available

# 1.0.4.1
* Fix bug with image on manual added game which would not remove text
* Fix selecting same game after closing configuration page

# 1.0.4.0
* Add GOG library support
* Add Epic Games library support
* Add Xbox library support

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