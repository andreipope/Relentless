using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class FeralArrivalAnimation
    {
        public Action<FeralArrivalAnimation> OnAnimationCompleted;

        public GameObject SelfObject;

        public SpriteRenderer Picture;

        private readonly ILoadObjectsManager _loader;

        private readonly Dictionary<object[], Action<object[]>> _onCompleteActions;

        private readonly float _animationLength = 4.0f;

        private readonly Vector3 _offset = new Vector3(0.1f, 0.5f, 0f);

        public FeralArrivalAnimation(Sprite sprite, Transform parent)
        {
            _loader = GameClient.Get<ILoadObjectsManager>();
            SelfObject = Object.Instantiate(_loader.GetObjectByPath<GameObject>("Prefabs/Gameplay/FeralArrivalAnimation"), parent);
            SelfObject.transform.localPosition = _offset;
            Picture = SelfObject.transform.Find("Picture").GetComponent<SpriteRenderer>();
            Picture.sprite = sprite;

            _onCompleteActions = new Dictionary<object[], Action<object[]>>();

            Animator anim = SelfObject.GetComponent<Animator>();
            AnimatorClipInfo[] clip = anim.GetCurrentAnimatorClipInfo(0);
            _animationLength = clip[0].clip.length * anim.speed;

            GameClient.Get<ITimerManager>().AddTimer(
                x =>
                {
                    OnAnimationCompleted?.Invoke(this);

                    Dispose();
                    foreach (KeyValuePair<object[], Action<object[]>> call in _onCompleteActions)
                    {
                        call.Value?.Invoke(call.Key);
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
            if (SelfObject != null)
            {
                Object.Destroy(SelfObject);
            }
        }
    }
}
