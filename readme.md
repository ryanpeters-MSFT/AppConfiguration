# Azure App Configuration
This is a quick example of using the Azure App Configuration SDK and client to plug configuration data into your existing `IConfiguration` instance with minimal changes required to the code. 

Replace the `AppConfigurationConnection` key with your App Configuration store connection string. Optionally, if you have any keys that link to KeyVault, set the `KeyVaultEndpoint` key/value as well. 

In addition, it briefly shows how to use `ConfigurationClient` to manipulate keys/values in your configuration store.