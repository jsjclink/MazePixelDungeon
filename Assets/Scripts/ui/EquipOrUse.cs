using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipOrUse : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Inventory inven = GameObject.Find("Inventory").GetComponent<Inventory>();
        GameSystemManager game_system_manager = GameObject.Find("GameSystem").GetComponent<GameSystemManager>();



        if (GetComponentInChildren<TMP_Text>().text == "EQUIP")
        {
            int idx = int.Parse(transform.parent.parent.name.Split("_")[1]);
            switch (inven.items[idx].item_type)
            {
                case (ITEM_TYPE.WEAPON):
                    if (GameObject.Find("WeaponEquipSlot").transform.GetChild(0).GetComponent<EquipSlot>().item == null)
                    {
                        GameObject.Find("WeaponEquipSlot").transform.GetChild(0).GetComponent<EquipSlot>().item = inven.items[idx];
                        inven.items.RemoveAt(idx);
                        inven.FreeSlot();
                        Destroy(transform.parent.parent.gameObject);
                    }
                    else
                    {
                        ItemInfo temp = inven.items[idx];
                        inven.items[idx] = GameObject.Find("WeaponEquipSlot").transform.GetChild(0).GetComponent<EquipSlot>().item;
                        GameObject.Find("WeaponEquipSlot").transform.GetChild(0).GetComponent<EquipSlot>().item = temp;
                        inven.FreeSlot();
                        Destroy(transform.parent.parent.gameObject);
                    }
                    break;
                case (ITEM_TYPE.ARMOR):
                    if (GameObject.Find("ArmorEquipSlot").transform.GetChild(0).GetComponent<EquipSlot>().item == null)
                    {
                        GameObject.Find("ArmorEquipSlot").transform.GetChild(0).GetComponent<EquipSlot>().item = inven.items[idx];
                        inven.items.RemoveAt(idx);
                        inven.FreeSlot();
                        Destroy(transform.parent.parent.gameObject);
                    }
                    else
                    {
                        ItemInfo temp = inven.items[idx];
                        inven.items[idx] = GameObject.Find("ArmorEquipSlot").transform.GetChild(0).GetComponent<EquipSlot>().item;
                        GameObject.Find("ArmorEquipSlot").transform.GetChild(0).GetComponent<EquipSlot>().item = temp;
                        inven.FreeSlot();
                        Destroy(transform.parent.parent.gameObject);
                    }
                    break;
                case (ITEM_TYPE.ARTIFACT):
                    if (GameObject.Find("ArtifactEquipSlot1").transform.GetChild(0).GetComponent<EquipSlot>().item == null && GameObject.Find("ArtifactEquipSlot2").transform.GetChild(0).GetComponent<EquipSlot>().item == null)
                    {
                        GameObject.Find("ArtifactEquipSlot1").transform.GetChild(0).GetComponent<EquipSlot>().item = inven.items[idx];
                        inven.items.RemoveAt(idx);
                        inven.FreeSlot();
                        Destroy(transform.parent.parent.gameObject);
                    }
                    else if (GameObject.Find("ArtifactEquipSlot1").transform.GetChild(0).GetComponent<EquipSlot>().item != null && GameObject.Find("ArtifactEquipSlot2").transform.GetChild(0).GetComponent<EquipSlot>().item == null)
                    {
                        GameObject.Find("ArtifactEquipSlot2").transform.GetChild(0).GetComponent<EquipSlot>().item = inven.items[idx];
                        inven.items.RemoveAt(idx);
                        inven.FreeSlot();
                        Destroy(transform.parent.parent.gameObject);
                    }
                    else if (GameObject.Find("ArtifactEquipSlot1").transform.GetChild(0).GetComponent<EquipSlot>().item == null && GameObject.Find("ArtifactEquipSlot2").transform.GetChild(0).GetComponent<EquipSlot>().item != null)
                    {
                        GameObject.Find("ArtifactEquipSlot1").transform.GetChild(0).GetComponent<EquipSlot>().item = inven.items[idx];
                        inven.items.RemoveAt(idx);
                        inven.FreeSlot();
                        Destroy(transform.parent.parent.gameObject);
                    }
                    else
                    {
                        ItemInfo temp = inven.items[idx];
                        inven.items[idx] = GameObject.Find("ArtifactEquipSlot1").transform.GetChild(0).GetComponent<EquipSlot>().item;
                        GameObject.Find("ArtifactEquipSlot1").transform.GetChild(0).GetComponent<EquipSlot>().item = temp;
                        inven.FreeSlot();
                        Destroy(transform.parent.parent.gameObject);
                    }
                    break;
            }
        }
        else if(GetComponentInChildren<TMP_Text>().text == "UNEQUIP")
        {
            inven.items.Add(GameObject.Find(this.transform.parent.parent.name.Split('_')[0].ToString()).transform.GetChild(0).GetComponent<EquipSlot>().item);
            GameObject.Find(this.transform.parent.parent.name.Split('_')[0].ToString()).transform.GetChild(0).GetComponent<EquipSlot>().item = null;
            inven.FreeSlot();
            Destroy(this.transform.parent.parent.gameObject);
        }
        else
        {
            int idx = int.Parse(transform.parent.parent.name.Split("_")[1]);
            switch (inven.items[idx].item_name)
            {
                case ITEM_NAME.FOOD_01:
                    game_system_manager.player_info.item_list.Remove(inven.items[idx]);
                    game_system_manager.player_info.hunger += 300;
                    game_system_manager.player_info.cur_hp += 5;
                    game_system_manager.HP_bar.GetComponent<Slider>().value += 5;
                    break;
                case ITEM_NAME.POTION_HP:
                    if(game_system_manager.player_info.cur_hp + 30 < game_system_manager.player_info.max_hp)
                    {
                        game_system_manager.player_info.item_list.Remove(inven.items[idx]);
                        game_system_manager.player_info.cur_hp += 30;
                        game_system_manager.HP_bar.GetComponent<Slider>().value += 30;
                    }
                    break;
            }
            inven.items.RemoveAt(idx);
            inven.FreeSlot();
            Destroy(transform.parent.parent.gameObject);
        }

    }
}
