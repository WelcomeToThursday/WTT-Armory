using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using UnityEngine;
using WTTArmoryClient.Patches;
using WTTArmoryClient.Properties;

namespace WTTArmoryClient
{
    [BepInPlugin("com.wtt.armory", "WTT-Armory", "2.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static GameWorld GameWorld;
        public static string[] GunIDs = ["678fe4a4906c7bd23722c71f", "679a6a534f3d279c99b135b9"];
        
        // Hyperburst configuration
        public static ConfigEntry<float> BurstROFMulti { get; private set; }
        public static ConfigEntry<float> BurstRecoilMulti { get; private set; }
        public static ConfigEntry<float> ShotResetDelay { get; private set; }
        public static ConfigEntry<int> ShotThreshold { get; private set; }
        public static ConfigEntry<int> BurstSpread { get; private set; }

        // Hyperburst state
        public static bool GunHasHyperburst { get; set; }
        public static bool IsFiring { get; set; }

        ///<summary> Shot count used to determine if recoil bonus should apply</summary>
        public static int RecoilShotCount { get; set; }

        ///<summary>Shot count with different timing to determine shot count in Update</summary>
        public static int ROFShotCount { get; set; }

        ///<summary>Timer to determine if firing has stopped</summary>
        public static float ShotTimer { get; set; }
        public Player You { get; set; }

        private void Awake()
        {
            // Hyperburst configuration with ordering
            BurstROFMulti = Config.Bind(
                "Hyperburst",
                "Hyperburst ROF Multi",
                3f,
                new ConfigDescription("Rate of fire multiplier during hyperburst. If Rifle has base firerate of 600, a multiplier of 3x = 1800 fire rate during hyper-burst.",
                    new AcceptableValueRange<float>(1f, 4f),
                    new ConfigurationManagerAttributes { Order = 1 }
                )
            );

            BurstRecoilMulti = Config.Bind(
                "Hyperburst",
                "Hyperburst Recoil Multi",
                0.5f,
                new ConfigDescription("Recoil multiplier during hyperburst.",
                    new AcceptableValueRange<float>(0.1f, 2f),
                    new ConfigurationManagerAttributes { Order = 2 }
                )
            );

            ShotResetDelay = Config.Bind(
                "Hyperburst",
                "Shot Reset Delay",
                0.05f,
                new ConfigDescription("Time delay after firing to determine if firing has stopped. Adjust this for timing issues.",
                    new AcceptableValueRange<float>(0.01f, 2f),
                    new ConfigurationManagerAttributes { Order = 3 }
                )
            );

            ShotThreshold = Config.Bind(
                "Hyperburst",
                "Hyperburst Shot Threshold",
                1,
                new ConfigDescription("Shot count when hype-rburst ends (adjust for timing issues). If hyper-burst should be 2 rounds, this value should be 1 as ROF change carries over to the next shot.",
                    new AcceptableValueRange<int>(0, 5),
                    new ConfigurationManagerAttributes { Order = 4 }
                )
            );

            BurstSpread = Config.Bind(
                "Hyperburst",
                "Hyperburst Shot Spread",
                50,
                new ConfigDescription("Higher value means higher spread. Spread is needed otherwise shots land on top of each other regardless of recoil multiplier.",
                    new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes { Order = 5 }
                )
            );

            // Hyper-burst patches
            new UpdateWeaponVariablesPatch().Enable();
            new ShootPatch().Enable();
            new FireRatePatch().Enable();
            new AutoFireRatePatch().Enable();
            new ShotVectorPatch().Enable();
        }

        private void Update()
        {
            // Get player reference
            if (You == null)
            {
                GameWorld gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld?.MainPlayer != null && gameWorld.MainPlayer.IsYourPlayer) 
                    You = gameWorld.MainPlayer;
            }
            else
            {
                Player.FirearmController fc = You.HandsController as Player.FirearmController;
                //Update hyperburst state
                UpdateHyperburst(fc);
            }
        }

        ///<summary>Handles shot reset timing</summary>
        private void UpdateHyperburst(Player.FirearmController fc)
        {
            if (fc == null || !GunHasHyperburst) return;

            if (IsFiring)
            {
                // Reset timer after delay to determine if firing has stopped
                ShotTimer += Time.deltaTime;
                if (!fc.autoFireOn && ShotTimer >= ShotResetDelay.Value)
                {
                    ShotTimer = 0f;
                    IsFiring = false;
                    RecoilShotCount = 0;
                    ROFShotCount = 0;
                }
            }
        }
    }
}