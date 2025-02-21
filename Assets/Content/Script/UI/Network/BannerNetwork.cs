using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BannerNetwork : NetworkBehaviour
{
    [SyncVar] private string uid = "0";
    [SyncVar(hook = nameof(OnChangePosition))] public Vector2 position = Vector2.zero;
    [SyncVar(hook = nameof(OnChangeUsername))] private string username = "Jugador_1";
    [SyncVar(hook = nameof(OnChangeCharacter))] private int character = 0;
    [SyncVar(hook = nameof(OnChangeStatus))] private string status = "Conectado";

    [Header("Panel Player")]
    [SerializeField] private GameObject panelPlayer;

    [Header("Data Player")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TextMeshProUGUI connectedText;

    [Header("Game Data")]
    [SerializeField] private GameData data;
    [SerializeField] private CharactersDatabase characterDB;

    [Header("Character")]
    [SerializeField] private Button nextCharacter;
    [SerializeField] private Button previousCharacter;
    [SerializeField] private Image characterSprite;

    # region Getters & Setters

    public string UID { get => uid; }
    public string Username { get => username; }
    public int Character { get => character; }

    #endregion

    #region Methods Profile Player 

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isOwned)
        {
            BlockCharacterButtons(false);
            return;
        }
        else BlockCharacterButtons(true);

        string uid = ProfileUser.uid;
        string name = ProfileUser.username;

        CmdSetProfilePlayer(uid, name);
    }

    [Command]
    private void CmdSetProfilePlayer(string uidProfile, string nameProfile)
    {
        uid = uidProfile;
        username = nameProfile;
        status = "Conectado";
    }

    #endregion

    #region Client Change Values 

    public void NextCharacter()
    {
        if (!isOwned) return;
        int nextCharacter = (character + 1) % characterDB.Length;
        CmdUpdateCharacter(nextCharacter);
    }

    public void PreviousCharacter()
    {
        if (!isOwned) return;
        int previousCharacter = (character - 1 + characterDB.Length) % characterDB.Length;
        CmdUpdateCharacter(previousCharacter);
    }

    [Server]
    public void ReadyPlayer(bool enable)
    {
        if (enable)
        {
            status = "Listo";
            RpcBlockCharacterButtons(false);
        }
        else
        {
            status = "Conectado";
            RpcBlockCharacterButtons(true);
        }
    }

    [ClientRpc]
    private void RpcBlockCharacterButtons(bool block)
    {
        if (!isOwned) return;
        BlockCharacterButtons(block);
    }

    private void BlockCharacterButtons(bool block)
    {
        nextCharacter.gameObject.SetActive(block);
        previousCharacter.gameObject.SetActive(block);
    }

    #endregion

    #region Server Change Values

    [Command]
    private void CmdUpdateCharacter(int newCharacter) => character = newCharacter;

    #endregion

    #region OnChange Values

    private void OnChangeUsername(string oldName, string newName)
    {
        nameInput.text = newName;
    }

    private void OnChangeCharacter(int oldCharacter, int newCharacter)
    {
        characterSprite.sprite = characterDB.GetCharacter(newCharacter).characterIcon;
    }

    private void OnChangePosition(Vector2 oldPosition, Vector2 newPosition)
    {
        RectTransform rectTransform = panelPlayer.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = newPosition;
    }

    private void OnChangeStatus(string oldStatus, string newStatus)
    {
        connectedText.text = newStatus;
    }

    #endregion

}
