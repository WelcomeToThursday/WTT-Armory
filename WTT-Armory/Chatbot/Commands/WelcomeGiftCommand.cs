using SPTarkov.Common.Logger;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Dialog;
using SPTarkov.Server.Core.Services.Commerce;
using SPTarkov.Server.Core.Services.Modding;
using WTTArmory.Models;

namespace WTTArmory.Chatbot.Commands
{
       [Injectable]
    public class WelcomeGiftCommand(
        MailSendService mailSendService,
        DialogueHelper dialogueHelper,
        ProfileDataService profileDataService,
        SptLogger<WelcomeGiftCommand> logger)
        : IWTTCommand
    {
        private const string ModKey = "WTT-Armory";
        private const int NormalGift = 3;
        private const int FirstWarn = 4;
        private const int FinalWarn = 5;
        private const int Punishment = 6;

        // ReSharper disable StringLiteralTypo
        public string Command => "welcomegift";
        public string CommandHelp => $"wtt {Command}\nSends a welcome gift to the player.";

        public async ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
        {
            var profileId = sessionId.ToString();

            var giftData = await profileDataService.GetProfileDataAsync<WTTWelcomeGiftData>(profileId, ModKey);
            if (giftData == null)
            {
                logger.Info("[WelcomeGiftCommand] Creating new WelcomeGiftData for profile.");
                giftData = new WTTWelcomeGiftData
                {
                    HasReceivedWelcomeGift = true
                };
            }

            giftData.WelcomeGiftUses++;
            var uses = giftData.WelcomeGiftUses;

            await profileDataService.SaveProfileDataAsync(profileId, ModKey, giftData);

            var dialogsInProfile = dialogueHelper.GetDialogsForProfile(profileId);
            var senderId = commandHandler.Id;
            if (!dialogsInProfile.ContainsKey(senderId))
            {
                dialogsInProfile[senderId] = new Dialogue
                {
                    Id = senderId,
                    Type = MessageType.UserMessage,
                    Messages = [],
                    Pinned = false,
                    New = 0,
                    AttachmentsNew = 0,
                    Users = [commandHandler]
                };
            }

            // ReSharper disable IdentifierTypo
            const string hipointTemplate = "679f2453d1970258c1df3fce";
            const string barrelTemplate = "679f24d15ff75ca48f6462a2";
            const string receiverTemplate = "679f24e723451339c041f28d";
            const string gripTemplate = "679f24e0434b197ed7a10468";
            const string magazineTemplate = "679f24edc0aba3ffa139b03f";
            const string spareMagTemplate = "679f24edc0aba3ffa139b03f";
            const string ammoBoxTemplate = "657025a4bfc87b3a34093250";

            // Behavior branching
            if (uses <= NormalGift)
            {
                SendNormalGift(sessionId, commandHandler, hipointTemplate, barrelTemplate, 
                    receiverTemplate, gripTemplate, magazineTemplate, spareMagTemplate, ammoBoxTemplate);
            }
            else if (uses == FirstWarn)
            {
                SendFirstWarning(sessionId, commandHandler, uses);
            }
            else if (uses == FinalWarn)
            {
                SendFinalWarning(sessionId, commandHandler, uses);
            }
            else if (uses == Punishment)
            {
                SendPunishment(sessionId, commandHandler, uses, hipointTemplate, barrelTemplate,
                    receiverTemplate, gripTemplate, magazineTemplate, spareMagTemplate, ammoBoxTemplate);
            }
            else // uses > PunishmentUses
            {
                SendDenial(sessionId, commandHandler, uses);
            }

            return request.DialogId;
        }

        private void SendNormalGift(MongoId sessionId, UserDialogInfo commandHandler,
            string hipointTemplate, string barrelTemplate, string receiverTemplate,
            string gripTemplate, string magazineTemplate, string spareMagTemplate, string ammoBoxTemplate)
        {
            var hipointId = new MongoId();
            var items = new List<Item>
            {
                new()
                {
                    Id = hipointId,
                    Template = hipointTemplate,
                    Upd = new Upd
                    {
                        Repairable = new UpdRepairable { MaxDurability = 100, Durability = 100 },
                        FireMode = new UpdFireMode { FireMode = "single" }
                    }
                },
                new() { Id = new MongoId(), Template = barrelTemplate, ParentId = hipointId, SlotId = "mod_barrel" },
                new() { Id = new MongoId(), Template = receiverTemplate, ParentId = hipointId, SlotId = "mod_reciever" },
                new() { Id = new MongoId(), Template = gripTemplate, ParentId = hipointId, SlotId = "mod_pistolgrip" },
                new() { Id = new MongoId(), Template = magazineTemplate, ParentId = hipointId, SlotId = "mod_magazine" },
                new() { Id = new MongoId(), Template = spareMagTemplate },
                new() { Id = new MongoId(), Template = spareMagTemplate },
                new() { Id = new MongoId(), Template = ammoBoxTemplate }
            };

            var normalMessage =
@"[SYSTEM NOTICE]
-> Status: ACTIVE
    WTT Armory Installation: DETECTED
    Initializing Welcome Protocol...
-> Status: DEPLOYING...
    Dispensing Complimentary Item...
-> Status: COMPLETE
    User Input: NOT REQUIRED";

            var details = new SendMessageDetails
            {
                RecipientId = sessionId,
                Sender = MessageType.MessageWithItems,
                DialogType = MessageType.UserMessage,
                SenderDetails = commandHandler,
                MessageText = normalMessage,
                Items = items,
                ItemsMaxStorageLifetimeSeconds = 172800
            };

            mailSendService.SendMessageToPlayer(details);
        }

        private void SendFirstWarning(MongoId sessionId, UserDialogInfo commandHandler, int uses)
        {
            var mockMessage =
$@"[WTTBot NOTICE]
-> Detected excessive request volume.
-> Usage count: {uses}
-> Response: No hi-points for you. Take a breather, PMC.

They don't just grow on trees you know...";

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler, mockMessage);
        }

