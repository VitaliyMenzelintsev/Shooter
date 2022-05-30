using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    Map currentMap;                 // ����� � �������
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    public Vector2 maxMapSize;

    [Range(0, 1)]
    public float outlinePercent;        // ������ �� ������� Tile
    public float tileSize;

    List<Coord> allTileCoords;          // ������ ���� �� ����� ������������ Tile
    Queue<Coord> shuffledTileCoords;    // ������ ������� ��� �������� ������������ ���������

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()                                      // ��������� �����
    {
        System.Random prng = new System.Random(currentMap.seed);   // ������������ �������
        currentMap = maps[mapIndex];

        // ��������� ���������
        allTileCoords = new List<Coord>();                  // ������������� ����� � ������������
        for (int x = 0; x < currentMap.mapSize.x; x++)      // ���������� ������� ������������ X � Y
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        // ��������� � ������ ShuffleArray ������� Utility, ����� �������� ����������
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        // �������� ������� Map Holder
        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // ����� ������
        for (int x = 0; x < currentMap.mapSize.x; x++) 
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y); // ��������� ������ = ���������� ��������� X � Y

                // ���������� ������ Tile �� �������, � �������� ������� � ��������� �� 90 �������� �� X
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;

                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;   // �������� �������
                newTile.parent = mapHolder;
            }
        }

        // �������� �����������
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y]; // ��������� ������ ����������. X � Y ���������� � int

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);       // ���������� �����������
        int currentObstacleCount = 0;                                             // ������� ���������� �����������

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();     // ��������� ���������� �� ������
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            // ���� ��������� ���������� �� � ������ ����� � ����� ��������� ��������, ����� ������� �����������
            if (randomCoord != currentMap.mapCentre && MapIsFullyAccesable(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleheight, (float)prng.NextDouble()); // ������ �����������
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);  // ������� ����������� = ���������� ������� ��������� X � Y

                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform; // ���������� �����������
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        // �������� NavMesh ����� �����
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * ((currentMap.mapSize.x + maxMapSize.x) / 4 * tileSize), Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * ((currentMap.mapSize.x + maxMapSize.x) / 4 * tileSize), Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * ((currentMap.mapSize.y + maxMapSize.y) / 4 * tileSize), Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * ((currentMap.mapSize.y + maxMapSize.y) / 4 * tileSize), Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
    }


    bool MapIsFullyAccesable(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];  //�������� ������ �������
        Queue<Coord> queue = new Queue<Coord>();   // �������� ����� �������
        queue.Enqueue(currentMap.mapCentre);
        mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

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
        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount); // ���������� ��������� ������ ������ ����� ����� ���������� �����������
        return targetAccessibleTileCount == accesibleTileCount;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2 + 0.5f + x, 0, -currentMap.mapSize.y / 2 + 0.5f + y) * tileSize;    // ���������������� Tile. 0.5f - ������ ����� ��������
    }


    public Coord GetRandomCoord()    // ����� ��������� ��������� ����������
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();       // ��������� ������� �������� �������
        shuffledTileCoords.Enqueue(randomCoord);                // ���������� ���������� ���������� � ����� �������
        return randomCoord;                                     // ����������� ��������� ����������
    }

    [System.Serializable]
    public struct Coord              // �������� ��������� � ������������
    {
        public int x;                // �������� ����
        public int y;

        public Coord(int _x, int _y) // �����, ����������� � ����������� ����������
        {
            x = _x;
            y = _y;
        }
        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }
    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0, 1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleheight;
        public Color foregrounColour;
        public Color backgrounfColour;

        public Coord mapCentre
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}

