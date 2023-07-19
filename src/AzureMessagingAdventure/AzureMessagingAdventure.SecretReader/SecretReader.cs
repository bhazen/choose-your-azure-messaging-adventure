using Microsoft.Extensions.Configuration;

namespace AzureMessagingAdventure.Secrets
{
    public static class SecretReader
    {
        public static SettingsType? ReadUserSecrets<AssemblyType, SettingsType>(string sectionName) 
            where AssemblyType : class
            where SettingsType : class
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<AssemblyType>()
                .Build();

            return config.GetSection(sectionName).Get<SettingsType>();
        }
    }
}