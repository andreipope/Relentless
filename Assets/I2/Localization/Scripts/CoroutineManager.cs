using UnityEngine;
using System.Collections;

namespace I2.Loc
{
	// This class is used to spawn the coroutine for Import Google Spreadsheet
	public class CoroutineManager : MonoBehaviour 
	{
		public static CoroutineManager pInstance
		{
			get{
				if (mInstance==null)
				{
					GameObject GO = new GameObject("GoogleTranslation");
					GO.hideFlags = GO.hideFlags | HideFlags.HideAndDontSave;
					mInstance = GO.AddComponent<CoroutineManager>();
				}
				return mInstance;
			}
		}
		static CoroutineManager mInstance;
	}
}
