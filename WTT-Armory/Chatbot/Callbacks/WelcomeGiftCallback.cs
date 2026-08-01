using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Modding;
using SPTarkov.Server.Core.Utils;
using WTTArmory.Chatbot.Commands;
using WTTArmory.Models;

namespace WTTArmory.Chatbot.Callbacks;

[Injectable]
public class WelcomeGiftCallback(
    JsonUtil jsonUtil,
    ProfileHelper profileHelper,
    WTTBot wttBot,
    SaveServer saveServer,
    IServiceProvider serviceProvider,
    ISptLogger<WelcomeGiftCallback> logger,
    ProfileDataService profileDataService
)
{
    private const string ModKey = "WTT-Armory";
    
    public async ValueTask HandleWelcomeGift(string sessionId)
    {
        try
        {
            var giftData = await profileDataService.GetProfileDataAsync<WTTWelcomeGiftData>(sessionId, ModKey);
            if (giftData == null)
            {
                var dummyRequest = new SendMessageRequest
                {
                    DialogId = new MongoId(),
                    Text = "wtt welcomegift",
                    Type = MessageType.UserMessage,
                    ReplyTo = null
                };

                var welcomeGiftCommand = serviceProvider.GetService<WelcomeGiftCommand>();
                if (welcomeGiftCommand != null)
                {
                    await welcomeGiftCommand.PerformAction(
                        wttBot.GetChatBot(),
                        sessionId,
                        dummyRequest
                    );
                }
                else
                {
                    logger.Error("WelcomeGiftCommand not available from service provider.");
                }

                logger.Info("Sent WTT welcome gift to player via command");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error sending WTT welcome gift: {ex.Message}");
        }
    }
}
