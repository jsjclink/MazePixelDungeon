using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SquareRoom;

public class GameSystemManager : MonoBehaviour
{
    const int EMPTY = 0;
    const int FLOOR = 1;
    const int WALL = 2;
    const int BRIDGE = 3;
    const int DOOR = 4;

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
    [SerializeField]
    GameObject player;

    [SerializeField]
    Camera cam;

    float touch_interval = 0.0f;
    int[,] cur_map;

    void Start()
    {
        int width = 70; int height = 70;

        SquareRoomMap square_room_map = new SquareRoomMap(width, height, new MapOption(15, (2, 2, 2, 2)));

        cur_map = square_room_map.GetMapArr();
        int[,] map = cur_map;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (map[i, j] == 0) Instantiate(empty_prefab, new Vector3(j, i, 0), Quaternion.identity);
                else if (map[i, j] == 1) Instantiate(floor_prefab, new Vector3(j, i, 0), Quaternion.identity);
                else if (map[i, j] == 2) Instantiate(wall_prefab, new Vector3(j, i, 0), Quaternion.identity);
                else if (map[i, j] == 3) Instantiate(bridge_prefab, new Vector3(j, i, 0), Quaternion.identity);
                else if (map[i, j] == 4) Instantiate(door_prefab, new Vector3(j, i, 0), Quaternion.identity);
            }
        }

        List<SquareSpaceInfo> space_list = square_room_map.space_list;
        int idx = Random.Range(0, space_list.Count);
        int spawn_x = Random.Range(space_list[idx].start_x+1, space_list[idx].end_x-1);
        int spawn_y = Random.Range(space_list[idx].start_y+1, space_list[idx].end_y-1);

        player.transform.position = new Vector3(spawn_x, spawn_y, 0);

        cam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);
    }

    private void Update()
    {
        if (Input.touchCount == 1)
        {
            touch_interval += Time.deltaTime;

            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touch_interval = 0.0f;
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                if(touch_interval < 0.1f)
                {
                    Debug.Log("touch_interval short");
                    Vector3 touch_pos = Camera.main.ScreenToWorldPoint(touch.position);
                    touch_pos = new Vector3((int)touch_pos.x, (int)touch_pos.y, 0);
                    Debug.Log("touch x, y : " + touch_pos.x + ", " + touch_pos.y);
                    List<(int, int)> path = CalculatePath(player.transform.position, touch_pos, cur_map);
                    if(path.Count > 0)
                    {
                        Debug.Log("path length : " + path.Count);
                        foreach((int, int) cur in path)
                        {
                            int x = cur.Item1;
                            int y = cur.Item2;
                            Instantiate(wall_prefab, new Vector3(x, y, 0), Quaternion.identity);
                        }
                        player.transform.position = touch_pos;
                    }
                    else
                    {
                        Debug.Log("path zero");
                    }
                }
                else
                {
                    Debug.Log("touch_interval long");
                }
            }
        }
    }

    private List<(int, int)> CalculatePath(Vector3 from, Vector3 to, int[,] map)
    {
        int height = map.GetLength(0); int width = map.GetLength(1);
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        List<(int, int)> path_arr = new List<(int, int)>();

        if (map[(int)to.y, (int)to.x] == EMPTY || map[(int)to.y, (int)to.x] == WALL) return path_arr;

        bool[,] visited = new bool[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (map[i, j] == EMPTY || map[i, j] == WALL) visited[i, j] = true;
                else visited[i, j] = false;
            }
        }
        visited[(int)from.y, (int)from.x] = true;

        (int, int)[,] prev = new (int, int)[height, width];
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                prev[i, j] = (-1, -1);
            }
        }

        Queue<(int, int)> queue = new Queue<(int, int)>();
        queue.Enqueue(((int)from.x, (int)from.y));

        while(queue.Count > 0)
        {
            (int, int) cur = queue.Dequeue();
            int x = cur.Item1;
            int y = cur.Item2;

            for(int i = 0; i < 4; i++)
            {
                int next_x = x + dx[i];
                int next_y = y + dy[i];
                if(next_x >= 0 && next_x < width && next_y >= 0 && next_y < height && !visited[next_y, next_x])
                {
                    visited[next_y, next_x] = true;
                    queue.Enqueue((next_x, next_y));
                    prev[next_y, next_x] = (x, y);
                }
            }
        }

        (int, int) cur_pt = ((int)to.x, (int)to.y);
        if (prev[(int)to.y, (int)to.x] == (-1, -1)) return path_arr;
        while (true)
        {
            int x = cur_pt.Item1;
            int y = cur_pt.Item2;
            if (x == -1 || y == -1) break;
            if ((x, y) == ((int)from.x, (int)from.y)) break;

            path_arr.Add(cur_pt);
            cur_pt = prev[y, x];
        }
        return path_arr;
    }
}