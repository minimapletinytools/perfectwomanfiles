using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ListAssetBundleContents{
    [MenuItem("Custom/Helper/List Asset Bundle Contents")]
    static void ListBundleContents()
    {
        AssetBundle ab = null;
        try
        {
            Debug.Log(Selection.activeObject);
            string load = "file://" + Application.dataPath.Remove(Application.dataPath.Length - "Assets".Length) + AssetDatabase.GetAssetPath(Selection.activeObject);
            Debug.Log(load);
            WWW web = new WWW(load);
            int counter = 0;
            int sleepTime = 5;
            while (!web.isDone)
            {
                System.Threading.Thread.Sleep(sleepTime);
                counter += sleepTime;
                if (counter > 100)
                    break;
            }
            Debug.Log("loaded WWW for " + counter + " ms");
            ab = web.assetBundle;
        }
        catch
        {
            Debug.Log("Selection not an asset bundle");
        }

        var expected = new List<string>(CharacterPreprocessor.sExpected);
        expected.Add("AUDIO");
        foreach (string e in expected)
        {
            if (ab.Contains(System.IO.Path.GetFileNameWithoutExtension(e)))
                Debug.Log("found " + System.IO.Path.GetFileNameWithoutExtension(e));
                //Debug.Log("missing " + System.IO.Path.GetFileNameWithoutExtension(e));
        }

        for (int i = 0; i < 100; i++)
        {
            if (ab.Contains("BG-" + (i + 1)))
                Debug.Log("found " + "BG-" + (i + 1));
            else break;
        }
        for (int i = 0; i < 100; i++)
        {
            if (ab.Contains("FG-" + (i + 1)))
                Debug.Log("found " + "FG-" + (i + 1));
            else break;
        }
        TextAsset ta = ab.LoadAsset("CD") as TextAsset;
        Debug.Log(ta.text);
        ab.Unload(true);
    }
}