        private void SendFinalWarning(MongoId sessionId, UserDialogInfo commandHandler, int uses)
        {
            var warningMessage =
$@"[WTTBot WARNING]
-> Usage count: {uses}
-> This is your final warning, PMC. Abuse of this system will not be tolerated.
-> Next request will trigger... consequences.";

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler, warningMessage);
        }

        private void SendPunishment(MongoId sessionId, UserDialogInfo commandHandler, int uses,
            string hipointTemplate, string barrelTemplate, string receiverTemplate,
            string gripTemplate, string magazineTemplate, string spareMagTemplate, string ammoBoxTemplate)
        {
            var items = new List<Item>();

            for (int i = 0; i < 12; i++)
            {
                var hipointId = new MongoId();

                items.Add(new Item
                {
                    Id = hipointId,
                    Template = hipointTemplate,
                    Upd = new Upd
                    {
                        Repairable = new UpdRepairable { MaxDurability = 100, Durability = 100 },
                        FireMode = new UpdFireMode { FireMode = "single" }
                    }
                });

                items.Add(new Item { Id = new MongoId(), Template = barrelTemplate, ParentId = hipointId, SlotId = "mod_barrel" });
                items.Add(new Item { Id = new MongoId(), Template = receiverTemplate, ParentId = hipointId, SlotId = "mod_reciever" });
                items.Add(new Item { Id = new MongoId(), Template = gripTemplate, ParentId = hipointId, SlotId = "mod_pistolgrip" });
                items.Add(new Item { Id = new MongoId(), Template = magazineTemplate, ParentId = hipointId, SlotId = "mod_magazine" });
                items.Add(new Item { Id = new MongoId(), Template = spareMagTemplate });
                items.Add(new Item { Id = new MongoId(), Template = ammoBoxTemplate });
            }

            var punishmentMessage =
$@"[WTTBot - SPAM DETECTED - COMMENCING LOCKDOWN OF GIFT SYSTEM]
-> Usage count: {uses}
-> Response: Compensation protocol C-12 initiated.
-> Dispensing 12x Hi-Points. 

You asked for it.";

            var details = new SendMessageDetails
            {
                RecipientId = sessionId,
                Sender = MessageType.MessageWithItems,
                DialogType = MessageType.UserMessage,
                SenderDetails = commandHandler,
                MessageText = punishmentMessage,
                Items = items,
                ItemsMaxStorageLifetimeSeconds = 172800
            };

            mailSendService.SendMessageToPlayer(details);
        }

        private void SendDenial(MongoId sessionId, UserDialogInfo commandHandler, int uses)
        {
            var denialMessage =
$@"[WTTBot - ACCESS DENIED]
-> Usage count: {uses}
-> Gift system permanently locked.
-> Please contact your local Therapist for psychological evaluation.";

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler, denialMessage);
        }
    }
}
