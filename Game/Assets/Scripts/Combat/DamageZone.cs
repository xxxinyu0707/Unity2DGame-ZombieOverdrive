using UnityEngine;
using ZombieOverdrive.Core;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.World;

namespace ZombieOverdrive.Combat
{
    public class DamageZone : MonoBehaviour
    {
        private enum TargetMode
        {
            Enemies,
            Player
        }

        private static readonly Collider2D[] Hits = new Collider2D[96];

        private TargetMode targetMode;
        private LayerMask targetMask;
        private float radius;
        private float damagePerSecond;
        private float duration;
        private float tickInterval;
        private float tickTimer;
        private float slowAmount;
        private float slowDuration;
        private LineRenderer ring;
        private SpriteRenderer fillRenderer;
        private SpriteRenderer[] emberRenderers;
        private Color visualColor;
        private float initialDuration;
        private float spinSeed;

        public static void SpawnEnemyZone(Vector3 position, float zoneRadius, float dps, float seconds, LayerMask mask, Color color, float slow = 0f)
        {
            SpawnEnemyZone(position, zoneRadius, dps, seconds, mask, color, slow, false);
        }

        public static void SpawnEnemyZone(Vector3 position, float zoneRadius, float dps, float seconds, LayerMask mask, Color color, float slow, bool fiery)
        {
            DamageZone zone = Create(position, zoneRadius, dps, seconds, color);
            zone.targetMode = TargetMode.Enemies;
            zone.targetMask = mask;
            zone.slowAmount = slow;
            zone.slowDuration = 0.35f;
            if (fiery)
            {
                zone.CreateFireVisuals(color);
            }
            else
            {
                zone.CreateHazardVisuals(color);
            }
        }

        public static void SpawnPlayerZone(Vector3 position, float zoneRadius, float dps, float seconds, Color color)
        {
            DamageZone zone = Create(position, zoneRadius, dps, seconds, color);
            zone.targetMode = TargetMode.Player;
            zone.targetMask = default;
            zone.CreateHazardVisuals(color);
        }

        private static DamageZone Create(Vector3 position, float zoneRadius, float dps, float seconds, Color color)
        {
            GameObject zoneObject = new GameObject("Damage Zone");
            zoneObject.transform.position = position;
            DamageZone zone = zoneObject.AddComponent<DamageZone>();
            zone.radius = Mathf.Max(0.1f, zoneRadius);
            zone.damagePerSecond = Mathf.Max(0f, dps);
            zone.duration = Mathf.Max(0.05f, seconds);
            zone.initialDuration = zone.duration;
            zone.tickInterval = 0.25f;
            zone.tickTimer = 0f;
            zone.visualColor = color;
            zone.spinSeed = Random.Range(-1f, 1f);
            return zone;
        }

        private void Update()
        {
            duration -= Time.deltaTime;
            tickTimer -= Time.deltaTime;
            if (tickTimer <= 0f)
            {
                tickTimer = tickInterval;
                TickDamage();
            }

            if (duration <= 0f)
            {
                Destroy(gameObject);
            }

            UpdateVisuals();
        }

        private void TickDamage()
        {
            float damage = damagePerSecond * tickInterval;
            if (targetMode == TargetMode.Player)
            {
                DamagePlayer(damage);
            }
            else
            {
                DamageEnemies(damage);
            }
        }

