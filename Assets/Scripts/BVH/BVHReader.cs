using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.IO;

public class BVHReader : MonoBehaviour
{
    [Header("========== 顯示相關 ==========")]
    public Text                 DebugText;                                          // 顯示有幾個 File
    public GameObject           Joint;                                              // 關節

    [Header("========== 場景切換 ==========")]
    public bool                 IsUnityChan = false;
    public int                  ShowIndex = -1;
    public int                  OffsetX = 50;

    [Header("========= Unity Chan 的設定 ==========")]
    public List<float>          OffSetHeight = new List<float>();


    private GameObject PeopleGroup;
    public List<GameObject>     People = new List<GameObject>();                    // 所有人的 GameObject
    public string[]             FilePaths;                                          // 存所有 File 的 Path
    private const int           Gap = 100;


    private void Awake()
    {
        #region 創造 People Group
        PeopleGroup = new GameObject();
        PeopleGroup.name = "People Group";
        #endregion
        #region 找所有的 BVH 檔案出來
        FilePaths = FindAllPath();

        if(!IsUnityChan)
            DebugText.text = "File Size: " + FilePaths.Length;
        #endregion
        #region 每個都跑過一次
        for (int i = 0; i < FilePaths.Length; i++)
        {
            #region 檔案前置處理
            string[] inputStr = File.ReadAllLines(FilePaths[i]);
            string[] BodyInfo = null;
            string[] MotionInfo = null;

            SplitInputStr(inputStr, ref BodyInfo, ref MotionInfo);
            #endregion
            #region 存進資料裡
            ParseByBodyData(BodyInfo, i + 1, FilePaths[i]);
            ParseByMotionData(MotionInfo, i);
            #endregion
            #region 位移
            if(!IsUnityChan)
            {
                int rowIndex = i % 6;
                int colIndex = i / 6;
                People[i].transform.Translate(new Vector3(rowIndex * Gap, 0, colIndex * Gap));
            }
            else
            {
                People[i].transform.Translate(new Vector3(0, 0, -1));

                RaycastHit hit;
                Physics.Raycast(People[i].transform.position, -Vector3.up, out hit);
                People[i].transform.Translate(new Vector3(0, -hit.distance, 0));

                People[i].SetActive(false);
            }
            #endregion
        }
        #endregion
    }

    // 找所有檔案的路徑
    private string[] FindAllPath()
    {
        string TempPath = "BVH Sample Files/";
        if (Directory.Exists("Builds"))
            TempPath = "Builds/" + TempPath;
        return Directory.GetFiles(TempPath);
    }


