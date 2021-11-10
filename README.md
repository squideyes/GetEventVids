The  **GetEventVids** application should be should be fairly easy to figure out and use, but just in case it isn't, the following should help.

GetEventVids is a WPF Windows desktop app that downloads and/or stream hi-res "event" videos from the Microsoft Channel 9 website.  To use the app, you'll need to compile it first (using Visual Studio 2022 and .NET 6), and then 

- **Select an event** from the events drop-down (VisualStudio2022, DotNetConf2021, etc.) 
- **Find one or more session** that you'd like to watch
  - To **filter the sessions shown**, enter a search term into the "Enter Filter-Text (for Title or Talent)" text-box and/or select "Selected" or "Favorites" in the filter drop-down (next to the Download button).
  - It's a good idea to click the heart-shaped **Favorite** button next to all of the videos you're interested in and then to select the "Favorites" item in the Filter combo-box.  With dozens of videos to stream and/or download, filtering the grid to only show your favorites makes the entire process much easier.

- Click on a video's **Info** button to display related show notes, links and presentations, etc.
- Click on a video's  **Stream** button to Stream a given session's video in hi-res.
- Click on a video's  **Play** button to play the video in your default media player (this only works if you've previously downloaded the video)
- To **download a video**, click it's checkbox (next to the Play button) and then click the Download button to save the video to your "**\{MyDocuments}\GetEventVids**" folder.  When the download is finished, the video's Play button will light up and may be clicked. *NOTE: Multiple videos may be downloaded at a single go.*
                
For additional info and/or source code, please visit the GetEventVids project site at **<a href="http://github.com/squideyes/GetEventVids" target="_blank">GitHub</a>**. To contact the author, please feel free to email <a href="mailto:louis_berman@epam.com">louis_berman@epam.com</a>.  BTW, improvements (via **pull requests**) or even suggestions would be very much appreciated!
