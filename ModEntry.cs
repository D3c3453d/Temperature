using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Temperature.Framework.APIs;
using Temperature.Framework.Controllers;
using Temperature.Framework.Data;
using Temperature.Framework.Misc;
using Netcode;
using StardewValley.Objects;

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
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            helper.Events.Player.Warped += OnPlayerWarped;

            helper.Events.Multiplayer.PeerConnected += OnPlayerConnected;
            helper.Events.Multiplayer.ModMessageReceived += OnMessageReceived;

            helper.Events.GameLoop.ReturnedToTitle += OnReturnToTitle;

            DataController.LoadData();
        }

        private void OnPlayerWarped(object sender, WarpedEventArgs e)
        {
            LogHelper.Debug($"{e.Player.Name} warped {e.OldLocation.Name} -> {e.NewLocation.Name}");
            PlayerData.CurrentSeasonData = DataController.UpdateSeasonData(Game1.player.currentLocation.GetSeason().ToString(), Game1.getFarm().GetSeason().ToString());
            PlayerData.CurrentWeatherData = DataController.UpdateWeatherData(Game1.player.currentLocation.GetWeather().Weather);
            PlayerData.CurrentLocationData = DataController.UpdateLocationData(Game1.player.currentLocation.Name);
        }

        private void OnBootsChange(NetRef<Boots> field, Boots oldValue, Boots newValue)
        {
            PlayerData.CurrentBootsData = DataController.UpdateBootsData(newValue);
        }

        private void OnPantsItemChange(NetRef<Clothing> field, Clothing oldValue, Clothing newValue)
        {
            PlayerData.CurrentPantsData = DataController.UpdatePantsData(newValue);
        }

        private void OnShirtItemChange(NetRef<Clothing> field, Clothing oldValue, Clothing newValue)
        {
            PlayerData.CurrentShirtData = DataController.UpdateShirtData(newValue);
        }

        private void OnHatChange(NetRef<Hat> field, Hat oldValue, Hat newValue)
        {
            PlayerData.CurrentHatData = DataController.UpdateHatData(newValue);
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

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !Context.IsWorldReady || Game1.paused)
                return;
        }
        private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !Context.IsWorldReady || Game1.paused)
                return;

            Game1.player.hat.fieldChangeVisibleEvent += OnHatChange;
            Game1.player.shirtItem.fieldChangeVisibleEvent += OnShirtItemChange;
            Game1.player.pantsItem.fieldChangeVisibleEvent += OnPantsItemChange;
            Game1.player.boots.fieldChangeVisibleEvent += OnBootsChange;

            PlayerData.EnvTemp = EnvTempController.Update(PlayerData.CurrentSeasonData,
            PlayerData.CurrentWeatherData, PlayerData.CurrentLocationData, Game1.player.currentLocation,
            Game1.player.GetBoundingBox().Center.X, Game1.player.GetBoundingBox().Center.Y, Game1.Date.TotalDays, Game1.CurrentMineLevel);
            LogHelper.Warn($"EnvTemp: {PlayerData.EnvTemp}");

            PlayerData.BodyTemp = BodyTempController.Update(PlayerData.BodyTemp, PlayerData.EnvTemp,
                PlayerData.CurrentHatData, PlayerData.CurrentShirtData, PlayerData.CurrentPantsData, PlayerData.CurrentBootsData);
            LogHelper.Warn($"BodyTemp: {PlayerData.BodyTemp}");
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            NetController.Sync();
            EnvTempController.FluctuationUpdate();
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            OnUpdateTicked(default, default);
            PlayerData.CurrentSeasonData = DataController.UpdateSeasonData(Game1.player.currentLocation.GetSeason().ToString(), Game1.getFarm().GetSeason().ToString());
            PlayerData.CurrentWeatherData = DataController.UpdateWeatherData(Game1.player.currentLocation.GetWeather().Weather);
            PlayerData.CurrentLocationData = DataController.UpdateLocationData(Game1.player.currentLocation.Name);

            PlayerData.CurrentHatData = DataController.UpdateHatData(Game1.player.hat.Value);
            PlayerData.CurrentShirtData = DataController.UpdateShirtData(Game1.player.shirtItem.Value);
            PlayerData.CurrentPantsData = DataController.UpdatePantsData(Game1.player.pantsItem.Value);
            PlayerData.CurrentBootsData = DataController.UpdateBootsData(Game1.player.boots.Value);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!NetController._firstLoad) NetController.Sync();
            NetController.Sync();
            PlayerData.CurrentSeasonData = DataController.UpdateSeasonData(Game1.player.currentLocation.GetSeason().ToString(), Game1.getFarm().GetSeason().ToString());
            PlayerData.CurrentWeatherData = DataController.UpdateWeatherData(Game1.player.currentLocation.GetWeather().Weather);
            PlayerData.CurrentLocationData = DataController.UpdateLocationData(Game1.player.currentLocation.Name);

            PlayerData.CurrentHatData = DataController.UpdateHatData(Game1.player.hat.Value);
            PlayerData.CurrentShirtData = DataController.UpdateShirtData(Game1.player.shirtItem.Value);
            PlayerData.CurrentPantsData = DataController.UpdatePantsData(Game1.player.pantsItem.Value);
            PlayerData.CurrentBootsData = DataController.UpdateBootsData(Game1.player.boots.Value);

        }

        private void OnPlayerConnected(object sender, PeerConnectedEventArgs e)
        {
            NetController.SyncSpecificPlayer(e.Peer.PlayerID);
        }

        private void OnMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            NetController.OnMessageReceived(e);
            PlayerData.CurrentSeasonData = DataController.UpdateSeasonData(Game1.player.currentLocation.GetSeason().ToString(), Game1.getFarm().GetSeason().ToString());
            PlayerData.CurrentWeatherData = DataController.UpdateWeatherData(Game1.player.currentLocation.GetWeather().Weather);
            PlayerData.CurrentLocationData = DataController.UpdateLocationData(Game1.player.currentLocation.Name);

            PlayerData.CurrentHatData = DataController.UpdateHatData(Game1.player.hat.Value);
            PlayerData.CurrentShirtData = DataController.UpdateShirtData(Game1.player.shirtItem.Value);
            PlayerData.CurrentPantsData = DataController.UpdatePantsData(Game1.player.pantsItem.Value);
            PlayerData.CurrentBootsData = DataController.UpdateBootsData(Game1.player.boots.Value);
        }
    }
}