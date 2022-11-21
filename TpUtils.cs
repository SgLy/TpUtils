using System;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.IO;

namespace TpUtils
{
    [ApiVersion(2, 1)]
    public class TpUtils: TerrariaPlugin
    {
        /// <summary>
        /// Gets the author(s) of this plugin
        /// </summary>
        public override string Author => "SgLy";

        public TpUtilsConfig labels;
        
        /// <summary>
        /// Gets the description of this plugin.
        /// A short, one lined description that tells people what your plugin does.
        /// </summary>
        public override string Description => "More teleport commands";
      
        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        public override string Name => "TpUtils";
      
        /// <summary>
        /// Gets the version of this plugin.
        /// </summary>
        public override Version Version => new Version(1, 0, 0, 0);
      
        /// <summary>
        /// Initializes a new instance of the TpUtils class.
        /// This is where you set the plugin's order and perfrom other constructor logic
        /// </summary>
        public TpUtils(Main game) : base(game) {
            for (int i = 0; i < 256; ++i) {
                this.lastDeathX[i] = (float)-1e6;
                this.lastDeathY[i] = (float)-1e6;
            }
        }
        
        /// <summary>
        /// Handles plugin initialization. 
        /// Fired when the server is started and the plugin is being loaded.
        /// You may register hooks, perform loading procedures etc here.
        /// </summary>
        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            ServerApi.Hooks.GamePostInitialize.Register(this, PostInitialize);
        }

        void OnInitialize(EventArgs e)
        {
            Commands.ChatCommands.Add(new Command("tp-utils.tpd", TPDeath, "tpd")
            {
                AllowServer = false,
                HelpText = "Teleport to last death position."
            });
            Commands.ChatCommands.Add(new Command("tp-utils.label", Label, "add")
            {
                AllowServer = false,
                HelpText = "Label current position."
            });
            Commands.ChatCommands.Add(new Command("tp-utils.lslabel", ListLabel, "ls")
            {
                AllowServer = false,
                HelpText = "List all labeled positions."
            });
            Commands.ChatCommands.Add(new Command("tp-utils.rmlabel", RemoveLabel, "rm")
            {
                AllowServer = false,
                HelpText = "Remove a labeled position."
            });
            Commands.ChatCommands.Add(new Command("tp-utils.tplabel", TpLabel, "tpl")
            {
                AllowServer = false,
                HelpText = "Teleport to labeled position."
            });
        }

        void PostInitialize(EventArgs e)
        {
            this.labels = new TpUtilsConfig(Main.worldID);
        }

        void OnLeave(LeaveEventArgs e)
        {
            this.lastDeathX[e.Who] = (float)-1e6;
            this.lastDeathY[e.Who] = (float)-1e6;
        }

        private string FormatLabel(int id, TpUtilsLabel label)
        {
            return $"<{id,2}> ({label.X,4:f0}, {label.Y,4:f0}) {label.name}";
        }

        void TPDeath(CommandArgs e)
        {
            int playerId = e.Player.Index;
            float X = this.lastDeathX[playerId];
            float Y = this.lastDeathY[playerId];
            if (X < -1e5 || Y < -1e5) {
                e.Player.SendErrorMessage("No recorded death position yet.");
                return;
            }
            e.Player.SendSuccessMessage($"Teleported to {X}, {Y}");
            e.Player.Teleport(X, Y);
        }

        void Label(CommandArgs e)
        {
            if (e.Parameters.Count != 1)
            {
                e.Player.SendErrorMessage($"Invalid syntax, usage: {Commands.Specifier}add <name>");
                return;
            }
            int id = this.labels.Count;
            string name = e.Parameters[0];
            var label = new TpUtilsLabel() {
                name = name,
                X = e.Player.X,
                Y = e.Player.Y,
            };
            e.Player.SendSuccessMessage($"Labeled {FormatLabel(id, label)}");
            this.labels.Add(label);
        }

        void ListLabel(CommandArgs e)
        {
            e.Player.SendInfoMessage($"All labeled position:");
            this.labels.ForEach((label, i) => {
                e.Player.SendInfoMessage($"  {FormatLabel(i, label)}");
            });
            e.Player.SendInfoMessage($"Total: {this.labels.Count} labels");
        }

        void RemoveLabel(CommandArgs e)
        {
            if (e.Parameters.Count != 1)
            {
                e.Player.SendErrorMessage($"Invalid syntax, usage: {Commands.Specifier}rm <id>");
                return;
            }
            int id = 0;
            try {
                id = Int32.Parse(e.Parameters[0]);
            }
            catch (FormatException)
            {
                e.Player.SendErrorMessage($"{e.Parameters[0]} is not a valid id, id should be an int");
                return;
            }
            var count = this.labels.Count;
            if (id < 0 || id >= count)
            {
                e.Player.SendErrorMessage($"{id} is not a valid id, id should between 0-{count - 1}");
                return;
            }
            var label = this.labels.Remove(id);
            e.Player.SendSuccessMessage($"Removed {FormatLabel(id, label)}");
        }

        void TpLabel(CommandArgs e)
        {
            if (e.Parameters.Count != 1)
            {
                e.Player.SendErrorMessage($"Invalid syntax, usage: {Commands.Specifier}tpl <id>");
                return;
            }
            int id = 0;
            try {
                id = Int32.Parse(e.Parameters[0]);
            }
            catch (FormatException)
            {
                e.Player.SendErrorMessage($"{e.Parameters[0]} is not a valid id, id should be an int");
                return;
            }
            var count = this.labels.Count;
            if (id < 0 || id >= count)
            {
                e.Player.SendErrorMessage($"{id} is not a valid id, id should between 0-{count - 1}");
                return;
            }
            var label = this.labels[id];
            e.Player.Teleport(label.X, label.Y);
            e.Player.SendSuccessMessage($"Teleported to {FormatLabel(id, label)}");
        }

        /// <summary>
        /// Handles plugin disposal logic.
        /// *Supposed* to fire when the server shuts down.
        /// You should deregister hooks and free all resources here.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Deregister hooks here
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            }
            base.Dispose(disposing);
        }

        private float[] lastDeathX = new float[256];
        private float[] lastDeathY = new float[256];

        private void OnGetData(GetDataEventArgs args)
        {
            // check if the packet sent is the player update packet
            if (args.MsgID == PacketTypes.PlayerDeathV2)
            {
                // create a memory stream used to read information from the packet
                using (MemoryStream data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                {
                    // player variable read from the first byte of the packet
                    int playerId = data.ReadByte();
                    TSPlayer player = TShock.Players[playerId];
                    this.lastDeathX[playerId] = player.X;
                    this.lastDeathY[playerId] = player.Y;
                }
            }
        }
    }
}
