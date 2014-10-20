Sky Jukebox
===========

Compact, powerful music player. (Currently WIP = not-so-powerful)

Uses 3rd-party libraries NAudio, NVorbis, TagLib#, FlacBox, LAME, Managed ListView.

System Requirements:
--------------------
  * Microsoft Windows Vista/7/8/8.1, 32/64-bit (works with Aero Glass mod for Win8.1)
  * Microsoft .NET Framework 4.5.1
  * Note 1: I have no intention of supporting Windows XP.
  * Note 2: Sadly, WPF is not cross-platform, nor is there any cross-platform implementation or alternative to it. Therefore there might not be a cross-platform version for a long, long time.

Notable features: (List is not final)
-----------------
  * Open a playlist in one click and play it. Supports M3U, M3U8.
  * Use the Playlist Editor to add songs to the list and organize them, and save the playlist in M3U format.
  * Compact Aero Glass WPF GUI (stays on top). Each button has a tooltip which describes it.
  * Dynamically change the color of the icons!
  * You can now choose the playback device!
  * NEW: Translucent playback progress display with customizable color!
  * You can minimize the player to the system tray to continue playback in the background.
    Double-click the system tray icon to open it again or right-click it for more options.
    Tray icon does not remain visible after exiting!
  * Correct shuffle mode: move backwards and forward within the temporarily shuffled list
  * Supports almost all major audio formats: Wave, MP3, Ogg-Vorbis, WMA, AIFF, M4A, FLAC(16bit) and more!
  * NEW: Codec plugin support! Developers can add support for other audio formats! Implement ICodec for NAudio-based playback, implement IAudioPlayer for other playback (only in versions from Oct. 11 onwards).
  * NEW: Skin support! You can create a skin in XML format, editor coming soon.
  * WORK IN PROGRESS: A lot of stuff planned. Some examples: MIDI playback, High-resolution mode, Minecraft intergration plugin / mod, more customization options, localization support, more playback features, ID3 tag / metadata editing, convert audio ...

Alpha version download (built 20-Oct-2014): http://www.mediafire.com/download/sass8a5qjcbv2so/Sky_Jukebox_v0.9.0_Alpha2.0_patched1_(20-Oct-2014).zip
This version has the new playback, icon, and skin engines as well as the playback progress display.
Patch 1: Fixed blocked dlls not loading on Win8.1+

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