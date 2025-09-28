using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs врагов")]
    public GameObject[] EnemyPrefabs;

    [Header("Точки спавна (empty GameObjects)")]
    public Transform[] SpawnPoints;

    [Header("Параметры спавна")]
    public int EnemiesPerWave = 3;
    public float TimeBetweenSpawns = 1f;
    public float TimeBetweenWaves = 5f;
    public int MaxWaves = 3;

    int currentWave = 0;
    EnemyManager enemyManager;

    void Start()
    {
        enemyManager = FindAnyObjectByType<EnemyManager>();
        if (enemyManager == null)
        {
            return;
        }

        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        while (currentWave < MaxWaves)
        {
            currentWave++;
            Debug.Log($"Волна {currentWave}");

            for (int i = 0; i < EnemiesPerWave; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(TimeBetweenSpawns);
            }

            yield return new WaitForSeconds(TimeBetweenWaves);
        }
    }

    void SpawnEnemy()
    {
        if (EnemyPrefabs.Length == 0 || SpawnPoints.Length == 0)
        {
            return;
        }

        GameObject prefab = EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)];
        Transform point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];

        GameObject enemy = Instantiate(prefab, point.position, point.rotation);

        EnemyController ctrl = enemy.GetComponent<EnemyController>();
    }
}
