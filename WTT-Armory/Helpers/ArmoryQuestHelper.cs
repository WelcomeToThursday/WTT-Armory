using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Tables;
using WTTServerCommonLib.Helpers;

namespace WTTArmory.Helpers
{
    [Injectable]
    public class ArmoryQuestHelper(TemplateTable templateTable, ISptLogger<ArmoryQuestHelper> logger, QuestHelper questHelper)
    {

        // Define weapon IDs
        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo
        private const string BROWNING_AUTO5 = "669fca7f9ed4916116c76d5e";
        private const string BROWNING_AUTO5_BOSS = "6840ebf5b8687ba34f8dfbca";
        private const string SERBU_SUPER_SHORTY = "6761b213607f9a6f79017d23";
        private const string SVD_DRAGUNOV = "6657bc8faeddd6b0a9b40224";
        private const string SVD_DRAGUNOV_GREEN = "6657bd4d3a4d6e7c33fd2fdc";
        private const string BERETTA_92FS = "6868d249cdee524f8c0ba45f";
        private const string CZ75B = "6661012d16fbd2fb75408f87";
        private const string CZ75B_OMEGA = "67748ec2e6a045dc97e2f978";
        private const string HI_POINT_C9 = "679f2453d1970258c1df3fce";
        private const string HK_UCP = "68433b58a8f9a618b11082d4";
        private const string LAR_GRIZZLY_MK5 = "677c9a47baecf3c4b2453365";
        private const string LAR_GRIZZLY_MK5_GOLD = "677ca3e62e9e964a11a55d8e";
        private const string LAR_GRIZZLY_MK5_STAINLESS = "677ca334da2787e0538c882d";
        private const string ARMORY_PRODIGY = "665fe0e865683281eb8e7ed6";
        private const string SR2_UDAV = "68677d09339b397ed3d37522";
        private const string STACCATO_XC = "68452c3da87156b67d9ec538";
        private const string WILSON_COMBAT_EDC_X9 = "6761b213607f9a6f79017d52";
        private const string CZ_SCORPION_EVO = "687afda52dc9fd6c0e14c602";
        private const string MINEBEA_PM9 = "6761b213607f9a6f79017cd8";
        private const string AEK971 = "0cb4a36dd2e587b46e813dbe";
        private const string AEK973 = "686093d590c3dce07984c38a";
        private const string BOFORS_AK5C = "6671ebcdd32bd95eb398e920";
        private const string BOFORS_AK5D = "682d3dd16900cb35564c8825";
        private const string AN94 = "678fe4a4906c7bd23722c71f";
        private const string AN94_KIROV = "679a6a534f3d279c99b135b9";
        private const string HK417 = "664a5b945636ce820472f225";
        private const string HK_G3 = "664274a4d2e5fe0439d545a6";
        private const string MK23 = "68b7f4060a4536984f82cf4b";
        private const string AG042 = "68c23b960f579b9b5fe021e7";
        private const string AG043 = "68cf56067ff6ceab0c2fd49e";
        private const string HK_XM8 = "6920b28eabc4f9d229cb7e49";
        private const string HK_XM8FDE = "6920a431c8f2ed5000c540a0";
        private const string IWI_CARMEL = "66ba249b102a9dd6040a6e7e";
        private const string IWI_CARMEL_FDE = "66ba26a6925f9921573224c9";
        private const string IWI_TAVOR_X95 = "66a47e98c486ec9d1af3a4da";
        private const string IWI_TAVOR_X95_FDE = "66a544c956621d3364f6085e";
        private const string KNIGHTS_ARMAMENT_PDW = "6761b213607f9a6f79017c7e";
        private const string PATRIOT = "66839591f4d0cba7b041b2af";
        private const string REMINGTON_ACR = "67a01e4ea2b82626b73d10a3";
        private const string REMINGTON_ACR_FDE = "67a01e4ea2b82626b73d10a4";
        private const string WAGES_OF_SIN_AR15 = "676b4e2ff185a450a0b300b4";
        private const string WAGES_OF_SIN_AR15_RED = "6761b213607f9a6f79017d40";
        private const string WAGES_OF_SIN_AR15_FDE = "676c3cac01023283e5f6a562";
        private const string ZASTAVA_M76 = "66e88596febdcf9daade16a8";
        private const string REMINGTON_MSR = "684e32eaec9f5eb3cacc7ca7";
        private const string M700_XL_ACTION = "deee28079e76d537f532021c";
        private const string M700_XL_ACTION_NORMA = "1bf618e47cce6d69bec01e9f";
        private const string AXMC_300_WINMAG = "68a3836826dffa87b5767c04";
        private const string HENRY_BIG_BOY = "6924c2f800ff4b8d9d689a5c";
        private const string CHEYTAC_M200 = "68fd4feab87d77a5aaf6bf64";
        private const string CHEYTAC_M200_FDE = "68fd7a61f2b8d96bb3119b19";
        private const string CHEYTAC_M200_BOSS = "68fd7d14ff5a09197b5ab82a";
        private const string XM109 = "6945dc69fe52c2415de357f7";
        private const string M1894 = "69417a3f787395b8cc0ab13e";
        private const string PMM12 = "68d40fb07130ef271f60991f";
        private const string PITVIPER = "684b257022b8260501141308";
        private const string PITVIPERCHROME = "684b64658c4c5f37f2c4e56a";
        private const string RUGER57 = "692f77045371aadbbb0ada2e";
        private const string PSAROCK = "68bf41bbb786f6e9315a5cab";
        private const string RO991 = "690ebacfc047a9a9f1a98782";
        private const string MASADA = "69066bef905ee9e06c462009";
        private const string MASADAFDE = "6906a1aeef59ca68d128e8b7";
        private const string MASADABOSS = "69161a1d649768162e8219ef";
        private const string UMP9 = "67b05e25d83f07b7b587c0b5";
        private const string SV98M = "69236a0b2d1260dbca41ef92";
        private const string F2000 = "6962f22fddc6698c6309b620";
        private const string F2000FDE = "6969867592a994a633084f70";
        private const string A545 = "69555be939fb0a1a6f0ad12e";
        private const string AK107 = "695ab544a219218449041700";
        private const string GROT = "696ce75f73805e693401aba0";
        private const string GROTFDE = "696fe2ebcf7469bf3805173f";
        private const string SA01 = "69777201f57d45a2ec02aafc";
        
        
        // Weapon Mods

