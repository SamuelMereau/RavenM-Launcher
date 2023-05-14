Publish release:

- Windows:
dotnet publish -c Release -r win-x64

- Mac
dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true
