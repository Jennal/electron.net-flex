# Electron with .Net Core flex

[Electron.Net](https://github.com/ElectronNET/Electron.NET) is a great project, but too heavy for me.

This project is totally inspired by [Electron.Net](https://github.com/ElectronNET/Electron.NET).

I am familiar with Web tech (html/css/javascript) and C#. When I decide to build a desktop project, I would like to use these tech. Then I found [Electron.Net](https://github.com/ElectronNET/Electron.NET). It is great, but can only work with ASP.NET base project, which means bad startup time, maybe 10s+. It is not acceptable for me. And the other problem is, you can't edit electron project main.js. It is not flexible for me. So I decide to build a more flexible project to do the same thing.

## Structure

- [Electron](https://github.com/electron/electron) : UI Display, parent process of C#, C# talk to electron by `stdout`, which means C# can call electron nodejs code
- [WatsonWebServer](https://github.com/jchristn/WatsonWebserver) : Web Content/API Provider
- [WatsonWebSocket](https://github.com/jchristn/WatsonWebsocket) : Communicate between Web Content and CSharp Logic, which means Web Content can call CSharp code, and CSharp code can call Web Content javascript code

All features mentioned above are still under construction.

## Requirement

- [dotnet core 5.0](https://dotnet.microsoft.com/)
- [nodejs](https://nodejs.org/en/download/)
- npm (included in nodejs)

## License

MIT-licensed

Enjoy!