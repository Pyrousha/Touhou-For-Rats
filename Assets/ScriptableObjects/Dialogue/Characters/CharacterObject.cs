using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/CharacterObject")]
public class CharacterObject : ScriptableObject
{
    [SerializeField] private string characterName;
    [SerializeField] private Sprite defaultPortraitSprite;
    [SerializeField] private AudioClip defaultVoice;

    public string CharacterName => characterName;
    public Sprite DefaultPortraitSprite => defaultPortraitSprite;
    public AudioClip DefaultVoice => defaultVoice;
    [field: SerializeField] public bool OnRight { get; private set; }
}
