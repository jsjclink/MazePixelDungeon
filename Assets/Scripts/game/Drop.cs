using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Drop : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Inventory inven = GameObject.Find("Inventory").GetComponent<Inventory>();
        int idx = int.Parse(transform.parent.parent.name.Split("_")[1]);
        GameObject.Find("GameSystem").GetComponent<GameSystemManager>().DropItem(inven.items[idx]);
        
        inven.items.RemoveAt(idx);
        inven.FreeSlot();
        Destroy(this.transform.parent.parent.gameObject);
    }

}
