using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareSpaceInfo
{
    public int start_x;
    public int start_y;
    public int end_x;
    public int end_y;

    public SquareSpaceInfo(int s_x, int s_y, int e_x, int e_y)
    {
        start_x = s_x;
        start_y = s_y;
        end_x = e_x;
        end_y = e_y;
    }
}


public class SquareRoomMapInfo : MapInfo
{
    public List<SquareSpaceInfo> space_list;

    public SquareRoomMapInfo(int hierarchy_idx, int layer_idx, int map_idx, int width, int height) : base(hierarchy_idx, layer_idx, map_idx, width, height)
    {
        this.space_list = DivideRect(new SquareSpaceInfo(2, 2, width - 2, height - 2), true); //항상 맵 끝은 2짜리 padding

        int remove_cnt = 0;
        if (this.space_list.Count - 15 > 0) remove_cnt = this.space_list.Count - 15; // 방 최대 15개
        for (int i = 0; i < remove_cnt; i++) RemoveRandomSpace();

        AddPaddingToRoom(2,2,2,2); //각 방마다 padding 2

        this.bridge_list = ConnectRectSpace(); // bridge list 만듦

        this.enemy_list = CreateEnemies(); //enemy 생성

        //star_list도 만들어야함 -> dungeon.cs에서 만듦 왜냐하면 hierarchy, layer 연결 다 끝나야 그걸 연결하는 stair를 만드니까
    }

    private int[,] GetMapArr()
    {
        int[,] map = new int[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                map[i, j] = 0; //EMPTY
            }
        }

        //space를 그림
        foreach (SquareSpaceInfo cur in this.space_list)
        {
            for (int i = cur.start_y; i < cur.end_y; i++)
            {
                for (int j = cur.start_x; j < cur.end_x; j++)
                {
                    if (i == cur.start_y || j == cur.start_x || i == cur.end_y - 1 || j == cur.end_x - 1) map[i, j] = 2; // wall
                    else map[i, j] = 1; //FLOOR
                }
            }
        }

        //stair를 그림
        foreach(StairInfo cur in this.stair_list)
        {
            map[cur.pos_y, cur.pos_x] = 5; //STAIR
        }


        //bridge를 그림
        foreach (BridgeInfo cur in this.bridge_list)
        {
            for (int i = 0; i < cur.path_arr.Count; i++)
            {
                int x = cur.path_arr[i].Item1;
                int y = cur.path_arr[i].Item2;
                if (map[y, x] == 2) map[y, x] = 4; //DOOR
                else if (map[y, x] == 0) map[y, x] = 3; //BRIDGE
            }
        }

        //주변에 bridge가 없으면 문을 없앰 (벽 끝에 연속적으로 생성된 문을 floor로 바꿈)
        foreach (SquareSpaceInfo cur in this.space_list)
        {
            for (int i = cur.start_y; i < cur.end_y; i++)
            {
                for (int j = cur.start_x; j < cur.end_x; j++)
                {
                    if (map[i, j] == 4)
                    {
                        if (map[i + 1, j] != 3 && map[i - 1, j] != 3 && map[i, j + 1] != 3 && map[i, j - 1] != 3) 
                        {
                            map[i, j] = 1; //FLOOR
                        }
                    }
                }
            }
        }

        //주변에 wall이 없으면 문을 없앰
        foreach (SquareSpaceInfo cur in space_list)
        {
            for (int i = cur.start_y; i < cur.end_y; i++)
            {
                for (int j = cur.start_x; j < cur.end_x; j++)
                {
                    if (map[i, j] == 4)
                    {
                        if (map[i + 1, j] != 2 && map[i - 1, j] != 2 && map[i, j + 1] != 2 && map[i, j - 1] != 2)
                        {
                            map[i, j] = 1; //FLOOR
                        }
                    }
                }
            }
        }

