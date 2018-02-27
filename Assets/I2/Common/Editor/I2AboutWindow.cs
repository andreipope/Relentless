﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace I2
{
	[Serializable]
	public class PluginsManagerData
	{
		public PluginData[] Plugins;
		public AssetData[] Assets;
		public AssetData[] Games;
		public string WebURL, ContactUs, AssetStoreURL, Videos;
		public string AlertText;
	}

	[Serializable]
	public class PluginData
	{
		[XmlAttribute]public string Name;
		public string Description;
		public string TrailerURL, DocumentationURL, TutorialsURL, ForumURL, AssetStoreURL;

		public string AssetStoreVersion, BetaVersion;
	}

	[Serializable]
	public class AssetData
	{
		[XmlAttribute]public string Name;
		public string URL;
	}

	[InitializeOnLoad]
	public class I2AboutHelper
	{
		public static bool DownloadError = false;
		public static WWW wwwPluginData = null;

		static I2AboutHelper()
		{
			string LastUpdateTime = EditorPrefs.GetString("I2AboutWindow Downloaded Time","");
			bool ShouldCheck = true;
			if (!string.IsNullOrEmpty(LastUpdateTime))
			{
				DateTime LastTime;
				if (!DateTime.TryParse(LastUpdateTime, out LastTime))
					ShouldCheck = true;
				else
				{
					double days = (DateTime.Now-LastTime).TotalDays;
					if (days>=1)
						ShouldCheck = true;
					else
					{
						/*if (days>=1)
						{
							I2AboutWindow.LoadPluginsData();
							EditorApplication.update += CheckIfAnyPluginNeedsUpgrading;						
						}*/
						//ShouldCheck = false;
					}
				}
			}
			
			if (ShouldCheck)
				StartConnection();
		}

		public static void StartConnection()
		{
			wwwPluginData = new WWW("http://bit.ly/1F0fIQx");//http://goo.gl/FjiyOR");//http://inter-illusion.com/assets/I2AssetStore.xml");
			EditorApplication.update += CheckConectionResult;
		}

		static void CheckConectionResult()
		{
			if (wwwPluginData!=null)
			{
				if (wwwPluginData.isDone)
				{
					EditorApplication.update -= CheckConectionResult;
					if (string.IsNullOrEmpty(wwwPluginData.error))
					{
						if (DeserializeData(wwwPluginData.text))
						{
							I2AboutWindow.DownloadedData = wwwPluginData.text;
						}
					}
					else
					{
						I2AboutWindow.LoadPluginsData();
					}
					wwwPluginData = null;
					CheckIfAnyPluginNeedsUpgrading();
				}
			}
			else
				EditorApplication.update -= CheckConectionResult;
		}

		public static bool DeserializeData( string dataXML )
		{
			try
			{
				System.IO.StringReader reader = new System.IO.StringReader(dataXML);
				System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer( typeof(PluginsManagerData) );
				I2AboutWindow.PluginsData = serializer.Deserialize( reader ) as PluginsManagerData;
			}catch (Exception){}

			return I2AboutWindow.PluginsData!=null;
		}

		static void CheckIfAnyPluginNeedsUpgrading()
		{
			EditorApplication.update -= CheckIfAnyPluginNeedsUpgrading;
			for (int i=0; i<I2AboutWindow.PluginsData.Plugins.Length; ++i)
			{
				string InstalledVersion = string.Empty;
				bool ShouldUpgrade = false;
				bool HasNewBeta = false;
				bool ShouldSkip = false;
				
				I2AboutWindow.GetShouldUpgrade( I2AboutWindow.PluginsData.Plugins[i], out InstalledVersion, out ShouldUpgrade, out HasNewBeta, out ShouldSkip );
				if (InstalledVersion=="none" || ShouldSkip)
					continue;
				
				if ((ShouldUpgrade && I2AboutWindow.bNotifyOfNewMainVersions) || (HasNewBeta && I2AboutWindow.bNotifyOfNewBetas))
				{
					//I2AboutWindow.DoShowScreen();
					return;
				}
			}

		}
	}

	public class I2AboutWindow	: EditorWindow
	{
		#region Styles
		
		static GUIStyle GUIStyle_Header {
			get{
				if (mGUIStyle_Header==null)
				{
					mGUIStyle_Header = new GUIStyle("HeaderLabel");
					mGUIStyle_Header.fontSize = 35;
					mGUIStyle_Header.normal.textColor = Color.Lerp(Color.white, Color.gray, 0.5f);
					mGUIStyle_Header.fontStyle = FontStyle.BoldAndItalic;
					mGUIStyle_Header.alignment = TextAnchor.UpperCenter;
				}
				return mGUIStyle_Header;
			}
		}
		static GUIStyle mGUIStyle_Header;

		//-----------------------------------

		static GUIStyle GUIStyle_PluginHeader {
			get{
				if (mGUIStyle_PluginHeader==null)
				{
					mGUIStyle_PluginHeader = new GUIStyle("HeaderLabel");
					mGUIStyle_PluginHeader.fontSize = 25;
					mGUIStyle_PluginHeader.normal.textColor = Color.Lerp(Color.white, Color.yellow, 0.8f);
					mGUIStyle_PluginHeader.fontStyle = FontStyle.BoldAndItalic;
					mGUIStyle_PluginHeader.alignment = TextAnchor.UpperCenter;
					mGUIStyle_PluginHeader.margin.top = -50;
				}
				return mGUIStyle_PluginHeader;
			}
		}
		static GUIStyle mGUIStyle_PluginHeader;

		//-----------------------------------
		
		static GUIStyle GUIStyle_SubHeader {
			get{
				if (mGUIStyle_SubHeader==null)
				{
					mGUIStyle_SubHeader = new GUIStyle("HeaderLabel");
					mGUIStyle_SubHeader.fontSize = 13;
					mGUIStyle_SubHeader.fontStyle = FontStyle.Italic;
					mGUIStyle_SubHeader.normal.textColor = Color.Lerp(Color.white, Color.yellow, 0.3f);
					mGUIStyle_SubHeader.margin.top = -50;
					mGUIStyle_SubHeader.alignment = TextAnchor.UpperCenter;
				}
				return mGUIStyle_SubHeader;
			}
		}
		static GUIStyle mGUIStyle_SubHeader;

		//-----------------------------------
		
		static GUIStyle GUIStyle_SectionHeader {
			get{
				if (mGUIStyle_SectionHeader==null)
				{
					mGUIStyle_SectionHeader = new GUIStyle("HeaderLabel");
					mGUIStyle_SectionHeader.fontSize = 20;
					mGUIStyle_SectionHeader.normal.textColor = Color.white;
					mGUIStyle_SectionHeader.fontStyle = FontStyle.Normal;
					mGUIStyle_SectionHeader.alignment = TextAnchor.UpperCenter;
					mGUIStyle_SectionHeader.margin.top = -50;
				}
				return mGUIStyle_SectionHeader;
			}
		}
		static GUIStyle mGUIStyle_SectionHeader;

		//-----------------------------------
		
		static GUIStyle GUIStyle_VersionLabel {
			get{
				if (mGUIStyle_VersionLabel==null)
				{
					mGUIStyle_VersionLabel = new GUIStyle(EditorStyles.label);
					mGUIStyle_VersionLabel.fontSize = 13;
					mGUIStyle_VersionLabel.fontStyle = FontStyle.Normal;
					mGUIStyle_VersionLabel.alignment = TextAnchor.UpperRight;
				}
				return mGUIStyle_VersionLabel;
			}
		}
		static GUIStyle mGUIStyle_VersionLabel;

		static GUIStyle GUIStyle_VersionUpgradeLabel {
			get{
				if (mGUIStyle_VersionUpgradeLabel==null)
				{
					mGUIStyle_VersionUpgradeLabel = new GUIStyle(EditorStyles.label);
					mGUIStyle_VersionUpgradeLabel.fontSize = 13;
					mGUIStyle_VersionUpgradeLabel.normal.textColor = Color.yellow;
					mGUIStyle_VersionUpgradeLabel.fontStyle = FontStyle.Bold;
					mGUIStyle_VersionUpgradeLabel.alignment = TextAnchor.UpperRight;
				}
				return mGUIStyle_VersionUpgradeLabel;
			}
		}
		static GUIStyle mGUIStyle_VersionUpgradeLabel;

		//-----------------------------------
		
		static GUIStyle GUIStyle_VersionTitle {
			get{
				if (mGUIStyle_VersionLabel==null)
				{
					mGUIStyle_VersionTitle = new GUIStyle(EditorStyles.label);
					mGUIStyle_VersionTitle.normal.textColor = Color.white;
					mGUIStyle_VersionTitle.fontSize = 13;
					mGUIStyle_VersionTitle.fontStyle = FontStyle.Bold;
					mGUIStyle_VersionTitle.alignment = TextAnchor.UpperRight;
				}
				return mGUIStyle_VersionTitle;
			}
		}
		static GUIStyle mGUIStyle_VersionTitle;

		
		#endregion

		#region Settings

		public static PluginsManagerData PluginsData;
		public static Dictionary<string, Texture2D> ImagePreviewDictionary = new Dictionary<string, Texture2D>();

		public static Dictionary<string, string> PluginsVersion = new Dictionary<string, string>();
		public static bool bNotifyOfNewBetas{
			get { return EditorPrefs.GetBool("I2AboutWindow Notify NewBetas", false); }
			set { EditorPrefs.SetBool("I2AboutWindow Notify NewBetas", value); }
		}
		public static bool bNotifyOfNewMainVersions{
			get { return EditorPrefs.GetBool("I2AboutWindow Notify NewMainVersions", true); }
			set { EditorPrefs.SetBool("I2AboutWindow Notify NewMainVersions", value); }
		}

		public static string DownloadedData {
			get { return EditorPrefs.GetString("I2AboutWindow Downloaded Data", string.Empty); }
			set { 
				EditorPrefs.SetString("I2AboutWindow Downloaded Data", value); 
				EditorPrefs.SetString("I2AboutWindow Downloaded Time", DateTime.Now.ToString());
			}
		}

		#endregion

		#region Editor and Loading Data

		[MenuItem("Help/About Inter-Illusion", false, 100 )]
		public static void DoShowScreen()
		{
			if (BuildPipeline.isBuildingPlayer || UnityEditorInternal.InternalEditorUtility.inBatchMode)
				return;
			
			PluginsData = null;
			EditorWindow.GetWindowWithRect<I2AboutWindow>(new Rect(0f, 0f, 670f, 510f), true, "Plugins Manager");
		}

		public static void LoadPluginsData()
		{
			string dataXML = /*string.Empty;*/DownloadedData;
			if (string.IsNullOrEmpty(dataXML))
			{
				LoadDefaultData();
			}
			else
			{
				I2AboutHelper.DeserializeData(dataXML);

				if (PluginsData==null)
					LoadDefaultData();
			}

			ImagePreviewDictionary.Clear();
			for (int i=0; i<PluginsData.Plugins.Length; ++i)
			{
				PluginData data = PluginsData.Plugins[i];
				ImagePreviewDictionary.Add (data.Name + "_Preview1", Resources.Load<Texture2D>(data.Name + "_Preview1"));
				ImagePreviewDictionary.Add (data.Name + "_Preview2", Resources.Load<Texture2D>(data.Name + "_Preview2"));
				ImagePreviewDictionary.Add (data.Name + "_Preview3", Resources.Load<Texture2D>(data.Name + "_Preview3"));
			}

			for (int i=0; i<PluginsData.Assets.Length; ++i)
				ImagePreviewDictionary.Add ("I2 Assets "+PluginsData.Assets[i].Name, Resources.Load<Texture2D>("I2 Assets "+PluginsData.Assets[i].Name));
			for (int i=0; i<PluginsData.Games.Length; ++i)
				ImagePreviewDictionary.Add ("I2 Games "+PluginsData.Games[i].Name, Resources.Load<Texture2D>("I2 Games "+PluginsData.Games[i].Name));

			/*System.IO.StringWriter writer = new System.IO.StringWriter(System.Globalization.CultureInfo.InvariantCulture);
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer( typeof(PluginsManagerData) );
			serializer.Serialize( writer, PluginsData );
			Debug.Log ( writer.ToString() );*/
		}

		static void LoadDefaultData ()
		{
			PluginsData = new PluginsManagerData () {
				WebURL = "http://www.inter-illusion.com",
				ContactUs = "http://www.inter-illusion.com/forum/index",
				AssetStoreURL = "https://www.assetstore.unity3d.com/en/#!/publisher/5916",
				Videos = "https://www.youtube.com/channel/UCBYYngAJJjCuoZhu1UbwoAw",
				AlertText = "Alert"
			};
			PluginsData.Plugins = new PluginData[2];
			PluginsData.Plugins [0] = new PluginData () {
				Name = "I2 Localization",
				Description = "The most complete Localization solution for Unity",
				TrailerURL = "https://www.youtube.com/watch?v=h27T3WFTOFE",
				DocumentationURL = "http://www.inter-illusion.com/assets/I2LocalizationManual/I2LocalizationManual.html",
				TutorialsURL = "http://inter-illusion.com/tools/i2-localization",
				ForumURL = "http://www.inter-illusion.com/forum/i2-localization",
				AssetStoreURL = "https://www.assetstore.unity3d.com/#!/content/14884",
				AssetStoreVersion = "2.6.1 f2",
				BetaVersion = "2.6.1 f2"
			};
			PluginsData.Plugins [1] = new PluginData () {
				Name = "I2 MiniGames",
				Description = "A clever alternative to Rewards and Energy Caps",
				TrailerURL = "https://www.youtube.com/channel/UCBYYngAJJjCuoZhu1UbwoAw",
				DocumentationURL = "http://inter-illusion.com/assets/MiniGamesManual/I2MiniGames.html",
				TutorialsURL = "http://inter-illusion.com/tools/i2-minigames",
				ForumURL = "http://inter-illusion.com/forum/minigames",
				AssetStoreURL = "http://u3d.as/ksR",
				AssetStoreVersion = "none",
				BetaVersion = "1.0.0 b1"
			};
			PluginsData.Assets = new AssetData[] {
				new AssetData () {
					Name = "Sci-Fi Indoor Facility",
					URL = "https://www.assetstore.unity3d.com/en/#!/content/31165"
				}
			};
			PluginsData.Games = new AssetData[] {
				new AssetData () {
					Name = "iTapPirate",
					URL = "http://inter-illusion.com/games/itappirate"
				},
				new AssetData () {
					Name = "Brush Master",
					URL = "http://inter-illusion.com/games/brush-master"
				}
			};
		}

		#endregion

		public void OnEnable()
		{
			I2AboutHelper.StartConnection();
		}

		public void OnGUI()
		{
			LoadPluginsData();

			GUIStyle mGUIStyle_Background = new GUIStyle("AS TextArea");
			mGUIStyle_Background.overflow.left = 50;
			mGUIStyle_Background.overflow.right = 50;
			mGUIStyle_Background.overflow.top = 50;
			mGUIStyle_Background.overflow.bottom = 50;

			GUI.backgroundColor = Color.Lerp (Color.black, Color.gray, 0.5f);
			GUILayout.BeginVertical(mGUIStyle_Background, GUILayout.ExpandHeight(true));
			GUI.backgroundColor = Color.white;

			
			GUILayout.Label("Inter Illusion", GUIStyle_Header);
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button ("Web", EditorStyles.miniLabel)) Application.OpenURL( PluginsData.WebURL );
				GUILayout.Space(15);
				if (GUILayout.Button ("Contact Us", EditorStyles.miniLabel)) Application.OpenURL( PluginsData.ContactUs );
				GUILayout.Space(15);
				if (GUILayout.Button ("Asset Store", EditorStyles.miniLabel)) Application.OpenURL( PluginsData.AssetStoreURL );
				GUILayout.Space(15);
				if (GUILayout.Button ("Videos", EditorStyles.miniLabel)) Application.OpenURL( PluginsData.Videos );
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();

		//---[ Plugins ]-----------------------------
			GUILayout.BeginHorizontal();
			GUILayout.Space (3);
			GUILayout.BeginVertical();
				for (int i=0; i<PluginsData.Plugins.Length; ++i)
				{
					OnGUI_Plugin(PluginsData.Plugins[i], (i==0 ? "flow node 1" : "flow node 5"));
					GUILayout.Space(5);
				}
			GUILayout.EndVertical();
			GUILayout.Space (3);
			GUILayout.EndHorizontal();

		//---[ Assets and Games ]-----------------------------
			GUILayout.FlexibleSpace();
			
			GUILayout.BeginHorizontal();
				OnGUI_Assets();
				GUILayout.FlexibleSpace();
				OnGUI_Games();
			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();

			GUIStyle ToolbarWide = new GUIStyle(EditorStyles.toolbar);
			ToolbarWide.fixedHeight = 0;

			GUILayout.BeginHorizontal(ToolbarWide, GUILayout.Height(20));
				bNotifyOfNewMainVersions = GUILayout.Toggle(bNotifyOfNewMainVersions, "Notify me of new versions");
				GUILayout.FlexibleSpace();
				bNotifyOfNewBetas = GUILayout.Toggle(bNotifyOfNewBetas, "Notify me of new betas");
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("I'll Check Later", EditorStyles.toolbarButton))
					this.Close();
				GUILayout.Space(10);
			GUILayout.EndHorizontal();

			OnGUI_ShowDownloading();

			GUILayout.EndVertical();
		}

		void OnGUI_ShowDownloading()
		{
			if (I2AboutHelper.wwwPluginData!=null)
			{
				int time = (int)((Time.realtimeSinceStartup % 2) * 2.5);
				string Loading = "Checking for new version" + ".....".Substring(0, time);
				Repaint();

				GUI.color = Color.gray;
				GUILayout.BeginHorizontal("AS TextArea");
					GUI.color = Color.white;
					GUILayout.Label (Loading, EditorStyles.miniLabel);
				GUILayout.EndHorizontal();
			}
		}

		void OnGUI_Plugin( PluginData pluginData, string BkgStyle )
		{
			GUIStyle style = new GUIStyle(BkgStyle);
			style.padding.top = 5;
			GUILayout.BeginHorizontal(style, GUILayout.Height(1));
				OnGUI_PluginDescription( pluginData );
				OnGUI_PluginVersion( pluginData );
			GUILayout.EndHorizontal();
		}

		void OnGUI_PluginDescription( PluginData pluginData )
		{
			GUILayout.BeginVertical();
				GUILayout.Label(pluginData.Name, GUIStyle_PluginHeader);
				GUILayout.Label(pluginData.Description, GUIStyle_SubHeader);
				GUILayout.FlexibleSpace();

				GUILayout.BeginHorizontal();

					GUILayout.Space(3);	OnGUI_PreviewImage( pluginData.Name+"_Preview1" );
					GUILayout.Space(3); OnGUI_PreviewImage( pluginData.Name+"_Preview2" );
					GUILayout.Space(3); OnGUI_PreviewImage( pluginData.Name+"_Preview3" );

					GUILayout.Space (5);

					GUILayout.BeginVertical();
						if (GUILayout.Button ("Trailer", EditorStyles.miniLabel)) 		Application.OpenURL( pluginData.TrailerURL );
						if (GUILayout.Button ("Tutorials", EditorStyles.miniLabel)) 	Application.OpenURL( pluginData.TutorialsURL );
					    if (GUILayout.Button ("Forum", EditorStyles.miniLabel)) 		Application.OpenURL( pluginData.ForumURL );
			    		if (GUILayout.Button ("Documentation", EditorStyles.miniLabel)) Application.OpenURL( pluginData.DocumentationURL );
			    		GUILayout.FlexibleSpace();
					GUILayout.EndVertical();

				GUILayout.EndHorizontal();

				GUILayout.Space(5);
			GUILayout.EndVertical();
		}

		void OnGUI_PreviewImage( string ImageName, int width=100, int height=70 )
		{
			Texture2D previewImage = null;

			ImagePreviewDictionary.TryGetValue(ImageName, out previewImage);

			if (previewImage!=null)
			{
				width = previewImage.width;
				height = previewImage.height;
				Rect rect = GUILayoutUtility.GetRect( width, height );
				EditorGUI.DrawPreviewTexture(rect, previewImage);
			}
			else
				GUILayout.Box("", GUILayout.Width(width), GUILayout.Height(height));
		}

		void OnGUI_PluginVersion( PluginData pluginData )
		{
			string InstalledVersion = string.Empty;
			bool HasNewMainVersion = false;
			bool HasNewBeta = false;
			bool shouldSkip = false;

			GetShouldUpgrade( pluginData, out InstalledVersion, out HasNewMainVersion, out HasNewBeta, out shouldSkip );

			GUILayout.BeginVertical(GUILayout.Width(150));

				GUILayout.FlexibleSpace ();

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label ("Installed Version", GUIStyle_VersionTitle);
					GUILayout.Label (InstalledVersion, GUIStyle_VersionLabel, GUILayout.Width(70));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label ("Asset Store", GUIStyle_VersionTitle);
					GUILayout.Label (pluginData.AssetStoreVersion, HasNewMainVersion ? GUIStyle_VersionUpgradeLabel : GUIStyle_VersionLabel, GUILayout.Width(70));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label ("Beta", GUIStyle_VersionTitle);
					GUILayout.Label (pluginData.BetaVersion, HasNewBeta ? GUIStyle_VersionUpgradeLabel : GUIStyle_VersionLabel, GUILayout.Width(70));
				GUILayout.EndHorizontal();

				if (HasNewBeta || HasNewMainVersion)
				{
					GUILayout.BeginHorizontal(GUILayout.Height(20));
						if (GUILayout.Button("Upgrade"))
							Application.OpenURL(pluginData.AssetStoreURL);
						GUILayout.Space(5);
						if (!shouldSkip && GUILayout.Button("Skip Version"))
						{
							EditorPrefs.SetString("SkipVersion "+pluginData.Name, pluginData.AssetStoreVersion+";"+pluginData.BetaVersion);
						}
						
					GUILayout.EndHorizontal();
					GUILayout.Space(5);
				}
				else
					GUILayout.Space(25);

			GUILayout.EndVertical();
		}

		public static void GetShouldUpgrade( PluginData pluginData, out string InstalledVersion, out bool ShouldUpgrade, out bool HasNewBeta, out bool shouldSkip)
		{
			if (PluginsVersion.TryGetValue(pluginData.Name, out InstalledVersion))
			{
				ShouldUpgrade = !string.IsNullOrEmpty(pluginData.AssetStoreVersion) && (pluginData.AssetStoreVersion!="none") && (pluginData.AssetStoreVersion.CompareTo(InstalledVersion)>0);
				HasNewBeta    = !string.IsNullOrEmpty(pluginData.BetaVersion) 		&& (pluginData.BetaVersion!="none") 	  && (pluginData.BetaVersion.CompareTo(InstalledVersion)>0);

				string skipVersion = EditorPrefs.GetString("SkipVersion "+pluginData.Name, "");
				shouldSkip = (skipVersion.Contains(pluginData.AssetStoreVersion) || 
  		              		  skipVersion.Contains(pluginData.BetaVersion));

			}
			else
			{
				InstalledVersion = "none";
				ShouldUpgrade 	 = !string.IsNullOrEmpty(pluginData.AssetStoreVersion) 	&& (pluginData.AssetStoreVersion!="none");
				HasNewBeta 		 = !string.IsNullOrEmpty(pluginData.BetaVersion) 		&& (pluginData.BetaVersion!="none");
				shouldSkip 		 = true;
			}
		}

		void OnGUI_Assets()
		{
			GUILayout.BeginVertical(GUILayout.Height(1), GUILayout.Width(1));
			
				GUILayout.Label("Assets", GUIStyle_SectionHeader);
				GUILayout.Space (5);

				GUILayout.BeginHorizontal();
				GUILayout.Space(30);
				for (int i=0; i<PluginsData.Assets.Length; ++i)
				{
					if (i!=0) GUILayout.Space(5);
					OnGUI_Asset( PluginsData.Assets[i], "I2 Assets " );
				}
				GUILayout.Space(30);
				GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}

		void OnGUI_Games()
		{
			GUILayout.BeginVertical(GUILayout.Height(1), GUILayout.Width(1));

				GUILayout.Label("Games", GUIStyle_SectionHeader);
				GUILayout.Space (5);
				GUILayout.BeginHorizontal();
				GUILayout.Space(30);
				for (int i=0; i<PluginsData.Games.Length; ++i)
				{
					if (i!=0) GUILayout.Space(10);
					OnGUI_Asset( PluginsData.Games[i], "I2 Games " );
				}
				GUILayout.Space(30);
				GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}

		void OnGUI_Asset (AssetData assetData, string ImagePrefix)
		{
			GUILayout.BeginVertical(GUILayout.Width(1));
				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					OnGUI_PreviewImage( ImagePrefix + assetData.Name );
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.Label(assetData.Name, GUIStyle_SubHeader);
			GUILayout.EndVertical();

			Rect rect = GUILayoutUtility.GetLastRect();
			if (GUI.Button ( rect, "", EditorStyles.label))
				Application.OpenURL( assetData.URL );
		}
	}
}