// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class WalkerArrivalAnimation {

        public Action<WalkerArrivalAnimation> OnAnimationCompleted;

        public GameObject selfObject;
        public SpriteRenderer picture;

        private ILoadObjectsManager _loader;
        private Dictionary<object[], Action<object[]>> _onCompleteActions;
        private float _animationLength = 2.5f;

        private Vector3 _offset = new Vector3(0.1f, 0.9f, 0f);

        public WalkerArrivalAnimation(Sprite sprite, Transform parent)
        {
            _loader = GameClient.Get<ILoadObjectsManager>();
            selfObject = GameObject.Instantiate(_loader.GetObjectByPath<GameObject>("Prefabs/Gameplay/WalkerArrivalAnimation"), parent);
            selfObject.transform.localPosition = _offset;
            picture = selfObject.transform.Find("Picture").GetComponent<SpriteRenderer>();
            picture.sprite = sprite;

            _onCompleteActions = new Dictionary<object[], Action<object[]>>();

            Animator anim = selfObject.GetComponent<Animator>();
            var clip = anim.GetCurrentAnimatorClipInfo(0);
            _animationLength = clip[0].clip.length * anim.speed;

            //Debug.Log("HeavyArrivalAnimation clip length: "+_animationLength);

            GameClient.Get<ITimerManager>().AddTimer((x) => 
            {
                if (OnAnimationCompleted != null) OnAnimationCompleted(this);
                Dispose();
                foreach (var call in _onCompleteActions)
                {
                    if (call.Value != null) call.Value(call.Key);
                }
                _onCompleteActions.Clear();
            }, time:_animationLength);
        }

        public void AddOnCompleteCallback(Action<object[]> call, object[] param = null)
        {
            _onCompleteActions.Add(param, call);
        }

        public void Dispose()
        {
            if (selfObject != null)
                GameObject.Destroy(selfObject);
        }
    }
}