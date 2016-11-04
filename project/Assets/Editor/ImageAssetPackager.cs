using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public static class ImageAssetPackager {
	
	
	[System.Serializable]
	public class ImageSizeData
	{
		public string Name {get; set;}
		public Vector2 Size {get; set;}
	}
	
    [MenuItem("Custom/Construct Image Bundle")]
    static void ImagePackage()
    {
		//IEnumerable<Object> files = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel).Where(e => is_image_file(e));
   		IEnumerable<Object> files = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets).Where(e => is_image_file(e));
		
		List<ImageSizeData> index = new List<ImageSizeData>();
		
		foreach(Object e in files)
		{
			Texture2D tex = e as Texture2D;
			CharacterPreprocessor.set_texture_for_reading(tex);
			index.Add(new ImageSizeData(){Name = tex.name,Size = new Vector3(tex.width,tex.height)});
			CharacterPreprocessor.set_texture_for_render(tex);
		}
		List<Object> assets = files.ToList();
		
		//serialize the index
		AssetDatabase.ImportAsset("Assets/INDEX.txt");
        TextAsset cdtxt = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/INDEX.txt", typeof(TextAsset));
        System.IO.Stream stream = System.IO.File.Open(AssetDatabase.GetAssetPath(cdtxt), System.IO.FileMode.Create);
        System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(List<ImageSizeData>));
        xs.Serialize(stream, index);
        stream.Close();
		AssetDatabase.ImportAsset("Assets/INDEX.txt");
		cdtxt = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/INDEX.txt", typeof(TextAsset));
		
		//Debug.Log ("index: " + cdtxt.text);
		assets.OrderBy(e=>e.name).ToList().ForEach(e => Debug.Log (e));
		
        assets.Add(cdtxt);
		BuildPipeline.BuildAssetBundle(
			Selection.activeObject, assets.ToArray(), 
			"Assets/Resources/IMAGES.unity3d", 
			BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, 
			EditorUserBuildSettings.activeBuildTarget);//, BuildOptions.UncompressedAssetBundle);//, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
    }

	public static bool is_image_file(Object aObj)
    {
        return System.IO.Path.GetExtension(AssetDatabase.GetAssetPath(aObj)) == ".png";
    }
}