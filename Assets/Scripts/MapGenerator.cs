using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    Map currentMap;                 // карта в массиве
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;
    public Vector2 maxMapSize;

    [Range(0, 1)]
    public float outlinePercent;        // отступ от каждого Tile
    public float tileSize;

    List<Coord> allTileCoords;          // массив лист со всеми координатами Tile
    Queue<Coord> shuffledTileCoords;    // массив очередь для хранения перемешанных координат
    Queue<Coord> shuffledOpenTileCoords;  // координаты открытых плиток
    Transform[,] tileMap;               // двумерный массив карты плиток. Будет нужен для поиска свободных плит для спавна врагов

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()                                      // генерация карты
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];   // изначально назначаем ей размер как у основной карты
        System.Random prng = new System.Random(currentMap.seed);   // рандомизация семечка
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.05f, currentMap.mapSize.y * tileSize);

        // Генерация координат
        allTileCoords = new List<Coord>();                  // инициализация листа с координатами
        for (int x = 0; x < currentMap.mapSize.x; x++)      // наполнение массива координатами X и Y
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        // обращение к методу ShuffleArray скрипта Utility, чтобы замешать координаты
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        // создание объекта Map Holder
        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // спавн плиток
        for (int x = 0; x < currentMap.mapSize.x; x++) 
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y); // положение плитки = координате положения X и Y

                // размещение нового Tile из префаба, в заданной позиции с поворотом на 90 градусов по X
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;

                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;   // создание отступа
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;   
            }
        }

        // создание препятствий
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y]; // двумерный массив переменных. X и Y приводятся к int

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);       // количество препятствий
        int currentObstacleCount = 0;                                             // текущее количество препятствий
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);               // дист координат всех открытых плиток

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();     // получение координаты из метода
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            // если случайная координата не в центре карты и карта полностью доступна, можно строить препятствия
            if (randomCoord != currentMap.mapCentre && MapIsFullyAccesable(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleheight, (float)prng.NextDouble()); // высота препятствий
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);  // позиция препятствия = координате позиции случайных X и Y

                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform; // размещение препятствий
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);

                // создание цветового градиента препятствий
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();            // отображение
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);   // создание материала
                float colourPercent = (float)randomCoord.y / currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregrounColour, currentMap.backgrounfColour, colourPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord); 
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

        // создание NavMesh рамок карты
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * ((currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize), Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * ((currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize), Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * ((currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize), Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * ((currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize), Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
    }


    bool MapIsFullyAccesable(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];  //создание нового массива
        Queue<Coord> queue = new Queue<Coord>();   // создание новой очереди
        queue.Enqueue(currentMap.mapCentre);
        mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

        int accesibleTileCount = 1; // до цикла проверки доступной считается только 1 плитка - центральная

        while (queue.Count > 0)      // проверка проходимости карты, учитывая препятствия
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
                                accesibleTileCount++;             // если цикл пройден успешно, количество доступных плиток увеличивается
                            }
                        }
                    }
                }
            }
        }
        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount); // количество доступных плиток размер карты минус количество препятствий
        return targetAccessibleTileCount == accesibleTileCount;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;    // позиционирование Tile. 0.5f - отступ между клетками
    }


    public Coord GetRandomCoord()    // метод получения случайной координаты
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();       // получение первого элемента очереди
        shuffledTileCoords.Enqueue(randomCoord);                // добавление полученной коррдинаты в конец очереди
        return randomCoord;                                     // возвращение случайной координаты
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    [System.Serializable]
    public struct Coord              // создание структуры с координатами
    {
        public int x;                // обьявили поля
        public int y;

        public Coord(int _x, int _y) // метод, принимающий и назначающий координаты
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


