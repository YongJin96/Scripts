using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPlayerState : MonoBehaviour
{
    public PlayerMovement Player { get => FindObjectOfType<PlayerMovement>(); }

    [Header("[Player UI]")]
    [SerializeField] private Image HpBar;
    [SerializeField] private Image StaminaBar;
    [SerializeField] private Image ScreenImage;

    [Header("[Player Stat]")]
    public bool IsInfinity = false;
    private float IncreaseAmount = 0.5f;
    private float StaminaTime;

    private void Start()
    {
        StartCoroutine(ReloadStamina());
    }

    private void Update()
    {
        UIUpdate();
    }

    private void UIUpdate()
    {
        HpBar.fillAmount = Player.CharacterStatData.CurrentHealth / Player.CharacterStatData.MaxHealth;
        StaminaBar.fillAmount = Player.CharacterStatData.CurrentStamina / Player.CharacterStatData.MaxStamina;
    }

    //회피 시 스태미너 감소
    public void ChangeStamina(float amount)
    {
        if (IsInfinity) return;

        StaminaTime = Time.time + 1.5f;
        Player.CharacterStatData.CurrentStamina -= amount;
        if (Player.CharacterStatData.CurrentStamina < 0)
        {
            Player.CharacterStatData.CurrentStamina = 0;
        }
    }

    public bool IsEmptyStamina()
    {
        return Player.CharacterStatData.CurrentStamina <= 0;
    }

    private IEnumerator ReloadStamina()
    {
        while (true)
        {
            if (StaminaTime < Time.time)
            {
                Player.CharacterStatData.CurrentStamina += IncreaseAmount;
            }
            if (Player.CharacterStatData.CurrentStamina >= Player.CharacterStatData.MaxStamina)
            {
                Player.CharacterStatData.CurrentStamina = Player.CharacterStatData.MaxStamina;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public IEnumerator HitEffect(float fadeAlpha, float fadeSpeed)
    {
        ScreenImage.color = Util.ParseHexToColor("800404");
        ScreenImage.DOFade(fadeAlpha, fadeSpeed);
        yield return new WaitForSeconds(fadeSpeed);
        ScreenImage.DOFade(0.0f, fadeSpeed);
    }

    public IEnumerator ScreenEffect(string hex, float fadeAlpha, float fadeSpeed)
    {
        ScreenImage.color = Util.ParseHexToColor(hex);
        ScreenImage.DOFade(fadeAlpha, fadeSpeed);
        yield return new WaitForSeconds(fadeSpeed);
        ScreenImage.DOFade(0.0f, fadeSpeed);
    }

    public IEnumerator ScreenEffect(Color color, float fadeAlpha, float fadeSpeed)
    {
        ScreenImage.color = color;
        ScreenImage.DOFade(fadeAlpha, fadeSpeed);
        yield return new WaitForSeconds(fadeSpeed);
        ScreenImage.DOFade(0.0f, fadeSpeed);
    }

    public IEnumerator ScreenEffect_Crouch(Color color, float fadeAlpha, float fadeSpeed)
    {
        ScreenImage.color = color;
        ScreenImage.DOFade(fadeAlpha, fadeSpeed);
        yield return new WaitWhile(() => Player.IsCrouch && !Player.IsSprint);
        ScreenImage.DOFade(0.0f, fadeSpeed);
    }

    public void OnScreenEffect(string hex, float fadeAlpha, float fadeSpeed)
    {
        ScreenImage.color = Util.ParseHexToColor(hex);
        ScreenImage.DOFade(fadeAlpha, fadeSpeed);
    }

    public void OnScreenEffect(Color color, float fadeAlpha, float fadeSpeed)
    {
        ScreenImage.color = color;
        ScreenImage.DOFade(fadeAlpha, fadeSpeed);
    }

    public void OffScreenEffect(float fadeSpeed)
    {
        ScreenImage.DOFade(0.0f, fadeSpeed);
    }
}