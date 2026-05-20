using UnityEngine;
using UnityEngine.UI;

namespace ZombieOverdrive.UI
{
    public class GameHud : MonoBehaviour
    {
        [SerializeField] private Text timerText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text killText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text messageText;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider xpSlider;

        private float targetHealth = 1f;
        private float targetXp = 0f;

        private void Start()
        {
            if (healthSlider != null)
            {
                targetHealth = healthSlider.value;
            }
            if (xpSlider != null)
            {
                targetXp = xpSlider.value;
            }
        }

        private void Update()
        {
            // Smoothly slide the health and experience bars
            if (healthSlider != null)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth, Time.deltaTime * 10f);
            }
            if (xpSlider != null)
            {
                xpSlider.value = Mathf.Lerp(xpSlider.value, targetXp, Time.deltaTime * 8f);
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetTimer(float elapsed, float duration)
        {
            float remaining = Mathf.Max(0f, duration - elapsed);
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            timerText.text = $"<color=#eef4ff><b>{minutes:00}:{seconds:00}</b></color>";
        }

        public void SetHealth(float current, float max)
        {
            healthText.text = $"HP: <color=#ef5350><b>{Mathf.CeilToInt(current)}</b></color> / {Mathf.CeilToInt(max)}";
            targetHealth = max > 0f ? current / max : 0f;
        }

        public void SetExperience(int current, int required)
        {
            targetXp = required > 0 ? (float)current / required : 0f;
        }

        public void SetLevel(int level)
        {
            levelText.text = $"等级: <color=#29b6f6><b>{level}</b></color>";
        }

        public void SetKills(int kills)
        {
            killText.text = $"击杀: <color=#ff7043><b>{kills}</b></color>";
        }

        public void SetGold(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"金币: <color=#ffd54f><b>{gold}</b></color>";
            }
        }

        public void SetMessage(string message)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
        }
    }
}
