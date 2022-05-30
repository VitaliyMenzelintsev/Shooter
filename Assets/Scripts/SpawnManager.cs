using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Wave[] waves;           // ������ ����
    public Enemy enemy;            // ��� ������, ������� ���������

    Wave currentWave;              // ������� �����
    int currentWaveNumber;         // ����� ������� �����

    int enemiesRemainingToSpawn;   // ���������� ������, ���������� �� ���������� ������ ������ �����
    int enemiesRemainigAlive;      // ���������� ������, ���������� ������
    float nextSpawnTime;           // ����� �� ���������� ������

    MapGenerator map;

    void Start()
    {
        map = FindObjectOfType<MapGenerator>(); 
        NextWave();                // ������ ������ ����� �� ������
    }

    void Update()
    {
        if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)    // ���� � ����� ��� ����� ���������� � ������� ������ ������, ��� �������� �� �����
        {
            enemiesRemainingToSpawn--;                                   // ���������� ������, ������� ����� ���������� �����������
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;   // ��������� ����� = ������� ����� + ����� ����� ��������

            StartCoroutine(SpawnEnemy());
        }
    }

   IEnumerator SpawnEnemy()
    {
        Transform randomTile = map.GetRandomOpenTile();
        Enemy spawnedEnemy = Instantiate(enemy, Vector3.zero, Quaternion.identity) as Enemy;  // ���������� �����
        spawnedEnemy.OnDeath += OnEnemyDeath;     // ��������� ����������� � ������ �����
    }

    void OnEnemyDeath()
    {
        enemiesRemainigAlive--;        // ���������� ����� ������ �����������

        if (enemiesRemainigAlive == 0)  // ���� ���������� ����� ������ = 0
        {
            NextWave();                // ������ ��������� �����
        }
    }

    void NextWave()
    {
        currentWaveNumber++;                                   // ������� ������ �����
        print("Wave: " + currentWaveNumber);
        if (currentWaveNumber - 1 < waves.Length) 
        {
            currentWave = waves[currentWaveNumber - 1];         // ������� �����

            enemiesRemainingToSpawn = currentWave.enemyCount;   // ���������� ������ �� ������ = ���������� ������ � ������� �����
            enemiesRemainigAlive = enemiesRemainingToSpawn;
        }
    }

    [System.Serializable]
    public class Wave                         // ����� ����� ��������� � ��������������
    {
        public int enemyCount;                // ���������� ������
        public float timeBetweenSpawns;       // ����� ����� ���������� ������ ������ �����

    }
}
