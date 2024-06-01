using System.Collections.Generic;
using System.Linq;
using Temperature.Framework.Misc;

namespace Temperature.Framework.Data
{
    public class GenericData<T>
    {
        public Dictionary<string, T> Data { get; set; } = new();

        public void LoadData(string path)
        {
            var rawData = ModEntry.Instance.Helper.Data.ReadJsonFile<Dictionary<string, T>>(path);
            if (rawData == null) return;
            var actualData = rawData.Where(db => (db.Key != null) && (db.Value != null)).ToList();
            actualData.ForEach(AddToInGameData);
        }

        private void AddToInGameData(KeyValuePair<string, T> db)
        {
            try
            {
                Data.Add(db.Key, db.Value);
            }
            catch (System.ArgumentException exception)
            {

                LogHelper.Trace($"({db.Key}) — Duplicate Entry!");
                LogHelper.Trace(exception.Message);
                LogHelper.Trace(exception.StackTrace);
            }
        }

    }
}