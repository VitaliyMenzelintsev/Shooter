using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;
    public Vector2 mapSize;
    public Vector2 maxMapSize;

    [Range(0, 1)]
    public float outlinePercent;        // отступ от каждого Tile
    [Range(0, 1)]
    public float obstaclePercent;       // процент препятствий на уровне

    public float tileSize;

     List<Coord> allTileCoords;          // массив лист со всеми координатами Tile
    Queue<Coord> shuffledTileCoords;    // массив очередь для хранения перемешанных координат

    public int seed = 10;               // семя генерации препятствий
    Coord mapCentre;

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()                    // генерация карты
    {
        allTileCoords = new List<Coord>();       // инициализация листа с координатами
        for (int x = 0; x < mapSize.x; x++)      // наполнение массива координатами X и Y
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        // обращение к методу ShuffleArray скрипта Utility с параметрами всех координат плиток и семенем генерации
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), seed));
        mapCentre = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);     

        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for (int x = 0; x < mapSize.x; x++)     // пока Х меньше, чем размер карты, Х приращается
        {
            for (int y = 0; y < mapSize.y; y++)    // пока Y меньше, чем размер карты, Y приращается
            {
                Vector3 tilePosition = CoordToPosition(x, y); // положение плитки = координате положения X и Y

                // размещение нового Tile из префаба, в заданной позиции с поворотом на 90 градусов по X
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;

                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;   // создание отступа
                newTile.parent = mapHolder;
            }
        }

        bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y]; // двумерный массив переменных. X и Y приводятся к int

        int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);       // количество препятствий
        int currentObstacleCount = 0;                                             // текущее количество препятствий

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();     // получение координаты из метода
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            // если случайная координата не в центре карты и карта полностью доступна при заданных параметрах, то можно строить препятствия
            if (randomCoord != mapCentre && MapIsFullyAccesable(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);  // позиция препятствия = координате позиции случайных X и Y

                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity) as Transform; // размещение препятствий
                newObstacle.parent = mapHolder;
                newObstacle.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left*((mapSize.x + maxMapSize.x) / 4 * tileSize), Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - maxMapSize.x) / 2, 1, mapSize.y) * tileSize;
        
        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * ((mapSize.x + maxMapSize.x) / 4 * tileSize), Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - maxMapSize.x) / 2, 1, mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * ((mapSize.y + maxMapSize.y) / 4 * tileSize), Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - mapSize.y) / 2) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * ((mapSize.y + maxMapSize.y) / 4 * tileSize), Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - mapSize.y) / 2) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
    }


    bool MapIsFullyAccesable(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];  //создание нового массива
        Queue<Coord> queue = new Queue<Coord>();   // создание новой очереди
        queue.Enqueue(mapCentre);
        mapFlags[mapCentre.x, mapCentre.y] = true;

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
        int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount); // количество доступных плиток размер карты минус количество препятствий
        return targetAccessibleTileCount == accesibleTileCount;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y) * tileSize;    // позиционирование Tile. 0.5f - отступ между клетками
    }


    public Coord GetRandomCoord()    // метод получения случайной координаты
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();       // получение первого элемента очереди
        shuffledTileCoords.Enqueue(randomCoord);                // добавление полученной коррдинаты в конец очереди
        return randomCoord;                                     // возвращение случайной координаты
    }

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
}
