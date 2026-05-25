// Language: C#
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Game.Components;
using System;
using System.Collections.Generic;

[MySessionComponentDescriptor(MyUpdateOrder.OncePerSecond)]
public class HourlyCredits : MySessionComponentBase
{
    private DateTime lastCreditTime;

    private const long CreditAmount = 100; // 10 million credits

    public override void UpdateAfterSimulation()
    {
        if (!MyAPIGateway.Session.IsServer)
            return;

        // Check if one hour has passed
        if ((DateTime.Now - lastCreditTime).TotalMinutes >= 1)
        {
            lastCreditTime = DateTime.Now;
            GiveCreditsToPlayers();
        }
    }

    private void GiveCreditsToPlayers()
    {
        List<IMyPlayer> players = new List<IMyPlayer>();
        MyAPIGateway.Players.GetPlayers(players);

        foreach (var player in players)
        {
            var identity = MyAPIGateway.Players.TryGetIdentity(player.IdentityId);
            if (identity != null)
            {
                identity.AddBalance(CreditAmount);
                MyAPIGateway.Utilities.ShowMessage("Credits", $"You have received {CreditAmount:N0} credits!");
            }
        }
    }
}