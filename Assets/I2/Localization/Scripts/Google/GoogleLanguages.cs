using UnityEngine;
using System;
using System.Collections.Generic;

namespace I2.Loc
{
	public static class GoogleLanguages
	{
		public static string GetLanguageCode( string Filter, bool ShowWarnings = false)
		{
			if (string.IsNullOrEmpty(Filter))
				return string.Empty;
			
			string[] Filters = Filter.ToLowerInvariant().Split(" /(),".ToCharArray());
			
			foreach (var kvp in mLanguageDef)
				if (LanguageMatchesFilter(kvp.Key, Filters))
					return kvp.Value.Code;
			
			if (ShowWarnings) 
				Debug.Log (string.Format ("Language '{0}' not recognized. Please, add the language code to GoogleTranslation.cs", Filter));
			return string.Empty;
		}
		
		
		public static List<string> GetLanguagesForDropdown(string Filter, string CodesToExclude)
		{
			string[] Filters = Filter.ToLowerInvariant().Split(" /(),".ToCharArray());
			
			List<string> Languages = new List<string>();

			foreach (var kvp in mLanguageDef)
				if (string.IsNullOrEmpty(Filter) || LanguageMatchesFilter(kvp.Key, Filters))
				{
					string code = string.Concat("[" + kvp.Value.Code + "]");
					if (!CodesToExclude.Contains(code))
						Languages.Add (kvp.Key + " " + code);
				}

			// Add headers to variants (e.g. "English/English"  before all English variants
			for (int i=Languages.Count-2; i>=0; --i)
			{
				string Prefix = Languages[i].Substring(0, Languages[i].IndexOf(" ["));
				if (Languages[i+1].StartsWith(Prefix))
				{
					Languages[i] = Prefix + "/"+ Languages[i];
					Languages.Insert(i+1, Prefix + "/");
				}
			}
			return Languages;
		}
		
		public static string GetClosestLanguage(string Filter)
		{
			if (string.IsNullOrEmpty(Filter))
				return string.Empty;

			string[] Filters = Filter.ToLowerInvariant().Split(" /(),".ToCharArray());
			
			foreach (var kvp in mLanguageDef)
				if (LanguageMatchesFilter(kvp.Key, Filters))
					return kvp.Key;//GetFormatedLanguageName( kvp.Key );
			
			return string.Empty;
		}
		
		// "Engl Unit" matches "English/United States"
		static bool LanguageMatchesFilter(string Language, string[] Filters)
		{
			Language = Language.ToLowerInvariant();
			for (int i=0,imax=Filters.Length; i<imax; ++i)
				if (Filters[i]!="")
				{
					if (!Language.Contains(Filters[i].ToLower()))
						return false;
					else
						Language = Language.Remove( Language.IndexOf(Filters[i]), Filters[i].Length );
				}
			return true;
		}
		
		
		// "Arabic/Algeria [ar-XX]" returns "Arabic (Algeria)"
		// "English/English [en]" returns "English"
		public static string GetFormatedLanguageName( string Language )
		{
			string BaseLanguage = string.Empty;

			//-- Remove code --------
			int Index = Language.IndexOf(" [");
			if (Index>0)
				Language = Language.Substring(0, Index);

			//-- Check for main language: "English/English [en]" returns "English" -----------
			Index = Language.IndexOf('/');
			if (Index>0)
			{
				BaseLanguage = Language.Substring(0, Index);
				if (Language == (BaseLanguage +"/"+BaseLanguage))
					return BaseLanguage;

				//-- Convert variants into right format
				Language = Language.Replace("/", " (") + ")";
			}

			return Language;
		}
		
		// English British   ->   "English Canada [en-CA]"
		public static string GetCodedLanguage( string Language, string code )
		{
			string DefaultCode = GetLanguageCode(Language,false);
			if (string.Compare(code, DefaultCode, StringComparison.OrdinalIgnoreCase)==0)
				return Language;
			return string.Concat(Language, " [",code,"]");
		}
		
