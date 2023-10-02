using BeauRoutine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : Singleton<DialogueUI>
{
    [SerializeField] private DialogueObject currDialogueObject;
    public DialogueEvent[] dialogueEvents;


    private GameObject interactableObj;
    [Space(10)]
    [SerializeField] private HorizontalLayoutGroup portraitTextLayout;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private Image speakerImage;

    [SerializeField] private GameObject dialogueParent;
    [SerializeField] private Animator anim;

    public bool isOpen { get; private set; }

    private TypewriterEffect typewriterEffect;
    private ResponseHandler responseHandler;

    private void Awake()
    {
        typewriterEffect = GetComponent<TypewriterEffect>();
        responseHandler = GetComponent<ResponseHandler>();
    }

    public void ShowDialogue(DialogueObject dialogueObject, GameObject newInteractableObj)
    {
        currDialogueObject = dialogueObject;

        if (newInteractableObj != null)
            interactableObj = newInteractableObj;

        foreach (DialogueResponseEvents responseEvents in interactableObj.GetComponents<DialogueResponseEvents>())
        {
            if (responseEvents.DialogueObject == currDialogueObject)
            {
                AddResponseEvents(responseEvents.Events);
                break;
            }
        }

        dialogueEvents = null;

        foreach (DialogueEvents dialogueEvents in interactableObj.GetComponents<DialogueEvents>())
        {
            if (dialogueEvents.DialogueObject == currDialogueObject)
            {
                AddDialogueEvents(dialogueEvents.Events);
                break;
            }
        }

        isOpen = true;

        dialogueParent.SetActive(true);

        if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "FadeIn")
            anim.SetTrigger("FadeIn");

        //StartCoroutine(StepThroughDialogue(currDialogueObject));
        Routine.Start(this, StepThroughDialogue(dialogueObject));
    }

    public void SetCurrDialogueObject(DialogueObject newDO)
    {
        currDialogueObject = newDO;
    }
    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        responseHandler.AddResponseEvents(responseEvents);
    }

    public void AddDialogueEvents(DialogueEvent[] events)
    {
        dialogueEvents = events;
    }

    public void ClearEvents()
    {
        responseHandler.AddResponseEvents(null);
        dialogueEvents = null;
    }

    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
    {
        for (int i = 0; i < dialogueObject.DialogueLines.Length; i++)
        {
            Dialogue currDialogueLine = dialogueObject.DialogueLines[i];

            CharacterObject speaker = currDialogueLine.Speaker;
            if (speaker == null)
            {
                Debug.LogError("No speaker set for DialogueObject \"" + dialogueObject.name + "\", on dialogue line " + i);
                yield break;
            }
            portraitTextLayout.reverseArrangement = speaker.OnRight;

            //Set speaker labels + icon
            nameLabel.text = speaker.CharacterName;

            //Set portrait and voice to override values, or default if null
            //Debug.Log("defaultSprite: ");
            //Debug.Log(speaker.DefaultPortraitSprite);

            //Debug.Log("override sprite:");
            //Debug.Log(currDialogueLine.OptionalOverrides.PortraitImageOverride);

            if (currDialogueLine.OptionalOverrides.PortraitImageOverride != null)
                speakerImage.sprite = currDialogueLine.OptionalOverrides.PortraitImageOverride;
            else
                speakerImage.sprite = speaker.DefaultPortraitSprite;

            AudioClip voiceClip;
            if (currDialogueLine.OptionalOverrides.VoiceClipOverride != null)
                voiceClip = currDialogueLine.OptionalOverrides.VoiceClipOverride;
            else
                voiceClip = speaker.DefaultVoice;

            yield return null;

            //show text
            string dialogue = currDialogueLine.Text;
            typewriterEffect.Run(dialogue, textLabel, voiceClip);

            while (typewriterEffect.IsRunning)
            {
                yield return null;
            }

            //After text is done typing
            textLabel.color = typewriterEffect.TextColor;
            yield return null;

            //yield return 1.0f;

            //if responses exist, don't let player close text box
            if ((i == dialogueObject.DialogueLines.Length - 1) && (dialogueObject.HasResponses))
            {
                //Handle Dialogue Events if they exist
                if ((dialogueEvents != null) && (dialogueEvents.Length > 0) && (i < dialogueEvents.Length))
                {
                    dialogueEvents[i].AfterTextSpoken?.Invoke();
                }
                break;
            }

            //Wait for input to show next slide
            yield return null;
            yield return new WaitUntil(() => InputHandler.Instance.Interact_Shoot.Down || InputHandler.Instance.SkipText.Holding);

            //Handle Dialogue Events if they exist
            if ((dialogueEvents != null) && (dialogueEvents.Length > 0) && (i < dialogueEvents.Length))
            {
                dialogueEvents[i].AfterTextSpoken?.Invoke();
            }
        }

        if (dialogueObject.HasResponses)
        {
            responseHandler.ShowResponses(dialogueObject.Responses);
        }
        else
        {
            if (dialogueObject.NextDialogueObject != null)
            {
                ShowDialogue(dialogueObject.NextDialogueObject, null);
            }
            else
            {
                CloseDialogueBox();
            }
        }
    }

    public void CloseDialogueBox()
    {
        currDialogueObject = null;

        anim.SetTrigger("FadeOut");

        textLabel.text = string.Empty;

        isOpen = false;
    }

    public void DisableDialogueBox()
    {
        dialogueParent.SetActive(false);

        //This shouldn't be needed, but Unity be Unity sometimes...
        currDialogueObject = null;
        textLabel.text = string.Empty;

        isOpen = false;
    }
}
