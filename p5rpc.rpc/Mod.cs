using p5rpc.rpc.Configuration;
using p5rpc.rpc.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using DiscordRPC;
using p5rpc.rpc;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using static p5rpc.rpc.Sequence;
using System.Diagnostics;

namespace p5rpc.rpc
{
    
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public unsafe class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;
    
        private readonly DiscordRpcClient _client;

        private nuint* _sequenceInfoPtr;

        private SequenceInfo* _sequenceInfo;

        private SequenceInfo _lastSequence;

        private Timer _timer;

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;

            Utils.Initialise(_logger, _configuration);

            var startupScannerController = _modLoader.GetController<IStartupScanner>();
            if (startupScannerController == null || !startupScannerController.TryGetTarget(out var startupScanner))
            {
                Utils.LogError("Unable to access startup scanner, please make sure you have Reloaded.Memory.Sigscan installed. Aborting initialisation");
                return;
            }

            _client = new DiscordRpcClient("1032265834111975424");

            startupScanner.AddMainModuleScan("48 89 1D ?? ?? ?? ?? EB ?? 48 8B 1D ?? ?? ?? ?? 48 8B 7B ??", Initialise);
        }

        private void Initialise(PatternScanResult result)
        {
            if(!result.Found)
            {
                Utils.LogError("Unable to find sequence info, abotrting initialisation.");
                return;
            }

            _sequenceInfoPtr = (nuint*)Utils.GetGlobalAddress((nuint)result.Offset + (nuint)Utils.BaseAddress + 3);
            Utils.LogDebug($"Sequence info ptr address: 0x{(nuint)_sequenceInfoPtr:X}");

            _client.Initialize();

            _timer = new Timer(InitTimer, null, 0, 100);
        }

        private void InitTimer(object? state)
        {
            if (*_sequenceInfoPtr == 0)
                return;
            _sequenceInfo = (*(SequenceInfo**)(*_sequenceInfoPtr + 72));
            _timer.Dispose();
            _timer = new Timer(Update, null, 0, 1);
            Utils.LogDebug($"Sequence info address: 0x{*_sequenceInfoPtr + 72:X}");
        }

        private void Update(object? state)
        {
            if (_lastSequence.CurrentSequence == _sequenceInfo->CurrentSequence && _lastSequence.LastSequence == _sequenceInfo->LastSequence && _lastSequence.Field0 == _sequenceInfo->Field0 && _lastSequence.Field3 == _sequenceInfo->Field3 && _lastSequence.Field4 == _sequenceInfo->Field4 && _lastSequence.Field5 == _sequenceInfo->Field5 && _lastSequence.EventInfo == _sequenceInfo->EventInfo) 
                return;
            _lastSequence = *_sequenceInfo;
            Utils.LogDebug($"\nCurr {_sequenceInfo->CurrentSequence}\nLast {_sequenceInfo->LastSequence}\nField 0 {_sequenceInfo->Field0}\nField 3 {_sequenceInfo->Field3}" +
                $"\nField 4 {_sequenceInfo->Field4}\nField 5 {_sequenceInfo->Field5}");
            if (_sequenceInfo->EventInfo != (EventInfo*)0)
            {
                Utils.LogDebug($"Event {_sequenceInfo->EventInfo->Major}_{_sequenceInfo->EventInfo->Minor}");
            }

            _client.SetPresence(new RichPresence()
            {
                Details = "Playing the game",
                State = "Very WIP RPC :D",
                Assets = new Assets()
                {
                    LargeImageKey = "logo",
                    LargeImageText = "P5R Logo",
                    //SmallImageKey = "logo"
                }
            });
        }

        public override void Disposing()
        {
            _client.Dispose();
            base.Disposing();
        }

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}