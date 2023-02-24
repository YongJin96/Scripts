using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
{
    [Header("[Object Pool]")]
    public IObjectPool<Projectile> ProjectilePool;

    private void Awake()
    {
        InitObjectPool();
    }

    private void InitObjectPool()
    {
        ProjectilePool = new ObjectPool<Projectile>(CreateProjectile, OnGetProjectile, OnReleaseProjectile, OnDestroyProjectile, maxSize: 20);
    }

    #region Projectile

    public Projectile CreateProjectile()
    {
        Projectile projectile = Instantiate(Resources.Load<Projectile>("Projectile/Fireball"));
        projectile.SetProjectilePool(ProjectilePool);
        return projectile;
    }

    public void OnGetProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(true);
        projectile.DestroyProjectile(true, 5f);
    }

    public void OnReleaseProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
        projectile.ResetProjectile();
    }

    public void OnDestroyProjectile(Projectile projectile)
    {
        Destroy(projectile.gameObject);
    }

    public void OnClearProjectile()
    {
        ProjectilePool.Clear();
    }

    #endregion
}