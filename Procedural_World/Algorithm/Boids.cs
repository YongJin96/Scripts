using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Boids : MonoBehaviour
{
    [HideInInspector] public Targeting Targeting;

    [Header("[Boid Options]")]
    [SerializeField] private BoidUnit BoidUnitPrefabs;
    [Range(1, 100)] public int BoidCount;
    [Range(0, 100)] public float SpawnRange = 30f;
    public Vector2 SpeedRange;
    public Vector2 ScaleRange;

    [Range(0, 1000)] public float CohesionWeight = 1f;
    [Range(0, 1000)] public float AlignmentWeight = 1f;
    [Range(0, 1000)] public float SeparationWeight = 1f;

    [Range(0, 1000)] public float BoundsWeight = 1f;
    [Range(0, 1000)] public float ObstacleWeight = 10f;
    [Range(0, 1000)] public float EgoWeight = 1f;

    void Start()
    {
        Init();

        for (int i = 0; i < BoidCount; i++)
        {
            Vector3 randomVec = Random.insideUnitSphere * SpawnRange;
            Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360f), 0f);
            BoidUnit currentUnit = Instantiate(BoidUnitPrefabs, this.transform.position + randomVec, randomRot);
            currentUnit.transform.localScale = Vector3.one * Random.Range(ScaleRange.x, ScaleRange.y);
            currentUnit.transform.SetParent(this.transform);
            currentUnit.InitializeUnit(this, Random.Range(SpeedRange.x, SpeedRange.y), i);
        }
    }

    private void FixedUpdate()
    {
        Targeting.NearestTarget(FindObjectOfType<PlayerMovement>());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            Hit();
        }
    }

    void Init()
    {
        Targeting = GetComponent<Targeting>();
    }

    void Hit()
    {
        this.transform.DOShakePosition(0.5f);
        this.transform.localScale -= Vector3.one * 0.4f;
        if (this.transform.localScale.x <= 0.5f) Destroy(this.gameObject);
    }
}
