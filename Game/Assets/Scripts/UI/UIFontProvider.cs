using UnityEngine;
using UnityEngine.UI;

namespace ZombieOverdrive.UI
{
    public static class UIFontProvider
    {
        private const string FontResourcePath = "Fonts/SourceHanSansSC-Regular";
        private static Font cachedFont;

        public static Font Font
        {
            get
            {
                if (cachedFont == null)
                {
                    cachedFont = Resources.Load<Font>(FontResourcePath);
                }

                return cachedFont != null
                    ? cachedFont
                    : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        public static void ApplyTo(Text text)
        {
            if (text == null)
            {
                return;
            }

            text.font = Font;
        }

        public static void ApplyToChildren(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Text[] texts = root.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                ApplyTo(texts[i]);
            }
        }
    }
}
