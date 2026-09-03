using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using SPT.Hitmarker.Models;
using SPT.Hitmarker.Utilities;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SPT.Hitmarker.Patches;

internal sealed class ActiveHealthApplyDamagePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ActiveHealthController).GetMethod(
            nameof(ActiveHealthController.ApplyDamage),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(EBodyPart), typeof(float), typeof(DamageInfo) },
            null);
    }

    private static string ResolveWeaponLabel(Item weaponItem)
    {
        if (weaponItem is not Weapon weapon)
        {
            return string.Empty;
        }

        ItemFactory itemFactory = Singleton<ItemFactory>.Instance;
        return itemFactory.BriefItemName(weapon, weapon.ShortName.Localized());
    }

    private static string ResolveAmmoName(string ammoTemplateId)
    {
        if (string.IsNullOrEmpty(ammoTemplateId))
        {
            return string.Empty;
        }

        ItemFactory itemFactory = Singleton<ItemFactory>.Instance;
        if (itemFactory.ItemTemplates.TryGetValue((MongoID)ammoTemplateId, out ItemTemplate template))
        {
            return template.ShortNameLocalizationKey.Localized();
        }

        return ammoTemplateId;
    }

    [PatchPostfix]
    private static void Postfix(
        ActiveHealthController __instance,
        EBodyPart __0,
        float __1,
        DamageInfo __2)
    {
        EBodyPart bodyPart = __0;
        float damageArgument = __1;
        DamageInfo damageInfo = __2;

        Player victim = __instance.Player;
        if (victim == null)
        {
            return;
        }

        Player attacker = damageInfo.Player?.iPlayer as Player;

        float bodyDamage = Mathf.Max(0f, damageInfo.DidBodyDamage);
        float armorDamage = Mathf.Max(0f, damageInfo.DidArmorDamage);

        if (bodyDamage <= 0.0001f && damageArgument > 0f)
        {
            bodyDamage = damageArgument;
        }

        bool blocked = damageInfo.BlockedBy.HasValue;
        bool deflected = damageInfo.DeflectedBy.HasValue;
        bool armorOnly = armorDamage > 0.01f && bodyDamage <= 0.01f || blocked || deflected;

        string weaponLabel = damageInfo.Weapon != null
            ? ResolveWeaponLabel(damageInfo.Weapon)
            : string.Empty;
        string ammoName = ResolveAmmoName(damageInfo.SourceId);

        float distance = 0f;
        try
        {
            Vector3 hitPoint = damageInfo.HitPoint;
            if (attacker != null && hitPoint != default)
            {
                distance = Vector3.Distance(attacker.Transform.position, hitPoint);
            }
            else if (attacker != null)
            {
                distance = Vector3.Distance(attacker.Transform.position, victim.Transform.position);
            }
        }
        catch
        {
            distance = 0f;
        }

        var damageEvent = new DamageEvent
        {
            AttackerId = attacker?.Profile?.ProfileId,
            AttackerName = attacker?.Profile?.Nickname ?? "Unknown",
            AttackerSide = attacker != null ? attacker.Side : EPlayerSide.Savage,
            VictimId = victim.Profile?.ProfileId,
            VictimName = victim.Profile?.Nickname ?? "Unknown",
            BodyPart = bodyPart.ToString(),
            DamageAmount = damageArgument,
            BodyDamage = bodyDamage,
            ArmorDamage = armorDamage,
            IsLocalAttacker = attacker != null && ReferenceEquals(attacker, State.LocalPlayer),
            IsLocalVictim = ReferenceEquals(victim, State.LocalPlayer),
            IsHeadshot = bodyPart == EBodyPart.Head,
            VictimIsDead = victim.HealthController != null && !victim.HealthController.IsAlive,
            WorldPos = damageInfo.HitPoint,
            WeaponLabel = weaponLabel,
            AmmoName = ammoName,
            IsArmorHit = armorOnly,
            Ricochet = deflected,
            Blocked = blocked,
            DistanceMeters = distance
        };

        if (Settings.DebugLog.Value && damageEvent.AttackerName != "Unknown")
        {
            Plugin.Log.LogInfo(
                $"Hit -> {damageEvent.VictimName} [{damageEvent.BodyPart}] " +
                $"dmg:{damageEvent.DamageAmount:0.##} body:{damageEvent.BodyDamage:0.##} " +
                $"armor:{damageEvent.ArmorDamage:0.##} armorHit:{damageEvent.IsArmorHit} " +
                $"ric:{damageEvent.Ricochet} blk:{damageEvent.Blocked} " +
                $"({damageEvent.WeaponLabel} / {damageEvent.AmmoName}) " +
                $"dist:{damageEvent.DistanceMeters:0}m");
        }

        if (damageEvent.VictimIsDead)
        {
            EventBus.RaiseKill(damageEvent);
        }
        else if (damageEvent.IsHeadshot)
        {
            EventBus.RaiseHeadshot(damageEvent);
        }
        else
        {
            EventBus.RaiseDamage(damageEvent);
        }
    }
}
