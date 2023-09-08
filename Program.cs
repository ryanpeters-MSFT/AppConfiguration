using Azure.Data.AppConfiguration;
using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

// toggle the current env (pull from env variable)
var environment = "development";

#region setup

var builder = WebApplication.CreateBuilder(args);

// use an environment variable for this
var connection = builder.Configuration["AppConfigurationConnection"];

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(connection);

    // load all settings in addition to those with the label matching the environment
    options.Select("*").Select("*", environment);

    options.UseFeatureFlags(options =>
    {
        // default is 30 secs
        //options.CacheExpirationInterval = TimeSpan.FromSeconds(60);

        // filter flags based on flag name and label
        //options.Select("TestApp:*", environment);
    });

    options.ConfigureKeyVault(options => 
    {
        // use an environment variable for this
        var keyVaultEndpoint = builder.Configuration["KeyVaultEndpoint"];

        // for using keyvault, a proper credential is required
        options.SetCredential(new DefaultAzureCredential());

        //options.SetSecretRefreshInterval(TimeSpan.FromSeconds(60));
    });

    var refresher = options.GetRefresher();
});

// handle creation of a ConfigurationClient for managing keys
builder.Services.AddTransient(sp => new ConfigurationClient(connection));

// info: https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-feature-flag-aspnet-core?tabs=core6x
builder.Services.AddFeatureManagement();

builder.Services.Configure<ConfigSettings>(builder.Configuration.GetSection("configSettings"));

var app = builder.Build();

#endregion

// get our services
var configClient = app.Services.GetService<ConfigurationClient>();
var configuration = app.Services.GetService<IConfiguration>();

#region output key/values from IConfiguration

// local and app configuration settings
Console.WriteLine($"Environment: {environment}");
Console.WriteLine($"LocalSetting: {configuration["LocalSetting"]}");
Console.WriteLine($"TestSetting: {configuration["TestSetting"]}");
Console.WriteLine($"Services/ClientServiceApi in {environment}: {configuration["Services/ClientServiceApi"]}");
Console.WriteLine($"Super secret PW from key vault: {configuration["DbPassword"]}");

#endregion

#region handle strongly-typed IOption<T> config types

// strongly-typed configuration settings
var configSettings = app.Services.GetService<IOptions<ConfigSettings>>();

Console.WriteLine($"Using typed config: API {configSettings.Value.Name} ({configSettings.Value.Id}) has value {configSettings.Value.Value}");

#endregion

#region use feature flags

// using feature management flags
var featureManager = app.Services.GetService<IFeatureManager>();

var encodeAllValues = await featureManager.IsEnabledAsync("EncodeAllValues");

Console.WriteLine($"Feature {nameof(encodeAllValues)} enabled? {encodeAllValues}");

#endregion

#region modify settings using ConfigurationClient

var key = "Apis/PetStoreApiUrl";
var value = "http://services.prod.company.lan/pets";

//var response = configClient.AddConfigurationSetting(key, value, environment);
var setResponse = configClient.SetConfigurationSetting(key, value, environment);
var deleteResponse = configClient.DeleteConfigurationSetting(key, environment);

#endregion