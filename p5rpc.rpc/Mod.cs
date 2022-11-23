using p5rpc.rpc.Configuration;
using p5rpc.rpc.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using DiscordRPC;
using p5rpc.rpc;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using System.Diagnostics;
using p5rpc.lib.interfaces;

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

        private Timer _timer;

        private IP5RLib _p5rLib;
        private IFlowCaller _flowCaller;

        private Field[]? _fields;
        private Event[]? _events;
        private Dictionary<string, string> _imageText;


        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;

            Utils.Initialise(_logger, _configuration);

            var libController = _modLoader.GetController<IP5RLib>();
            if (libController == null || !libController.TryGetTarget(out _p5rLib!))
            {
                Utils.LogError("Could not get p5r library, please make sure you have p5rpc.lib installed.");
                return;
            }
            _flowCaller = _p5rLib.FlowCaller;

            _fields = Utils.LoadFile<Field[]>("fields.json", _modLoader.GetDirectoryForModId(_modConfig.ModId));
            _events = Utils.LoadFile<Event[]>("events.json", _modLoader.GetDirectoryForModId(_modConfig.ModId));
            _imageText = Utils.LoadFile<Dictionary<string, string>>("imageText.json", _modLoader.GetDirectoryForModId(_modConfig.ModId)) ?? new Dictionary<string, string>();
            if (_fields == null || _events == null)
                return;

            _client = new DiscordRpcClient("1032265834111975424");
            _client.Initialize();

            _timer = new Timer(Update, null, 0, 5000);
        }

        private void Update(object? state)
        {
            Utils.LogDebug("Updating presence");
            RichPresence presence = new RichPresence();
            presence.Assets = new Assets();

            ProcessField(presence);
            ProcessEvent(presence);

            _client.SetPresence(presence);
        }

        private void ProcessField(RichPresence presence)
        {
            if (!_flowCaller.Ready())
                return;
            int fieldMajor = _flowCaller.FLD_GET_MAJOR();
            int fieldMinor = _flowCaller.FLD_GET_MINOR();

            Field? field = _fields.FirstOrDefault(f => f.Major == fieldMajor && f.Minor == fieldMinor);

            Utils.LogDebug($"In field {fieldMajor}_{fieldMinor} ({(field != null ? field.Name : "undocumented")})");

            string imageKey = "logo";
            string imageText = "P5R Logo";
            string description = "Roaming somewhere";
            if (field != null)
            {
                if (field.ImageKey != null)
                {
                    imageKey = field.ImageKey;
                    imageText = _imageText.ContainsKey(imageKey) ? _imageText[imageKey] : "No image text found :(";
                }
                description = field.Description;
                if (field.State != null)
                    presence.State = field.State;
            }
            presence.Assets.LargeImageText = imageText;
            presence.Assets.LargeImageKey = imageKey;
            presence.Details = description;

            if (field != null && field.InMetaverse)
                ProcessMetaverse(presence, field);

            if (field != null && field.InBattle)
                ProcessBattle(presence, field);            
        }

        private void ProcessBattle(RichPresence presence, Field field)
        {

        }

        private void ProcessMetaverse(RichPresence presence, Field field)
        {
            
        }

        private void ProcessEvent(RichPresence presence)
        {
            var sequence = _p5rLib.Sequencer.GetSequenceInfo();
            if (sequence.EventInfo.InEvent())
            {
                Event eventInfo = _events.First(e => e.Major == sequence.EventInfo.Major && e.Minor == sequence.EventInfo.Minor);
                Utils.LogDebug($"In event {sequence.EventInfo.Major}_{sequence.EventInfo.Minor} ({(eventInfo != null ? eventInfo.Name : "undocumented")})");
                presence.Details = eventInfo.Description;
                if (eventInfo.State != null)
                    presence.State = eventInfo.State;
                if (eventInfo.ImageKey != null)
                {
                    presence.Assets.LargeImageKey = eventInfo.ImageKey;
                    presence.Assets.LargeImageText = _imageText.ContainsKey(eventInfo.ImageKey) ? _imageText[eventInfo.ImageKey] : "No image text found :(";

                }
            }
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