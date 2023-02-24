using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AISense_Detection))]
public class FovEditor : Editor
{
    private void OnSceneGUI()
    {
        AISense_Detection detection = (AISense_Detection)target;

        Vector3 FromAnglePos = detection.CirclePoint(-detection.DetectionAngle * 0.5f);

        Handles.color = new Color(1.0f, 1.0f, 1.0f, 0.2f);

        Handles.DrawWireDisc(detection.transform.position, Vector3.up, detection.DetectionRange);

        Handles.DrawSolidArc(detection.transform.position, Vector3.up, FromAnglePos, detection.DetectionAngle, detection.DetectionRange);

        Handles.Label(detection.transform.position + (detection.transform.forward * 2.0f), detection.DetectionAngle.ToString());
    }
}
