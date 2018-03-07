using UnityEditor;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables

		List<string> mCharSetTool_Languages = new List<string>();
		string mCharSet;
        bool mCharSetTool_CaseSensitive = false;

		#endregion
		
		#region GUI Generate Script
		
		void OnGUI_Tools_CharSet()
		{
			// remove missing languages
			for (int i=mCharSetTool_Languages.Count-1; i>=0; --i)
			{
				if (mLanguageSource.GetLanguageIndex(mCharSetTool_Languages[i])<0)
					mCharSetTool_Languages.RemoveAt(i);
			}

			GUILayout.BeginHorizontal (EditorStyles.toolbar);
			GUILayout.Label ("Languages:", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal ();

			//--[ Language List ]--------------------------

			mScrollPos_Languages = GUILayout.BeginScrollView( mScrollPos_Languages, "AS TextArea", GUILayout.MinHeight (100), GUILayout.MaxHeight(Screen.height), GUILayout.ExpandHeight(false));
            bool computeSet = false;

			for (int i=0, imax=mLanguageSource.mLanguages.Count; i<imax; ++i)
			{
				GUILayout.BeginHorizontal();
					var language = mLanguageSource.mLanguages[i].Name;
					bool hasLanguage = mCharSetTool_Languages.Contains(language);
					bool newValue = GUILayout.Toggle (hasLanguage, "", "OL Toggle", GUILayout.ExpandWidth(false));
					GUILayout.Label(language);
				GUILayout.EndHorizontal();

				if (hasLanguage != newValue)
				{
					if (newValue) 
						mCharSetTool_Languages.Add(language);
					else 
						mCharSetTool_Languages.Remove(language);

                    computeSet = true;
				}
			}
			
			GUILayout.EndScrollView();

			//GUILayout.Space (5);
			
			GUI.backgroundColor = Color.Lerp (Color.gray, Color.white, 0.2f);
			GUILayout.BeginVertical("AS TextArea", GUILayout.Height(1));
			GUI.backgroundColor = Color.white;
			
			EditorGUILayout.HelpBox("This tool shows all characters used in the selected languages", UnityEditor.MessageType.Info);
			
            GUILayout.Space (5);
            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.changed = false;
                mCharSetTool_CaseSensitive = GUILayout.Toggle(mCharSetTool_CaseSensitive, "Case-Sensitive", GUILayout.ExpandWidth(false));
                if (GUI.changed)
                    computeSet = true;
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
			GUILayout.Space (5);

            if (computeSet)
                UpdateCharSets();

			GUILayout.Label ("Used Characters: (" + mCharSet.Length+")");
				EditorGUILayout.TextArea (mCharSet ?? "");
			GUILayout.EndVertical ();
		}
		
		#endregion

		#region Generate Script File

		void UpdateCharSets ()
		{
			mCharSet = "";
			var sb = new HashSet<char> ();
			var LanIndexes = new List<int> ();
			for (int i=0; i<mLanguageSource.mLanguages.Count; ++i)
				if (mCharSetTool_Languages.Contains(mLanguageSource.mLanguages[i].Name))
				    LanIndexes.Add(i);

			foreach (var termData in mLanguageSource.mTerms) 
			{
				for (int i=0; i<LanIndexes.Count; ++i)
				{
					AppendToCharSet( sb, termData.Languages[LanIndexes[i]] );
					AppendToCharSet( sb, termData.Languages_Touch[LanIndexes[i]] );
				}
			}
			mCharSet = new string(sb.ToArray().OrderBy(c=>c).ToArray ());
		}

		void AppendToCharSet( HashSet<char> sb, string text )
		{
            foreach (char c in text)
            {
                if (!mCharSetTool_CaseSensitive)
                {
                    sb.Add(char.ToLowerInvariant(c));
                    sb.Add(char.ToUpperInvariant(c));
                }
                else
                    sb.Add(c);
            }
		}
		


		#endregion
	}
}
