using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PathEditingMenu : MonoBehaviour
{
    [Header("========== Menu ==========")]
    public Dropdown Menu;
    
    [Header("========== Unity Chan ==========")]
    public BVHUnityChan UnityChan;
    public BVHReader    reader;
    
    private int LastSelectIndex = 0;


    private void Start()
    {
        RefreshFileList();
    }

    private void Update()
    {
        UnityChan.UpdateMotionPos(reader.People[LastSelectIndex]);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    // Drop Down 相關
    public void SelectDropDownList(int index)
    {
        LastSelectIndex = index;
        ShowSelectIndexPeople(index);

        BVHPeople people = reader.People[index].GetComponent<BVHPeople>();

        UnityChan.GetDefaultData(people.MotionStringList.ToArray());
        Menu.value = index;
    }

    private void ShowSelectIndexPeople(int index)
    {
        for (int i = 0; i < reader.People.Count; i++)
            reader.People[i].SetActive(i == index);
        LastSelectIndex = index;
    }

    public void RefreshFileList()
    {
        // 把所有檔案 撈近來
        string[] FilePaths = reader.FilePaths;
        #region Menu 相關
        // 清除 options
        Menu.ClearOptions();

        // 加上 Options
        List<string> options = new List<string>();
        for (int i = 0; i < FilePaths.Length; i++)
        {
            string[] NameList = FilePaths[i].Split('/');
            options.Add(NameList[NameList.Length - 1]);
        }
        Menu.AddOptions(options);
        #endregion

        // 顯示第一個
        SelectDropDownList(1);
    }
}
