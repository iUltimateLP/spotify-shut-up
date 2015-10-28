# spotify-shut-up
This is a little handy Spotify ad-blocker written in C#. The work it does is simple: Once there is an ad, it mutes the output
of the Spotify Application in your Windows Sound Mixer.

# Features
 + **No GUI needed**   
   It works within a console application, which a few simple commands.   
 + **Hidable**  
   The application is designed to be as quiet as possible. It starts in the tray and can be restored via a tray icon.
 + **Technicially not illegal**   
   The application works like a normal ad-blocker. Where ads should be, arent ads. In terms of audio, what this app does is, it
   mutes the Spotify application in your Windows Sound Mixer, so instead of 30 seconds ad you hear 30 seconds silence. It is
   the same as you would go ahead and mute Spotify every time an ad is played, so they're earning money anway :joy:
 + **Multiple sound card support**   
   If you are a person like me, hearing Spotify via an external sound card for hearing music with the stereo, this is right
   for you aswell. Whereas other Spotify ad blockers only mute Spotify at the default output device, this mutes Spotify
   on every sound output device.
   
# Libraries
 + This software uses the SpotifyAPI written by JohnnyCrazy (https://github.com/JohnnyCrazy/SpotifyAPI-NET)   
 + This software uses the NAudio libraries written by the NAudio team (https://github.com/naudio/NAudio)   
 **NOTE: I did not made any of those libraries myself, all credits belong to JohnnyCrazy and the NAudio team.**
