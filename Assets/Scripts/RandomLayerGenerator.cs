using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


//hierarchy : 1 ~ n, floor : 0 ~ m, layer_idx : 0 ~ 4, map_idx : 0 ~ l
public class MapInfo
{
	public int hierarchy_idx;
	public int floor;     //´øÀü¿¡¼­ ÁøÂ¥ Ãþ
	public int layer_idx; //°èÃþ¿¡¼­ »ó´ëÀûÀÎ Ãþ
	public int map_idx;
	public List<MapInfo> connected_map_list;

	public MapInfo(int hierarchy_idx, int layer_idx, int map_idx)
	{
		this.hierarchy_idx = hierarchy_idx;
		this.layer_idx = layer_idx;
		this.floor = (hierarchy_idx - 1) * 5 + layer_idx;
		this.map_idx = map_idx;
		connected_map_list = new List<MapInfo>();
	}
}

class Hierarchy
{
	private int hierarchy_idx;
	private int total_map_num;
	//public int[] mapnum_of_layer = new int[5]; //Ãþ¿¡ ÀÖ´Â ¸Ê °³¼ö
	public List<MapInfo>[] mapInfos_of_layer = new List<MapInfo>[5]; //Ãþ¸¶´Ù ¸Ê Á¤º¸ ÀúÀå
	//public List<MapInfo> info = new List<MapInfo>();

	public Hierarchy(int hierarchy_idx, int difficulty)
	{
		this.hierarchy_idx = hierarchy_idx;
		for (int i = 0; i < 5; i++)
		{
			if (i == 0)
			{
				//mapnum_of_layer[i] = 1;
				mapInfos_of_layer[i].Add(new MapInfo(hierarchy_idx, 0, 0));
				total_map_num++;
			}
			else
			{
				//mapnum_of_layer[i] = i * hierarchy_idx + Random.Range(-i + 1, i + difficulty + 1);
				int mapnum = i * hierarchy_idx + Random.Range(-i + 1, i + difficulty + 1);
				for (int j = 0; j < mapnum; j++)
				{
					mapInfos_of_layer[i].Add(new MapInfo(hierarchy_idx, i, j));
					total_map_num++;
				}
			}
		}
		//0-0ÀÌ¶û 1-0 ¿¬°á
		mapInfos_of_layer[0][0].connected_map_list.Add(mapInfos_of_layer[1][0]);
		mapInfos_of_layer[1][0].connected_map_list.Add(mapInfos_of_layer[0][0]);
	}
	public void Create()
	{
		while (!ConnectionCheck())
		{
			int from_layer_idx = Random.Range(1, 5); // 0(0-0)Àº ¾È»ÌÀ½ ÀÌ¹Ì 1-1ÀÌ¶û ¿¬°á ÇßÀ¸´Ï±ñ
			int from_map_idx = Random.Range(0, mapInfos_of_layer[from_layer_idx].Count);
			MapInfo from = mapInfos_of_layer[from_layer_idx][from_map_idx];

			List<MapInfo> choose_list = new List<MapInfo>();

			int min_find_layer = from_layer_idx - 1;
			if (min_find_layer < 1) min_find_layer = 1;

			int max_find_layer = from_layer_idx + 1;
			if (max_find_layer > 4) max_find_layer = 4;

			for(int i = min_find_layer; i <= max_find_layer; i++)
            {
				foreach(MapInfo cur in mapInfos_of_layer[i])
                {
					if (cur != from) choose_list.Add(cur);
                }
            }

			foreach(MapInfo to in choose_list)
            {
                if (!from.connected_map_list.Contains(to))
                {
					from.connected_map_list.Add(to);
					to.connected_map_list.Add(from);
					break;
                }
            }
		}
	}

	public bool ConnectionCheck()
	{
		List<MapInfo> connected_list = new List<MapInfo>();
		connected_list.Add(mapInfos_of_layer[0][0]);

		Queue<MapInfo> queue = new Queue<MapInfo>();
		queue.Enqueue(mapInfos_of_layer[0][0]);

		while(queue.Count > 0)
        {
			MapInfo cur = queue.Dequeue();
			foreach (MapInfo m in cur.connected_map_list)
			{
				if (!connected_list.Contains(m))
				{
					connected_list.Add(m);
					queue.Enqueue(m);
				}
			}
        }
		if (connected_list.Count == total_map_num) return true;
		else return false;
	}

	public void Connect(Hierarchy hierarchy)
	{
		int map_idx = Random.Range(0, mapInfos_of_layer[4].Count);
		MapInfo from = mapInfos_of_layer[4][map_idx];

		from.connected_map_list.Add(hierarchy.mapInfos_of_layer[0][0]);
		hierarchy.mapInfos_of_layer[0][0].connected_map_list.Add(from);
	}
}


public class RandomLayerGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
