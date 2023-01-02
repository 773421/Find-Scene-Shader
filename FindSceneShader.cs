using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class FindSceneShader : MonoBehaviour
{
    static string[] assetGUIDs;

    static string[] assetPaths;

    static string[] allAssetPaths;

    static List<string> allMaterial = new List<string>();

    static List<Shader> allShaders = new List<Shader>();



    static ShderTimeTool shadertimeTool = new ShderTimeTool();

    [MenuItem("Game/ArtTools/StartFindShader")]

    public static void FindAssetRefMenu()
    {

        if (Application.isPlaying)
        {

            Debug.Log("开始查找场景内shader引用");

            shadertimeTool.Perform(15, FindShder);

        }

        else
        {

            if (Selection.assetGUIDs.Length == 0)
            {

                Debug.Log("请先选择任意一个组件，再击此菜单");

                return;
            }

            assetGUIDs = Selection.assetGUIDs;

            assetPaths = new string[assetGUIDs.Length];

            for (int i = 0; i < assetGUIDs.Length; i++)
            {

                assetPaths[i] = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

            }

            allAssetPaths = AssetDatabase.GetAllAssetPaths();

            FindAssetRef();

        }
    }

    /// <summary>

    /// 运行模式下查找场景内引用shader

    /// </summary>

    static void FindShder()
    {

        var meshRender = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();

        Debug.Log(meshRender.Length);

        foreach (var item in meshRender)
        {

            Debug.Log(item.materials);

            var mtrs = item.materials;

            foreach (var mtr in mtrs)
            {

                Debug.Log(mtr);

                allShaders.Add(mtr.shader);

            }
        }



        for (int i = 0; i < allShaders.Count; i++)
        {
            for (int j = allShaders.Count - 1; j > i; j--)
            {
                if (allShaders[i] == allShaders[j])
                {

                    allShaders.RemoveAt(j);

                }
            }
        }
    }

    /// <summary>

    /// 离线模式下查找场景内引用shader

    /// </summary>

    static void FindAssetRef()
    {

        Debug.Log(string.Format("开始查找引用{0}的资源。", string.Join(",", assetPaths)));

        List<string> logInfo = new List<string>();

        string path;

        string log;

        string materialPath;

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < allAssetPaths.Length; i++)
        {

            path = allAssetPaths[i];

            if (path.EndsWith(".prefab") || path.EndsWith(".unity"))
            {

                string content = File.ReadAllText(path);

                if (content == null)
                {

                    Debug.Log("没有资源引用");

                    continue;

                }

                for (int j = 0; j < assetGUIDs.Length; j++)
                {
                    if (content.IndexOf(assetGUIDs[j]) > 0)
                    {

                        log = string.Format("{0} 引用了  {1}", path, assetPaths[j]);

                        materialPath = assetPaths[j];

                        logInfo.Add(log);

                        allMaterial.Add(materialPath);

                    }
                }
            }
        }

        for (int i = 0; i < allMaterial.Count; i++)
        {
            for (int j = allMaterial.Count - 1; j > i; j--)
            {
                if (allMaterial[i] == allMaterial[j])
                {

                    allMaterial.RemoveAt(j);

                }
            }
        }

        List<Shader> allShader = new List<Shader>();

        for (int i = 0; i < allMaterial.Count; i++)
        {

            Debug.Log(allMaterial[i].ToString());

            var mtil = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(allMaterial[i]);

            sb.Append((mtil.shader.name + "\r\n").ToString());

        }

        FileStream fs = new FileStream(Application.dataPath + "/AllShader/ShaderName.txt", FileMode.Create);

        byte[] bytes = new UTF8Encoding().GetBytes(sb.ToString());

        fs.Write(bytes, 0, bytes.Length);

        fs.Close();

        Debug.Log("查找完成");

    }
    [MenuItem("Game/ArtTools/StopFindShader")]

    public static void Stop()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < allShaders.Count; i++)
        {

            Debug.Log(allShaders[i].ToString());

            sb.Append((allShaders[i].name + "\r\n").ToString());

        }

        FileStream fs = new FileStream(Application.dataPath + "/AllShader/ShaderName.txt", FileMode.Create);

        byte[] bytes = new UTF8Encoding().GetBytes(sb.ToString());

        fs.Write(bytes, 0, bytes.Length);

        fs.Close();

        Debug.Log("查找完成");

        shadertimeTool.Stop();

    }

}

public class ShderTimeTool : Editor
{

    /// <summary>

    /// 延迟秒数

    /// </summary>

    private float m_Delay;

    private Action m_Callback;

    private float m_StartupTime;



    public void Perform(float delay, Action callback)

    {

        m_Delay = delay;

        m_Callback = callback;



        EditorApplication.update += Update;

    }

    // 主动停止

    public void Stop()
    {
        m_StartupTime = 0;

        m_Callback = null;

        EditorApplication.update -= Update;

    }

    private void Update()
    {

        if (m_StartupTime <= 0) m_StartupTime = Time.realtimeSinceStartup;

        if (Time.realtimeSinceStartup - m_StartupTime >= m_Delay)
        {

            if (m_Callback != null) m_Callback();

            m_StartupTime = Time.realtimeSinceStartup;
        }
    }
}
