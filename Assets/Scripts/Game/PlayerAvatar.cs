using UnityEngine;

using DG.Tweening;
using GrandDevs.CZB;
using GrandDevs.CZB.Common;

public class PlayerAvatar : MonoBehaviour
{
    private bool _isDead = false;

    public Player playerInfo;
    public bool IsBottom;
    public int index { get { return IsBottom ? 0 : 1; } }

    public GameObject avatarObject,
                     avatarDeathObject,
                     spellObject,
                     weaponObject;

    public GameObject avatarTypeHighlight;

    public Animator avatarAnimator, deathAnimamtor;

    public FadeTool manaBarFadeTool;

    private void Start()
    {
        //avatarObject = transform.Find("Hero_Object").gameObject;
        //avatarDeathObject = transform.Find("HeroDeath").gameObject;
        avatarAnimator.enabled = false;
        deathAnimamtor.enabled = false;
        deathAnimamtor.StopPlayback();
    }

    private Player GetTargetPlayer()
    {
        var players = GameClient.Get<IGameplayManager>().PlayersInGame;

        if (IsBottom)
        {
            foreach (var player in players)
            {
                if (player.IsLocalPlayer)
                    return player;
            }
        }
        else
        {
            foreach (var player in players)
            {
                if (!player.IsLocalPlayer)
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
        if (now <= 0 && !_isDead)
        {
            OnAvatarDie();

            _isDead = true;
        }
    }

    public void SetupTutorial()
    {
        if (GameClient.Get<ITutorialManager>().IsTutorial)
        {
            playerInfo.PlayerHPChangedEvent += OnHealthChangedHandler;
        }
    }

    public void OnAvatarDie()
    {
        manaBarFadeTool.FadeIn();

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
