﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkyScript : MonoBehaviour
{
    public Sprite SkySprite;
    private SpriteRenderer _skySpriteRenderer;
    private float _depth = 100f;
    private float _lastCameraX;

    void Awake()
    {
        GameObject gameObject = new GameObject();
        _skySpriteRenderer = gameObject.AddComponent<SpriteRenderer>() as SpriteRenderer;
        _skySpriteRenderer.sprite = SkySprite;
        Vector3 worldBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 worldBottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
        Vector3 worldTopLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
        float width = System.Math.Abs(worldBottomRight.x - worldBottomLeft.x);
        float height = System.Math.Abs(worldTopLeft.y - worldBottomLeft.y);
        float xScale = width / _skySpriteRenderer.size.x;
        float yScale = 12f * height / _skySpriteRenderer.size.y;
        _skySpriteRenderer.transform.localScale = new Vector3(xScale, yScale, 1f);
        _skySpriteRenderer.transform.position = new Vector3(0f, _skySpriteRenderer.bounds.size.y/2 - height, _depth);

        _skySpriteRenderer.gameObject.layer = Constants.NoCollisionLayer;
        SetSkyStripPosition(0f);
    }

    private void SetSkyStripPosition(float xShift)
    { 
        /*
        Vector3 worldBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 worldTopLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
        Vector3 worldBottomRight = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
        float width = worldBottomRight.x - worldBottomLeft.x;
        float height = worldTopLeft.x - worldBottomLeft.x;
        float newXPos = Camera.main.transform.position.x - width / 2;
        */

        float newXPos = _skySpriteRenderer.transform.position.x + xShift;
        float newYPos = _skySpriteRenderer.transform.position.y;
        float newZPos = _depth;
        _skySpriteRenderer.transform.position = new Vector3(newXPos, newYPos, _depth);
    }

    void Update()
    {
        float delta = Camera.main.transform.position.x - _lastCameraX;
        SetSkyStripPosition(delta);
        _lastCameraX = Camera.main.transform.position.x;
    }
}