        private const string m700_PGW_Defense_C14_Timberwolf = "c8548cdf0e490383177f37c1";
        private const string  windtalker = "68fd62d343a142b14be0c3ec";
        private const string  windtalker_fde = "68fd63e8842f704b09062fd1";
        private const string  windtalker_titan= "68fd6401e11fcc19f3a8d365";
        private const string  msr_supp = "684e49af525c602815316f8d";
        private const string  susat= "2322ee962f6c77a1284abc1e";
        private const string  aimpoint_cs= "03f6c86c56945862820e1006";
        private const string  mars= "5ae3d18efa17cbd51e898ced";
        
        public void ModifyQuests()
        {
            var quests = templateTable.Quests;

            // ReSharper disable CommentTypo
            // ====================== PRAPOR QUESTS ======================

            // Punisher Part 4 (59ca264786f77445a80ed044)
            questHelper.AddWeaponsToKillCondition(quests, "59ca264786f77445a80ed044", [
                BROWNING_AUTO5, BROWNING_AUTO5_BOSS, SERBU_SUPER_SHORTY
            ]);

            // Punisher Part 6 (59ca2eb686f77445a80ed049)
            questHelper.AddWeaponsToKillCondition(quests, "59ca2eb686f77445a80ed049", [
                SVD_DRAGUNOV, SVD_DRAGUNOV_GREEN
            ]);

            // Mall Cop (64e7b99017ab941a6f7bf9d7)
            questHelper.AddWeaponsToKillCondition(quests, "64e7b99017ab941a6f7bf9d7", [
                BERETTA_92FS, CZ75B, CZ75B_OMEGA, HI_POINT_C9, HK_UCP,
                LAR_GRIZZLY_MK5, LAR_GRIZZLY_MK5_GOLD, LAR_GRIZZLY_MK5_STAINLESS,
                ARMORY_PRODIGY, SR2_UDAV, STACCATO_XC, WILSON_COMBAT_EDC_X9, MK23, PMM12, PITVIPERCHROME, PITVIPER, RUGER57, PSAROCK
            ]);

            // Tickets, Please (64e7b9a4aac4cd0a726562cb)
            questHelper.AddWeaponsToKillCondition(quests, "64e7b9a4aac4cd0a726562cb", [
                CZ_SCORPION_EVO, MINEBEA_PM9, RO991, UMP9
            ]);

            // District Patrol (64e7b9bffd30422ed03dad38)
            questHelper.AddWeaponsToKillCondition(quests, "64e7b9bffd30422ed03dad38", [
                AEK971, AEK973, BOFORS_AK5C, BOFORS_AK5D, AN94, AN94_KIROV,
                HK417, HK_G3, HK_XM8, IWI_CARMEL, IWI_CARMEL_FDE, IWI_TAVOR_X95,
                IWI_TAVOR_X95_FDE, KNIGHTS_ARMAMENT_PDW, PATRIOT, REMINGTON_ACR,
                REMINGTON_ACR_FDE, WAGES_OF_SIN_AR15, WAGES_OF_SIN_AR15_FDE,
                WAGES_OF_SIN_AR15_RED, ZASTAVA_M76, SVD_DRAGUNOV, SVD_DRAGUNOV_GREEN,
                AG042, AG043, HK_XM8FDE, MASADA, MASADAFDE, MASADABOSS, F2000, F2000FDE, GROT,GROTFDE, AK107, A545, SA01
            ]);

            // ====================== SKIER QUESTS ======================

            // Stirrup (596b455186f77457cb50eccb)
            questHelper.AddWeaponsToKillCondition(quests, "596b455186f77457cb50eccb", [
                BERETTA_92FS, CZ75B, CZ75B_OMEGA, HI_POINT_C9, HK_UCP,
                LAR_GRIZZLY_MK5, LAR_GRIZZLY_MK5_GOLD, LAR_GRIZZLY_MK5_STAINLESS,
                ARMORY_PRODIGY, SR2_UDAV, STACCATO_XC, WILSON_COMBAT_EDC_X9, MK23, PMM12, PITVIPERCHROME, PITVIPER, RUGER57, PSAROCK
            ]);

            // Silent Caliber (5c0bc91486f7746ab41857a2)
            questHelper.AddWeaponsToKillCondition(quests, "5c0bc91486f7746ab41857a2", [
                BROWNING_AUTO5, BROWNING_AUTO5_BOSS, SERBU_SUPER_SHORTY
            ]);

            // Setup (5c1234c286f77406fa13baeb)
            questHelper.AddWeaponsToKillCondition(quests, "5c1234c286f77406fa13baeb", [
                BROWNING_AUTO5, BROWNING_AUTO5_BOSS, SERBU_SUPER_SHORTY
            ]);

            // Connections Up North (6764174c86addd02bc033d68)
            questHelper.AddWeaponsToKillCondition(quests, "6764174c86addd02bc033d68", [
                REMINGTON_MSR, CHEYTAC_M200, CHEYTAC_M200_FDE, CHEYTAC_M200_BOSS,  M1894, M700_XL_ACTION, M700_XL_ACTION_NORMA, AXMC_300_WINMAG, HENRY_BIG_BOY, SV98M
            ]);

            // ====================== PEACEKEEPER QUESTS ======================

            // Spa Tour Part 1 (5a03153686f77442d90e2171)
            questHelper.AddWeaponsToKillCondition(quests, "5a03153686f77442d90e2171", [
                BROWNING_AUTO5, BROWNING_AUTO5_BOSS, SERBU_SUPER_SHORTY
            ]);

            // Worst Job (63a9b229813bba58a50c9ee5)
            questHelper.AddWeaponsToKillCondition(quests, "63a9b229813bba58a50c9ee5", [
                WAGES_OF_SIN_AR15, WAGES_OF_SIN_AR15_RED, WAGES_OF_SIN_AR15_FDE
            ]);

            // ====================== JAEGER QUESTS ======================

            var tarkovShooterWeapons = new[]
            {
                REMINGTON_MSR, CHEYTAC_M200, CHEYTAC_M200_FDE, CHEYTAC_M200_BOSS,  M1894, M700_XL_ACTION, M700_XL_ACTION_NORMA, AXMC_300_WINMAG, HENRY_BIG_BOY, SV98M
            };

            // Tarkov Shooter Part 1-8 (WEAPONS)
            questHelper.AddWeaponsToKillCondition(quests, "5bc4776586f774512d07cf05", tarkovShooterWeapons); // Part 1
            questHelper.AddWeaponsToKillCondition(quests, "5bc479e586f7747f376c7da3", tarkovShooterWeapons); // Part 2
            questHelper.AddWeaponsToKillCondition(quests, "5bc47dbf86f7741ee74e93b9", tarkovShooterWeapons); // Part 3
            questHelper.AddWeaponsToKillCondition(quests, "5bc480a686f7741af0342e29", tarkovShooterWeapons); // Part 4
            questHelper.AddWeaponsToKillCondition(quests, "5bc4826c86f774106d22d88b", tarkovShooterWeapons); // Part 5
            questHelper.AddWeaponsToKillCondition(quests, "5bc4836986f7740c0152911c", tarkovShooterWeapons); // Part 6
            questHelper.AddWeaponsToKillCondition(quests, "5bc4856986f77454c317bea7", tarkovShooterWeapons); // Part 7
            questHelper.AddWeaponsToKillCondition(quests, "5bc4893c86f774626f5ebf3e", tarkovShooterWeapons); // Part 8

            // Tarkov Shooter Part 1-8 (MODS)
            
            // Part 1
            questHelper.AddWeaponModToCondition(quests, "5bc4776586f774512d07cf05", mars, "5aa66be6e5b5b0214e506e97", false);
            questHelper.AddWeaponModToCondition(quests, "5bc4776586f774512d07cf05", susat, "5aa66be6e5b5b0214e506e97", false);
            questHelper.AddWeaponModToCondition(quests, "5bc4776586f774512d07cf05", aimpoint_cs, "5aa66be6e5b5b0214e506e97", false);
            
            // Part 7
            questHelper.AddWeaponModToCondition(quests, "5bc4856986f77454c317bea7", m700_PGW_Defense_C14_Timberwolf, "593d489686f7745c6255d58a", true);
            questHelper.AddWeaponModToCondition(quests, "5bc4856986f77454c317bea7", windtalker, "593d489686f7745c6255d58a", true);
            questHelper.AddWeaponModToCondition(quests, "5bc4856986f77454c317bea7", windtalker_fde, "593d489686f7745c6255d58a", true);
            questHelper.AddWeaponModToCondition(quests, "5bc4856986f77454c317bea7", windtalker_titan, "593d489686f7745c6255d58a", true);
            questHelper.AddWeaponModToCondition(quests, "5bc4856986f77454c317bea7", msr_supp, "593d489686f7745c6255d58a", true);

            
            // Claustrophobia (669fa3979b0ce3feae01a130)
            questHelper.AddWeaponsToKillCondition(quests, "669fa3979b0ce3feae01a130", [
                BROWNING_AUTO5, BROWNING_AUTO5_BOSS, SERBU_SUPER_SHORTY
            ]);

            // ====================== MECHANIC QUESTS ======================

            // Psycho Sniper (5c0be13186f7746f016734aa)
            questHelper.AddWeaponsToKillCondition(quests, "5c0be13186f7746f016734aa", [
                REMINGTON_MSR, CHEYTAC_M200, CHEYTAC_M200_FDE, CHEYTAC_M200_BOSS,  M1894, M700_XL_ACTION, M700_XL_ACTION_NORMA, AXMC_300_WINMAG, HENRY_BIG_BOY, SV98M
            ]);

            // Shooter Born in Heaven (5c0bde0986f77479cf22c2f8)
            questHelper.AddWeaponsToKillCondition(quests, "5c0bde0986f77479cf22c2f8", [
                REMINGTON_MSR, CHEYTAC_M200, CHEYTAC_M200_FDE, CHEYTAC_M200_BOSS,  M1894, M700_XL_ACTION, M700_XL_ACTION_NORMA, AXMC_300_WINMAG, HENRY_BIG_BOY, SV98M
            ]);

            // Make Amends Equipment (6261482fa4eb80027c4f2e11)
            questHelper.AddWeaponsToFindOrHandoverCondition(quests, "6261482fa4eb80027c4f2e11", [
                SVD_DRAGUNOV, SVD_DRAGUNOV_GREEN
            ]);
        }
    }
}
