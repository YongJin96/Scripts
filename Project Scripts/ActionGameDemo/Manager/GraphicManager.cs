using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicManager : MonoBehaviour
{
    [Header("[Graphic Options]")]
    public int ResolutionWidth = 1920;                             // �ػ� �ʺ�
    public int ResolutionHeight = 1080;                            // �ػ� ����
    public int FullScreenMode = 0;                                 // ȭ�� ���          (0 : ��ü ȭ��, 1 : ��ü â���, 2 : â���)
    public int FrameRate = 144;                                    // �ֻ���
    public int TextureQuality = 0;                                 // �ؽ�ó ����Ƽ       (0 ~ 2, �������� ���ػ� ���)
    public int ShadowQuality = 3;                                  // �׸��� ����Ƽ       (0 ~ 3, �������� ���ػ� ���)
    public int AntiAliasing = 8;                                   // ��Ƽ �ٸ����       (0 : ������, 2 : x2, 4 : x4, 8 : x8)
    public int IsVSync = 0;                                        // ���� ����ȭ         (0 : ����, 1 : �ѱ�)
    public int IsAnisotropicFiltering = 0;                         // ���漺 ���͸�     (0 : ����, 1 : �ѱ�)

    void OnEnable()
    {
        Screen.SetResolution(ResolutionWidth, ResolutionHeight, (FullScreenMode)FullScreenMode, FrameRate);
        Application.targetFrameRate = FrameRate;
        Debug.LogError(Screen.currentResolution);
    }
}
