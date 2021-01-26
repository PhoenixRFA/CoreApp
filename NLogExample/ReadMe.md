##Описание
Применение поставщика логирования - NLog

[Wiki](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-3)

##Установка
1. Install Nuget package: NLog.Web.AspNetCore 4.9+
2. Create nlog.config file
3. Set in config properties "Copy to bin folder" to "Copy if newer"
4. Add NLog in Program.cs
5. Configire appsetting.json
PROFIT

Logger installed and can be used as DI (ILogger<SomeCategory>)