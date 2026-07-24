using Exiled.API.Features;
using Exiled.Events;
using System;

using ServerEvents = Exiled.Events.Handlers.Server;
using Exiled.Events.EventArgs.Server;

public class Main : Plugin<Config>
{
    public static Main Instance;

    public override string Name { get; } = "CombatAI";
    public override string Author { get; } = "A3indae";
    public override Version Version { get; } = new Version(1, 0);

    public override void OnEnabled()
    {
        Instance = this;
        base.OnEnabled();

        ServerEvents.EndingRound += OnEndingRound;
    }
    public override void OnDisabled()
    {
        ServerEvents.EndingRound -= OnEndingRound;

        Instance = null;
        base.OnDisabled();
    }

    private void OnEndingRound(EndingRoundEventArgs ev)
    {
        CombatAI.API.NavmeshHandler.ClearNavmesh();
        CombatAI.API.CombatAIHandler.ClearCombatAI();
    }
}