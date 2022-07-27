using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class LoadDatas : MonoBehaviour
{
    public List<string> saves = new List<string>();

    [SerializeField]
    private Transform slot_parent;
    [SerializeField]
    private LoadSlot[] slots;

    [SerializeField]
    public GameObject load_slot_prefab;

    private void Awake()
    {

        string path = Application.persistentDataPath + "/";
        if (Directory.Exists(path))
        {

            DirectoryInfo di = new DirectoryInfo(path);
            foreach(FileInfo cur in di.GetFiles())
            {
                SlotSpawn(load_slot_prefab, cur.Name.Split('.')[0]);
            }

        }
        
        slots = slot_parent.GetComponentsInChildren<LoadSlot>();

        FreeSlot();

    }

    public void FreeSlot()
    {
        int i = 0;
        for (; i < saves.Count && i < slots.Length; i++)
        {
            slots[i].file = saves[i];
        }

    }

    public void AddItem(string file)
    {

        if (saves.Count < slots.Length)
        {
            saves.Add(file);
            FreeSlot();
        }
        else
        {
            print("½½·ÔÀÌ °¡µæ Âü");
        }

    }

    public void SlotSpawn(GameObject prefab, string name)
    {
        load_slot_prefab.GetComponentInChildren<TMP_Text>().text = name;
        GameObject slot = Instantiate(load_slot_prefab, new Vector3(), Quaternion.identity, GameObject.Find("Content").transform);
    }

}
