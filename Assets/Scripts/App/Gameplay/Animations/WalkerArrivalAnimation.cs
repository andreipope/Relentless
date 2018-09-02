using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class WalkerArrivalAnimation
    {
        private readonly ILoadObjectsManager _loader;

        private readonly Dictionary<object[], Action<object[]>> _onCompleteActions;

        private readonly float _animationLength = 2.5f;

        private readonly Vector3 _offset = new Vector3(0.1f, 0.9f, 0f);

        public Action<WalkerArrivalAnimation> OnAnimationCompleted;

        public GameObject selfObject;

        public SpriteRenderer picture;

        public WalkerArrivalAnimation(Sprite sprite, Transform parent)
        {
            _loader = GameClient.Get<ILoadObjectsManager>();
            selfObject = Object.Instantiate(_loader.GetObjectByPath<GameObject>("Prefabs/Gameplay/WalkerArrivalAnimation"), parent);
            selfObject.transform.localPosition = _offset;
            picture = selfObject.transform.Find("Picture").GetComponent<SpriteRenderer>();
            picture.sprite = sprite;

            _onCompleteActions = new Dictionary<object[], Action<object[]>>();

            Animator anim = selfObject.GetComponent<Animator>();
            AnimatorClipInfo[] clip = anim.GetCurrentAnimatorClipInfo(0);
            _animationLength = clip[0].clip.length * anim.speed;

            // Debug.Log("HeavyArrivalAnimation clip length: "+_animationLength);
            GameClient.Get<ITimerManager>().AddTimer(
                x =>
                {
                    if (OnAnimationCompleted != null)
                    {
                        OnAnimationCompleted(this);
                    }

                    Dispose();
                    foreach (KeyValuePair<object[], Action<object[]>> call in _onCompleteActions)
                    {
                        if (call.Value != null)
                        {
                            call.Value(call.Key);
                        }
                    }

                    _onCompleteActions.Clear();
                },
                time: _animationLength);
        }

        public void AddOnCompleteCallback(Action<object[]> call, object[] param = null)
        {
            _onCompleteActions.Add(param, call);
        }

        public void Dispose()
        {
            if (selfObject != null)
            {
                Object.Destroy(selfObject);
            }
        }
    }
}
