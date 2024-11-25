using Mirror;
using Mirror.Examples.Basic;

public class PlayerNetData : NetworkBehaviour
{
    // User Data
    [SyncVar] private string uid;
    [SyncVar] private string nickName;

    // Game Data
    [SyncVar] private int characterID;
    [SyncVar] private int position;
    [SyncVar(hook = nameof(OnChangePoints))] private int points;

    // Finances
    [SyncVar] private int money;
    [SyncVar] private int salary;
    [SyncVar] private int invest;
    [SyncVar] private int debt;
    private readonly SyncList<PlayerInvestment> investments = new SyncList<PlayerInvestment>();
    private readonly SyncList<PlayerExpense> expenses = new SyncList<PlayerExpense>();

    // Finances Turn
    private int income;
    private int expense;

    #region Getters

    public string UID { get => uid; set => uid = value; }
    public string Nickname { get => nickName; }

    public int CharacterID { get => characterID; }
    public int Position { get => position; }
    public int Points { get => points; }

    public int Money { get => money; }
    public int Salary { get => salary; }
    public int Invest { get => invest; }
    public int Debt { get => debt; }

    public int Income { get => income; }
    public int Expense { get => expense; }

    #endregion

    public void Initialize(string uid, string nickName, int characterID)
    {
        this.uid = uid;
        this.nickName = nickName;
        this.characterID = characterID;

        position = 0;
        points = 0;
        money = 10000;
        salary = 0;
        invest = 0;
        debt = 0;
        income = 0;
        expense = 0;
    }

    #region Server Change Values 

    [Server]
    public void AddPoints(int points)
    {
        this.points += points;
    }

    #endregion

    #region OnChange Values

    private void OnChangePoints(int oldPoints, int newPoints)
    {
        GameUINetManager.UpdatePoints(uid, newPoints);
    }

    #endregion



}
