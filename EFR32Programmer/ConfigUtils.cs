using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EFR32Programmer
{
    class ConfigUtils
    {
        private static readonly string configFile = "EFR32Programmer.json";

        public async Task<bool> SaveConfig(Config config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                using (FileStream fs = new FileStream(configFile, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        await sw.WriteAsync(json);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }

        }

        public async Task<Config> LoadConfig()
        {
            try
            {
                using (FileStream fs = new FileStream(configFile, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string json = await sr.ReadToEndAsync();
                        var config = JsonConvert.DeserializeObject<Config>(json);
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return new Config();
            }
        }
    }
}