        private void DamageEnemies(float damage)
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, Hits, targetMask);
            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = Hits[i].GetComponent<EnemyHealth>();
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.TakeDamage(damage);
                    EnemyController controller = Hits[i].GetComponent<EnemyController>();
                    if (controller != null)
                    {
                        controller.MarkBurning(0.45f);
                        if (slowAmount > 0f)
                        {
                            controller.ApplySlow(slowAmount, slowDuration);
                        }
                    }
                }

                DestructibleCrate crate = Hits[i].GetComponent<DestructibleCrate>();
                if (crate != null)
                {
                    crate.TakeDamage(damage);
                }
            }
        }

        private void DamagePlayer(float damage)
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, Hits);
            for (int i = 0; i < count; i++)
            {
                PlayerHealth player = Hits[i].GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    return;
                }
            }
        }

        private void CreateRing(Color color)
        {
            ring = gameObject.AddComponent<LineRenderer>();
            ring.material = new Material(Shader.Find("Sprites/Default"));
            ring.loop = true;
            ring.useWorldSpace = false;
            ring.positionCount = 32;
            ring.startColor = color;
            ring.endColor = new Color(color.r, color.g, color.b, Mathf.Min(color.a, 0.25f));
            ring.startWidth = 0.04f;
            ring.endWidth = 0.04f;
            ring.sortingOrder = 11;
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = Mathf.PI * 2f * i / ring.positionCount;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
            }
        }

        private void CreateFireVisuals(Color color)
        {
            fillRenderer = CreateDisc("Burning Ground Fill", new Color(1f, 0.23f, 0.04f, 0.2f), radius * 1.78f, -0.02f, 5);
            emberRenderers = new SpriteRenderer[13];
            for (int i = 0; i < emberRenderers.Length; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float distance = Random.Range(radius * 0.12f, radius * 0.84f);
                SpriteRenderer ember = CreateDisc("Burning Ember", new Color(1f, Random.Range(0.36f, 0.76f), 0.06f, Random.Range(0.34f, 0.72f)), Random.Range(0.08f, 0.22f), 0.02f + i * 0.001f, 12);
                ember.transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;
                emberRenderers[i] = ember;
            }
        }

        private void CreateHazardVisuals(Color color)
        {
            fillRenderer = CreateDisc("Hazard Fill", new Color(color.r, color.g, color.b, Mathf.Min(0.24f, color.a)), radius * 1.72f, -0.02f, 5);
            emberRenderers = new SpriteRenderer[7];
            for (int i = 0; i < emberRenderers.Length; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float distance = Random.Range(radius * 0.08f, radius * 0.72f);
                SpriteRenderer mote = CreateDisc("Hazard Mote", new Color(color.r, color.g, color.b, Random.Range(0.2f, 0.48f)), Random.Range(0.1f, 0.24f), 0.02f + i * 0.001f, 11);
                mote.transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;
                emberRenderers[i] = mote;
            }
        }

        private SpriteRenderer CreateDisc(string objectName, Color color, float size, float zOffset, int sortingOrder)
        {
            GameObject visual = new GameObject(objectName);
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = new Vector3(0f, 0f, zOffset);
            visual.transform.localScale = Vector3.one * size;
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = GetDiscSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void UpdateVisuals()
        {
            if (initialDuration <= 0f)
            {
                return;
            }

            float life = Mathf.Clamp01(duration / initialDuration);
            float pulse = 0.82f + Mathf.Sin(Time.time * 10.5f + spinSeed) * 0.12f;
            if (fillRenderer != null)
            {
                fillRenderer.color = new Color(visualColor.r, Mathf.Max(visualColor.g, 0.2f + 0.1f * pulse), Mathf.Max(visualColor.b, 0.02f), Mathf.Min(0.24f, visualColor.a) * life);
                fillRenderer.transform.localScale = Vector3.one * radius * 1.78f * (0.92f + 0.1f * pulse);
            }

            if (ring != null)
            {
                ring.startColor = new Color(visualColor.r, visualColor.g, visualColor.b, Mathf.Min(0.78f, visualColor.a) * life);
                ring.endColor = new Color(1f, 0.86f, 0.18f, 0.22f * life);
                ring.transform.Rotate(0f, 0f, (18f + spinSeed * 8f) * Time.deltaTime);
            }

            if (emberRenderers == null)
            {
                return;
            }

            for (int i = 0; i < emberRenderers.Length; i++)
            {
                if (emberRenderers[i] == null)
                {
                    continue;
                }

                float emberPulse = 0.65f + 0.35f * Mathf.Sin(Time.time * (7f + i * 0.37f) + i);
                Color color = emberRenderers[i].color;
                color.a = Mathf.Lerp(0.18f, 0.78f, emberPulse) * life;
                emberRenderers[i].color = color;
                emberRenderers[i].transform.localScale = Vector3.one * Mathf.Lerp(0.08f, 0.24f, emberPulse);
                emberRenderers[i].transform.Rotate(0f, 0f, (70f + i * 8f) * Time.deltaTime);
            }
        }

        private static Sprite discSprite;

        private static Sprite GetDiscSprite()
        {
            if (discSprite != null)
            {
                return discSprite;
            }

            const int size = 24;
            const float center = (size - 1) * 0.5f;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01((1f - distance) * 1.45f);
                    texture.SetPixel(x, y, distance <= 1f ? new Color(1f, 1f, 1f, alpha) : clear);
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply(false);
            discSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            return discSprite;
        }
    }
}
