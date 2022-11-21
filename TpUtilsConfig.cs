using System.IO;
using System.Collections.Generic;
using TShockAPI;
using Newtonsoft.Json;
using System;

namespace TpUtils
{
    using LabelList = List<TpUtilsLabel>;
    using Config = Dictionary<int, List<TpUtilsLabel>>;

    public class TpUtilsLabel
    {
        public float X;
        public float Y;
        public string name;
    }

    public class TpUtilsConfig
    {
        private static string path = Path.Combine(TShock.SavePath, "tp-utils.json");

        int worldId;

        public TpUtilsConfig(int worldId)
        {
            this.worldId = worldId;
        }

        private static Config ReadConfig()
        {
            if (File.Exists(path))
            {
                using (StreamReader file = File.OpenText(path))
                {
                    var serializer = new JsonSerializer();
                    return (Config)serializer.Deserialize(file, typeof(Config));
                }
            }
            return new Config();
        }

        private static void WriteConfig(Config config)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, config);
            }
        }

        private static void WithConfig(Action<Config> fn)
        {
            var config = TpUtilsConfig.ReadConfig();
            fn(config);
            TpUtilsConfig.WriteConfig(config);
        }

        private LabelList ReadLabels()
        {
            var config = TpUtilsConfig.ReadConfig();
            if (config.ContainsKey(this.worldId))
            {
                return config[this.worldId];
            }
            return new LabelList();
        }

        private void UpdateLabels(LabelList list)
        {
            TpUtilsConfig.WithConfig(config => {
                config[this.worldId] = list;
            });
        }

        private void WithLabels(Action<LabelList> fn)
        {
            var labels = this.ReadLabels();
            fn(labels);
            this.UpdateLabels(labels);
        }

        public void Add(TpUtilsLabel label)
        {
            this.WithLabels(labels => {
                labels.Add(label);
            });
        }

        public TpUtilsLabel Remove(int id)
        {
            TpUtilsLabel label = new TpUtilsLabel();
            this.WithLabels(labels => {
                label = labels[id];
                labels.RemoveAt(id);
            });
            return label;
        }

        public int Count {
            get { return this.ReadLabels().Count; }
        }

        public TpUtilsLabel this[int index]
        {
            get { return this.ReadLabels()[index]; }
        }

        public void ForEach(Action<TpUtilsLabel, int> fn)
        {
            this.WithLabels(labels => {
                for (int i = 0; i < labels.Count; ++i)
                {
                    fn(labels[i], i);
                }
            });
        }
    }
}
