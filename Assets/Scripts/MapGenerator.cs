using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class SpaceInfo
{
    public int start_x;
    public int start_y;
    public int end_x;
    public int end_y;

    public SpaceInfo(int s_x, int s_y, int e_x, int e_y)
    {
        start_x = s_x;
        start_y = s_y;
        end_x = e_x;
        end_y = e_y;
    }
}
class BridgeInfo
{
    public SpaceInfo from_space;
    public SpaceInfo to_space;
    
    public BridgeInfo(SpaceInfo from_space, SpaceInfo to_space)
    {
        this.from_space = from_space;
        this.to_space = to_space;
    }
}

class MapInfo
{
    public int[,] map;
    public int width; public int height;
    public List<SpaceInfo> space_list;
    public List<BridgeInfo> bridge_list;

    public MapInfo(List<SpaceInfo> space_list, int width, int height)
    {
        this.space_list = space_list;
        this.width = width;
        this.height = height;
        this.bridge_list = new List<BridgeInfo>();

        UpdateMap();
    }

    public void AddPaddingToMap(int top, int bottom, int left, int right)
    {
        foreach (SpaceInfo cur in space_list)
        {
            if (cur.start_x + left + 7 < cur.end_x) cur.start_x += left;
            if (cur.start_y + top + 7 < cur.end_y) cur.start_y += top;
            if (cur.end_x - right - 7 > cur.start_x) cur.end_x -= right;
            if (cur.end_y - bottom - 7 > cur.start_y) cur.end_y -= bottom;
        }

        UpdateMap();
    }

    public void UpdateMap()
    {
        map = new int[height, width];

        foreach (SpaceInfo cur in space_list)
        {
            Debug.Log("s_x : " + cur.start_x + " s_y : " + cur.start_y + " e_x : " + cur.end_x + " e_y : " + cur.end_y);
            for (int i = cur.start_y; i < cur.end_y; i++)
            {
                for (int j = cur.start_x; j < cur.end_x; j++)
                {
                    if (i == cur.start_y || j == cur.start_x || i == cur.end_y - 1 || j == cur.end_x - 1) map[i, j] = 0;
                    else map[i, j] = 1;
                }
            }
        }

        foreach(BridgeInfo cur in bridge_list)
        {
            int mid_x1 = (cur.from_space.start_x + cur.from_space.end_x) / 2;
            int mid_x2 = (cur.to_space.start_x + cur.to_space.end_x) / 2;

            int mid_y1 = (cur.from_space.start_y + cur.from_space.end_y) / 2;
            int mid_y2 = (cur.to_space.start_y + cur.to_space.end_y) / 2;
            Debug.Log("" + mid_x1 + ", " + mid_y1 + ", " + mid_x2 + ", " + mid_y2);

            if(mid_x1 < mid_x2)
            {
                for (int i = mid_x1; i <= mid_x2; i++) map[mid_y1, i] = 1;

                if (mid_y1 < mid_y2)
                {
                    for (int i = mid_y1; i < mid_y2; i++) map[i, mid_x2] = 1;
                }
                else
                {
                    for (int i = mid_y2; i < mid_y1; i++) map[i, mid_x2] = 1;
                }
            }
            else
            {
                for (int i = mid_x2; i <= mid_x1; i++) map[mid_y2, i] = 1;

                if (mid_y1 < mid_y2)
                {
                    for (int i = mid_y1; i < mid_y2; i++) map[i, mid_x1] = 1;
                }
                else
                {
                    for (int i = mid_y2; i < mid_y1; i++) map[i, mid_x1] = 1;
                }
            }
        }
    }

    public void RemoveRandomSpace()
    {
        space_list.RemoveAt(Random.Range(0, space_list.Count));

        UpdateMap();
    }

