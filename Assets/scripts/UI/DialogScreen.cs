using Game.Entities;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Windows;

public class DialogScreen : MainScreen, INavigation, IInteraction
{
    public string dialogId;
    public Dialogue currentDialog;
    private TextMeshProUGUI text;
    private bool canInteract;
    [Header("Hierarchy Component")]
    private TextMeshProUGUI title;
    [SerializeField]
    private Transform dialogBox;
    [SerializeField]
    private Transform dialogOptions;
    private List<Transform> dialogOptionList;

    public int field { get; set; }
    public int prevField { get; set; }
    public int totalFields { get; set; }


    private void Awake()
    {
        canInteract = false;
        if (dialogBox.Find("Title").TryGetComponent(out TextMeshProUGUI title))
        {
            this.title = title;
        }
        if (dialogBox.Find("Text").TryGetComponent(out TextMeshProUGUI text))
        {
            this.text = text;
        }
        dialogOptionList = new List<Transform>();
        foreach (Transform child in dialogOptions)
        {
            dialogOptionList.Add(child);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(nextDialog());
    }

    private void OnDisable()
    {
        text.text = "";
    }

    public void navigation(Vector2 value)
    {
        if (value.y > 0 && field > 0)
        {
            prevField = field;
            field--;
        }
        if (value.y < 0 && field < totalFields - 1)
        {
            prevField = field;
            field++;
        }
        if (dialogOptionList[prevField].TryGetComponent(out AnimationSystem prevAnimationSystem))
        {
            prevAnimationSystem.changeAnimation("active");
        }
        if (dialogOptionList[field].TryGetComponent(out AnimationSystem currentAnimationSystem))
        {
            currentAnimationSystem.changeAnimation("selected");
        }
    }

    public int scroller(float field)
    {
        throw new System.NotImplementedException();
    }

    public void interact(int value = 0)
    {
        if (canInteract)
        {
            canInteract = false;
            DialogManager.Instance.invokeEvent(currentDialog.action);

            if (hasNoReply(currentDialog.reply))
            {
                if (string.IsNullOrEmpty(currentDialog.nextDialog))
                {
                    UIManager.Instance.toPrevScreen();
                }
                else
                {
                    currentDialog = DialogManager.Instance.findDialog(currentDialog.nextDialog);
                    StartCoroutine(nextDialog());
                }
            }
            else
            {
                DialogManager.Instance.invokeEvent(currentDialog.reply[field].action);

                if (!currentDialog.reply[field].breakDialog)
                {
                    if (currentDialog.reply[field].nextDialog != null)
                    {
                        currentDialog = DialogManager.Instance.findDialog(currentDialog.reply[field].nextDialog);

                        StartCoroutine(nextDialog());
                    }
                    else
                    {
                        UIManager.Instance.toPrevScreen();
                    }

                    dialogOptions.gameObject.SetActive(false);
                }
            }
        }
    }

    private bool hasNoReply(List<Reply> reply)
    {
        return reply == null || reply.Count == 0;
    }

    public void startDialog(string dialogId)
    {
        currentDialog = DialogManager.Instance.findDialog(dialogId);
        UIManager.Instance.toScreen(ScreenName.DIALOG);
        dialogOptions.gameObject.SetActive(false);
    }

    IEnumerator nextDialog()
    {
        canInteract = false;
        yield return new WaitForSeconds(0.2f);
        string text = currentDialog.text;
        this.text.text = "";
        this.title.text = currentDialog.title;
        foreach (char letter in text.ToCharArray())
        {
            this.text.text += letter;
            yield return new WaitForSeconds(0.02f);
        }
        canInteract = true;
        if (currentDialog.reply.Count > 0 && currentDialog.reply != null)
        {
            activeOption();
        }
    }

    private void activeOption()
    {
        dialogOptions.gameObject.SetActive(true);
        List<Reply> reply = currentDialog.reply;

        totalFields = reply.Count;
        field = 0;
        prevField = 0;

        if (totalFields > 0 && totalFields <= 3)
        {

            int index = 0;
            dialogOptionList.ForEach(d =>
            {
                if(index < totalFields)
                {
                    if (!d.gameObject.activeSelf)
                    {
                        d.gameObject.SetActive(true);
                    }
                    if (d.Find("Text").TryGetComponent(out TextMeshProUGUI text))
                    {
                        text.text = reply[index].text;
                    }
                } else
                {
                    d.gameObject.SetActive(false);
                }
                if (d.TryGetComponent(out AnimationSystem animationSystem) && d.gameObject.activeSelf)
                {
                    animationSystem.changeAnimation("active");
                }
                index++;
            });
            if(dialogOptionList[0].TryGetComponent(out AnimationSystem animationSystem))
            {
                animationSystem.changeAnimation("selected");
            }
        }
    }
}
