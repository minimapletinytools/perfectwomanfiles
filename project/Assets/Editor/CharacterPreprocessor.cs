using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class CharacterPreprocessor {
    static List<string> sLimbs = new List<string>()
    {
        "HEAD",
        "LLA",
        "LLL",
        "LUA",
        "LUL",
        "RLA",
        "RLL",
        "RUA",
        "RUL",
        "TORSO",
        "WAIST"
    };
    public static List<string> sExpected = new List<string>()
    {
        "POSITIONS.png",
        //"BACKGROUND.png",
    };

    static CharacterPreprocessor()
    {
		//character no longer requires limbs
        //sExpected.AddRange(sLimbs.ConvertAll<string>(s => s + "_A.png"));
        //sExpected.AddRange(sLimbs.ConvertAll<string>(s => s + "_B.png"));
    }


    //TODO test me...
	//note this depends on previous computation of sizes from create_main_character
    static void create_mini_character(Object aMain, IEnumerable<Object> aObjects, CharacterData.CharacterDataSizes aSizes)
    {
        float scaleAmount = 0.1f;

        IEnumerable<Object> aImages = aObjects.Where(f => (sLimbs.Contains(strip_to_root(f)) && !is_B_image(f))).OrderBy(f => f.name);
        foreach (Object f in aImages)
        {
            Texture2D tex = f as Texture2D;
            set_texture_for_reading(tex);
			
			//we could also recompute the sizes...
			//aSizes.mLimbSizes.Add(new Vector2(((Texture2D)f).width, ((Texture2D)f).height));

            tex.Resize((int)(tex.width * scaleAmount), (int)(tex.height * scaleAmount));
			
			
			//actually, I will use a custom shader for this instead so this step is not necessary
            /*Color[] colors = tex.GetPixels();
            for (int i = 0; i < colors.Length; i++)
                if (colors[i].a != 0)
                    colors[i] = new Color(0.5f, 0.5f, 0.5f, colors[i].a);
            tex.SetPixels(colors);*/
			
			//no need to dot his here since we know textures are small
			set_texture_for_render(tex);
        }
		
		for(int i = 0; i < aSizes.mMountingPositions.Count; i++)
			for(int j = 0; j < aSizes.mMountingPositions[i].Count; j++)	
				aSizes.mMountingPositions[i][j]*=scaleAmount;
		
		//scale sizes from previously computed values
        for (int i = 0; i < aSizes.mLimbSizes.Count; i++)
            aSizes.mLimbSizes[i] = aSizes.mLimbSizes[i] * scaleAmount;

        //package
        TextAsset cdtxt = serialize_cd(aSizes);

        IEnumerable<Object> package = aObjects.Where(f => !is_B_image(f)).Where(f => aImages.Contains(f));
        List<Object> assets = new List<Object>();
        foreach (Object f in package)
            assets.Add(f);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(cdtxt));
        cdtxt = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/CD.txt", typeof(TextAsset));
        assets.Add(cdtxt);
        //Debug.Log(cdtxt.text);
        BuildPipeline.BuildAssetBundle(aMain, assets.ToArray(), "Assets/Resources/" + aMain.name + "_mini.unity3d", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, EditorUserBuildSettings.activeBuildTarget);//, BuildOptions.UncompressedAssetBundle);//, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
    }

    static CharacterData.CharacterDataSizes create_main_character(Object aMain, IEnumerable<Object> aObjects)
    {

        string output = "";

        List<Object> valueList = aObjects.ToList();
        CharacterData.CharacterDataSizes cd = new CharacterData.CharacterDataSizes();
        cd.mName = aMain.name;

        //first process <limb>_B images
        IEnumerable<Object> bImages = valueList.Where(f => (sLimbs.Contains(strip_to_root(f)) && is_B_image(f))).OrderBy(f => strip_to_root(f));
		//int[] desiredSizes = new int[]{1,1,1,2,2,1,1,2,2,4,3}; 
		int[] desiredSizes = new int[]{2,2,2,2,2,2,2,2,2,4,3}; //this one has limb ends and head end
		int counter = 0;
        foreach (Object f in bImages)
        {
			//Debug.Log ("processing " + f.name);
            Texture2D limbProcessing = f as Texture2D;
            set_texture_for_reading(limbProcessing);
            cd.mMountingPositions.Add(get_limb_positions_in_order(limbProcessing));
			if(desiredSizes[counter] == 2)
			{
				foreach(var e in cd.mMountingPositions.Last())
				{
					//Debug.Log(e);
				}
			}
			if(desiredSizes[counter] != cd.mMountingPositions.Last().Count)
				Debug.Log ("issue with " + f.name + " need " + desiredSizes[counter] + " found " + cd.mMountingPositions.Last().Count);
			counter++;
        }
		
		//record <limb>_A  image sizes
		IEnumerable<Object> aImages = aObjects.Where(f => (sLimbs.Contains(strip_to_root(f)) && !is_B_image(f))).OrderBy(f => f.name);
        foreach (Object f in aImages)
        {

            Texture2D tex = f as Texture2D;
            set_texture_for_reading(tex);
			cd.mLimbSizes.Add(new Vector2(((Texture2D)f).width, ((Texture2D)f).height));
			set_texture_for_render(tex);
		}

        //parse POSITIONS.png
		try{
	        Object positionImage = valueList.Single(f => strip_to_root(f) == "POSITIONS");
	        Texture2D bgProcessing = positionImage as Texture2D;
	        set_texture_for_reading(bgProcessing);
			try{cd.mOffset = get_character_position(bgProcessing);}
			catch{Debug.Log ("no character position found, this must be the sunset bundle");}
		

			//EFFECTS
			TextAsset effects;
			try{ effects = valueList.Single(f => strip_to_root(f) == "EFFECTS") as TextAsset;}
			catch{ effects = null;}
			
			//BACKGROUND STUFF
			var staticElts = valueList.Where(
				f => strip_to_root(f).StartsWith("FG") || 
				strip_to_root(f).StartsWith("BG") || 
				strip_to_root(f).StartsWith("CUTSCENE") ||
				strip_to_root(f).StartsWith("GIFT_") ||
				strip_to_root(f).StartsWith("SUNSET_") ||		//special for sunset, we will read, font, sun, and score label from another bundle
                strip_to_root(f).StartsWith("START_") 
				);
			foreach(var e in staticElts) Debug.Log (e.name);
			foreach(var img in staticElts)
			{
				CharacterData.ImageSizeOffsetAnimationData isoad = new CharacterData.ImageSizeOffsetAnimationData();
				if(effects != null)
					isoad.AnimationEffect = find_effect_string(effects,img.name);
				isoad.Name = img.name;
				set_texture_for_reading((Texture2D)img);
				isoad.Size = new Vector2(((Texture2D)img).width,((Texture2D)img).height);
				isoad.Offset = find_position(bgProcessing,img.name);
				set_texture_for_render((Texture2D)img);
				cd.mStaticElements.Add(isoad);
			}


			
			
			
			//old background parsing
	        IEnumerable<Object> bgImage = valueList.Where(f => strip_to_root(f) == "BACKGROUND");
	        foreach (Object f in bgImage)
	        {
	            set_texture_for_reading((Texture2D)f);
	            cd.mBackSize = (new Vector2(((Texture2D)f).width, ((Texture2D)f).height));
	        }
			if(bgImage.Count() > 0)
	        	set_texture_for_render((Texture2D)bgImage.First());

		}catch(UnityException e)
		{
			Debug.Log ("problem with POSITIONS, probably mini character " + e.StackTrace);
		}
		
        //package
        TextAsset cdtxt = serialize_cd(cd);
        IEnumerable<Object> package = aObjects.Where(f => !is_B_image(f)).Where(f => strip_to_root(f) != "POSITIONS");//.Where(f => strip_to_root(f) != "AUDIO");
        List<Object> assets = new List<Object>();
        foreach (Object f in package)
            assets.Add(f);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(cdtxt));
        cdtxt = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/CD.txt", typeof(TextAsset));
        assets.Add(cdtxt);
        //Debug.Log(cdtxt.text);