        return map;
    }

    public (int[,], List<EnemyInfo>) GetMapInfos()
    {
        return (GetMapArr(), this.enemy_list);
    }

    private List<SquareSpaceInfo> DivideRect(SquareSpaceInfo cur, bool vertical)
    {
        if (cur.end_x - cur.start_x < 15 || cur.end_y - cur.start_y < 15) return new List<SquareSpaceInfo>();

        List<SquareSpaceInfo> arr1 = new List<SquareSpaceInfo>();
        List<SquareSpaceInfo> arr2 = new List<SquareSpaceInfo>();
        List<SquareSpaceInfo> ret_arr = new List<SquareSpaceInfo>();

        if (vertical)
        {
            int weight1 = Random.Range(1, 3);
            int weight2 = Random.Range(1, 3);
            int mid = (cur.start_x * weight1 + cur.end_x * weight2) / (weight1 + weight2);

            bool divide_vertical;
            if (mid - cur.start_x > cur.end_y - cur.start_y) divide_vertical = true;
            else divide_vertical = false;
            arr1 = DivideRect(new SquareSpaceInfo(cur.start_x, cur.start_y, mid, cur.end_y), divide_vertical);
            if (arr1.Count == 0)
            {
                SquareSpaceInfo new_space = new SquareSpaceInfo(cur.start_x, cur.start_y, mid, cur.end_y);
                ret_arr.Add(new_space);
            }
            while (arr1.Count > 0)
            {
                ret_arr.Add(arr1[0]);
                arr1.RemoveAt(0);
            }
            if (cur.end_x - mid > cur.end_y - cur.start_y) divide_vertical = true;
            else divide_vertical = false;
            arr2 = DivideRect(new SquareSpaceInfo(mid, cur.start_y, cur.end_x, cur.end_y), divide_vertical);
            if (arr2.Count == 0)
            {
                SquareSpaceInfo new_space = new SquareSpaceInfo(mid, cur.start_y, cur.end_x, cur.end_y);
                ret_arr.Add(new_space);
            }
            while (arr2.Count > 0)
            {
                ret_arr.Add(arr2[0]);
                arr2.RemoveAt(0);
            }
        }
        else
        {
            int weight1 = Random.Range(1, 3);
            int weight2 = Random.Range(1, 3);
            int mid = (cur.start_y * weight1 + cur.end_y * weight2) / (weight1 + weight2);

            bool divide_vertical;
            if (cur.end_x - cur.start_x > mid - cur.start_y) divide_vertical = true;
            else divide_vertical = false;
            arr1 = DivideRect(new SquareSpaceInfo(cur.start_x, cur.start_y, cur.end_x, mid), divide_vertical);
            if (arr1.Count == 0)
            {
                SquareSpaceInfo new_space = new SquareSpaceInfo(cur.start_x, cur.start_y, cur.end_x, mid);
                ret_arr.Add(new_space);
            }
            while (arr1.Count > 0)
            {
                ret_arr.Add(arr1[0]);
                arr1.RemoveAt(0);
            }
            if (cur.end_x - cur.start_x > cur.end_y - mid) divide_vertical = true;
            else divide_vertical = false;
            arr2 = DivideRect(new SquareSpaceInfo(cur.start_x, mid, cur.end_x, cur.end_y), divide_vertical);
            if (arr2.Count == 0)
            {
                SquareSpaceInfo new_space = new SquareSpaceInfo(cur.start_x, mid, cur.end_x, cur.end_y);
                ret_arr.Add(new_space);
            }
            while (arr2.Count > 0)
            {
                ret_arr.Add(arr2[0]);
                arr2.RemoveAt(0);
            }
        }
        return ret_arr;
    }
    private void RemoveRandomSpace()
    {
        space_list.RemoveAt(Random.Range(0, space_list.Count));
    }

    private void AddPaddingToRoom(int top, int bottom, int left, int right)
    {
        foreach (SquareSpaceInfo cur in space_list)
        {
            if (cur.start_x + left + 7 < cur.end_x) cur.start_x += left;
            if (cur.start_y + top + 7 < cur.end_y) cur.start_y += top;
            if (cur.end_x - right - 7 > cur.start_x) cur.end_x -= right;
            if (cur.end_y - bottom - 7 > cur.start_y) cur.end_y -= bottom;
        }
    }

    private List<BridgeInfo> ConnectRectSpace()
    {
        List<SquareSpaceInfo> unconnected_space_list = new List<SquareSpaceInfo>(space_list);
        List<BridgeInfo> bridge_list = new List<BridgeInfo>();

        while (unconnected_space_list.Count > 1)
        {
            SquareSpaceInfo space1 = unconnected_space_list[0];
            SquareSpaceInfo space2 = unconnected_space_list[1];

            unconnected_space_list.RemoveAt(0);
            unconnected_space_list.RemoveAt(0);

            unconnected_space_list.Add(space1);

            bridge_list.Add(new BridgeInfo(space1, space2));
        }

        return bridge_list;
    }

    private List<EnemyInfo> CreateEnemies()
    {
        List<EnemyInfo> enemy_list = new List<EnemyInfo>();
        for (int i = 0; i < 10; i++)
        {
            int idx = Random.Range(0, space_list.Count);
            int spawn_x = Random.Range(space_list[idx].start_x + 1, space_list[idx].end_x - 1);
            int spawn_y = Random.Range(space_list[idx].start_y + 1, space_list[idx].end_y - 1);
            enemy_list.Add(new EnemyInfo(spawn_x, spawn_y));
        }
        return enemy_list;
    }

    public void CreateStairs()
    {
        List<StairInfo> stair_list = new List<StairInfo>();

        foreach(MapInfo to in this.connected_map_list)
        {
            int idx = Random.Range(0, space_list.Count);
            int spawn_x = Random.Range(space_list[idx].start_x + 1, space_list[idx].end_x - 1);
            int spawn_y = Random.Range(space_list[idx].start_y + 1, space_list[idx].end_y - 1);
            stair_list.Add(new StairInfo(spawn_x, spawn_y, to));
        }

        this.stair_list = stair_list;
    }
}
