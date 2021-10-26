﻿using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loader : MonoBehaviour
{
    public GameObject gameManager;
    public GameObject uiManager;
    private void Awake()
    {
        if (GameManager.Instance == null)
			Instantiate(gameManager);

        if (UIManager.Instance == null)
            Instantiate(uiManager);
    }
}
