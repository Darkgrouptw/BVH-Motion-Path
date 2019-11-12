using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ConnectInfo
{
    public int StartIndex;
    public int EndIndex;

    public string StartName;
    public string EndName;

    public ConnectInfo(string startName, string endName)
    {
        StartIndex      = -1;
        EndIndex        = -1;
        this.StartName  = startName;
        this.EndName    = endName;
    }
};

public class BVHPeople : MonoBehaviour
{
    [Header("========== Motion 的資料 ==========")]
    public List<string>             MotionStringList = new List<string>();
    public List<List<float>>        MotionData = new List<List<float>>();

    [Header("========== 關節的 GameObject =========")]
    public List<GameObject>         Joints = new List<GameObject>();
    public List<LineRenderer>       Bones = new List<LineRenderer>();
    public List<ConnectInfo>        BonesInfo = new List<ConnectInfo>();
    public float                    InitMotionHeight = 0;

    public float                    framePerSec;
    private float                   time = 0;
    private float                   HeightOffset = 0;

    private void Update()
    {
        #region 顯示動作
        int CurrentIndex = (int)((time - 0.0001f) / framePerSec);
        float CurrentTime = time - CurrentIndex * framePerSec;

        MotionMove(CurrentIndex, CurrentTime);
        UpdateBones();
        #endregion 
        #region 增加時間 & 判斷是否超過了
        time += Time.deltaTime;
        float FinalTime = framePerSec * (MotionData.Count - 1);
        if(time >= FinalTime)
            time -= FinalTime;
        #endregion
    }

    private void MotionMove(int CurrentIndex, float CurrentTime)
    {
        float XPos, YPos, ZPos, XRotation, YRoation, ZRotation;
        XPos = YPos = ZPos = XRotation = YRoation = ZRotation = -1;

        string lastName = "";
        string[] PartInfo = null;
        for (int i = 0; i < MotionStringList.Count; i++)
        {
            PartInfo = MotionStringList[i].Split(' ');
            if(lastName != "" && lastName != PartInfo[0])
            {
                ApplyMotionValue(lastName, XPos, YPos, ZPos, XRotation, YRoation, ZRotation);
                lastName = PartInfo[0];
                XPos = YPos = ZPos = XRotation = YRoation = ZRotation = -1;
            }

            // First Case
            if(lastName == "")
            {
                lastName = PartInfo[0];
                GetMotionValue(PartInfo[1], CurrentIndex, i, ref XPos, ref YPos, ref ZPos, ref XRotation, ref YRoation, ref ZRotation);
            }
            else if(lastName == PartInfo[0])
                GetMotionValue(PartInfo[1], CurrentIndex, i, ref XPos, ref YPos, ref ZPos, ref XRotation, ref YRoation, ref ZRotation);
        }
        ApplyMotionValue(lastName, XPos, YPos, ZPos, XRotation, YRoation, ZRotation);
    }
    private void UpdateBones()
    {
        for(int i = 0; i < Bones.Count; i++)
        {
            ConnectInfo info = BonesInfo[i];
            if (info.StartIndex == -1 || info.EndIndex == -1)
            {
                info.StartIndex = SearchJointIndexByName(info.StartName);
                info.EndIndex = SearchJointIndexByName(info.EndName);
            }
            
            Bones[i].SetPosition(0, Joints[info.StartIndex].transform.position);
            Bones[i].SetPosition(1, Joints[info.EndIndex].transform.position);
        }
    }

    private int SearchJointIndexByName(string name)
    {
        for (int i = 0; i < Joints.Count; i++)
            if (Joints[i].name == name)
                return i;
        return -1;
    }

    // Current Index 是 現在在第幾個動作的 Set 裡
    // DataIndex 是在 Set 裡面的哪一個 Index
    private void GetMotionValue(string name, int CurrentIndex, int DataIndex,
        ref float xpos, ref float ypos, ref float zpos, ref float xrotation, ref float yrotation, ref float zrotation)
    {
        int NextIndex = (CurrentIndex + 1) % MotionData.Count;
        
        // 作超過180 的判斷
        float CurrentAngle = 0, NextAngle = 0;
        if(name.ToLower().Contains("rotation"))
        {
            CurrentAngle = MotionData[CurrentIndex][DataIndex];
            NextAngle = MotionData[NextIndex][DataIndex];
            if (Mathf.Abs(CurrentAngle - NextAngle) > 180)
                if (NextAngle > 0)
                    NextAngle -= 360;
                else
                    NextAngle += 360;
        }

        switch (name)
        {
            case "Xposition":
                xpos = Lerp(MotionData[CurrentIndex][DataIndex], MotionData[NextIndex][DataIndex]);
                break;
            case "Yposition":
                ypos = Lerp(MotionData[CurrentIndex][DataIndex], MotionData[NextIndex][DataIndex]);
                break;
            case "Zposition":
                zpos = Lerp(MotionData[CurrentIndex][DataIndex], MotionData[NextIndex][DataIndex]);
                break;
            case "Xrotation":
                xrotation = Lerp(CurrentAngle, NextAngle);
                break;
            case "Yrotation":
                yrotation = Lerp(CurrentAngle, NextAngle);
                break;
            case "Zrotation":
                zrotation = Lerp(CurrentAngle, NextAngle);
                break;
        }
    }
    private void ApplyMotionValue(string name, float xpos, float ypos, float zpos,
        float xrotation, float yrotation, float zrotation)
    {
        // 拿出要動的 GameObject
        int index = SearchJointIndexByName(name);
        if (index == -1)
            Debug.LogError("讀取的檔案的問題!!");

        // 更改他的資訊
        if (xpos != -1 || ypos != -1 || zpos != -1)
            Joints[index].transform.localPosition = new Vector3(xpos, ypos, zpos);

        if (xrotation != -1 || yrotation != -1 || zrotation != -1)
        {
            Quaternion x = Quaternion.identity * Quaternion.AngleAxis(xrotation , this.transform.right);
            Quaternion y = Quaternion.identity * Quaternion.AngleAxis(yrotation, this.transform.up);
            Quaternion z = Quaternion.identity * Quaternion.AngleAxis(zrotation, this.transform.forward);
            Joints[index].transform.localRotation = z * x * y;
        }
    }

    private float Lerp(float Min, float Max)
    {
        int CurrentIndex = (int)((time - 0.00001f) / framePerSec);
        float prograss = Mathf.Clamp(time - CurrentIndex * framePerSec, 0 , framePerSec) / framePerSec;
        return (Max - Min) * prograss + Min;
    }
}
