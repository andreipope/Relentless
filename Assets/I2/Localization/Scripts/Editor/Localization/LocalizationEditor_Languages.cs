using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables
        private bool mShouldDetectStoreIntegration = true;
        private bool mStoreIntegrated_IOS = false;
        private bool mStoreIntegrated_Android = false;
		private List<TranslationRequest> mTranslationRequests = new List<TranslationRequest> ();

		#endregion

		void OnGUI_Languages()
		{
			//GUILayout.Space(5);

			OnGUI_ShowMsg();

			OnGUI_LanguageList();

            OnGUI_StoreIntegration();
		}

		#region GUI Languages
		
		void OnGUI_LanguageList()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
				GUILayout.FlexibleSpace();
				GUILayout.Label ("Languages:", EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
				GUILayout.FlexibleSpace();
				GUILayout.Label ("Code:", EditorStyles.miniLabel, GUILayout.Width(76));
			GUILayout.EndHorizontal();
			
			//--[ Language List ]--------------------------

			int IndexLanguageToDelete = -1;
			int LanguageToMoveUp = -1;
			int LanguageToMoveDown = -1;
			mScrollPos_Languages = GUILayout.BeginScrollView( mScrollPos_Languages, "AS TextArea", GUILayout.MinHeight (100), GUILayout.MaxHeight(Screen.height), GUILayout.ExpandHeight(false));

			List<string> codes = GoogleLanguages.GetAllInternationalCodes();
			codes.Sort();
			codes.Insert(0, string.Empty);

			for (int i=0, imax=mProp_Languages.arraySize; i<imax; ++i)
			{
				GUILayout.BeginHorizontal();

				SerializedProperty Prop_Lang = mProp_Languages.GetArrayElementAtIndex(i);
				SerializedProperty Prop_LangName = Prop_Lang.FindPropertyRelative("Name");
				SerializedProperty Prop_LangCode = Prop_Lang.FindPropertyRelative("Code");

				if (GUILayout.Button ("X", "toolbarbutton", GUILayout.ExpandWidth(false)))
				{
					IndexLanguageToDelete = i;
				}
				
				GUILayout.BeginHorizontal(EditorStyles.toolbar);

				GUI.changed = false;
				string LanName = EditorGUILayout.TextField(Prop_LangName.stringValue, GUILayout.ExpandWidth(true));
				if (GUI.changed && !string.IsNullOrEmpty(LanName))
				{
					Prop_LangName.stringValue = LanName;
					GUI.changed = false;
				}

				int Index = Mathf.Max(0, codes.IndexOf (Prop_LangCode.stringValue));
				GUI.changed = false;
				Index = EditorGUILayout.Popup(Index, codes.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(60));
				if (GUI.changed && Index>=0)
				{
					Prop_LangCode.stringValue = codes[Index];
				}

				GUILayout.EndHorizontal();

				GUI.enabled = (i<imax-1);
				if (GUILayout.Button( "\u25BC", EditorStyles.toolbarButton, GUILayout.Width(18))) LanguageToMoveDown = i;
				GUI.enabled = i>0;
				if (GUILayout.Button( "\u25B2", EditorStyles.toolbarButton, GUILayout.Width(18))) LanguageToMoveUp = i;
				GUI.enabled = true;

				if (GUILayout.Button( new GUIContent("Show", "Preview all localizations into this language"), EditorStyles.toolbarButton, GUILayout.Width(35))) 
				{
					LocalizationManager.SetLanguageAndCode( LanName, Prop_LangCode.stringValue, false, true);
				}

				//GUI.enabled = false;
				if (GUILayout.Button( new GUIContent("Translate", "Translate all empty terms"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) 
				{
					TranslateAllToLanguage( LanName );
				}
				//GUI.enabled = true;
				
				GUILayout.EndHorizontal();
			}
			
			GUILayout.EndScrollView();
			
			OnGUI_AddLanguage( mProp_Languages );

			if (mConnection_WWW!=null && mConnection_Text.Contains("Translating"))
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
			
			if (IndexLanguageToDelete>=0)
			{
				mLanguageSource.RemoveLanguage( mLanguageSource.mLanguages[IndexLanguageToDelete].Name );
				serializedObject.Update();
				ParseTerms(true, false);
                mShouldDetectStoreIntegration = true;
			}

			if (LanguageToMoveUp>=0)   SwapLanguages( LanguageToMoveUp, LanguageToMoveUp-1 );
			if (LanguageToMoveDown>=0) SwapLanguages( LanguageToMoveDown, LanguageToMoveDown+1 );
		}

		void SwapLanguages( int iFirst, int iSecond )
		{
			serializedObject.ApplyModifiedProperties();
			LanguageSource Source = mLanguageSource;

			SwapValues( Source.mLanguages, iFirst, iSecond );
			foreach (TermData termData in Source.mTerms)
			{
				SwapValues ( termData.Languages, iFirst, iSecond );
				SwapValues ( termData.Languages_Touch, iFirst, iSecond );
				SwapValues ( termData.Flags, iFirst, iSecond );
			}
			serializedObject.Update();
		}

		void SwapValues( List<LanguageData> mList, int Index1, int Index2 )
		{
			LanguageData temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}
		void SwapValues( string[] mList, int Index1, int Index2 )
		{
			string temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}
		void SwapValues( byte[] mList, int Index1, int Index2 )
		{
			byte temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}

		
		void OnGUI_AddLanguage( SerializedProperty Prop_Languages)
		{
			//--[ Add Language Upper Toolbar ]-----------------
			
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			mLanguages_NewLanguage = EditorGUILayout.TextField("", mLanguages_NewLanguage, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			GUI.enabled = !string.IsNullOrEmpty (mLanguages_NewLanguage);
			if (GUILayout.Button("Add", EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				Prop_Languages.serializedObject.ApplyModifiedProperties();
				mLanguageSource.AddLanguage( mLanguages_NewLanguage, GoogleLanguages.GetLanguageCode(mLanguages_NewLanguage) );
				Prop_Languages.serializedObject.Update();
                mShouldDetectStoreIntegration = true;
				mLanguages_NewLanguage = "";
                GUI.FocusControl(string.Empty);
			}
			GUI.enabled = true;
			
			GUILayout.EndHorizontal();
			
			
			//--[ Add Language Bottom Toolbar ]-----------------
			
			GUILayout.BeginHorizontal();
			
			//-- Language Dropdown -----------------
			string CodesToExclude = string.Empty;
			foreach (var LanData in mLanguageSource.mLanguages)
				CodesToExclude = string.Concat(CodesToExclude, "[", LanData.Code, "]");

			List<string> Languages = GoogleLanguages.GetLanguagesForDropdown(mLanguages_NewLanguage, CodesToExclude);

			GUI.changed = false;
			int index = EditorGUILayout.Popup(0, Languages.ToArray(), EditorStyles.toolbarDropDown);

			if (GUI.changed && index>=0)
			{
				mLanguages_NewLanguage = GoogleLanguages.GetFormatedLanguageName( Languages[index] );
			}
			
			
			if (GUILayout.Button("Add", EditorStyles.toolbarButton, GUILayout.Width(50)) && index>=0)
			{
				Prop_Languages.serializedObject.ApplyModifiedProperties();
				mLanguages_NewLanguage = GoogleLanguages.GetFormatedLanguageName( Languages[index] );
				if (!string.IsNullOrEmpty(mLanguages_NewLanguage)) 
					mLanguageSource.AddLanguage( mLanguages_NewLanguage, GoogleLanguages.GetLanguageCode(mLanguages_NewLanguage) );
				Prop_Languages.serializedObject.Update();
                mShouldDetectStoreIntegration = true;
				mLanguages_NewLanguage = "";
                GUI.FocusControl(string.Empty);
			}
			
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUI.color = Color.white;
		}


		void TranslateAllToLanguage (string lanName)
		{
			if (!GoogleTranslation.CanTranslate ()) 
			{
				ShowError ("WebService is not set correctly or needs to be reinstalled");
				return;
			}

			int LanIndex = mLanguageSource.GetLanguageIndex (lanName);
			string code = mLanguageSource.mLanguages [LanIndex].Code;

			mTranslationRequests.Clear ();
			foreach (var termData in mLanguageSource.mTerms) 
			{
				if (!string.IsNullOrEmpty((GUI_SelectedInputType==0 ? termData.Languages : termData.Languages_Touch)[LanIndex]))
					continue;
				
				string sourceCode, sourceText;
				FindTranslationSource( LanguageSource.GetKeyFromFullTerm(termData.Term), termData, code, out sourceText, out sourceCode );

				mTranslationRequests.Add( new TranslationRequest(){
					Term = termData.Term,
					Text=sourceText,
					LanguageCode=sourceCode,
					TargetLanguagesCode=new string[]{code}
				} );
			}

			mConnection_WWW = GoogleTranslation.GetTranslationWWW (mTranslationRequests);
			mConnection_Text = "Translating";
			mConnection_Callback = OnLanguageTranslated;
			EditorApplication.update += CheckForConnection;
		}

		void OnLanguageTranslated( string Result, string Error )
		{
			//Debug.Log (Result);
			if (!string.IsNullOrEmpty(Error)/* || !Result.Contains("<i2>")*/)
		    {
				ShowError ("Unable to access Google or not valid request");
				return;
			}

			ClearErrors();
			Error = GoogleTranslation.ParseTranslationResult (Result, mTranslationRequests);
			if (!string.IsNullOrEmpty(Error))
			{
				ShowError (Error);
				return;
			}


			foreach (var request in mTranslationRequests)
			{
				if (request.Results == null)	// Handle cases where not all translations were valid
						continue;
								
				var termData = mLanguageSource.GetTermData(request.Term);
				if (termData==null)
					continue;

				string lastCode="";
				int lastcodeIdx= 0;

				for (int i=0; i<request.Results.Length; ++i)
				{
					//--[ most of the time is a single code, so this works as a cache
					if (lastCode!=request.TargetLanguagesCode[i])
					{
						lastCode = request.TargetLanguagesCode[i];
						lastcodeIdx = mLanguageSource.GetLanguageIndexFromCode( lastCode );
					}

					if (GUI_SelectedInputType==0)
						termData.Languages[lastcodeIdx] = request.Results[i];
					else
						termData.Languages_Touch[lastcodeIdx] = request.Results[i];
				}
			}
		}

		#endregion

        #region Store Integration

        void OnGUI_StoreIntegration()
        {
           if (mShouldDetectStoreIntegration)
                DetectStoreIntegration();
            
            GUIStyle lstyle = new GUIStyle (EditorStyles.label);
            lstyle.richText = true;

            GUILayout.BeginHorizontal ();
                GUILayout.Label (new GUIContent("Store Integration:", "Setups the stores to detect that the game has localization, Android adds strings.xml for each language. IOS modifies the Info.plist"), EditorStyles.boldLabel);

                GUILayout.BeginVertical();
                    GUILayout.BeginHorizontal ();
                        if (mStoreIntegrated_IOS)
                            GUILayout.Label( new GUIContent("<color=green><size=16>\u2713</size></color>  IOS", "Integrated Successfully"), lstyle, GUILayout.Width(90));
                        else
                        {
                            GUILayout.Label( new GUIContent("<color=green><size=16>\u2717</size></color>  IOS", "Missing"), lstyle, GUILayout.Width(90));
                            if (GUILayout.Button(new GUIContent("Generate", "Setups the stores to detect that the game has localization, IOS modifies the Info.plist.\n\nWill be implemented in the next version"), GUILayout.Width(70)))
                                IntegrateStore_IOS();
                        }
                    GUILayout.EndHorizontal ();

                    GUILayout.BeginHorizontal ();
                        if (mStoreIntegrated_Android)
                            GUILayout.Label( new GUIContent("<color=green><size=16>\u2713</size></color>  Android", "Integrated Successfully"), lstyle, GUILayout.Width(90));
                        else
                        {
                            GUILayout.Label( new GUIContent("<color=green><size=16>\u2717</size></color>  Android", "Missing"), lstyle, GUILayout.Width(90));
                            if (GUILayout.Button(new GUIContent("Generate", "Setups the stores to detect that the game has localization, Android adds strings.xml for each language."), GUILayout.Width(70)))
                                IntegrateStore_Android();
                        }
                    GUILayout.EndHorizontal ();
                GUILayout.EndVertical();
            GUILayout.EndHorizontal ();            
        }

        void DetectStoreIntegration()
        {
            mShouldDetectStoreIntegration = false;
            mStoreIntegrated_IOS = DetectStoreIntegration_IOS(false);
            mStoreIntegrated_Android = DetectStoreIntegration_Android(false);
        }

 		#endregion

		#region Store Integration Android
		
        bool DetectStoreIntegration_Android(bool integrate )
        {
            string resFolder = Application.dataPath + "/Plugins/Android/I2Localization";

			string Manifest = 	"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
								"<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" package=\"com.InterIllusion.I2Localization\">\n"+
								"	<application/>\n"+
								"</manifest>";

			if (!StoreIntegrationAndroid_Item(resFolder, "AndroidManifest.xml", Manifest, integrate))
				return false;

			if (!StoreIntegrationAndroid_Item(resFolder, "project.properties", "target=android-6\nandroid.library=true", integrate))
				return false;

			string stringXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<resources>\n\t<string name=\"t\">t</string>\n</resources>";
			if (!StoreIntegrationAndroid_Item(resFolder+"/res/values", "strings.xml", stringXML, integrate))
                return false;
            

			var list = new List<string> ();
			list.Add (resFolder + "/res/values");
            foreach (var lan in mLanguageSource.mLanguages)
            {
                string code = lan.Code;
                if (code == null || code.Length < 2)
                    continue;

				code = code.Replace("-", "-r");
				string dir = resFolder + "/res/values-" + code;

				if (list.Contains(dir))
					continue;
				list.Add(dir);
				

				if (!StoreIntegrationAndroid_Item(dir, "strings.xml", stringXML, integrate))
                    return false;
            }
			try
			{
				var folders = System.IO.Directory.GetDirectories(resFolder+"/res");
				foreach( var folder in folders )
					if (!list.Contains(folder.Replace("\\","/")))
					{
						if (!integrate)
					    	return false;
						System.IO.Directory.Delete(folder, true);
						if (System.IO.File.Exists(folder+".meta"))
							System.IO.File.Delete(folder+".meta");
					}
			}
			catch(System.Exception)
			{
			}

			AssetDatabase.Refresh();

            return true;
        }

        bool StoreIntegrationAndroid_Item(string folder, string fileName, string text, bool integrate )
        {
            try
            {
                if (!System.IO.Directory.Exists(folder))
                {
                    if (!integrate)
                        return false;

                    System.IO.Directory.CreateDirectory(folder);
                }

                if (!System.IO.File.Exists(folder + "/"+fileName))
                {
                    if (!integrate)
                       return false;
					System.IO.File.WriteAllText(folder + "/"+fileName, text);
                }
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

		void IntegrateStore_Android()
		{
			mStoreIntegrated_Android = DetectStoreIntegration_Android(true);
		}

		#endregion

		#region Store Integration IOS

		bool DetectStoreIntegration_IOS( bool integrate )
		{
			string filePath = Application.dataPath + "/Plugins/iOS/Info.plist";
			try
			{
				if (!System.IO.File.Exists (filePath))
					return integrate ? IntegrateStore_IOS_newPList() : false;
				
				string text = System.IO.File.ReadAllText(filePath);
				if (!text.StartsWith("<?xml"))
					return integrate ? IntegrateStore_IOS_newPList() : false;
				
				return IntegrateStore_IOS_updatePList(integrate);
			}			
			catch (System.Exception)
			{
				return false;
			}
		}

		bool IntegrateStore_IOS_newPList ()
		{
			var sb = new System.Text.StringBuilder ();
			sb.AppendLine ("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			sb.AppendLine ("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
			sb.AppendLine ("<plist version=\"1.0\">");
			sb.AppendLine ("<dict>");
			sb.AppendLine ("	<key>CFBundleDevelopmentRegion</key>");
			sb.AppendLine ("	<string>en</string>");
			sb.AppendLine ("	<key>CFBundleExecutable</key>");
			sb.AppendLine ("	<string>${EXECUTABLE_NAME}</string>");
			sb.AppendLine ("	<key>CFBundleIdentifier</key>");
			sb.AppendLine ("	<string>com.unity3d.${PRODUCT_NAME}</string>");
			sb.AppendLine ("	<key>CFBundleInfoDictionaryVersion</key>");
			sb.AppendLine ("	<string>6.0</string>");
			sb.AppendLine ("	<key>CFBundleName</key>");
			sb.AppendLine ("	<string>${PRODUCT_NAME}</string>");
			sb.AppendLine ("	<key>CFBundlePackageType</key>");
			sb.AppendLine ("	<string>APPL</string>");
			sb.AppendLine ("	<key>LSRequiresIPhoneOS</key>");
			sb.AppendLine ("	<true/>");
			sb.AppendLine ("	<key>UILaunchStoryboardName</key>");
			sb.AppendLine ("	<string>LaunchScreen</string>");
			sb.AppendLine ("	<key>UIRequiredDeviceCapabilities</key>");
			sb.AppendLine ("	<array><string>armv7</string></array>");

			sb.Append (GetStoreIntegrationIOS_PlistKeys ());

			sb.AppendLine ("</dict>");
			sb.AppendLine ("</plist>");

			try
			{
				string folder = Application.dataPath + "/Plugins/iOS";
				
				if (!System.IO.Directory.Exists(folder))
					System.IO.Directory.CreateDirectory(folder);

				System.IO.File.WriteAllText (folder+"/Info.plist", sb.ToString (), System.Text.Encoding.UTF8);
				AssetDatabase.Refresh();
				return true;
			}			
			catch (System.Exception)
			{
				return false;
			}
		}

		bool IntegrateStore_IOS_updatePList ( bool integrate )
		{
			try
			{
				string filePath = Application.dataPath + "/Plugins/iOS/Info.plist";
				
				var sb = System.IO.File.ReadAllText(filePath);

				string locString = GetStoreIntegrationIOS_PlistKeys();
				if (sb.Contains(locString))
					return true;

				if (!integrate)
					return false;

				int idxStart = sb.IndexOf("<key>CFBundleLocalizations</key>");
				if (idxStart<0)
					return integrate ? IntegrateStore_IOS_newPList() : false;

				idxStart = sb.LastIndexOf("\n");
				if (idxStart<0)
					return integrate ? IntegrateStore_IOS_newPList() : false;

				int idxEnd = sb.IndexOf("</array>", idxStart);
				if (idxEnd<0)
					return integrate ? IntegrateStore_IOS_newPList() : false;

				sb = sb.Remove(idxStart, idxEnd-idxStart);
				sb.Insert(idxStart, locString);

				System.IO.File.WriteAllText (filePath, sb, System.Text.Encoding.UTF8);
				AssetDatabase.Refresh();
				return true;
			}			
			catch (System.Exception)
			{
				return false;
			}
		}

        void IntegrateStore_IOS()
        {
			mStoreIntegrated_IOS = DetectStoreIntegration_IOS( true );
        }

		string GetStoreIntegrationIOS_PlistKeys()
		{
			var sb = new System.Text.StringBuilder ();
			sb.AppendLine ("	<key>CFBundleLocalizations</key>");
			sb.AppendLine ("	<array>");

			var list = new List<string> ();
			foreach (var lan in mLanguageSource.mLanguages)
			{
				string code = lan.Code;
				if (code == null || code.Length < 2)
					continue;

				code = code.Substring(0, 2);
				if (list.Contains(code))
					continue;
				list.Add(code);

				sb.AppendFormat ("		<string>{0}</string>\n", code);
			}
			sb.AppendLine ("	</array>");
			return sb.ToString ();
		}

        #endregion
	}
}