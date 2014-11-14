Sky Jukebox
===========

Compact, powerful music player. (Currently WIP = not-so-powerful)

Uses 3rd-party libraries NAudio, NVorbis, TagLib#, FlacBox, LAME, Managed ListView.

System Requirements:
--------------------
  * Microsoft Windows Vista/7/8/8.1, 32/64-bit (works with Aero Glass mod for Win8.1)
  * Microsoft .NET Framework 4.5.1 or 4.5.2
  * Note 1: I have no intention of supporting Windows XP.
  * Note 2: Sadly, WPF is not cross-platform, nor is there any cross-platform implementation or alternative to it. Therefore there might not be a cross-platform version for a long, long time.

Notable features: (List is not final)
-----------------
  * Open a playlist in one click and play it. Supports M3U, M3U8.
  * Use the Playlist Editor to add songs to the list and organize them, and save the playlist in M3U format.
  * Compact Aero Glass WPF GUI (stays on top). Each button has a tooltip which describes it.
  * Dynamically change the color of the icons and the background color!
  * You can now choose the playback device!
  * NEW: Translucent playback progress display with customizable color!
  * You can minimize the player to the system tray to continue playback in the background.
    Double-click the system tray icon to open it again or right-click it for more options.
    Tray icon does not remain visible after exiting!
  * Correct shuffle mode: move backwards and forward within the temporarily shuffled list
  * Supports almost all major audio formats: Wave, MP3, Ogg-Vorbis, WMA, AIFF, M4A, FLAC and more! (FLAC support WIP, will support 8/16/24/32bit)
  * NEW: Codec plugin support! Developers can add support for other audio formats! For non-NAudio playback - implement IAudioPlayer, for NAudio playback - implement ICodec, then add the attribute ExtensionAttribute.
  * NEW: Skin support! You can create a skin in XML format, editor coming soon.
  * Explorer context menu intergration with custom text!
  * WORK IN PROGRESS: A lot of stuff planned. Some examples: MIDI playback, High-resolution mode, Minecraft intergration plugin / mod, more customization options, localization support, more playback features, ID3 tag / metadata editing, convert audio ...

Alpha version download (built 05-Nov-2014): http://www.mediafire.com/download/b7btwb180f2xcur/Sky_Jukebox_v0.9.0_Alpha3.0-s2_05-Nov-2014.zip

Changes in Alpha3.0-s2: More breaking changes
Fixed shuffle
Fixed explorer context menu thingy
Added option to restart as admin
Added support for loading folder from ClArgs

Changes in Alpha3.0: HUGE behind the scenes changes and some more NEW stuff:
Added background color setting
Improved personalization window
Added defaults for all settings
Made Sky Jukebox single-instance per user
Refactored most of the code
MiniPlayer now forces use of Aero theme, removed white border on buttons
Made the code of MiniPlayer more modular, moved all main startup code to App
Cleaned up Icon and Xml code
New icon!
Fixed crash and incorrect behaviour when reaching the end of a file
Fixed redundant stuff in Next and Previous
Fixed bugs with skin setting and default settings
Fixed personalization not taking effect
Fixed crash when saving settings to a new file (workaround for File.Create bug)
Fixed build paths, cleaned up build configurations
Prevent crash when saving settings to null path


License:
--------

	Sky Jukebox: Compact, powerful music player
	Copyright (C) 2014 OronDF343
	Contact: orondf343@gmail.com
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
