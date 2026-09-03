using EFT;
using UnityEngine;

namespace SPT.Hitmarker.Models;

public sealed class DamageEvent
{
    public string AmmoName;
    public float ArmorDamage;
    public string AttackerId;
    public string AttackerName;
    public EPlayerSide AttackerSide;
    public bool Blocked;
    public float BodyDamage;
    public string BodyPart;
    public float DamageAmount;
    public EDamageType DamageType;
    public float DistanceMeters;
    public bool IsArmorHit;
    public bool IsHeadshot;
    public bool IsLocalAttacker;
    public bool IsLocalVictim;
    public bool Ricochet;
    public float Time;
    public string VictimId;
    public bool VictimIsDead;
    public string VictimName;
    public string WeaponLabel;
    public Vector3? WorldPos;
}
