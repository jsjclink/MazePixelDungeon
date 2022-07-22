using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairInfo
{
    public int pos_x;
    public int pos_y;
    public MapInfo connected_map;
    public StairInfo(int x, int y, MapInfo connected_map)
    {
        this.pos_x = x;
        this.pos_y = y;
        this.connected_map = connected_map;
    }
}
