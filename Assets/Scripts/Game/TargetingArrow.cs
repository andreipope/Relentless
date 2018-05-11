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

    public BoardCreature selectedCard { get; set; }
    public PlayerAvatar selectedPlayer { get; set; }

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

    private GameObject _middleBlockContainer,
                       _arrowContainer;

    private bool _isStartMove = false;

    private Vector3 newRotation,
                oldPosition;



    protected void Init()
    {
        _arrowContainer = new GameObject("ArrowContainer");
        _arrowContainer.transform.SetParent(transform, false);
        //_arrowContainer = gameObject;
        _arrowContainer.transform.position = initialPos;

        _middleBlockContainer = new GameObject("MiddleBlockContainer");
        _middleBlockContainer.transform.SetParent(_arrowContainer.transform, false);

        _sizeMiddleBlock = middlePrefab.GetComponent<MeshRenderer>().bounds.size;
        _sizeHeadBlock = headPrefab.transform.Find("ArrowObj").GetComponent<MeshRenderer>().bounds.size;
        _sizeStartBlock = startPrefab.transform.Find("ArrowObj").GetComponent<MeshRenderer>().bounds.size;

        var position = initialPos + new Vector3(0, _sizeStartBlock.z / 1, 0.02f);

        startArrow = MonoBehaviour.Instantiate(startPrefab, _arrowContainer.transform);
        startArrow.transform.position = position;
        _startPosition = position + Vector3.up * _sizeMiddleBlock.z * 1.5f;

        headArrow = MonoBehaviour.Instantiate(headPrefab, _arrowContainer.transform);
        headArrow.transform.position = initialPos;

        head = headArrow.transform.Find("ArrowObj").gameObject;

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
        }
    }

    public void Begin(Vector2 pos)
    {
        startedDrag = true;
        initialPos = pos;
        //initialPos.z = -5f;
        Init();
        var rb = headArrow.GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
    }

    public void UpdateLength(Vector3 pos)
    {
        //pos.z = -5f;
        _distanceBettwenPoints = Vector3.Distance(_startPosition, pos);

        _deltaMove = headArrow.transform.position - pos;

        if (_deltaMove != Vector3.zero)
        {
            if (_isStartMove)
            {
                _isStartMove = false;
            }
            headArrow.transform.position = pos;
            _startPosition = startArrow.transform.position;// + startArrow.transform.up * _sizeMiddleBlock.z;

            _endPosition = pos;// - (headArrow.transform.up * 0.4f);
            _endPosition.z = pos.z + 0.05f;

            float countTemp = (Vector3.Distance(_startPosition, _endPosition) / (_sizeMiddleBlock.y + _interval));          
            Vector3 segment = new Vector3(_endPosition.x - _startPosition.x, _endPosition.y - _startPosition.y) / countTemp;
            int count = Mathf.CeilToInt(countTemp) -1;

            if (Vector3.Distance(initialPos, _endPosition) < 1.5f)
                startArrow.SetActive(false);
            else
                startArrow.SetActive(true);

            var angle = Mathf.Atan2(pos.y - initialPos.y, pos.x - initialPos.x) * 180 / Mathf.PI;
            var rotation = Quaternion.Euler(0, 180, -(angle - 90));

            //startArrow.transform.rotation = rotation;
            //headArrow.transform.rotation = rotation;

            _middleBlockContainer.transform.position = _endPosition;
            _middleBlockContainer.transform.localPosition = new Vector3(0, _middleBlockContainer.transform.localPosition.y, 0);
            headArrow.transform.localPosition = new Vector3(0, headArrow.transform.localPosition.y, 0);
            //_middleBlockContainer.transform.localRotation = rotation;
            _arrowContainer.transform.rotation = rotation;

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

        var blocks = _allMiddleBlocks.OrderBy(x => (_middleBlockContainer.transform.position - x.transform.position).sqrMagnitude).ToList();
        blocks.Reverse();
        //for (int i = 0; i < blocks.Count; i++)
        //{
        //    index = i;
        //    Debug.Log("index: " + index + "position: " + blocks[i].transform.position);
        //}
        GameObject target = startArrow;
        bool isFirst = false;
        for (int i = 0; i < blocks.Count; i++)
        {
            if (i > 0)
                target = blocks[i - 1];
            else
            {
                isFirst = true;
            }
            UpdateSinusoidalPosition(blocks[i], target);
            if (isFirst)
            {
                blocks[0].transform.localEulerAngles = new Vector3(25, 0, 0);
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
        if (GameManager.Instance.tutorial)
            return;

        oldPosition = block.transform.localPosition;

        if (Vector3.Distance(block.transform.localPosition, Vector3.zero) < 0.15f)
        {
            //coef = 0;
            //block.transform.position = _startPosition;
            var removeBlock = _allMiddleBlocks.Find((x) => x.transform.localPosition.y == _allMiddleBlocks.Min(k => k.transform.localPosition.y));
            block.transform.localPosition = removeBlock.transform.localPosition - Vector3.up * (_sizeMiddleBlock.y + _interval);
        }

        block.transform.localPosition = Vector3.MoveTowards(block.transform.localPosition, Vector3.zero, Time.deltaTime * _speedMove);
        //UpdateSinusoidalPosition(block);

    }

    private void UpdateSinusoidalPosition(GameObject block, GameObject target, bool isFirst = false)
    {
        newRotation = block.transform.eulerAngles;
        newPosition = block.transform.localPosition;
        var disCenter = Vector3.Distance(_endPosition, _endPosition / 2);
        var disCur = Vector3.Distance(new Vector3(block.transform.position.x, block.transform.position.y, 0), _endPosition / 2);
        //coef = (1 / disCenter * disCur);
        //float sin = Mathf.Sin(Mathf.Clamp01(coef) * Mathf.PI) * 2;//Mathf.Sin(coef);
        //newPosition.z = sin;
        var coef = 1 - (1 / disCenter * disCur);
        newPosition.z = _distanceBettwenPoints / 4f * coef;
        //Vector3.Distance(_startPosition, _endPosition));

        Vector3 targetPosition = headArrow.transform.position;

        block.transform.localPosition = newPosition;


        float minDis = float.MaxValue;
        float dis;
        foreach (var item in _allMiddleBlocks)
        {
            dis = Vector3.Distance(block.transform.position, item.transform.position);
            if (minDis > dis && item.transform.localPosition.y < block.transform.localPosition.y)
            {
                targetPosition = item.transform.position;
                minDis = dis;
            }
            // Debug.LogError(item.transform.localPosition.y + " | " + block.transform.localPosition.y);
        }
        // Debug.LogError("MY: " + block.transform.position + " Target: " + targetPosition);
        //if (targetPosition == headArrow.transform.position)
        //    Debug.LogError(block.transform.position);


        newRotation.x = AngleBetweenVector(block.transform, target.transform.position);//AngleBetweenVector2(block.transform.position, targetPosition, block.transform.eulerAngles);
        newRotation.y = 0;
        newRotation.z = 0;

        //  Debug.DrawLine(block.transform.position, targetPosition, Color.red);
        //newRotation.x = Vector3.Angle(block.transform.position, targetPosition);
        block.transform.localEulerAngles = newRotation;
    }

    private float AngleBetweenVector2(Vector3 vec1, Vector3 vec2, Vector3 dirA)
    {
        Vector3 dir = (vec1 - vec2).normalized;
        float sign = (vec2.z < vec1.z) ? -1.0f : 1.0f;

        Debug.DrawRay(vec1, dir, Color.red);

        Vector3 angle = Vector3.zero;

        //Debug.Log(dir);

        //if (dir.x < 0)
        //{
        //    angle = Vector3.Cross(dirA, dirA + dir);
        //}
        //else
        angle = Vector3.Cross(dirA, dirA - dir);
        //float angleX = (Vector3.Angle(, (vec2 - vec1))) + 90 * sign;

        if (vec2.z < vec1.z)
            Debug.Log("position: " + vec1 + "angle: " + angle);

        return angle.x;

        Vector3 diference = vec2 - vec1;

        //Vector3 v2 = vec2 - vec1;
        //return Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
    }

    private float AngleBetweenVector(Transform tr1, Vector3 vec2)
    {
        Vector3 vec1 = tr1.position;
        Vector3 dir = (vec1 - vec2).normalized;
        Debug.DrawRay(vec1, dir, Color.red);

        Vector3 dirDis = (_endPosition - _startPosition).normalized;

        Vector3 diference = vec2 - tr1.position;
        float sign = (vec2.z < tr1.position.z) ? 1.0f : -1.0f;
        float angleX = Vector3.Angle(dirDis, diference) * sign + 180;
        return angleX;
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