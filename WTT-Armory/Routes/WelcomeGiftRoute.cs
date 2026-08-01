using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;
using WTTArmory.Chatbot.Callbacks;

namespace WTTArmory.Routes;

[Injectable(TypePriority = OnLoadOrder.Routers + 1)]
public class WTTBotWelcomeGiftStaticRouter(
    JsonUtil jsonUtil,
    WelcomeGiftCallback welcomeGiftCallback)
: StaticRouter(
    jsonUtil,
    [
        new RouteAction(
            "/client/friend/list",
            async (_, _, sessionId, output, _) =>
            {
                await welcomeGiftCallback.HandleWelcomeGift(sessionId.ToString());
                return output;
            })
    ]);