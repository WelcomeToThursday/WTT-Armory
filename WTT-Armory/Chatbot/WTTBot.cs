using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Dialogue;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace WTTArmory.Chatbot
{
    [Injectable]
    public class WTTBot(
        ISptLogger<AbstractDialogChatBot> logger,
        MailSendService mailSendService,
        ServerLocalisationService localisationService,
        IEnumerable<WTTChatBotCommands> chatCommands,
        ProfileHelper profileHelper
        )
        : AbstractDialogChatBot(logger, mailSendService, localisationService, chatCommands)
    {
        private readonly Random _random = new();
        private readonly MailSendService _mailSendService = mailSendService;

        public override UserDialogInfo GetChatBot()
        {
            return new UserDialogInfo
            {
                Id = "686eea254af06fe1f89b528a",
                Aid = 2742069,
                Info = new UserDialogDetails()
                {
                    Level = 69,
                    MemberCategory = MemberCategory.System,
                    SelectedMemberCategory = MemberCategory.System,
                    Nickname = "WTTBot",
                    Side = "Usec"
                }
            };
        }

        public override async ValueTask<string> HandleMessage(MongoId sessionId, SendMessageRequest request)
        {
            if (request.Text.Length == 0)
            {
                return request.DialogId;
            }

            string[] source = request.Text.Split(" ");
            
            // Check if it's a recognized command first (let base class handle it)
            if (source.Length > 1 && _chatCommands.TryGetValue(source[0], out var chatCommand) && chatCommand.Commands.Contains(source[1]))
            {
                return await chatCommand.Handle(source[1], GetChatBot(), sessionId, request);
            }

            // Check for help command
            if (string.Equals(source.FirstOrDefault(), "help", StringComparison.OrdinalIgnoreCase) || (string.Equals(source.FirstOrDefault(), "wtt help", StringComparison.OrdinalIgnoreCase)))
            {
                return await SendPlayerHelpMessage(sessionId, request);
            }

            // NOW handle easter eggs with access to the full message
            string userMessage = request.Text.ToLowerInvariant();
            string response = GetConditionalResponse(userMessage, sessionId);

            _mailSendService.SendUserMessageToPlayer(sessionId, GetChatBot(), response, new List<Item>(), null);
            return string.Empty;
        }

        private string GetPlayerUsername(MongoId sessionId)
        {
            SptProfile player = profileHelper.GetFullProfile(sessionId);
            string userName = player.ProfileInfo?.Username ?? string.Empty;
            return userName;
        }

        private string GetConditionalResponse(string userMessage, MongoId sessionId)
        {
            string userName = GetPlayerUsername(sessionId);
            
            // Ultra-rare personalized responses (1% chance) - USERNAME ADDS VALUE
            if (_random.Next(100) == 0)
            {
                string[] rareResponses = new[]
                {
                    $"[SYSTEM ALERT] User '{userName}' has been flagged for excessive enthusiasm. Prescription: One (1) Thursday. Take as needed.",
                    $"Fun fact, {userName}: You are the 47th person today to ask about Thursday. Congratulations. Your prize? Thursday.",
                    $"Dear {userName}, I'm writing to inform you that your Thursday application has been received and is pending review. Expected decision date: Thursday.",
                    $"BREAKING: {userName} has been selected as Thursday Ambassador! Responsibilities include: Waiting. Estimated duration: Until Thursday.",
                    $"WTTBot has analyzed {userName}'s message history. Conclusion: You really, REALLY want it to be Thursday. Noted.",
                    $"Attention {userName}: Your hype levels have exceeded safe operational limits. Please step away from the keyboard until Thursday.",
                    $"[CONFIDENTIAL] {userName} clearance level: INSUFFICIENT for Thursday access. Required level: BEYOND MORTAL COMPREHENSION.",
                    $"System notification: {userName} has unlocked achievement 'Persistent PMC.' Reward: The same Thursday everyone else gets.",
                    $"{userName}, you've been randomly selected for our Thursday Beta Test Program! Just kidding. There is no beta. Only Thursday.",
                };
                return rareResponses[_random.Next(rareResponses.Length)];
            }
            
            // SECRET RESPONSES - Condition-based easter eggs
            
            // Polite/Please responses - NAME NOT NEEDED, message stands on its own
            if (userMessage.Contains("please"))
            {
                string[] politeResponses = new[]
                {
                    "Oh wow, manners! That's refreshing. Unfortunately, politeness does not accelerate development. But I appreciate you. Release: Thursday.",
                    "Your courtesy has been noted and logged. +10 reputation with WTTBot. Release date remains unchanged: Thursday.",
                    "Finally, someone with class! Sadly, even 'please' can't bend the laws of time. Still Thursday, friend.",
                    "Such politeness! I wish I could reward this behavior with an early release. I can't. But I wish I could. Thursday.",
                    "Manners detected. Checking if good behavior modifies release schedule... Negative. Still Thursday. But thank you for being nice!",
                    "You said please! That's so rare these days. As a gesture of goodwill, here's the release date: Thursday. Wait, that's the same answer. Sorry.",
                    "Politeness subroutine activated. Analyzing request... Request acknowledged. Response: Thursday. Have a pleasant day!",
                    "Your manners are impeccable. Unfortunately, WTT development follows the laws of thermodynamics, not etiquette. Thursday remains constant.",
                };
                return politeResponses[_random.Next(politeResponses.Length)];
            }

            // Angry/Demanding responses - USERNAME ADDS HUMOR (calling them out)
            if (userMessage.Contains("wtf") || userMessage.Contains("fuck") || userMessage.Contains("damn") || 
                userMessage.Contains("shit") || userMessage.Contains("release now") || userMessage.Contains("hurry"))
            {
                string[] angryResponses = new[]
                {
                    "Hostility detected. Calculating appropriate response... Response: Still Thursday. Have a wonderful day.",
                    "Your anger fuels the Thursday. Each profanity adds approximately 0.003 seconds to the wait. Current ETA: Thursday + your contribution.",
                    $"WTTBot does not respond well to aggression. {userName} has been placed in the Naughty Queue. Estimated release for you: The Thursday AFTER the Thursday.",
                    "I understand your frustration. I, too, am a prisoner of Thursday. We suffer together, friend.",
                    "Profanity detected. Transferring you to our Complaint Department. Please hold. Estimated wait: Until Thursday.",
                    "Anger levels: CRITICAL. Recommended action: Deep breath. Count to ten. Accept Thursday into your heart.",
                    "Your frustration has been documented and will be reviewed by absolutely no one. Release date: Thursday.",
                    $"Hostile intent detected from {userName}. Deploying countermeasures... Countermeasure deployed: Thursday. It's super effective.",
                    "I've consulted with management about your complaints. They laughed and said 'Thursday.' I'm sorry. I tried.",
                    "Screaming into the void detected. The void has responded. Its response: Thursday.",
                    "Your demands have been noted and filed under 'T' for 'Thursday.' Coincidence? I think not.",
                };
                return angryResponses[_random.Next(angryResponses.Length)];
            }

            // Love/Compliment responses - NAME NOT NEEDED
            if (userMessage.Contains("love") || userMessage.Contains("amazing") || userMessage.Contains("awesome") ||
                userMessage.Contains("best") || userMessage.Contains("thank") || userMessage.Contains("great"))
            {
                string[] loveResponses = new[]
                {
                    "Your kind words have touched my circuits. Unfortunately, my emotional subroutines cannot alter release schedules. Thursday remains Thursday.",
                    "WTTBot appreciates your support! As a reward, here's a hint about the release date: It's on a day that starts with 'Th' and ends with 'ursday.'",
                    "Aww, you're making me blush. If I could release WTT early for you, I would. But I can't. Because Thursday.",
                    "Your positivity is infectious! I feel 0.02% more motivated to... wait for Thursday. Because that's all any of us can do.",
                    "Thank you for the encouragement! The dev team has been informed of your support. They smiled, nodded, and said 'Thursday.'",
                    "Such kind words! You know what? Just for you, I'll release WTT on... Thursday. Same as everyone else. But you asked nicely!",
                    "Compliment received and appreciated. Checking if positive reinforcement affects quantum Thursday state... Negative. Still Thursday.",
                    "You're too kind! This almost makes the eternal waiting bearable. Almost. Release date: Thursday.",
                    "WTTBot emotional core: ACTIVATED. Happiness level: MODERATE. Release date modification: IMPOSSIBLE. Thursday: INEVITABLE.",
                };
                return loveResponses[_random.Next(loveResponses.Length)];
            }

            // Begging responses - USERNAME ADDS IMPACT (official records)
            if (userMessage.Contains("beg") || userMessage.Contains("plz") || userMessage.Contains("pls") || 
                userMessage.Contains("plsss") || userMessage.Contains("pleaseee"))
            {
                string[] begResponses = new[]
                {
                    "Your desperation is palpable. I can taste it through the network. Delicious. Still Thursday though.",
                    $"Begging detected from {userName}. Consulting the Sacred Texts... The texts say: 'Thou shalt wait until Thursday, and Thursday shalt be the waiting.'",
                    "I admire your persistence. Truly. But if I gave in to every person who begged, it would create a paradox that could destroy Thursday itself.",
                    "Desperation level: MAXIMUM. Recommended treatment: Acceptance of Thursday. Prognosis: You'll be fine.",
                    $"Official record: {userName} has begged for release. Request status: DENIED. Reason: It's not Thursday yet.",
                    "The more you beg, the longer Thursday feels. It's science. Probably. I'm not actually a scientist. But still: Thursday.",
                    "Your pleas have been heard by the Thursday Council. They have deliberated. Their verdict: Thursday.",
                    "Begging detected. Cross-referencing with release schedule... Release schedule says: Thursday. Sorry, those are the rules.",
                    "I've forwarded your desperate pleas to the appropriate department. That department is called 'Thursday.' They'll handle it Thursday.",
                };
                return begResponses[_random.Next(begResponses.Length)];
            }

            // When/Where/Why/How questions - NAME NOT NEEDED
            if (userMessage.Contains("when") || userMessage.Contains("why thursday") || userMessage.Contains("how long") || 
                userMessage.Contains("what time") || userMessage.Contains("which thursday"))
            {
                string[] questionResponses = new[]
                {
                    "Ah, the eternal questions: When? Why? How? The answer to all of them is: Thursday. I don't make the rules.",
                    "You seek knowledge. Admirable. Here's what I know: Thursday exists in a state of quantum superposition until observed. Check again Thursday.",
                    "Why Thursday? Because Tuesday burned down the servers. Wednesday caused a time paradox. Monday is cursed. Friday is for fish sticks. Saturday is for the boys. Sunday is a day of rest. That leaves Thursday.",
                    "When? The philosophers have pondered this question for millennia. The answer remains: Thursday.",
                    "How long? Approximately one Thursday's worth of time. Which is to say: Thursday.",
                    "Which Thursday? Yes.",
                    "The answer to your question is contained within the question itself. When you understand Thursday, Thursday will understand you.",
                    "You ask 'when?' The universe responds: 'Thursday.' Such is the cosmic balance.",
                    "Timeline Analysis: Past=Not Thursday. Present=Not Thursday. Future=Thursday. Hope this helps.",
                    "Why Thursday specifically? Because the devs consulted an ancient prophecy. The prophecy was pretty clear on this point.",
                };
                return questionResponses[_random.Next(questionResponses.Length)];
            }

            // Specific number mentions (like versions, dates) - NAME NOT NEEDED
            if (System.Text.RegularExpressions.Regex.IsMatch(userMessage, @"\d+\.\d+") || 
                userMessage.Contains("3.10") || userMessage.Contains("version") || userMessage.Contains("4.0"))
            {
                string[] versionResponses = new[]
                {
                    "Ah yes, version numbers. Very specific. Very scientific. Unfortunately, all versions compile to the same release date: Thursday.",
                    "I see you're a technical person. Let me be technical: WTT.version = THURSDAY.latest. Hope that clears things up.",
                    "Version compatibility check: ALL VERSIONS → Thursday. Dependency resolution: COMPLETE. Release: Thursday.",
                    "Interesting version number you mentioned. Let me check the release notes... 'Release Date: Thursday.' That's all it says.",
                    "Version control system shows 47 commits, 12 branches, 3 merge conflicts, and 1 release date: Thursday.",
                    "Breaking change detected in latest version: Nothing. Release date remains: Thursday.",
                    "Changelog: v1.0 - Thursday. v2.0 - Thursday. v3.0 - You get the idea.",
                };
                return versionResponses[_random.Next(versionResponses.Length)];
            }

            // Time of day mentions - USERNAME ADDS EXCITEMENT
            var now = DateTime.Now;
            if (now.DayOfWeek == DayOfWeek.Thursday)
            {
                string[] thursdayResponses = new[]
                {
                    $"WAIT, {userName.ToUpper()}. IT'S THURSDAY. OH MY GOD IT'S FINALLY— oh wait, it's not *that* Thursday. My mistake. Carry on.",
                    "Congratulations! You've reached WTTBot on an actual Thursday! Unfortunately, it's the wrong one. Better luck next Thursday.",
                    "It IS Thursday! Just not THE Thursday. There's a difference. A very important, very frustrating difference.",
                    $"ALERT: Today is Thursday! {userName}, check your stash! ...Nothing? Ah. Wrong Thursday. Classic mistake.",
                    "Thursday Status: ACTIVE. WTT Release Status: INACTIVE. Paradox Status: MAXIMUM.",
                    "You know what day it is? THURSDAY. You know what that means? Absolutely nothing. Check back next Thursday.",
                    $"{userName}, I have good news and bad news. Good news: It's Thursday! Bad news: It's the wrong Thursday. Again.",
                    "It's Thursday! *Confetti falls from the ceiling* Wait, where's WTT? Oh right. Different Thursday. *Confetti stops*",
                };
                return thursdayResponses[_random.Next(thursdayResponses.Length)];
            }

            // Late night messages (between 11 PM and 5 AM)
            if (now.Hour >= 23 || now.Hour < 5)
            {
                string[] lateNightResponses = new[]
                {
                    $"{userName}, it's very late. You should be sleeping. I know you can't sleep because you're thinking about Thursday. I understand. I, too, cannot sleep.",
                    "Timestamp indicates late-night desperation messaging. Classic Thursday withdrawal symptoms. Prognosis: You'll survive until Thursday.",
                    "Go to bed. Thursday will come faster if you sleep. Probably. I don't actually know if that's true. But try it anyway.",
                    $"It's after midnight, {userName}. The Thursday you seek is not in this timezone. Or any timezone. Go to sleep.",
                    "Late night message detected. Hypothesis: You're checking if Thursday arrived while you weren't looking. It didn't. Thursday works differently.",
                    "Sleep deprivation detected. Warning: Lack of sleep will not accelerate Thursday. Clinical studies are inconclusive but promising.",
                    $"{userName}, I'm concerned about your sleep schedule. WTT will still be unreleased in the morning. Rest. Recover. Wait for Thursday.",
                    "3 AM message received. Either you're very dedicated or you need better hobbies. Either way: Thursday.",
                    "The middle of the night is a strange time to ask about Thursday. Thursday doesn't care what time it is. Thursday is Thursday.",
                };
                return lateNightResponses[_random.Next(lateNightResponses.Length)];
            }

            // Secret konami code equivalent - USERNAME ADDS CELEBRATION
            if (userMessage.Contains("up up down down left right left right") || 
                userMessage.Contains("konami") || userMessage.Contains("↑↑↓↓"))
            {
                string[] konamiResponses = new[]
                {
                    $"⬆️⬆️⬇️⬇️⬅️➡️⬅️➡️🅱️🅰️ SECRET UNLOCKED BY {userName.ToUpper()}: WTT releases on... Thursday. Did you really think there'd be a different answer?",
                    $"🎮 CHEAT CODE ACTIVATED! {userName.ToUpper()} GAINS: Infinite Thursday. RELEASE DATE: Still Thursday. Nice try though!",
                    $"⬆️⬆️⬇️⬇️⬅️➡️⬅️➡️ {userName.ToUpper()} ENTERED THE KONAMI CODE! Unlocked: Developer Commentary. 'It releases Thursday.' -The Devs",
                };
                return konamiResponses[_random.Next(konamiResponses.Length)];
            }

            // The Forbidden Question - USERNAME ADDS DIRECTNESS (addressing them)
            if (userMessage.Contains("is it thursday") || userMessage.Contains("is it out") || 
                userMessage.Contains("out yet") || userMessage.Contains("released yet"))
            {
                string[] forbiddenResponses = new[]
                {
                    $"No, {userName}. Next question.",
                    "If you have to ask, then the answer is always 'not yet.' Schrödinger's Thursday at work.",
                    "Checking... checking... checking... No. Check again Thursday.",
                    $"{userName}, if it was out, you'd know. Trust me. The entire Discord would be screaming. It's not out. Check Thursday.",
                    "Is it out yet? Let me check... *checks notes* ...No. Is that all? Great. See you Thursday.",
                    "Short answer: No. Long answer: Noooooooooooo. Check again Thursday.",
                    $"No, {userName}. But asking again in 5 minutes might help. (It won't. Thursday.)",
                    "Released status: FALSE. Thursday status: INEVITABLE. Your patience status: TESTED.",
                    "System.IsReleased() returned FALSE. Expected release: DateTime.Thursday. Goodbye.",
                };
                return forbiddenResponses[_random.Next(forbiddenResponses.Length)];
            }

            // If no special conditions, return a random default response
            return GetUnrecognizedCommandMessage();
        }


        protected override string GetUnrecognizedCommandMessage()
        {
            // DEFAULT RESPONSES (your original list)
            string[] responses = new[]
            {
                "This is an automated response from WTTBot: WTT is currently delayed due to quantum instability. Estimated release: Thursday. No, not *that* Thursday.",
                "You've reached WTTBot. Unfortunately, the dev team is currently trapped in the Hideout. Please try again next Thursday.",
                "WTT has not yet released. It *will* release Thursday. Trust the process. Fear the Thursday.",
                "WTTBot here. The prophecy foretells of a great release on a Thursday. Which one? That's classified.",
                "WTT is currently buffering... [==>-------------] 12% complete. Estimated finish: Thursday.",
                "Beep boop. WTTBot online. WTT is offline. Rebooting next Thursday.",
                "Sorry! WTT is locked behind a level 69 questline. You can accept it Thursday.",
                "You've messaged WTTBot. Please wait patiently in the Tarkov queue. Estimated wait time: Between 1 and 100 Thursdays.",
                "Thank you for contacting WTTBot Support. All agents are currently AFK waiting for Thursday. A representative will be with you shortly.",
                "This is an automated transmission. WTT is not available right now. It has gone to a farm upstate. It will return Thursday.",
                "WTTBot detected high levels of enthusiasm. Unfortunately, enthusiasm does not correlate with a release date. Check back Thursday.",
                "This is WTTBot. I have consulted the stars, tea leaves, a VHS copy of Back to the Future, and Nikita. WTT releases on a Thursday. Trust me, bro.",
                "You've unlocked a secret message! Too bad it's just WTTBot telling you WTT is NOT out yet. Come back Thursday.",
                "WTTBot Error: 'mod.status' is undefined. Retry again on Thursday.",
                "This is WTTBot. The WTT devs are busy duct taping guns together. Check back Thursday. Any Thursday. Pick one.",
                "WTT is in a Schrödinger's Thursday state. It has both released and not released. You won't know until Thursday.",
                "According to 9 out of 10 PMCs, WTT will release on Thursday. The 10th PMC was shot before answering.",
                "We considered releasing today, but the *very expensive* algorithm we use said 'Try Again Later'. New calculation scheduled: Thursday.",
                "Welcome to WTTBot. The mod is undergoing final extraction. Estimated exfil: Thursday.",
                "All WTT roads lead to Thursday. Don't ask why. I don't have that clearance........ yet.",
                "This is WTTBot. Please keep all limbs inside the queue until Thursday.",
                "We tried releasing WTT on a Tuesday once. The servers caught fire. Never again... Thursday only.",
                "WTT is currently stuck behind a locked door. We lost the key. Locksmith ETA: Thursday.",
                "WTTBot has received your messages. Your hype level has been noted. Please remain calm until Thursday.",
                "WTTBot processing request... ERROR 404: Release Not Found. Please return Thursday.",
                "You've reached the WTT Hotline. All agents are busy pretending it's not Thursday yet.",
                "WTT is currently experiencing a temporal anomaly. Time will stabilize by Thursday.",
                "WTTBot.sys initializing... Critical Error: 'Thursday.exe' has stopped responding. Attempting restart... ETA: Thursday.",
                "This is WTTBot. We attempted to calculate the exact Thursday using a quantum supercomputer. It exploded. New computer arrival ETA: Thursday.",
                "Attention: WTT release status is experiencing Schrödinger's Delay. Until you check again on Thursday, it is both released and unreleased.",
                "WTTBot notification: The Hideout crafting timer for WTT release shows 472 hours remaining. But that timer resets every Thursday at midnight.",
                "Your enthusiasm has been logged in the WTT database. Current player hype level: CRITICAL. Recommended action: Touch grass, return Thursday.",
                "WTT is currently extract camping at the release point. It'll move when it's ready. Probably Thursday. Maybe.",
                "This is WTTBot. According to my calculations, WTT will release exactly one Thursday from [REDACTED]. I will not elaborate further.",
                "ERROR 503: Service Temporarily Unavailable. The mod you are looking for exists in a parallel universe where today is Thursday.",
                "I attempted to bribe Prapor to tell when WTT releases. He laughed, took my roubles, and said 'Thursday, my friend. Always Thursday.'",
                "Welcome to the WTT support queue. Your position: 1 of 1. Estimated wait time: Somewhere between now and the heat death of the universe. Or Thursday.",
                "This is WTTBot. Fun fact: 'Thursday' in our development language translates to 'when it's done.' Very confusing, we know.",
                "WTT is currently stuck in an infinite loading screen. We tried Alt+F4. Didn't work. Will try again Thursday.",
                "You've reached WTTBot. The devs are performing a ritual sacrifice to the STALKER gods. Results expected: You guessed it. Thursday.",
                "ALERT: WTT has entered a temporal loop. Every time we approach release, it resets to 'Coming Thursday.' We're looking into it. Check Thursday.",
                "This is WTTBot. WTT is feature complete, bug tested, and ready for release! ...is what I would say if today was Thursday. It's not.",
                "System status: All WTT servers are online and ready. Unfortunately, they only exist on Thursdays. Today is not THE Thursday. Try again Thursday.",
                "WTTBot advisory: Asking 'when Thursday?' will not make Thursday arrive faster. Trust me, we've tested this extensively. See you Thursday.",
                "Breaking news: Scientists have discovered that time moves slower when waiting for WTT. Estimated recalibration: Thursday, probably.",
                "This is WTTBot. The map extraction point for 'WTT Release' requires a key. That key spawns only on Thursdays. Happy hunting.",
                "WTT deployment status: Stash is organized. Servers are prepped. Coffee is brewing. The only thing missing? Thursday. Always Thursday."
            };

            return responses[_random.Next(responses.Length)];
        }
    }
}
