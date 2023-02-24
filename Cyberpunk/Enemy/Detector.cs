using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eDetectorState
{
    NONE = 0,       // 기본
    DOUBT = 1,      // 의심
    GUARD = 2,      // 경계
    DETECTION = 3,  // 발견
}

public class Detector : MonoBehaviour
{
    #region Vars

    private const float MinValue = 0f;
    private const float MaxValue = 100f;
    private bool IsDetectorEffect = false;

    [HideInInspector] public const float DetectorIncrease = 8f;
    [HideInInspector] public const float DetectorDecrease = 10f;

    public float GetDetectorIntensity => DetectorSlider.value;

    [Header("Detector Setting")]
    public eDetectorState CurrentDetectorState = eDetectorState.NONE;
    public LayerMask TargetLayer;
    public bool IsTrace = false;
    public bool IsDetector = false;
    public float DetectorRange = 20f;
    public float DetectorAngle = 140f;

    [Header("Detector UI")]
    public GameObject DetectorObject;
    public Slider DetectorSlider;
    public List<Color> StateColorList = new List<Color>();

    #endregion

    #region Init

    void Start()
    {
        Init();
    }

    private void LateUpdate()
    {
        ActionState();
    }

    #endregion

    #region Coroutine

    IEnumerator DetectorEffect()
    {
        if (!IsDetectorEffect && this.enabled)
        {
            IsDetectorEffect = true;
            SlowMotionManager.Instance.OnSlowMotion(0.2f, 0.15f);
            //CinemachineManager.Instance.SetColorAdjustments(new Color(0.8f, 0.8f, 0.8f), true);
            yield return new WaitForSeconds(0.5f);
            //CinemachineManager.Instance.SetColorAdjustments(Color.white, false);
        }
    }

    #endregion

    #region Func

    void Init()
    {
        DetectorSlider.minValue = MinValue;
        DetectorSlider.maxValue = MaxValue;
    }

    void CheckState()
    {
        if (GetDetectorIntensity <= StateValue.None)
        {
            CurrentDetectorState = eDetectorState.NONE;
        }
        else if (GetDetectorIntensity <= StateValue.Doubt && GetDetectorIntensity > StateValue.None)
        {
            CurrentDetectorState = eDetectorState.DOUBT;
        }
        else if (GetDetectorIntensity <= StateValue.Guard && GetDetectorIntensity > StateValue.Doubt)
        {
            CurrentDetectorState = eDetectorState.GUARD;
        }
        else
        {
            CurrentDetectorState = eDetectorState.DETECTION;
        }
    }

    void ActionState()
    {
        DetectorSlider.image.color = StateColorList[(int)CurrentDetectorState];

        switch (CurrentDetectorState)
        {
            case eDetectorState.NONE:
                DetectorRange = 20f;
                DetectorAngle = 140f;
                break;

            case eDetectorState.DOUBT:
                DetectorRange = 20f;
                DetectorAngle = 180f;
                break;

            case eDetectorState.GUARD:
                DetectorRange = 30f;
                DetectorAngle = 270f;
                break;

            case eDetectorState.DETECTION:
                DetectorRange = 30f;
                DetectorAngle = 360f;
                break;
        }
    }

    public void SetActive(bool isActive)
    {
        if (GetDetectorIntensity > StateValue.Guard) isActive = false;
        DetectorObject.SetActive(isActive);
    }

    public void SetPosition(Vector3 pos)
    {
        DetectorObject.transform.position = Camera.main.WorldToScreenPoint(pos);
    }

    public void SetDetector(Transform targetTransform, float intensity, float speed)
    {
        if (targetTransform != null)
        {
            float dist = Vector3.Distance(transform.position, targetTransform.position);
            if (dist <= 5f)
            {
                DetectorSlider.value += intensity * speed * 10f;
            }
            else
            {
                DetectorSlider.value += intensity * speed;
            }
        }
        else
        {
            DetectorSlider.value += intensity * speed;

            if (DetectorSlider.value <= 0f)
            {
                ResetDetector();
            }
        }

        CheckState();
    }

    public void Detection_Immediately()
    {
        CurrentDetectorState = eDetectorState.DETECTION;
        DetectorSlider.value = MaxValue;
        IsTrace = true;
        IsDetectorEffect = true;
    }

    public Vector3 CirclePoint(float angle)
    {
        angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    public bool IsTraceTarget(Transform targetTransform)
    {
        if (targetTransform == null) return false;

        IsTrace = false;
        Collider[] colls = Physics.OverlapSphere(transform.position, DetectorRange, TargetLayer);

        foreach (var coll in colls)
        {
            Vector3 direction = transform.position - targetTransform.position;

            if (Vector3.Angle(transform.forward, -direction.normalized) < DetectorAngle * 0.5f)
            {
                if (!IsTrace)
                    StartCoroutine(DetectorEffect());
                IsTrace = true;
            }
        }

        return IsTrace;
    }

    public bool IsDetectorTarget(Transform targetTransform)
    {
        if (targetTransform == null) return false;

        IsDetector = false;
        RaycastHit hit;
        Vector3 direction = transform.position - targetTransform.position;
        if (Physics.Raycast(transform.position + transform.TransformDirection(0f, 1f, 0f), -direction.normalized, out hit, DetectorRange, TargetLayer))
        {
            IsDetector = hit.collider.CompareTag("Player");
        }

        return IsDetector;
    }

    void ResetDetector()
    {
        IsTrace = false;
        IsDetector = false;
        IsDetectorEffect = false;
    }

    #endregion

    public struct StateValue
    {
        public const float None = 25f;
        public const float Doubt = 50f;
        public const float Guard = 85f;
        public const float Detection = 100f;
    }
}
