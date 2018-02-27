//#define NGUI

using UnityEngine;
using System.Collections;

namespace I2.Loc
{

	public class LanguagePopup : MonoBehaviour 
	{
		public LanguageSource Source;

	#if NGUI
		void Start ()
		{
			UIPopupList mList = GetComponent<UIPopupList>();
			mList.items = Source.GetLanguages();

			EventDelegate.Add(mList.onChange, OnValueChange);
			int idx = mList.items.IndexOf(LocalizationManager.CurrentLanguage);
			mList.value = mList.items[idx>=0 ? idx : 0];
		}

		public void OnValueChange ()
		{
			LocalizationManager.CurrentLanguage = UIPopupList.current.value;
		}
	#endif
	}
}