using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Servers;
using WTTArmory.Chatbot;
using WTTArmory.Helpers;
using Range = SemanticVersioning.Range;

namespace WTTArmory;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.wtt.armory";
    public override string Name { get; init; } = "WTT-Armory";
    public override string Author { get; init; } = "GrooveypenguinX";
    public override List<string>? Contributors { get; init; } = null;
    public override SemanticVersioning.Version Version { get; init; } = new(typeof(ModMetadata).Assembly.GetName().Version?.ToString(3));
    public override Range SptVersion { get; init; } = new("~4.0.2");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, Range>? ModDependencies { get; init; } = new()
    {
        { "com.wtt.commonlib", new Range("~2.0.24") }
    };
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; } = true;
    public override string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class WTTArmory(
    WTTServerCommonLib.WTTServerCommonLib wttCommon,
    ConfigServer configServer,
    WTTBot wttBot,
    ArmoryQuestHelper armoryQuestHelper) : IOnLoad
{
    public async Task OnLoad()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly);
        await wttCommon.CustomLootspawnService.CreateCustomLootSpawns(assembly);
        await wttCommon.CustomQuestService.CreateCustomQuests(assembly);
        await wttCommon.CustomAssortSchemeService.CreateCustomAssortSchemes(assembly);
        await wttCommon.CustomBotLoadoutService.CreateCustomBotLoadouts(assembly);
        await wttCommon.CustomLocaleService.CreateCustomLocales(assembly);
        await wttCommon.CustomQuestZoneService.CreateCustomQuestZones(assembly);
        await wttCommon.CustomStaticSpawnService.CreateCustomStaticSpawns(assembly);
        await wttCommon.CustomAchievementService.CreateCustomAchievements(assembly);
        armoryQuestHelper.ModifyQuests();
        
        var myBot = wttBot.GetChatBot();
        var coreConfig = configServer.GetConfig<CoreConfig>();
        coreConfig.Features.ChatbotFeatures.Ids[myBot.Info?.Nickname ?? throw new InvalidOperationException()] = myBot.Id;
        coreConfig.Features.ChatbotFeatures.EnabledBots[myBot.Id] = true;
    }
}
