using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPlayerState : MonoBehaviour
{
    public PlayerMovement Player { get => FindObjectOfType<PlayerMovement>(); }

    [Header("[Player UI]")]
    [SerializeField] private Image ScreenImage;

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