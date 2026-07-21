using UnityEngine;
using UnityEngine.UI;

public static class DemoUiFactory
{
    public static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.layer = parent.gameObject.layer;
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.localScale = Vector3.one;
        return gameObject;
    }

    public static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject gameObject = CreateUiObject(name, parent);
        Image image = gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    public static Text CreateText(string name, Transform parent, string value, int fontSize, TextAnchor alignment, Color color)
    {
        Text text = CreateUiObject(name, parent).AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.text = value;
        text.raycastTarget = false;
        return text;
    }

    public static Button CreateButton(string name, Transform parent, Sprite sprite, string label)
    {
        Image image = CreateImage(name, parent, sprite, new Color(0.18f, 0.21f, 0.28f, 0.98f));
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.28f, 0.5f, 0.72f, 1f);
        colors.pressedColor = new Color(0.16f, 0.4f, 0.64f, 1f);
        button.colors = colors;
        Text text = CreateText("Label", button.transform, label, 22, TextAnchor.MiddleCenter, Color.white);
        Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
        return button;
    }

    public static void Stretch(RectTransform rect, float left, float right, float bottom, float top)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    public static void SetAnchoredRect(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
