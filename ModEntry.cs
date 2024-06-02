using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Temperature.Framework.APIs;
using Temperature.Framework.Controllers;
using Temperature.Framework.Data;
using Temperature.Framework.Misc;

namespace Temperature
{
    public class ModEntry : Mod
    {
        public static ModEntry Instance { get; private set; }

        public static PlayerData PlayerData { get; set; }

        public static Config Config { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Config = Helper.ReadConfig<Config>();

            // Textures.LoadTextures();

            helper.Events.GameLoop.GameLaunched += OnGameLaunch;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += OnUpdate;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            helper.Events.Multiplayer.PeerConnected += OnPlayerConnected;
            helper.Events.Multiplayer.ModMessageReceived += OnMessageReceived;

            helper.Events.GameLoop.ReturnedToTitle += OnReturnToTitle;

            DataController.LoadData();
        }

        private void OnGameLaunch(object sender, GameLaunchedEventArgs e)
        {
            // bool result = new ConfigMenuInitializer(ModManifest, Helper, Config,
            //                                         Instance.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu")).InitializeModMenu();
            // string message = result ? "Generic Mod Menu successfully loaded for this mod!" :
            //                           "Generic Mod Menu isn't found... skip.";

            // Monitor.Log(message, LogLevel.Info);
        }

        private void OnReturnToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            NetController._firstLoad = false;
        }

        private void OnUpdate(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !Context.IsWorldReady || Game1.paused)
                return;
            EnvTempController.Update();
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            NetController.Sync();
            EnvTempController.FluctuationUpdate();
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            OnUpdate(default, default);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!NetController._firstLoad) NetController.Sync();
            NetController.Sync();
        }

        private void OnPlayerConnected(object sender, PeerConnectedEventArgs e) =>
                     NetController.SyncSpecificPlayer(e.Peer.PlayerID);

        private void OnMessageReceived(object sender, ModMessageReceivedEventArgs e) =>
                     NetController.OnMessageReceived(e);
    }
}