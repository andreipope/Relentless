using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Loom.ZombieBattleground.Localization;

public class TestUpdateLanguage : MonoBehaviour {

	void Update () {
        for(int i = 0; i < 6; ++i)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
                LocalizationUtil.SetLanguage((LocalizationUtil.Language)i, true);
        }
	}
}
