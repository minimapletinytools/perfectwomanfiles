using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public static class DuplicateBundleScript {

	/* TODO DELETE
	public static string[] character_names = new string[]
	{
		"0-1",
		"05-1","05-2","05-3",
		"16-1","16-2","16-3",
		"27-1","27-2","27-3",
		"34-1","34-2","34-3",
		"45-1","45-2","45-3",
		"60-1","60-2","60-3",
		"80-1","80-2","80-3",
		"100"
	};
	
    [MenuItem("Custom/DuplicateMain")]
    static void DuplicateMain()
    {
		string filepath = AssetDatabase.GetAssetPath(Selection.activeObject);
		string origName = Path.GetFileName(filepath);
		string folder = Path.GetDirectoryName(filepath);
		Directory.CreateDirectory(folder + "/duplicated");
		var newNames = character_names.Select(e=>(e + ".unity3d"));//.Where(e=>(e!=origName));
		foreach(string e in newNames)
			System.IO.File.Copy(filepath,folder + "/duplicated/" + e);
	}
	
	[MenuItem("Custom/DuplicateMini")]
    static void DuplicateMini()
    {
		string filepath = AssetDatabase.GetAssetPath(Selection.activeObject);
		string origName = Path.GetFileName(filepath);
		string folder = Path.GetDirectoryName(filepath);
		Directory.CreateDirectory(folder + "/duplicated");
		var newNames = character_names.Select(e=>(e + "_mini.unity3d"));//.Where(e=>(e!=origName));
		foreach(string e in newNames)
			System.IO.File.Copy(filepath,folder + "/duplicated/" + e);
	}*/
	
}