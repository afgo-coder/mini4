using System.Collections.Generic;
using UnityEngine;

namespace Mini4.Tower
{
    public static class ProjectilePoolManager
    {
        private sealed class Pool
        {
            public readonly GameObject Prefab;
            public readonly Queue<TowerProjectile> Inactive = new Queue<TowerProjectile>();
            public readonly Transform Root;

            public Pool(GameObject prefab, Transform root)
            {
                Prefab = prefab;
                Root = root;
            }
        }

        private static readonly Dictionary<int, Pool> Pools = new Dictionary<int, Pool>();
        private static Transform _poolRoot;

        public static TowerProjectile Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                return null;
            }

            Pool pool = GetOrCreatePool(prefab);
            TowerProjectile projectile = pool.Inactive.Count > 0 ? pool.Inactive.Dequeue() : CreateProjectile(pool);
            if (projectile == null)
            {
                return null;
            }

            Transform projectileTransform = projectile.transform;
            projectileTransform.SetPositionAndRotation(position, rotation);
            projectileTransform.SetParent(null);
            projectile.gameObject.SetActive(true);
            return projectile;
        }

        public static void Return(TowerProjectile projectile)
        {
            if (projectile == null)
            {
                return;
            }

            GameObject prefab = projectile.PoolPrefab;
            if (prefab == null)
            {
                Object.Destroy(projectile.gameObject);
                return;
            }

            Pool pool = GetOrCreatePool(prefab);
            projectile.ResetState();
            projectile.transform.SetParent(pool.Root, false);
            projectile.gameObject.SetActive(false);
            pool.Inactive.Enqueue(projectile);
        }

        private static Pool GetOrCreatePool(GameObject prefab)
        {
            int key = prefab.GetInstanceID();
            if (Pools.TryGetValue(key, out Pool pool))
            {
                return pool;
            }

            EnsureRoot();
            GameObject poolRootObject = new GameObject(prefab.name + "_Pool");
            poolRootObject.transform.SetParent(_poolRoot, false);
            pool = new Pool(prefab, poolRootObject.transform);
            Pools.Add(key, pool);
            return pool;
        }

        private static TowerProjectile CreateProjectile(Pool pool)
        {
            GameObject instance = Object.Instantiate(pool.Prefab, pool.Root);
            TowerProjectile projectile = instance.GetComponent<TowerProjectile>();
            if (projectile == null)
            {
                projectile = instance.AddComponent<TowerProjectile>();
            }

            projectile.SetPoolPrefab(pool.Prefab);
            instance.SetActive(false);
            return projectile;
        }

        private static void EnsureRoot()
        {
            if (_poolRoot != null)
            {
                return;
            }

            GameObject root = new GameObject("ProjectilePools");
            Object.DontDestroyOnLoad(root);
            _poolRoot = root.transform;
        }
    }
}
