Sky Jukebox
===========

Compact, powerful music player. (Currently WIP = not-so-powerful)

Uses 3rd-party libraries NAudio, NAudio.Wma, NAudio.Flac, NVorbis, NVorbis.NAudioSupport, TagLib#, Extended WPF Toolkit.

Official blog: http://orondf343.t15.org

System Requirements:
--------------------
  * Microsoft Windows Vista/7/8/8.1, 32/64-bit (compatible with Aero Glass mod for Win8.1)
  * Microsoft .NET Framework 4.5.1 / 4.5.2
  * Note 1: I have no intention of supporting Windows XP.
  * Note 2: Sadly, WPF is not cross-platform, nor is there any cross-platform implementation or alternative to it. Therefore there might not be a cross-platform version for a long, long time.

Notable features: (List is as of Alpha4)
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

OUTDATED Alpha version download (built 05-Nov-2014): http://www.mediafire.com/download/b7btwb180f2xcur/Sky_Jukebox_v0.9.0_Alpha3.0-s2_05-Nov-2014.zip

Changes in Alpha3.0-s2: More breaking changes
Fixed shuffle
Fixed explorer context menu thingy
Added option to restart as Admin
Added support for loading folders from ClArgs

Contact me:
-----------

Website: http://orondf343.t15.org
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
