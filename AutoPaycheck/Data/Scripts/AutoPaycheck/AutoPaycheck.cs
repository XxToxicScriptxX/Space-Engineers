using Sandbox.ModAPI;
using Sandbox.Game.World;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using VRage.Game.Components;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using System;
using System.Collections.Generic;
using VRage.ModAPI;

namespace AutoPaycheck
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, "AutoPaycheckComponent")]
    public class AutoPaycheckComponent : MySessionComponentBase
    {
        private const long PayoutAmount = 10000; // credits per payout
        private const int SecondsBetweenPayouts = 10; // testing interval

        private const ushort MESSAGE_ID = 42424; // HUD message channel
        private const ushort CHAT_MESSAGE_ID = 42425; // Chat message channel

        private const int UpdatesPerSecond = 60;
        private int ticksSinceLastPayout = 0;
        private int ticksPerPayout = UpdatesPerSecond * SecondsBetweenPayouts;

        private bool initialized = false;

        // Funny paycheck reasons
        private readonly List<string> Reasons = new List<string>()
        {
            "Insurance payout for slamming into an asteroid at unsafe speeds",
            "Hazard bonus for standing too close to a hydrogen tank",
            "Compensation for emotional damage caused by clang",
            "Reimbursement for involuntary rapid unplanned disassembly",
            "Overtime pay for staring at a refinery for 3 hours",
            "Bonus for not dying this cycle",
            "Settlement for workplace accident involving a grinder",
            "Refund for purchasing a clearly cursed ship blueprint",
            "Hazard pay for entering a questionable airlock",
            "Compensation for witnessing a rover flip 17 times",
            "Bonus for surviving another day in this economy",
            "Reimbursement for damages caused by ‘creative engineering’",
            "Payment for services rendered: mostly screaming",
            "Clang settlement payout",
            "Reward for not pressing the big red button… yet"
        };

        public override void LoadData()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
            {
                MyAPIGateway.Multiplayer.RegisterMessageHandler(MESSAGE_ID, OnHudMessage);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(CHAT_MESSAGE_ID, OnChatMessage);
            }
        }

        protected override void UnloadData()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
            {
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(MESSAGE_ID, OnHudMessage);
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(CHAT_MESSAGE_ID, OnChatMessage);
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            if (!initialized)
            {
                initialized = true;
                MyLog.Default.WriteLineAndConsole("[AutoPaycheck] Initialized.");
            }

            ticksSinceLastPayout++;

            if (ticksSinceLastPayout >= ticksPerPayout)
            {
                ticksSinceLastPayout = 0;
                DoPayout();
            }
        }

        private void DoPayout()
        {
            try
            {
                List<IMyPlayer> players = new List<IMyPlayer>();
                MyAPIGateway.Multiplayer.Players.GetPlayers(players);

                int count = 0;

                foreach (var player in players)
                {
                    if (player == null || player.IsBot || player.Character == null)
                        continue;

                    long identityId = player.IdentityId;

                    // Pay the player
                    MyBankingSystem.ChangeBalance(identityId, PayoutAmount);
                    count++;

                    // Pick a random funny reason
                    string reason = Reasons[MyUtils.GetRandomInt(0, Reasons.Count)];

                    // HUD message
                    string hudMsg = $"+{PayoutAmount:N0} SC — {reason}";
                    byte[] hudData = System.Text.Encoding.UTF8.GetBytes(hudMsg);

                    MyAPIGateway.Multiplayer.SendMessageTo(
                        MESSAGE_ID,
                        hudData,
                        player.SteamUserId
                    );

                    // Chat message
                    string chatMsg = $"[AutoPaycheck] +{PayoutAmount:N0} SC — {reason}";
                    byte[] chatData = System.Text.Encoding.UTF8.GetBytes(chatMsg);

                    MyAPIGateway.Multiplayer.SendMessageTo(
                        CHAT_MESSAGE_ID,
                        chatData,
                        player.SteamUserId
                    );
                }

                MyLog.Default.WriteLineAndConsole($"[AutoPaycheck] Paid {PayoutAmount} SC to {count} online player(s).");
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole("[AutoPaycheck] ERROR: " + e);
            }
        }

        // CLIENT-SIDE: HUD popup
        private void OnHudMessage(byte[] data)
        {
            string msg = System.Text.Encoding.UTF8.GetString(data);
            MyAPIGateway.Utilities.ShowNotification(msg, 5000, "White");
        }

        // CLIENT-SIDE: Chat message
        private void OnChatMessage(byte[] data)
        {
            string msg = System.Text.Encoding.UTF8.GetString(data);
            MyAPIGateway.Utilities.ShowMessage("AutoPaycheck", msg);
        }
    }
}
