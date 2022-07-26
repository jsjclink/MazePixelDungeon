using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum TERRAIN_TYPE
{
    EMPTY, FLOOR, WALL, BRIDGE, DOOR, STAIR
}
public enum SPECIFIC_TERRAIN_TYPE
{
    NONE, WALL_LEFT, WALL_RIGHT, WALL_TOP, WALL_BOTTOM
}

[Serializable]
public class TerrainInfo
{
    public TERRAIN_TYPE terrain_type;
    public SPECIFIC_TERRAIN_TYPE specific_terrain_type;
    public int pos_x;
    public int pos_y;

    public bool visited;
    public bool in_player_sight;
    
    public TerrainInfo(TERRAIN_TYPE terrain_type, SPECIFIC_TERRAIN_TYPE specific_terrain_type, int pos_x, int pos_y)
    {
        this.terrain_type = terrain_type;
        this.specific_terrain_type = specific_terrain_type;
        this.pos_x = pos_x;
        this.pos_y = pos_y;

        this.visited = false;
        this.in_player_sight = false;
    }
}
