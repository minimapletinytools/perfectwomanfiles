using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class ConstructPoseBundle
{
	[MenuItem("Custom/Construct Pose Bundle")]
    static void construct_pose_bundle()
    {
        IEnumerable<Object> files = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets).Where(e => is_text_file(e));
		Debug.Log ("making a pose bundle out of " + files.Count() + " files");
        BuildPipeline.BuildAssetBundle(
			Selection.activeObject, files.ToArray(), 
			"Assets/Resources/POSES.unity3d", 
			BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, 
			EditorUserBuildSettings.activeBuildTarget);//, BuildOptions.UncompressedAssetBundle);//, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
    }

	[MenuItem("Custom/Construct Bulk Pose Bundle")]
	static void construct_bulknpose_bundle()
	{
		//new version, serializes as a dictionary in one big file
		IEnumerable<Object> files = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets).Where(e => is_text_file(e));
		Debug.Log ("making a pose bundle out of " + files.Count() + " files");
		Dictionary<string,string> index = new Dictionary<string, string> ();
		foreach (var e in files) {
			index[e.name] = (e as TextAsset).text;
			Debug.Log ("set data for " + e.name);
		}
		
		
		AssetDatabase.ImportAsset("Assets/POSEDICT.txt");
		TextAsset cdtxt = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/POSEDICT.txt", typeof(TextAsset));
		System.IO.Stream stream = System.IO.File.Open(AssetDatabase.GetAssetPath(cdtxt), System.IO.FileMode.Create);
		//System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(Dictionary<string,string>));
		//xs.Serialize(stream, index);
		var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
		bf.Serialize (stream, index);
		stream.Close();
		AssetDatabase.ImportAsset("Assets/POSEDICT.txt");
		cdtxt = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/POSEDICT.txt", typeof(TextAsset));
		
		List<Object> assets = new List<Object> ();
		assets.Add (cdtxt);
		
		BuildPipeline.BuildAssetBundle(
			Selection.activeObject, assets.ToArray(), 
			"Assets/Resources/BULKPOSES.unity3d", 
			BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, 
			EditorUserBuildSettings.activeBuildTarget);//, BuildOptions.UncompressedAssetBundle);//, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
	}
	
	public static bool is_text_file(Object aObj)
    {
        return System.IO.Path.GetExtension(AssetDatabase.GetAssetPath(aObj)) == ".txt";
    }
}
