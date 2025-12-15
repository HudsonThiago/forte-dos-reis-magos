using Game.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Tool
{
    DEFAULT=0,
    SHOVEL=1,
    TROWEL=2,
    BRUSH=3
}

[Serializable]
public class FoundObjects
{
    public string description;
    public GameObject image;
    public bool founded;
}

public class DigScreen : MainScreen
{
    private bool canInteract;
    public List<Sprite> imageList;
    public Tool tool;
    public List<Transform> buttonList;
    public Transform excavationTransform;
    public List<List<GameObject>> excavationList;
    public int[,] matriz;
    public List<FoundObjects> foundObjects;

    private void OnEnable()
    {
        startMinigame();
    }

    void startMinigame()
    {
        excavationList = new List<List<GameObject>>();
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
        excavationList.Clear();

        foreach (Transform row in excavationTransform)
        {
            List<GameObject> collumns = new List<GameObject>();

            foreach (Transform cell in row)
            {
                if(cell.gameObject.TryGetComponent(out Image image))
                {
                    image.sprite = imageList[2];
                    image.color = new Color(1, 1, 1, 1);
                }
                collumns.Add(cell.gameObject);
            }

            excavationList.Add(collumns);
        }

        foundObjects.ForEach(i =>
        {
            i.image.SetActive(false);
        });
    }

    public void getLocation(int coord)
    {
        int x = (int)(coord / 15);
        int y = coord % 15;

        if (tool.Equals(Tool.BRUSH))
        {
            setHole(x, y, 1);
        }
        if (tool.Equals(Tool.TROWEL))
        {
            setHole(x, y, 2);
            setHole(x - 1, y, 1);
            setHole(x + 1, y, 1);
            setHole(x, y - 1, 1);
            setHole(x, y + 1, 1);
        }
        if (tool.Equals(Tool.SHOVEL))
        {
            setHole(x, y, 2);
            setHole(x - 1, y, 2);
            setHole(x + 1, y, 2);
            setHole(x, y - 1, 2);
            setHole(x, y + 1, 2);
            setHole(x - 1, y - 1, 1);
            setHole(x + 1, y + 1, 1);
            setHole(x + 1, y - 1, 1);
            setHole(x - 1, y + 1, 1);
        }

        checkFoundedRelics();

        if(
             foundObjects[0].founded &&
             foundObjects[1].founded &&
             foundObjects[2].founded &&
             foundObjects[3].founded &&
             foundObjects[4].founded &&
             foundObjects[5].founded
          )
        {
            UIManager.Instance.finishExcavation();
        }
    }

    public void checkFoundedRelics(){

        // Pote de barro
        if (!foundObjects[0].founded)
        {
            if (CheckArea(1, 1, 3))
            {
                foundObjects[0].image.SetActive(true);
                foundObjects[0].founded = true;
            }
        }

        // Prato de cerâmica
        if (!foundObjects[1].founded)
        {
            if (CheckArea(4, 9, 3))
            {
                foundObjects[1].image.SetActive(true);
                foundObjects[1].founded = true;
            }
        }

        // Projétil de ferro
        if (!foundObjects[2].founded)
        {
            if (CheckArea(1, 12, 2))
            {
                foundObjects[2].image.SetActive(true);
                foundObjects[2].founded = true;
            }
        }

        // Cachimbo
        if (!foundObjects[3].founded)
        {
            if (CheckArea(0, 6, 2))
            {
                foundObjects[3].image.SetActive(true);
                foundObjects[3].founded = true;
            }
        }

        // Pote de cerâmica
        if (!foundObjects[4].founded)
        {
            if (CheckArea(6, 3, 2))
            {
                foundObjects[4].image.SetActive(true);
                foundObjects[4].founded = true;
            }
        }

        // Moedas
        if (!foundObjects[5].founded)
            if (
                matriz[1, 10] == 0 &&
                matriz[3, 5] == 0 &&
                matriz[5, 13] == 0 &&
                matriz[6, 7] == 0
               )
            {
                {
                foundObjects[5].image.SetActive(true);
                foundObjects[5].founded = true;
            }
        }
    }
    private bool CheckArea(int startX, int startY, int r)
    {
        for (int x = 0; x < r; x++)
        {
            for (int y = 0; y < r; y++)
            {
                if (matriz[startX + x, startY + y] != 0)
                    return false;
            }
        }
        return true;
    }


    public void setHole(int x, int y, int hole)
    {
        if (x >= 0 && x < 8 && y >= 0 && y < 15)
        {
            matriz[x, y] = matriz[x, y] - hole;
            if (excavationList[x][y].TryGetComponent(out Image image))
            {
                if (matriz[x, y] - 1 >= 0)
                {
                    image.sprite = imageList[matriz[x, y] - 1];
                    image.color = new Color(1, 1, 1, 1);
                }
                else
                {
                    matriz[x, y] = 0;
                    image.sprite = null;
                    image.color = new Color(0, 0, 0, 0);
                }
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
