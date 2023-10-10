using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    public enum BulletType
    {
        player,
        lanternFire,
        enemy_circle,
        sandwich,
        star
    }

    [System.Serializable]
    public struct BulletAndIndex
    {
        [field: SerializeField] public BulletType Type { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }
    }

    public enum EnemyType
    {
        eye,
        skull,
        sniper,
        Boss = -1
    }

    [System.Serializable]
    public struct EnemyAndIndex
    {
        [field: SerializeField] public EnemyType Type { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }
    }

    [SerializeField] private GameObject prefab_bulletSpark;
    [SerializeField] private GameObject prefab_pickup;
    [SerializeField] private List<BulletAndIndex> bullets;
    private Dictionary<BulletType, GameObject> bulletsPrefabsDictionary;
    [SerializeField] private List<EnemyAndIndex> enemies;
    private Dictionary<EnemyType, GameObject> enemyPrefabsDictionary;

    [Space(10)]
    [SerializeField] private Transform bulletSparkParent;
    [SerializeField] private Transform pickupParent;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private Transform bulletParent;

    private Dictionary<Type, List<MonoBehaviour>> listDictionary = new Dictionary<Type, List<MonoBehaviour>>();
    private Dictionary<EnemyType, List<Enemy>> enemyTypeDictionary = new Dictionary<EnemyType, List<Enemy>>();
    private Dictionary<BulletType, List<Bullet>> bulletTypeDictionary = new Dictionary<BulletType, List<Bullet>>();
    private List<Enemy> aliveEnemies = new List<Enemy>();
    private List<Bullet> activeBullets = new List<Bullet>();

    private void Start()
    {
        bulletsPrefabsDictionary = new Dictionary<BulletType, GameObject>();
        foreach (BulletAndIndex b in bullets)
            bulletsPrefabsDictionary.Add(b.Type, b.Prefab);

        enemyPrefabsDictionary = new Dictionary<EnemyType, GameObject>();
        foreach (EnemyAndIndex b in enemies)
            enemyPrefabsDictionary.Add(b.Type, b.Prefab);
    }

    public T GetFromPool<T>() where T : MonoBehaviour
    {
        GameObject prefabToUse;
        Transform parentToUse;

        if (typeof(T) == typeof(BulletSpark))
        {
            prefabToUse = prefab_bulletSpark;
            parentToUse = bulletSparkParent;
        }
        else if (typeof(T) == typeof(Pickup))
        {
            prefabToUse = prefab_pickup;
            parentToUse = pickupParent;
        }
        else
        {
            Debug.LogError($"Wtf you doing bro, there is no pool of type \"{typeof(T)}\" to get from");
            return null;
        }

        if (!listDictionary.TryGetValue(typeof(T), out List<MonoBehaviour> listToUse))
        {
            listToUse = new List<MonoBehaviour>();
            listDictionary.Add(typeof(T), listToUse);
        }


        if (listToUse.Count > 0)
        {
            T objToReturn = (T)listToUse[listToUse.Count - 1];
            listToUse.RemoveAt(listToUse.Count - 1);
            return objToReturn;
        }

        return Instantiate(prefabToUse, parentToUse).GetComponent<T>();
    }

    /// <summary>
    /// Adds an object to the corresponding pool, and then disable it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_objToAdd"></param>
    public void AddToPool<T>(T _objToAdd) where T : MonoBehaviour
    {
        if (typeof(T) != typeof(BulletSpark) && typeof(T) != typeof(Pickup))
        {
            Debug.LogError($"Wtf you doing bro, there is no pool of type \"{typeof(T)}\" to add to");
            return;
        }

        if (!listDictionary.TryGetValue(typeof(T), out List<MonoBehaviour> listToUse))
        {
            listToUse = new List<MonoBehaviour>();
            listDictionary.Add(typeof(T), listToUse);
        }

        listToUse.Add(_objToAdd);
        _objToAdd.gameObject.SetActive(false);
    }

    private int enemyNum = 0;

    public Enemy GetEnemyOfType(EnemyType _index)
    {
        if (!enemyTypeDictionary.TryGetValue(_index, out List<Enemy> listToUse))
        {
            listToUse = new List<Enemy>();
            enemyTypeDictionary.Add(_index, listToUse);
        }

        if (listToUse.Count > 0)
        {
            Enemy objToReturn = listToUse[listToUse.Count - 1];
            listToUse.RemoveAt(listToUse.Count - 1);
            if (aliveEnemies.Contains(objToReturn))
                Debug.LogError("WHAT!!!!");
            else
                aliveEnemies.Add(objToReturn);
            return objToReturn;
        }

        Enemy newEnemy = Instantiate(enemyPrefabsDictionary[_index], enemyParent).GetComponent<Enemy>();
        newEnemy.gameObject.name = "Enemy" + enemyNum;
        enemyNum++;
        newEnemy.SetIndex(_index);

        if (aliveEnemies.Contains(newEnemy))
            Debug.LogError("WHAAAAAAAAAAAT!!!!");

        aliveEnemies.Add(newEnemy);

        return newEnemy;
    }

    /// <summary>
    /// Adds an object to the corresponding pool, and then disable it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_objToAdd"></param>
    public void AddToEnemyPool(Enemy _objToAdd, EnemyType _index)
    {
        if (!enemyTypeDictionary.TryGetValue(_index, out List<Enemy> listToUse))
        {
            listToUse = new List<Enemy>();
            enemyTypeDictionary.Add(_index, listToUse);
        }

        listToUse.Add(_objToAdd);
        if (!aliveEnemies.Remove(_objToAdd))
            Debug.LogError("HOW!!!!");
        _objToAdd.gameObject.SetActive(false);
    }

    public Bullet GetBulletOfType(BulletType _index)
    {
        if (!bulletTypeDictionary.TryGetValue(_index, out List<Bullet> listToUse))
        {
            listToUse = new List<Bullet>();
            bulletTypeDictionary.Add(_index, listToUse);
        }

        if (listToUse.Count > 0)
        {
            Bullet objToReturn = listToUse[listToUse.Count - 1];
            listToUse.RemoveAt(listToUse.Count - 1);
            if (activeBullets.Contains(objToReturn))
                Debug.LogError("bwat!!!!");
            else
                activeBullets.Add(objToReturn);
            return objToReturn;
        }

        Bullet bullet = Instantiate(bulletsPrefabsDictionary[_index], bulletParent).GetComponent<Bullet>();
        bullet.SetType(_index);

        if (activeBullets.Contains(bullet))
            Debug.LogError("ACTUALLY IMPOSSIBLE!!!!!!!");

        activeBullets.Add(bullet);

        return bullet;
    }

    /// <summary>
    /// Adds an object to the corresponding pool, and then disable it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_objToAdd"></param>
    public void AddToBulletPool(Bullet _objToAdd)
    {
        BulletType type = _objToAdd.BulletType;
        if (!bulletTypeDictionary.TryGetValue(type, out List<Bullet> listToUse))
        {
            listToUse = new List<Bullet>();
            bulletTypeDictionary.Add(type, listToUse);
        }

        listToUse.Add(_objToAdd);
        if (!activeBullets.Remove(_objToAdd))
            Debug.LogError("HOW_B!!!!");
        _objToAdd.gameObject.SetActive(false);
    }

    public Vector3 GetClosestEnemy(Vector3 bulletPos)
    {
        if (boss != null)
        {
            return boss.transform.position - bulletPos;
        }

        float smallestDist = 10000;
        Vector3 toEnemy = Vector3.zero;

        foreach (Enemy enemy in aliveEnemies)
        {
            Vector3 dist = enemy.transform.position - bulletPos;
            if (dist.sqrMagnitude < smallestDist)
            {
                smallestDist = dist.sqrMagnitude;
                toEnemy = dist;
            }
        }

        return toEnemy;
    }

    private Transform boss;
    public void SetBoss(Transform boss)
    {
        this.boss = boss;
    }

    public void DestroyAllBullets()
    {
        while (activeBullets.Count > 0)
        {
            AddToBulletPool(activeBullets[activeBullets.Count - 1]);
        }
    }
}
