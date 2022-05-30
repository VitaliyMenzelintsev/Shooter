using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternateMapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2 mapSize;

    [Range(0, 1)]
    public float outlinePercent;        // отступ от каждого Tile
    [Range(0, 1)]
    public float obstaclePercent;       // процент препятствий на уровне

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
            Vector3 tilePosition = Coord.ToPosition(coord.x, coord.y, mapSize); // положение плитки = координате положения X и Y

            // размещение нового Tile из префаба, в заданной позиции с поворотом на 90 градусов по X
            Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;

            newTile.localScale = Vector3.one * (1 - outlinePercent);   // создание отступа
            newTile.parent = mapHolder;
        }

        bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y]; // двумерный массив переменных. X и Y приводятся к int

        int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);       // количество препятствий
        int currentObstacleCount = 0;                                             // текущее количество препятствий

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = shuffledTileCoords.Dequeue();     // верхняя координата из очереди перемешанных координат
            
            // если случайная координата не в центре карты и карта полностью доступна при заданных параметрах, то можно строить препятствия
            if (randomCoord != mapCentre && MapIsFullyAccesable(obstacleMap, currentObstacleCount))
            {
                CreateObstacle(randomCoord, ref obstacleMap, mapHolder);
            }
        }
    }

    private void CreateObstacle(Coord randomCoord, ref bool[,] obstacleMap, Transform mapHolder)
    {
        Vector3 obstaclePosition = Coord.ToPosition(randomCoord.x, randomCoord.y, mapSize);  // позиция препятствия = координате позиции случайных X и Y

        Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity) as Transform; // размещение препятствий
        newObstacle.parent = mapHolder;
        obstacleMap[randomCoord.x, randomCoord.y] = true;
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

    public Coord GetRandomCoord()    // метод получения случайной координаты
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();       // получение первого элемента очереди
        shuffledTileCoords.Enqueue(randomCoord);                // добавление полученной коррдинаты в конец очереди
        return randomCoord;                                     // возвращение случайной координаты
    }

    private void FillFields()
    {
        allTileCoords = Utility.GetAllCoords(mapSize);  // заполняем координаты
        // обращение к методу ShuffleArray скрипта Utility с параметрами всех координат плиток и семенем генерации
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), seed));
        mapCentre = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);
    }
}