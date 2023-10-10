using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ObjectPool;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private bool startOnSceneStart;
    [SerializeField] private float DEBUG_START_TIME = 0;

    [System.Serializable]
    public struct EnemySpawn_Struct
    {
        [field: SerializeField] public List<EnemyType> enemyTypeIndices { get; private set; }
        [field: SerializeField] public int numEnemies { get; private set; }
        [field: SerializeField] public float startSpawnTime { get; private set; }
        [field: SerializeField] public float spawnInterval { get; private set; }
        [field: SerializeField] public Vector3 offsetPos { get; private set; }
        [field: SerializeField] public List<int> pathIndices { get; private set; }
    }

    public struct EnemySpawn
    {
        public EnemySpawn(EnemyType enemyTypeIndex, float spawnTime, int pathIndex, Vector3 offsetPos)
        {
            this.enemyTypeIndex = enemyTypeIndex;
            this.spawnTime = spawnTime;
            this.pathIndex = pathIndex;
            this.offsetPos = offsetPos;
        }

        [field: SerializeField] public EnemyType enemyTypeIndex { get; private set; }
        [field: SerializeField] public float spawnTime { get; private set; }
        [field: SerializeField] public int pathIndex { get; private set; }
        [field: SerializeField] public Vector3 offsetPos { get; private set; }
    }

    [SerializeField] private Transform pathsParent;

    private Dictionary<int, Path> indexToPathDict = new Dictionary<int, Path>();
    [SerializeField] private List<GameObject> enemyIndices;

    [Space(10)]
    [SerializeField] private List<EnemySpawn_Struct> enemySpawnPattern_Serialized;
    private List<EnemySpawn> enemies;
    private EnemySpawn? nextEnemy;
    private float nextSpawnTime;
    private float startTime;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < pathsParent.childCount; i++)
        {
            indexToPathDict.Add(i, pathsParent.GetChild(i).GetComponent<Path>());
        }

        enemies = new List<EnemySpawn>();
        foreach (EnemySpawn_Struct enemy in enemySpawnPattern_Serialized)
        {
            for (int i = 0; i < enemy.numEnemies; i++)
            {
                if (DEBUG_START_TIME > 0)
                {
                    if (enemy.startSpawnTime + i * enemy.spawnInterval > DEBUG_START_TIME)
                        enemies.Add(new EnemySpawn(
                            enemy.enemyTypeIndices[i % enemy.enemyTypeIndices.Count],
                            enemy.startSpawnTime + i * enemy.spawnInterval - DEBUG_START_TIME,
                            enemy.pathIndices[i % enemy.pathIndices.Count],
                            i * enemy.offsetPos));
                }
                else
                {
                    enemies.Add(new EnemySpawn(
                        enemy.enemyTypeIndices[i % enemy.enemyTypeIndices.Count],
                        enemy.startSpawnTime + i * enemy.spawnInterval,
                        enemy.pathIndices[i % enemy.pathIndices.Count],
                        i * enemy.offsetPos));
                }
            }
        }
        enemies = enemies.OrderByDescending(x => x.spawnTime).ToList();

        if (startOnSceneStart)
            StartLevel();
    }

    public void StartLevel()
    {
        startTime = Time.time;
        GetNextEnemy();
    }

    private void GetNextEnemy()
    {
        if (enemies.Count > 0)
        {
            nextEnemy = enemies[enemies.Count - 1];
            nextSpawnTime = startTime + ((EnemySpawn)nextEnemy).spawnTime;
            enemies.RemoveAt(enemies.Count - 1);
        }
        else
            nextEnemy = null;
    }

    // Update is called once per frame
    void Update()
    {
        while (nextEnemy != null && Time.time >= nextSpawnTime)
        {
            EnemySpawn enemyToSpawn = (EnemySpawn)nextEnemy;
            EnemyType index = enemyToSpawn.enemyTypeIndex;

            if (index == EnemyType.Boss)
            {
                Boss.Instance.GetComponent<MoveToPos>().StartMoving();
            }
            else
            {
                //Spawn that enemy
                Enemy enemy = ObjectPool.Instance.GetEnemyOfType(index);
                enemy.SetPath(indexToPathDict[enemyToSpawn.pathIndex], enemyToSpawn.offsetPos);

                enemy.gameObject.SetActive(true);
            }

            GetNextEnemy();
        }
    }

    public void OnStageFinished()
    {
        UIController.Instance.OnStageCleared();
    }
}
