using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ShopUtils;

namespace ItemLibrary.Wrappers
{
    internal static class ShopUtilsCompatibility
    {
        private static bool? _enabled;

        public static bool Enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("hyydsz-ShopUtils");
                }
                return (bool)_enabled;
            }
        }

        public static int ShopUtilsRegisteredEntries
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            get
            {
                /*if (Enabled == false) Complains if we do this, we need to do manual checks
                    return 0;*/
                ItemPlugin.Logger.LogDebug("ShopUtils is present");
                FieldInfo? entries = typeof(Entries).GetField("registerEntries", BindingFlags.Static | BindingFlags.NonPublic);

                if (entries == null)
                {
                    ItemPlugin.Logger.LogError("Getting registerEntries field returned null!");
                    return 0;
                }

                object registeredEntriesObject = entries.GetValue(null);
                var registeredEntries = (List<Type>)registeredEntriesObject;
                return registeredEntries.Count();
            }
        }
    }
}
