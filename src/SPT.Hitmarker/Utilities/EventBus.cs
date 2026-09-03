using System;
using SPT.Hitmarker.Models;
using UnityEngine;

namespace SPT.Hitmarker.Utilities;

internal static class EventBus
{
    public static event Action<DamageEvent> OnDamage;
    public static event Action<DamageEvent> OnHeadshot;
    public static event Action<DamageEvent> OnKill;

    public static void RaiseDamage(DamageEvent damageEvent)
    {
        damageEvent.Time = Time.unscaledTime;
        OnDamage?.Invoke(damageEvent);
    }

    public static void RaiseHeadshot(DamageEvent damageEvent)
    {
        damageEvent.Time = Time.unscaledTime;
        OnHeadshot?.Invoke(damageEvent);
    }

    public static void RaiseKill(DamageEvent damageEvent)
    {
        damageEvent.Time = Time.unscaledTime;
        OnKill?.Invoke(damageEvent);
    }
}