		// "English Canada [en-CA]" ->  "English Canada", "en-CA"
		public static void UnPackCodeFromLanguageName( string CodedLanguage, out string Language, out string code )
		{
			if (string.IsNullOrEmpty(CodedLanguage))
			{
				Language = string.Empty;
				code = string.Empty;
				return;
			}
			int Index = CodedLanguage.IndexOf("[");
			if (Index<0)
			{
				Language = CodedLanguage;
				code = GetLanguageCode(Language);
			}
			else
			{
				Language = CodedLanguage.Substring(0,Index).Trim();
				code = CodedLanguage.Substring(Index+1, CodedLanguage.IndexOf("]",Index)-Index-1);
			}
		}

		public static string GetGoogleLanguageCode( string InternationalCode )
		{
			foreach( var kvp in mLanguageDef )
				if (InternationalCode == kvp.Value.Code)
					return (!string.IsNullOrEmpty(kvp.Value.GoogleCode)) ? kvp.Value.GoogleCode : InternationalCode;

			return InternationalCode;
		}

		public static List<string> GetAllInternationalCodes()
		{
			var set = new HashSet<string>();

			foreach( var kvp in mLanguageDef )
				set.Add(kvp.Value.Code);

			return new List<string>(set);
		}

		public struct LanguageCodeDef
		{
			public string Code;		// Language International Code
			public string GoogleCode;	// Google Translator doesn't support all languages, this is the code of closest supported language
		}

