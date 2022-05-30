using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Wave[] waves;           // массив волн
    public Enemy enemy;            // тип врагов, которые спавнятся

    Wave currentWave;              // текущая волна
    int currentWaveNumber;         // номер текущей волны

    int enemiesRemainingToSpawn;   // количество врагов, оставшееся до следующего спавна внутри волны
    int enemiesRemainigAlive;      // количество врагов, оставшихся вживых
    float nextSpawnTime;           // время до следующего спавна

    private void Start()
    {
        NextWave();                // запуск первой волны на старте
    }

    void Update()
    {
        if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)    // если в волне ещё нужно заспавнить и времени прошло больше, чем заложено на спавн
        {
            enemiesRemainingToSpawn--;                                   // количество врагов, которых нужно заспавнить уменьшается
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;   // следующий спавн = текущее время + время между спавнами

            Enemy spawnedEnemy = Instantiate(enemy, Vector3.zero, Quaternion.identity) as Enemy;  // порождение врага
            spawnedEnemy.OnDeath += OnEnemyDeath;     // получение уведомления о смерти Врага
        }
    }

    void OnEnemyDeath()
    {
        enemiesRemainigAlive--;        // количество живых врагов уменьшается

        if (enemiesRemainigAlive == 0)  // если количество живых врагов = 0
        {
            NextWave();                // запуск следующей волны
        }
    }

    void NextWave()
    {
        currentWaveNumber++;                                   // прирост номера волны
        print("Wave: " + currentWaveNumber);
        if (currentWaveNumber - 1 < waves.Length) 
        {
            currentWave = waves[currentWaveNumber - 1];         // текущая волна

            enemiesRemainingToSpawn = currentWave.enemyCount;   // количество врагов до спавна = количество врагов в текущей волне
            enemiesRemainigAlive = enemiesRemainingToSpawn;
        }
    }

    [System.Serializable]
    public class Wave                         // класс волна описывает её характеристики
    {
        public int enemyCount;                // количество врагов
        public float timeBetweenSpawns;       // время между появлением врагов внутри волны

    }
}
