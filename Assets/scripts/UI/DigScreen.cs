using Game.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public enum Tool
{
    DEFAULT=0,
    SHOVEL=1,
    TROWEL=2,
    BRUSH=3
}
public class DigScreen : MainScreen
{
    private bool canInteract;
    public Tool tool;
    public List<Transform> buttonList;
    public Transform excavationTransform;
    public int[,] matriz;

    private void OnEnable()
    {
        tool = Tool.DEFAULT;
        canInteract = false;
        int rows = 8;
        int cols = 15;

        matriz = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matriz[i, j] = 3;
            }
        }
    }

    private void OnDisable()
    {
    }

    public void activateButton(string tool)
    {
        if (Enum.TryParse<Tool>(tool, out var newTool))
        {
            if(this.tool != newTool)
            {
                buttonList.ForEach((b) =>
                {
                    if(b.TryGetComponent(out AnimationSystem animationSystem))
                    {
                        if (animationSystem.currentAnimation.Equals("activate"))
                        {
                            animationSystem.changeAnimation("deactivate");
                        }
                    }
                });
                if (buttonList[(int)newTool - 1].TryGetComponent(out AnimationSystem animationSystem))
                {
                    this.tool = newTool;
                    animationSystem.changeAnimation("activate");
                }
            }
        }
    }
}
