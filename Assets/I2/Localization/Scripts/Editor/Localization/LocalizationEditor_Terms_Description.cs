//#define UGUI
//#define NGUI
//#define DFGUI

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables	
		static internal bool mKeysDesc_AllowEdit = false;
		static internal int GUI_SelectedInputType
		{
			get{ if (mGUI_SelectedInputType<0) 
					mGUI_SelectedInputType = PlayerPrefs.GetInt ("I2 InputType", TermData.IsTouchType()?1:0);
				return mGUI_SelectedInputType; 
			}
			set{ if (value!=mGUI_SelectedInputType) 
					PlayerPrefs.SetInt ("I2 InputType", value);
				 mGUI_SelectedInputType = value;
			}
		}
		static internal int mGUI_SelectedInputType = -1;

		static internal int GUI_SelectedPluralType = 0;
		#endregion
		
		#region Key Description
		
		void OnGUI_KeyList_ShowKeyDetails()
		{
			GUI.backgroundColor = Color.Lerp(Color.blue, Color.white, 0.9f);
			GUILayout.BeginVertical("AS TextArea", GUILayout.Height(1));
			OnGUI_Keys_Languages(mKeyToExplore, null);
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Delete"))
				EditorApplication.update += DeleteCurrentKey;
			
			if (GUILayout.Button("Merge"))
			{
				mCurrentViewMode = eViewMode.Tools;
				mCurrentToolsMode = eToolsMode.Merge;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUI.backgroundColor = Color.white;
		}

		void DeleteTerm( string Term )
		{
			mLanguageSource.RemoveTerm (Term);
			RemoveParsedTerm(Term);
			mSelectedKeys.Remove(Term);
			if (Term==mKeyToExplore)
				mKeyToExplore = string.Empty;

			mTermList_MaxWidth = -1;			
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(mLanguageSource);
			mShowableTerms.Clear ();
		}
		
		void DeleteCurrentKey()
		{
			EditorApplication.update -= DeleteCurrentKey;
			DeleteTerm (mKeyToExplore);
			mKeyToExplore = "";
            EditorApplication.update += DoParseTermsInCurrentScene;
		}

		TermData AddTerm( string Term, bool AutoSelect = true )
		{
			TermData data = mLanguageSource.AddTerm(Term, eTermType.Text);
			GetParsedTerm(Term);

			if (AutoSelect)
			{
				if (!mSelectedKeys.Contains(Term))
					mSelectedKeys.Add(Term);

				string sCategory = LanguageSource.GetCategoryFromFullTerm(Term);
				if (!mSelectedCategories.Contains(sCategory))
				{
					mSelectedCategories.Add(sCategory);
					mShowableTerms.Clear ();
				}
			}
			mTermList_MaxWidth = -1;			
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(mLanguageSource);
			return data;
		}
		
		// this method shows the key description and the localization to each language
		public static void OnGUI_Keys_Languages( string Key, Localize localizeCmp, bool IsPrimaryKey=true )
		{
			if (Key==null)
				Key = string.Empty;

			TermData termdata = null;

            LanguageSource source = (localizeCmp == null ? mLanguageSource : localizeCmp.Source);
			if (source==null)
				source = LocalizationManager.GetSourceContaining(Key, false);

			if (source==null)
			{
				if (localizeCmp == null)
					source = LocalizationManager.Sources[0];
				else
					source = LocalizationManager.GetSourceContaining(IsPrimaryKey ? localizeCmp.SecondaryTerm : localizeCmp.Term, true);
			}


            if (string.IsNullOrEmpty(Key))
			{
				EditorGUILayout.HelpBox( "Select a Term to Localize", UnityEditor.MessageType.Info );
				return;
			}
			else
			{
               	termdata = source.GetTermData(Key);
				if (termdata==null && localizeCmp!=null)
				{
					var realSource = LocalizationManager.GetSourceContaining(Key, false);
					if (realSource != null)
					{
						termdata = realSource.GetTermData(Key);
						source = realSource;
					}
				}
				if (termdata==null)
				{
					EditorGUILayout.HelpBox( string.Format("Key '{0}' is not Localized or it is in a different Language Source", Key), UnityEditor.MessageType.Error );
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Add Term to Source"))
					{
                        source.AddTerm(Key, eTermType.Text);
						GetParsedTerm(Key);
						SetAllTerms_When_InferredTerms_IsInSource ();
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					
					return;
				}
			}

			//--[ Type ]----------------------------------
			if (localizeCmp==null)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label ("Type:", GUILayout.ExpandWidth(false));
				eTermType NewType = (eTermType)EditorGUILayout.EnumPopup(termdata.TermType, GUILayout.ExpandWidth(true));
				if (termdata.TermType != NewType)
					termdata.TermType = NewType;
				GUILayout.EndHorizontal();
			}


			//--[ Description ]---------------------------

			mKeysDesc_AllowEdit = GUILayout.Toggle(mKeysDesc_AllowEdit, "Description", EditorStyles.foldout, GUILayout.ExpandWidth(true));

			if (mKeysDesc_AllowEdit)
			{
                string NewDesc = EditorGUILayout.TextArea( termdata.Description, LocalizationEditor.Style_WrapTextField );
				if (NewDesc != termdata.Description)
				{
					termdata.Description = NewDesc;
					EditorUtility.SetDirty(source);
				}
			}
			else
                EditorGUILayout.HelpBox( string.IsNullOrEmpty(termdata.Description) ? "No description" : termdata.Description, UnityEditor.MessageType.Info );

			OnGUI_Keys_Language_SpecializationsBar ();

			//--[ Languages ]---------------------------
			GUILayout.BeginVertical ("AS TextArea", GUILayout.Height (1));

				OnGUI_Keys_LanguageTranslations (Key, localizeCmp, IsPrimaryKey, ref termdata, source);

				if (termdata.TermType == eTermType.Text)
				{
					GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if (GUILayout.Button("Translate All", GUILayout.Width(85)))
						{
							string mainText = localizeCmp == null ? LanguageSource.GetKeyFromFullTerm(Key) : localizeCmp.GetMainTargetsText ();

							for (int i=0; i<source.mLanguages.Count; ++ i)
								if (string.IsNullOrEmpty(termdata.Languages[i]))
								{
									if (GUI_SelectedInputType==0)
									{
										Translate( mainText, ref termdata, ref termdata.Languages[i], source.mLanguages[i].Code );
										termdata.Flags[i] |= (byte)TranslationFlag.AutoTranslated_Normal;
									}
									else
									{
										Translate( mainText, ref termdata, ref termdata.Languages_Touch[i], source.mLanguages[i].Code );
										termdata.Flags[i] |= (byte)TranslationFlag.AutoTranslated_Touch;
									}
								}
							GUI.FocusControl(string.Empty);
						}
					GUILayout.EndHorizontal();
				}
			GUILayout.EndVertical();
		}

		static void OnGUI_Keys_Language_SpecializationsBar()
		{
			GUILayout.BeginHorizontal();
				GUI_SelectedInputType = GUITools.DrawTabs(GUI_SelectedInputType, new string[]{"Normal|Transation for Non Touch devices.\nThis allows using 'click' instead of 'tap', etc", "Touch|Normal|Transation for Touch devices.\nThis allows using 'tap' instead of 'click', etc"}, null);
				GUI.enabled = false;
					GUI_SelectedPluralType = GUITools.DrawShadowedTabs(GUI_SelectedPluralType, new string[]{"Zero", "One", "Two", "Few", "Many", "Other"}, 18, false);
				GUI.enabled = true;
				GUI.Label( GUILayoutUtility.GetLastRect(), new GUIContent("",null,"Plurals are not enabled in this version.\nIt will be one of the new features for version 3"));
			GUILayout.EndHorizontal();
			GUILayout.Space(-1);
						

			//static public int DrawTabs( int Index, string[] Tabs, GUIStyle Style=null, int height=25, bool expand = true)
			/*GUIStyle MyStyle = new GUIStyle(Style!=null?Style:GUI.skin.FindStyle("dragtab"));
			MyStyle.fixedHeight=0;

			GUILayout.BeginHorizontal();
			for (int i=0; i<Tabs.Length; ++i)
			{
					int idx = Tabs[i].IndexOf('|');
					if (idx>0)
					{
							string text = Tabs[i].Substring(0, idx);
							string tooltip = Tabs[i].Substring(idx+1);
							if ( GUILayout.Toggle(Index==i, new GUIContent(text, tooltip), MyStyle, GUILayout.Height(height), GUILayout.ExpandWidth(expand)) && Index!=i) 
							{
									Index=i;
									GUI.FocusControl(string.Empty);
							}
					}
					else
					{
							if ( GUILayout.Toggle(Index==i, Tabs[i], MyStyle, GUILayout.Height(height), GUILayout.ExpandWidth(expand)) && Index!=i) 
							{
									Index=i;
									GUI.FocusControl(string.Empty);
							}
					}
			}
			GUILayout.EndHorizontal();
			return Index;*/
		}

		static void OnGUI_Keys_LanguageTranslations (string Key, Localize localizeCmp, bool IsPrimaryKey, ref TermData termdata, LanguageSource source)
		{
			bool IsSelect = Event.current.type==EventType.MouseUp;
			for (int i=0; i< source.mLanguages.Count; ++ i)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label (source.mLanguages[i].Name, GUILayout.Width(100));

				string Translation = (GUI_SelectedInputType==0 ? termdata.Languages[i] : termdata.Languages_Touch[i]) ?? string.Empty;
				if (string.IsNullOrEmpty(Translation))
				{
					Translation = (GUI_SelectedInputType==1 ? termdata.Languages[i] : termdata.Languages_Touch[i]) ?? string.Empty;
				}

				if (termdata.Languages[i] != termdata.Languages_Touch[i] && !string.IsNullOrEmpty(termdata.Languages[i]) && !string.IsNullOrEmpty(termdata.Languages_Touch[i]))
					GUI.contentColor = GUITools.LightYellow;

				if (termdata.TermType == eTermType.Text)
				{
					GUI.changed = false;
					string CtrName = "TranslatedText"+i;
					GUI.SetNextControlName(CtrName);

					bool autoTranslated = termdata.IsAutoTranslated( i, GUI_SelectedInputType==1 );

                    Translation = EditorGUILayout.TextArea(Translation, LocalizationEditor.Style_WrapTextField, GUILayout.Width(Screen.width-260 - (autoTranslated ? 20 : 0)));
					if (GUI.changed)
					{
						if (GUI_SelectedInputType==0)
						{
							termdata.Languages[i] = Translation;
							termdata.Flags[i] &= byte.MaxValue ^ (byte)TranslationFlag.AutoTranslated_Normal;
						}
						else
						{
							termdata.Languages_Touch[i] = Translation;
							termdata.Flags[i] &= byte.MaxValue ^ (byte)TranslationFlag.AutoTranslated_Touch;
						}
						EditorUtility.SetDirty(source);
					}

					if (localizeCmp!=null &&
					    (GUI.changed || (GUI.GetNameOfFocusedControl()==CtrName && IsSelect)))
					{
						if (IsPrimaryKey && string.IsNullOrEmpty(localizeCmp.Term))
						{
							localizeCmp.mTerm = Key;
							EditorUtility.SetDirty(localizeCmp);
						}

						if (!IsPrimaryKey && string.IsNullOrEmpty(localizeCmp.SecondaryTerm))
						{
							localizeCmp.mTermSecondary = Key;
							EditorUtility.SetDirty(localizeCmp);
						}

						string PreviousLanguage = LocalizationManager.CurrentLanguage;
						LocalizationManager.PreviewLanguage(source.mLanguages[i].Name);
						localizeCmp.OnLocalize(true);
						LocalizationManager.PreviewLanguage(PreviousLanguage);
					}
					GUI.contentColor = Color.white;

					if (autoTranslated)
					{
						if (GUILayout.Button(new GUIContent("\u2713"/*"A"*/,"Translated by Google Translator\nClick the button to approve the translation"), EditorStyles.toolbarButton, GUILayout.Width(autoTranslated ? 20 : 0)))
						{
							termdata.Flags[i] &= (byte)(byte.MaxValue ^ (byte)(GUI_SelectedInputType==0 ? TranslationFlag.AutoTranslated_Normal : TranslationFlag.AutoTranslated_Touch));
						}
					}

					if (GUILayout.Button("Translate", EditorStyles.toolbarButton, GUILayout.Width(80)))
					{
						string mainText = localizeCmp == null ? LanguageSource.GetKeyFromFullTerm(Key) : localizeCmp.GetMainTargetsText ();
						if (GUI_SelectedInputType==0)
						{
							Translate( mainText, ref termdata, ref termdata.Languages[i], source.mLanguages[i].Code );
							termdata.Flags[i] |= (byte)TranslationFlag.AutoTranslated_Normal;
						}
						else
						{
							Translate( mainText, ref termdata, ref termdata.Languages_Touch[i], source.mLanguages[i].Code );
							termdata.Flags[i] |= (byte)TranslationFlag.AutoTranslated_Touch;
						}
						GUI.FocusControl(string.Empty);
					}
				}
				else
				{
					string MultiSpriteName = string.Empty;

					if (termdata.TermType==eTermType.Sprite && Translation.EndsWith("]"))	// Handle sprites of type (Multiple):   "SpritePath[SpriteName]"
					{
						int idx = Translation.LastIndexOf("[");
						int len = Translation.Length-idx-2;
						MultiSpriteName = Translation.Substring(idx+1, len);
						Translation = Translation.Substring(0, idx);
					}

					Object Obj = null;

					// Try getting the asset from the References section
					if (localizeCmp!=null)
						Obj = localizeCmp.FindTranslatedObject<Object>(Translation);
					if (Obj==null && source != null)
						Obj = source.FindAsset(Translation);

					// If it wasn't in the references, Load it from Resources
					if (Obj==null && localizeCmp==null)
						Obj = ResourceManager.pInstance.LoadFromResources<Object>(Translation);

					System.Type ObjType = typeof(Object);
					switch (termdata.TermType)
					{
						case eTermType.Font			: ObjType = typeof(Font); break;
						case eTermType.Texture		: ObjType = typeof(Texture); break;
						case eTermType.AudioClip	: ObjType = typeof(AudioClip); break;
						case eTermType.GameObject	: ObjType = typeof(GameObject); break;
						case eTermType.Sprite		: ObjType = typeof(Sprite); break;
						case eTermType.Material		: ObjType = typeof(Material); break;
#if NGUI
						case eTermType.UIAtlas		: ObjType = typeof(UIAtlas); break;
						case eTermType.UIFont		: ObjType = typeof(UIFont); break;
#endif
#if DFGUI
						case eTermType.dfFont		: ObjType = typeof(dfFont); break;
						case eTermType.dfAtlas		: ObjType = typeof(dfAtlas); break;
#endif

#if TK2D
						case eTermType.TK2dFont			: ObjType = typeof(tk2dFont); break;
						case eTermType.TK2dCollection	: ObjType = typeof(tk2dSpriteCollection); break;
#endif

#if TextMeshPro_Pre53
						case eTermType.TextMeshPFont	: ObjType = typeof(TMPro.TextMeshProFont); break;
#elif TextMeshPro
						case eTermType.TextMeshPFont	: ObjType = typeof(TMPro.TMP_FontAsset); break;
#endif

#if SVG
						case eTermType.SVGAsset	: ObjType = typeof(SVGImporter.SVGAsset); break;
#endif

						case eTermType.Object		: ObjType = typeof(Object); break;
					}

					if (Obj!=null && !string.IsNullOrEmpty(MultiSpriteName))
					{
						string sPath = AssetDatabase.GetAssetPath(Obj);
						Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(sPath);
						Obj = null;
						for (int j=0, jmax=objs.Length; j<jmax; ++j)
							if (objs[j].name.Equals(MultiSpriteName))
							{
								Obj = objs[j];
								break;
							}
					}

					bool bShowTranslationLabel = (Obj==null && !string.IsNullOrEmpty(Translation));
					if (bShowTranslationLabel)
					{
						GUI.backgroundColor=GUITools.DarkGray;
						GUILayout.BeginVertical("AS TextArea", GUILayout.Height(1));
						GUILayout.Space(2);
						
						GUI.backgroundColor = Color.white;
					}

					Object NewObj = EditorGUILayout.ObjectField(Obj, ObjType, true, GUILayout.ExpandWidth(true));
					if (Obj!=NewObj && NewObj!=null)
					{
						string sPath = AssetDatabase.GetAssetPath(NewObj);
						AddObjectPath( ref sPath, localizeCmp, NewObj );
						if (HasObjectInReferences(NewObj, localizeCmp))
							sPath = NewObj.name;
						else
						if (termdata.TermType==eTermType.Sprite)
							sPath+="["+NewObj.name+"]";

						if (GUI_SelectedInputType==0)
							termdata.Languages[i] = sPath;
						else
							termdata.Languages_Touch[i] = sPath;
						EditorUtility.SetDirty(source);
					}

					if (bShowTranslationLabel)
					{
						GUILayout.BeginHorizontal();
							GUI.color = Color.red;
							GUILayout.FlexibleSpace();
							GUILayout.Label (Translation, EditorStyles.miniLabel);
							GUILayout.FlexibleSpace();
							GUI.color = Color.white;
						GUILayout.EndHorizontal();
						GUILayout.EndVertical();
					}
				}
				
				GUILayout.EndHorizontal();
			}
		}

		/*static public int DrawTranslationTabs( int Index )
		{
			GUIStyle MyStyle = new GUIStyle(GUI.skin.FindStyle("dragtab"));
			MyStyle.fixedHeight=0;
			
			GUILayout.BeginHorizontal();
			for (int i=0; i<Tabs.Length; ++i)
			{
				if ( GUILayout.Toggle(Index==i, Tabs[i], MyStyle, GUILayout.Height(height)) && Index!=i) 
					Index=i;
			}
			GUILayout.EndHorizontal();
			return Index;
		}*/

		static bool HasObjectInReferences( Object obj, Localize localizeCmp )
		{
			if (localizeCmp!=null && System.Array.IndexOf(localizeCmp.TranslatedObjects, obj)>=0)
				return true;

			if (mLanguageSource!=null && System.Array.IndexOf(mLanguageSource.Assets, obj)>=0)
				return true;

			return false;
		}

		static void AddObjectPath( ref string sPath, Localize localizeCmp, Object NewObj )
		{
			if (RemoveResourcesPath (ref sPath))
				return;

			// If its not in the Resources folder and there is no object reference already in the
			// Reference array, then add that to the Localization component or the Language Source
			if (HasObjectInReferences(NewObj, localizeCmp))
				return;

			if (localizeCmp!=null)
			{
				int Length = localizeCmp.TranslatedObjects.Length;
				System.Array.Resize( ref localizeCmp.TranslatedObjects, Length+1);
				localizeCmp.TranslatedObjects[Length] = NewObj;
				EditorUtility.SetDirty(localizeCmp);
			}
			else
			if (mLanguageSource!=null)
			{
				int Length = mLanguageSource.Assets.Length;
				System.Array.Resize( ref mLanguageSource.Assets, Length+1);
				mLanguageSource.Assets[Length] = NewObj;
				EditorUtility.SetDirty(mLanguageSource);
			}
		}
		
		static void Translate ( string Key, ref TermData termdata, ref string sTarget, string TargetLanguageCode )
		{
			#if UNITY_WEBPLAYER
			ShowError ("Contacting google translation is not yet supported on WebPlayer" );
			#else

			if (!GoogleTranslation.CanTranslate())
			{
				ShowError ("WebService is not set correctly or needs to be reinstalled");
				return;
			}

			// Translate first language that has something
			// If no language found, translation will fallback to autodetect language from key

			string sourceCode, sourceText;
			FindTranslationSource( Key, termdata, TargetLanguageCode, out sourceText, out sourceCode );
			sTarget = GoogleTranslation.ForceTranslate( sourceText, sourceCode, TargetLanguageCode );
			
			#endif
		}

		static void FindTranslationSource( string Key, TermData termdata, string TargetLanguageCode, out string sourceText, out string sourceLanguageCode )
		{
			sourceLanguageCode = "auto";
			sourceText = Key;
			
			string[] lans1 = (GUI_SelectedInputType==0 ? termdata.Languages : termdata.Languages_Touch);
			string[] lans2 = (GUI_SelectedInputType==0 ? termdata.Languages_Touch : termdata.Languages);

			for (int i=0, imax=lans1.Length; i<imax; ++i)
				if (!string.IsNullOrEmpty(lans1[i]))
				{
					sourceText = lans1[i];
					if (mLanguageSource.mLanguages[i].Code != TargetLanguageCode)
					{
						sourceLanguageCode = mLanguageSource.mLanguages[i].Code;
						return;
					}
				}
			for (int i=0, imax=lans2.Length; i<imax; ++i)
				if (!string.IsNullOrEmpty(lans2[i]))
				{
					sourceText = lans2[i];
					if (mLanguageSource.mLanguages[i].Code != TargetLanguageCode)
					{
						sourceLanguageCode = mLanguageSource.mLanguages[i].Code;
						return;
					}
				}			
		}
		
		#endregion
	}
}
