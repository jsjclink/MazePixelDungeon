using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyInfo
{
	private int hierarchy_idx;
	public List<MapInfo>[] mapInfos_of_layer = new List<MapInfo>[5]; //Ãþ¸¶´Ù ¸Ê Á¤º¸ ÀúÀå

	public HierarchyInfo(int hierarchy_idx, int difficulty)
	{
		this.hierarchy_idx = hierarchy_idx;
		for (int i = 0; i < 5; i++)
		{
			mapInfos_of_layer[i] = new List<MapInfo>();
			if (i == 0)
			{
				mapInfos_of_layer[i].Add(new SquareRoomMapInfo(hierarchy_idx, 0, 0, 70, 70));
			}
			else
			{
				int mapnum = i * (hierarchy_idx + 1) + Random.Range(-i + 1, i + difficulty + 1);
				if (mapnum > 5) mapnum = 5;
				for (int j = 0; j < mapnum; j++)
				{
					mapInfos_of_layer[i].Add(new SquareRoomMapInfo(hierarchy_idx, i, j, 70, 70));
				}
			}
		}

		ConnectMapInsideHieararchy();
	}
	public void ConnectMapInsideHieararchy()
	{
		//0-0ÀÌ¶û 1-0 ¿¬°á
		mapInfos_of_layer[0][0].connected_map_list.Add(mapInfos_of_layer[1][0]);
		mapInfos_of_layer[1][0].connected_map_list.Add(mapInfos_of_layer[0][0]);

		List<MapInfo> unconnected_list = new List<MapInfo>();

		while ((unconnected_list = FindUnConnected()).Count > 0)
		{
			int rand_idx = Random.Range(0, unconnected_list.Count);
			int from_layer_idx = unconnected_list[rand_idx].layer_idx;
			int from_map_idx = unconnected_list[rand_idx].map_idx;
			MapInfo from = mapInfos_of_layer[from_layer_idx][from_map_idx];

			List<MapInfo> choose_list = new List<MapInfo>();

			int min_find_layer = from_layer_idx - 1;
			if (min_find_layer < 1) min_find_layer = 1;

			int max_find_layer = from_layer_idx + 1;
			if (max_find_layer > 4) max_find_layer = 4;

			for (int i = min_find_layer; i <= max_find_layer; i++)
			{
				foreach (MapInfo cur in mapInfos_of_layer[i])
				{
					if (cur.id != from.id) choose_list.Add(cur);
				}
			}

			if (from.connected_map_list.Count == choose_list.Count) continue; //prevent infinite loop

			while (true)
			{
				int idx = Random.Range(0, choose_list.Count);
				MapInfo to = choose_list[idx];
				if (!from.connected_map_list.Contains(to))
				{
					from.connected_map_list.Add(to);
					to.connected_map_list.Add(from);
					break;
				}
			}
		}
	}

	public List<MapInfo> FindUnConnected()
	{
		Dictionary<string, bool> visited = new Dictionary<string, bool>();
		for (int i = 0; i < 5; i++)
		{
			foreach (MapInfo cur in mapInfos_of_layer[i])
			{
				visited[cur.id] = false;
			}
		}
		visited[mapInfos_of_layer[0][0].id] = true;

		Queue<MapInfo> queue = new Queue<MapInfo>();
		queue.Enqueue(mapInfos_of_layer[0][0]);

		while (queue.Count > 0)
		{
			MapInfo cur = queue.Dequeue();
			foreach (MapInfo m in cur.connected_map_list)
			{
				if (!visited[m.id])
				{
					visited[m.id] = true;
					queue.Enqueue(m);
				}
			}
		}

		List<MapInfo> unconnected_list = new List<MapInfo>();
		for (int i = 0; i < 5; i++)
		{
			foreach (MapInfo m in mapInfos_of_layer[i])
			{
				if (visited[m.id] == false) unconnected_list.Add(m);
			}
		}
		return unconnected_list;
	}

	public void Connect(HierarchyInfo hierarchy)
	{
		int map_idx = Random.Range(0, mapInfos_of_layer[4].Count);
		MapInfo from = mapInfos_of_layer[4][map_idx];

		from.connected_map_list.Add(hierarchy.mapInfos_of_layer[0][0]);
		hierarchy.mapInfos_of_layer[0][0].connected_map_list.Add(from);
	}
}
