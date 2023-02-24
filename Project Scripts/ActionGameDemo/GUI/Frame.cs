using UnityEngine;

public class Frame : MonoBehaviour
{
    float DeltaTime = 0.0f;

    void Update()
    {
        DeltaTime += (Time.deltaTime - DeltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int screen_width = Screen.width;
        int screen_height = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, screen_width, screen_height * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = screen_height * 2 / 100;
        style.normal.textColor = Color.green;
        float msec = DeltaTime * 1000.0f;
        float fps = 1.0f / DeltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}
