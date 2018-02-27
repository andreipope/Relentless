using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace I2.Loc
{
	public class ParsedTerm
	{
		public string Category, Term, FullTerm;
		public int Usage;
		public TermData termData;
	}
	
	public partial class LocalizationEditor
	{
		#region Variables

		public static SortedDictionary<string, ParsedTerm> mParsedTerms = new SortedDictionary<string, ParsedTerm>(); // All Terms resulted from parsing the scenes and collecting the Localize.Term and how many times the terms are used
		public static SortedDictionary<string, int> mParsedCategories = new SortedDictionary<string, int>();	// Categories and how many terms used them

		public static List<ParsedTerm> mShowableTerms = new List<ParsedTerm> ();	// this contains the terms from mParsedTerms that should be shown in the list (filtered by search string, usage, etc)
		public static bool mParseTermsIn_Scenes = true;
		public static bool mParseTermsIn_Scripts = true;

		#endregion
		
		#region GUI Parse Keys
		
		void OnGUI_Tools_ParseTerms()
		{
			OnGUI_ScenesList();

			GUI.backgroundColor = Color.Lerp (Color.gray, Color.white, 0.2f);
			GUILayout.BeginVertical("AS TextArea", GUILayout.Height(1));
			GUI.backgroundColor = Color.white;

			GUILayout.Space (5);

				EditorGUILayout.HelpBox("This tool searches all Terms used in the selected scenes and updates the usage counter in the Terms Tab", UnityEditor.MessageType.Info);

				GUILayout.Space (5);

				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace();
					GUILayout.BeginHorizontal ("Box");
					mParseTermsIn_Scenes = GUILayout.Toggle(mParseTermsIn_Scenes, new GUIContent("Parse SCENES", "Opens the selected scenes and finds all the used terms"));
					GUILayout.FlexibleSpace();
					mParseTermsIn_Scripts = GUILayout.Toggle(mParseTermsIn_Scripts, new GUIContent("Parse SCRIPTS", "Searches all .cs files and counts all terms like: ScriptLocalization.Get(\"xxx\")"));
					GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace();
					if (GUILayout.Button("Parse Localized Terms"))
					{
						if (mParseTermsIn_Scripts)
							EditorApplication.update += ParseTermsInScripts;
						EditorApplication.update += ParseTermsInSelectedScenes;
					}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}
		
		#endregion

		#region Parsed Terms Handlers

		public static ParsedTerm GetParsedTerm( string Term )
		{
			ParsedTerm data;
			if (!mParsedTerms.TryGetValue(Term, out data))
			{
				data = new ParsedTerm();
				data.Usage = 0;
				data.FullTerm = Term;
				LanguageSource.DeserializeFullTerm( Term, out data.Term, out data.Category );
				mParsedCategories[data.Category]=1;
				mParsedTerms[Term] = data;
				mShowableTerms.Clear();
			}
			return data;
		}

		public static void RemoveParsedTerm( string Term )
		{
			mParsedTerms.Remove(Term);
			string category, key;
			LanguageSource.DeserializeFullTerm( Term, out key, out category );

			int usage;
			if (mParsedCategories.TryGetValue(category, out usage))
			{
				if (usage<=1)
					mParsedCategories.Remove(category);
				else
					mParsedCategories[category]=usage-1;
			}
		}

		public static void DecreaseParsedTerm( string Term )
		{
			ParsedTerm data = GetParsedTerm(Term);
			data.Usage = Mathf.Max (0, data.Usage-1);
		}


		#endregion

		#region ParseKeys

		public static void ParseTermsInSelectedScenes()
		{
			EditorApplication.update -= ParseTermsInSelectedScenes;
			ParseTerms(false);
		}

        public static void DoParseTermsInCurrentScene()
        {
            EditorApplication.update -= DoParseTermsInCurrentScene;
			ParseTerms(true);
        }
		
		static void ParseTerms( bool OnlyCurrentScene, bool OpenTermsTab = true)
		{ 
			mIsParsing = true;

			mParsedTerms.Clear();
			mSelectedKeys.Clear ();
            mParsedCategories.Clear();

			//if (mParseTermsIn_Scripts)
			//	ParseTermsInScripts();

			if (mParseTermsIn_Scenes)
			{
				if (!OnlyCurrentScene)
					ExecuteActionOnSelectedScenes( FindTermsInCurrentScene );
				else 
					FindTermsInCurrentScene();
			}
			
			FindTermsNotUsed();
			
			if (mParsedTerms.Count<=0)
			{
				ShowInfo ("No terms where found during parsing");
				return;
			}


            if (mLanguageSource==null || mSelectedCategories.Count==0)
            {
                mSelectedCategories.Clear();
                foreach (var kvp in mParsedCategories)
                    mSelectedCategories.Add(kvp.Key);
				mShowableTerms.Clear ();
            }
            else
            {
                var sourceCategories = mLanguageSource.GetCategories();
                mSelectedCategories.RemoveAll(x => !sourceCategories.Contains(x));
                foreach (var kvp in mParsedCategories)
                    if (!sourceCategories.Contains(kvp.Key))
					{
                        mSelectedCategories.Add(kvp.Key);
						mShowableTerms.Clear ();
					}
            }

			if (OpenTermsTab) 
			{
				mFlagsViewKeys = ((int)eFlagsViewKeys.Used | (int)eFlagsViewKeys.NotUsed | (int)eFlagsViewKeys.Missing);
				mCurrentViewMode = eViewMode.Keys;
			}
			mIsParsing = false;
		}
		
		static void FindTermsInCurrentScene()
		{
			Localize[] Locals = (Localize[])Resources.FindObjectsOfTypeAll(typeof(Localize));
			
			if (Locals==null)
				return;
			
			for (int i=0, imax=Locals.Length; i<imax; ++i)
			{
				Localize localize = Locals[i];
                if (localize==null || (localize.Source!=null && localize.Source!=mLanguageSource) || localize.gameObject==null || !GUITools.ObjectExistInScene(localize.gameObject))
					continue;
				 
				string Term, SecondaryTerm;
				//Term = localize.Term;
				//SecondaryTerm = localize.SecondaryTerm;
				localize.GetFinalTerms( out Term, out SecondaryTerm );

				if (!string.IsNullOrEmpty(Term))
					GetParsedTerm(Term).Usage++;

				if (!string.IsNullOrEmpty(SecondaryTerm))
					GetParsedTerm(SecondaryTerm).Usage++;
			}
		}

		static void FindTermsNotUsed()
		{
			// every Term that is in the DB but not in mParsedTerms

			foreach (TermData termData in mLanguageSource.mTerms)
				GetParsedTerm(termData.Term);	// this will create the ParsedTerm if it doesn't exist
		}

        static void ParseTermsInScripts() 
		{
            string[] scriptFiles = AssetDatabase.GetAllAssetPaths().Where(path => path.ToLower().EndsWith(".cs")).ToArray();

            Regex regex = new Regex(@"ScriptLocalization\.Get\(\""(.*?)\""\)", RegexOptions.Multiline);

            foreach (string scriptFile in scriptFiles) 
			{
                string scriptContents = File.ReadAllText(scriptFile);
                MatchCollection matches = regex.Matches(scriptContents);
                for (int matchNum = 0; matchNum < matches.Count; matchNum++) 
				{
                    Match match = matches[matchNum];
                    string term = match.Groups[1].Value;
                    GetParsedTerm(term).Usage++;
                }
            }
            
        }
		#endregion

		#region Misc

		public static void SetAllTerms_When_InferredTerms_IsInSource()
		{
			var Locals = Resources.FindObjectsOfTypeAll(typeof(Localize)) as Localize[];

			if (Locals==null)
				return;

			foreach (var localize in Locals) 
			{
				if (localize == null || (localize.Source != null && localize.Source != mLanguageSource) || localize.gameObject == null || !GUITools.ObjectExistInScene (localize.gameObject))
					continue;

				if (!string.IsNullOrEmpty (localize.mTerm) && !string.IsNullOrEmpty (localize.SecondaryTerm))
					continue;

				ApplyInferredTerm( localize );
			}

			ParseTerms (true);
		}

		public static void ApplyInferredTerm( Localize localize)
		{
			if (mLanguageSource==null)
				return;
			if (!string.IsNullOrEmpty (localize.mTerm) && !string.IsNullOrEmpty (localize.mTermSecondary))
				return;

			string sTerm, sSecTerm;
			localize.GetFinalTerms (out sTerm, out sSecTerm);

			if (string.IsNullOrEmpty (localize.mTerm))
			{ 
				var termData = mLanguageSource.GetTermData (sTerm, true);
				if (termData!=null)
					localize.mTerm = termData.Term;
			}

			if (string.IsNullOrEmpty (localize.mTermSecondary))
			{
				var termData = mLanguageSource.GetTermData (sSecTerm, true);
				if (termData!=null)
					localize.mTermSecondary = termData.Term;
			}

			localize.Source = mLanguageSource;
		}

		#endregion
	}
}
