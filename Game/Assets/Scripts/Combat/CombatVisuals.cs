using UnityEngine;

namespace ZombieOverdrive.Combat
{
    public static class CombatVisuals
    {
        private static Sprite diamondSprite;

        public static void SpawnMuzzleFlash(Vector3 position, Vector2 direction, Color color, float size = 0.35f)
        {
            GameObject flash = new GameObject("Muzzle Flash");
            flash.transform.position = position;
            flash.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateDiamondSprite();
            renderer.color = color;
            renderer.sortingOrder = 13;
            flash.transform.localScale = new Vector3(size * 1.6f, size, 1f);
            Object.Destroy(flash, 0.07f);
        }

        public static void SpawnRing(Vector3 position, Color color, float size, float seconds)
        {
            GameObject ring = new GameObject("Impact Ring");
            ring.transform.position = position;
            LineRenderer line = ring.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = 28;
            line.startColor = color;
            line.endColor = color;
            line.startWidth = 0.04f;
            line.endWidth = 0.04f;
            line.sortingOrder = 12;

            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = Mathf.PI * 2f * i / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * size);
            }

            Object.Destroy(ring, seconds);
        }

        public static LineRenderer CreateTransientPolyline(string name, int pointCount, Color startColor, Color endColor, float width, float seconds)
        {
            GameObject lineObject = new GameObject(name);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.useWorldSpace = true;
            line.positionCount = pointCount;
            line.startColor = startColor;
            line.endColor = endColor;
            line.startWidth = width;
            line.endWidth = width;
            line.sortingOrder = 13;
            Object.Destroy(lineObject, seconds);
            return line;
        }

        public static void SpawnTelegraphLine(Vector3 start, Vector3 end, float width, float seconds, Color color)
        {
            LineRenderer line = CreateTransientPolyline("Attack Telegraph", 2, color, color, width, seconds);
            line.sortingOrder = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }

        public static void SpawnTelegraphCircle(Vector3 position, float radius, float seconds, Color color)
        {
            GameObject circle = new GameObject("Attack Circle Telegraph");
            circle.transform.position = position;
            LineRenderer line = circle.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = 36;
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, Mathf.Min(color.a, 0.18f));
            line.startWidth = 0.055f;
            line.endWidth = 0.055f;
            line.sortingOrder = 2;

            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = Mathf.PI * 2f * i / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
            }

            Object.Destroy(circle, seconds);
        }

        public static void SpawnExplosion(Vector3 position, Color color, float radius, float seconds)
        {
            SpawnRing(position, color, radius, seconds);
            SpawnRing(position, new Color(color.r, color.g, color.b, Mathf.Min(color.a * 0.45f, 0.45f)), radius * 0.55f, seconds * 0.85f);
        }

        private static Sprite CreateDiamondSprite()
        {
            if (diamondSprite != null)
            {
                return diamondSprite;
            }

            const int size = 16;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            int center = size / 2;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool fill = Mathf.Abs(x - center) + Mathf.Abs(y - center) <= 7;
                    texture.SetPixel(x, y, fill ? Color.white : clear);
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply(false);
            diamondSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 16f);
            return diamondSprite;
        }
    }
}
