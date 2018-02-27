using UnityEngine;
using System.Collections;

namespace I2.Loc
{

	public class CallbackNotification : MonoBehaviour 
	{
		public void OnModifyLocalization()
		{
			if (string.IsNullOrEmpty(Localize.MainTranslation))
				return;
			
			string PlayerColor = LocalizationManager.GetTermTranslation( "Color/Red" );
			
			Localize.MainTranslation = Localize.MainTranslation.Replace("{PLAYER_COLOR}", PlayerColor);
		}
	}
}