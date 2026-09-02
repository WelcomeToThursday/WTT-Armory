using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static EFT.Player;
using static EFT.Player.FirearmController;

namespace WTTArmoryClient.Patches
{

    ///<summary>Hyper-burst can reduce recoil too much, need to artifically induce spread.</summary>
    public class ShotVectorPatch : ModulePatch
    {
        private static FieldInfo _playerField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(FirearmController).GetMethod("AdjustShotVectors", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void Postfix(FirearmController __instance, ref Vector3 direction)
        {
            Player player = (Player)_playerField.GetValue(__instance);
            if (!player.IsYourPlayer || !Plugin.GunHasHyperburst)
                return;

            Logger.LogInfo(Plugin.ROFShotCount);

            // Don't apply to first shot
            // This patch occurs earlier than ShootPatch, so ROFShotCount will start at 0
            bool applySpread = Plugin.ROFShotCount >= 1 && Plugin.ROFShotCount <= Plugin.ShotThreshold.Value;

            if (!applySpread)
                return;

            float verticalSpread = Plugin.BurstSpread.Value * 0.00005f; // Scale down, value needs to be very small to avoid excessive spread
            verticalSpread = UnityEngine.Random.Range(verticalSpread * 0.45f, verticalSpread);
            float horizontalSpread = verticalSpread * 0.75f;
            horizontalSpread *= UnityEngine.Random.value < 0.5f ? -1f : 1f; // Randomly apply left or right spread

            direction = new Vector3(
                direction.x + horizontalSpread,
                direction.y + verticalSpread,
                direction.z
            );
        }
    }


    ///<summary>Actual firerate timing is handled in this Update() method. Firerate is used elsewhere for sound effects</summary>
    public class FireRatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AutomaticFireOperation).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(AutomaticFireOperation __instance)
        {
            bool isYourPlayer = __instance.Player.IsYourPlayer;

            if (!isYourPlayer || !Plugin.GunHasHyperburst || __instance.Weapon.SelectedFireMode == Weapon.EFireMode.single)
                return;

            bool doHyperBurst = Plugin.ROFShotCount <= Plugin.ShotThreshold.Value;
            float burstMulti = doHyperBurst ? Plugin.BurstROFMulti.Value : 1f;

            //Float_5 is used to store the weapon's fire rate for lifetime of FC. When player switches weapon, this values would get reset.
            //So it's set here to allow for a dynamic fire rate.
            __instance._shotsTime = 60f / (int)(__instance.Weapon.FireRate * burstMulti);
        }
    }

    ///<summary>Firerate value is used elsewhere for sound effects etc., but for actual firerate timing it's only checked once so GClass2029.Update() patch is also needed</summary>
    public class AutoFireRatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Weapon).GetMethod("get_FireRate", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(Weapon __instance, ref int __result)
        {
            bool isYourPlayer = __instance?.Owner?.ID != null && __instance.Owner.ID == Singleton<GameWorld>.Instance?.MainPlayer?.ProfileId;

            if (!isYourPlayer || !Plugin.GunHasHyperburst || __instance.SelectedFireMode == Weapon.EFireMode.single)
                return true;

            bool doHyperBurst = Plugin.ROFShotCount <= Plugin.ShotThreshold.Value;
            float burstMulti = doHyperBurst ? Plugin.BurstROFMulti.Value : 1f;

            __result = (int)(__instance.GetTemplate<WeaponTemplate>().bFirerate * burstMulti);
            return false;
        }
    }

    ///<summary>Entry point used to determine the player has fired a shot</summary>
    public class ShootPatch : ModulePatch
    {
        private static FieldInfo _playerField;
        private static FieldInfo _fcField;

        private static bool DoHyperBurst(Player player)
        {
            return player != null && player.IsYourPlayer && Plugin.GunHasHyperburst && player.MovementContext.CurrentState.Name != EPlayerState.Stationary;
        }

        protected override MethodBase GetTargetMethod()
        {
            ShootPatch._playerField = AccessTools.Field(typeof(Player.FirearmController), "_player");
            ShootPatch._fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(ProceduralWeaponAnimation).GetMethod("Shoot", BindingFlags.Instance | BindingFlags.Public);
        }

        ///<summary>Prefix timing is too early for reliable ROF adjustment but is needed to adjust recoil strength</summary>
        [PatchPrefix]
        private static void PatchPrefix(ProceduralWeaponAnimation __instance, ref float str)
        {
            Player.FirearmController firearmController = (Player.FirearmController)ShootPatch._fcField.GetValue(__instance);
            if (firearmController == null)
                return;

            Player player = (Player)ShootPatch._playerField.GetValue(firearmController);
            if (!DoHyperBurst(player))
                return;

            Plugin.IsFiring = true;
            Plugin.ShotTimer = 0f;
            Plugin.RecoilShotCount++;

            bool isDoingHyperBurst = firearmController.Item.SelectedFireMode != Weapon.EFireMode.single && Plugin.RecoilShotCount <= Plugin.ShotThreshold.Value;

            str *= isDoingHyperBurst ? Plugin.BurstRecoilMulti.Value : 1f;
        }

        ///<summary>Postfix timing works for ROF adjustment</summary>
        [PatchPostfix]
        private static void PatchPostFix(ProceduralWeaponAnimation __instance, ref float str)
        {
            Player.FirearmController firearmController = (Player.FirearmController)ShootPatch._fcField.GetValue(__instance);
            if (firearmController == null)
                return;

            Player player = (Player)ShootPatch._playerField.GetValue(firearmController);
            if (!DoHyperBurst(player))
                return;

            Plugin.IsFiring = true;
            Plugin.ShotTimer = 0f;
            Plugin.ROFShotCount++;
        }
    }

    ///<summary>Is called when weapon is equipped, used as entry point to determine if the weapon is hyper-burst-able</summary>
    public class UpdateWeaponVariablesPatch : ModulePatch
    {
        private static FieldInfo _playerField;
        private static FieldInfo _fcField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(Player.FirearmController), "_player");
            _fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(ProceduralWeaponAnimation).GetMethod("UpdateWeaponVariables", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(ProceduralWeaponAnimation __instance)
        {
            Player.FirearmController firearmController = (Player.FirearmController)UpdateWeaponVariablesPatch._fcField.GetValue(__instance);
            if (firearmController == null)
                return;

            Player player = (Player)UpdateWeaponVariablesPatch._playerField.GetValue(firearmController);

            if (player == null || !player.IsYourPlayer)
                return;

            Plugin.GunHasHyperburst = Plugin.GunIDs.Contains(firearmController.Weapon.TemplateId.ToString());
        }
    }
}