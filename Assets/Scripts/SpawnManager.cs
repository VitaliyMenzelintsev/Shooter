using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Wave[] waves;           // массив волн
    public Enemy enemy;            // тип врагов, которые спавнятся
    LivingEntity playerEntity;     // ссылка на игрока как сущность
    Transform playerT;             // ссылка на игрока как игровой обьект 

    Wave currentWave;              // текущая волна
    int currentWaveNumber;         // номер текущей волны

    int enemiesRemainingToSpawn;   // количество врагов, оставшееся до следующего спавна внутри волны
    int enemiesRemainigAlive;      // количество врагов, оставшихся вживых
    float nextSpawnTime;           // время до следующего спавна

    MapGenerator map;

    float timeBetweenCampingChecks = 2;          // время между прверками на кэмпинг
    float campingThresholdDistance = 1.5f;       // дистанция перемещения игрока, считающаяся кэмпингом
    float nextCampingCheckTime;                  // время следующей проверки
    Vector3 campingPositionOld;                  // координаты позиции кэмпинга
    bool isCamping;                              // игрок стоит на месте

    bool isDisabled;

    void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        nextCampingCheckTime = timeBetweenCampingChecks + Time.deltaTime;
        campingPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;         // подписка метода 

        map = FindObjectOfType<MapGenerator>(); 
        NextWave();                // запуск первой волны на старте
    }

    void Update()
    {
        if(Time.deltaTime > nextCampingCheckTime)
        {
            nextCampingCheckTime = timeBetweenCampingChecks + Time.deltaTime;                                 // время проверки
            isCamping = (Vector3.Distance(playerT.position, campingPositionOld) < campingThresholdDistance);  // проверка на кэмпинг
            campingPositionOld = playerT.position;
        }

        if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)    // если в волне ещё нужно заспавнить и времени прошло больше, чем заложено на спавн
        {
            enemiesRemainingToSpawn--;                                   // количество врагов, которых нужно заспавнить уменьшается
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;   // следующий спавн = текущее время + время между спавнами

            StartCoroutine(SpawnEnemy());
        }
    }

   IEnumerator SpawnEnemy()
    {
        // настройка мигания плитки перед спавном
        float spawnDelay = 1;
        float tileFlashSpeed = 4;
        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping) spawnTile = map.GetTileFromPosition(playerT.position);   // если игрок кэмпит, то место спавна перестаёт быть рандомным
        Material tileMaterial = spawnTile.GetComponent<Renderer>().material;          // сохранение цвета 
        Color initialColor = tileMaterial.color;         // опредление начального цвета 
        Color flashColor = Color.red;              // цвет мигания плитки при спавне врага
        float spawnTimer = 0;

        while(spawnTimer < spawnDelay)
        {
            tileMaterial.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));  // мигание

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;  // порождение врага
        spawnedEnemy.OnDeath += OnEnemyDeath;     // получение уведомления о смерти Врага
    }

    void OnPlayerDeath()
    {
        isDisabled = true;

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
