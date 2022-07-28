using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;


public enum TOUCH_INPUT_TYPE
{
    NONE, MOVE_INTERRUPT, PLAYER_MOVE_TO_POS, PLAYER_ENTER_STAIR, PLAYER_ATTACK_ENEMY
}
public enum TURN_TYPE
{
    ENEMY_TURN, PLAYER_TURN
}

[Serializable]
public class SaveData
{
    [Serializable]
    public class EnemySerializedList
    {
        public List<EnemyInfo> enemy_serialized_list;
        public EnemySerializedList()
        {
            enemy_serialized_list = new List<EnemyInfo>();
        }
    }
    [Serializable]
    public class ItemSerializedList
    {
        public List<ItemInfo> item_serialized_list;
        public ItemSerializedList()
        {
            item_serialized_list = new List<ItemInfo>();
        }
    }
    [Serializable]
    public class SeedInfo
    {
        public int seed;
        public SeedInfo(int seed)
        {
            this.seed = seed;
        }
    }

    public PlayerInfo player_info;
    public EnemySerializedList enemySerialized;
    public ItemSerializedList itemSerialized;
    public SeedInfo seedInfo;

    public SaveData(Dungeon dungeon, int seed, PlayerInfo player_info)
    {
        this.enemySerialized = new EnemySerializedList();
        this.itemSerialized = new ItemSerializedList();
        this.seedInfo = new SeedInfo(seed);
        this.player_info = player_info;

        foreach (HierarchyInfo hierarchy in dungeon.hierarchy_list)
        {
            foreach(List<MapInfo> map_linfo_list in hierarchy.mapInfos_of_layer)
            {
                foreach(MapInfo map_info in map_linfo_list)
                {
                    foreach(EnemyInfo enemy in map_info.enemy_list)
                    {
                        this.enemySerialized.enemy_serialized_list.Add(enemy);
                    }
                    foreach(ItemInfo item in map_info.item_list)
                    {
                        this.itemSerialized.item_serialized_list.Add(item);
                    }
                }
            }
        }
    }

}
public class TouchInfo
{
    public TOUCH_INPUT_TYPE type;
    public Vector3 move_to_pos;
    public MapInfo from;
    public MapInfo to;
    public EnemyInfo enemy;

    public TouchInfo(TOUCH_INPUT_TYPE type) //NONE, MOVE_INTERRUPT
    {
        this.type = type;
    }
    public TouchInfo(TOUCH_INPUT_TYPE type, MapInfo from, MapInfo to) // PLAYER_ENTER_STAIR
    {
        this.type = type;
        this.from = from;
        this.to = to;
    }
    public TouchInfo(TOUCH_INPUT_TYPE type, Vector3 touch_pos) // PLAYER_MOVE_TO_POS
    {
        this.type = type;
        this.move_to_pos = touch_pos;
    }
    public TouchInfo(TOUCH_INPUT_TYPE type, EnemyInfo enemy) // ATTACK_ENEMY
    {
        this.type = type;
        this.enemy = enemy;
    }
}

public class TurnInfo
{
    public TURN_TYPE turn_type;
    public UnitInfo unit;

    public TurnInfo(TURN_TYPE turn_type, UnitInfo unit)
    {
        this.turn_type = turn_type;
        this.unit = unit;
    }
}

public class GameSystemManager : MonoBehaviour
{
    [SerializeField]
    GameObject empty_prefab;
    [SerializeField]
    GameObject floor_prefab;
    [SerializeField]
    GameObject wall_prefab;
    [SerializeField]
    GameObject wall_left_prefab;
    [SerializeField]
    GameObject wall_right_prefab;
    [SerializeField]
    GameObject bridge_prefab;
    [SerializeField]
    GameObject door_prefab;
    [SerializeField]
    GameObject stair_prefab;
    [SerializeField]
    GameObject shadow_prefab;

    [SerializeField]
    GameObject enemy_rat_prefab;
    [SerializeField]
    GameObject enemy_gnoll_prefab;
    [SerializeField]
    GameObject enemy_crab_prefab;
    [SerializeField]
    GameObject boss_01_prefab;
    [SerializeField]
    GameObject sheep_prefab;

    [SerializeField]
    GameObject sword_01_prefab;
    [SerializeField]
    GameObject ax_01_prefab;
    [SerializeField]
    GameObject armor_01_prefab;
    [SerializeField]
    GameObject armor_02_prefab;
    [SerializeField]
    GameObject artifact_01_prefab;
    [SerializeField]
    GameObject gold_prefab;
    [SerializeField]
    GameObject ring_01_prefab;
    [SerializeField]
    GameObject ring_02_prefab;
    [SerializeField]
    GameObject food_01_prefab;
    [SerializeField]
    GameObject potion_hp_prefab;
    [SerializeField]
    GameObject scroll_boss_prefab;
    [SerializeField]
    GameObject scroll_sheep_prefab;

    [SerializeField]
    public GameObject HP_bar;
    [SerializeField]
    public TMP_Text Layer;
    [SerializeField]
    GameObject player_object;

    [SerializeField]
    Camera cam;

    float touch_began_time;
    float prev_action_time;

    public Dungeon dungeon;
    public PlayerInfo player_info;

    public MapInfo cur_map_info;
    public TerrainInfo[,] terrain_info_arr;
    GameObject[,] terrain_object_arr;
    GameObject[,] shadow_object_arr;
    public List<EnemyInfo> enemy_list;
    public Dictionary<EnemyInfo, GameObject> enemy_object_dict;
    public List<ItemInfo> item_list;
    public Dictionary<ItemInfo, GameObject> item_object_dict;
    public List<(GameObject, int)> sheep_object_list;

    Queue<TurnInfo> turn_queue;

    int game_idx;
    bool game_loaded;
    int seed;
    bool turn_passed;

