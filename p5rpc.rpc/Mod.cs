using p5rpc.rpc.Configuration;
using p5rpc.rpc.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using DiscordRPC;
using p5rpc.lib.interfaces;
using static p5rpc.lib.interfaces.Sequence;

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

        private Field _lastField;
        private EventInfo? _currentEvent;
        private EventInfo? _lastEvent;
        private List<string> _states = new();
        private bool _stateChanged = false;
        private RichPresence _presence;

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

            _presence = new RichPresence();
            _presence.Timestamps = Timestamps.Now;
            _presence.Assets = new Assets();

            _p5rLib.Sequencer.EventStarted += EventStarted;

            _timer = new Timer(Update, null, 0, 5000);
        }

        private void EventStarted(EventInfo eventInfo)
        {
            _currentEvent = eventInfo;
        }

        private void Update(object? state)
        {
            if (!_flowCaller.Ready())
                return;
            Utils.LogDebug("Updating presence");

            int fieldMajor = _flowCaller.FLD_GET_MAJOR();
            int fieldMinor = _flowCaller.FLD_GET_MINOR();
            Field? field = ProcessField(fieldMajor, fieldMinor);

            var sequence = _p5rLib.Sequencer.GetSequenceInfo();

            if ((fieldMajor != -1 || fieldMinor != -1) && (sequence.CurrentSequence != SequenceType.EVENT && sequence.CurrentSequence != SequenceType.EVENT_VIEWER))
                _currentEvent = null; // Not in an event if it isn't -1_-1

            if (field != null && field.InBattle)
                ProcessBattle(field);

            ProcessEvent();

            if (field != null && (field.Major != -1 || field.Minor != -1) || _currentEvent != null)
                ProcessDetails();
            else
                _presence.State = null;

            _client.SetPresence(_presence);
        }

        private void ProcessDetails()
        {
            DayOfWeek dayOfWeek = (DayOfWeek)_flowCaller.GET_DAYOFWEEK();
            int month = _flowCaller.GET_MONTH();
            int day = _flowCaller.GET_DAY();
            TimeOfDay time = (TimeOfDay)_flowCaller.GET_TIME();
            int weather = _flowCaller.GET_WEATHER();

            if (_stateChanged)
            {
                _stateChanged = false;
                _states.Add("{dateInfo}");
            }

            if (_states.Count != 0)
            {
                switch (_states[0])
                {
                    case "{dateInfo}":
                        _presence.State = $"{dayOfWeek} {time.ToString().Replace("_", " ")} {month}/{day}";
                        break;
                    case "{playerLvl}":
                        _presence.State = $"Joker level {_flowCaller.GET_PLAYER_LV(1, 0)}";
                        break;
                    case "{party}":
                        _presence.State = GetPartyState();
                        break;
                    default:
                        _presence.State = _states[0];
                        break;
                }
                _states.Add(_states[0]);
                _states.RemoveAt(0);
            }
        }

        private Field? ProcessField(int fieldMajor, int fieldMinor)
        {
            Field? field = _fields.FirstOrDefault(f => f.Major == fieldMajor && f.Minor == fieldMinor);

            Utils.LogDebug($"In field {fieldMajor}_{fieldMinor} ({(field != null ? field.Name : "undocumented")})");

            string imageKey = "logo";
            string imageText = "P5R Logo";
            string description = "Roaming somewhere";
            if (field != null)
            {
                if (_lastField == null || (_lastField.Major != fieldMajor || _lastField.Minor != fieldMinor))
                {
                    _stateChanged = true;
                    _states.Clear();
                    if (field.State != null)
                        _states.Add(field.State);
                    if (field.InMetaverse)
                    {
                        _states.Add("{party}");
                        _states.Add("{playerLvl}");
                    }
                }
                _lastField = field;
                if (field.ImageKey != null)
                {
                    imageKey = field.ImageKey;
                    imageText = _imageText.ContainsKey(imageKey) ? _imageText[imageKey] : "No image text found :(";
                }
                description = field.Description;
            }
            _presence.Assets.LargeImageText = imageText;
            _presence.Assets.LargeImageKey = imageKey;
            _presence.Details = description;

            return field;
        }

        private void ProcessBattle(Field field)
        {
            int encountId = _flowCaller.FLD_GET_ENCOUNTID(0);
            Utils.LogDebug($"Current encounter is {encountId}");
        }

        private string GetPartyState()
        {
            List<PartyMember> party = new List<PartyMember>();
            for (int i = 0; i < 3; i++)
            {
                var member = (PartyMember)_flowCaller.GET_PARTY(i + 1);
                if (member != PartyMember.Joker)
                    party.Add(member);
            }

            if (party.Count == 0)
                return "Alone";
            else
            {
                string stateStr = $"With {party[0]}";
                for (int i = 1; i < party.Count; i++)
                {
                    if (i != party.Count - 1)
                        stateStr += $", {party[i]}";
                    else
                        stateStr += $", and {party[i]}";
                }
                return stateStr;
            }
        }

        private void ProcessEvent()
        {
            if (_currentEvent == null)
                return;

            Event? eventInfo = _events.FirstOrDefault(e => e.Major == _currentEvent.Major && e.Minor == _currentEvent.Minor);
            Utils.LogDebug($"In event {_currentEvent} ({(eventInfo != null ? eventInfo.Name : "undocumented")})");

            int fieldMajor = _flowCaller.FLD_GET_PREV_MAJOR();
            int fieldMinor = _flowCaller.FLD_GET_PREV_MINOR();

            if (eventInfo == null)
            {
                _lastEvent = _currentEvent;
                ProcessField(fieldMajor, fieldMinor);
                return;
            }

            _presence.Details = eventInfo.Description;
            if (_lastEvent == null || (_currentEvent.Major != eventInfo.Major || _currentEvent.Minor != eventInfo.Minor))
            {
                _stateChanged = true;
                _states.Clear();
                if (eventInfo.State != null)
                    _states.Add(eventInfo.State);
            }
            if (eventInfo.ImageKey != null)
            {
                _presence.Assets.LargeImageKey = eventInfo.ImageKey;
                _presence.Assets.LargeImageText = _imageText.ContainsKey(eventInfo.ImageKey) ? _imageText[eventInfo.ImageKey] : "No image text found :(";
            }
            else
            {
                Field? field = _fields.FirstOrDefault(f => f.Major == fieldMajor && f.Minor == fieldMinor);
                if (field != null && field.ImageKey != null)
                {
                    _presence.Assets.LargeImageKey = field.ImageKey;
                    _presence.Assets.LargeImageText = _imageText.ContainsKey(field.ImageKey) ? _imageText[field.ImageKey] : "No image text found :(";
                }
            }
            _lastEvent = _currentEvent;
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