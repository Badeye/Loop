﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMovingKillDot : LevelObject
{
    [Header("Settings")]
    public float fadeDuration = 0.5f;

    [Header("Components")]
    public SpriteRenderer sr;

    [Header("List of GridDots")]
    //List of GridDots to define where the Dot should move
    public List<GridDot> gridDotList;

    int lenghtOfList;
    int listPosition;

    Transform target;
    float directionIndicatorSpeed = 5f;

    //The StartDot where the MovingKD was placed
    GridDot startDot;

    //The direction the MovingKD is looking at
    Vector2 lookDirection;

    private float defaultLocalScale;            // default player dot size
    private float scale = 0f;
    private float opacity = 0f;

    //ParticleSystems
    ParticleSystem killFeedback;
    public GameObject gridDotParticleSystem;

    //Animation Component
    private Animation deathAnim;

    //To make sure things only get called once in OnTriggerEnter2D
    int count = 0;

    //40DFFFFF HexColor to color the GridDots the MovingKillDot is moving on
    Color myColor = new Color32(0x40, 0xDF, 0xFF, 0xFF);

    private void Start()
    {
        RythmManager.onBPM.AddListener(OnRythmMove);

        defaultLocalScale = transform.localScale.x;
        StartCoroutine(Fade(true));

        killFeedback = GetComponent<ParticleSystem>();

        deathAnim = GetComponentInChildren<Animation>();

        startDot = transform.parent.gameObject.GetComponent<GridDot>();
        gridDotList.Insert(0, startDot);
        listPosition = 0;

        lenghtOfList = gridDotList.Count;

        ColorGridDots();
        AddParticleSystemToGridDots();
    }

    private void Update()
    {
        UpdateLookDirection();
    }

    void OnRythmMove(BPMinfo bpm)
    {
        if (bpm.Equals(RythmManager.movingKillDotBPM))
        {
            if(Player.allowMove)
            {
                MoveToNextDot();

                foreach (GridDot dot in gridDotList)
                {
                    ParticleSystem ps = dot.GetComponentInChildren<ParticleSystem>();
                    ps.Play();
                }
            }
        }

        if (bpm.Equals(RythmManager.playerBPM) || bpm.Equals(RythmManager.playerDashBPM) || bpm.Equals(RythmManager.movingKillDotBPM))
        {
            if (IsTouchingPlayer(false))
            {
                Game.SetState(Game.State.DeathOnNextBeat);
                count = 0;
            }
        }

    }

    void AddParticleSystemToGridDots()
    {
        foreach (GridDot dot in gridDotList)
        {
            GameObject particleSystem = GameObject.Instantiate(gridDotParticleSystem, Vector2.zero, Quaternion.identity);
            particleSystem.transform.parent = dot.transform;
            particleSystem.transform.localPosition = Vector3.zero;
            particleSystem.transform.localScale = gridDotParticleSystem.transform.localScale;
        }
    }

    void ColorGridDots()
    {
        foreach(GridDot dot in gridDotList)
        {
            SpriteRenderer spriteRenderer = dot.GetComponent<SpriteRenderer>();
            spriteRenderer.color = myColor;
        }
    }

    void UpdateLookDirection()
    {
        foreach(GridDot dot in gridDotList)
        {
            if(this.transform.position == gridDotList[listPosition].transform.position)
            {
                if(listPosition < (lenghtOfList-1))
                {
                    lookDirection = gridDotList[listPosition + 1].transform.position - this.transform.position;
                    target = gridDotList[listPosition + 1].transform;
                    lookDirection = lookDirection.normalized;
                    listPosition = listPosition + 1;
                }

                else if(listPosition == (lenghtOfList-1))
                {
                    lookDirection = gridDotList[0].transform.position - this.transform.position;
                    target = gridDotList[0].transform;
                    lookDirection = lookDirection.normalized;
                    listPosition = 0;
                }

                Vector2 direction = target.position - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, directionIndicatorSpeed * Time.deltaTime);

                Debug.Log("LIST POSITION" + listPosition);
            }
        }
    }

    void MoveToNextDot()
    {
        Remove();

        GridDot activeDot = transform.parent.gameObject.GetComponent<GridDot>();
        GridDot nextDot = Grid.GetNearestActiveMovingDot(activeDot, lookDirection);

        this.transform.parent = nextDot.transform;
        this.transform.localPosition = Vector3.zero;

        Appear();
    }

    public void Remove()
    {
        StopCoroutine("Fade");
        StartCoroutine(Fade(false));
    }
    
    public void Appear()
    {
        StopCoroutine("Fade");
        StartCoroutine(Fade(true));
    }

    public void PlayParticleSystem()
    {
        killFeedback.Play();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (deathAnim != null)
                deathAnim.Play("MovingKillDotDeathFast");

            if (count < 1)
            {
                AudioManager.instance.Play("OnDeathTrigger");
                PlayParticleSystem();
                count++;
            }

            if(Player.dot0 != null && Player.dot0.transform.position == this.transform.position)
            {
                SpriteRenderer sr = Player.dot0.GetComponent<SpriteRenderer>();
                sr.enabled = false;
            }

            else if(Player.dot1 != null && Player.dot1.transform.position == this.transform.position)
            {
                SpriteRenderer sr = Player.dot1.GetComponent<SpriteRenderer>();
                sr.enabled = false;
            }
        }
    }

    IEnumerator Fade(bool fadeIn)
    {
        float elapsedTime = 0f;
        float startScale = scale;
        float startOpacity = opacity;

        while (elapsedTime <= fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            Color c = sr.color;
            if (fadeIn)
            {
                scale = Mathf.SmoothStep(startScale, defaultLocalScale, (elapsedTime / fadeDuration));
                transform.localScale = new Vector3(scale, scale, 1);

                opacity = Mathf.SmoothStep(startOpacity, 1, (elapsedTime / fadeDuration));
            }
            else
            {
                scale = Mathf.SmoothStep(startScale, 0, (elapsedTime / fadeDuration));
                transform.localScale = new Vector3(scale, scale, 1);
                opacity = Mathf.SmoothStep(startOpacity, 0, (elapsedTime / fadeDuration));
            }

            c.a = opacity;
            sr.color = c;

            yield return null;
        }

        yield return null;
    }
}
