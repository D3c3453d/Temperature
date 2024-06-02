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
            var actualData = rawData.Where(data => (data.Key != null) && (data.Value != null)).ToList();
            actualData.ForEach(AddToInGameData);
        }

        private void AddToInGameData(KeyValuePair<string, T> data)
        {
            try
            {
                Data.Add(data.Key, data.Value);
            }
            catch (System.ArgumentException exception)
            {

                LogHelper.Trace($"({data.Key}) — Duplicate Entry!");
                LogHelper.Trace(exception.Message);
                LogHelper.Trace(exception.StackTrace);
            }
        }

    }
}