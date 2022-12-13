using System.Collections.Generic;

namespace com.unity.cliprojectsetup
{
    public abstract class PlatformConfig<T>
    {
        public PlatformConfig() => ConfigManager<T>.Instance.AddConfig(this);

        public abstract void Configure(T platformSettings);
    }

    public class ConfigManager<T>
    {
        List<PlatformConfig<T>> platformConfigs;

        public static ConfigManager<T> Instance = new ConfigManager<T>();

        private ConfigManager() => platformConfigs = new List<PlatformConfig<T>>();

        public void Configure(T platformSettings)
        {
            // Remove any empty configs
            platformConfigs.RemoveAll(config => config == null);

            // Loop through all the knock configs
            foreach (var config in platformConfigs)
            {
                config.Configure(platformSettings);
            } 
        }

        public void AddConfig(PlatformConfig<T> platformConfig) => platformConfigs.Add(platformConfig);
    }
}