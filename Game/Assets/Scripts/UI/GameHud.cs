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

        public void SetTimer(float elapsed, float duration)
        {
            float remaining = Mathf.Max(0f, duration - elapsed);
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        public void SetHealth(float current, float max)
        {
            healthText.text = $"生命 {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            healthSlider.value = max > 0f ? current / max : 0f;
        }

        public void SetExperience(int current, int required)
        {
            xpSlider.value = required > 0 ? (float)current / required : 0f;
        }

        public void SetLevel(int level)
        {
            levelText.text = $"等级 {level}";
        }

        public void SetKills(int kills)
        {
            killText.text = $"击杀 {kills}";
        }

        public void SetGold(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"金币 {gold}";
            }
        }

        public void SetMessage(string message)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
        }
    }
}
