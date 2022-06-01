using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public bool devMode;
         
    public Wave[] waves;           // массив волн
    public Enemy enemy;            // тип врагов, которые спавн€тс€
    LivingEntity playerEntity;     // ссылка на игрока как сущность
    Transform playerT;             // ссылка на игрока как игровой обьект 

    Wave currentWave;              // текуща€ волна
    int currentWaveNumber;         // номер текущей волны

    int enemiesRemainingToSpawn;   // количество врагов, оставшеес€ до следующего спавна внутри волны
    int enemiesRemainigAlive;      // количество врагов, оставшихс€ вживых
    float nextSpawnTime;           // врем€ до следующего спавна

    MapGenerator map;

    float timeBetweenCampingChecks = 2;          // врем€ между прверками на кэмпинг
    float campingThresholdDistance = 1.5f;       // дистанци€ перемещени€ игрока, считающа€с€ кэмпингом
    float nextCampingCheckTime;                  // врем€ следующей проверки
    Vector3 campingPositionOld;                  // координаты позиции кэмпинга
    bool isCamping;                              // игрок стоит на месте

    bool isDisabled;                             // используетс€ в событии смерти игрока

    public event System.Action<int> OnNewWave;

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
            nextCampingCheckTime = timeBetweenCampingChecks + Time.deltaTime;                                 // врем€ проверки
            
            isCamping = (Vector3.Distance(playerT.position, campingPositionOld) < campingThresholdDistance);  // проверка на кэмпинг
            campingPositionOld = playerT.position;
        }

        if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)    // если в волне ещЄ нужно заспавнить и времени прошло больше, чем заложено на спавн
        {
            enemiesRemainingToSpawn--;                                   // количество врагов, которых нужно заспавнить уменьшаетс€
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;   // следующий спавн = текущее врем€ + врем€ между спавнами

            StartCoroutine("SpawnEnemy");
        }
        if (devMode)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine("SpawnEnemy");
                foreach(Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    GameObject.Destroy(enemy.gameObject);
                }
                NextWave();
            }
        }
    }

   IEnumerator SpawnEnemy()
    {
        // настройка мигани€ плитки перед спавном
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping)
        {
            spawnTile = map.GetTileFromPosition(playerT.position); // если игрок кэмпит, то место спавна перестаЄт быть рандомным
        }   
        Material tileMaterial = spawnTile.GetComponent<Renderer>().material;          // сохранение цвета 
        Color initialColor = tileMaterial.color;         // опредление начального цвета 
        Color flashColor = Color.red;              // цвет мигани€ плитки при спавне врага
        float spawnTimer = 0;

        while(spawnTimer < spawnDelay)
        {
            tileMaterial.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));  // мигание

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;  // порождение врага
        spawnedEnemy.OnDeath += OnEnemyDeath;     // получение уведомлени€ о смерти ¬рага
        
        spawnedEnemy.SetCharacteristics           // создание врага с конкретными характеристиками
            (currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);
    }

    void OnPlayerDeath()
    {
        isDisabled = true;

    }

    void OnEnemyDeath()
    {
        enemiesRemainigAlive--;        // количество живых врагов уменьшаетс€

        if (enemiesRemainigAlive == 0)  // если количество живых врагов = 0
        {
            NextWave();                // запуск следующей волны
        }
    }

    //void ResetPlayerPosition()
    //{
    //    playerT.position = map.GetTileFromPosition(Vector3.zero).position + (Vector3.up * 3);
    //}

    void NextWave()
    {
        currentWaveNumber++;                                   // прирост номера волны
        print("Wave: " + currentWaveNumber);
        if (currentWaveNumber - 1 < waves.Length) 
        {
            currentWave = waves[currentWaveNumber - 1];         // текуща€ волна

            enemiesRemainingToSpawn = currentWave.enemyCount;   // количество врагов до спавна = количество врагов в текущей волне
            enemiesRemainigAlive = enemiesRemainingToSpawn;

            if (OnNewWave != null)                              
            {
                OnNewWave(currentWaveNumber);
            }
            //ResetPlayerPosition();
        }
    }

    [System.Serializable]
    public class Wave                         // класс волна описывает еЄ характеристики
    {
        public bool infinite;
        public int enemyCount;                // количество врагов
        public float timeBetweenSpawns;       // врем€ между по€влением врагов внутри волны

        public float moveSpeed;               // скорость врагов
        public int hitsToKillPlayer;          // урон до убийства игрока
        public float enemyHealth;
        public Color skinColor;         
    }
}
