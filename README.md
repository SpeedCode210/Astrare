# ![Astrare](https://raw.githubusercontent.com/SpeedCode210/Astrare/main/Astrare/astrareLogo.png)

[![Copr build status](https://copr.fedorainfracloud.org/coprs/raoufouldali/astrare/package/astrare/status_image/last_build.png)](https://copr.fedorainfracloud.org/coprs/raoufouldali/astrare/package/astrare/)

Various tools for amateur astronomers !

## Features

### Showing aster ephemerides

![Aster ephemerides graph](https://github.com/SpeedCode210/Astrare/blob/main/ScreenshotApp.png?raw=true)

### Showing moon phase

![Aster ephemerides graph](https://github.com/SpeedCode210/Astrare/blob/main/ScreenMoon.png?raw=true)

### Listing events of the day

![Aster ephemerides graph](https://github.com/SpeedCode210/Astrare/blob/main/ScreenEventsShow.png?raw=true)

### Exporting PDF with the events of the month/year

![Aster ephemerides graph](https://github.com/SpeedCode210/Astrare/blob/main/ScreenEvents.png?raw=true)

### Exporting PDF with all informations of the day

![Aster ephemerides graph](https://github.com/SpeedCode210/Astrare/blob/main/ScreenPDF.png?raw=true)

### Exporting graphic almanach for the year

![Aster ephemerides graph](https://github.com/SpeedCode210/Astrare/blob/main/ScreenAlmanach.png?raw=true)

## Installation

### On Linux distributions based on Fedora Linux from Fedora Copr

```
sudo dnf copr enable raoufouldali/astrare
sudo dnf install astrare
```

### Other systems

Just get the .exe/.rpm/.deb/.tar.gz from the last release and install it, or compile directly from source.

## Dependencies

- Windows 7+ / Any Linux distribution 
- Python 3.7 or later
- Kosmorrolib Python library (automatically installed by Astrare)

## Used libraries

- [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia) for making the GUI.
- [Kosmorro Lite](https://github.com/SpeedCode210/kosmorro-lite) Made by [SpeedCode210](https://github.com/SpeedCode210/) for interfacing with Kosmorrolib.
- [Kosmorrolib](https://github.com/kosmorro/lib) Made by [Jérôme Deuchnord](https://github.com/Deuchnord) for getting ephemerides and events.
- [VectSharp](https://github.com/arklumpus/VectSharp) Made by [Giorgio Bianchini](https://github.com/arklumpus) for generating graphs.
- [iText 7 Community](https://github.com/itext/itext7-dotnet) for generating PDFs.
