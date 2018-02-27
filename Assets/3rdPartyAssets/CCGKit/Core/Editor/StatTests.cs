// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using NUnit.Framework;

using CCGKit;

/// <summary>
/// Unit tests for the stat system.
/// </summary>
public class StatTests
{
    [Test]
    public void TestMinValue()
    {
        var lifeStat = new Stat();
        lifeStat.name = "Life";
        lifeStat.baseValue = 20;
        lifeStat.minValue = 0;
        lifeStat.maxValue = 99;

        var lifeNerf = new Modifier(-30);
        lifeStat.modifiers.Add(lifeNerf);

        Assert.AreEqual(lifeStat.effectiveValue, 0);
    }

    [Test]
    public void TestMaxValue()
    {
        var lifeStat = new Stat();
        lifeStat.name = "Life";
        lifeStat.baseValue = 20;
        lifeStat.minValue = 0;
        lifeStat.maxValue = 99;

        var lifeBuff = new Modifier(100);
        lifeStat.modifiers.Add(lifeBuff);

        Assert.AreEqual(lifeStat.effectiveValue, 99);
    }

    [Test]
    public void TestModifierDuration()
    {
        var lifeStat = new Stat();
        lifeStat.name = "Life";
        lifeStat.baseValue = 20;
        lifeStat.minValue = 0;
        lifeStat.maxValue = 99;

        var lifeBuff = new Modifier(1, 2);
        lifeStat.modifiers.Add(lifeBuff);

        Assert.AreEqual(lifeStat.effectiveValue, 21);

        lifeStat.OnEndTurn();

        Assert.AreEqual(lifeStat.effectiveValue, 21);

        lifeStat.OnEndTurn();

        Assert.AreEqual(lifeStat.effectiveValue, 20);
    }
}
