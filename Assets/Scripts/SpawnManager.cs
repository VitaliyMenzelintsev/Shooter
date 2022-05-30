using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Wave[] waves;           // ������ ����
    public Enemy enemy;            // ��� ������, ������� ���������
    LivingEntity playerEntity;     // ������ �� ������ ��� ��������
    Transform playerT;             // ������ �� ������ ��� ������� ������ 

    Wave currentWave;              // ������� �����
    int currentWaveNumber;         // ����� ������� �����

    int enemiesRemainingToSpawn;   // ���������� ������, ���������� �� ���������� ������ ������ �����
    int enemiesRemainigAlive;      // ���������� ������, ���������� ������
    float nextSpawnTime;           // ����� �� ���������� ������

    MapGenerator map;

    float timeBetweenCampingChecks = 2;          // ����� ����� ��������� �� �������
    float campingThresholdDistance = 1.5f;       // ��������� ����������� ������, ����������� ���������
    float nextCampingCheckTime;                  // ����� ��������� ��������
    Vector3 campingPositionOld;                  // ���������� ������� ��������
    bool isCamping;                              // ����� ����� �� �����

    bool isDisabled;

    void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        nextCampingCheckTime = timeBetweenCampingChecks + Time.deltaTime;
        campingPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;         // �������� ������ 

        map = FindObjectOfType<MapGenerator>(); 
        NextWave();                // ������ ������ ����� �� ������
    }

    void Update()
    {
        if(Time.deltaTime > nextCampingCheckTime)
        {
            nextCampingCheckTime = timeBetweenCampingChecks + Time.deltaTime;                                 // ����� ��������
            isCamping = (Vector3.Distance(playerT.position, campingPositionOld) < campingThresholdDistance);  // �������� �� �������
            campingPositionOld = playerT.position;
        }

        if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)    // ���� � ����� ��� ����� ���������� � ������� ������ ������, ��� �������� �� �����
        {
            enemiesRemainingToSpawn--;                                   // ���������� ������, ������� ����� ���������� �����������
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;   // ��������� ����� = ������� ����� + ����� ����� ��������

            StartCoroutine(SpawnEnemy());
        }
    }

   IEnumerator SpawnEnemy()
    {
        // ��������� ������� ������ ����� �������
        float spawnDelay = 1;
        float tileFlashSpeed = 4;
        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping) spawnTile = map.GetTileFromPosition(playerT.position);   // ���� ����� ������, �� ����� ������ �������� ���� ���������
        Material tileMaterial = spawnTile.GetComponent<Renderer>().material;          // ���������� ����� 
        Color initialColor = tileMaterial.color;         // ���������� ���������� ����� 
        Color flashColor = Color.red;              // ���� ������� ������ ��� ������ �����
        float spawnTimer = 0;

        while(spawnTimer < spawnDelay)
        {
            tileMaterial.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));  // �������

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;  // ���������� �����
        spawnedEnemy.OnDeath += OnEnemyDeath;     // ��������� ����������� � ������ �����
    }

    void OnPlayerDeath()
    {
        isDisabled = true;

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
