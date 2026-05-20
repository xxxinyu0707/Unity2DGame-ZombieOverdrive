using UnityEngine;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.World;

namespace ZombieOverdrive.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class SingularityOrb : MonoBehaviour
    {
        [SerializeField] private float speed = 4.5f;
        [SerializeField] private float tickInterval = 0.35f;

        private static readonly Collider2D[] Hits = new Collider2D[64];
        private Vector2 direction;
        private float damage;
        private float radius;
        private float pullStrength;
        private float timer;
        private float tickTimer;
        private LayerMask enemyMask;
        private LineRenderer outerSwirl;
        private LineRenderer innerSwirl;
        private SpriteRenderer coreRenderer;
        private SpriteRenderer glowRenderer;
        private SpriteRenderer[] moteRenderers;
        private float[] moteAngles;
        private float[] moteDistances;
        private float[] moteSpeeds;
        private bool evolved;
        private float maxHealthPercentDamage;
        private bool absorbProjectiles;
        private bool collapseOnEnd;
        private bool ending;
        private float visualSeed;

        public void Launch(Vector2 launchDirection, float orbDamage, float pullRadius, float pullForce, float lifetime, LayerMask mask)
        {
            Launch(launchDirection, orbDamage, pullRadius, pullForce, lifetime, mask, false);
        }

        public void Launch(Vector2 launchDirection, float orbDamage, float pullRadius, float pullForce, float lifetime, LayerMask mask, bool evolvedOrb)
        {
            Launch(launchDirection, orbDamage, pullRadius, pullForce, lifetime, mask, evolvedOrb, 0f, false, false);
        }

        public void Launch(Vector2 launchDirection, float orbDamage, float pullRadius, float pullForce, float lifetime, LayerMask mask, bool evolvedOrb, float percentDamage, bool absorbBullets, bool collapse)
        {
            direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : Vector2.right;
            damage = orbDamage;
            radius = pullRadius;
            pullStrength = pullForce;
            timer = lifetime;
            tickTimer = 0f;
            enemyMask = mask;
            evolved = evolvedOrb;
            maxHealthPercentDamage = Mathf.Max(0f, percentDamage);
            absorbProjectiles = absorbBullets;
            collapseOnEnd = collapse;
            ending = false;
            visualSeed = Random.Range(0f, 100f);
            gameObject.SetActive(true);
            EnsureVisuals();
            UpdateVisualShape();
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            transform.Rotate(0f, 0f, (evolved ? 220f : 130f) * Time.deltaTime);
            timer -= Time.deltaTime;
            tickTimer -= Time.deltaTime;
            UpdateVisuals();

            if (tickTimer <= 0f)
            {
                tickTimer = evolved ? tickInterval * 0.72f : tickInterval;
                PullAndDamage();
            }

            if (timer <= 0f)
            {
                EndOrb();
            }
        }

        private void EnsureVisuals()
        {
            if (outerSwirl != null)
            {
                return;
            }

            coreRenderer = CreateDisc("Singularity Core", new Color(0.015f, 0f, 0.035f, 0.96f), 0.72f, 13);
            glowRenderer = CreateDisc("Accretion Glow", new Color(0.62f, 0.18f, 1f, 0.34f), 1.35f, 7);
            outerSwirl = CreateSwirl("Outer Accretion Disk", 84, 0.05f, 8);
            innerSwirl = CreateSwirl("Inner Accretion Disk", 58, 0.035f, 12);
            moteRenderers = new SpriteRenderer[18];
            moteAngles = new float[moteRenderers.Length];
            moteDistances = new float[moteRenderers.Length];
            moteSpeeds = new float[moteRenderers.Length];
            for (int i = 0; i < moteRenderers.Length; i++)
            {
                moteRenderers[i] = CreateDisc("Falling Star", new Color(0.78f, 0.42f, 1f, Random.Range(0.28f, 0.7f)), Random.Range(0.055f, 0.13f), 11);
                moteAngles[i] = Random.Range(0f, Mathf.PI * 2f);
                moteDistances[i] = Random.Range(0.25f, 0.95f);
                moteSpeeds[i] = Random.Range(1.4f, 3.4f);
            }
        }

        private void UpdateVisualShape()
        {
            if (outerSwirl == null)
            {
                return;
            }

            SetSwirl(outerSwirl, radius, evolved ? 0.24f : 0.14f, evolved ? new Color(0.86f, 0.35f, 1f, 0.5f) : new Color(0.55f, 0.28f, 1f, 0.34f));
            SetSwirl(innerSwirl, radius * 0.52f, evolved ? 0.42f : 0.26f, evolved ? new Color(1f, 0.62f, 1f, 0.62f) : new Color(0.75f, 0.5f, 1f, 0.44f));
        }

        private void UpdateVisuals()
        {
            float pulse = 0.88f + Mathf.Sin(Time.time * (evolved ? 9f : 6.5f) + visualSeed) * 0.1f;
            if (coreRenderer != null)
            {
                coreRenderer.transform.localScale = Vector3.one * radius * (evolved ? 0.26f : 0.2f) * pulse;
            }

            if (glowRenderer != null)
            {
                glowRenderer.transform.localScale = new Vector3(radius * (evolved ? 1.15f : 0.88f) * (1.03f - pulse * 0.08f), radius * (evolved ? 0.72f : 0.56f) * pulse, 1f);
                glowRenderer.transform.Rotate(0f, 0f, (evolved ? -180f : -112f) * Time.deltaTime);
                glowRenderer.color = evolved ? new Color(0.78f, 0.16f, 1f, 0.38f) : new Color(0.48f, 0.18f, 1f, 0.28f);
            }

            if (outerSwirl != null)
            {
                outerSwirl.transform.Rotate(0f, 0f, (evolved ? 190f : 120f) * Time.deltaTime);
            }

            if (innerSwirl != null)
            {
                innerSwirl.transform.Rotate(0f, 0f, (evolved ? -280f : -180f) * Time.deltaTime);
            }

            if (moteRenderers == null)
            {
                return;
            }

            for (int i = 0; i < moteRenderers.Length; i++)
            {
                if (moteRenderers[i] == null)
                {
                    continue;
                }

                moteAngles[i] += Time.deltaTime * moteSpeeds[i] * (evolved ? 1.45f : 1f);
                moteDistances[i] = Mathf.MoveTowards(moteDistances[i], 0.18f, Time.deltaTime * (evolved ? 0.18f : 0.1f));
                if (moteDistances[i] <= 0.2f)
                {
                    moteDistances[i] = Random.Range(0.78f, 1.05f);
                    moteAngles[i] = Random.Range(0f, Mathf.PI * 2f);
                }

                float spiralRadius = radius * moteDistances[i];
                Vector3 tangent = new Vector3(Mathf.Cos(moteAngles[i]), Mathf.Sin(moteAngles[i]), 0f);
                moteRenderers[i].transform.localPosition = tangent * spiralRadius;
                moteRenderers[i].transform.localScale = Vector3.one * Mathf.Lerp(0.05f, evolved ? 0.18f : 0.13f, moteDistances[i]);
                moteRenderers[i].color = evolved ? new Color(0.92f, 0.55f, 1f, 0.62f) : new Color(0.68f, 0.38f, 1f, 0.46f);
            }
        }

        private SpriteRenderer CreateDisc(string name, Color color, float scale, int sortingOrder)
        {
            GameObject visual = new GameObject(name);
            visual.transform.SetParent(transform, false);
            visual.transform.localScale = Vector3.one * scale;
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateDiscSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private LineRenderer CreateSwirl(string name, int pointCount, float width, int sortingOrder)
        {
            GameObject lineObject = new GameObject(name);
            lineObject.transform.SetParent(transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.useWorldSpace = false;
            line.loop = false;
            line.positionCount = pointCount;
            line.startWidth = width;
            line.endWidth = width * 0.35f;
            line.sortingOrder = sortingOrder;
            return line;
        }

        private void SetSwirl(LineRenderer line, float outerRadius, float spiralTightness, Color color)
        {
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, color.a * 0.08f);
            for (int i = 0; i < line.positionCount; i++)
            {
                float t = (float)i / Mathf.Max(1, line.positionCount - 1);
                float angle = t * Mathf.PI * 2f * (evolved ? 3.4f : 2.5f);
                float r = Mathf.Lerp(outerRadius, outerRadius * spiralTightness, t);
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * r);
            }
        }

        private void PullAndDamage()
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, Hits, enemyMask);
            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = Hits[i].GetComponent<EnemyHealth>();
                DestructibleCrate crate = Hits[i].GetComponent<DestructibleCrate>();
                if (crate != null)
                {
                    crate.TakeDamage(damage);
                }

                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                float percentDamage = maxHealthPercentDamage > 0f ? enemy.MaxHealth * maxHealthPercentDamage : 0f;
                enemy.TakeDamage(damage + percentDamage);
                EnemyController controller = Hits[i].GetComponent<EnemyController>();
                if (controller != null)
                {
                    Vector2 pull = ((Vector2)transform.position - (Vector2)Hits[i].transform.position).normalized * pullStrength;
                    controller.ApplyKnockback(pull);
                    if (evolved)
                    {
                        controller.ApplySlow(0.35f, 0.45f);
                    }
                }
            }

            if (absorbProjectiles)
            {
                AbsorbProjectiles();
            }

            if (evolved)
            {
                CombatVisuals.SpawnSoftBurst(transform.position, new Color(0.8f, 0.32f, 1f, 0.34f), radius * 0.22f, 0.08f, 5, false);
            }
        }

        private void AbsorbProjectiles()
        {
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, radius);
            for (int i = 0; i < nearby.Length; i++)
            {
                AcidProjectile acid = nearby[i].GetComponent<AcidProjectile>();
                if (acid != null)
                {
                    Destroy(acid.gameObject);
                    CombatVisuals.SpawnRing(acid.transform.position, new Color(0.55f, 1f, 0.35f, 0.35f), 0.22f, 0.08f);
                }
            }
        }

        private void EndOrb()
        {
            if (ending)
            {
                return;
            }

            ending = true;
            if (collapseOnEnd)
            {
                CombatVisuals.SpawnSingularityCollapse(transform.position, radius, evolved);
                int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius * 1.05f, Hits, enemyMask);
                for (int i = 0; i < count; i++)
                {
                    EnemyHealth enemy = Hits[i].GetComponent<EnemyHealth>();
                    if (enemy != null && enemy.IsAlive)
                    {
                        enemy.TakeDamage(damage * 3f + enemy.MaxHealth * 0.025f);
                    }
                }
            }

            Destroy(gameObject);
        }

        private static Sprite discSprite;

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
                    float alpha = Mathf.Clamp01((1f - distance) * 1.65f);
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
