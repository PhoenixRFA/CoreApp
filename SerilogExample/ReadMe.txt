https://github.com/serilog/serilog/wiki/Getting-Started

1. Install Nuget package: Serilog.AspNetCore, Serilog.Sinks.*
2. Configire appsetting.json

2. Create nlog.config file
3. Set in config properties "Copy to bin folder" to "Copy if newer"
4. Add NLog in Program.cs
PROFIT

Logger installed and can be used as DI (ILogger<SomeCategory>)