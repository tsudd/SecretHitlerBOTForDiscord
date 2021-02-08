using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Bot
{
    class ConfigProvider
    {
        const string jsonName = "config.json";
        readonly BotConfig botSettings;

        public ConfigProvider()
        {
            try
            {
                var pathToDir = Directory.GetCurrentDirectory();
                using var stream = new StreamReader(pathToDir + "\\" + jsonName);
                var json = stream.ReadToEnd();
                botSettings = JsonSerializer.Deserialize<BotConfig>(json);
            } 
            catch (Exception)
            {
                botSettings = new BotConfig();
                File.Create(jsonName);
            }
            
        }

        public object GetSetting(Options option)
        {
            object ans = "";

            foreach (var property in botSettings.GetType().GetProperties())
            {
                if (property.Name == option.ToString())
                {
                    ans = property.GetValue(botSettings);
                }
            }

            return ans;
        }
    }
}
