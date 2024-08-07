using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Temperature.Framework.Misc;
using Temperature.Framework.Data;


namespace Temperature.Framework.Controllers
{
    public class NetController
    {
        private static readonly IModHelper Helper = ModEntry.Instance.Helper;

        private static readonly IManifest Manifest = ModEntry.Instance.ModManifest;

        public static bool _firstLoad;

        public static void SyncSpecificPlayer(long player_id)
        {
            if (Context.IsMainPlayer)
            {
                PlayerData _playerData = Helper.Data.ReadSaveData<PlayerData>($"{player_id}") ?? new PlayerData();
                SyncBody _toSend = new(_playerData,
                    DataController.Seasons.Data,
                    DataController.Weather.Data,
                    DataController.Locations.Data,
                    DataController.Clothing.Data,
                    DataController.Objects.Data);

                Helper.Data.WriteSaveData($"{player_id}", _playerData);

                LogHelper.Trace($"Sending important PlayerData to farmhand {player_id}.");
                Helper.Multiplayer.SendMessage(
                    message: _toSend,
                    messageType: "SaveDataFromHost",
                    modIDs: [Manifest.UniqueID],
                    playerIDs: [player_id]
                );
            }
        }

        public static void SyncAllPlayers()
        {
            if (Context.IsMainPlayer)
            {
                LogHelper.Trace($"Sending important Data to all farmhands.");
                foreach (Farmer farmer in Game1.getOnlineFarmers())
                {
                    PlayerData _playerData = Helper.Data.ReadSaveData<PlayerData>($"{farmer.UniqueMultiplayerID}") ?? new PlayerData();
                    SyncBody _toSend = new(_playerData,
                        DataController.Seasons.Data,
                        DataController.Weather.Data,
                        DataController.Locations.Data,
                        DataController.Clothing.Data,
                        DataController.Objects.Data);

                    Helper.Data.WriteSaveData($"{farmer.UniqueMultiplayerID}", _playerData);

                    LogHelper.Trace($"Sending important Data to farmhand {farmer.UniqueMultiplayerID}.");
                    Helper.Multiplayer.SendMessage(
                        message: _toSend,
                        messageType: "SaveDataFromHost",
                        modIDs: [Manifest.UniqueID],
                        playerIDs: [farmer.UniqueMultiplayerID]
                    );
                }
            }
        }

        public static void Sync()
        {
            if (Context.IsMainPlayer)
            {
                if (Game1.IsMultiplayer) LogHelper.Trace($"Saving host PlayerData.");

                PlayerData _playerData = Helper.Data.ReadSaveData<PlayerData>($"{Game1.player.UniqueMultiplayerID}") ?? new PlayerData();
                if (!_firstLoad)
                {
                    ModEntry.PlayerData = _playerData;
                    _firstLoad = true;
                }
                Helper.Data.WriteSaveData($"{Game1.player.UniqueMultiplayerID}", ModEntry.PlayerData);
            }
            else
            {
                LogHelper.Trace($"Sending important PlayerData to host.");

                Helper.Multiplayer.SendMessage(
                    message: ModEntry.PlayerData,
                    messageType: "SaveDataToHost",
                    modIDs: [Manifest.UniqueID],
                    playerIDs: [Game1.MasterPlayer.UniqueMultiplayerID]
                );
            }
        }

        public static void OnMessageReceived(ModMessageReceivedEventArgs e)
        {
            if (!Context.IsMainPlayer && e.FromModID == Manifest.UniqueID && e.Type == "SaveDataFromHost")
            {
                SyncBody _body = e.ReadAs<SyncBody>();
                ModEntry.PlayerData = _body.playerData;
                DataController.Seasons.Data = _body.seasons;
                DataController.Weather.Data = _body.weather;
                DataController.Locations.Data = _body.locations;
                DataController.Clothing.Data = _body.clothing;
                DataController.Objects.Data = _body.objects;

                LogHelper.Trace("Received important PlayerData from host.");
            }

            if (Context.IsMainPlayer && e.FromModID == Manifest.UniqueID && e.Type == "SaveDataToHost")
            {
                PlayerData _playerData = e.ReadAs<PlayerData>();
                LogHelper.Trace($"Received important PlayerData from player {e.FromPlayerID}.");
                Helper.Data.WriteSaveData($"{e.FromPlayerID}", _playerData);
            }
        }
    }

    public class SyncBody
    {
        public PlayerData playerData;
        public Dictionary<string, EnvModifiers> seasons;
        public Dictionary<string, EnvModifiers> weather;
        public Dictionary<string, EnvModifiers> locations;
        public Dictionary<string, Dictionary<string, ClothingModifiers>> clothing;
        public Dictionary<string, ObjectModifiers> objects;

        public SyncBody(PlayerData _playerData,
            Dictionary<string, EnvModifiers> _seasons,
            Dictionary<string, EnvModifiers> _weather,
            Dictionary<string, EnvModifiers> _locations,
            Dictionary<string, Dictionary<string, ClothingModifiers>> _clothing,
            Dictionary<string, ObjectModifiers> _objects)
        {
            playerData = _playerData;
            seasons = _seasons;
            weather = _weather;
            locations = _locations;
            clothing = _clothing;
            objects = _objects;
        }
    }
}
