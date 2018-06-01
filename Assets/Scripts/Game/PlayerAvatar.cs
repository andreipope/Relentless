// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

using CCGKit;
using DG.Tweening;
using GrandDevs.CZB;
using GrandDevs.CZB.Common;

/// <summary>
/// This class holds information about a player avatar from the game scene, which can be clicked
/// to select a target player for an effect or during combat (this will send the appropriate
/// information to the server).
/// </summary>
public class PlayerAvatar : MonoBehaviour
{
    public PlayerInfo playerInfo;
    public bool IsBottom;
    public int index { get { return IsBottom ? 0 : 1; } }

    public GameObject avatarObject, avatarDeathObject, spellObject, weaponObject;

    public GameObject avatarTypeHighlight;

    public Animator avatarAnimator, deathAnimamtor;

    private void Start()
    {
        //avatarObject = transform.Find("Hero_Object").gameObject;
        //avatarDeathObject = transform.Find("HeroDeath").gameObject;
        avatarDeathObject.SetActive(false);
        avatarAnimator.enabled = false;
        deathAnimamtor.enabled = false;
    }

    private Player GetTargetPlayer()
    {
        var players = FindObjectsOfType<Player>();
        if (IsBottom)
        {
            foreach (var player in players)
            {
                if (player.isLocalPlayer && player.isHuman)
                    return player;
            }
        }
        else
        {
            foreach (var player in players)
            {
                if (!player.isLocalPlayer || (player.isLocalPlayer && !player.isHuman))
                    return player;
            }
        }
        return null;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.transform.parent != null)
        {
            var targetingArrow = collider.transform.parent.parent.GetComponent<TargetingArrow>();
            if (targetingArrow != null)
            {
                targetingArrow.OnPlayerSelected(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.transform.parent != null)
        {
            var targetingArrow = collider.transform.parent.parent.GetComponent<TargetingArrow>();
            if (targetingArrow != null)
            {
                targetingArrow.OnPlayerUnselected(this);
            }
        }
    }

    private void OnHealthChangedHandler(int was, int now)
    {
        if (now <= 0)
            OnAvatarDie();
    }

    public void SetupTutorial()
    {
        if (GameClient.Get<ITutorialManager>().IsTutorial)
        {
            playerInfo.namedStats[Constants.TAG_LIFE].onValueChanged += OnHealthChangedHandler;
        }
    }

    public void OnAvatarDie()
    {
		avatarDeathObject.SetActive(true);
		avatarAnimator.enabled = true;
        deathAnimamtor.enabled = true;
        avatarTypeHighlight.SetActive(false);
        spellObject.SetActive(false);
        weaponObject.SetActive(false);
        avatarAnimator.Play(0);
		deathAnimamtor.Play(0);

        GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.HERO_DEATH, Constants.HERO_DEATH_SOUND_VOLUME, false, false);
    }
}
