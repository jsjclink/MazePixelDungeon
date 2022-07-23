using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Close : MonoBehaviour, IPointerClickHandler
{
    public GameObject item_info;

    public void OnPointerClick(PointerEventData eventData)
    {

        Destroy(item_info.gameObject);
    }

    void Start()
    {
        
    }

}
