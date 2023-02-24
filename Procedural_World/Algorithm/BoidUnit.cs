using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidUnit : MonoBehaviour
{
    [Header("Info")]
    private Boids MyBoids;
    private List<BoidUnit> Neighbours = new List<BoidUnit>();

    private Vector3 TargetVector;
    private Vector3 EgoVector;
    private float Speed;
    private float AdditionalSpeed = 0f;

    [Header("Neighbour")]
    [SerializeField] float ObstacleDistance;
    [SerializeField] float FovAngle = 120f;
    [SerializeField] int MaxNeighbourCount = 50;
    [SerializeField] float NeighbourDistance = 10f;
    [SerializeField] float TargetSpeed = 10f;

    [Header("ETC")]
    [SerializeField] LayerMask BoidUnitLayer;
    [SerializeField] LayerMask ObstacleLayer;

    Coroutine FindNeighbour;
    Coroutine CalculateEgoVector;

    void FixedUpdate()
    {
        if (MyBoids.Targeting.IsTargeting)
            TargetMove();
        else
            CalculateMove();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Blade"))
        {
            Hit();
            Vector3 colliderPoint = other.ClosestPoint(transform.position);
            Vector3 colliderNormal = transform.position - colliderPoint;
            Instantiate(Resources.Load<GameObject>("Effect/Spark Effect"), colliderPoint, Quaternion.LookRotation(-colliderNormal.normalized));
            Instantiate(Resources.Load<GameObject>("Effect/Distortion Effect"), colliderPoint, Quaternion.identity);
            CinemachineManager.Instance.Shake(5f, 0.3f);
            SlowMotionManager.Instance.OnSlowMotion(0.1f, 0.018f);
        }
    }

    public void InitializeUnit(Boids boids, float speed, int myIndex)
    {
        MyBoids = boids;
        Speed = speed;

        FindNeighbour = StartCoroutine("FindNeighbourCoroutine");
        CalculateEgoVector = StartCoroutine("CalculateEgoVectorCoroutine");
    }

    IEnumerator CalculateEgoVectorCoroutine()
    {
        Speed = Random.Range(MyBoids.SpeedRange.x, MyBoids.SpeedRange.y);
        EgoVector = Random.insideUnitSphere;
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        CalculateEgoVector = StartCoroutine("CalculateEgoVectorCoroutine");
    }

    IEnumerator FindNeighbourCoroutine()
    {
        Neighbours.Clear();

        Collider[] colls = Physics.OverlapSphere(transform.position, NeighbourDistance, BoidUnitLayer);
        for (int i = 0; i < colls.Length; i++)
        {
            if (Vector3.Angle(transform.forward, colls[i].transform.position - transform.position) <= FovAngle)
            {
                if (colls[i].GetComponent<BoidUnit>() != null)
                    Neighbours.Add(colls[i].GetComponent<BoidUnit>());
            }
            if (i > MaxNeighbourCount)
            {
                break;
            }
        }
        yield return new WaitForSeconds(Random.Range(0.5f, 2f));
        FindNeighbour = StartCoroutine("FindNeighbourCoroutine");
    }

    IEnumerator HitCoroutine()
    {
        if (this.enabled)
        {
            this.enabled = false;
            this.gameObject.AddComponent<Rigidbody>();
            yield return new WaitForSeconds(5f);
            this.enabled = true;
            Destroy(this.gameObject.GetComponent<Rigidbody>());
        }
    }

    private Vector3 CalculateCohesionVector()
    {
        Vector3 cohesionVec = Vector3.zero;
        if (Neighbours.Count > 0)
        {
            // 이웃 Unit들의 위치 더하기
            for (int i = 0; i < Neighbours.Count; i++)
            {
                cohesionVec += Neighbours[i].transform.position;
            }
        }
        else
        {
            // 이웃이 없으면 Vector3.zero 반환
            return cohesionVec;
        }

        // 중심 위치로의 벡터 찾기
        cohesionVec /= Neighbours.Count;
        cohesionVec -= transform.position;
        cohesionVec.Normalize();
        return cohesionVec;
    }

    private Vector3 CalculateAlignmentVector()
    {
        Vector3 alignmentVec = transform.forward;
        if (Neighbours.Count > 0)
        {
            // 이웃들이 향하는 방향의 평균 방향으로 이동
            for (int i = 0; i < Neighbours.Count; i++)
            {
                alignmentVec += Neighbours[i].transform.forward;
            }
        }
        else
        {
            // 이들이 없으면 그냥 forward로 이동
            return alignmentVec;
        }

        alignmentVec /= Neighbours.Count;
        alignmentVec.Normalize();
        return alignmentVec;
    }

    private Vector3 CalculateSeparationVector()
    {
        Vector3 separationVec = Vector3.zero;
        if (Neighbours.Count > 0)
        {
            // 이웃들을 피하는 방향으로 이동
            for (int i = 0; i < Neighbours.Count; i++)
            {
                separationVec += (transform.position - Neighbours[i].transform.position);
            }
        }
        else
        {
            // 이웃이 없으면 Vector3.zero 반환
            return separationVec;
        }
        separationVec /= Neighbours.Count;
        separationVec.Normalize();
        return separationVec;
    }

    private Vector3 CalculateBoundsVector()
    {
        Vector3 offsetToCenter = MyBoids.transform.position - transform.position;
        return offsetToCenter.magnitude >= MyBoids.SpawnRange ? offsetToCenter.normalized : Vector3.zero;
    }

    private Vector3 CalculateObstacleVector()
    {
        Vector3 obstacleVec = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, ObstacleDistance, ObstacleLayer))
        {
            Debug.DrawLine(transform.position, hit.point, Color.black);
            obstacleVec = hit.normal;
            AdditionalSpeed = 10;
        }
        return obstacleVec;
    }

    private void CalculateMove()
    {
        if (AdditionalSpeed > 0f)
            AdditionalSpeed -= Time.deltaTime;

        // Calculate all the vectors we need
        Vector3 cohesionVec = CalculateCohesionVector() * MyBoids.CohesionWeight;
        Vector3 alignmentVec = CalculateAlignmentVector() * MyBoids.AlignmentWeight;
        Vector3 separationVec = CalculateSeparationVector() * MyBoids.SeparationWeight;
        // 추가적인 방향
        Vector3 boundsVec = CalculateBoundsVector() * MyBoids.BoundsWeight;
        Vector3 obstacleVec = CalculateObstacleVector() * MyBoids.ObstacleWeight;
        Vector3 egoVec = EgoVector * MyBoids.EgoWeight;

        TargetVector = cohesionVec + alignmentVec + separationVec + boundsVec + obstacleVec + egoVec;

        // Steer and Move
        TargetVector = Vector3.Slerp(this.transform.forward, TargetVector, Time.deltaTime);
        TargetVector = TargetVector.normalized;
        if (TargetVector == Vector3.zero)
            TargetVector = EgoVector;

        this.transform.position += TargetVector * (Speed + AdditionalSpeed) * Time.deltaTime;
        this.transform.rotation = Quaternion.LookRotation(TargetVector);
    }

    private void TargetMove()
    {
        if (MyBoids.Targeting.TargetTransform == null) return;

        TargetVector = transform.position - MyBoids.Targeting.TargetTransform.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Chest).position;
        TargetVector = -TargetVector.normalized;
        TargetVector = Vector3.Slerp(this.transform.forward, TargetVector, Time.deltaTime * TargetSpeed);

        this.transform.position += TargetVector * (Speed + AdditionalSpeed) * Time.deltaTime;
        this.transform.rotation = Quaternion.LookRotation(TargetVector);

        if (Vector3.Distance(transform.position, MyBoids.Targeting.TargetTransform.position) <= 0.5f)
        {
            MyBoids.transform.localScale += Vector3.one * 0.01f;
        }
    }

    public void Hit()
    {
        StartCoroutine(HitCoroutine());
    }
}
