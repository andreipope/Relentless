using UnityEngine;
using System.Collections;

namespace I2
{
	// This class is used to spawn coroutines from outside of MonoBehaviors
	public class CoroutineManager : MonoBehaviour 
	{
		public static Coroutine Start(IEnumerator coroutine)
		{
			if (mInstance==null) 
			{
				GameObject GO = new GameObject("_Coroutiner");
				GO.hideFlags = GO.hideFlags | HideFlags.HideAndDontSave;
				mInstance = GO.AddComponent<CoroutineManager>();
				if (Application.isPlaying)
					Object.DontDestroyOnLoad (GO);
			}
			
			return mInstance.StartCoroutine(coroutine);
		}
		private static CoroutineManager mInstance;
	}
}
