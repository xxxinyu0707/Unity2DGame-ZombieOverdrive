using UnityEngine;

namespace ZombieOverdrive.Combat
{
    public static class CombatVisuals
    {
        private static Sprite diamondSprite;
        private static Sprite discSprite;

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
            SpawnSoftBurst(position, color, radius, seconds, 10, true);
        }

        public static void SpawnCrateBreak(Vector3 position)
        {
            SpawnSoftBurst(position, new Color(1f, 0.72f, 0.28f, 0.85f), 0.9f, 0.22f, 12, true);
        }

        public static void SpawnBombBlast(Vector3 position, float radius)
        {
            SpawnSoftBurst(position, new Color(1f, 0.82f, 0.18f, 0.78f), radius, 0.34f, 22, true);
            SpawnSoftBurst(position, new Color(1f, 0.25f, 0.08f, 0.52f), radius * 0.58f, 0.24f, 14, false);
        }

        public static void SpawnSingularityCollapse(Vector3 position, float radius, bool evolved)
        {
            SpawnSoftBurst(position, evolved ? new Color(0.92f, 0.32f, 1f, 0.82f) : new Color(0.56f, 0.28f, 1f, 0.62f), radius * (evolved ? 0.95f : 0.7f), evolved ? 0.32f : 0.22f, evolved ? 24 : 14, false);
            GameObject root = new GameObject("Singularity Collapse");
            root.transform.position = position;
            Object.Destroy(root, evolved ? 0.42f : 0.28f);

            int streakCount = evolved ? 18 : 10;
            for (int i = 0; i < streakCount; i++)
            {
                float angle = i * Mathf.PI * 2f / streakCount + Random.Range(-0.12f, 0.12f);
                float start = radius * Random.Range(0.45f, 0.95f);
                Vector3 from = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * start;
                LineRenderer line = CreateTransientPolyline("Collapse Streak", 2, new Color(0.92f, 0.55f, 1f, 0.68f), new Color(0.18f, 0f, 0.28f, 0.05f), evolved ? 0.07f : 0.045f, evolved ? 0.32f : 0.22f);
                line.transform.SetParent(root.transform, false);
                line.useWorldSpace = false;
                line.SetPosition(0, from);
                line.SetPosition(1, from * Random.Range(0.12f, 0.26f));
            }
        }

        public static void SpawnSoftBurst(Vector3 position, Color color, float radius, float seconds, int particleCount, bool shards)
        {
            GameObject root = new GameObject("Soft Burst");
            root.transform.position = position;
            Object.Destroy(root, seconds);

            SpriteRenderer core = CreateDiscRenderer(root.transform, "Burst Core", color, radius * 0.9f, 0f, 12);
            FadeAndExpand fade = core.gameObject.AddComponent<FadeAndExpand>();
            fade.Initialize(core, color, radius * 1.25f, seconds, 0.52f);

            for (int i = 0; i < particleCount; i++)
            {
                float angle = i * Mathf.PI * 2f / particleCount + Random.Range(-0.18f, 0.18f);
                float distance = Random.Range(radius * 0.12f, radius * 0.95f);
                float size = Random.Range(radius * 0.035f, radius * 0.095f);
                Color particleColor = new Color(
                    Mathf.Clamp01(color.r + Random.Range(-0.08f, 0.08f)),
                    Mathf.Clamp01(color.g + Random.Range(-0.12f, 0.12f)),
                    Mathf.Clamp01(color.b + Random.Range(-0.08f, 0.08f)),
                    Random.Range(0.36f, 0.82f));

                SpriteRenderer particle = CreateDiscRenderer(root.transform, shards ? "Burst Shard" : "Burst Puff", particleColor, size, 0.02f + i * 0.001f, 13);
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;
                particle.transform.localPosition = offset;
                particle.transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + Random.Range(-35f, 35f));
                if (shards)
                {
                    particle.transform.localScale = new Vector3(size * Random.Range(0.55f, 0.85f), size * Random.Range(1.4f, 2.4f), 1f);
                }

                FadeAndDrift drift = particle.gameObject.AddComponent<FadeAndDrift>();
                drift.Initialize(particle, particleColor, offset.normalized * Random.Range(radius * 0.75f, radius * 1.55f), seconds * Random.Range(0.68f, 1f), shards ? Random.Range(120f, 260f) : Random.Range(-35f, 35f));
            }
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

        private static SpriteRenderer CreateDiscRenderer(Transform parent, string name, Color color, float size, float zOffset, int sortingOrder)
        {
            GameObject visual = new GameObject(name);
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = new Vector3(0f, 0f, zOffset);
            visual.transform.localScale = Vector3.one * size;
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateDiscSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static Sprite CreateDiscSprite()
        {
            if (discSprite != null)
            {
                return discSprite;
            }

            const int size = 24;
            float center = (size - 1) * 0.5f;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01((1f - distance) * 1.55f);
                    texture.SetPixel(x, y, distance <= 1f ? new Color(1f, 1f, 1f, alpha) : clear);
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply(false);
            discSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            return discSprite;
        }

        private sealed class FadeAndExpand : MonoBehaviour
        {
            private SpriteRenderer target;
            private Color color;
            private float targetSize;
            private float duration;
            private float startScale;
            private float age;

            public void Initialize(SpriteRenderer renderer, Color initialColor, float finalSize, float seconds, float alphaScale)
            {
                target = renderer;
                color = new Color(initialColor.r, initialColor.g, initialColor.b, initialColor.a * alphaScale);
                targetSize = finalSize;
                duration = Mathf.Max(0.03f, seconds);
                startScale = renderer.transform.localScale.x;
            }

            private void Update()
            {
                if (target == null)
                {
                    return;
                }

                age += Time.deltaTime;
                float t = Mathf.Clamp01(age / duration);
                float scale = Mathf.Lerp(startScale, targetSize, Mathf.SmoothStep(0f, 1f, t));
                target.transform.localScale = Vector3.one * scale;
                target.color = new Color(color.r, color.g, color.b, color.a * (1f - t));
            }
        }

        private sealed class FadeAndDrift : MonoBehaviour
        {
            private SpriteRenderer target;
            private Color color;
            private Vector3 velocity;
            private float duration;
            private float spin;
            private float age;

            public void Initialize(SpriteRenderer renderer, Color initialColor, Vector3 driftVelocity, float seconds, float spinSpeed)
            {
                target = renderer;
                color = initialColor;
                velocity = driftVelocity;
                duration = Mathf.Max(0.03f, seconds);
                spin = spinSpeed;
            }

            private void Update()
            {
                if (target == null)
                {
                    return;
                }

                age += Time.deltaTime;
                float t = Mathf.Clamp01(age / duration);
                target.transform.localPosition += velocity * Time.deltaTime * (1f - t * 0.6f);
                target.transform.Rotate(0f, 0f, spin * Time.deltaTime);
                target.color = new Color(color.r, color.g, color.b, color.a * (1f - t));
            }
        }
    }
}
