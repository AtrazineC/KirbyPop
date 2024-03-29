﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Ball : MonoBehaviour
{
  public UIManager UIManager;

  public Rigidbody2D rb;
  public Rigidbody2D hook;
  public GameObject nextBall;
  public bool useOneKey = true;

  public float releaseTime = 0.15f;
  private float currentDragDistance = 2.25f;
  private float maxDragDistance = 2.75f;
  private float minDragDistance = 0.4f;

  private bool aiming = true;
  private bool hasShot = false;

  private float angle = (float)(Math.PI);
  private bool direction = true;

  [Header("Unity stuff")]
  public Image healthbar;
  public Canvas healthCanvas;

  private Vector3 launch_direction
  {
    get
    {
      return new Vector3((float)(Math.Cos(angle)), (float)(Math.Sin(angle)), 0).normalized;
    }
  }

  private double originX;
  private double originY;

  private LineRenderer lineRenderer;
  private static int traj_nsteps = 20;
  private Vector2[] traj_arr = new Vector2[traj_nsteps];

  private Vector3 calculate_velocity()
  {
    // Calculates the launch velocity
    return -(currentDragDistance / maxDragDistance) * launch_direction * 20f;
  }

  private void calculate_traj()
  {
    Vector3 v_i = calculate_velocity();
    for (int i = 0; i < traj_nsteps; i++)
    {
      float stepsize = i * 0.02f;
      traj_arr[i].x = rb.position.x + v_i.x * stepsize;
      traj_arr[i].y = rb.position.y + v_i.y * stepsize + 0.5f * Physics.gravity.y * stepsize * stepsize;
    }
  }


  public void move(int action)
  {
    if (!hasShot)
    {
      switch (action)
      {
        // up
        case 0:
          { currentDragDistance = (currentDragDistance <= maxDragDistance) ? currentDragDistance + 0.15f : currentDragDistance; break; }
        // down
        case 1:
          { currentDragDistance = (currentDragDistance >= minDragDistance) ? currentDragDistance - 0.15f : currentDragDistance; break; }
        // left
        case 2:
          { angle = (angle <= 8.5 * Math.PI / 6) ? 0.1f + angle : angle; break; }
        // right
        case 3: { angle = (angle >= 4 * Math.PI / 6) ? -0.1f + angle : angle; break; }
        // shoot (space)
        case 4:
          {
            hasShot = true;
            rb.isKinematic = false;
            rb.velocity = calculate_velocity();
            StartCoroutine(Release());
            break;
          }
      }
      rb.MovePosition((Vector3)hook.position + (currentDragDistance * launch_direction));
    }
  }

  private void Start()
  {
    rb.isKinematic = true;
    lineRenderer = gameObject.AddComponent<LineRenderer>();
    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    lineRenderer.widthMultiplier = 0.07f;
    lineRenderer.positionCount = traj_nsteps;
    originX = hook.position.x;
    originY = hook.position.y;

    rb.position = ((Vector3)hook.position + (currentDragDistance * launch_direction));
  }

  void Update()
  {
    healthbar.fillAmount = currentDragDistance / maxDragDistance;

    if (useOneKey)
    {
      if (Input.GetKey(KeyCode.Space))
      {
        if (aiming)
        {
          // Setting the trajectory of the ball
          direction = (angle <= 4 * Math.PI / 6) ? true : direction;
          direction = (angle >= 8.5 * Math.PI / 6) ? false : direction;
          angle = direction ? angle + 0.01f : angle - 0.01f;

          transform.position = (Vector3)hook.position + (currentDragDistance * launch_direction);
        }
        else
        {
          // Setting the power of the ball
          direction = (currentDragDistance >= maxDragDistance) ? false : direction;
          direction = (currentDragDistance <= minDragDistance) ? true : direction;
          currentDragDistance = direction ? currentDragDistance + 0.025f : currentDragDistance - 0.025f;

          transform.position = (Vector3)hook.position + (currentDragDistance * launch_direction);
        }
      }

      // Change from aiming to power to launch. 
      if (Input.GetKeyUp(KeyCode.Space))
      {
        if (aiming)
        {
          aiming = false;
        }
        else if (hasShot == false)
        {
          hasShot = true;
          rb.isKinematic = false;
          rb.velocity = calculate_velocity();
          StartCoroutine(Release());
        }
      }
    }
    else
    {
      if (Input.GetKeyDown(KeyCode.UpArrow))
      {
        move(0);
      }
      else if (Input.GetKeyDown(KeyCode.DownArrow))
      {
        move(1);
      }
      else if (Input.GetKeyDown(KeyCode.LeftArrow))
      {
        move(2);
      }
      else if (Input.GetKeyDown(KeyCode.RightArrow))
      {
        move(3);
      }
      else if (Input.GetKeyDown(KeyCode.Space))
      {
        move(4);
      }
    }


    // For handling the trajectory line
    if (rb.isKinematic)
    {
      for (int i = 0; i < traj_nsteps; i++)
      {
        calculate_traj();
        lineRenderer.SetPosition(i, traj_arr[i]);
      }

    }
    else
    {
      lineRenderer.enabled = false;
    }


  }
  IEnumerator Release()
  {
    healthCanvas.enabled = false;
    // For launching the ball, and moving control to the next ball. 
    UIManager.BallsUsed++;
    yield return new WaitForSeconds(releaseTime);
    this.enabled = false;

    yield return new WaitForSeconds(2f);
    if (nextBall != null && UIManager.EnemiesAlive != 0)
    {
      nextBall.SetActive(true);
    }
    else
    {
      yield return new WaitForSeconds(3f);
      if (UIManager.EnemiesAlive != 0)
      {
        UIManager.levelFail();
      }
    }
  }
}

