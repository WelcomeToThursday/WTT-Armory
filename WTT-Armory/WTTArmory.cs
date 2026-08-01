using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using WTTArmory.Chatbot;
using WTTArmory.Helpers;
using Range = SemanticVersioning.Range;

namespace WTTArmory;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.wtt.armory";
    public string Name { get; init; } = "WTT-Armory";
    public string Author { get; init; } = "GrooveypenguinX";
    public List<string>? Contributors { get; init; } = null;
    public SemanticVersioning.Version Version { get; init; } = new(typeof(ModMetadata).Assembly.GetName().Version?.ToString(3));
    public Range SptVersion { get; init; } = new("~4.1.0");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, Range>? ModDependencies { get; init; } = new()
    {
        { "com.wtt.commonlib", new Range("^3.0.0") }
    };
    public string? Url { get; init; }
    public string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 2)]
public class WTTArmory(
    WTTServerCommonLib.WTTServerCommonLib wttCommon,
    CoreConfig coreConfig,
    WTTBot wttBot,
    ArmoryQuestHelper armoryQuestHelper) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
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
        coreConfig.Features.ChatbotFeatures.Ids[myBot.Info?.Nickname ?? throw new InvalidOperationException()] = myBot.Id;
        coreConfig.Features.ChatbotFeatures.EnabledBots[myBot.Id] = true;
    }
}
