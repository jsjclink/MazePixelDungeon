using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInfo
{
    public string id;
    public int hierarchy_idx;
    public int layer_idx;
    public int map_idx;

    public int width;
    public int height;

    public int floor;

    public List<MapInfo> connected_map_list;
    public List<StairInfo> stair_list;
    public List<BridgeInfo> bridge_list;

    //ป๓ลย
    public List<EnemyInfo> enemy_list;
    public List<ItemInfo> item_list;
    public TerrainInfo[, ] terrain_info_arr;
    public bool visited;

    public MapInfo(int hierarchy_idx, int layer_idx, int map_idx, int width, int height)
    {
        this.hierarchy_idx = hierarchy_idx;
        this.layer_idx = layer_idx;
        this.map_idx = map_idx;
        this.width = width;
        this.height = height;

        this.floor = hierarchy_idx * 5 + layer_idx;
        this.id = this.floor + "_" + map_idx;

        this.connected_map_list = new List<MapInfo>();
        this.stair_list = new List<StairInfo>();
        this.bridge_list = new List<BridgeInfo>();

        this.enemy_list = new List<EnemyInfo>();
        this.item_list = new List<ItemInfo>();
        this.terrain_info_arr = new TerrainInfo[height, width];
        this.visited = false;
    }
}
