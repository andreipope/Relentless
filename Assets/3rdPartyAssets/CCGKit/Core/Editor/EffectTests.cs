// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using NUnit.Framework;

using CCGKit;

/// <summary>
/// Unit tests for the provided card effects.
/// </summary>
public class EffectTests
{
    [Test]
    public void TestSetPlayerStatEffect()
    {
        var gameState = CreateTestGameState();

        var effect = new SetPlayerStatEffect();
        effect.statId = 0;
        var constant = new ConstantValue();
        constant.constant = 5;
        effect.value = constant;
        effect.Resolve(gameState, gameState.currentPlayer);

        Assert.AreEqual(gameState.currentPlayer.stats[0].effectiveValue, 5);
        Assert.AreEqual(gameState.currentOpponent.stats[0].effectiveValue, 20);
    }

    [Test]
    public void TestResetPlayerStatEffect()
    {
        var gameState = CreateTestGameState();

        var setEffect = new SetPlayerStatEffect();
        setEffect.statId = 0;
        var constant = new ConstantValue();
        constant.constant = 25;
        setEffect.value = constant;
        setEffect.Resolve(gameState, gameState.currentPlayer);

        Assert.AreEqual(gameState.currentPlayer.stats[0].effectiveValue, 25);
        Assert.AreEqual(gameState.currentOpponent.stats[0].effectiveValue, 20);

        var resetEffect = new ResetPlayerStatEffect();
        resetEffect.statId = 0;
        resetEffect.Resolve(gameState, gameState.currentPlayer);

        Assert.AreEqual(gameState.currentPlayer.stats[0].effectiveValue, 20);
        Assert.AreEqual(gameState.currentOpponent.stats[0].effectiveValue, 20);
    }

    [Test]
    public void TestIncreasePlayerStatEffect()
    {
        var gameState = CreateTestGameState();

        var effect = new IncreasePlayerStatEffect();
        effect.statId = 0;
        var constant = new ConstantValue();
        constant.constant = 1;
        effect.value = constant;
        effect.Resolve(gameState, gameState.currentPlayer);

        Assert.AreEqual(gameState.currentPlayer.stats[0].effectiveValue, 21);
        Assert.AreEqual(gameState.currentOpponent.stats[0].effectiveValue, 20);
    }

    [Test]
    public void TestDecreasePlayerStatEffect()
    {
        var gameState = CreateTestGameState();

        var effect = new DecreasePlayerStatEffect();
        effect.statId = 0;
        var constant = new ConstantValue();
        constant.constant = 1;
        effect.value = constant;
        effect.Resolve(gameState, gameState.currentPlayer);

        Assert.AreEqual(gameState.currentPlayer.stats[0].effectiveValue, 19);
        Assert.AreEqual(gameState.currentOpponent.stats[0].effectiveValue, 20);
    }

    [Test]
    public void TestSetCardStatEffect()
    {
        var gameState = CreateTestGameState();

        var effect = new SetCardStatEffect();
        effect.statId = 0;
        var constant = new ConstantValue();
        constant.constant = 5;
        effect.value = constant;

        var card = gameState.currentPlayer.zones[0].cards[0];
        effect.Resolve(gameState, card);

        Assert.AreEqual(card.stats[0].effectiveValue, 5);
    }

    [Test]
    public void TestResetCardStatEffect()
    {
        var gameState = CreateTestGameState();

        var setEffect = new SetCardStatEffect();
        setEffect.statId = 0;
        var constant = new ConstantValue();
        constant.constant = 5;
        setEffect.value = constant;

        var card = gameState.currentPlayer.zones[0].cards[0];
        setEffect.Resolve(gameState, card);

        Assert.AreEqual(card.stats[0].effectiveValue, 5);

        var resetEffect = new ResetCardStatEffect();
        resetEffect.statId = 0;
        resetEffect.Resolve(gameState, card);

        Assert.AreEqual(card.stats[0].effectiveValue, 2);
    }

    [Test]
    public void TestIncreaseCardStatEffect()
    {
        var gameState = CreateTestGameState();

        var effect = new IncreaseCardStatEffect();
        effect.statId = 0;
        var constant = new ConstantValue();
        constant.constant = 1;
        effect.value = constant;

        var card = gameState.currentPlayer.zones[0].cards[0];
        effect.Resolve(gameState, card);

        Assert.AreEqual(card.stats[0].effectiveValue, 3);
    }

    [Test]
    public void TestDecreaseCardStatEffect()
    {
        var gameState = CreateTestGameState();

        var effect = new DecreaseCardStatEffect();
        effect.statId = 0;
        var constant = new ConstantValue();
        constant.constant = 1;
        effect.value = constant;

        var card = gameState.currentPlayer.zones[0].cards[0];
        effect.Resolve(gameState, card);

        Assert.AreEqual(card.stats[0].effectiveValue, 1);
    }

    private GameState CreateTestGameState()
    {
        var gameState = new GameState();
        gameState.players.Add(new PlayerInfo());
        gameState.players.Add(new PlayerInfo());
        gameState.currentPlayer = gameState.players[0];
        gameState.currentOpponent = gameState.players[1];

        foreach (var player in gameState.players)
        {
            var life = new Stat();
            life.statId = 0;
            life.name = "Life";
            life.baseValue = 20;
            life.originalValue = 20;
            life.minValue = 0;
            life.maxValue = 99;
            player.stats[life.statId] = life;
            player.namedStats[life.name] = life;
        }

        foreach (var player in gameState.players)
        {
            var zone = new RuntimeZone();
            zone.zoneId = 0;
            zone.instanceId = 0;
            zone.name = "Test zone";
            zone.maxCards = 100;

            var life = new Stat();
            life.statId = 0;
            life.name = "Life";
            life.baseValue = 2;
            life.originalValue = 2;
            life.minValue = 0;
            life.maxValue = 99;

            var card = new RuntimeCard();
            card.cardId = 0;
            card.instanceId = 0;
            card.stats[life.statId] = life;
            card.namedStats[life.name] = life;

            zone.AddCard(card);

            player.zones[zone.zoneId] = zone;
            player.namedZones[zone.name] = zone;
        }

        return gameState;
    }
}
