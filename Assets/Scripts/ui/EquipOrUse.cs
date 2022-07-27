using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquipOrUse : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Inventory inven = GameObject.Find("Inventory").GetComponent<Inventory>();
        int idx = int.Parse(transform.parent.parent.name.Split("_")[1]);

        if (GetComponentInChildren<TMP_Text>().text == "EQUIP")
        {
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

    }
}
