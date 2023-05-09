using System;
using System.IO;
using System.Linq;

namespace ConfigureProject
{
    public abstract class XRPlatformSettings<T>
    {
        protected abstract void ConfigureXr(T platformSettings);

        public static void Configure(T platformSettings)
        {
            var settingsList = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(XRPlatformSettings<T>).IsAssignableFrom(p) && !p.IsAbstract).ToList();

            foreach (var obj in settingsList.Select(type => Activator.CreateInstance(type) as XRPlatformSettings<T>)
                .Where(obj => obj != null))
            {
                obj.ConfigureXr(platformSettings);
            }
        }
    }
}