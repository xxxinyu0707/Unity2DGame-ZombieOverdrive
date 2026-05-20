using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ZombieOverdrive.Audio;

namespace ZombieOverdrive.UI
{
    public class UIInteractiveEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float hoverScale = 1.04f;
        [SerializeField] private float clickScale = 0.96f;
        [SerializeField] private float animationSpeed = 12f;
        [SerializeField] private bool playSounds = true;
        [SerializeField] private bool animateOutline = true;
        [SerializeField] private Color hoverOutlineColor = new Color(0.14f, 0.7f, 1f, 0.85f);

        private Vector3 targetScale = Vector3.one;
        private Vector3 originalScale = Vector3.one;
        private bool isHovered = false;
        private bool isPressed = false;

        private Outline outline;
        private Color originalOutlineColor;
        private bool hasOutline = false;

        private void Awake()
        {
            originalScale = transform.localScale;
            targetScale = originalScale;

            outline = GetComponent<Outline>();
            if (outline != null)
            {
                hasOutline = true;
                originalOutlineColor = outline.effectColor;
            }
        }

        private void OnEnable()
        {
            transform.localScale = originalScale;
            targetScale = originalScale;
            isHovered = false;
            isPressed = false;
            if (hasOutline && outline != null)
            {
                outline.effectColor = originalOutlineColor;
            }
        }

        public static UIInteractiveEffect AddTo(Button button, Color hoverOutlineColor, Color normalOutlineColor)
        {
            if (button == null) return null;
            UIInteractiveEffect effect = button.GetComponent<UIInteractiveEffect>();
            if (effect == null)
            {
                effect = button.gameObject.AddComponent<UIInteractiveEffect>();
            }
            effect.ConfigureOutline(normalOutlineColor, hoverOutlineColor);
            return effect;
        }

        public void ConfigureOutline(Color normalColor, Color hoverColor)
        {
            outline = GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
                outline.effectDistance = new Vector2(1.5f, 1.5f);
            }
            hasOutline = true;
            originalOutlineColor = normalColor;
            hoverOutlineColor = hoverColor;
            outline.effectColor = normalColor;
        }

        private void Update()
        {
            // Use Time.unscaledDeltaTime so the animation plays even when the game is paused
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);

            if (animateOutline && hasOutline && outline != null)
            {
                Color targetColor = isPressed
                    ? Color.Lerp(hoverOutlineColor, Color.white, 0.5f)
                    : (isHovered ? hoverOutlineColor : originalOutlineColor);
                outline.effectColor = Color.Lerp(outline.effectColor, targetColor, Time.unscaledDeltaTime * animationSpeed);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateTargetScale();
            if (playSounds && GameAudio.Instance != null)
            {
                GameAudio.Play(GameSound.Menu, 0.28f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            UpdateTargetScale();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            UpdateTargetScale();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            UpdateTargetScale();
        }

        private void UpdateTargetScale()
        {
            if (isPressed)
            {
                targetScale = originalScale * clickScale;
            }
            else if (isHovered)
            {
                targetScale = originalScale * hoverScale;
            }
            else
            {
                targetScale = originalScale;
            }
        }
    }
}
