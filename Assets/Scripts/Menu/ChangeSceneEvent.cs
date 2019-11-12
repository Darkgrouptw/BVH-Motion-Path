using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class ChangeSceneEvent : MonoBehaviour
{
    #region 按照順序的事件
    public void MenuToAllBVHScene()
    {
        SceneManager.LoadScene(1);
    }

    public void MenuToPathEditingScene()
    {
        SceneManager.LoadScene(2);
    }
    #endregion
    #region 返回事件
    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
    #endregion
}
