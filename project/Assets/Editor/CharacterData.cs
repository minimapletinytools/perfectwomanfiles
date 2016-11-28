using UnityEngine;
using System.Collections.Generic;

public class CharacterData{
	
	[System.Serializable]
	public class ImageSizeOffsetAnimationData
	{
		public string Name {get; set;}
		public string AnimationEffect {get; set;}
		public Vector3 Offset {get; set;}
		public Vector2 Size {get; set;}
	}
	
    [System.Serializable]
    public class CharacterDataSizes
    {
        //ordered as in CharacterPreprocessor.sLimbs
        public List<List<Vector3>> mMountingPositions = new List<List<Vector3>>();
		public List<Vector2> mLimbSizes = new List<Vector2>();
		
		public List<ImageSizeOffsetAnimationData> mStaticElements = new List<ImageSizeOffsetAnimationData>();

        public Vector2 mBackSize = new Vector2();
        public Vector2 mOffset = new Vector2();
        public string mName = "";
    }

	
	/* the old one DELTE
    [System.Serializable]
    public class CharacterDataSizes
    {
        //ordered as in CharacterPreprocessor.sLimbs
        public List<List<Vector3>> mMountingPositions = new List<List<Vector3>>();
        public List<Vector3> mBackgroundPositions = new List<Vector3>();
        public List<Vector3> mForegroundPositions = new List<Vector3>();

        public List<Vector2> mLimbSizes = new List<Vector2>();
        public List<Vector2> mBackgroundSizes = new List<Vector2>();
        public List<Vector2> mForegroundSizes = new List<Vector2>();

        public Vector2 mBackSize = new Vector2();

        public Vector2 mOffset = new Vector2();

        public string mName = "";
    }*/
}
