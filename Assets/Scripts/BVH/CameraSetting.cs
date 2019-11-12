using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum CameraType
{
    FollowingMode = 0,
    WorldMode = 1
};

public class CameraSetting : MonoBehaviour
{
    [Header("========== 相機相關 ==========")]
    public Camera camera;
    public GameObject UnityChan;

    [Header("========== Control Point 相關 ==========")]
    public static int SelectIndex = -1;

    private Vector3 worldOffset = new Vector3(0, 2.48f, 4.44f);
    private Vector3 worldRotationOffset = new Vector3(10, 180, 0);
    private Vector3 unityChanOffset = new Vector3(-5, 5, 0);
    private CameraType type = CameraType.WorldMode;


    private void Update()
    {
        switch(type)
        {
            case CameraType.FollowingMode:
                camera.transform.position = UnityChan.transform.position + unityChanOffset;
                camera.transform.LookAt(UnityChan.transform.position);
                break;
        }
    }

    public void CameraFollowingMode()
    {
        type = CameraType.FollowingMode;
    }

    public void CameraWorldMode()
    {
        type = CameraType.WorldMode;
        camera.transform.position = worldOffset;
        camera.transform.eulerAngles = worldRotationOffset;
    }

}
