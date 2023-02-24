using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpaceShip : Robot
{
    public enum eSpawnMethod
    {
        None = 0,
        Once = 1,  // 한꺼번에 스폰
        Order = 2, // 순차적으로 스폰
    }

    [Header("[SpaceShip - Spawn Setting]")]
    public eSpawnMethod SpawnMethod = eSpawnMethod.None;
    public SpawnData.eSpawnType SpawnType = SpawnData.eSpawnType.None;
    public List<Transform> SpawnTransforms = new List<Transform>();
    public float SpawnDuration = 1.0f;
    public int SpawnCount = 1;

    protected override void OnStart()
    {
        base.OnStart();


    }

    protected override IEnumerator SetRobotMoveType()
    {
        while (!IsDead)
        {
            yield return new WaitWhile(() => IsStop);
            switch (RobotMoveType)
            {
                case eCharacterMoveType.None:

                    break;

                case eCharacterMoveType.Strafe:

                    break;

                case eCharacterMoveType.Flying:
                    transform.DOMoveY(HeightOffsetY + transform.TransformDirection(0.0f, Mathf.Sin(Time.time * HeightMoveSpeed) * HeightIntensity, 0.0f).y, MoveSpeed);
                    break;

                case eCharacterMoveType.Swimming:

                    break;

                case eCharacterMoveType.Path:
                    Path.DOPlay();
                    if (!Path.enabled)
                    {
                        switch (SpawnMethod)
                        {
                            case eSpawnMethod.None:

                                break;

                            case eSpawnMethod.Once:
                                SpawnManager.Instance.Spawn(SpawnManager.Instance.GetSpawnPrefab(SpawnType, 0), transform.position, transform.rotation, SpawnCount, () =>
                                {

                                });
                                break;

                            case eSpawnMethod.Order:
                                StartCoroutine(SpawnManager.Instance.SpawnDelay(SpawnManager.Instance.GetSpawnPrefab(SpawnType, 0), transform.position, transform.rotation, SpawnCount, SpawnDuration, () =>
                                {
                                    // 스폰이 끝난뒤 처리할 로직                               
                                    //transform.DOPath(new Vector3[] { transform.forward + Vector3.up * 10.0f, transform.forward + Vector3.up * 50.0f }, 15.0f, PathType.CatmullRom, PathMode.Full3D).SetDelay(2.0f).OnComplete(() => Destroy(this.gameObject));
                                    RobotRig.isKinematic = false;
                                    float leaveTime = 5.0f;
                                    IEnumerator Leave()
                                    {
                                        yield return new WaitForSeconds(1.0f);

                                        while (leaveTime > 0.0f)
                                        {
                                            leaveTime -= Time.deltaTime;
                                            RobotRig.AddForce((transform.forward + Vector3.up) * 1000.0f, ForceMode.Impulse);
                                            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(RobotRig.velocity.normalized), Time.deltaTime);
                                            yield return new WaitForFixedUpdate();
                                        }
                                        RobotRig.Sleep();
                                        RobotRig.isKinematic = true;
                                        gameObject.SetActive(false);
                                    }
                                    StartCoroutine(Leave());
                                }));
                                break;
                        }
                        yield break;
                    }
                    break;
            }

            yield return new WaitForFixedUpdate();
        }
    }
}