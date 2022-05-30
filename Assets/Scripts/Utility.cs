using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility 
{
    public static T[] ShuffleArray<T>(T[] array, int seed)   // метод перемешивания массива листа координат
    {
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length -1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }
        return array;
    }

    public static List<Coord> GetAllCoords(Vector2 mapSize)
    {
        var result = new List<Coord>();         // инициализация листа с координатами
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                result.Add(new Coord(x, y));
            }
        }
        return result;
    }
}
