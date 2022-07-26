using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EnemyInfo: UnitInfo{
    public int hierarchy_idx;
    public int layer_idx;
    public int map_idx;

    public int pos_x;
    public int pos_y;
    public int hp = 10;
    public int attack_pt = 5;

    public List<(int, int)> cur_path;

    public EnemyInfo(UNIT_TYPE unit_type, int x, int y, int hierarchy_idx, int layer_idx, int map_idx)
    {
        this.unit_type = unit_type;
        this.hierarchy_idx = hierarchy_idx;
        this.layer_idx = layer_idx;
        this.map_idx = map_idx;
        this.pos_x = x;
        this.pos_y = y;
        this.unit_state = UNIT_STATE.IDLE;
        cur_path = new List<(int, int)>();
    }
    public void SetPos(int x, int y)
    {
        this.pos_x = x;
        this.pos_y = y;
    }
    public bool DetectPlayer(PlayerInfo playerInfo, TerrainInfo[,] map)
    {
        int dx = playerInfo.pos_x - this.pos_x;
        int dy = playerInfo.pos_y - this.pos_y;
        if (dx * dx + dy * dy < 20) return true;
        else return false;
    }
    public void SetPathTo(int target_x, int target_y, TerrainInfo[,] map, List<EnemyInfo> enemy_list, PlayerInfo playerInfo)
    {
        int height = map.GetLength(0); int width = map.GetLength(1);
        int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
        int[] dy = { 1, 1, 0, -1, -1, -1, 0, 1 };

        this.cur_path = new List<(int, int)>();

        if (map[target_y, target_x].terrain_type == TERRAIN_TYPE.EMPTY || map[target_y, target_x].terrain_type == TERRAIN_TYPE.WALL) return;

        bool[,] visited = new bool[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (map[i, j].terrain_type == TERRAIN_TYPE.EMPTY || map[i, j].terrain_type == TERRAIN_TYPE.WALL) visited[i, j] = true;
                else visited[i, j] = false;
            }
        }
        foreach (EnemyInfo enemy in enemy_list)
        {
            visited[enemy.pos_y, enemy.pos_x] = true;
        }
        if(target_x != playerInfo.pos_x || target_y != playerInfo.pos_y) visited[playerInfo.pos_y, playerInfo.pos_x] = true;
        visited[this.pos_y, this.pos_x] = true;

        (int, int)[,] prev = new (int, int)[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                prev[i, j] = (-1, -1);
            }
        }

        Queue<(int, int)> queue = new Queue<(int, int)>();
        queue.Enqueue((this.pos_x, this.pos_y));

        while (queue.Count > 0)
        {
            (int, int) cur = queue.Dequeue();
            int x = cur.Item1;
            int y = cur.Item2;

            for (int i = 0; i < 8; i++)
            {
                int next_x = x + dx[i];
                int next_y = y + dy[i];
                if (next_x >= 0 && next_x < width && next_y >= 0 && next_y < height && !visited[next_y, next_x])
                {
                    visited[next_y, next_x] = true;
                    queue.Enqueue((next_x, next_y));
                    prev[next_y, next_x] = (x, y);
                }
            }
        }

        (int, int) cur_pt = (target_x, target_y);
        if (prev[target_y, target_x] == (-1, -1)) return;

        Stack<(int, int)> tmp = new Stack<(int, int)>();
        while (true)
        {
            int x = cur_pt.Item1;
            int y = cur_pt.Item2;
            if (x == -1 || y == -1) break;
            if ((x, y) == (this.pos_x, this.pos_y)) break;

            tmp.Push(cur_pt);
            cur_pt = prev[y, x];
        }
        while (tmp.Count > 0)
        {
            this.cur_path.Add(tmp.Pop());
        }
        this.cur_path.RemoveAt(this.cur_path.Count - 1);
    }
    public bool DetectObstacle(int target_x, int target_y, List<EnemyInfo> enemy_list, PlayerInfo playerInfo)
    {
        foreach(EnemyInfo enemy in enemy_list)
        {
            if (target_x == enemy.pos_x && target_y == enemy.pos_y) return true;
        }
        if (target_x == playerInfo.pos_x && target_y == playerInfo.pos_y) return true;

        return false;
    }
}