    void Start()
    {
        //이거는 나중에 바꿔주자
        sheep_object_list = new List<(GameObject, int)>();

        turn_queue = new Queue<TurnInfo>();
        prev_action_time = Time.time;
        turn_passed = false;

        Application.targetFrameRate = 30; // 배터리 최적화

        //앞에 씬에서 받아옴
        //game_idx = 1;
        //game_loaded = false;

        game_loaded = SendingInfo.is_loaded;
        if(game_loaded) game_idx = int.Parse(SendingInfo.name.Split('_')[1]);
        else
        {
            int max = 0;
            DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath + "/");
            foreach (FileInfo File in di.GetFiles())
            {
                Debug.Log(File.Name);
                int file_idx = int.Parse(File.Name.Split('.')[0].Split('_')[1]);
                if (max < file_idx) max = file_idx;
            }
            game_idx = max + 1;
            Debug.Log("GAMEIDX : " + game_idx);
        }
        //InitGame에서 해줌
        //terrain_info_arr = new TerrainInfo[0, 0];
        //terrain_object_arr = new GameObject[0, 0];
        //shadow_object_arr = new GameObject[0, 0];
        //enemy_list = new List<EnemyInfo>();
        //enemy_object_dict = new Dictionary<EnemyInfo, GameObject>();
        InitGame(game_loaded, game_idx);
    }

    private void Update()
    {
        //get input
        TouchInfo touch_info = GetTouch();

        //update info - by touch_info
        switch (touch_info.type)
        {
            case TOUCH_INPUT_TYPE.NONE:
                break;
            case TOUCH_INPUT_TYPE.MOVE_INTERRUPT:
                this.player_info.cur_path.Clear();
                this.player_info.SetState(UNIT_STATE.IDLE);
                break;
            case TOUCH_INPUT_TYPE.PLAYER_MOVE_TO_POS:
                this.player_info.SetPathTo((int)touch_info.move_to_pos.x, (int)touch_info.move_to_pos.y, this.terrain_info_arr, enemy_list);
                if(this.player_info.cur_path.Count != 0)
                {
                    this.player_info.SetState(UNIT_STATE.MOVING);
                    if (turn_queue.Count == 0) turn_queue.Enqueue(new TurnInfo(TURN_TYPE.PLAYER_TURN, this.player_info));
                }
                else
                {
                    this.player_info.SetState(UNIT_STATE.IDLE);
                }   
                break;
            case TOUCH_INPUT_TYPE.PLAYER_ENTER_STAIR:
                ChangeMap(touch_info.from, touch_info.to);
                this.player_info.SetState(UNIT_STATE.IDLE);
                if (turn_queue.Count == 0) turn_queue.Enqueue(new TurnInfo(TURN_TYPE.PLAYER_TURN, this.player_info));
                else { turn_queue = new Queue<TurnInfo>(); turn_queue.Enqueue(new TurnInfo(TURN_TYPE.PLAYER_TURN, this.player_info)); }
                break;
            case TOUCH_INPUT_TYPE.PLAYER_ATTACK_ENEMY:
                this.player_info.SetState(UNIT_STATE.ENGAGING, touch_info.enemy);
                if (turn_queue.Count == 0) turn_queue.Enqueue(new TurnInfo(TURN_TYPE.PLAYER_TURN, this.player_info));
                break;
        }

        //update info - every 0.1f sec (by turn)
        if(turn_queue.Count > 0)
        {
            float action_interval = Time.time - prev_action_time;
            if (action_interval > 0.15f)
            {
                List<EnemyInfo> turn_used_enemy_list = new List<EnemyInfo>();
                //ENGAGE
                TurnInfo turn_info = turn_queue.Dequeue();
                switch (turn_info.turn_type)
                {
                    case TURN_TYPE.PLAYER_TURN:
                        Debug.Log("turn_queue.Count : " + turn_queue.Count);
                        if (this.player_info.unit_state == UNIT_STATE.IDLE || this.player_info.unit_state == UNIT_STATE.SLEEPING) break;
                        switch (this.player_info.unit_state)
                        {
                            case UNIT_STATE.IDLE:
                                this.player_info.animation_state = ANIMATION_STATE.IDLE;
                                break;
                            case UNIT_STATE.MOVING:
                                Debug.Log("cur_path.count : " + this.player_info.cur_path.Count);
                                if(this.player_info.cur_path.Count > 0)
                                {
                                    if (this.player_info.DetectObstacle(this.player_info.cur_path[0].Item1, this.player_info.cur_path[0].Item2, enemy_list))
                                    {
                                        this.player_info.cur_path.Clear();
                                    }
                                    else
                                    {
                                        this.player_info.SetPos(this.player_info.cur_path[0].Item1, this.player_info.cur_path[0].Item2);
                                        this.player_info.cur_path.RemoveAt(0);
                                    }
                                }
                                if(this.player_info.cur_path.Count == 0) this.player_info.SetState(UNIT_STATE.IDLE);
                                this.player_info.animation_state = ANIMATION_STATE.MOVING;
                                break;
                            case UNIT_STATE.ENGAGING:
                                Debug.Log("ATTACKED ENEMY");
                                int item_attack_pt = 0;
                                foreach(ItemInfo item in player_info.equip_list)
                                {
                                    if (item.item_type == ITEM_TYPE.WEAPON)
                                    {
                                        switch (item.item_name)
                                        {
                                            case ITEM_NAME.SWORD_01:
                                                item_attack_pt = 5;
                                                break;
                                            case ITEM_NAME.AX_01:
                                                item_attack_pt = 7;
                                                break;
                                        }
                                    }
                                }
                                this.player_info.engaging_unit.hp -= (this.player_info.attack_pt + item_attack_pt);
                                if (this.player_info.engaging_unit.hp <= 0)
                                {
                                    Destroy(enemy_object_dict[this.player_info.engaging_unit]);
                                    enemy_list.Remove(this.player_info.engaging_unit);
                                }
                                this.player_info.SetState(UNIT_STATE.IDLE);
                                this.player_info.animation_state = ANIMATION_STATE.ENGAGING;
                                break;
                            case UNIT_STATE.SLEEPING:
                                break;
                        }
                        foreach (EnemyInfo enemy in enemy_list)
                        {
                            bool contains = false;
                            foreach (UnitInfo e in turn_used_enemy_list) if (e == enemy) contains = true;
                            if (contains) continue;
                            int dx = this.player_info.pos_x - (int)enemy.pos_x;
                            int dy = this.player_info.pos_y - (int)enemy.pos_y;
                            if ((dx == -1 || dx == 0 || dx == 1) && (dy == -1 || dy == 0 || dy == 1))
                            {
                                turn_queue.Enqueue(new TurnInfo(TURN_TYPE.ENEMY_TURN, enemy));
                                turn_used_enemy_list.Add(enemy);
                            }
                        }
                        if (this.player_info.unit_state != UNIT_STATE.IDLE && this.player_info.unit_state != UNIT_STATE.SLEEPING)
                        {
                            turn_queue.Enqueue(new TurnInfo(TURN_TYPE.PLAYER_TURN, this.player_info));
                        }
                        UpdateShadow();
                        string output = "turn_queue : ";
                        foreach (TurnInfo cur in turn_queue)
                        {
                            output += cur.turn_type + ",";
                        }
                        Debug.Log(output);
                        SaveData();
                        break;
                    case TURN_TYPE.ENEMY_TURN:
                        ((EnemyInfo)turn_info.unit).animation_state = ANIMATION_STATE.ENGAGING;
                        int armor = 0;
                        foreach(ItemInfo item in this.player_info.equip_list)
                        {
                            if(item.item_type == ITEM_TYPE.ARMOR)
                            {
                                switch (item.item_name)
                                {
                                    case ITEM_NAME.ARMOR_01:
                                        armor = 5;
                                        break;
                                    case ITEM_NAME.ARMOR_02:
                                        armor = 10;
                                        break;
                                }
                            }
                        }
                        this.player_info.cur_hp -= (int)(((EnemyInfo)turn_info.unit).attack_pt * ((20.0f - (float)armor)/20.0f));
                        this.HP_bar.GetComponent<Slider>().value -= (int)(((EnemyInfo)turn_info.unit).attack_pt * ((20.0f - (float)armor) / 20.0f));
                        if(this.player_info.cur_hp <= 0)
                        {
                            GameObject.Find("Canvas").transform.Find("GameOverPopUp").gameObject.SetActive(true);
                        }
                        break;
                }
                Debug.Log("playerInfo.hp" + this.player_info.cur_hp);
                //attack을 queue에 넣은 애들 말고는 움직여야함
                if (true || turn_queue.Count == 0 || turn_queue.Peek().turn_type == TURN_TYPE.PLAYER_TURN)
                {
                    foreach (EnemyInfo enemy in enemy_list)
                    {
                        if (enemy.DetectPlayer(this.player_info, this.terrain_info_arr))
                        {
                            enemy.SetPathTo(this.player_info.pos_x, this.player_info.pos_y, this.terrain_info_arr, enemy_list, this.player_info);
                            if (enemy.cur_path.Count > 0)
                            {
                                if (enemy.DetectObstacle(enemy.cur_path[0].Item1, enemy.cur_path[0].Item2, enemy_list, this.player_info))
                                {
                                    enemy.SetPathTo(this.player_info.pos_x, this.player_info.pos_y, this.terrain_info_arr, enemy_list, this.player_info);
                                }
                                else
                                {
                                    enemy.SetPos(enemy.cur_path[0].Item1, enemy.cur_path[0].Item2);
                                    enemy.cur_path.RemoveAt(0);
                                }
                            }
                        }
                        else
                        {
                            if (enemy.cur_path.Count > 0)
                            {
                                if (enemy.DetectObstacle(enemy.cur_path[0].Item1, enemy.cur_path[0].Item2, enemy_list, this.player_info))
                                {
                                    enemy.SetPathTo(enemy.cur_path[enemy.cur_path.Count - 1].Item1, enemy.cur_path[enemy.cur_path.Count - 1].Item2, this.terrain_info_arr, enemy_list, this.player_info);
                                }
                                else
                                {
                                    enemy.SetPos(enemy.cur_path[0].Item1, enemy.cur_path[0].Item2);
                                    enemy.cur_path.RemoveAt(0);
                                }
                            }
                            else
                            {
                                List<SquareSpaceInfo> space_list = ((SquareRoomMapInfo)this.cur_map_info).space_list;
                                int idx = UnityEngine.Random.Range(0, space_list.Count);
                                int move_x = UnityEngine.Random.Range(space_list[idx].start_x + 1, space_list[idx].end_x - 1);
                                int move_y = UnityEngine.Random.Range(space_list[idx].start_y + 1, space_list[idx].end_y - 1);
                                enemy.SetPathTo(move_x, move_y, this.terrain_info_arr, enemy_list, this.player_info);
                            }
                        }
                    }
                }
                prev_action_time = Time.time;
                turn_passed = true;
                this.player_info.hunger--;
                int artifact_01_cnt = 0;
                foreach (ItemInfo item in this.player_info.equip_list)
                {
                    if (item.item_name == ITEM_NAME.ARTIFACT_01) artifact_01_cnt++;
                }
                if (this.player_info.cur_hp + artifact_01_cnt > 100) this.player_info.cur_hp = 100;
                else
                {
                    this.player_info.cur_hp += artifact_01_cnt;
                    this.HP_bar.GetComponent<Slider>().value += artifact_01_cnt;
                }
                if (this.player_info.hunger < 0)
                {
                    this.player_info.cur_hp -= 2;
                    this.HP_bar.GetComponent<Slider>().value -= 2;
                    if (this.player_info.cur_hp <= 0)
                    {
                        GameObject.Find("Canvas").transform.Find("GameOverPopUp").gameObject.SetActive(true);
                    }
                }
                Debug.Log("Player_info : " + this.player_info.hunger);
            }
        }

        if (turn_passed)
        {
            Debug.Log("turn_passed");
            //sheep list
            List<(GameObject, int)> tmp_list = new List<(GameObject, int)>();
            foreach((GameObject, int) sheep_info in sheep_object_list)
            {
                if (sheep_info.Item2 < 0) Destroy(sheep_info.Item1);
                else
                {
                    tmp_list.Add((sheep_info.Item1, sheep_info.Item2 - 1));
                }                
            }
            sheep_object_list = tmp_list;

            //animation set-player
            switch (this.player_info.animation_state)
            {
                case ANIMATION_STATE.IDLE:
                    {
                        this.player_object.GetComponent<Animator>().SetInteger("unit_state", 0);
                        break;
                    }   
                case ANIMATION_STATE.MOVING:
                    {
                        int dx = this.player_info.pos_x - (int)this.player_object.transform.position.x;
                        if (dx > 0) this.player_object.GetComponent<SpriteRenderer>().flipX = false;
                        else this.player_object.GetComponent<SpriteRenderer>().flipX = true;
                        this.player_object.GetComponent<Animator>().SetInteger("unit_state", 1);
                        StartCoroutine(ResetAnimation(this.player_object.GetComponent<Animator>()));
                        break;
                    }                
                case ANIMATION_STATE.ENGAGING:
                    {
                        int dx = this.player_info.engaging_unit.pos_x - this.player_info.pos_x;
                        if (dx > 0) this.player_object.GetComponent<SpriteRenderer>().flipX = false;
                        else this.player_object.GetComponent<SpriteRenderer>().flipX = true;
                        this.player_object.GetComponent<Animator>().SetInteger("unit_state", 2);
                        StartCoroutine(ResetAnimation(this.player_object.GetComponent<Animator>()));
                        break;
                    }       
                case ANIMATION_STATE.SLEEPING:
                    break;
            }
            this.player_info.animation_state = ANIMATION_STATE.IDLE;
            //animation set-player
            foreach(EnemyInfo enemy in this.enemy_list)
            {
                switch (enemy.animation_state)
                {
                    case ANIMATION_STATE.IDLE:
                        {
                            int dx = enemy.pos_x - (int)enemy_object_dict[enemy].transform.position.x;
                            if (dx > 0) enemy_object_dict[enemy].GetComponent<SpriteRenderer>().flipX = false;
                            else enemy_object_dict[enemy].GetComponent<SpriteRenderer>().flipX = true;
                            //enemy_object_dict[enemy].GetComponent<Animator>().SetInteger("unit_state", 0);
                            break;
                        }
                    case ANIMATION_STATE.ENGAGING:
                        {
                            int dx = this.player_info.pos_x - enemy.pos_x;
                            if (dx > 0) enemy_object_dict[enemy].GetComponent<SpriteRenderer>().flipX = false;
                            else enemy_object_dict[enemy].GetComponent<SpriteRenderer>().flipX = true;
                            enemy_object_dict[enemy].GetComponent<Animator>().SetInteger("unit_state", 2);
                            StartCoroutine(ResetAnimation(enemy_object_dict[enemy].GetComponent<Animator>()));
                            break;
                        }  
                }
            }


            //draw
            if (player_object.transform.position.x != this.player_info.pos_x || player_object.transform.position.y != this.player_info.pos_y)
            {
                player_object.transform.position = new Vector3(this.player_info.pos_x, this.player_info.pos_y, 0);
                cam.transform.position = new Vector3(this.player_info.pos_x, this.player_info.pos_y, -10);
            }
            foreach (EnemyInfo enemy in enemy_list)
            {
                if (this.terrain_info_arr[enemy.pos_y, enemy.pos_x].in_player_sight)
                {
                    this.enemy_object_dict[enemy].SetActive(true);
                    enemy_object_dict[enemy].transform.position = new Vector3(enemy.pos_x, enemy.pos_y, 0);
                }
                else
                {
                    this.enemy_object_dict[enemy].SetActive(false);
                }
            }

            for (int i = 0; i < this.terrain_object_arr.GetLength(0); i++)
            {
                for (int j = 0; j < this.terrain_object_arr.GetLength(1); j++)
                {
                    if (terrain_info_arr[i, j].in_player_sight)
                    {
                        Color color = this.shadow_object_arr[i, j].GetComponent<SpriteRenderer>().color;
                        this.shadow_object_arr[i, j].GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 0.0f);
                    }
                    else if (this.terrain_info_arr[i, j].visited)
                    {
                        Color color = this.shadow_object_arr[i, j].GetComponent<SpriteRenderer>().color;
                        this.shadow_object_arr[i, j].GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 0.5f);
                    }
                }
            }

            turn_passed = false;
        }
        
    }

    public void ChangeMap(MapInfo from, MapInfo to)
    {
        //이전 object 삭제
        for(int i = 0; i < this.terrain_object_arr.GetLength(0); i++)
        {
            for(int j = 0; j < this.terrain_object_arr.GetLength(1); j++)
            {
                Destroy(this.terrain_object_arr[i, j]);
            }
        }
        for (int i = 0; i < this.shadow_object_arr.GetLength(0); i++)
        {
            for (int j = 0; j < this.shadow_object_arr.GetLength(1); j++)
            {
                Destroy(this.shadow_object_arr[i, j]);
            }
        }
        foreach (EnemyInfo cur in this.enemy_list)
        {
            Destroy(this.enemy_object_dict[cur]);
        }
        foreach (ItemInfo cur in this.item_list)
        {
            Destroy(this.item_object_dict[cur]);
        }

        //새로운 정보 init
        this.cur_map_info = to;
        this.terrain_info_arr = this.cur_map_info.terrain_info_arr;
        this.enemy_list = this.cur_map_info.enemy_list;
        this.item_list = this.cur_map_info.item_list;

        this.terrain_object_arr = new GameObject[this.terrain_info_arr.GetLength(0), this.terrain_info_arr.GetLength(1)];
        this.shadow_object_arr = new GameObject[this.terrain_info_arr.GetLength(0), this.terrain_info_arr.GetLength(1)];
        this.enemy_object_dict = new Dictionary<EnemyInfo, GameObject>();
        this.item_object_dict = new Dictionary<ItemInfo, GameObject>();


        //player 위치 to맵으로
        this.player_info.hierarchy_idx = to.hierarchy_idx;
        this.player_info.layer_idx = to.layer_idx;
        this.player_info.map_idx = to.map_idx;

        //이동한 stair 위치에 player위치
        StairInfo target_stair = null;
        foreach (StairInfo cur in this.cur_map_info.stair_list)
        {
            if (cur.connected_map.id == from.id) target_stair = cur;
        }
        if (target_stair != null)
        {
            this.player_info.SetPos(target_stair.pos_x, target_stair.pos_y);
            this.player_object.transform.position = new Vector3(this.player_info.pos_x, this.player_info.pos_y, 0);
        }
        else //일어나면 안되는 일!
        {
            Debug.Log("Fatal error : stair find failure");
            List<SquareSpaceInfo> space_list = ((SquareRoomMapInfo)this.cur_map_info).space_list;
            int idx = UnityEngine.Random.Range(0, space_list.Count);
            int spawn_x = UnityEngine.Random.Range(space_list[idx].start_x + 1, space_list[idx].end_x - 1);
            int spawn_y = UnityEngine.Random.Range(space_list[idx].start_y + 1, space_list[idx].end_y - 1);
            this.player_info.SetPos(spawn_x, spawn_y);
            this.player_object.transform.position = new Vector3(spawn_x, spawn_y, 0);
        }

        //terrain-shadow init     
        for (int i = 0; i < this.terrain_info_arr.GetLength(0); i++)
        {
            for (int j = 0; j < this.terrain_info_arr.GetLength(1); j++)
            {
                int dx = player_info.pos_x - j;
                int dy = player_info.pos_y - i;
                if (this.player_info.pos_x == j && this.player_info.pos_y == i)
                {
                    terrain_info_arr[i, j].visited = true;
                    terrain_info_arr[i, j].in_player_sight = true;
                }
                else if (dx * dx + dy * dy < 20)
                {
                    if (player_info.PlayerSight(j, i, terrain_info_arr))
                    {
                        terrain_info_arr[i, j].visited = true;
                        terrain_info_arr[i, j].in_player_sight = true;
                    }
                    else
                    {
                        terrain_info_arr[i, j].in_player_sight = false;
                    }
                }
            }
        }

        //draw terrain
        for (int i = 0; i < this.terrain_info_arr.GetLength(0); i++)
        {
            for (int j = 0; j < this.terrain_info_arr.GetLength(1); j++)
            {
                if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.EMPTY)
                {
                    this.terrain_object_arr[i, j] = Instantiate(empty_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }
                else if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.FLOOR)
                {
                    this.terrain_object_arr[i, j] = Instantiate(floor_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }

                else if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.WALL)
                {
                    this.terrain_object_arr[i, j] = Instantiate(wall_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }
                else if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.BRIDGE)
                {
                    this.terrain_object_arr[i, j] = Instantiate(bridge_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }
                else if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.DOOR)
                {
                    this.terrain_object_arr[i, j] = Instantiate(door_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }
                else if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.STAIR)
                {
                    this.terrain_object_arr[i, j] = Instantiate(stair_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }
            }
        }

        //draw enemies
        foreach (EnemyInfo cur in this.enemy_list)
        {
            Debug.Log(cur.unit_type);
            switch (cur.unit_type)
            {
                case UNIT_TYPE.ENEMY_RAT:
                    enemy_object_dict[cur] = Instantiate(enemy_rat_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case UNIT_TYPE.ENEMY_GNOLL:
                    enemy_object_dict[cur] = Instantiate(enemy_gnoll_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case UNIT_TYPE.ENEMY_CRAB:
                    enemy_object_dict[cur] = Instantiate(enemy_crab_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case UNIT_TYPE.ENEMY_BOOS_01:
                    enemy_object_dict[cur] = Instantiate(boss_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
            }
        }

        //create items
        foreach (ItemInfo cur in this.item_list)
        {
            switch (cur.item_name)
            {
                case ITEM_NAME.SWORD_01:
                    item_object_dict[cur] = Instantiate(sword_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.AX_01:
                    item_object_dict[cur] = Instantiate(ax_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.ARMOR_01:
                    item_object_dict[cur] = Instantiate(armor_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.ARMOR_02:
                    item_object_dict[cur] = Instantiate(armor_02_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.ARTIFACT_01:
                    item_object_dict[cur] = Instantiate(artifact_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.FOOD_01:
                    item_object_dict[cur] = Instantiate(food_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.POTION_HP:
                    item_object_dict[cur] = Instantiate(potion_hp_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.RING_01:
                    item_object_dict[cur] = Instantiate(ring_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.RING_02:
                    item_object_dict[cur] = Instantiate(ring_02_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.SCROLL_BOSS:
                    item_object_dict[cur] = Instantiate(scroll_boss_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.SCROLL_SHEEP:
                    item_object_dict[cur] = Instantiate(scroll_sheep_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
            }
        }

        //create shadows
        for (int i = 0; i < this.terrain_object_arr.GetLength(0); i++)
        {
            for (int j = 0; j < this.terrain_object_arr.GetLength(1); j++)
            {
                GameObject shadow = Instantiate(shadow_prefab, new Vector3(j, i, 0), Quaternion.identity);
                if (terrain_info_arr[i, j].in_player_sight == true)
                {
                    Color color = shadow.GetComponent<SpriteRenderer>().color;
                    shadow.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 0);
                }
                this.shadow_object_arr[i, j] = shadow;
            }
        }

        //camera
        cam.transform.position = new Vector3(this.player_info.pos_x, this.player_info.pos_y, -10);

        //ui update
        Layer.text = (this.player_info.hierarchy_idx*5 + this.player_info.layer_idx) + "-" + this.player_info.map_idx;
    }
    private TouchInfo GetTouch()
    {
        TouchInfo touch_info = new TouchInfo(TOUCH_INPUT_TYPE.NONE);

        //만약 ui를 터치 했을 때
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return touch_info;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touch_began_time = Time.time;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                float touch_interval = Time.time - touch_began_time;
                if (touch_interval >= 0.2f) return new TouchInfo(TOUCH_INPUT_TYPE.NONE); // touch interval 짧으면 NONE return함

                if (this.player_info.cur_path.Count > 0) return new TouchInfo(TOUCH_INPUT_TYPE.MOVE_INTERRUPT); //이동중에 누르면 이동 취소
                else                                                                                      //이동중이 아닐 때 누르면(멈춰있을 때)
                {
                    Vector3 touch_pos = Camera.main.ScreenToWorldPoint(touch.position);
                    touch_pos = new Vector3(touch_pos.x + 0.3f, touch_pos.y + 0.3f, 0); //보정치 0.3f 더해주면 좀 더 터치가 정확해짐
                    Debug.Log("touch_pos.x + 0.3f, touch_pos.y + 0.3f :" + touch_pos.x + ", " + touch_pos.y);

                    if (this.player_info.pos_x == (int)touch_pos.x && this.player_info.pos_y == (int)touch_pos.y) //나를 눌렀을 때
                    {
                        if(FindItemAt((int)touch_pos.x, (int)touch_pos.y) != null)
                        {
                            ItemInfo item = FindItemAt((int)touch_pos.x, (int)touch_pos.y);
                            this.player_info.item_list.Add(item);
                            GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").GetComponent<Inventory>().FreeSlot();
                            GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").GetComponent<Inventory>().items.Add(item);
                            GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").GetComponent<Inventory>().FreeSlot();
                            Destroy(item_object_dict[item]);
                            this.item_list.Remove(item);
                            this.cur_map_info.item_list = this.item_list;
                            Debug.Log("ITEM : " + this.player_info.item_list.Count);
                        }
                        else if (terrain_info_arr[(int)touch_pos.y, (int)touch_pos.x].terrain_type == TERRAIN_TYPE.STAIR)   //그 위치가 계단과 같을 때
                        {
                            foreach (StairInfo cur in this.cur_map_info.stair_list)
                            {
                                if (cur.pos_x == (int)touch_pos.x && cur.pos_y == (int)touch_pos.y) // 해당하는 stair를 찾음
                                {
                                    return new TouchInfo(TOUCH_INPUT_TYPE.PLAYER_ENTER_STAIR, this.cur_map_info, cur.connected_map);
                                }
                            }
                        }
                    }
                    else                                                                               //나를 누르지 않았을 때
                    {
                        int dx = this.player_info.pos_x - (int)touch_pos.x;
                        int dy = this.player_info.pos_y - (int)touch_pos.y;
                        if ((dx == -1 || dx == 0 || dx == 1) && (dy == -1 || dy == 0 || dy == 1))       //내 주변을 눌렀는데
                        {
                            foreach (EnemyInfo enemy in enemy_list)
                            {
                                if (enemy.pos_x == (int)touch_pos.x && enemy.pos_y == (int)touch_pos.y) //그게 enemy면
                                {
                                    Debug.Log("ENEMY CLICKED in dist 1");
                                    return new TouchInfo(TOUCH_INPUT_TYPE.PLAYER_ATTACK_ENEMY, enemy);  //attack enemy
                                }
                            }
                        }

                        return new TouchInfo(TOUCH_INPUT_TYPE.PLAYER_MOVE_TO_POS, touch_pos);
                    }
                }
            }
        }
        return touch_info;
    }

    public void SaveData()
    {
        //현재 enemy 위치를 정확하게 업데이트 해주고 나서 저장!
        this.cur_map_info.enemy_list = this.enemy_list;
        SaveData data = new SaveData(this.dungeon, this.seed, this.player_info);
        string json_string = JsonUtility.ToJson(data);

        File.WriteAllText(Path.Combine(Application.persistentDataPath, "dungeondata_" + this.game_idx + ".Json"), json_string);
    }

    public void InitGame(bool game_loaded,int game_idx)
    {
        //정보 init
        if (game_loaded)
        {
            string load_data = File.ReadAllText(Path.Combine(Application.persistentDataPath, "dungeondata_" + game_idx + ".Json"));
            SaveData data = JsonUtility.FromJson<SaveData>(load_data);
            SaveData.EnemySerializedList enemySerialized = data.enemySerialized;
            SaveData.ItemSerializedList itemSerialized = data.itemSerialized;
            SaveData.SeedInfo seedInfo = data.seedInfo;
            this.seed = seedInfo.seed;
            UnityEngine.Random.InitState(seedInfo.seed);
            Dungeon load_dungeon = new Dungeon(3);
            //load data 했을 때 생성된 object들은 heap에 new로 할당된게 아니라서 없어지는것 같음
            //그래서 단순 할당 하는게 아니라 
            //load_dungeon.hierarchy_list[enemy.hierarchy_idx].mapInfos_of_layer[enemy.layer_idx][enemy.map_idx].enemy_list.Add(new EnemyInfo(enemy.unit_type, enemy.pos_x, enemy.pos_y, enemy.hierarchy_idx, enemy.layer_idx, enemy.map_idx));
            //이거처럼 new로 해주는게 안전함
            //player도 그냥 그렇게 해주면 좋을듯
            foreach (HierarchyInfo hierarchy in load_dungeon.hierarchy_list)
            {
                foreach (List<MapInfo> map_linfo_list in hierarchy.mapInfos_of_layer)
                {

                    foreach (MapInfo map_info in map_linfo_list)
                    {
                        map_info.enemy_list = new List<EnemyInfo>();
                        map_info.item_list = new List<ItemInfo>();
                    }
                }
            }
            foreach (EnemyInfo enemy in enemySerialized.enemy_serialized_list)
            {
                load_dungeon.hierarchy_list[enemy.hierarchy_idx].mapInfos_of_layer[enemy.layer_idx][enemy.map_idx].enemy_list.Add(new EnemyInfo(enemy.unit_type, enemy.pos_x, enemy.pos_y, enemy.hierarchy_idx, enemy.layer_idx, enemy.map_idx));
            }
            foreach(ItemInfo item in itemSerialized.item_serialized_list)
            {
                load_dungeon.hierarchy_list[item.hierarchy_idx].mapInfos_of_layer[item.layer_idx][item.map_idx].item_list.Add(new ItemInfo(item.pos_x, item.pos_y, item.item_type, item.specific_item_type, item.item_name, item.enchant_cnt, item.hierarchy_idx, item.layer_idx, item.map_idx));
            }
            this.player_info = new PlayerInfo(data.player_info.hierarchy_idx, data.player_info.layer_idx, data.player_info.map_idx, data.player_info.max_hp, data.player_info.cur_hp, data.player_info.attack_pt, data.player_info.pos_x, data.player_info.pos_y, data.player_info.item_list, data.player_info.hunger, data.player_info.equip_list);
            this.dungeon = load_dungeon;
            this.cur_map_info = this.dungeon.hierarchy_list[this.player_info.hierarchy_idx].mapInfos_of_layer[this.player_info.layer_idx][this.player_info.map_idx];
            this.terrain_info_arr = (TerrainInfo[,])this.cur_map_info.terrain_info_arr.Clone();
            this.enemy_list = new List<EnemyInfo>(this.cur_map_info.enemy_list);
            this.item_list = this.cur_map_info.item_list;
            /*
            this.enemy_list = new List<EnemyInfo>();
            foreach(EnemyInfo enemy in this.cur_map_info.enemy_list)
            {
                this.enemy_list.Add(enemy);
            }*/
            this.terrain_object_arr = new GameObject[this.terrain_info_arr.GetLength(0), this.terrain_info_arr.GetLength(1)];
            this.shadow_object_arr = new GameObject[this.terrain_info_arr.GetLength(0), this.terrain_info_arr.GetLength(1)];
            this.enemy_object_dict = new Dictionary<EnemyInfo, GameObject>();
            this.item_object_dict = new Dictionary<ItemInfo, GameObject>();
            this.HP_bar.GetComponent<Slider>().maxValue = this.player_info.max_hp;
            this.HP_bar.GetComponent<Slider>().value = this.player_info.cur_hp;

            //equip_list_ui_init
            foreach (ItemInfo equip_item in this.player_info.equip_list)
            {

                switch (equip_item.item_type)
                {
                    case ITEM_TYPE.WEAPON:
                        GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").transform.Find("Base").transform.Find("Bag").transform.Find("WeaponEquipSlot").GetChild(0).GetComponent<EquipSlot>().item = equip_item;
                        break;
                    case ITEM_TYPE.ARMOR:
                        GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").transform.Find("Base").transform.Find("Bag").transform.Find("ArmorEquipSlot").GetChild(0).GetComponent<EquipSlot>().item = equip_item;
                        break;
                    case ITEM_TYPE.ARTIFACT:
                        if (GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").transform.Find("Base").transform.Find("Bag").transform.Find("ArtifactEquipSlot1").GetChild(0).GetComponent<EquipSlot>().item == null)
                            GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").transform.Find("Base").transform.Find("Bag").transform.Find("ArtifactEquipSlot1").GetChild(0).GetComponent<EquipSlot>().item = equip_item;
                        else
                            GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").transform.Find("Base").transform.Find("Bag").transform.Find("ArtifactEquipSlot2").GetChild(0).GetComponent<EquipSlot>().item = equip_item;
                        break;
                    case ITEM_TYPE.RING:
                        GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").transform.Find("Base").transform.Find("Bag").transform.Find("RingEquipSlot").GetChild(0).GetComponent<EquipSlot>().item = equip_item;
                        break;
                }
            }

            Debug.Log("item_list : " + this.item_list.Count);
        }
        else
        {
            this.seed = UnityEngine.Random.Range(0, 100000);
            UnityEngine.Random.InitState(seed);

            int difficulty = 3;
            this.dungeon = new Dungeon(difficulty);
            this.player_info = new PlayerInfo();
            this.cur_map_info = dungeon.hierarchy_list[0].mapInfos_of_layer[1][0];
            this.terrain_info_arr = this.cur_map_info.terrain_info_arr;
            this.enemy_list = this.cur_map_info.enemy_list;
            this.item_list = this.cur_map_info.item_list;
            this.HP_bar.GetComponent<Slider>().maxValue = this.player_info.max_hp;
            this.HP_bar.GetComponent<Slider>().value = this.player_info.cur_hp;

            this.terrain_object_arr = new GameObject[this.terrain_info_arr.GetLength(0), this.terrain_info_arr.GetLength(1)];
            this.shadow_object_arr = new GameObject[this.terrain_info_arr.GetLength(0), this.terrain_info_arr.GetLength(1)];
            this.enemy_object_dict = new Dictionary<EnemyInfo, GameObject>();
            this.item_object_dict = new Dictionary<ItemInfo, GameObject>();

            this.player_info.item_list.Add(new ItemInfo(0, 0, ITEM_TYPE.SCROLL, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.SCROLL_BOSS, 0, 0, 0, 0));
            this.player_info.item_list.Add(new ItemInfo(0, 0, ITEM_TYPE.SCROLL, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.SCROLL_SHEEP, 0, 0, 0, 0));
            this.player_info.item_list.Add(new ItemInfo(0, 0, ITEM_TYPE.SCROLL, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.SCROLL_SHEEP, 0, 0, 0, 0));
            this.player_info.item_list.Add(new ItemInfo(0, 0, ITEM_TYPE.ARMOR, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.ARMOR_01, 0, 0, 0, 0));
            this.player_info.item_list.Add(new ItemInfo(0, 0, ITEM_TYPE.WEAPON, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.SWORD_01, 0, 0, 0, 0));
            this.player_info.item_list.Add(new ItemInfo(0, 0, ITEM_TYPE.FOOD, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.FOOD_01, 0, 0, 0, 0));
            this.player_info.item_list.Add(new ItemInfo(0, 0, ITEM_TYPE.POTION, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.POTION_HP, 0, 0, 0, 0));
            this.player_info.item_list.Add(new ItemInfo(0, 0, ITEM_TYPE.ARTIFACT, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.ARTIFACT_01, 0, 0, 0, 0));
        }
        //set player position
        if (game_loaded)
        {
            this.player_object.transform.position = new Vector3(this.player_info.pos_x, this.player_info.pos_y, 0);
        }
        else
        {
            StairInfo target_stair = null;
            foreach (StairInfo cur in this.cur_map_info.stair_list)
            {
                if (cur.connected_map.id == dungeon.hierarchy_list[0].mapInfos_of_layer[0][0].id) target_stair = cur;
            }
            if (target_stair != null)
            {
                this.player_info.SetPos(target_stair.pos_x, target_stair.pos_y);
                this.player_object.transform.position = new Vector3(this.player_info.pos_x, this.player_info.pos_y, 0);
            }
        }

        //terrain-shadow init
        for(int i = 0; i < this.terrain_info_arr.GetLength(0); i++)
        {
            for(int j = 0; j < this.terrain_info_arr.GetLength(1); j++)
            {
                int dx = player_info.pos_x - j;
                int dy = player_info.pos_y - i;
                if(this.player_info.pos_x == j && this.player_info.pos_y == i)
                {
                    terrain_info_arr[i, j].visited = true;
                    terrain_info_arr[i, j].in_player_sight = true;
                }
                else if(dx*dx + dy*dy < 20)
                {
                    if (player_info.PlayerSight(j, i, terrain_info_arr))
                    {
                        terrain_info_arr[i, j].visited = true;
                        terrain_info_arr[i, j].in_player_sight = true;
                    }
                    else
                    {
                        terrain_info_arr[i, j].in_player_sight = false;
                    }
                }
            }
        }

        //inven init
        foreach(ItemInfo item in this.player_info.item_list)
        {
            GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").GetComponent<Inventory>().FreeSlot();
            GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").GetComponent<Inventory>().items.Add(item);
            GameObject.Find("Canvas").transform.Find("InvenPopUp").transform.Find("Inventory").GetComponent<Inventory>().FreeSlot();
        }

        //create terrain objects
        for (int i = 0; i < this.terrain_info_arr.GetLength(0); i++)
        {
            for (int j = 0; j < this.terrain_info_arr.GetLength(1); j++)
            {
                if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.EMPTY)
                {
                    this.terrain_object_arr[i, j] = Instantiate(empty_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }
                else if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.FLOOR)
                {
                    this.terrain_object_arr[i, j] = Instantiate(floor_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }
                else if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.WALL)
                {
                    this.terrain_object_arr[i, j] = Instantiate(wall_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }
                else if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.BRIDGE)
                {
                    this.terrain_object_arr[i, j] = Instantiate(bridge_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }
                else if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.DOOR)
                {
                    this.terrain_object_arr[i, j] = Instantiate(door_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }
                else if (this.terrain_info_arr[i, j].terrain_type == TERRAIN_TYPE.STAIR)
                {
                    this.terrain_object_arr[i, j] = Instantiate(stair_prefab, new Vector3(j, i, 0), Quaternion.identity);
                }
            }
        }

        //create enemy objects
        foreach (EnemyInfo cur in this.enemy_list)
        {
            switch (cur.unit_type) {   
                case UNIT_TYPE.ENEMY_RAT:
                    enemy_object_dict[cur] = Instantiate(enemy_rat_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case UNIT_TYPE.ENEMY_GNOLL:
                    enemy_object_dict[cur] = Instantiate(enemy_gnoll_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case UNIT_TYPE.ENEMY_CRAB:
                    enemy_object_dict[cur] = Instantiate(enemy_crab_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case UNIT_TYPE.ENEMY_BOOS_01:
                    enemy_object_dict[cur] = Instantiate(boss_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
            }  
        }

        //create items
        foreach (ItemInfo cur in this.item_list)
        {
            Debug.Log(cur.item_name);
            switch (cur.item_name)
            {
                case ITEM_NAME.SWORD_01:
                    item_object_dict[cur] = Instantiate(sword_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.AX_01:
                    item_object_dict[cur] = Instantiate(ax_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.ARMOR_01:
                    item_object_dict[cur] = Instantiate(armor_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.ARMOR_02:
                    item_object_dict[cur] = Instantiate(armor_02_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.ARTIFACT_01:
                    item_object_dict[cur] = Instantiate(artifact_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.FOOD_01:
                    item_object_dict[cur] = Instantiate(food_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.POTION_HP:
                    item_object_dict[cur] = Instantiate(potion_hp_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.RING_01:
                    item_object_dict[cur] = Instantiate(ring_01_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.RING_02:
                    item_object_dict[cur] = Instantiate(ring_02_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.SCROLL_BOSS:
                    item_object_dict[cur] = Instantiate(scroll_boss_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
                case ITEM_NAME.SCROLL_SHEEP:
                    item_object_dict[cur] = Instantiate(scroll_sheep_prefab, new Vector3(cur.pos_x, cur.pos_y, 0), Quaternion.identity);
                    break;
            }
        }

        //create shadows
        for (int i = 0; i < this.terrain_object_arr.GetLength(0); i++)
        {
            for (int j = 0; j < this.terrain_object_arr.GetLength(1); j++)
            {
                GameObject shadow = Instantiate(shadow_prefab, new Vector3(j, i, 0), Quaternion.identity);
                if (terrain_info_arr[i, j].in_player_sight == true)
                {
                    Color color = shadow.GetComponent<SpriteRenderer>().color;
                    shadow.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 0);
                }
                this.shadow_object_arr[i, j] = shadow;
            }
        }

        //camera
        cam.transform.position = new Vector3(this.player_info.pos_x, this.player_info.pos_y, -10);
    }

    public void PrintFileList()
    {
        DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath + "/");
        foreach (FileInfo File in di.GetFiles())
        {
            Debug.Log(File.Name);
        }
    }

    private void UpdateShadow()
    {
        for (int i = 0; i < this.terrain_info_arr.GetLength(0); i++)
        {
            for (int j = 0; j < this.terrain_info_arr.GetLength(1); j++)
            {
                int dx = player_info.pos_x - j;
                int dy = player_info.pos_y - i;
                if (this.player_info.pos_x == j && this.player_info.pos_y == i)
                {
                    terrain_info_arr[i, j].visited = true;
                    terrain_info_arr[i, j].in_player_sight = true;
                }
                else if (dx * dx + dy * dy < 20)
                {
                    if (player_info.PlayerSight(j, i, terrain_info_arr))
                    {
                        terrain_info_arr[i, j].visited = true;
                        terrain_info_arr[i, j].in_player_sight = true;
                    }
                    else
                    {
                        terrain_info_arr[i, j].in_player_sight = false;
                    }
                }
                else
                {
                    terrain_info_arr[i, j].in_player_sight = false;
                }
            }
        }
    }
    IEnumerator ResetAnimation(Animator animator)
    {
        yield return new WaitForSeconds(0.15f);
        Debug.Log("RESETANIMATION");
        animator.SetInteger("unit_state", 0);
    }

    public void CreateAndDestroySheep(int pos_x, int pos_y)
    {
        sheep_object_list.Add((Instantiate(sheep_prefab, new Vector3(pos_x, pos_y, 0), Quaternion.identity), 3));
    }

    public ItemInfo FindItemAt(int x, int y)
    {
        foreach(ItemInfo item in this.item_list)
        {
            if (item.pos_x == x && item.pos_y == y) return item;
        }
        return null;
    }

    public void DropItem(ItemInfo item)
    {
        this.player_info.item_list.Remove(item);
        ItemInfo new_item = new ItemInfo(this.player_info.pos_x, this.player_info.pos_y, item.item_type, item.specific_item_type, item.item_name, item.enchant_cnt, this.player_info.hierarchy_idx, this.player_info.layer_idx, this.player_info.map_idx);
        
        this.item_list.Add(new_item);
        switch (new_item.item_name)
        {
            case ITEM_NAME.SWORD_01:
                this.item_object_dict[new_item] = Instantiate(sword_01_prefab, new Vector3(new_item.pos_x, new_item.pos_y, 0), Quaternion.identity);
                break;
            case ITEM_NAME.AX_01:
                this.item_object_dict[new_item] = Instantiate(ax_01_prefab, new Vector3(new_item.pos_x, new_item.pos_y, 0), Quaternion.identity);
                break;
            case ITEM_NAME.ARMOR_01:
                this.item_object_dict[new_item] = Instantiate(armor_01_prefab, new Vector3(new_item.pos_x, new_item.pos_y, 0), Quaternion.identity);
                break;
            case ITEM_NAME.ARMOR_02:
                this.item_object_dict[new_item] = Instantiate(armor_02_prefab, new Vector3(new_item.pos_x, new_item.pos_y, 0), Quaternion.identity);
                break;
            case ITEM_NAME.ARTIFACT_01:
                this.item_object_dict[new_item] = Instantiate(artifact_01_prefab, new Vector3(new_item.pos_x, new_item.pos_y, 0), Quaternion.identity);
                break;
            case ITEM_NAME.FOOD_01:
                this.item_object_dict[new_item] = Instantiate(food_01_prefab, new Vector3(new_item.pos_x, new_item.pos_y, 0), Quaternion.identity);
                break;
            case ITEM_NAME.POTION_HP:
                this.item_object_dict[new_item] = Instantiate(potion_hp_prefab, new Vector3(new_item.pos_x, new_item.pos_y, 0), Quaternion.identity);
                break;
            case ITEM_NAME.RING_01:
                this.item_object_dict[new_item] = Instantiate(ring_01_prefab, new Vector3(new_item.pos_x, new_item.pos_y, 0), Quaternion.identity);
                break;
            case ITEM_NAME.RING_02:
                this.item_object_dict[new_item] = Instantiate(ring_02_prefab, new Vector3(new_item.pos_x, new_item.pos_y, 0), Quaternion.identity);
                break;
            case ITEM_NAME.SCROLL_BOSS:
                this.item_object_dict[new_item] = Instantiate(scroll_boss_prefab, new Vector3(new_item.pos_x, new_item.pos_y, 0), Quaternion.identity);
                break;
            case ITEM_NAME.SCROLL_SHEEP:
                this.item_object_dict[new_item] = Instantiate(scroll_sheep_prefab, new Vector3(new_item.pos_x, new_item.pos_y, 0), Quaternion.identity);
                break;
        }
    }
}