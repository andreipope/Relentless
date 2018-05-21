// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using CCGKit;

public class SpellCardView : CardView
{

	public override void PopulateWithInfo(RuntimeCard card, string setName)
	{
		base.PopulateWithInfo(card, setName);

		bodyText.transform.position -= UnityEngine.Vector3.up;
        nameText.transform.position -= UnityEngine.Vector3.up * 0.8f;
	}

    public override void PopulateWithLibraryInfo(GrandDevs.CZB.Data.Card card, string setName = "", int amount = 0)
    {
        base.PopulateWithLibraryInfo(card, setName, amount);

		bodyText.transform.position -= UnityEngine.Vector3.up;
		nameText.transform.position -= UnityEngine.Vector3.up * 0.8f;
	}
}