    // Parse 檔案相關
    private void SplitInputStr(string[] inputStr,ref string[] Data, ref string[] Motion)
    {
        bool IsMotion = false;
        List<string> DataArray = new List<string>();
        List<string> MotionArray = new List<string>();

        for(int i = 0; i < inputStr.Length; i++)
            if(!IsMotion)
            {
                if (inputStr[i] != "MOTION")
                    DataArray.Add(inputStr[i]);
                else
                {
                    IsMotion = true;
                    MotionArray.Add(inputStr[i]);
                }
            }
            else
                MotionArray.Add(inputStr[i]);

        Data = DataArray.ToArray();
        Motion = MotionArray.ToArray();
    }
    private void ParseByBodyData(string []data, int index, string FilePath)
    {
        #region 創一個新的 People
        string tempName = "";
        GameObject tempPeople = new GameObject();
        BVHPeople script = tempPeople.AddComponent<BVHPeople>();

        string[] FilePathList = FilePath.Split('/');
        tempPeople.name = "People " + index + " " + FilePathList[FilePathList.Length - 1];
        #endregion
        #region Parse 出高度
        float TempHeight = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] == "HIERARCHY")
                continue;
            #region Root & Joint & End Site
            else if (data[i].Contains("ROOT") || data[i].Contains("JOINT"))
            {
                string[] name = data[i].Split(' ');
                name = ParseAllUselessItems(name);
                tempName = name[1];
            }
            else if (data[i].Contains("End Site"))
                continue;
            #endregion
            #region 讀到 上括號 下括號
            else if (data[i].Contains("{"))
                continue;
            else if (data[i].Contains("}"))
                continue;
            #endregion
            #region 跟物體 Info 有關係
            else if (data[i].Contains("OFFSET"))
            {
                data[i] = data[i].Replace("\t", " ");
                string[] info = data[i].Split(' ');
                info = ParseAllUselessItems(info);

                #region Hip 以上是 Top，以 Hip 以下是 Buttom
                switch (tempName)
                {
                    case "LeftHip":
                    case "LeftKnee":
                    case "LeftAnkle":
                        TempHeight -= float.Parse(info[2]);
                        break;
                    case "Chest":
                    case "LeftCollar":
                    case "LeftShoulder":
                    case "Neck":
                    case "Head":
                        TempHeight += float.Parse(info[2]);
                        break;
                }
                #endregion
            }
            else if (data[i].Contains("CHANNELS"))
                continue;
            #endregion
        }
        #endregion
        #region Parse 資料
        GameObject tempJoint = null;
        Transform p = null;

        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] == "HIERARCHY")
                continue;
            #region Root & Joint & End Site
            else if (data[i].Contains("ROOT") || data[i].Contains("JOINT"))
            {
                string[] name = data[i].Split(' ');
                name = ParseAllUselessItems(name);
                tempName = name[1];

                tempJoint = GameObject.Instantiate(Joint);

                // 將資料丟進去
                script.Joints.Add(tempJoint);
            }
            else if (data[i].Contains("End Site"))
            {
                string[] info = tempJoint.transform.parent.name.Split(' ');

                tempName = info[0] + " End";
                tempJoint = GameObject.Instantiate(Joint);

                script.Joints.Add(tempJoint);
            }
            #endregion
            #region 讀到 上括號 下括號
            else if (data[i].Contains("{"))
            {
                if (p == null)
                    p = tempPeople.transform;
                else
                    p = p.GetChild(p.childCount - 1);
            }
            else if (data[i].Contains("}"))
                p = p.parent;
            #endregion
            #region 跟物體 Info 有關係
            else if(data[i].Contains("OFFSET"))
            {
                data[i] = data[i].Replace("\t", " ");
                string[] info = data[i].Split(' ');
                info = ParseAllUselessItems(info);

                tempJoint.transform.SetParent(p);
                tempJoint.name = tempName;

                float x = float.Parse(info[info.Length - 3]);
                float y = float.Parse(info[info.Length - 2]);
                float z = float.Parse(info[info.Length - 1]);

                tempJoint.transform.localPosition = new Vector3(x, y, z);

                if (tempJoint.transform.parent.name.Contains("People"))
                    continue;
                #region 加上 Line Render 把線畫出來
                LineRenderer line = tempJoint.AddComponent<LineRenderer>();
                line.positionCount = 2;
                line.useWorldSpace = true;

                if(IsUnityChan)
                {
                    line.startWidth = 0.01f;
                    line.endWidth = 0.05f;
                }
                else
                {
                    line.startWidth = 0.1f;
                    line.endWidth = 1;
                }

                script.Bones.Add(line);
                #endregion
                #region 先把連接的資料連起來
                ConnectInfo connectInfo = new ConnectInfo(tempJoint.name, tempJoint.transform.parent.name);
                script.BonesInfo.Add(connectInfo);
                #endregion
            }
            else if(data[i].Contains("CHANNELS"))
            {
                data[i] = data[i].Replace("\t", " ");
                
                string[] info = data[i].Split(' ');
                info = ParseAllUselessItems(info);

                // CHANNELS 3 XX XX XX
                int count = int.Parse(info[1]);
                for (int j = 0; j < count; j++)
                    script.MotionStringList.Add(tempName + " " + info[j + 2]);
            }
            #endregion
        }
        #endregion

        if (IsUnityChan)
        {
            tempPeople.transform.localPosition += new Vector3(0, 0.785f, 0);
            tempPeople.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        }

        // 加到堆疊裡
        tempPeople.GetComponent<BVHPeople>().InitMotionHeight = TempHeight;

        tempPeople.transform.SetParent(PeopleGroup.transform);
        People.Add(tempPeople);
    }
    private void ParseByMotionData(string []data,int index)
    {
        BVHPeople tempPeople = People[index].GetComponent<BVHPeople>();
        for (int i = 0; i < data.Length; i++)
            if (data[i].StartsWith("MOTION"))
                continue;
            else if(data[i].StartsWith("Frames:"))
                continue;
            else if(data[i].StartsWith("Frame Time:"))
            {
                data[i] = data[i].Replace("\t", " ");
                string[] info = data[i].Split(' ');
                info = ParseAllUselessItems(info);

                tempPeople.framePerSec = float.Parse(info[info.Length - 1]);
            }
            else
            {
                data[i] = data[i].Replace("\t", " ");
                string[] info = data[i].Split(' ');
                info = ParseAllUselessItems(info);

                // 怕有東西雷
                if (info.Length <= 1)
                    break;
                
                List<float> motionData = new List<float>();
                for (int j = 0; j < info.Length; j++)
                {
                    float listData = float.Parse(info[j]);
                    motionData.Add(listData);
                }
                tempPeople.MotionData.Add(motionData);
            }

    }
    private string[] ParseAllUselessItems(string[] inputData)
    {
        List<string> output = new List<string>();
        for (int i = 0; i < inputData.Length; i++)
            if (inputData[i] != "")
                output.Add(inputData[i]);
        return output.ToArray();
    }
}
