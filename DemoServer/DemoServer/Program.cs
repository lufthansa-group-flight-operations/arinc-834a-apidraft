using DemoServer.DataAccess;
using DemoServer.Formatter;
using DemoServer.Services.acars;
using DemoServer.Services.adif;
using DemoServer.WebSockets;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(
    opt =>
    {
        opt.RespectBrowserAcceptHeader = true;
        opt.OutputFormatters.Insert(0, new AvionicParameterOutputFormatter());
    })
    //.AddXmlSerializerFormatters()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add a single Data Source service to the Service Repository
builder.Services.AddSingleton<IAvionicDataSource, AvionicDataSourceEmulator>();

// Define the Acars Emulator to be used for the ACARS interface.
builder.Services.AddSingleton<IAcarsMessageService, AcarsMessageServiceEmulator>();

// Add a WebSocketClientHandler as a "Transient" Service to the Service Repository,
// which means, that it delivers a separate Handler for each client.
builder.Services.AddTransient<IWebSocketClientHandlerAcParameter, WebSocketClientHandlerAcParameter>();
builder.Services.AddTransient<IWebSocketClientHandlerAcars, WebSocketClientHandlerAcars>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// Create and define WebSocket options deviation from default
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(5)    
};

// Add WebSocket functionality with above settings.
app.UseWebSockets(webSocketOptions);

// This allows to access the /Test.html
app.UseStaticFiles();
app.UseRouting();

// May be required to disable behind reverse proxy
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();