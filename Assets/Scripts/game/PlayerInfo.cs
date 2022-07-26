using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlayerInfo: UnitInfo{
    public int hierarchy_idx;
    public int layer_idx;
    public int map_idx;
    public int hp = 100;
    public int attack_pt = 5;
    public int pos_x;
    public int pos_y;

    public List<(int, int)> cur_path;

    public EnemyInfo engaging_unit;

    public ANIMATION_STATE animation_state;

    public PlayerInfo()
    {
        this.unit_type = UNIT_TYPE.PLAYER;
        this.cur_path = new List<(int, int)>();
        this.unit_state = UNIT_STATE.IDLE;
        this.animation_state = ANIMATION_STATE.IDLE;
    }
    public PlayerInfo(int hierarchy_idx, int layer_idx, int map_idx, int hp, int attack_pt, int pos_x, int pos_y)
    {
        this.unit_type = UNIT_TYPE.PLAYER;
        this.cur_path = new List<(int, int)>();
        this.unit_state = UNIT_STATE.IDLE;
        this.hierarchy_idx = hierarchy_idx;
        this.layer_idx = layer_idx;
        this.map_idx = map_idx;
        this.hp = hp;
        this.attack_pt = attack_pt;
        this.pos_x = pos_x;
        this.pos_y = pos_y;
        this.animation_state = ANIMATION_STATE.IDLE;
    }

    public void SetPos(int x, int y)
    {
        this.pos_x = x;
        this.pos_y = y;
    }
    public void SetPathTo(int target_x, int target_y, TerrainInfo[,] map, List<EnemyInfo> enemy_list)
    {
        int height = map.GetLength(0); int width = map.GetLength(1);
        int[] dx = {0, 1, 1, 1, 0, -1, -1, -1};
        int[] dy = {1, 1, 0, -1, -1, -1, 0, 1};

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
        foreach(EnemyInfo enemy in enemy_list)
        {
            visited[enemy.pos_y, enemy.pos_x] = true;
        }
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
    }
    public bool PlayerSight(int target_x, int target_y, TerrainInfo[,] map)
    {
        int height = map.GetLength(0); int width = map.GetLength(1);
        int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
        int[] dy = { 1, 1, 0, -1, -1, -1, 0, 1 };

        bool[,] visited = new bool[height, width];
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

        if (prev[target_y, target_x] == (-1, -1)) return false;
        (int, int) cur_pt = prev[target_y, target_x];

        while (true)
        {
            int x = cur_pt.Item1;
            int y = cur_pt.Item2;
            if (x == -1 || y == -1) break;
            if ((x, y) == (this.pos_x, this.pos_y)) break;

            if (map[y, x].terrain_type == TERRAIN_TYPE.WALL || map[y, x].terrain_type == TERRAIN_TYPE.DOOR) return false;
            cur_pt = prev[y, x];
        }
        return true;
    }
    public bool DetectObstacle(int target_x, int target_y, List<EnemyInfo> enemy_list)
    {
        foreach (EnemyInfo enemy in enemy_list)
        {
            if (target_x == enemy.pos_x && target_y == enemy.pos_y) return true;
        }
        return false;
    }
    public void SetState(UNIT_STATE unit_state)
    {
        this.unit_state = unit_state;
    }
    public void SetState(UNIT_STATE unit_state, EnemyInfo enemy)
    {
        this.unit_state = unit_state;
        this.engaging_unit = enemy;
    }
}
