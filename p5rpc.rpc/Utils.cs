using p5rpc.rpc.Configuration;
using Reloaded.Mod.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace p5rpc.rpc
{
    internal static class Utils
    {
        private static ILogger _logger;
        private static Config _config;
        internal static nint BaseAddress { get; private set; }


        internal static void Initialise(ILogger logger, Config config)
        {
            _logger = logger;
            _config = config;
            using var thisProcess = Process.GetCurrentProcess();
            BaseAddress = thisProcess.MainModule!.BaseAddress;
        }

        internal static void LogDebug(string message)
        {
            if (_config.DebugEnabled)
                _logger.WriteLine($"[Rich Presence] {message}");
        }

        internal static void Log(string message)
        {
            _logger.WriteLine($"[Rich Presence] {message}");
        }

        internal static void LogError(string message, Exception e)
        {
            _logger.WriteLine($"[Rich Presence] {message}: {e.Message}", System.Drawing.Color.Red);
        }

        internal static void LogError(string message)
        {
            _logger.WriteLine($"[Rich Presence] {message}", System.Drawing.Color.Red);

        }
        
        /// <summary>
        /// Gets the address of a global from something that references it
        /// </summary>
        /// <param name="ptrAddress">The address to the pointer to the global (like in a mov instruction or something)</param>
        /// <returns>The address of the global</returns>
        internal static unsafe nuint GetGlobalAddress(nuint ptrAddress)
        {
            return (nuint)(*(int*)ptrAddress) + ptrAddress + 4;
        }

        public static T? LoadFile<T>(string fileName, string modDir)
        {
            try
            {
                 string json = File.ReadAllText(Path.Combine(modDir, fileName));
                 return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception e)
            {
                LogError($"Error loading {Path.GetFileNameWithoutExtension(fileName)} information. Please make sure {fileName} exists in the mod's folder.", e);
            }
            return default(T);
        }
    }
}
