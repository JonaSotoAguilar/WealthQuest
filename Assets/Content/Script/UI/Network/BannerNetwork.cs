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
        if (!isOwned) return;

        string uid = ProfileUser.uid;
        string name = ProfileUser.username;

        // Verificar si es una partida Cargada o Nueva
        if (data.DataExists())
        {
            foreach (PlayerData player in data.playersData)
            {
                if (player.UID == uid)
                {
                    CmdSetProfilePlayer(uid, name, data.GetPlayerData(uid).CharacterID);
                    return;
                }
            }

            // Si no se encuentra el usuario en la partida, se desconecta
            WQRelayManager.Instance.StopClient();
        }
        else
        {
            CmdSetProfilePlayer(uid, name, 0);
        }
    }

    [Command]
    private void CmdSetProfilePlayer(string uidProfile, string nameProfile, int charID)
    {
        uid = uidProfile;
        username = nameProfile;
        character = charID;
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

    #endregion

    #region Server Change Values

    [Command]
    private void CmdUpdateCharacter(int newCharacter) => character = newCharacter;

    #endregion

    #region OnChange Values

    private void OnChangeUsername(string oldName, string newName)
    {
        nameInput.text = newName;
        connectedText.text = "Conectado";
        connectedText.color = Color.green;
    }

    private void OnChangeCharacter(int oldCharacter, int newCharacter)
    {
        characterSprite.sprite = characterDB.GetCharacter(newCharacter).characterIcon;
    }

    // FIXME: Eliminar
    private void OnChangePosition(Vector2 oldPosition, Vector2 newPosition)
    {
        RectTransform rectTransform = panelPlayer.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = newPosition;
    }

    #endregion

}
