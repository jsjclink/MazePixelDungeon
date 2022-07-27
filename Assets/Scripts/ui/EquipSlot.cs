using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquipSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    Image image;
    [SerializeField]
    GameObject ItemInfoPopUp;

    public int idx;

    private ItemInfo _item;
    public ItemInfo item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item != null)
            {
                image.sprite = _item.item_image;
                image.color = new Color(1, 1, 1, 1);
            }
            else
            {
                image.color = new Color(1, 1, 1, 0);
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {

        if (_item != null)
        {
            GameObject item_info = Instantiate(ItemInfoPopUp, new Vector3(540, 960), Quaternion.identity, GameObject.Find("Canvas").transform);
            item_info.name = this.transform.parent.name + "_PopUp";

            item_info.GetComponentsInChildren<Image>()[2].sprite = _item.item_image;
            item_info.GetComponentsInChildren<TMP_Text>()[0].text = _item.item_name.ToString();

            item_info.GetComponentsInChildren<TMP_Text>()[2].text = "UNEQUIP";

        }

    }
}
