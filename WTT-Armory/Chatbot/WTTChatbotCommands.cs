using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialogue.Commando;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace WTTArmory.Chatbot
{
    [Injectable]
    public class WTTChatBotCommands : IChatCommand
    {
        private readonly IDictionary<string, IWTTCommand> _wttCommands;

        public WTTChatBotCommands(IEnumerable<IWTTCommand> wttCommands)
        {
            _wttCommands = wttCommands.ToDictionary(c => c.Command);
        }

        public string GetCommandHelp(string command)
        {
            return _wttCommands.TryGetValue(command, out var value) ? value.CommandHelp : string.Empty;
        }

        public string CommandPrefix => "wtt";

        public List<string> Commands => new List<string>(_wttCommands.Keys);

        public async ValueTask<string> Handle(string command, UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
        {
            if (!_wttCommands.TryGetValue(command, out var cmd))
            {
                return request.DialogId;
            }

            return await cmd.PerformAction(commandHandler, sessionId, request);
        }
    }
}