using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Temperature.Framework.Common;
using Temperature.Framework.Databases;


namespace Temperature.Framework.Misc
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
                Data _data = Helper.Data.ReadSaveData<Data>($"{player_id}") ?? new Data();
                SyncBody _toSend = new SyncBody(_data,
                    DbController.Seasons.Data,
                    DbController.Weather.Data,
                    DbController.Locations.Data,
                    DbController.Clothes.Data,
                    DbController.Objects.Data);

                Helper.Data.WriteSaveData($"{player_id}", _data);

                Debugger.Log($"Sending important Data to farmhand {player_id}.", "Trace");
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
                Debugger.Log($"Sending important Data to all farmhands.", "Trace");
                foreach (Farmer farmer in Game1.getOnlineFarmers())
                {
                    Data _data = Helper.Data.ReadSaveData<Data>($"{farmer.UniqueMultiplayerID}") ?? new Data();
                    SyncBody _toSend = new SyncBody(_data,
                        DbController.Seasons.Data,
                        DbController.Weather.Data,
                        DbController.Locations.Data,
                        DbController.Clothes.Data,
                        DbController.Objects.Data);

                    Helper.Data.WriteSaveData($"{farmer.UniqueMultiplayerID}", _data);

                    Debugger.Log($"Sending important Data to farmhand {farmer.UniqueMultiplayerID}.", "Trace");
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
                if (Game1.IsMultiplayer) Debugger.Log($"Saving host Data.", "Trace");

                Data _data = Helper.Data.ReadSaveData<Data>($"{Game1.player.UniqueMultiplayerID}") ?? new Data();
                if (!_firstLoad)
                {
                    ModEntry.Data = _data;
                    _firstLoad = true;
                }
                Helper.Data.WriteSaveData($"{Game1.player.UniqueMultiplayerID}", ModEntry.Data);
            }
            else
            {
                Debugger.Log($"Sending important Data to host.", "Trace");

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
                DbController.Seasons.Data = _body.seasons;
                DbController.Weather.Data = _body.weather;
                DbController.Locations.Data = _body.locations;
                DbController.Clothes.Data = _body.clothes;
                DbController.Objects.Data = _body.objects;

                Debugger.Log("Received important Data from host.", "Trace");
            }

            if (Context.IsMainPlayer && e.FromModID == Manifest.UniqueID && e.Type == "SaveDataToHost")
            {
                Data _data = e.ReadAs<Data>();
                Debugger.Log($"Received important Data from player {e.FromPlayerID}.", "Trace");
                Helper.Data.WriteSaveData($"{e.FromPlayerID}", _data);
            }
        }
    }

    public class SyncBody
    {
        public Data data;
        public Dictionary<string, EnvModifiers> seasons;
        public Dictionary<string, EnvModifiers> weather;
        public Dictionary<string, EnvModifiers> locations;
        public Dictionary<string, Dictionary<string, ClothModifiers>> clothes;
        public Dictionary<string, ObjectModifiers> objects;

        public SyncBody(Data _data,
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