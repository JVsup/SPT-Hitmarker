using UnityEngine;

namespace SPT.Hitmarker.Utilities;

internal static class GLDrawer
{
    private static GUIStyle _style;
    private static Texture2D _texture;

    private static GUIStyle Style(int size, TextAnchor anchor, Font font)
    {
        _style ??= new GUIStyle(GUI.skin.label);
        _style.fontSize = size;
        _style.alignment = anchor;
        _style.font = font;
        _style.richText = false;
        return _style;
    }

    private static Texture2D Texture()
    {
        if (_texture != null)
        {
            return _texture;
        }

        _texture = new Texture2D(1, 1, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Bilinear
        };
        _texture.SetPixel(0, 0, Color.white);
        _texture.Apply();
        return _texture;
    }

    public static void DrawLine(Vector2 start, Vector2 end, float width, Color color)
    {
        Vector2 difference = end - start;
        float angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        float length = difference.magnitude;
        var rectangle = new Rect(start.x, start.y, length, width);
        Matrix4x4 matrix = GUI.matrix;
        GUI.color = color;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(rectangle, Texture());
        GUI.matrix = matrix;
    }

    public static void DrawCross(
        float centerX,
        float centerY,
        float size,
        float thickness,
        Color baseColor,
        HitmarkerStyle style,
        Color headshotColor,
        bool pulse)
    {
        if (style == HitmarkerStyle.X)
        {
            DrawLine(
                new Vector2(centerX - size, centerY - size),
                new Vector2(centerX - size * 0.4f, centerY - size * 0.4f),
                thickness,
                baseColor);
            DrawLine(
                new Vector2(centerX + size, centerY - size),
                new Vector2(centerX + size * 0.4f, centerY - size * 0.4f),
                thickness,
                baseColor);
            DrawLine(
                new Vector2(centerX - size, centerY + size),
                new Vector2(centerX - size * 0.4f, centerY + size * 0.4f),
                thickness,
                baseColor);
            DrawLine(
                new Vector2(centerX + size, centerY + size),
                new Vector2(centerX + size * 0.4f, centerY + size * 0.4f),
                thickness,
                baseColor);
        }
        else
        {
            DrawLine(
                new Vector2(centerX - size, centerY),
                new Vector2(centerX - size * 0.4f, centerY),
                thickness,
                baseColor);
            DrawLine(
                new Vector2(centerX + size, centerY),
                new Vector2(centerX + size * 0.4f, centerY),
                thickness,
                baseColor);
            DrawLine(
                new Vector2(centerX, centerY - size),
                new Vector2(centerX, centerY - size * 0.4f),
                thickness,
                baseColor);
            DrawLine(
                new Vector2(centerX, centerY + size),
                new Vector2(centerX, centerY + size * 0.4f),
                thickness,
                baseColor);
        }

        if (!pulse)
        {
            return;
        }

        float pulseAlpha = Mathf.Abs(Mathf.Sin(Time.unscaledTime * 8f)) * 0.5f + 0.5f;
        Color pulseColor = headshotColor;
        pulseColor.a *= pulseAlpha;
        DrawLine(
            new Vector2(centerX - size, centerY - size),
            new Vector2(centerX + size, centerY + size),
            1f,
            pulseColor);
        DrawLine(
            new Vector2(centerX - size, centerY + size),
            new Vector2(centerX + size, centerY - size),
            1f,
            pulseColor);
    }

    public static void DrawText(
        string text,
        Vector2 position,
        int size,
        Color color,
        TextAnchor anchor,
        Font font,
        bool backdrop)
    {
        GUIStyle style = Style(size, anchor, font);
        var content = new GUIContent(text);
        Vector2 dimensions = style.CalcSize(content);
        var rectangle = new Rect(position.x, position.y, dimensions.x + 8f, dimensions.y + 2f);

        if (anchor is TextAnchor.MiddleCenter or TextAnchor.MiddleLeft or TextAnchor.MiddleRight)
        {
            rectangle.y -= rectangle.height * 0.5f;
        }

        if (anchor is TextAnchor.MiddleCenter or TextAnchor.UpperCenter or TextAnchor.LowerCenter)
        {
            rectangle.x -= rectangle.width * 0.5f;
        }

        if (anchor is TextAnchor.MiddleRight or TextAnchor.UpperRight or TextAnchor.LowerRight)
        {
            rectangle.x -= rectangle.width;
        }

        if (backdrop)
        {
            var backdropColor = new Color(0f, 0f, 0f, color.a * 0.4f);
            var backdropRectangle = new Rect(
                rectangle.x - 2f,
                rectangle.y - 1f,
                rectangle.width + 4f,
                rectangle.height + 2f);
            Color previousColor = GUI.color;
            GUI.color = backdropColor;
            GUI.DrawTexture(backdropRectangle, Texture());
            GUI.color = previousColor;
        }

        GUI.color = color;
        GUI.Label(rectangle, text, style);
    }

    public static float MeasureTextWidth(string text, int size, Font font)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0f;
        }

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = size,
            font = font
        };
        return style.CalcSize(new GUIContent(text)).x;
    }
}
