using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBannerNetwork : NetworkBehaviour
{
    public readonly SyncVar<Vector2> position = new SyncVar<Vector2>();
    private readonly SyncVar<string> uid = new SyncVar<string>();
    private readonly SyncVar<string> username = new SyncVar<string>();
    private readonly SyncVar<int> character = new SyncVar<int>(0);

    [Header("Data Player")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private RawImage imageCharacter;
    [SerializeField] private TextMeshProUGUI connectedText;

    [Header("Game Data")]
    [SerializeField] private GameData data;
    [SerializeField] private CharactersDatabase characterDB;

    #region Methods Getters & Setters

    public string UID { get => uid.Value; }
    public string Username { get => username.Value; }
    public int Character { get => character.Value; }

    #endregion

    void Awake()
    {
        username.OnChange += OnChangeUsername;
        character.OnChange += OnChangeCharacter;
        position.OnChange += OnChangePosition;
        imageCharacter.texture = characterDB.GetCharacter(0).characterIcon;
    }

    #region Methods Profile Player 

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner) return;

        // Cargar el perfil del usuario
        ProfileUser.LoadProfile();
        string uid = ProfileUser.UID;
        string name = ProfileUser.Username;

        // Verificar si es una partida Cargada o Nueva
        if (data.DataExists())
        {
            bool playerExists = false;
            foreach (PlayerData player in data.playersData)
            {
                if (player.UID == uid)
                {
                    playerExists = true;
                    SetProfilePlayer(uid, name); 
                    break;
                }
            }

            // Usuario no existe en la partida
            if (!playerExists) NetworkManager.ClientManager.StopConnection();
        }
        else
        {
            SetProfilePlayer(uid, name);
        }
    }

    [ServerRpc]
    private void SetProfilePlayer(string uidProfile, string nameProfile)
    {
        uid.Value = uidProfile;
        username.Value = nameProfile;
    }

    #endregion

    #region Methods OnChange Values

    private void OnChangeUsername(string oldName, string newName, bool asServer)
    {
        nameInput.text = newName;
        connectedText.text = "Conectado";
        connectedText.color = Color.green;
    }

    private void OnChangeCharacter(int oldCharacter, int newCharacter, bool asServer)
    {
        imageCharacter.texture = characterDB.GetCharacter(newCharacter).characterIcon;
    }

    private void OnChangePosition(Vector2 oldPosition, Vector2 newPosition, bool asServer)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = newPosition;
    }

    #endregion

    #region Methods Change Values Client

    public void NextCharacter()
    {
        if (!IsOwner) return;
        int nextCharacter = (character.Value + 1) % characterDB.Length;
        CmdUpdateCharacter(nextCharacter);
    }

    public void PreviousCharacter()
    {
        if (!IsOwner) return;
        int previousCharacter = (character.Value - 1 + characterDB.Length) % characterDB.Length;
        CmdUpdateCharacter(previousCharacter);
    }

    #endregion


    #region Methods Change Values Server

    [ServerRpc]
    private void CmdUpdateCharacter(int newCharacter) => character.Value = newCharacter;

    #endregion

}