using UnityEngine;
using System.Collections;
using System.Reflection;

namespace I2.Loc
{
	public class RealTimeTranslation : MonoBehaviour 
	{
		string OriginalText = "This is an example showing how to use the google translator to translate chat messages within the game.\nIt also supports multiline translations.",
			   TranslatedText = string.Empty;
		bool IsTranslating = false;	

		public void OnGUI()
		{
			GUILayout.Label("Translate:");
			OriginalText = GUILayout.TextArea(OriginalText, GUILayout.Width(Screen.width));

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();

				if (GUILayout.Button ("English -> Español", GUILayout.Height(100))) StartTranslating("en", "es");
				if (GUILayout.Button ("Español -> English", GUILayout.Height(100))) StartTranslating("es", "en");

			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			GUILayout.TextArea(TranslatedText, GUILayout.Width(Screen.width));

			GUILayout.Space(10);


			if (IsTranslating)
			{
				GUILayout.Label ("Contacting Google....");
			}
		}

		void StartTranslating( string fromCode, string toCode )
		{
			IsTranslating = true;

			// fromCode could be "auto" to autodetect the language
			GoogleTranslation.Translate( OriginalText, fromCode, toCode, OnTranslationReady);
		}

		void OnTranslationReady( string Translation )
		{
			TranslatedText = Translation;
			IsTranslating = false;
		}
	}
}