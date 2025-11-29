using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Dialogue
{
    public string id;
    public string title;
    public string text;
    public List<Reply> reply;
    public string nextDialog;
    public string action;
}

[Serializable]
public class Reply
{
    public string text;
    public string nextDialog;
    public string action;
    public bool breakDialog = false;
}

[Serializable]
public class Dialogues
{
    public List<Dialogue> dialogueList;
    public GameObject mainScreen;
    public GameObject dialogScreen;

}

[Serializable]
public class DialogAction
{
    public string id;
    public UnityEvent action;
}

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;
    [SerializeField] private TextAsset dialogText;
    public List<Dialogue> dialogueList;
    public List<DialogAction> actionList;

    public void testEvent(string str)
    {
        Debug.Log(str);
    }

    public void invokeEvent(string str)
    {
        DialogAction dialogEvent = actionList.FirstOrDefault<DialogAction>(action => action.id.Equals(str));
        if(dialogEvent != null)
        {
            dialogEvent.action.Invoke();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);

        Dialogues dialogues = JsonUtility.FromJson<Dialogues>(dialogText.text);
        dialogueList = dialogues.dialogueList;
    }

    public Dialogue findDialog(string dialogId)
    {
        Dialogue dialog = dialogueList.FirstOrDefault<Dialogue>(d => d.id.Equals(dialogId));
        if (dialog != null)
        {
            return dialog;
        }
        return null;
    }

}