		public static Dictionary<string,LanguageCodeDef> mLanguageDef = new Dictionary<string, LanguageCodeDef>()  
		{
			{"Afrikaans", 			new LanguageCodeDef(){Code="af"}},
			{"Albanian", 			new LanguageCodeDef(){Code="sq"}},
			{"Arabic", 				new LanguageCodeDef(){Code="ar"}},
			{"Arabic/Algeria", 		new LanguageCodeDef(){Code="ar-DZ", GoogleCode="ar"}},
			{"Arabic/Bahrain", 		new LanguageCodeDef(){Code="ar-BH", GoogleCode="ar"}},
			{"Arabic/Egypt", 		new LanguageCodeDef(){Code="ar-EG", GoogleCode="ar"}},
			{"Arabic/Iraq", 		new LanguageCodeDef(){Code="ar-IQ", GoogleCode="ar"}},
			{"Arabic/Jordan", 		new LanguageCodeDef(){Code="ar-JO", GoogleCode="ar"}},
			{"Arabic/Kuwait", 		new LanguageCodeDef(){Code="ar-KW", GoogleCode="ar"}},
			{"Arabic/Lebanon", 		new LanguageCodeDef(){Code="ar-LB", GoogleCode="ar"}},
			{"Arabic/Libya", 		new LanguageCodeDef(){Code="ar-LY", GoogleCode="ar"}},
			{"Arabic/Morocco", 		new LanguageCodeDef(){Code="ar-MA", GoogleCode="ar"}},
			{"Arabic/Oman", 		new LanguageCodeDef(){Code="ar-OM", GoogleCode="ar"}},
			{"Arabic/Qatar", 		new LanguageCodeDef(){Code="ar-QA", GoogleCode="ar"}},
			{"Arabic/Saudi Arabia", new LanguageCodeDef(){Code="ar-SA", GoogleCode="ar"}},
			{"Arabic/Syria", 		new LanguageCodeDef(){Code="ar-SY", GoogleCode="ar"}},
			{"Arabic/Tunisia", 		new LanguageCodeDef(){Code="ar-TN", GoogleCode="ar"}},
			{"Arabic/U.A.E.", 		new LanguageCodeDef(){Code="ar-AE", GoogleCode="ar"}},
			{"Arabic/Yemen", 		new LanguageCodeDef(){Code="ar-YE", GoogleCode="ar"}},
			{"Armenian", 			new LanguageCodeDef(){Code="hy"}},
			{"Azerbaijani", 		new LanguageCodeDef(){Code="az"}},
			{"Basque",		new LanguageCodeDef(){Code="eu"}},
			{"Basque/Spain", 		new LanguageCodeDef(){Code="eu-ES", GoogleCode="eu"}},
			{"Belarusian", 			new LanguageCodeDef(){Code="be"}},
			{"Bosnian", 			new LanguageCodeDef(){Code="bs"}},
			{"Bulgariaa", 			new LanguageCodeDef(){Code="bg"}},
			{"Catalan", 			new LanguageCodeDef(){Code="ca"}},
			{"Chinese",				new LanguageCodeDef(){Code="zh", 	GoogleCode="zh-CN"}},
			{"Chinese/Hong Kong",	new LanguageCodeDef(){Code="zh-HK", GoogleCode="zh-TW"}},
			{"Chinese/Macau", 		new LanguageCodeDef(){Code="zh-MO", GoogleCode="zh-CN"}},
			{"Chinese/PRC", 		new LanguageCodeDef(){Code="zh-CN", GoogleCode="zh-CN"}},
			{"Chinese/Simplified", 	new LanguageCodeDef(){Code="zh-CN", GoogleCode="zh-CN"}},
			{"Chinese/Singapore", 	new LanguageCodeDef(){Code="zh-SG", GoogleCode="zh-CN"}},
			{"Chinese/Taiwan", 		new LanguageCodeDef(){Code="zh-TW", GoogleCode="zh-TW"}},
			{"Chinese/Traditional", new LanguageCodeDef(){Code="zh-TW", GoogleCode="zh-TW"}},
			{"Croatian", 			new LanguageCodeDef(){Code="hr"}},
			{"Croatian/Bosnia and Herzegovina", new LanguageCodeDef(){Code="hr-BA", GoogleCode="hr"}},
			{"Czech", 				new LanguageCodeDef(){Code="cs"}},
			{"Danish", 				new LanguageCodeDef(){Code="da"}},
			//{"Dhivehi", new LanguageCodeDef(){Code="diV"}},		//---------------
			//{"Divehi", new LanguageCodeDef(){Code="dv"}},		//---------------
			{"Dutch", 				new LanguageCodeDef(){Code="nl"}},
			{"Dutch/Belgium", 		new LanguageCodeDef(){Code="nl-BE", GoogleCode="nl"}},
			{"Dutch/Netherlands", 	new LanguageCodeDef(){Code="nl-NL", GoogleCode="nl"}},
			{"English", 			new LanguageCodeDef(){Code="en"}},
			{"English/Australia", 	new LanguageCodeDef(){Code="en-AU", GoogleCode="en"}},
			{"English/Belize", 		new LanguageCodeDef(){Code="en-BZ", GoogleCode="en"}},
			{"English/Canada", 		new LanguageCodeDef(){Code="en-CA", GoogleCode="en"}},
			{"English/Caribbean", 	new LanguageCodeDef(){Code="en-CB", GoogleCode="en"}},
			{"English/Ireland", 	new LanguageCodeDef(){Code="en-IE", GoogleCode="en"}},
			{"English/Jamaica", 	new LanguageCodeDef(){Code="en-JM", GoogleCode="en"}},
			{"English/New Zealand", new LanguageCodeDef(){Code="en-NZ", GoogleCode="en"}},
			{"English/Republic of the Philippines", new LanguageCodeDef(){Code="en-PH", GoogleCode="en"}},
			{"English/South Africa",new LanguageCodeDef(){Code="en-ZA", GoogleCode="en"}},
			{"English/Trinidad", 	new LanguageCodeDef(){Code="en-TT", GoogleCode="en"}},
			{"English/United Kingdom",new LanguageCodeDef(){Code="en-GB", GoogleCode="en"}},
			{"English/United States",new LanguageCodeDef(){Code="en-US", GoogleCode="en"}},
			{"English/Zimbabwe", 	new LanguageCodeDef(){Code="en-ZW", GoogleCode="en"}},
			{"Esperanto", 			new LanguageCodeDef(){Code="eo"}},
			{"Estonian", 			new LanguageCodeDef(){Code="et"}},
			{"Faeroese", 			new LanguageCodeDef(){Code="fo"}},
			//{"Farsi", new LanguageCodeDef(){Code="fa"}},		//--------------------
			{"Filipino", 			new LanguageCodeDef(){Code="tl"}},
			{"Finnish", 			new LanguageCodeDef(){Code="fi"}},
			{"French", 				new LanguageCodeDef(){Code="fr"}},
			{"French/Belgium", 		new LanguageCodeDef(){Code="fr-BE", GoogleCode="fr"}},
			{"French/Canada", 		new LanguageCodeDef(){Code="fr-CA", GoogleCode="fr"}},
			{"French/France", 		new LanguageCodeDef(){Code="fr-FR", GoogleCode="fr"}},
			{"French/Luxembourg", 	new LanguageCodeDef(){Code="fr-LU", GoogleCode="fr"}},
			{"French/Principality of Monaco", new LanguageCodeDef(){Code="fr-MC", GoogleCode="fr"}},
			{"French/Switzerland", 	new LanguageCodeDef(){Code="fr-CH", GoogleCode="fr"}},
			//{"Gaelic", 	new LanguageCodeDef(){Code="gd"}}, //------------------
			{"Galician", 			new LanguageCodeDef(){Code="gl"}},
			{"Galician/Spain", 		new LanguageCodeDef(){Code="gl-ES", GoogleCode="gl"}},
			{"Georgian", 			new LanguageCodeDef(){Code="ka"}},
			{"German", 				new LanguageCodeDef(){Code="de"}},
			{"German/Austria", 		new LanguageCodeDef(){Code="de-AT", GoogleCode="de"}},
			{"German/Germany", 		new LanguageCodeDef(){Code="de-DE", GoogleCode="de"}},
			{"German/Liechtenstein",new LanguageCodeDef(){Code="de-LI", GoogleCode="de"}},
			{"German/Luxembourg", 	new LanguageCodeDef(){Code="de-LU", GoogleCode="de"}},
			{"German/Switzerland", 	new LanguageCodeDef(){Code="de-CH", GoogleCode="de"}},
			{"Greek", 				new LanguageCodeDef(){Code="el"}},
			{"Gujarati", 			new LanguageCodeDef(){Code="gu"}},
			{"Hebrew", 				new LanguageCodeDef(){Code="he", GoogleCode="iw"}},
			{"Hindi", 				new LanguageCodeDef(){Code="hi"}},
			{"Hungarian", 			new LanguageCodeDef(){Code="hu"}},
			{"Icelandic", 			new LanguageCodeDef(){Code="is"}},
			{"Indonesian", 			new LanguageCodeDef(){Code="id"}},
			{"Irish", 				new LanguageCodeDef(){Code="ga"}},
			{"Italian", 			new LanguageCodeDef(){Code="it"}},
			{"Italian/Italy", 		new LanguageCodeDef(){Code="it-IT", GoogleCode="it"}},
			{"Italian/Switzerland", new LanguageCodeDef(){Code="it-CH", GoogleCode="it"}},
			{"Japanese", 			new LanguageCodeDef(){Code="ja"}},
			{"Kannada", 			new LanguageCodeDef(){Code="kn"}},
			{"Kazakh", 				new LanguageCodeDef(){Code="kk"}},
			//{"Konkani", new LanguageCodeDef(){Code="koK"}},//----------------
			{"Korean", 				new LanguageCodeDef(){Code="ko"}},
			{"Kurdish", 			new LanguageCodeDef(){Code="ku"}},
			{"Kyrgyz", 				new LanguageCodeDef(){Code="ky"}},
			{"Latin", 				new LanguageCodeDef(){Code="la"}},
			{"Latvian", 			new LanguageCodeDef(){Code="lv"}},
			{"Lithuanian", 			new LanguageCodeDef(){Code="lt"}},
			{"Macedonian", 			new LanguageCodeDef(){Code="mk"}},
			{"Malay", 				new LanguageCodeDef(){Code="ms"}},
			{"Malay/Brunei Darussalam", new LanguageCodeDef(){Code="ms-BN", GoogleCode="ms"}},
			{"Malay/Malaysia", 		new LanguageCodeDef(){Code="ms-MY", GoogleCode="ms"}},
			{"Malayalam", 			new LanguageCodeDef(){Code="ml"}},
			{"Maltese", 			new LanguageCodeDef(){Code="mt"}},
			{"Maori", 				new LanguageCodeDef(){Code="mi"}},
			{"Marathi", 			new LanguageCodeDef(){Code="mr"}},
			{"Mongolian", 			new LanguageCodeDef(){Code="mn"}},
			{"Northern Sotho", 		new LanguageCodeDef(){Code="ns", GoogleCode="nso"}},
			{"Norwegian", 			new LanguageCodeDef(){Code="nb", GoogleCode="no"}},
			{"Norwegian/Nynorsk", 	new LanguageCodeDef(){Code="nn", GoogleCode="no"}},
			{"Pashto", 				new LanguageCodeDef(){Code="ps"}},
			{"Persian", 			new LanguageCodeDef(){Code="fa"}},
			{"Polish", 				new LanguageCodeDef(){Code="pl"}},
			{"Portuguese", 			new LanguageCodeDef(){Code="pt"}},
			{"Portuguese/Brazil", 	new LanguageCodeDef(){Code="pt-BR", GoogleCode="pt"}},
			{"Portuguese/Portugal", new LanguageCodeDef(){Code="pt-PT", GoogleCode="pt"}},
			{"Punjabi", 			new LanguageCodeDef(){Code="pa"}},
			{"Quechua", 			new LanguageCodeDef(){Code="qu"}},
			{"Quechua/Bolivia", 	new LanguageCodeDef(){Code="qu-BO", GoogleCode="qu"}},
			{"Quechua/Ecuador", 	new LanguageCodeDef(){Code="qu-EC", GoogleCode="qu"}},
			{"Quechua/Peru", 		new LanguageCodeDef(){Code="qu-PE", GoogleCode="qu"}},
			{"Rhaeto-Romanic", 		new LanguageCodeDef(){Code="rm", GoogleCode="ro"}},
			{"Romanian", 			new LanguageCodeDef(){Code="ro"}},
			{"Russian", 			new LanguageCodeDef(){Code="ru"}},
			{"Russian/Republic of Moldova", new LanguageCodeDef(){Code="ru-MO", GoogleCode="ru"}},
			//{"Sami/Finland", new LanguageCodeDef(){Code="se-FI"}}, //--------------
			//{"Sami/Lappish", new LanguageCodeDef(){Code="sz"}}, //--------------
			//{"Sami/Northern", new LanguageCodeDef(){Code="se-NO"}}, //--------------
			//{"Sami/Sweden", new LanguageCodeDef(){Code="se-SE"}}, //--------------
			//{"Sanskrit", new LanguageCodeDef(){Code="sa"}}, //--------------
			{"Serbian", 			new LanguageCodeDef(){Code="sr"}},
			{"Serbian/Bosnia and Herzegovina", 	new LanguageCodeDef(){Code="sr-BA", GoogleCode="sr"}},
			{"Serbian/Serbia and Montenegro", 	new LanguageCodeDef(){Code="sr-SP", GoogleCode="sr"}},
			{"Slovak", 				new LanguageCodeDef(){Code="sk"}},
			{"Slovenian", 			new LanguageCodeDef(){Code="sl"}},
			//{"Sorbian", new LanguageCodeDef(){Code="sb"}}, //------------------------
			{"Spanish", 			new LanguageCodeDef(){Code="es"}},
			{"Spanish/Argentina", 	new LanguageCodeDef(){Code="es-AR", GoogleCode="es"}},
			{"Spanish/Bolivia", 	new LanguageCodeDef(){Code="es-BO", GoogleCode="es"}},
			{"Spanish/Castilian", 	new LanguageCodeDef(){Code="es-ES", GoogleCode="es"}},
			{"Spanish/Chile", 		new LanguageCodeDef(){Code="es-CL", GoogleCode="es"}},
			{"Spanish/Colombia", 	new LanguageCodeDef(){Code="es-CO", GoogleCode="es"}},
			{"Spanish/Costa Rica", 	new LanguageCodeDef(){Code="es-CR", GoogleCode="es"}},
			{"Spanish/Dominican Republic", new LanguageCodeDef(){Code="es-DO", GoogleCode="es"}},
			{"Spanish/Ecuador", 	new LanguageCodeDef(){Code="es-EC", GoogleCode="es"}},
			{"Spanish/El Salvador", new LanguageCodeDef(){Code="es-SV", GoogleCode="es"}},
			{"Spanish/Guatemala", 	new LanguageCodeDef(){Code="es-GT", GoogleCode="es"}},
			{"Spanish/Honduras", 	new LanguageCodeDef(){Code="es-HN", GoogleCode="es"}},
			{"Spanish/Mexico", 		new LanguageCodeDef(){Code="es-MX", GoogleCode="es"}},
			{"Spanish/Nicaragua", 	new LanguageCodeDef(){Code="es-NI", GoogleCode="es"}},
			{"Spanish/Panama", 		new LanguageCodeDef(){Code="es-PA", GoogleCode="es"}},
			{"Spanish/Paraguay", 	new LanguageCodeDef(){Code="es-PY", GoogleCode="es"}},
			{"Spanish/Peru", 		new LanguageCodeDef(){Code="es-PE", GoogleCode="es"}},
			{"Spanish/Puerto Rico", new LanguageCodeDef(){Code="es-PR", GoogleCode="es"}},
			{"Spanish/Spain", 		new LanguageCodeDef(){Code="es"}},
			{"Spanish/Uruguay", 	new LanguageCodeDef(){Code="es-UY", GoogleCode="es"}},
			{"Spanish/Venezuela", 	new LanguageCodeDef(){Code="es-VE", GoogleCode="es"}},
			//{"Sutu", new LanguageCodeDef(){Code="sx"}},//---------------
			{"Swahili", 			new LanguageCodeDef(){Code="sw"}},
			{"Swedish",				new LanguageCodeDef(){Code="sv"}},
			{"Swedish/Finland", 	new LanguageCodeDef(){Code="sv-FI", GoogleCode="sv"}},
			{"Swedish/Sweden", 		new LanguageCodeDef(){Code="sv-SE", GoogleCode="sv"}},
			//{"Syriac", new LanguageCodeDef(){Code="syR"}},//-----------
			{"Tamil", 				new LanguageCodeDef(){Code="ta"}},
			{"Tatar", 				new LanguageCodeDef(){Code="tt"}},
			{"Telugu", 				new LanguageCodeDef(){Code="te"}},
			{"Thai", 				new LanguageCodeDef(){Code="th"}},
			//{"Tsonga", new LanguageCodeDef(){Code="ts"}},//-----------
			//{"Tswana", new LanguageCodeDef(){Code="tn"}},//-----------
			{"Turkish", 			new LanguageCodeDef(){Code="tr"}},
			{"Ukrainian", 			new LanguageCodeDef(){Code="uk"}},
			{"Urdu", 				new LanguageCodeDef(){Code="ur"}},
			{"Uzbek", 				new LanguageCodeDef(){Code="uz"}},
			//{"Venda", new LanguageCodeDef(){Code="ve"}},//------------
			{"Vietnamese", 			new LanguageCodeDef(){Code="vi"}},
			{"Welsh", 				new LanguageCodeDef(){Code="cy"}},
			{"Xhosa", 				new LanguageCodeDef(){Code="xh"}},
			{"Yiddish", 			new LanguageCodeDef(){Code="yi"}},
			{"Zulu", 				new LanguageCodeDef(){Code="zu"}}
		};
	}
}