#if UNITY_XBOXONE
        BuildPipeline.BuildAssetBundle(aMain, assets.ToArray(), "Assets/Resources/XB1" + aMain.name + ".unity3d", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, EditorUserBuildSettings.activeBuildTarget);//, BuildOptions.UncompressedAssetBundle);//, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
#else
        BuildPipeline.BuildAssetBundle(aMain, assets.ToArray(), "Assets/Resources/" + aMain.name + ".unity3d", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, EditorUserBuildSettings.activeBuildTarget);//, BuildOptions.UncompressedAssetBundle);//, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
#endif
        //Debug.Log(output);
        return cd;
    }

    [MenuItem("Custom/Construct Character Bundle")]
    static void ConvertTest()
    {


        IEnumerable<Object> folders = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel).Where(e => is_folder(e));
        Dictionary<Object, IEnumerable<Object>> fileMap = new Dictionary<Object, IEnumerable<Object>>();
		Debug.Log ("trying to process " + folders.Count() + " folders");
        foreach (Object e in folders)
        {
            Object checkAgainst = e;
            fileMap[e] = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets).Where<Object>(delegate(Object f) { return is_asset_in_directory(f, checkAgainst); });
        }
        Dictionary<Object, IEnumerable<Object>> filteredMap = new Dictionary<Object, IEnumerable<Object>>();
        foreach (KeyValuePair<Object, IEnumerable<Object>> e in fileMap)
        {
			if (is_character(e.Value) || is_mini_character(e.Value))
				filteredMap.Add(e.Key, e.Value);
        }
        Debug.Log("Processing " + filteredMap.Count() + " characters.");
        foreach (KeyValuePair<Object, IEnumerable<Object>> e in filteredMap)
        {
			Debug.Log ("processing "+e.Key);
            //Debug.Log(e.Key.name + " " +  AssetDatabase.GetAssetPath(e.Value.First()));
			try{
            	CharacterData.CharacterDataSizes cd = create_main_character(e.Key, e.Value);
            	//create_mini_character(e.Key, e.Value, cd);
			}
			catch(UnityException f)
			{
				Debug.Log("error processing character " + e.Key);
				throw f;
			}
			Debug.Log ("sucssefully processed " + e.Key);
        }
    }
	
	
	public static TextAsset serialize_cd(CharacterData.CharacterDataSizes aData)
	{
		//package
		AssetDatabase.ImportAsset("Assets/CD.txt");
		TextAsset cdtxt = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/CD.txt", typeof(TextAsset));
		//Debug.Log (cdtxt.text);
        System.IO.Stream stream = System.IO.File.Open(AssetDatabase.GetAssetPath(cdtxt), System.IO.FileMode.Create);
        System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(CharacterData.CharacterDataSizes));
        xs.Serialize(stream, aData);
        //System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //bFormatter.Serialize(stream, aData);
        stream.Close();
        return cdtxt;
    }

    public static Vector3 get_character_position(Texture2D aTex)
    {
        return find_first_color(new Color32(0, 0, 0, 255), aTex);
    }
    public static List<Vector3> get_background_positions_in_order(Texture2D aTex)
    {
        List<Vector3> r = new List<Vector3>();
        for (int i = 0; i < 255/5; i++)
        {
            try
            {
                r.Add(find_first_color(new Color32(255,0,(byte)(5*i),255), aTex));
            }
            catch
            {
                break;
            }
        }
        return r;
    }
    public static List<Vector3> get_foreground_positions_in_order(Texture2D aTex)
    {
        List<Vector3> r = new List<Vector3>();
        for (int i = 0; i < 255/5; i++)
        {
            try
            {
                r.Add(find_first_color(new Color32(0, 255,(byte)(5 * i), 255), aTex));
            }
            catch
            {
                break;
            }
        }
        return r;
    }
    public static List<Vector3> get_limb_positions_in_order(Texture2D aTex)
    {
        List<Vector3> r = new List<Vector3>();
        for (int i = 0; i < 4; i++)
        {
            try
            {
                r.Add(get_attachment_point(i, aTex));
            }
            catch
            {
                break;
            }
        }
        return r;
    }
	
	public static void set_audio_for_2D(AudioClip aClip)
	{
		string path = AssetDatabase.GetAssetPath(aClip);
        AudioImporter audioImporter = AudioImporter.GetAtPath(path) as AudioImporter;
		audioImporter.threeD = false;
		//audioImporter.loadType = AudioClipLoadType.Streaming;
		AssetDatabase.ImportAsset(path);
		
	}

    //texture import setting options
    public static void set_texture_for_render(Texture2D aTex)
    {
        string path = AssetDatabase.GetAssetPath(aTex);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        textureImporter.textureType = TextureImporterType.Image;
        textureImporter.mipmapEnabled = false;
		textureImporter.isReadable = false;
        textureImporter.filterMode = FilterMode.Point; //this does not look the way it's suppose to..
       	textureImporter.textureFormat = TextureImporterFormat.RGBA32;
		//textureImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
        textureImporter.normalmap = false;
		//textureImporter.maxTextureSize = 1024;
        textureImporter.maxTextureSize = 2048;
		textureImporter.npotScale = TextureImporterNPOTScale.None;
		textureImporter.wrapMode = TextureWrapMode.Clamp;
        TextureImporterSettings st = new TextureImporterSettings();
        textureImporter.ReadTextureSettings(st);
        textureImporter.SetTextureSettings(st);
        AssetDatabase.ImportAsset(path);
    }

    public static void set_texture_for_reading(Texture2D aTex)
    {
        string path = AssetDatabase.GetAssetPath(aTex);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        textureImporter.textureType = TextureImporterType.Advanced;
        textureImporter.npotScale = TextureImporterNPOTScale.None;
        textureImporter.isReadable = true;
        textureImporter.mipmapEnabled = false;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureFormat = TextureImporterFormat.RGBA32;
        textureImporter.normalmap = false;
        textureImporter.maxTextureSize = 4096;
        TextureImporterSettings st = new TextureImporterSettings();
        textureImporter.ReadTextureSettings(st);
        st.wrapMode = TextureWrapMode.Clamp;
        textureImporter.SetTextureSettings(st);
        AssetDatabase.ImportAsset(path);
    }

   
	
	//new asset stuff ugg
	public static string find_effect_string(TextAsset aEffects, string aFilename)
	{
		var result = System.Text.RegularExpressions.Regex.Split(aEffects.text, "\r\n|\r|\n");
		foreach(string e in result)
		{
			string[] parse = e.Split(null);
			if(parse[0] == aFilename)
			{
				if(parse.Length > 1)
				{
					string r = "";
					for(int i = 1; i < parse.Length; i++)
						r += parse[i] + " ";
					return r;
				}
				else return "";
			}
		}
		return "";
		
	}
	public static Vector3 find_position(Texture2D aPosition, string aFilename)
	{
		if(aFilename.StartsWith("CUTSCENE"))
		{
			int firstIndex = System.Convert.ToInt32(aFilename[8]+"");
			if(firstIndex == 4) //CUTSCENE4 actually uses CUTSCENE2 colors
				firstIndex = 2;
			int secondIndex = System.Convert.ToInt32(aFilename.Substring(10)+"");
			Color32 find = new Color32((byte)(firstIndex*5), (byte)(secondIndex*5),255,255);
			//Debug.Log (find);
			try{
				return find_first_color(find,aPosition);
			}
			catch {
				
				Debug.Log ("error finding " + aFilename);
				return Vector3.zero;
				//throw new UnityException("Couldn't find " + aFilename + " in positions");
			}
		}
		else if (aFilename.StartsWith("FG") || aFilename.StartsWith("BG"))
		{
			int index = System.Convert.ToInt32(aFilename.Substring(3)+"");
			Color32 find = aFilename.StartsWith("BG") ? new Color32(255,0,(byte)(5*(index-1)),255) : new Color32(0,255,(byte)(5*(index-1)),255);
			try{
				return find_first_color(find,aPosition);
			}
			catch {
				Debug.Log ("error finding " + aFilename);
				return Vector3.zero;
				//throw new UnityException("Couldn't find " + aFilename + " in positions");
			}
		}
		else if(aFilename.Contains("SUNSET"))
		{
			string[] findMe = new string[]{"05","16","27","34","45","60","85","110","999"};
			for(int i = 0; i < findMe.Length; i++)
			{
				if(aFilename.Contains(findMe[i]))
				{
					Color32 find = new Color32(255,0,(byte)(5*(i+1)),255);
					try{
						return find_first_color(find,aPosition);
					}
					catch {
						Debug.Log ("error finding " + aFilename);
						return Vector3.zero;
						throw new UnityException("Couldn't find " + aFilename + " in positions");
					}
				}
			}
		}
		else if(aFilename.Contains("GIFT"))
		{
			Debug.Log ("adding " + aFilename);	
			string[] findMe = new string[]{"05","16","27","34","45","60","85","110","999"};
			for(int i = 0; i < findMe.Length; i++)
			{
				if(aFilename.Contains(findMe[i]))
				{
					Color32 find = new Color32(255,0,(byte)(5*(i+1)),255);
					try{
						return find_first_color(find,aPosition);
					}
					catch {
						Debug.Log ("error finding " + aFilename);
						return Vector3.zero;
						throw new UnityException("Couldn't find " + aFilename + " in positions");
					}
				}
			}
		}
        else if (aFilename.Contains("START")) //this is a dud
        {
            return new Vector3();
        }
		throw new UnityException("Invalid filename");
	}
	
	
	
    //other asset options
    public static bool is_B_image(Object aObj)
    {
        string r = System.IO.Path.GetFileNameWithoutExtension(aObj.name);
        if (r[r.Length - 1] == 'B')
            return true;
        return false;
    }
	public static bool is_mini_character(IEnumerable<Object> aObjects)
	{
		List<string> expected = new List<string>();
		expected.AddRange(sLimbs.ConvertAll<string>(s => s + "_A.png"));
		expected.AddRange(sLimbs.ConvertAll<string>(s => s + "_B.png"));
		foreach (string e in expected)
		{
			bool pass = false;
			foreach (Object f in aObjects)
			{
				if (System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(f)).Equals(e))
				{
					pass = true;
					break;
				}
			}
			if (!pass)
			{
				Debug.Log("missing " + e);
				return false;
			}
		}
		return true;
	}
	public static bool is_character(IEnumerable<Object> aObjects)
	{
		//aObjects.ToList().ForEach(f => Debug.Log(f));
		foreach (string e in sExpected)
        {
            bool pass = false;
            foreach (Object f in aObjects)
            {
                if (System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(f)).Equals(e))
                {
                    pass = true;
                    break;
                }
            }
            if (!pass)
			{
				Debug.Log("missing " + e);
				return false;
			}
        }
        return true;
    }

    //asset/directory operations
    public static string strip_to_root(Object aObj)
    {
        string r = System.IO.Path.GetFileNameWithoutExtension(aObj.name);
        if (r.Length >= 2 && r[r.Length - 2] == '_')
            return r.Substring(0, r.Length - 2);
        return r;
    }
    public static bool is_asset_in_directory(Object aObj, Object aDir)
    {
        bool r = is_asset_in_directory(aObj,AssetDatabase.GetAssetPath(aDir));
		return r;
    }
    public static bool is_asset_in_directory(Object aObj, string aDir)
    {
        return AssetDatabase.GetAssetPath(aObj).StartsWith(aDir);
    }
    public static bool is_asset(Object aObj)
    {
        return AssetDatabase.Contains(aObj);
    }
    public static bool is_folder(Object aObj)
    {
        if (!is_asset(aObj))
            return false;
        return System.IO.Directory.Exists(AssetDatabase.GetAssetPath(aObj));
    }


    //image operations 
    static bool is_same_color(Color32 c1, Color32 c2)
    {
        return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b &&
			(c1.a > 100 && c2.a > 100);
    }
    static Vector3 index_to_position(int i, Texture2D aTex)
    {

        int x = i % aTex.width - aTex.width / 2;
        int y = i / aTex.width - aTex.height / 2;

        return new Vector3(-convert_units(x), convert_units(y));
    }
    public static Vector3 find_first_color(Color32 c, Texture2D aTex)
    {

        Color32[] colors = aTex.GetPixels32();
        for (int i = 0; i < colors.Length; i++)
        {
            if (is_same_color(colors[i], c))
            {

                return index_to_position(i, aTex);
            }
        }
        //return Vector3.zero;
        throw new UnityException("color " + c.ToString() + " not found in texture " + aTex.name);
    }

    public static Vector3 get_attachment_point(int aId, Texture2D aTex)
    {
        Color32 c;
        switch (aId)
        {
            case 0:
                c = new Color32(255, 0, 0, 255);
                break;
            case 1:
                c = new Color32(0, 255, 0, 255);
                break;
            case 2:
                c = new Color32(0, 0, 255, 255);
                break;
            case 3:
                c = new Color32(255, 255, 0, 255);
                break;
            default:
                return Vector3.zero;
        }
        return find_first_color(c, aTex);
    }
    public static float convert_units(float val)
    {
        return val;
    }








	
	//TODO DELETE
	//[MenuItem("Custom/Construct Graveyard Bundle")]
	static void Graveyard()
	{
		if (Selection.activeObject.name != "999")
		{
			Debug.Log("graveyard not selected");
			return;
		}
		List<Object> valueList = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets).ToList();
		CharacterData.CharacterDataSizes cd = new CharacterData.CharacterDataSizes();
		cd.mName = Selection.activeObject.name;
		
		
		//parse POSITIONS.png
		//TODO special colors here
		Object positionImage = valueList.Single(f => strip_to_root(f) == "POSITIONS");
		Texture2D bgProcessing = positionImage as Texture2D;
		set_texture_for_reading(bgProcessing);
		
		//new parsing
		TextAsset effects;
		try{ effects = valueList.Single(f => strip_to_root(f) == "EFFECTS") as TextAsset;}
		catch{ effects = null;}
		var staticElts = valueList.Where(f => strip_to_root(f).StartsWith("FG") || strip_to_root(f).StartsWith("BG") || strip_to_root(f).StartsWith("CUTSCENE"));
		//foreach(var e in staticElts) Debug.Log (e.name);
		foreach(var img in staticElts)
		{
			CharacterData.ImageSizeOffsetAnimationData isoad = new CharacterData.ImageSizeOffsetAnimationData();
			if(effects != null)
				isoad.AnimationEffect = find_effect_string(effects,img.name);
			isoad.Name = img.name;
			set_texture_for_reading((Texture2D)img);
			isoad.Size = new Vector2(((Texture2D)img).width,((Texture2D)img).height);
			isoad.Offset = find_position(bgProcessing,img.name);
			set_texture_for_render((Texture2D)img);
			cd.mStaticElements.Add(isoad);
		}
		
		//old backgronud parsing
		IEnumerable<Object> bgImage = valueList.Where(f => strip_to_root(f) == "BACKGROUND");
		foreach (Object f in bgImage)
		{
			set_texture_for_reading((Texture2D)f);
			cd.mBackSize = (new Vector2(((Texture2D)f).width, ((Texture2D)f).height));
		}
		set_texture_for_render((Texture2D)bgImage.First());
		
		//set audio to 2D
		foreach( Object f in valueList.Where (g=>strip_to_root(g) == "AUDIO"))
			set_audio_for_2D((AudioClip)f);
		
		//package
		TextAsset cdtxt = serialize_cd(cd);
		IEnumerable<Object> package = valueList.Where(f => !is_B_image(f)).Where(f => strip_to_root(f) != "POSITIONS");//.Where(f => strip_to_root(f) != "AUDIO");
		List<Object> assets = new List<Object>();
		foreach (Object f in package)
			assets.Add(f);
		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(cdtxt));
		cdtxt = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/CD.txt", typeof(TextAsset));
		assets.Add(cdtxt);
		BuildPipeline.BuildAssetBundle(Selection.activeObject, assets.ToArray(), "Assets/Resources/" + cd.mName + ".unity3d", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, EditorUserBuildSettings.activeBuildTarget);//, BuildOptions.UncompressedAssetBundle);//, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
		
	}

}
