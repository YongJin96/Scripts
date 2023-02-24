using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicManager : MonoBehaviour
{
    [Header("[Graphic Options]")]
    public int ResolutionWidth = 1920;                             // 해상도 너비
    public int ResolutionHeight = 1080;                            // 해상도 높이
    public int FullScreenMode = 0;                                 // 화면 모드          (0 : 전체 화면, 1 : 전체 창모드, 2 : 창모드)
    public int FrameRate = 144;                                    // 주사율
    public int TextureQuality = 0;                                 // 텍스처 퀄리티       (0 ~ 2, 낮을수록 고해상도 사용)
    public int ShadowQuality = 3;                                  // 그림자 퀄리티       (0 ~ 3, 높을수록 고해상도 사용)
    public int AntiAliasing = 8;                                   // 안티 앨리어싱       (0 : 사용안함, 2 : x2, 4 : x4, 8 : x8)
    public int IsVSync = 0;                                        // 수직 동기화         (0 : 끄기, 1 : 켜기)
    public int IsAnisotropicFiltering = 0;                         // 비등방성 필터링     (0 : 끄기, 1 : 켜기)

    void OnEnable()
    {
        Screen.SetResolution(ResolutionWidth, ResolutionHeight, (FullScreenMode)FullScreenMode, FrameRate);
        Application.targetFrameRate = FrameRate;
        Debug.LogError(Screen.currentResolution);
    }
}
