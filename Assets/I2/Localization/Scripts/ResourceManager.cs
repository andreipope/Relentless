using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace I2.Loc
{
	public class ResourceManager : MonoBehaviour 
	{
		#region Singleton
		public static ResourceManager pInstance
		{
			get {
				bool changed = mInstance==null;

				if (mInstance==null)
					mInstance = (ResourceManager)Object.FindObjectOfType(typeof(ResourceManager));

				if (mInstance==null)
				{
					GameObject GO = new GameObject("I2ResourceManager", typeof(ResourceManager));
					GO.hideFlags = GO.hideFlags | HideFlags.HideAndDontSave;	// Only hide it if this manager was autocreated
					mInstance = GO.GetComponent<ResourceManager>();
				}

				if (changed && Application.isPlaying)
					DontDestroyOnLoad(mInstance.gameObject);

				return mInstance;
			}
		}
		static ResourceManager mInstance;

		#endregion

		#region Management

        private void Awake()
        {
            SceneManager.sceneLoaded += SceneManager_SceneLoaded;   
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_SceneLoaded; 
        }

        void SceneManager_SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            LocalizationManager.UpdateSources();
        }

        #endregion

        #region Assets

        public Object[] Assets;

		// This function tries finding an asset in the Assets array, if not found it tries loading it from the Resources Folder
		public T GetAsset<T>( string Name ) where T : Object
		{
			T Obj = FindAsset( Name ) as T;
			if (Obj!=null)
				return Obj;

			return LoadFromResources<T>( Name );
		}

		Object FindAsset( string Name )
		{
			if (Assets!=null)
			{
				for (int i=0, imax=Assets.Length; i<imax; ++i)
					if (Assets[i]!=null && Assets[i].name == Name)
						return Assets[i];
			}
			return null;
		}

		public bool HasAsset( Object Obj )
		{
			if (Assets==null)
				return false;
			return System.Array.IndexOf (Assets, Obj) >= 0;
		}

		#endregion

		#region Resources Cache

		// This cache is kept for a few moments and then cleared
		// Its meant to avoid doing several Resource.Load for the same Asset while Localizing 
		// (e.g. Lot of labels could be trying to Load the same Font)
		Dictionary<string, Object> mResourcesCache = new Dictionary<string, Object>(); // This is used to avoid re-loading the same object from resources in the same frame
		bool mCleaningScheduled = false;

		public T LoadFromResources<T>( string Path ) where T : Object
		{
			if (string.IsNullOrEmpty(Path))
				return null;
			
			Object Obj;
			// Doing Resource.Load is very slow so we are catching the recently loaded objects
			if (mResourcesCache.TryGetValue(Path, out Obj) && Obj!=null)
			{
				return Obj as T;
			}

			T obj = null;

			if (Path.EndsWith("]", System.StringComparison.OrdinalIgnoreCase))	// Handle sprites (Multiple) loaded from resources :   "SpritePath[SpriteName]"
			{
				int idx = Path.LastIndexOf("[", System.StringComparison.OrdinalIgnoreCase);
				int len = Path.Length-idx-2;
				string MultiSpriteName = Path.Substring(idx+1, len);
				Path = Path.Substring(0, idx);
				
				T[] objs = Resources.LoadAll<T>(Path);
				for (int j=0, jmax=objs.Length; j<jmax; ++j)
					if (objs[j].name.Equals(MultiSpriteName))
					{
						obj = objs[j];
						break;
					}
			}
			else
				obj = Resources.Load<T>(Path);

			mResourcesCache[Path] = obj;

			if (!mCleaningScheduled)
			{
				Invoke("CleanResourceCache", 0.1f);
				mCleaningScheduled = true;
			}
			return obj;
		}

		public void CleanResourceCache()
		{
			mResourcesCache.Clear();
			Resources.UnloadUnusedAssets();

			CancelInvoke();
			mCleaningScheduled = false;
		}

		#endregion
	}
}