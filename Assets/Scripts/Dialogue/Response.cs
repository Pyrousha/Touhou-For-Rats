using UnityEngine;

[System.Serializable]
public class Response
{
    [SerializeField] private string responseText;
    [SerializeField] private DialogueObject nextDialogueObject;

    public string ResponseText => responseText;

    public DialogueObject DialogueObject => nextDialogueObject;
}
