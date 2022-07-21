using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SquareRoom;

public class CreateMap : MonoBehaviour
{
    [SerializeField]
    GameObject empty_prefab;
    [SerializeField]
    GameObject floor_prefab;
    [SerializeField]
    GameObject wall_prefab;
    [SerializeField]
    GameObject bridge_prefab;
    [SerializeField]
    GameObject door_prefab;

    void Start()
    {
        int width = 50; int height = 50;

        SquareRoomMap square_room_map = new SquareRoomMap(width, height, new MapOption(15, (2, 2, 2, 2)));


        int[,] map = square_room_map.GetMapArr();
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (map[i, j] == 0) Instantiate(empty_prefab, new Vector3(j + 0.5f, i + 0.5f, 0), Quaternion.identity);
                else if (map[i, j] == 1) Instantiate(floor_prefab, new Vector3(j + 0.5f, i + 0.5f, 0), Quaternion.identity);
                else if (map[i, j] == 2) Instantiate(wall_prefab, new Vector3(j + 0.5f, i + 0.5f, 0), Quaternion.identity);
                else if (map[i, j] == 3) Instantiate(bridge_prefab, new Vector3(j + 0.5f, i + 0.5f, 0), Quaternion.identity);
                else if (map[i, j] == 4) Instantiate(door_prefab, new Vector3(j + 0.5f, i + 0.5f, 0), Quaternion.identity);
            }
        }
    }
}
