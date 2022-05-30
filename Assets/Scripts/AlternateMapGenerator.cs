using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternateMapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2 mapSize;

    [Range(0, 1)]
    public float outlinePercent;        // ������ �� ������� Tile
    [Range(0, 1)]
    public float obstaclePercent;       // ������� ����������� �� ������

    List<Coord> allTileCoords;          // ������ ���� �� ����� ������������ Tile
    Queue<Coord> shuffledTileCoords;    // ������ ������� ��� �������� ������������ ���������

    public int seed = 10;               // ���� ��������� �����������
    Coord mapCentre;

    void Start()
    {
        GenerateMap();
    }


    public void GenerateMap()                    // ��������� �����
    {
        FillFields();

        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        foreach (var coord in allTileCoords)
        {
            Vector3 tilePosition = Coord.ToPosition(coord.x, coord.y, mapSize); // ��������� ������ = ���������� ��������� X � Y

            // ���������� ������ Tile �� �������, � �������� ������� � ��������� �� 90 �������� �� X
            Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;

            newTile.localScale = Vector3.one * (1 - outlinePercent);   // �������� �������
            newTile.parent = mapHolder;
        }

        bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y]; // ��������� ������ ����������. X � Y ���������� � int

        int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);       // ���������� �����������
        int currentObstacleCount = 0;                                             // ������� ���������� �����������

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = shuffledTileCoords.Dequeue();     // ������� ���������� �� ������� ������������ ���������
            
            // ���� ��������� ���������� �� � ������ ����� � ����� ��������� �������� ��� �������� ����������, �� ����� ������� �����������
            if (randomCoord != mapCentre && MapIsFullyAccesable(obstacleMap, currentObstacleCount))
            {
                CreateObstacle(randomCoord, ref obstacleMap, mapHolder);
            }
        }
    }

    private void CreateObstacle(Coord randomCoord, ref bool[,] obstacleMap, Transform mapHolder)
    {
        Vector3 obstaclePosition = Coord.ToPosition(randomCoord.x, randomCoord.y, mapSize);  // ������� ����������� = ���������� ������� ��������� X � Y

        Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity) as Transform; // ���������� �����������
        newObstacle.parent = mapHolder;
        obstacleMap[randomCoord.x, randomCoord.y] = true;
    }

    bool MapIsFullyAccesable(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];  //�������� ������ �������
        Queue<Coord> queue = new Queue<Coord>();   // �������� ����� �������
        queue.Enqueue(mapCentre);
        mapFlags[mapCentre.x, mapCentre.y] = true;

        int accesibleTileCount = 1; // �� ����� �������� ��������� ��������� ������ 1 ������ - �����������

        while (queue.Count > 0)      // �������� ������������ �����, �������� �����������
        {
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accesibleTileCount++;             // ���� ���� ������� �������, ���������� ��������� ������ �������������
                            }
                        }
                    }
                }
            }
        }
        int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount); // ���������� ��������� ������ ������ ����� ����� ���������� �����������
        return targetAccessibleTileCount == accesibleTileCount;
    }

    public Coord GetRandomCoord()    // ����� ��������� ��������� ����������
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();       // ��������� ������� �������� �������
        shuffledTileCoords.Enqueue(randomCoord);                // ���������� ���������� ���������� � ����� �������
        return randomCoord;                                     // ����������� ��������� ����������
    }

    private void FillFields()
    {
        allTileCoords = Utility.GetAllCoords(mapSize);  // ��������� ����������
        // ��������� � ������ ShuffleArray ������� Utility � ����������� ���� ��������� ������ � ������� ���������
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), seed));
        mapCentre = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);
    }
}