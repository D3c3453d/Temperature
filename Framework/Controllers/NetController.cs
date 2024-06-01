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
                PlayerData _data = Helper.Data.ReadSaveData<PlayerData>($"{player_id}") ?? new PlayerData();
                SyncBody _toSend = new SyncBody(_data,
                    DataController.Seasons.Data,
                    DataController.Weather.Data,
                    DataController.Locations.Data,
                    DataController.Clothes.Data,
                    DataController.Objects.Data);

                Helper.Data.WriteSaveData($"{player_id}", _data);

                LogHelper.Trace($"Sending important Data to farmhand {player_id}.");
                Helper.Multiplayer.SendMessage(
                    message: _toSend,
                    messageType: "SaveDataFromHost",
                    modIDs: new[] { Manifest.UniqueID },
                    playerIDs: new[] { player_id }
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
                    PlayerData _data = Helper.Data.ReadSaveData<PlayerData>($"{farmer.UniqueMultiplayerID}") ?? new PlayerData();
                    SyncBody _toSend = new SyncBody(_data,
                        DataController.Seasons.Data,
                        DataController.Weather.Data,
                        DataController.Locations.Data,
                        DataController.Clothes.Data,
                        DataController.Objects.Data);

                    Helper.Data.WriteSaveData($"{farmer.UniqueMultiplayerID}", _data);

                    LogHelper.Trace($"Sending important Data to farmhand {farmer.UniqueMultiplayerID}.");
                    Helper.Multiplayer.SendMessage(
                        message: _toSend,
                        messageType: "SaveDataFromHost",
                        modIDs: new[] { Manifest.UniqueID },
                        playerIDs: new[] { farmer.UniqueMultiplayerID }
                    );
                }
            }
        }

        public static void Sync()
        {
            if (Context.IsMainPlayer)
            {
                if (Game1.IsMultiplayer) LogHelper.Trace($"Saving host Data.");

                PlayerData _data = Helper.Data.ReadSaveData<PlayerData>($"{Game1.player.UniqueMultiplayerID}") ?? new PlayerData();
                if (!_firstLoad)
                {
                    ModEntry.Data = _data;
                    _firstLoad = true;
                }
                Helper.Data.WriteSaveData($"{Game1.player.UniqueMultiplayerID}", ModEntry.Data);
            }
            else
            {
                LogHelper.Trace($"Sending important Data to host.");

                Helper.Multiplayer.SendMessage(
                    message: ModEntry.Data,
                    messageType: "SaveDataToHost",
                    modIDs: new[] { Manifest.UniqueID },
                    playerIDs: new[] { Game1.MasterPlayer.UniqueMultiplayerID }
                );
            }
        }

        public static void OnMessageReceived(ModMessageReceivedEventArgs e)
        {
            if (!Context.IsMainPlayer && e.FromModID == Manifest.UniqueID && e.Type == "SaveDataFromHost")
            {
                SyncBody _body = e.ReadAs<SyncBody>();
                ModEntry.Data = _body.data;
                DataController.Seasons.Data = _body.seasons;
                DataController.Weather.Data = _body.weather;
                DataController.Locations.Data = _body.locations;
                DataController.Clothes.Data = _body.clothes;
                DataController.Objects.Data = _body.objects;

                LogHelper.Trace("Received important Data from host.");
            }

            if (Context.IsMainPlayer && e.FromModID == Manifest.UniqueID && e.Type == "SaveDataToHost")
            {
                PlayerData _data = e.ReadAs<PlayerData>();
                LogHelper.Trace($"Received important Data from player {e.FromPlayerID}.");
                Helper.Data.WriteSaveData($"{e.FromPlayerID}", _data);
            }
        }
    }

    public class SyncBody
    {
        public PlayerData data;
        public Dictionary<string, EnvModifiers> seasons;
        public Dictionary<string, EnvModifiers> weather;
        public Dictionary<string, EnvModifiers> locations;
        public Dictionary<string, Dictionary<string, ClothModifiers>> clothes;
        public Dictionary<string, ObjectModifiers> objects;

        public SyncBody(PlayerData _data,
            Dictionary<string, EnvModifiers> _seasons,
            Dictionary<string, EnvModifiers> _weather,
            Dictionary<string, EnvModifiers> _locations,
            Dictionary<string, Dictionary<string, ClothModifiers>> _clothes,
            Dictionary<string, ObjectModifiers> _objects)
        {
            data = _data;
            seasons = _seasons;
            weather = _weather;
            locations = _locations;
            clothes = _clothes;
            objects = _objects;
        }
    }
}