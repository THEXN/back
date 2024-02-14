using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace BP
{
    [ApiVersion(2, 1)]
    public class BackPlugin : TerrariaPlugin
    {
        // 记录每个玩家的冷却时间结束点
        private Dictionary<int, DateTime> cooldowns = new Dictionary<int, DateTime>();

        public override string Author => "Megghy,熙恩改";
        public override string Description => "允许玩家传送回死亡地点";
        public override string Name => "BackPlugin";
        public override Version Version => new Version(1, 0, 0, 3);
        public static Configuration Config;
        public BackPlugin(Main game) : base(game)
        {
            LoadConfig();
        }
        private static void LoadConfig()
        {
            Config = Configuration.Read(Configuration.FilePath);
            Config.Write(Configuration.FilePath);

        }
        private static void ReloadConfig(ReloadEventArgs args)
        {
            LoadConfig();
            args.Player?.SendSuccessMessage("[{0}] 重新加载配置完毕。", typeof(BackPlugin).Name);
        }
        public override void Initialize()
        {
            GeneralHooks.ReloadEvent += ReloadConfig;
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.ServerLeave.Register(this, ResetPos);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnPlayerJoin);
            GetDataHandlers.KillMe += OnDead;

            // 添加/back命令
            Commands.ChatCommands.Add(new Command("back", Back, "back")
            {
                HelpText = "返回最后一次死亡的位置",
                AllowServer = false // 不允许在控制台使用
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerLeave.Deregister(this, ResetPos);
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnPlayerJoin);
                GetDataHandlers.KillMe -= OnDead;
            }

            base.Dispose(disposing);
        }

        private void OnInitialize(EventArgs args)
        {
            // 插件初始化逻辑
        }

        private void ResetPos(LeaveEventArgs args)
        {
            // 玩家离开服务器时，移除死亡地点数据
            var list = TSPlayer.FindByNameOrID(Main.player[args.Who].name);
            if (list.Count > 0)
                list[0].RemoveData("DeadPoint");
        }

        private void Back(CommandArgs args)
        {
            // 获取玩家死亡地点
            var data = args.Player.GetData<Point>("DeadPoint");

            // 检查玩家是否已经复活
            if (args.Player.TPlayer.dead)
            {
                args.Player.SendErrorMessage("你尚未复活，无法传送回死亡地点.");
            }
            else if (data != Point.Zero)
            {
                // 检查是否可以使用命令（冷却时间）
                if (!CanUseCommand(args.Player))
                {
                    var remainingCooldown = GetRemainingCooldown(args.Player);
                    args.Player.SendErrorMessage($"你还需要等待 {remainingCooldown.TotalSeconds:F} 秒才能再次使用此命令.");
                    return;
                }

                try
                {
                    // 传送玩家至死亡地点
                    args.Player.Teleport(data.X, data.Y, 1);
                    args.Player.SendSuccessMessage($"已传送至死亡地点 [c/8DF9D8:<{data.X / 16} - {data.Y / 16}>].");

                    // 设置命令冷却时间
                    SetCooldown(args.Player);
                }
                catch (Exception ex)
                {
                    TShock.Log.Error($"BackPlugin: 传送玩家 {args.Player.Name} 时发生错误: {ex}");
                }
            }
            else
            {
                args.Player.SendErrorMessage("你还未死亡过");
            }
        }

        private void OnDead(object o, GetDataHandlers.KillMeEventArgs args)
        {
            // 在玩家死亡时记录死亡地点
            args.Player.SetData("DeadPoint", new Point((int)args.Player.X, (int)args.Player.Y));
        }

        private void OnPlayerJoin(GreetPlayerEventArgs args)
        {
            // 在玩家加入服务器时，将死亡地点数据初始化为零
            var list = TSPlayer.FindByNameOrID(Main.player[args.Who].name);
            if (list.Count > 0)
                list[0].SetData("DeadPoint", Point.Zero);
        }

        private bool CanUseCommand(TSPlayer player)
        {
            // 检查玩家是否可以使用命令（是否在冷却中）
            if (cooldowns.ContainsKey(player.Index))
            {
                var cooldownEnd = cooldowns[player.Index];
                if (DateTime.Now < cooldownEnd)
                {
                    return false; // 冷却中，不能使用命令
                }
            }

            return true; // 可以使用命令
        }

        private TimeSpan GetRemainingCooldown(TSPlayer player)
        {
            // 获取玩家剩余的冷却时间
            if (cooldowns.ContainsKey(player.Index))
            {
                var cooldownEnd = cooldowns[player.Index];
                var remainingTime = cooldownEnd - DateTime.Now;
                return remainingTime > TimeSpan.Zero ? remainingTime : TimeSpan.Zero;
            }

            return TimeSpan.Zero; // 没有冷却时间，返回零
        }

        private void SetCooldown(TSPlayer player)
        {
            // 设置玩家的命令冷却时间
            var cooldownDuration = TimeSpan.FromSeconds(Config.back冷却时间); 
            var cooldownEnd = DateTime.Now.Add(cooldownDuration);

            if (cooldowns.ContainsKey(player.Index))
            {
                cooldowns[player.Index] = cooldownEnd;
            }
            else
            {
                cooldowns.Add(player.Index, cooldownEnd);
            }
        }
    }
}

