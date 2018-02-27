using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace I2.Loc
{
	public enum eSpreadsheetUpdateMode { None, Replace, Merge, AddNewTerms };

	public partial class LanguageSource
	{
		public WWW Export_Google_CreateWWWcall( eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace )
		{
			#if UNITY_WEBPLAYER
			Debug.Log ("Contacting google translation is not yet supported on WebPlayer" );
			return null;
			#else
			string Data = Export_Google_CreateData();

			WWWForm form = new WWWForm();
			form.AddField("key", Google_SpreadsheetKey);
			form.AddField("action", "SetLanguageSource");
			form.AddField("data", Data);
			form.AddField("updateMode", UpdateMode.ToString());

			WWW www = new WWW(Google_WebServiceURL, form);
			return www;
			#endif
		}

		string Export_Google_CreateData()
		{
			List<string> Categories = GetCategories(true);
			StringBuilder Builder = new StringBuilder();
			
			bool bFirst = true;
			foreach (string category in Categories)
			{
				if (bFirst)
					bFirst = false;
				else
					Builder.Append("<I2Loc>");

				//string CSV = Export_CSV(category);
				string CSV = Export_I2CSV(category);
				Builder.Append(category);
				Builder.Append("<I2Loc>");
				Builder.Append(CSV);
			}
			return Builder.ToString();
		}
	}
}