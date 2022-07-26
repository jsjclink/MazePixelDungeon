using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Dungeon
{
    public HierarchyInfo[] hierarchy_list;
    
    public Dungeon(int difficulty)
    {
        this.hierarchy_list = new HierarchyInfo[5];
        for (int i = 0; i < 5; i++)
        {
            this.hierarchy_list[i] = new HierarchyInfo(i, difficulty);
        }
        for (int i = 0; i < 4; i++)
        {
            this.hierarchy_list[i].Connect(this.hierarchy_list[i + 1]);
        }

        for(int i = 0; i < 5; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                foreach(MapInfo cur in this.hierarchy_list[i].mapInfos_of_layer[j])
                {
                    ((SquareRoomMapInfo)cur).CreateStairs();
                }
            }
        }
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                foreach (MapInfo cur in this.hierarchy_list[i].mapInfos_of_layer[j])
                {
                    ((SquareRoomMapInfo)cur).InitTerrainInfoArr();
                }
            }
        }
    }
}

