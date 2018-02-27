using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace I2.Loc
{
	[CustomEditor(typeof(LanguageSource))]
	public partial class LocalizationEditor : Editor
	{
		#region Variables
		
		SerializedProperty 	mProp_Assets, mProp_Languages, 
							mProp_Google_WebServiceURL, mProp_GoogleUpdateFrequency, mProp_GoogleUpdateDelay, mProp_Google_SpreadsheetKey, mProp_Google_SpreadsheetName, 
							mProp_Spreadsheet_LocalFileName, mProp_Spreadsheet_LocalCSVSeparator, mProp_CaseInsensitiveTerms, mProp_Spreadsheet_LocalCSVEncoding;

		public static LanguageSource mLanguageSource;

		static bool mIsParsing = false;  // This is true when the editor is opening several scenes to avoid reparsing objects

		#endregion
		
		#region Variables GUI
		
		GUIStyle Style_ToolBar_Big, Style_ToolBarButton_Big;

 		
		public GUISkin CustomSkin;

		static Vector3 mScrollPos_Languages;
		static string mLanguages_NewLanguage = "";

		#endregion

        #region Styles

        public static GUIStyle Style_WrapTextField {
            get{ 
                if (mStyle_WrapTextField==null)
                {
                    mStyle_WrapTextField = new GUIStyle("textField");
                    mStyle_WrapTextField.wordWrap = true;
                }
                return mStyle_WrapTextField;
            }
        }
        static GUIStyle mStyle_WrapTextField;

        #endregion

		#region Inspector

		void OnEnable()
		{
			var newSource = target as LanguageSource;
			bool ForceParse = (mLanguageSource != newSource);
			mLanguageSource = newSource;

			if (!LocalizationManager.Sources.Contains(mLanguageSource))
				LocalizationManager.UpdateSources();
			mProp_Assets 					= serializedObject.FindProperty("Assets");
			mProp_Languages 				= serializedObject.FindProperty("mLanguages");
			mProp_Google_WebServiceURL		= serializedObject.FindProperty("Google_WebServiceURL");
			mProp_GoogleUpdateFrequency 	= serializedObject.FindProperty("GoogleUpdateFrequency");
			mProp_GoogleUpdateDelay			= serializedObject.FindProperty("GoogleUpdateDelay");
			mProp_Google_SpreadsheetKey 	= serializedObject.FindProperty("Google_SpreadsheetKey");
			mProp_Google_SpreadsheetName	= serializedObject.FindProperty("Google_SpreadsheetName");
			mProp_CaseInsensitiveTerms 		= serializedObject.FindProperty("CaseInsensitiveTerms");
			mProp_Spreadsheet_LocalFileName = serializedObject.FindProperty("Spreadsheet_LocalFileName");
			mProp_Spreadsheet_LocalCSVSeparator = serializedObject.FindProperty("Spreadsheet_LocalCSVSeparator");
			mProp_Spreadsheet_LocalCSVEncoding = serializedObject.FindProperty("Spreadsheet_LocalCSVEncoding");
			if (!mIsParsing)
			{
				if (string.IsNullOrEmpty(mLanguageSource.Google_SpreadsheetKey))
					mSpreadsheetMode = eSpreadsheetMode.Local;
				else
					mSpreadsheetMode = eSpreadsheetMode.Google;

				mCurrentViewMode = (mLanguageSource.mLanguages.Count>0 ? eViewMode.Keys : eViewMode.Languages);

				UpdateSelectedKeys();

				if (ForceParse || mParsedTerms.Count < mLanguageSource.mTerms.Count )
					ParseTerms(true);
			}
			mShowableTerms.Clear ();
			//UpgradeManager.EnablePlugins();
		}

		void UpdateSelectedKeys()
		{
			// Remove all keys that are not in this source
			string trans;
			for (int i=mSelectedKeys.Count-1; i>=0; --i)
				if (!mLanguageSource.TryGetTermTranslation(mSelectedKeys[i], out trans))
					mSelectedKeys.RemoveAt(i);

			// Remove all Categories that are not in this source
			/*var mCateg = mLanguageSource.GetCategories();
			for (int i=mSelectedCategories.Count-1; i>=0; --i)
				if (!mCateg.Contains(mSelectedCategories[i]))
					mSelectedCategories.RemoveAt(i);
			if (mSelectedCategories.Count==0)
				mSelectedCategories = mCateg;*/

			if (mSelectedScenes.Count==0)
				mSelectedScenes.Add (Editor_GetCurrentScene());
        }
        
        public override void OnInspectorGUI()
		{
			// Load Test:
			/*if (mLanguageSource.mTerms.Count<4000)
			{
				mLanguageSource.mTerms.Clear();
				for (int i=0; i<4500; ++i)
					mLanguageSource.AddTerm("ahh"+i.ToString("00000"), eTermType.Text, false);
				mLanguageSource.UpdateDictionary();
			}*/

			mIsParsing = false;
			//serializedObject.UpdateIfDirtyOrScript();
			if (mLanguageSource.mTerms.Count<1000)
				Undo.RecordObject(target, "LanguageSource");

			GUI.backgroundColor = Color.Lerp (Color.black, Color.gray, 1);
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_Background);
			GUI.backgroundColor = Color.white;
			
			if (GUILayout.Button("Language Source", LocalizeInspector.GUIStyle_Header))
			{
				Application.OpenURL(LocalizeInspector.HelpURL_Documentation);
			}

				InitializeStyles();

				GUILayout.Space(10);

				OnGUI_Main();

			GUILayout.Space (10);
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
				if (GUILayout.Button("v"+LocalizationManager.GetVersion(), EditorStyles.miniLabel))
					Application.OpenURL(LocalizeInspector.HelpURL_ReleaseNotes);

				GUILayout.FlexibleSpace ();
				if (GUILayout.Button("Tutorials", EditorStyles.miniLabel))
					Application.OpenURL(LocalizeInspector.HelpURL_Tutorials);
			
				GUILayout.Space(10);

				if (GUILayout.Button("Ask a Question", EditorStyles.miniLabel))
					Application.OpenURL(LocalizeInspector.HelpURL_forum);

				GUILayout.Space(10);

				if (GUILayout.Button("Documentation", EditorStyles.miniLabel))
					Application.OpenURL(LocalizeInspector.HelpURL_Documentation);
			GUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0;

			GUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();
		}

		/*void OnDisable()
		{
			if (!mIsParsing)
				mParsedTerms.Clear ();
		}*/


		#endregion
	}
}