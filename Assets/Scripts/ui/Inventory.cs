using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ItemInfo> items = new List<ItemInfo>();

    [SerializeField]
    private Transform slot_parent;
    [SerializeField]
    private Slot[] slots;

    [SerializeField]
    public GameObject slot_prefab;

    private void Awake()
    {
        for (int i = 0; i < 20; i++)
            SlotSpawn(slot_prefab, i + 1);

        slots = slot_parent.GetComponentsInChildren<Slot>();

        FreeSlot();
        AddItem(new ItemInfo(0, 0, ITEM_TYPE.WEAPON, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.SWORD_01, 0));
        AddItem(new ItemInfo(0, 0, ITEM_TYPE.WEAPON, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.AX_01, 0));
        AddItem(new ItemInfo(0, 0, ITEM_TYPE.ARMOR, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.ARMOR_01, 0));
        AddItem(new ItemInfo(0, 0, ITEM_TYPE.ARMOR, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.ARMOR_02, 0));
        AddItem(new ItemInfo(0, 0, ITEM_TYPE.ARTIFACT, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.ARTIFACT_01, 0));
        AddItem(new ItemInfo(0, 0, ITEM_TYPE.ARTIFACT, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.ARTIFACT_02, 0));
        AddItem(new ItemInfo(0, 0, ITEM_TYPE.ARTIFACT, SPECIFIC_ITEM_TYPE.NONE, ITEM_NAME.ARTIFACT_03, 0));
    }

    public void FreeSlot()
    {
        int i = 0;
        for (; i < items.Count && i < slots.Length; i++)
        {
            slots[i].item = items[i];
        }
        for (; i < slots.Length; i++)
        {
            slots[i].item = null;
        }

    }

    public void AddItem(ItemInfo item)
    {

        if (items.Count < slots.Length)
        {
            items.Add(item);
            FreeSlot();
        }
        else
        {
            print("Full Slot");
        }

    }

    public void SlotSpawn(GameObject prefab, int i)
    {

        GameObject slot = Instantiate(slot_prefab, new Vector3(), Quaternion.identity, GameObject.Find("Content").transform);
        slot.name = "slot_" + i.ToString();

    }


}