using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class Dialogue
{
    public string id;
    public string title;
    public string text;
    public List<Reply> reply;
    public string nextDialog;
}

[Serializable]
public class Reply
{
    public string text;
    public string nextDialog;
}

[Serializable]
public class Dialogues
{
    public List<Dialogue> dialogueList;
    public GameObject mainScreen;
    public GameObject dialogScreen;

}

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;
    [SerializeField] private TextAsset dialogText;
    public List<Dialogue> dialogueList;

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


