using System.Collections.Generic;
using System.Linq;
using Temperature.Framework.Misc;
using System.IO;
using System.Text.Json;

namespace Temperature.Framework.Data
{
    public class GenericData<T>
    {
        public Dictionary<string, T> Data { get; set; } = [];

        public void LoadData(string path)
        {
            string jsonData = null;
            Dictionary<string, T> rawData = null;

            try { jsonData = File.ReadAllText(path); }
            catch (FileNotFoundException) { return; }
            catch (DirectoryNotFoundException) { return; }
            if (jsonData == null) return;

            try { rawData = JsonSerializer.Deserialize<Dictionary<string, T>>(jsonData); }
            catch (JsonException exception)
            {
                LogHelper.Warn($"({path}) — Can't Deserialize!");
                LogHelper.Warn(exception.Message);
                LogHelper.Warn(exception.StackTrace);
                return;
            }
            if (rawData == null) return;
            rawData.ToList().ForEach(AddToInGameData);
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