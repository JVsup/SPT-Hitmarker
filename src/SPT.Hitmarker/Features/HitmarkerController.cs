using System.Collections.Generic;
using System.Linq;
using SPT.Hitmarker.Models;
using SPT.Hitmarker.Utilities;
using UnityEngine;

namespace SPT.Hitmarker.Features;

public sealed class HitmarkerController : MonoBehaviour
{
    private readonly List<HitEntry> _hits = new();
    private readonly List<Rect> _occupied = new();
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        float now = Time.unscaledTime;
        if (_hits.Count == 0)
        {
            return;
        }

        float retention = Mathf.Max(
            Settings.HitmarkerFadeSeconds.Value,
            Settings.NumbersLifetimeSeconds.Value);
        _hits.RemoveAll(hit => now - hit.Event.Time > retention);
    }

    private void OnEnable()
    {
        EventBus.OnDamage += OnDamage;
        EventBus.OnHeadshot += OnHeadshot;
        EventBus.OnKill += OnKill;
    }

    private void OnDisable()
    {
        EventBus.OnDamage -= OnDamage;
        EventBus.OnHeadshot -= OnHeadshot;
        EventBus.OnKill -= OnKill;
    }

    private void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        _occupied.Clear();

        if (Settings.HitmarkerEnabled.Value)
        {
            DrawHitmarker();
        }

        if (Settings.NumbersEnabled.Value)
        {
            DrawNumbers();
        }
    }

    private void OnDamage(DamageEvent damageEvent)
    {
        if (!damageEvent.IsLocalAttacker)
        {
            return;
        }

        _hits.Add(new HitEntry { Event = damageEvent });
        SoundBank.PlayHit();
    }

    private void OnHeadshot(DamageEvent damageEvent)
    {
        if (!damageEvent.IsLocalAttacker)
        {
            return;
        }

        _hits.Add(new HitEntry { Event = damageEvent });
        SoundBank.PlayHeadshot();
    }

    private void OnKill(DamageEvent damageEvent)
    {
        if (!damageEvent.IsLocalAttacker)
        {
            return;
        }

        _hits.Add(new HitEntry { Event = damageEvent, IsKill = true });
        SoundBank.PlayKill();
    }

    private static Color PickHitColor(DamageEvent damageEvent, bool isKill)
    {
        if (isKill)
        {
            return Settings.HitmarkerKillColor.Value;
        }

        if (damageEvent.IsHeadshot)
        {
            return Settings.HeadshotColor.Value;
        }

        if (damageEvent.IsArmorHit && damageEvent.BodyDamage <= 0.01f)
        {
            return Settings.ArmorHitColor.Value;
        }

        return Settings.HitmarkerColor.Value;
    }

    private void DrawHitmarker()
    {
        HitEntry last = _hits.LastOrDefault();
        if (last == null)
        {
            return;
        }

        DamageEvent damageEvent = last.Event;
        float age = Time.unscaledTime - damageEvent.Time;
        if (age > Settings.HitmarkerFadeSeconds.Value)
        {
            return;
        }

        float fade = Mathf.Clamp01(1f - age / Settings.HitmarkerFadeSeconds.Value);
        Color color = PickHitColor(damageEvent, last.IsKill);
        color.a = Settings.HitmarkerOpacity.Value * fade;

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;
        float markerSize = Settings.HitmarkerSizePx.Value;

        if (Settings.HitmarkerStyleMode.Value == HitmarkerStyle.Image)
        {
            Texture2D texture = last.IsKill
                ? TextureBank.HitmarkerKill()
                : damageEvent.IsHeadshot
                    ? TextureBank.HitmarkerHeadshot()
                    : TextureBank.Hitmarker();

            float imageSize = markerSize * 2f;
            if (texture)
            {
                var rectangle = new Rect(
                    centerX - imageSize,
                    centerY - imageSize,
                    imageSize * 2f,
                    imageSize * 2f);
                Color previousColor = GUI.color;
                GUI.color = color;
                GUI.DrawTexture(rectangle, texture, ScaleMode.ScaleToFit, true);
                GUI.color = previousColor;
                return;
            }
        }

        bool pulse = damageEvent.IsHeadshot && Settings.HeadshotPulse.Value || last.IsKill;
        GLDrawer.DrawCross(
            centerX,
            centerY,
            markerSize,
            2f,
            color,
            Settings.HitmarkerStyleMode.Value,
            Settings.HeadshotColor.Value,
            pulse);
    }

    private void DrawNumbers()
    {
        if (_hits.Count == 0)
        {
            return;
        }

        float now = Time.unscaledTime;
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;
        List<HitEntry> visible = _hits
            .Where(hit => now - hit.Event.Time <= Settings.NumbersLifetimeSeconds.Value)
            .OrderBy(hit => hit.Event.Time)
            .ToList();

        if (!Settings.NumbersAtHitPosition.Value)
        {
            int row = 0;
            float offsetX = Settings.HitmarkerSizePx.Value + 12f;
            foreach (HitEntry hit in visible)
            {
                DrawNumberLine(
                    hit.Event,
                    new Vector2(centerX + offsetX, centerY),
                    row,
                    true,
                    TextAnchor.MiddleLeft);
                row++;
            }

            return;
        }

        foreach (HitEntry hit in visible)
        {
            Vector2 position = ComputeScreenPointForHit(
                hit.Event.WorldPos,
                Settings.NumbersEdgeClampPadding.Value);
            int fontSize = Settings.NumbersFontSize.Value;
            const TextAnchor anchor = TextAnchor.MiddleCenter;
            Rect preferred = MeasureFullLineRect(hit.Event, position, fontSize, anchor);
            Rect placed = PlaceWithoutOverlap(preferred, fontSize + 6f, 20);
            var drawPosition = new Vector2(position.x, placed.center.y);
            DrawNumberLine(hit.Event, drawPosition, 0, true, anchor);
        }
    }

    private static Rect MeasureFullLineRect(
        DamageEvent damageEvent,
        Vector2 basePosition,
        int fontSize,
        TextAnchor anchor)
    {
        BuildLineStrings(damageEvent, out string main, out string armor, out string ricochet, out _);

        float mainWidth = GLDrawer.MeasureTextWidth(main, fontSize, Settings.Font);
        float armorWidth = string.IsNullOrEmpty(armor)
            ? 0f
            : GLDrawer.MeasureTextWidth(armor, fontSize, Settings.Font);
        float ricochetWidth = string.IsNullOrEmpty(ricochet)
            ? 0f
            : GLDrawer.MeasureTextWidth(ricochet, fontSize, Settings.Font);

        const float mainToArmorPadding = 16f;
        const float armorToRicochetPadding = 8f;
        float width = mainWidth
                      + (armorWidth > 0f ? mainToArmorPadding + armorWidth : 0f)
                      + (ricochetWidth > 0f ? armorToRicochetPadding + ricochetWidth : 0f)
                      + 8f;
        float height = fontSize + 6f;
        var rectangle = new Rect(basePosition.x, basePosition.y, width, height);

        switch (anchor)
        {
            case TextAnchor.MiddleCenter:
                rectangle.x -= rectangle.width * 0.5f;
                rectangle.y -= rectangle.height * 0.5f;
                break;
            case TextAnchor.MiddleLeft:
                rectangle.y -= rectangle.height * 0.5f;
                break;
            case TextAnchor.MiddleRight:
                rectangle.x -= rectangle.width;
                rectangle.y -= rectangle.height * 0.5f;
                break;
        }

        return rectangle;
    }

    private Rect PlaceWithoutOverlap(Rect preferred, float stepY, int maxSteps)
    {
        if (!IntersectsAny(preferred))
        {
            _occupied.Add(preferred);
            return preferred;
        }

        for (int index = 1; index <= maxSteps; index++)
        {
            float direction = index % 2 == 1 ? 1f : -1f;
            float offset = direction * Mathf.Ceil(index * 0.5f) * stepY;
            Rect candidate = preferred;
            candidate.y += offset;
            candidate.y = Mathf.Clamp(candidate.y, 0f, Screen.height - candidate.height);

            if (IntersectsAny(candidate))
            {
                continue;
            }

            _occupied.Add(candidate);
            return candidate;
        }

        _occupied.Add(preferred);
        return preferred;
    }

    private bool IntersectsAny(Rect rectangle)
    {
        for (int index = 0; index < _occupied.Count; index++)
        {
            if (rectangle.Overlaps(_occupied[index]))
            {
                return true;
            }
        }

        return false;
    }

    private static void BuildLineStrings(
        DamageEvent damageEvent,
        out string main,
        out string armor,
        out string ricochet,
        out int damageInteger)
    {
        int fleshInteger = Mathf.RoundToInt(Mathf.Max(0f, damageEvent.BodyDamage));
        int armorInteger = Mathf.RoundToInt(Mathf.Max(0f, damageEvent.ArmorDamage));

        string template = Settings.NumbersTemplate.Value ?? "{dmg}";
        bool usesSplitTokens = template.Contains("{flesh}") || template.Contains("{armor}");

        if (usesSplitTokens)
        {
            string flesh = fleshInteger > 0 ? fleshInteger.ToString() : string.Empty;
            string armorValue = armorInteger > 0 ? armorInteger.ToString() : string.Empty;
            string baseMain = template
                .Replace("{flesh}", flesh)
                .Replace("{bp}", damageEvent.BodyPart ?? string.Empty)
                .Replace("{dmg}", (fleshInteger + armorInteger).ToString());

            if (template.Contains("{armor}"))
            {
                main = baseMain.Replace("{armor}", string.Empty);
                armor = armorInteger > 0 ? "(" + armorValue + ")" : string.Empty;
            }
            else
            {
                main = baseMain;
                armor = string.Empty;
            }

            main = main.Replace("( )", string.Empty).Replace("()", string.Empty);
            while (main.Contains("  "))
            {
                main = main.Replace("  ", " ");
            }

            main = main.Trim();
            ricochet = damageEvent.Ricochet ? "Ricochet" : string.Empty;
            damageInteger = fleshInteger;
            return;
        }

        damageInteger = Mathf.RoundToInt(Mathf.Max(
            0f,
            damageEvent.BodyDamage > 0f ? damageEvent.BodyDamage : damageEvent.DamageAmount));
        main = template
            .Replace("{dmg}", damageInteger.ToString())
            .Replace("{bp}", damageEvent.BodyPart ?? string.Empty);
        armor = armorInteger >= 1 ? "(" + armorInteger + ")" : string.Empty;
        ricochet = damageEvent.Ricochet ? "Ricochet" : string.Empty;
    }

    private static void DrawNumberLine(
        DamageEvent damageEvent,
        Vector2 basePosition,
        int rowOffset,
        bool riseByAge,
        TextAnchor anchor)
    {
        float age = Time.unscaledTime - damageEvent.Time;
        float lifetimeProgress = Mathf.Clamp01(age / Settings.NumbersLifetimeSeconds.Value);
        float fade = 1f - lifetimeProgress;
        float rise = riseByAge
            ? Mathf.Lerp(0f, Settings.NumbersRisePixels.Value, lifetimeProgress)
            : 0f;
        int fontSize = Settings.NumbersFontSize.Value;

        Color mainColor = PickHitColor(damageEvent, damageEvent.VictimIsDead);
        mainColor.a *= fade;

        BuildLineStrings(
            damageEvent,
            out string mainText,
            out string armorText,
            out string ricochetText,
            out _);

        var position = new Vector2(
            basePosition.x,
            basePosition.y - rise + rowOffset * (fontSize + 4f));
        GLDrawer.DrawText(
            mainText,
            position,
            fontSize,
            mainColor,
            anchor,
            Settings.Font,
            Settings.NumbersBackdrop.Value);

        float mainWidth = GLDrawer.MeasureTextWidth(mainText, fontSize, Settings.Font);
        float armorWidth = string.IsNullOrEmpty(armorText)
            ? 0f
            : GLDrawer.MeasureTextWidth(armorText, fontSize, Settings.Font);
        const float mainToArmorPadding = 16f;
        const float armorToRicochetPadding = 8f;

        if (!string.IsNullOrEmpty(armorText))
        {
            Color armorColor = Settings.ArmorHitColor.Value;
            armorColor.a *= fade;

            Vector2 armorPosition;
            TextAnchor armorAnchor;
            if (anchor == TextAnchor.MiddleRight)
            {
                armorPosition = new Vector2(position.x - mainWidth - mainToArmorPadding, position.y);
                armorAnchor = TextAnchor.MiddleRight;
            }
            else if (anchor == TextAnchor.MiddleCenter)
            {
                armorPosition = new Vector2(position.x + mainWidth * 0.5f + mainToArmorPadding, position.y);
                armorAnchor = TextAnchor.MiddleLeft;
            }
            else
            {
                armorPosition = new Vector2(position.x + mainWidth + mainToArmorPadding, position.y);
                armorAnchor = TextAnchor.MiddleLeft;
            }

            GLDrawer.DrawText(
                armorText,
                armorPosition,
                fontSize,
                armorColor,
                armorAnchor,
                Settings.Font,
                Settings.NumbersBackdrop.Value);

            if (!string.IsNullOrEmpty(ricochetText))
            {
                Color ricochetColor = Settings.NumbersColor.Value;
                ricochetColor.a *= fade;

                Vector2 ricochetPosition;
                TextAnchor ricochetAnchor;
                if (anchor == TextAnchor.MiddleRight)
                {
                    ricochetPosition = new Vector2(armorPosition.x - armorToRicochetPadding, position.y);
                    ricochetAnchor = TextAnchor.MiddleRight;
                }
                else
                {
                    ricochetPosition = new Vector2(
                        armorPosition.x + armorWidth + armorToRicochetPadding,
                        position.y);
                    ricochetAnchor = TextAnchor.MiddleLeft;
                }

                GLDrawer.DrawText(
                    ricochetText,
                    ricochetPosition,
                    fontSize,
                    ricochetColor,
                    ricochetAnchor,
                    Settings.Font,
                    Settings.NumbersBackdrop.Value);
            }
        }
        else if (!string.IsNullOrEmpty(ricochetText))
        {
            Color ricochetColor = Settings.NumbersColor.Value;
            ricochetColor.a *= fade;

            Vector2 ricochetPosition;
            TextAnchor ricochetAnchor;
            if (anchor == TextAnchor.MiddleRight)
            {
                ricochetPosition = new Vector2(position.x - mainWidth - armorToRicochetPadding, position.y);
                ricochetAnchor = TextAnchor.MiddleRight;
            }
            else if (anchor == TextAnchor.MiddleCenter)
            {
                ricochetPosition = new Vector2(
                    position.x + mainWidth * 0.5f + armorToRicochetPadding,
                    position.y);
                ricochetAnchor = TextAnchor.MiddleLeft;
            }
            else
            {
                ricochetPosition = new Vector2(position.x + mainWidth + armorToRicochetPadding, position.y);
                ricochetAnchor = TextAnchor.MiddleLeft;
            }

            GLDrawer.DrawText(
                ricochetText,
                ricochetPosition,
                fontSize,
                ricochetColor,
                ricochetAnchor,
                Settings.Font,
                Settings.NumbersBackdrop.Value);
        }
    }

    private Vector2 ComputeScreenPointForHit(Vector3? worldPosition, float edgePadding)
    {
        if (!_camera || !worldPosition.HasValue)
        {
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        Vector3 viewportPosition = _camera.WorldToViewportPoint(worldPosition.Value);
        if (viewportPosition.z < 0f)
        {
            float x = viewportPosition.x < 0.5f ? edgePadding : 1f - edgePadding;
            const float y = 0.5f;
            return new Vector2(x * Screen.width, (1f - y) * Screen.height);
        }

        viewportPosition.x = Mathf.Clamp(viewportPosition.x, edgePadding, 1f - edgePadding);
        viewportPosition.y = Mathf.Clamp(viewportPosition.y, edgePadding, 1f - edgePadding);
        return new Vector2(
            viewportPosition.x * Screen.width,
            (1f - viewportPosition.y) * Screen.height);
    }

    private sealed class HitEntry
    {
        public DamageEvent Event;
        public bool IsKill;
    }
}
