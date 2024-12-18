using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using SS.Mancala.API.Hubs;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Ui.MsSqlServerProvider;
using Serilog.Ui.Web;
using SS.Mancala.API.Services;
using System.Reflection;
using WebApi.Helpers;
using SS.Mancala.PL.Data;
using SS.Mancala.BL;


public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.


        builder.Services.AddSignalR()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            });


        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Mancala API",
                Version = "v1",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Brittany Young",
                    Email = "500122975@fvtc.edu",
                    Url = new Uri("https://www.fvtc.edu")
                }

            });

            var xmlfile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlpath = Path.Combine(AppContext.BaseDirectory, xmlfile);
            c.IncludeXmlComments(xmlpath);

        });




        // Add Connection information
        builder.Services.AddDbContextPool<MancalaEntities>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("MancalaConnection"));
            //options.UseSqlServer(connectionString);
            options.UseLazyLoadingProxies();
        });

        // configure strongly typed settings object
        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
        // configure DI for application services
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<MoveManager>();
        builder.Services.AddScoped<GameManager>();
        builder.Services.AddScoped<PitManager>();


        string connection = builder.Configuration.GetConnectionString("MancalaConnection");

        builder.Services.AddSerilogUi(options =>
        {
            options.UseSqlServer(connection, "Logs");
        });

        var configSettings = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();


        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configSettings)
            .CreateLogger();

        builder.Services
            .AddLogging(c => c.AddDebug())
            .AddLogging(c => c.AddSerilog())
            .AddLogging(c => c.AddEventLog())
            .AddLogging(c => c.AddConsole());

        var app = builder.Build();

        app.UseSerilogUi(options =>
        {
            options.RoutePrefix = "logs";
        });

        Log.Warning("API Program");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || true)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // custom jwt auth middleware
        app.UseMiddleware<JwtMiddleware>();

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        //app.MapControllers();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<MancalaHub>("/mancalaHub");
        });

        app.Run();
    }

    public static async Task<string> GetSecret(string secretName)
    {
        try
        {
            var keyVaultName = "kv-101521081-500122975";
            var kvUri = $"https://{keyVaultName}.vault.azure.net";

            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            var secret = await client.GetSecretAsync(secretName);
            Console.WriteLine(secret.Value.Value.ToString());
            return secret.Value.Value.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

}