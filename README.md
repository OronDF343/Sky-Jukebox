Sky Jukebox
===========

Compact, powerful music player. (Currently WIP = not-so-powerful)

***Alpha 5 will be released Soon(TM) on the Releases page! Alpha4 can be found below.***

As of Feb. 21, uses the 3rd-party libraries NAudio, NAudio.WindowsMediaFormat, NAudio.Flac (modified code from CSCore), NVorbis, NVorbis.NAudioSupport, Extended WPF Toolkit Community Edition, DirectoryInfoEx, TagLib#, ExifLib, Octokit.

Official blog: ***Moved! New link:*** https://orondf343.wordpress.com/

System Requirements:
--------------------
  * Microsoft Windows Vista/7/8/8.1, 32/64-bit (compatible with Aero Glass mod for Win8.1)
  * Microsoft .NET Framework 4.5.1 / 4.5.2
  * Note 1: I have no intention of supporting Windows XP.
  * Note 2: Sadly, WPF is not cross-platform, nor is there any cross-platform implementation or alternative to it. Therefore there might not be a cross-platform version for a long, long time.

Notable features: (List is as of Alpha4, and needs a rewrite)
-----------------
  * Quick Load feature: One click to open a playlist, files or a folder.
  * Supports almost all major audio formats: Wave, MP3, Ogg Vorbis, WMA, AIFF, M4A, FLAC, etc.
  * Playlist Editor with customizable display and sorting. Asynchronous tag loading for a better user experience.
  * Compact Aero Glass WPF GUI (stays on top, single instance). Each button has a tooltip which describes it.
  * The player can be minimized to the system tray and playback will continue in the background.
    Double-click the system tray icon to open the player window or right-click it for more options.
    Tray icon does not remain visible after exiting!
  * Dynamically change the color of the icons, the background color, and the playback progress display color.
  * Customize the Now Playing text using variables.
  * Support for choosing a specific playback device.
  * API for developers: Create Plugins, AudioPlayers and Codecs.
    Plugins add features accessible to the user
	AudioPlayers implement audio playback using a different engine
	Codecs extend the format support of the NAudio playback engine by adding a new kind of WaveStream
  * Skin support: Create skins in XML format, editor coming soon.
  * Explorer context menu integration with custom text!
  * WORK IN PROGRESS: A lot of stuff planned: MIDI playback, Minecraft integration plugin/mod, image backgrounds, localization support, ID3 tag / metadata editing, convert audio ...

Alpha version download (built 31-Dec-2014): http://www.mediafire.com/download/sfcaforcy7mfvfv/Sky_Jukebox_Alpha_4_(31-Dec-2014).zip

Changes in Alpha 4: (Hopefully this is all of them)
  * NEW: New Playlist Editor! Show/hide columns, sort playlist, correct detection if playlist needs to be saved, improved preformance (async), no scrolling lag.
  * NEW: New settings window! Most of the settings are now accessible. Volume & balance control.
  * NEW: Option to restore MiniPlayer location.
  * NEW: New settings XML engine - editing and resetting work properly now.
  * NEW: Customizable Now Playing text.
  * NEW: QuickLoad widget! Easily load files directly from the MiniPlayer.
  * NEW: Plugins widget! Shows buttons to open Plugin GUIs.
  * CHANGE: Proper FLAC support! Does not crash anymore, any file supported.
  * CHANGE: API overhaul (WIP).
  * CHANGE: Switched to WPF Bindings for everything!
  * CHANGE: Refactoring, refactoring, and more refactoring!
  * CHANGE: Now can load folders from command line.
  * CHANGE: Disable hotkeys by default.
  * CHANGE: Removed MLV.
  * CHANGE: Removed redundant intertop for the windows.
  * CHANGE: Updated NAudio and NVorbis.
  * CHANGE: MusicInfo memory tweaks.
  * CHANGE: Priority set to AboveNormal for better preformance under load.
  * CHANGE: Misc. memory usage and preformance tweaks.
  * FIX: Broken loop mode.
  * FIX: Memory leak/allocation of dialogs.
  * FIX: Crash when adding a folder containing an unsupported file.
  * FIX: Crash when loading an inaccessible subdirectory.
  * FIX: Empty playlist crashes.
  * FIX: Output device not updating immediately.
  * FIX: Track name not updating on shuffle.
  * FIX: Shift key presses "eaten".
  * FIX: Volume & Balance rounding errors.
  * FIX: Command line arguments will now work again.
  * FIX: Handle errors loading plugins, make sure it is the correct architecture.
  * FIX: Recoloring icon now shows pause icon correctly when playing.

Contact me:
-----------

Website: ***Moved! New link:*** https://orondf343.wordpress.com/
E-mail: orondf343@gmail.com
Twitter: @OronDF343

License:
--------

	Sky Jukebox: Compact, powerful music player
	Copyright (C) 2014 OronDF343
	GNU GPL3
	
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.
	
	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.
	
	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
