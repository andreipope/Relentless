// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;

using CCGKit;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class TargetingArrow : MonoBehaviour
{
    [HideInInspector]
    public Target effectTarget;

    [HideInInspector]
    public EffectTarget targetType;

    public Action onTargetSelected;

    public BoardCreature selectedCard { get; protected set; }
    public PlayerAvatar selectedPlayer { get; protected set; }

    public Material material;

    //public Texture2D headTexture;

    public GameObject headPrefab;
    public GameObject middlePrefab;
    public GameObject startPrefab;

    public Texture2D targetTexture;

    protected LineRenderer lineRenderer;
    protected GameObject head;
    protected GameObject target;
    protected Vector2 uvAnimationRate = new Vector2(-1.5f, 0.0f);
    protected Vector2 uvOffset = Vector2.zero;

    protected bool startedDrag;
    protected Vector3 initialPos;

    protected BoardCreature boardCreature;

    protected GameObject headArrow,
                         startArrow;
    private List<GameObject> _allMiddleBlocks;

    private Vector3 _sizeMiddleBlock,
                    _sizeHeadBlock,
                    _sizeStartBlock,
                    _startPosition,
                    _deltaMove,
                    _endPosition;

    private float _interval = .4f,
                  _distanceBettwenPoints = 0f,
                  _speedMove = 0f,
                  _startSpeed = 2f,
                  _startDistance = 3f;

    private int _blockCount = 0;

    private GameObject _middleBlockContainer;

    private bool _isStartMove = false;

    private Vector3 newRotation,
                oldPosition;



    protected void Init()
    {
        startArrow = MonoBehaviour.Instantiate(startPrefab, transform);
        startArrow.transform.position = initialPos;

        headArrow = MonoBehaviour.Instantiate(headPrefab, transform);
        headArrow.transform.position = initialPos;

        _middleBlockContainer = new GameObject("MiddleBlockContainer");
        _middleBlockContainer.transform.SetParent(transform, false);

        _sizeMiddleBlock = middlePrefab.GetComponent<MeshRenderer>().bounds.size;
        _sizeHeadBlock = headArrow.transform.GetComponent<MeshRenderer>().bounds.size;
        _sizeStartBlock = startArrow.GetComponent<MeshRenderer>().bounds.size;

        _allMiddleBlocks = new List<GameObject>();
    }

    protected virtual void Update()
    {
        if (startedDrag)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            UpdateLength(mousePos);
        }
    }

    protected virtual void LateUpdate()
    {
        if (startedDrag)
        {
            uvOffset += (uvAnimationRate * Time.deltaTime);
            //lineRenderer.material.SetTextureOffset("_MainTex", uvOffset);
        }
    }

    public void Begin(Vector2 pos)
    {
        

        startedDrag = true;
        initialPos = pos;

        Init();

        _startPosition = initialPos;// + (Vector3.up * (_sizeStartBlock.y / 2));

        var rb = headArrow.GetComponent<Rigidbody2D>();
        rb.isKinematic = true;

    }

    public void UpdateLength(Vector3 pos)
    {
        _deltaMove = headArrow.transform.position - pos;

        if (_deltaMove != Vector3.zero)
        {
            if (_isStartMove)
            {
                _isStartMove = false;
                //foreach (var item in _allMiddleBlocks)
                //{
                //    DOTween.Kill(item);
                //}
            }

            _endPosition = pos - (headArrow.transform.up * (_sizeHeadBlock.y / 5f));
            float countTemp = (Vector3.Distance(_startPosition, _endPosition) / (_sizeMiddleBlock.y + _interval));
            Vector3 segment = new Vector3(_endPosition.x - _startPosition.x, _endPosition.y - _startPosition.y) / countTemp;
            int count = Mathf.CeilToInt(countTemp);


            headArrow.transform.position = pos;

            var angle = Mathf.Atan2(pos.y - initialPos.y, pos.x - initialPos.x) * 180 / Mathf.PI;
            var rotation = Quaternion.Euler(0, 180, -(angle - 90));
            startArrow.transform.localRotation = rotation;
            headArrow.transform.localRotation = rotation;

            _middleBlockContainer.transform.position = pos;
            _middleBlockContainer.transform.localRotation = rotation;


            if (count != _blockCount)
            {
                GameObject block = null;
                for (int i = 0; i < count; i++)
                {
                    if (_blockCount < count)
                    {
                        _blockCount++;
                        block = MonoBehaviour.Instantiate(middlePrefab, _middleBlockContainer.transform, false);
                        block.transform.localEulerAngles = Vector3.zero;                       

                        if (_allMiddleBlocks.Count != 0)
                        {
                            var removeBlock = _allMiddleBlocks.Find((x) => x.transform.localPosition.y == _allMiddleBlocks.Min(k => k.transform.localPosition.y));
                            block.transform.localPosition = removeBlock.transform.localPosition - Vector3.up * (_sizeMiddleBlock.y + _interval);//pos - (segment * _blockCount); //temp
                        }
                        else
                            block.transform.localPosition = Vector3.zero;


                        _allMiddleBlocks.Add(block);

                    }
                    else if (_blockCount > count)
                    {
                        float minValue = float.MaxValue;

                        var removeBlock = _allMiddleBlocks.Find((x) => x.transform.localPosition.y == _allMiddleBlocks.Min(k => k.transform.localPosition.y));

                        MonoBehaviour.Destroy(removeBlock);
                        _allMiddleBlocks.Remove(removeBlock);
                        _blockCount--;
                    }
                    else
                        break;
                }

            }
        }
        else
        {


            _distanceBettwenPoints = Vector3.Distance(initialPos, pos);
            if (_distanceBettwenPoints < 3)
                _speedMove = (_startSpeed * _distanceBettwenPoints) / _startDistance;
            else if(_speedMove != _startSpeed)
                _speedMove = _startSpeed;

            //if (_isStartMove)
            //    return;
            foreach (var item in _allMiddleBlocks)
            {
                MoveBlock(item);
            }
            
        }

    }

    protected void DestroyMiddleBlocks()
    {
        _blockCount = 0;
        foreach (var item in _allMiddleBlocks)
        {
            MonoBehaviour.Destroy(item);
        }
        _allMiddleBlocks.Clear();
    }


    public float coef = 0f;
    private Vector3 newPosition;
    private void MoveBlock(GameObject block)
    {

        oldPosition = block.transform.localPosition;       

        if (Vector3.Distance(block.transform.position, _endPosition) < 0.15f)
        {
            //coef = 0;
            //block.transform.position = _startPosition;
            var removeBlock = _allMiddleBlocks.Find((x) => x.transform.localPosition.y == _allMiddleBlocks.Min(k => k.transform.localPosition.y));
            block.transform.localPosition = removeBlock.transform.localPosition - Vector3.up * (_sizeMiddleBlock.y + _interval);
        }

        block.transform.localPosition = Vector3.MoveTowards(block.transform.localPosition, Vector3.zero, Time.deltaTime * _speedMove);
        //UpdateSinusoidalPosition(block);

    }

    private void UpdateSinusoidalPosition(GameObject block)
    {
        
        newRotation = block.transform.eulerAngles;
        newPosition = block.transform.localPosition;
        var disCenter = Vector3.Distance(initialPos, _endPosition);
        var disCur = Vector3.Distance(new Vector3(block.transform.position.x, block.transform.position.y, 0), _endPosition);
        coef = (100 / disCenter * disCur) / 100;
        float sin = Mathf.Sin(Mathf.Clamp01(coef) * Mathf.PI);//Mathf.Sin(coef);
        newPosition.z = sin;
        block.transform.localPosition = newPosition;
        newRotation.x = AngleBetweenVector2(oldPosition, block.transform.localPosition) - 90;
        block.transform.localEulerAngles = newRotation;
        //Debug.LogError(AngleBetweenVector2(block.transform.localPosition, newPosition));

        //Debug.LogError("Time: " + block.transform.position);

        //Debug.Log("sin: " + sin + "coef: " + coef + "disCenter: " + disCenter + "disCur: " + disCur);
    }

    private float AngleBetweenVector2(Vector3 vec1, Vector3 vec2)
    {
        Vector3 diference = vec2 - vec1;
        float sign = (vec2.z < vec1.z) ? -1.0f : 1.0f;
        return Vector3.Angle(Vector3.forward, diference);
    }


    public virtual void OnCardSelected(BoardCreature creature)
    {
    }

    public virtual void OnCardUnselected(BoardCreature creature)
    {
    }

    public virtual void OnPlayerSelected(PlayerAvatar player)
    {
    }

    public virtual void OnPlayerUnselected(PlayerAvatar player)
    {
    }

    protected void CreateTarget(Vector2 pos)
    {
        Destroy(target);
        var rect = new Rect(0, 0, targetTexture.width, targetTexture.height);
        var targetSprite = Sprite.Create(targetTexture, rect, new Vector2(0.5f, 0.5f));
        target = new GameObject();
        target.transform.parent = gameObject.transform;
        var sprite = target.AddComponent<SpriteRenderer>();
        sprite.sprite = targetSprite;
        sprite.sortingLayerName = "HandCards";
        target.transform.position = pos;
        target.transform.localScale = new Vector2(1.5f, 1.5f);
    }
}