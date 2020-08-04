# BroadCastCompressor
## Description
This console application is intended to compress large video files into mp4 files using FFMPEG tools and FFMpegCore C# wrapper

## Prerrequisites
- Install FFMPEG in the computer you will be compressing files in. You can get the binaries from this page: https://ffmpeg.org/download.html#build-windows
- Add FFMPEG "bin" folder to the PATH variable (Linux or Windows). Check this link as a guidance: https://www.java.com/en/download/help/path.xml
- Install .NET Core 3.1. Download it from this URL: https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.1.302-windows-x64-installer

## NUGet Packages
 - Check the csproj file to check which dependencies will be required to build this console application As a summary you will need: NLog, FFMpegCore, related dlls
 
## Configurations
  - Check and modify appSettings.json to configure this console application accordingly
  - Here's a sample configuration content:
  
  {
    "OperatorNumber": 1,
    "RecordingFolder": "D:\\Work\\SolucionesTecnologicasLL\\ParticipacionCiudadana\\MonitoreoMedios2020\\testGrabacionFolder",
    "CompressedFilesDestinationFolder": "D:\\Work\\SolucionesTecnologicasLL\\ParticipacionCiudadana\\MonitoreoMedios2020\\testDestinoFolder",
    "FilesToCompress": ["*.ts" ]
  }
  
  - The following configurations have to be configured prior to begin the compression process:
    * OperatorNumber --> The number of operator (from 1 to 6)
    * RecordingFolder --> The folder which the console application will take the uncompressed files to be processed
    * CompressedFilesDestinationFolder --> The folder that has the resulting and compressed files
    * FilesToCompress --> Array of all the types of files that will be compressed. We just need to describe the extension along . E.g.: "*.mpg", "*.ts", "*.avi"
    
## How to run
  - Place the binaries in a folder called "BroadCastCompressor" into the Program Files folder
  - Open a cmd and run it (be careful on configuring the application properly before run it)
  - That's it !
