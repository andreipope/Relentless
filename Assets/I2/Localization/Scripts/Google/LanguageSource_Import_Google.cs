using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace I2.Loc
{
	public partial class LanguageSource
	{
		#region Variables

		public string Google_WebServiceURL;
		public string Google_SpreadsheetKey;
		public string Google_SpreadsheetName;
		public string Google_LastUpdatedVersion;

		public enum eGoogleUpdateFrequency { Always, Never, Daily, Weekly, Monthly }
		public eGoogleUpdateFrequency GoogleUpdateFrequency = eGoogleUpdateFrequency.Weekly;

		public float GoogleUpdateDelay = 5; // How many second to delay downloading data from google (to avoid lag on the startup)

		public event Action<LanguageSource> Event_OnSourceUpdateFromGoogle;
		
		#endregion

		#region Connection to Web Service 

		public void Delayed_Import_Google()
		{
			Import_Google ();
		}

		public void Import_Google_FromCache()
		{
			if (!Application.isPlaying)
					return;
						
			string PlayerPrefName = GetSourcePlayerPrefName();
			string I2SavedData = PlayerPrefs.GetString("I2Source_"+PlayerPrefName, null);
			if (string.IsNullOrEmpty (I2SavedData))
				return;

			//--[ Compare with current version ]-----
			bool shouldUpdate = false;
			string savedSpreadsheetVersion = Google_LastUpdatedVersion;
			if (PlayerPrefs.HasKey("I2SourceVersion_"+PlayerPrefName))
			{
				savedSpreadsheetVersion = PlayerPrefs.GetString("I2SourceVersion_"+PlayerPrefName, Google_LastUpdatedVersion);
//				Debug.Log (Google_LastUpdatedVersion + " - " + savedSpreadsheetVersion);
				shouldUpdate = savedSpreadsheetVersion.CompareTo( Google_LastUpdatedVersion )>0;
			}

			if (!shouldUpdate)
			{
				PlayerPrefs.DeleteKey("I2Source_"+PlayerPrefName);
				PlayerPrefs.DeleteKey("I2SourceVersion_"+PlayerPrefName);
				return;
			}

			Google_LastUpdatedVersion = savedSpreadsheetVersion;

			Debug.Log ("[I2Loc] Using Saved (PlayerPref) data in 'I2Source_"+PlayerPrefName+"'" );
			Import_Google_Result(I2SavedData, eSpreadsheetUpdateMode.Replace);
		}

		public void Import_Google( bool ForceUpdate = false)
		{
			if (GoogleUpdateFrequency==eGoogleUpdateFrequency.Never)
				return;

			#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
			#endif

			string PlayerPrefName = GetSourcePlayerPrefName();
			if (!ForceUpdate && GoogleUpdateFrequency!=eGoogleUpdateFrequency.Always)
			{
				string sTimeOfLastUpdate = PlayerPrefs.GetString("LastGoogleUpdate_"+PlayerPrefName, "");
				DateTime TimeOfLastUpdate;
				if (DateTime.TryParse(sTimeOfLastUpdate, out TimeOfLastUpdate))
				{
					double TimeDifference = (DateTime.Now-TimeOfLastUpdate).TotalDays;
					switch (GoogleUpdateFrequency)
					{
						case eGoogleUpdateFrequency.Daily 	: if (TimeDifference<1) return;
																break;
						case eGoogleUpdateFrequency.Weekly 	: if (TimeDifference<8) return;
																break;
						case eGoogleUpdateFrequency.Monthly : if (TimeDifference<31) return;
																break;
					}
				}
			}
			PlayerPrefs.SetString("LastGoogleUpdate_"+PlayerPrefName, DateTime.Now.ToString());

			//--[ Checking google for updated data ]-----------------
			CoroutineManager.pInstance.StartCoroutine(Import_Google_Coroutine());
		}

		string GetSourcePlayerPrefName()
		{
			// If its a global source, use its name, otherwise, use the name and the level it is in
			if (System.Array.IndexOf(LocalizationManager.GlobalSources, name)>=0)
				return name;
			else
			{
				#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
				return Application.loadedLevelName + "_" + name;
				#else
				return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name+"_"+name;
				#endif
			}
		}

		IEnumerator Import_Google_Coroutine()
		{
			WWW www = Import_Google_CreateWWWcall();
			if (www==null)
				yield break;

			while (!www.isDone)
				yield return null;

			//Debug.Log ("Google Result: " + www.text);
			if (string.IsNullOrEmpty(www.error) && www.text != "\"\"")
			{
				var errorMsg = Import_Google_Result(www.text, eSpreadsheetUpdateMode.Replace, true);
				if (string.IsNullOrEmpty(errorMsg))
				{
					if (Event_OnSourceUpdateFromGoogle!=null)
						Event_OnSourceUpdateFromGoogle(this);

					LocalizationManager.LocalizeAll();
					Debug.Log ("Done Google Sync");
				}
				else
				    Debug.Log ("Done Google Sync: source was up-to-date");
			}
			else
				Debug.Log ("Language Source was up-to-date with Google Spreadsheet");
		}

		public WWW Import_Google_CreateWWWcall( bool ForceUpdate = false )
		{
			#if UNITY_WEBPLAYER
			Debug.Log ("Contacting google translation is not yet supported on WebPlayer" );
			return null;
			#else

			if (!HasGoogleSpreadsheet())
				return null;

			string savedVersion = PlayerPrefs.GetString("I2SourceVersion_"+GetSourcePlayerPrefName(), Google_LastUpdatedVersion);
			if (savedVersion.CompareTo(Google_LastUpdatedVersion)>0)
				Google_LastUpdatedVersion = savedVersion;

			string query =  string.Format("{0}?key={1}&action=GetLanguageSource&version={2}", 
			                              Google_WebServiceURL,
			                              Google_SpreadsheetKey,
			                              ForceUpdate ? "0" : Google_LastUpdatedVersion);
			WWW www = new WWW(query);
			return www;
			#endif
		}

		public bool HasGoogleSpreadsheet()
		{
			return !string.IsNullOrEmpty(Google_WebServiceURL) && !string.IsNullOrEmpty(Google_SpreadsheetKey);
		}

		public string Import_Google_Result( string JsonString, eSpreadsheetUpdateMode UpdateMode, bool saveInPlayerPrefs = false )
		{
			string ErrorMsg = string.Empty;
            if (string.IsNullOrEmpty(JsonString) || JsonString == "\"\"")
			{
				return ErrorMsg;
			}

			int idxV = JsonString.IndexOf("version=");
			int idxSV = JsonString.IndexOf("script_version=");
			if (idxV<0 || idxSV<0)
			{
				return "Invalid Response from Google, Most likely the WebService needs to be updated";
			}

			idxV += "version=".Length;
			idxSV += "script_version=".Length;
			
			string newSpreadsheetVersion = JsonString.Substring(idxV, JsonString.IndexOf(",",idxV)-idxV);
			var scriptVersion = int.Parse( JsonString.Substring(idxSV, JsonString.IndexOf(",",idxSV)-idxSV));

			if (scriptVersion!=LocalizationManager.GetRequiredWebServiceVersion())
			{
				return "The current Google WebService is not supported.\nPlease, delete the WebService from the Google Drive and Install the latest version.";
			}

			//Debug.Log (Google_LastUpdatedVersion + " - " + newSpreadsheetVersion);
			if (!saveInPlayerPrefs && newSpreadsheetVersion.CompareTo(Google_LastUpdatedVersion) <= 0 )
			#if UNITY_EDITOR
				return "";
			#else
				return "LanguageSource is up-to-date";
			#endif

			if (saveInPlayerPrefs)
			{
				string PlayerPrefName = GetSourcePlayerPrefName();
				PlayerPrefs.SetString("I2Source_"+PlayerPrefName, JsonString);
				PlayerPrefs.SetString("I2SourceVersion_"+PlayerPrefName, newSpreadsheetVersion);
				PlayerPrefs.Save();
			}
			Google_LastUpdatedVersion = newSpreadsheetVersion;
			
			if (UpdateMode == eSpreadsheetUpdateMode.Replace)
					ClearAllData();

			int CSVstartIdx = JsonString.IndexOf("[i2category]");
			while (CSVstartIdx>0)
			{
				CSVstartIdx += "[i2category]".Length;
				int endCat = JsonString.IndexOf("[/i2category]", CSVstartIdx);
				string category = JsonString.Substring(CSVstartIdx, endCat-CSVstartIdx);
				endCat += "[/i2category]".Length;

				int endCSV = JsonString.IndexOf("[/i2csv]", endCat);
				string csv = JsonString.Substring(endCat, endCSV-endCat);

				CSVstartIdx = JsonString.IndexOf("[i2category]", endCSV);

				Import_I2CSV( category, csv, UpdateMode );
				
				// Only the first CSV should clear the Data
				if (UpdateMode == eSpreadsheetUpdateMode.Replace)
					UpdateMode = eSpreadsheetUpdateMode.Merge;
			}

#if UNITY_EDITOR
			if (!string.IsNullOrEmpty(ErrorMsg))
				UnityEditor.EditorUtility.SetDirty(this);
#endif
			return ErrorMsg;
		}

		#endregion
	}
}