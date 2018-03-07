using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables
		
		Vector2 mScrollPos_Keys = Vector2.zero;
		
		public static string mKeyToExplore;  								// Key that should show all the language details
		string KeyList_Filter = "";
		float mRowSize=-1;
		float ScrollHeight;
		float mTermList_MaxWidth = -1;

		public static List<string> mSelectedKeys = new List<string>(); 	// Selected Keys in the list of mParsedKeys
		public static List<string> mSelectedCategories = new List<string>();


		enum eFlagsViewKeys
		{
			Used = 1<<1,
			Missing = 1<<2, 
			NotUsed = 1<<3
		};
		static int mFlagsViewKeys = ((int)eFlagsViewKeys.Used | (int)eFlagsViewKeys.NotUsed | (int)eFlagsViewKeys.Missing);

		public static string mTermsList_NewTerm = null;
		#endregion
		
		#region GUI Key List

		float ExpandedViewHeight = 0;
		float TermsListHeight = 0;
		void OnGUI_KeysList(bool AllowExpandKey = true, float Height = 300.0f, bool ShowTools=true)
		{
			if (mTermList_MaxWidth<=0)
				CalculateTermsListMaxWidth();

			//--[ List Filters ]--------------------------------------
			OnGUI_ShowMsg();

			GUILayout.BeginHorizontal();
				GUIStyle bstyle = new GUIStyle ("toolbarbutton");
				bstyle.fontSize = 15;
				if (GUILayout.Button (new GUIContent("\u21bb", "Parse Scene and update terms list with missing and unused terms"), bstyle, GUILayout.Width(25)))
                    EditorApplication.update += DoParseTermsInCurrentScene;
			GUILayout.Space (1);

			var oldFlags = mFlagsViewKeys;
			mFlagsViewKeys = OnGUI_FlagToogle("Used","Shows All Terms referenced in the parsed scenes", 				mFlagsViewKeys, (int)eFlagsViewKeys.Used);
			mFlagsViewKeys = OnGUI_FlagToogle("Not Used", "Shows all Terms from the Source that are not been used", 	mFlagsViewKeys, (int)eFlagsViewKeys.NotUsed);
			mFlagsViewKeys = OnGUI_FlagToogle("Missing","Shows all Terms Used but not defined in the Source", 			mFlagsViewKeys, (int)eFlagsViewKeys.Missing);
			if (oldFlags!=mFlagsViewKeys)
				mShowableTerms.Clear ();

			OnGUI_SelectedCategories();

			GUILayout.EndHorizontal();

			//--[ Keys List ]-----------------------------------------
			mScrollPos_Keys = GUILayout.BeginScrollView( mScrollPos_Keys, false, false, "horizontalScrollbar", "verticalScrollbar", "AS TextArea", GUILayout.MinHeight(Height), GUILayout.MaxHeight(Screen.height), GUILayout.ExpandHeight(false));

			bool bAnyValidUsage = false;

			mRowSize = EditorStyles.toolbar.fixedHeight;
			if (Event.current!=null && Event.current.type == EventType.Layout)
				ScrollHeight = mScrollPos_Keys.y;

			float YPosMin = -ScrollHeight;
			int nSkip = 0;
			int nDraw = 0;
			if (TermsListHeight<=0)
				TermsListHeight = Screen.height;

			if (mShowableTerms.Count == 0)
				UpdateTermsToShownInList ();

			float SkipSize = 0;
			foreach (var parsedTerm in mShowableTerms)
			{
				string sKey = parsedTerm.Term;
				string sCategory = parsedTerm.Category;
				string FullKey = parsedTerm.FullTerm;

				int nUses = parsedTerm.Usage;
				bAnyValidUsage = bAnyValidUsage | (nUses>=0);

				ShowTerm_termData = parsedTerm.termData;

				// Skip lines outside the view -----------------------
				YPosMin += mRowSize;
				SkipSize += mRowSize;
				float YPosMax = YPosMin + mRowSize;
				bool isExpanded = (AllowExpandKey && mKeyToExplore==FullKey);
				if (!isExpanded && (YPosMax<-2*mRowSize || YPosMin>/*Screen.height*/TermsListHeight+mRowSize))
				{
					if (YPosMin>TermsListHeight+mRowSize)
						break;

					nSkip++;
					continue;
				}
				nDraw++;

				//------------------------------------------------------

				OnGUI_KeyHeader (sKey, sCategory, FullKey, nUses, YPosMin-mRowSize+mScrollPos_Keys.y);

				//--[ Key Details ]-------------------------------
				
				if (isExpanded)
				{
					GUILayout.Space(SkipSize);
					SkipSize = 0;
					OnGUI_KeyList_ShowKeyDetails();
					Rect rect = GUILayoutUtility.GetLastRect();
					if (rect.height>5)
						ExpandedViewHeight = rect.height;
					YPosMin += ExpandedViewHeight;
				}
			}
			SkipSize += (mShowableTerms.Count - nDraw-nSkip) * mRowSize;
			GUILayout.Space(SkipSize);
			OnGUI_KeysList_AddKey();

			GUILayout.Label("", GUILayout.Width(mTermList_MaxWidth+10+30), GUILayout.Height(1));

			GUILayout.EndScrollView();

			Rect ListRect = GUILayoutUtility.GetLastRect();
			if (ListRect.height>5)
				TermsListHeight = ListRect.height;
			
			OnGUI_Keys_ListSelection();    // Selection Buttons
			
//			if (!bAnyValidUsage)
//				EditorGUILayout.HelpBox("Use (Tools\\Parse Terms) to find how many times each of the Terms are used", UnityEditor.MessageType.Info);

			if (ShowTools)
			{
				GUILayout.BeginHorizontal();
				GUI.enabled = (mSelectedKeys.Count>0 || !string.IsNullOrEmpty(mKeyToExplore));
					if (GUILayout.Button (new GUIContent("Add Terms", "Add terms to Source"))) 		 AddTermsToSource();
					if (GUILayout.Button (new GUIContent("Remove Terms", "Remove Terms from Source"))) 	 RemoveTermsFromSource();

					GUILayout.FlexibleSpace ();

					if (GUILayout.Button ("Change Category")) OpenTool_ChangeCategoryOfSelectedTerms();
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace ();
					bool newBool = GUILayout.Toggle(mLanguageSource.CaseInsensitiveTerms, "Case Insensitive Terms");
					if (newBool != mLanguageSource.CaseInsensitiveTerms)
					{
						mProp_CaseInsensitiveTerms.boolValue = newBool;
					}
					GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal();
			}
			//Debug.Log ("Draw: " + nDraw + " Skip: " + nSkip);
		}

		void UpdateTermsToShownInList()
		{
			mShowableTerms.Clear ();
			foreach (KeyValuePair<string, ParsedTerm> kvp in mParsedTerms)
			{
				ParsedTerm parsedTerm = kvp.Value;
				if (ShouldShowTerm (parsedTerm.Term, parsedTerm.Category, parsedTerm.Usage, parsedTerm))
					mShowableTerms.Add(parsedTerm);
			}
		}

		void OnGUI_KeyHeader (string sKey, string sCategory, string FullKey, int nUses, float YPosMin)
		{
			//--[ Toggle ]---------------------
			GUI.Box(new Rect(2, YPosMin+2, 18, mRowSize), "", "Toolbar");
			OnGUI_SelectableToogleListItem (new Rect(2, YPosMin+3, 15, mRowSize), FullKey, ref mSelectedKeys, "OL Toggle");

			bool bEnabled = mSelectedKeys.Contains (FullKey);
			//--[ Number of Objects using this key ]---------------------
			if (nUses >= 0) 
			{
				if (nUses == 0) 
				{
					GUI.color = Color.Lerp (Color.gray, Color.white, 0.5f);
					GUI.Label (new Rect(20, YPosMin+2, 30, mRowSize), nUses.ToString (), "toolbarbutton");
				}
				else 
				{
					if (GUI.Button (new Rect(20, YPosMin+2, 30, mRowSize), nUses.ToString (), "toolbarbutton"))
						SelectObjectsUsingKey (FullKey);
				}
			}
			else 
			{
				GUI.color = Color.Lerp (Color.red, Color.white, 0.6f);
				if (GUI.Button (new Rect(20, YPosMin+2, 30, mRowSize), "", "toolbarbutton")) 
				{
					mCurrentToolsMode = eToolsMode.Parse;
					mCurrentViewMode = eViewMode.Tools;
				}
			}
			GUI.color = Color.white;

			TermData termData = ShowTerm_termData!=null ? ShowTerm_termData : mLanguageSource.GetTermData (FullKey);
			bool bKeyIsMissing = (termData == null);
			float MinX = 50;
			if (bKeyIsMissing) 
			{
				Rect rect = new Rect(50, YPosMin+2, mRowSize, mRowSize+2);
				GUI.DrawTexture (rect, GUI.skin.GetStyle ("CN EntryWarn").normal.background);
				GUI.Label (rect, new GUIContent ("", "This term is used in the scene, but its not localized in the Language Source"));
				MinX += rect.width;
			}
			else MinX += 3;

			float listWidth = Mathf.Max (Screen.width, mTermList_MaxWidth);
			Rect rectKey = new Rect(MinX, YPosMin+2, listWidth-MinX, mRowSize);
			if (mKeyToExplore == FullKey) 
			{
				GUI.backgroundColor = Color.Lerp (Color.blue, Color.white, 0.8f);
				if (GUI.Button (rectKey, new GUIContent (sKey, EditorStyles.foldout.onNormal.background), "AS TextArea")) 
				{
					mKeyToExplore = string.Empty;
					ClearErrors ();
				}
				GUI.backgroundColor = Color.white;
			}
			else 
			{
				GUIStyle LabelStyle = EditorStyles.label;
				if (!bKeyIsMissing && !TermHasAllTranslations (termData)) 
				{
					LabelStyle = new GUIStyle (EditorStyles.label);
					LabelStyle.fontStyle = FontStyle.Italic;
					GUI.color = Color.Lerp (Color.white, Color.yellow, 0.5f);
				}
				if (!bEnabled)
					GUI.contentColor = Color.Lerp (Color.gray, Color.white, 0.3f);
				if (GUI.Button (rectKey, sKey, LabelStyle)) 
				{
					SelectTerm (FullKey);
					ClearErrors ();
				}
				if (!bEnabled)
					GUI.contentColor = Color.white;
				GUI.color = Color.white;
			}
			//--[ Category ]--------------------------
			if (sCategory != LanguageSource.EmptyCategory) 
			{
				if (mKeyToExplore == FullKey) 
				{
                    rectKey.x = listWidth - 100-38;
					rectKey.width = 100;
					if (GUI.Button (rectKey, sCategory, EditorStyles.toolbarButton))
						OpenTool_ChangeCategoryOfSelectedTerms ();
				}
				else
				{
					GUIStyle stl = new GUIStyle(EditorStyles.miniLabel);
					stl.alignment = TextAnchor.MiddleRight;
					rectKey.width = 100;//EditorStyles.miniLabel.CalcSize(new GUIContent(sCategory)).x;
					rectKey.x = listWidth - rectKey.width - 38-13;

					if (GUI.Button (rectKey, sCategory, stl)) 
					{
						SelectTerm (FullKey);
						ClearErrors ();
					}
				}
			}
		}


		void CalculateTermsListMaxWidth()
		{
			mTermList_MaxWidth = 0;
			foreach (KeyValuePair<string, ParsedTerm> kvp in mParsedTerms)
			{
				var size = EditorStyles.label.CalcSize(new GUIContent(kvp.Key));
				mTermList_MaxWidth  = Mathf.Max (mTermList_MaxWidth, size.x);
			}
		}

		bool TermHasAllTranslations( TermData data )
		{
			for (int i=0, imax=data.Languages.Length; i<imax; ++i)
				if (string.IsNullOrEmpty(data.Languages[i]) && string.IsNullOrEmpty(data.Languages_Touch[i]))
					return false;
			return true;
		}

		void OnGUI_KeysList_AddKey()
		{
			GUILayout.BeginHorizontal();
				GUI.color = Color.Lerp(Color.gray, Color.white, 0.5f);
				bool bWasEnabled = (mTermsList_NewTerm!=null);
				bool bEnabled = !GUILayout.Toggle (!bWasEnabled, "+", EditorStyles.toolbarButton, GUILayout.Width(30));
				GUI.color = Color.white;

				if (bWasEnabled  && !bEnabled) mTermsList_NewTerm = null;
				if (!bWasEnabled &&  bEnabled) mTermsList_NewTerm = string.Empty;

				if (bEnabled)
				{
					GUILayout.BeginHorizontal(EditorStyles.toolbar);
					mTermsList_NewTerm = EditorGUILayout.TextField(mTermsList_NewTerm, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
					GUILayout.EndHorizontal();

					LanguageSource.ValidateFullTerm( ref mTermsList_NewTerm );
					if (string.IsNullOrEmpty(mTermsList_NewTerm) || mLanguageSource.ContainsTerm(mTermsList_NewTerm))
						GUI.enabled = false;
	
					if (GUILayout.Button ("Create Key", "toolbarbutton", GUILayout.ExpandWidth(false)))
					{
						AddTerm(mTermsList_NewTerm);
						SelectTerm( mTermsList_NewTerm );
						ClearErrors();
						mTermsList_NewTerm = null;
						SetAllTerms_When_InferredTerms_IsInSource ();
					}
					GUI.enabled = true;
				}
			GUILayout.EndHorizontal();
		}

		void OpenTool_ChangeCategoryOfSelectedTerms()
		{
			mCurrentViewMode = eViewMode.Tools;
			mCurrentToolsMode = eToolsMode.Categorize;
			if (!string.IsNullOrEmpty(mKeyToExplore) && !mSelectedKeys.Contains(mKeyToExplore))
				mSelectedKeys.Add(mKeyToExplore);
			mSelectedKeys.Sort();
		}

		void OnGUI_SelectedCategories()
		{
			List<string> mCategories = new List<string>();
			mCategories.AddRange( mParsedCategories.Keys );

			if (mCategories.Count==0)
				return;

			//--[ Compress Mask ]-------------------
			int Mask = 0;
			for (int i=0, imax=mCategories.Count; i<imax; ++i)
				if (mSelectedCategories.Contains( mCategories[i] ))
					Mask |= (1<<i);
			
			//--[ GUI ]-----------------------------
			GUI.changed = false;
			Mask = EditorGUILayout.MaskField(Mask, mCategories.ToArray(), EditorStyles.toolbarDropDown, GUILayout.Width(100));

			//--[ Decompress Mask ]-------------------
			if (GUI.changed)
			{
				GUI.changed = false;
				mSelectedCategories.Clear();
				mShowableTerms.Clear ();
				for (int i=0, imax=mCategories.Count; i<imax; ++i)
					if ( (Mask & (1<<i)) > 0 )
						mSelectedCategories.Add (mCategories[i]);
			}
		}

		// Bottom part of the Key list (buttons: All, None, Used,...  to select the keys)
		void OnGUI_Keys_ListSelection()
		{
			GUILayout.BeginHorizontal("toolbarbutton");
			
			if (GUILayout.Button(new GUIContent("All","Selects All Terms in the list"), "toolbarbutton", GUILayout.ExpandWidth(false)))  
			{ 
				mSelectedKeys.Clear();
				foreach (var kvp in mParsedTerms)
					if (ShouldShowTerm(kvp.Value.Term, kvp.Value.Category, kvp.Value.Usage))
						mSelectedKeys.Add ( kvp.Key );
			}
			if (GUILayout.Button(new GUIContent("None","Clears the selection"), "toolbarbutton", GUILayout.ExpandWidth(false))) { mSelectedKeys.Clear(); }
			GUILayout.Space (5);
			
			GUI.enabled = ((mFlagsViewKeys & (int)eFlagsViewKeys.Used)>1);
			if (GUILayout.Button(new GUIContent("Used","Selects All Terms referenced in the parsed scenes"), "toolbarbutton", GUILayout.ExpandWidth(false))) 
			{ 
				mSelectedKeys.Clear(); 
				foreach (var kvp in mParsedTerms)
					if (kvp.Value.Usage > 0 && ShouldShowTerm(kvp.Value.Term, kvp.Value.Category, kvp.Value.Usage))
						mSelectedKeys.Add ( kvp.Key );
			}
			GUI.enabled = ((mFlagsViewKeys & (int)eFlagsViewKeys.NotUsed)>1);
			if (GUILayout.Button(new GUIContent("Not Used", "Selects all Terms from the Source that are not been used"), "toolbarbutton", GUILayout.ExpandWidth(false))) 
			{ 
				mSelectedKeys.Clear(); 
				foreach (var kvp in mParsedTerms)
					if (kvp.Value.Usage == 0 && ShouldShowTerm(kvp.Value.Term, kvp.Value.Category, kvp.Value.Usage))
						mSelectedKeys.Add ( kvp.Key );
			}

			GUI.enabled = ((mFlagsViewKeys & (int)eFlagsViewKeys.Missing)>1);
			if (GUILayout.Button(new GUIContent("Missing","Selects all Terms Used but not defined in the Source"), "toolbarbutton", GUILayout.ExpandWidth(false)))
			{ 
				mSelectedKeys.Clear(); 
				foreach (var kvp in mParsedTerms)
					if (!mLanguageSource.ContainsTerm( kvp.Key ) && ShouldShowTerm(kvp.Value.Term, kvp.Value.Category, kvp.Value.Usage))
						mSelectedKeys.Add ( kvp.Key );
			}
			GUI.enabled = true;
			GUI.SetNextControlName ("TermsFilter");

			GUI.changed = false;
			KeyList_Filter = EditorGUILayout.TextField(KeyList_Filter, GUI.skin.GetStyle("ToolbarSeachTextField"), GUILayout.ExpandWidth(true));
			if (GUILayout.Button (string.Empty, string.IsNullOrEmpty(KeyList_Filter) ? "ToolbarSeachCancelButtonEmpty" : "ToolbarSeachCancelButton", GUILayout.ExpandWidth(false)))
				KeyList_Filter = string.Empty;

			if (GUI.changed)
			{
				mShowableTerms.Clear();
				GUI.changed = false;
			}

			GUILayout.EndHorizontal();
		}
		
		
		#endregion

		#region Filtering

		public bool ShouldShowTerm (string FullTerm)
		{
			ParsedTerm termData;
			if (!mParsedTerms.TryGetValue(FullTerm, out termData))
				return false;
			
			return ShouldShowTerm (termData.Term, termData.Category, termData.Usage, termData);
		}

		private TermData ShowTerm_termData;
		public bool ShouldShowTerm (string Term, string Category, int nUses, ParsedTerm parsedTerm=null )
		{
			if (!string.IsNullOrEmpty(Category) && !mSelectedCategories.Contains(Category)) 
				return false;
			
			if (!StringContainsFilter(Term, KeyList_Filter)) 
				return false;

			if (!string.IsNullOrEmpty(Category) && Category!=LanguageSource.EmptyCategory)
				Term = Category + "/" + Term;

			if (parsedTerm != null && parsedTerm.termData != null)
				ShowTerm_termData = parsedTerm.termData;
			else
			{
				ShowTerm_termData = mLanguageSource.GetTermData (Term);
				if (parsedTerm!=null)
					parsedTerm.termData = ShowTerm_termData;
			}
			bool bIsMissing = ShowTerm_termData == null;

			if (nUses<0) return true;

			if ((mFlagsViewKeys & (int)eFlagsViewKeys.Missing)>0 && bIsMissing) return true;
			if ((mFlagsViewKeys & (int)eFlagsViewKeys.Missing)==0 && bIsMissing) return false;

			if ((mFlagsViewKeys & (int)eFlagsViewKeys.Used)>0 && nUses>0) return true;
			if ((mFlagsViewKeys & (int)eFlagsViewKeys.NotUsed)>0 && nUses==0) return true;

			return false;
		}

		bool StringContainsFilter( string Term, string Filter )
		{
			if (string.IsNullOrEmpty(Filter))
				return true;
			Term = Term.ToLower();
			string[] Filters = Filter.ToLower().Split(";,".ToCharArray());
			for (int i=0, imax=Filters.Length; i<imax; ++i)
				if (Term.Contains(Filters[i]))
					return true;
			
			return false;
		}

		#endregion
		
		#region Add/Remove Keys to DB
		
		void AddTermsToSource()
		{
			if (!string.IsNullOrEmpty (mKeyToExplore) && !mSelectedKeys.Contains(mKeyToExplore))
				mSelectedKeys.Add (mKeyToExplore);

			for (int i=mSelectedKeys.Count-1; i>=0; --i)
			{
				string key = mSelectedKeys[i];

				if (!ShouldShowTerm(key))
					continue;

				AddTerm(key);
				mSelectedKeys.RemoveAt(i);
			}
			SetAllTerms_When_InferredTerms_IsInSource ();
		}
		
		void RemoveTermsFromSource()
		{
			if (!string.IsNullOrEmpty (mKeyToExplore) && !mSelectedKeys.Contains(mKeyToExplore))
				mSelectedKeys.Add (mKeyToExplore);
						
			for (int i=mSelectedKeys.Count-1; i>=0; --i)
			{
				string key = mSelectedKeys[i];
				
				if (!ShouldShowTerm(key)) 
					continue;

				DeleteTerm(key);
			}

            EditorApplication.update += DoParseTermsInCurrentScene;
		}

		#endregion
		
		#region Select Objects in Current Scene


		public static void SelectTerm( string Key, bool SwitchToKeysTab=false )
		{
			GUI.FocusControl(null);
			mKeyToExplore = Key;
			mKeysDesc_AllowEdit = false;
			if (SwitchToKeysTab)
				mCurrentViewMode = eViewMode.Keys;
		}


		void SelectObjectsUsingKey( string Key )
		{
			List<GameObject> SelectedObjs = new List<GameObject>();
			
			Localize[] Locals = (Localize[])Resources.FindObjectsOfTypeAll(typeof(Localize));
			
			if (Locals==null)
				return;
			
			for (int i=0, imax=Locals.Length; i<imax; ++i)
			{
				Localize localize = Locals[i];
				if (localize==null || localize.gameObject==null || !GUITools.ObjectExistInScene(localize.gameObject))
					continue;

				string Term, SecondaryTerm;
				localize.GetFinalTerms( out Term, out SecondaryTerm );

				if (Key==Term || Key==SecondaryTerm)
					SelectedObjs.Add (localize.gameObject);
			}

			if (SelectedObjs.Count>0)
				Selection.objects = SelectedObjs.ToArray();
			else
				ShowWarning("The selected Terms are not used in this Scene. Try opening other scenes"); 
		}

		#endregion

	}
}