    public void CreateBridges()
    {
        List<SpaceInfo> unconnected_space_list = new List<SpaceInfo>(space_list);

        while(unconnected_space_list.Count > 1)
        {
            SpaceInfo space1 = unconnected_space_list[0];
            SpaceInfo space2 = unconnected_space_list[1];

            unconnected_space_list.RemoveAt(0);
            unconnected_space_list.RemoveAt(0);

            //space1, 2중에 하나 선택하는것도 랜덤
            unconnected_space_list.Add(space1);

            bridge_list.Add(new BridgeInfo(space1, space2));
        }

        UpdateMap();
    }

}

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    GameObject wall_prefab;
    [SerializeField]
    GameObject floor_prefab;

    void Start()
    {
        int width = 100; int height = 100;
        MapInfo mapInfo = CreateMap(width, height);
        //mapInfo.AddPaddingToMap(2, 2, 2, 2);

        
        for (int i = 0; i < mapInfo.space_list.Count - 10; i++)
        {
            mapInfo.RemoveRandomSpace();
        }

        mapInfo.CreateBridges();


        int[,] map = mapInfo.map;
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                if (map[i, j] == 0) Instantiate(wall_prefab, new Vector3(j + 0.5f, i + 0.5f, 0), Quaternion.identity);
                else Instantiate(floor_prefab, new Vector3(j + 0.5f, i + 0.5f, 0), Quaternion.identity);
            }
        }
    }

    MapInfo CreateMap(int width, int height)
    {
        List<SpaceInfo> space_list = DivideRect(new SpaceInfo(0, 0, width, height), true);

        return new MapInfo(space_list, width, height);
    }

    List<SpaceInfo> DivideRect(SpaceInfo cur, bool vertical)
    {
        if (cur.end_x - cur.start_x < 20 || cur.end_y - cur.start_y < 20) return new List<SpaceInfo>();

        List<SpaceInfo> arr1 = new List<SpaceInfo>();
        List<SpaceInfo> arr2 = new List<SpaceInfo>();
        List<SpaceInfo> ret_arr = new List<SpaceInfo>();

        if (vertical)
        {
            int weight1 = Random.Range(1, 3);
            int weight2 = Random.Range(1, 3);
            int mid = (cur.start_x * weight1 + cur.end_x * weight2) / (weight1 + weight2);

            bool divide_vertical;
            if (mid - cur.start_x > cur.end_y - cur.start_y) divide_vertical = true;
            else divide_vertical = false;
            arr1 = DivideRect(new SpaceInfo(cur.start_x, cur.start_y, mid, cur.end_y), divide_vertical);
            if(arr1.Count == 0)
            {
                SpaceInfo new_space = new SpaceInfo(cur.start_x, cur.start_y, mid, cur.end_y);
                ret_arr.Add(new_space);
            }
            while(arr1.Count > 0)
            {
                ret_arr.Add(arr1[0]);
                arr1.RemoveAt(0);
            }
            if (cur.end_x - mid > cur.end_y - cur.start_y) divide_vertical = true;
            else divide_vertical = false;
            arr2 = DivideRect(new SpaceInfo(mid, cur.start_y, cur.end_x, cur.end_y), divide_vertical);
            if(arr2.Count == 0)
            {
                SpaceInfo new_space = new SpaceInfo(mid, cur.start_y, cur.end_x, cur.end_y);
                ret_arr.Add(new_space);
            }
            while(arr2.Count > 0)
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
            arr1 = DivideRect(new SpaceInfo(cur.start_x, cur.start_y, cur.end_x, mid), divide_vertical);
            if(arr1.Count == 0)
            {
                SpaceInfo new_space = new SpaceInfo(cur.start_x, cur.start_y, cur.end_x, mid);
                ret_arr.Add(new_space);
            }
            while(arr1.Count > 0)
            {
                ret_arr.Add(arr1[0]);
                arr1.RemoveAt(0);
            }
            if (cur.end_x - cur.start_x > cur.end_y - mid) divide_vertical = true;
            else divide_vertical = false;
            arr2 = DivideRect(new SpaceInfo(cur.start_x, mid, cur.end_x, cur.end_y), divide_vertical);
            if(arr2.Count == 0)
            {
                SpaceInfo new_space = new SpaceInfo(cur.start_x, mid, cur.end_x, cur.end_y);
                ret_arr.Add(new_space);
            }
            while(arr2.Count > 0)
            {
                ret_arr.Add(arr2[0]);
                arr2.RemoveAt(0);
            }
        }
        return ret_arr;
    }
 }
