using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables

		static Dictionary<string, string> mGoogleSpreadsheets = new Dictionary<string, string>();

		WWW mConnection_WWW;
		Action<string, string> mConnection_Callback;
		//float mConnection_TimeOut;

		string mConnection_Text = string.Empty;

		string mWebService_Status;

		#endregion
		
		#region GUI
		
		void OnGUI_Spreadsheet_Google()
		{
			GUILayout.Space(20);

#if UNITY_WEBPLAYER
			mConnection_Text = string.Empty;
			EditorGUILayout.HelpBox("Google Synchronization is not supported when in WebPlayer mode." + mConnection_Text, MessageType.Info);

			mProp_GoogleUpdateFrequency.enumValueIndex = mProp_GoogleUpdateFrequency.enumValueIndex;  // to avoid the warning "unused"
#else
			
			OnGUI_GoogleCredentials();
			
			OnGUI_ShowMsg();

			if (string.IsNullOrEmpty(mProp_Google_WebServiceURL.stringValue))
				return;

			if (mWebService_Status == "Offline")
				return;

			GUILayout.Space(20);

			GUI.backgroundColor = Color.Lerp(Color.gray, Color.white, 0.5f);
			GUILayout.BeginVertical("AS TextArea", GUILayout.Height (1));
			GUI.backgroundColor = Color.white;
				GUILayout.Space(10);
				OnGUI_GoogleSpreadsheetsInGDrive();
			GUILayout.EndVertical();

			if (mConnection_WWW!=null)
			{
				// Connection Status Bar
				int time = (int)((Time.realtimeSinceStartup % 2) * 2.5);
				string Loading = mConnection_Text + ".....".Substring(0, time);
				GUI.color = Color.gray;
				GUILayout.BeginHorizontal("AS TextArea");
				GUILayout.Label (Loading, EditorStyles.miniLabel);
				GUI.color = Color.white;
				if (GUILayout.Button("Cancel", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
					StopConnectionWWW();
				GUILayout.EndHorizontal();
				Repaint();
			}
			//else
			//	GUILayout.Space(10);


			GUI.changed = false;
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
					LanguageSource.eGoogleUpdateFrequency GoogleUpdateFrequency = (LanguageSource.eGoogleUpdateFrequency)mProp_GoogleUpdateFrequency.enumValueIndex;
					GoogleUpdateFrequency = (LanguageSource.eGoogleUpdateFrequency)EditorGUILayout.EnumPopup("Auto Update Frequency", GoogleUpdateFrequency, GUILayout.ExpandWidth(true));
					if (GUI.changed)
					mProp_GoogleUpdateFrequency.enumValueIndex = (int)GoogleUpdateFrequency;

					GUILayout.Space(10);
					GUILayout.Label("Delay:");
						mProp_GoogleUpdateDelay.floatValue = EditorGUILayout.FloatField(mProp_GoogleUpdateDelay.floatValue, GUILayout.Width(30));
					GUILayout.Label("secs");

				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(5);

			GUI.changed = false;
			bool OpenDataSourceAfterExport = EditorPrefs.GetBool("I2Loc OpenDataSourceAfterExport", true);

			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				OpenDataSourceAfterExport = GUILayout.Toggle(OpenDataSourceAfterExport, "Open Spreadsheet after Export");
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			if (GUI.changed)
			{
				GUI.changed = false;
				EditorPrefs.SetBool("I2Loc OpenDataSourceAfterExport", OpenDataSourceAfterExport);
			}

#endif

			GUILayout.Space(5);
		}
		
		void OnGUI_GoogleCredentials()
		{
			GUI.enabled = mConnection_WWW==null;

			GUI.changed = false;

			string WebServiceHelp = "The web service is a script running on the google drive where the spreadsheet you want to use is located.\nThat script allows the game to synchronize the localization even after the game is published.";

			GUILayout.BeginHorizontal();
				GUILayout.Label (new GUIContent("Web Service URL:", WebServiceHelp),  GUILayout.Width(110));

				GUI.SetNextControlName ("WebServiceURL");
				mProp_Google_WebServiceURL.stringValue = EditorGUILayout.TextField(mProp_Google_WebServiceURL.stringValue);

				if (!string.IsNullOrEmpty(mWebService_Status))
				{
					if (mWebService_Status=="Online")
					{
						GUI.color = Color.green;
						GUILayout.Label( "", GUILayout.Width(17));
						Rect r = GUILayoutUtility.GetLastRect(); r.xMin += 3; r.yMin-= 3; r.xMax+= 2; r.yMax+=2;
						GUI.Label( r, new GUIContent("\u2713", "Online"), EditorStyles.whiteLargeLabel);
						GUI.color = Color.white;
					}
					else
					if (mWebService_Status=="Offline")
					{
						GUI.color = Color.red;
						GUILayout.Label( "", GUILayout.Width(17));
						Rect r = GUILayoutUtility.GetLastRect(); r.xMin += 3; r.yMin-= 3; r.xMax+= 2; r.yMax+=2;
						GUI.Label( r, new GUIContent("\u2717", mWebService_Status), EditorStyles.whiteLargeLabel);
						GUI.color = Color.white;
					}
					else
					if (mWebService_Status=="UnsupportedVersion")
					{
						Rect rect = GUILayoutUtility.GetLastRect();
						float Width = 15;
						rect.xMin = rect.xMax+1;
						rect.xMax = rect.xMin + rect.height;
						GUI.DrawTexture( rect, GUI.skin.GetStyle("CN EntryWarn").normal.background);
						GUI.Label(rect, new GUIContent("", "The current Google WebService is not supported.\nPlease, delete the WebService from the Google Drive and Install the latest version."));
						GUILayout.Space (Width);
					}
				}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Space (118);
				if (GUILayout.Button(new GUIContent("Install", "This opens the Web Service Script and shows you steps to install and authorize it on your Google Drive"), EditorStyles.toolbarButton))
				{
					ClearErrors();
					Application.OpenURL("https://goo.gl/RBCO0o");  // V4:https://script.google.com/d/1T7e5_40NcgRyind-yeg4PAkHz9TNZJ22F4RcbOvCpAs03JNf1vKNNTZB/newcopy
					//Application.OpenURL("https://goo.gl/wFSbv2");// V3:https://script.google.com/d/1CxQDSXflsXRaH3M7xGfrIDrFwOIHWPsYTWi4mRZ_k77nyIInTgIk63Kd/newcopy");
				}
				if (GUILayout.Button("Verify", EditorStyles.toolbarButton))
				{
					ClearErrors();
					VerifyGoogleService(mProp_Google_WebServiceURL.stringValue);
					GUI.changed = false;
				}
			GUILayout.EndHorizontal();


			if (string.IsNullOrEmpty(mProp_Google_WebServiceURL.stringValue))
			{
				EditorGUILayout.HelpBox(WebServiceHelp, MessageType.Info);
			}

			if (GUI.changed)
			{
				if (string.IsNullOrEmpty(mProp_Google_WebServiceURL.stringValue))
				{
					mProp_Google_SpreadsheetKey.stringValue = string.Empty;
					mProp_Google_SpreadsheetName.stringValue = string.Empty;
				}							


				// If the web service changed then clear the cached spreadsheet keys
				mGoogleSpreadsheets.Clear();
				
				GUI.changed = false;
				ClearErrors();
			}
			GUI.enabled = true;
		}
		
		void OnGUI_GoogleSpreadsheetsInGDrive()
		{
			GUI.enabled = mConnection_WWW==null;

			string[] Spreadsheets;
			string[] SpreadsheetsKey;
			if (mGoogleSpreadsheets.Count>0 || string.IsNullOrEmpty(mProp_Google_SpreadsheetKey.stringValue))
			{
				Spreadsheets = (new List<string>(mGoogleSpreadsheets.Keys)).ToArray();
				SpreadsheetsKey = (new List<string>(mGoogleSpreadsheets.Values)).ToArray();
			}
			else
			{
				Spreadsheets = new string[]{mProp_Google_SpreadsheetName.stringValue ?? string.Empty};
				SpreadsheetsKey = new string[]{mProp_Google_SpreadsheetKey.stringValue ?? string.Empty};
			}
			int mSpreadsheetIndex = System.Array.IndexOf(SpreadsheetsKey, mProp_Google_SpreadsheetKey.stringValue);

			//--[ Spreadsheets ]------------------
			GUILayout.BeginHorizontal();
				GUILayout.Space(10);
				GUILayout.Label ("In Google Drive:", GUILayout.Width(100));

				GUI.changed = false;
				mSpreadsheetIndex = EditorGUILayout.Popup(mSpreadsheetIndex, Spreadsheets, EditorStyles.toolbarPopup);
				if (GUI.changed && mSpreadsheetIndex >= 0)
				{
					mProp_Google_SpreadsheetKey.stringValue = SpreadsheetsKey[mSpreadsheetIndex];
					mProp_Google_SpreadsheetName.stringValue = Spreadsheets[mSpreadsheetIndex];
					GUI.changed = false;
				}

				GUI.enabled = !string.IsNullOrEmpty(mProp_Google_SpreadsheetKey.stringValue) && mConnection_WWW==null;
				if (GUILayout.Button("X", EditorStyles.toolbarButton,GUILayout.ExpandWidth(false)))
					mProp_Google_SpreadsheetKey.stringValue = string.Empty;
				GUI.enabled = true;
				GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.Space(2);

			//--[ Spreadsheets Operations ]------------------
			GUILayout.BeginHorizontal();
				GUILayout.Space(114);
				if (GUILayout.Button("New", EditorStyles.toolbarButton,GUILayout.ExpandWidth(true)))
					Google_NewSpreadsheet();

				GUI.enabled = !string.IsNullOrEmpty(mProp_Google_SpreadsheetKey.stringValue) && mConnection_WWW==null;
				if (GUILayout.Button("Open", EditorStyles.toolbarButton,GUILayout.ExpandWidth(true)))
					OpenGoogleSpreadsheet(mProp_Google_SpreadsheetKey.stringValue);					
				GUI.enabled = mConnection_WWW==null;

				GUILayout.Space(5);

				if (GUILayout.Button("Refresh", EditorStyles.toolbarButton,GUILayout.ExpandWidth(true)))
					Google_FindSpreadsheets();

				GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.Space(15);

			if (!string.IsNullOrEmpty(mProp_Google_SpreadsheetKey.stringValue))
				OnGUI_GoogleButtons_ImportExport( mProp_Google_SpreadsheetKey.stringValue );

			GUI.enabled = true;
		}


		void OnGUI_GoogleButtons_ImportExport( string SpreadsheetKey )
		{
			GUI.enabled = !string.IsNullOrEmpty(SpreadsheetKey) && mConnection_WWW==null;

			GUILayout.BeginHorizontal();
				GUILayout.Space(10);

				eSpreadsheetUpdateMode Mode = SynchronizationButtons("Import");
				if ( Mode!= eSpreadsheetUpdateMode.None)
				{
					ClearErrors();
					serializedObject.ApplyModifiedProperties();
				
					Import_Google(Mode);
				}

				GUILayout.FlexibleSpace();

				Mode = SynchronizationButtons("Export");
				if ( Mode != eSpreadsheetUpdateMode.None)
				{
					ClearErrors();
					serializedObject.ApplyModifiedProperties();
				
					Export_Google(Mode);
				}

				GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUI.enabled = true;
		}

		eSpreadsheetUpdateMode SynchronizationButtons( string Operation, bool ForceReplace = false )
		{
			eSpreadsheetUpdateMode Result = eSpreadsheetUpdateMode.None;
			GUILayout.BeginVertical("AS TextArea", GUILayout.Width (1));
			GUI.backgroundColor = Color.white;

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label(Operation, EditorStyles.miniLabel);
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			
				GUILayout.BeginHorizontal();
					if (GUILayout.Button( "Replace", EditorStyles.toolbarButton, GUILayout.Width(60)))
						Result = eSpreadsheetUpdateMode.Replace;

					if (ForceReplace) GUI.enabled = false;
					if (GUILayout.Button( "Merge", EditorStyles.toolbarButton, GUILayout.Width(60))) 
						Result = eSpreadsheetUpdateMode.Merge;
						
					if (GUILayout.Button( "Add New", EditorStyles.toolbarButton, GUILayout.Width(60)))
						Result = eSpreadsheetUpdateMode.AddNewTerms;
					GUI.enabled = mConnection_WWW==null;
					GUILayout.Space(1);
				GUILayout.EndHorizontal();

				GUILayout.Space(2);
			GUILayout.EndVertical();

			return Result;
		}
		#endregion

		void VerifyGoogleService( string WebServiceURL )
		{
			#if UNITY_WEBPLAYER
			ShowError ("Contacting google translation is not yet supported on WebPlayer" );
			#else
			StopConnectionWWW();
			mWebService_Status = null;	
			mConnection_WWW = new WWW(WebServiceURL + "?action=Ping");
			mConnection_Callback = OnVerifyGoogleService;
			EditorApplication.update += CheckForConnection;
			mConnection_Text = "Verifying Web Service";
			//mConnection_TimeOut = Time.realtimeSinceStartup + 10;
			#endif
		}
		
		void OnVerifyGoogleService( string Result, string Error )
		{
			if (Result.Contains("Authorization is required to perform that action"))
			{
				ShowWarning("You need to authorize the webservice before using it. Check the steps 4 and 5 in the WebService Script");
				mWebService_Status = "Offline";
				return;
			}

			try
			{
				var data = I2.Loc.SimpleJSON.JSON.Parse(Result).AsObject;
				int version = int.Parse (data["script_version"]);
				int requiredVersion = LocalizationManager.GetRequiredWebServiceVersion();
				mWebService_Status = (requiredVersion<=version ? "Online" : "UnsupportedVersion");
				ClearErrors();
			}
			catch(Exception)
			{
				ShowError ("Unable to access the WebService");
				mWebService_Status = "Offline";
			}
		}


		void Export_Google( eSpreadsheetUpdateMode UpdateMode )
		{
			StopConnectionWWW();
			LanguageSource source = ((LanguageSource)target);
			mConnection_WWW = source.Export_Google_CreateWWWcall( UpdateMode );
			if (mConnection_WWW==null)
			{
				OnExported_Google(string.Empty, "WebPlayer can't contact Google");
			}
			else
			{
				mConnection_Callback = OnExported_Google;
				EditorApplication.update += CheckForConnection;
				mConnection_Text = "Uploading spreadsheet";
				//mConnection_TimeOut = Time.realtimeSinceStartup + 10;
			}
		}

		void OnExported_Google( string Result, string Error )
		{
			if (!string.IsNullOrEmpty(Error))
			{
				Debug.Log (Error);
				ShowError("Unable to access google");
				return;
			}

			if (EditorPrefs.GetBool("I2Loc OpenDataSourceAfterExport", true))
				OpenGoogleSpreadsheet( ((LanguageSource)target).Google_SpreadsheetKey );
		}

		static void OpenGoogleSpreadsheet( string SpreadsheetKey )
		{
			ClearErrors();
			string SpreadsheetUrl = "https://docs.google.com/spreadsheet/ccc?key=" + SpreadsheetKey;
			Application.OpenURL(SpreadsheetUrl);
		}

		
		void Import_Google( eSpreadsheetUpdateMode UpdateMode )
		{
			StopConnectionWWW();
			LanguageSource source = ((LanguageSource)target);
			mConnection_WWW = source.Import_Google_CreateWWWcall(true);
			if (mConnection_WWW==null)
			{
				OnImported_Google(string.Empty, "Unable to import from google", eSpreadsheetUpdateMode.Replace);
			}
			else
			{
				mConnection_Callback=null;
				switch (UpdateMode)
				{
					case eSpreadsheetUpdateMode.Replace : mConnection_Callback += OnImported_Google_Replace; break;
					case eSpreadsheetUpdateMode.Merge : mConnection_Callback += OnImported_Google_Merge; break;
					case eSpreadsheetUpdateMode.AddNewTerms : mConnection_Callback += OnImported_Google_AddNewTerms; break;
				}
				EditorApplication.update += CheckForConnection;
				mConnection_Text = "Downloading spreadsheet";
				//mConnection_TimeOut = Time.realtimeSinceStartup + 10;
			}
		}

		void OnImported_Google_Replace( string Result, string Error ) 	{ OnImported_Google(Result, Error, eSpreadsheetUpdateMode.Replace); }
		void OnImported_Google_Merge( string Result, string Error ) 		{ OnImported_Google(Result, Error, eSpreadsheetUpdateMode.Merge); }
		void OnImported_Google_AddNewTerms( string Result, string Error ) { OnImported_Google(Result, Error, eSpreadsheetUpdateMode.AddNewTerms); }

		void OnImported_Google( string Result, string Error, eSpreadsheetUpdateMode UpdateMode )
		{
			if (!string.IsNullOrEmpty(Error))
			{
				ShowError("Unable to access google");
				return;
			}
			LanguageSource source = ((LanguageSource)target);
			string ErrorMsg = source.Import_Google_Result(Result, UpdateMode);
			bool HasErrors = !string.IsNullOrEmpty(ErrorMsg);
			if (HasErrors)
				ShowError(ErrorMsg);

			serializedObject.Update();
			ParseTerms(true, !HasErrors);
			mSelectedKeys.Clear ();
			mSelectedCategories.Clear();
			mShowableTerms.Clear ();
			mLanguageSource.GetCategories(false, mSelectedCategories);

			UnityEditor.EditorUtility.SetDirty (target);
			AssetDatabase.SaveAssets();
		}

		void CheckForConnection()
		{
			if (mConnection_WWW!=null && mConnection_WWW.isDone)
			{
				Action<string, string> callback = mConnection_Callback;
				string Result = string.Empty;
				string Error = mConnection_WWW.error;

				if (string.IsNullOrEmpty(Error))
					Result = mConnection_WWW.text;

				StopConnectionWWW();
				if (callback!=null)
					callback(Result, Error);
			}
			/*else
			if (Time.realtimeSinceStartup > mConnection_TimeOut+30)
			{
				Action<string, string> callback = mConnection_Callback;
				StopConnectionWWW();
				if (callback!=null)
					callback(string.Empty, "Time Out");
			}*/
		}

		void StopConnectionWWW()
		{
			EditorApplication.update -= CheckForConnection;				
			mConnection_WWW = null;
			mConnection_Callback = null;
		}
		
		#region New Spreadsheet

		void Google_NewSpreadsheet()
		{
			#if UNITY_WEBPLAYER
			ShowError ("Contacting google translation is not yet supported on WebPlayer" );
			#else

			ClearErrors();
			string SpreadsheetName;

			LanguageSource source = (LanguageSource)target;
			if (!GUITools.ObjectExistInScene(source.gameObject) && LocalizationManager.IsGlobalSource(source.name))
				SpreadsheetName = string.Format("{0} Localization", PlayerSettings.productName);
			else
				SpreadsheetName = string.Format("{0} {1} {2}", PlayerSettings.productName, Editor_GetCurrentScene(), source.name);

			string query =  mProp_Google_WebServiceURL.stringValue + "?action=NewSpreadsheet&name=" + Uri.EscapeDataString(SpreadsheetName);

			mConnection_WWW = new WWW(query);
			mConnection_Callback = Google_OnNewSpreadsheet;
			EditorApplication.update += CheckForConnection;
			mConnection_Text = "Creating Spreadsheet";
			//mConnection_TimeOut = Time.realtimeSinceStartup + 10;
			#endif
		}

		void Google_OnNewSpreadsheet( string Result, string Error )
		{
			if (!string.IsNullOrEmpty(Error))
			{
				ShowError("Unable to access google");
				return;
			}

			try
			{
				var data = I2.Loc.SimpleJSON.JSON.Parse(Result).AsObject;

				string name = data["name"];
				string key = data["id"];

				serializedObject.Update();
				mProp_Google_SpreadsheetKey.stringValue = key;
				mProp_Google_SpreadsheetName.stringValue = name;
				serializedObject.ApplyModifiedProperties();
				mGoogleSpreadsheets[name] = key;

				LanguageSource source = (LanguageSource)target;
				if (source.mTerms.Count>0 || source.mLanguages.Count>0)
					Export_Google( eSpreadsheetUpdateMode.Replace );
				else
				if (EditorPrefs.GetBool("I2Loc OpenDataSourceAfterExport", true))
					OpenGoogleSpreadsheet( key );

			}
			catch(Exception e)
			{
				ShowError (e.Message);
			}
		}

		#endregion

		#region FindSpreadsheets		

		void Google_FindSpreadsheets()
		{
			#if UNITY_WEBPLAYER
			ShowError ("Contacting google translation is not yet supported on WebPlayer" );
			#else
			string query =  mProp_Google_WebServiceURL.stringValue + "?action=GetSpreadsheetList";
			mConnection_WWW = new WWW(query);
			mConnection_Callback = Google_OnFindSpreadsheets;
			EditorApplication.update += CheckForConnection;
			mConnection_Text = "Accessing google";
			//mConnection_TimeOut = Time.realtimeSinceStartup + 10;
			#endif
		}

		void Google_OnFindSpreadsheets( string Result, string Error)
		{
			if (!string.IsNullOrEmpty(Error))
			{
				ShowError("Unable to access google");
				return;
			}

			try
			{
				mGoogleSpreadsheets.Clear();
				var data = I2.Loc.SimpleJSON.JSON.Parse(Result).AsObject;
				foreach (KeyValuePair<string, I2.Loc.SimpleJSON.JSONNode> element in data)
					mGoogleSpreadsheets[element.Key] = element.Value;
			}
			catch(Exception e)
			{
				ShowError (e.Message);
			}

		}

		#endregion
	}
}