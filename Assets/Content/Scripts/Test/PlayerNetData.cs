using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerNetData : NetworkBehaviour
{
    private readonly SyncVar<string> uid = new SyncVar<string>("");
    private readonly SyncVar<int> index = new SyncVar<int>(0);
    private readonly SyncVar<string> nickName = new SyncVar<string>("Jugador");
    private readonly SyncVar<int> points = new SyncVar<int>(0);
    protected readonly SyncVar<int> position = new SyncVar<int>(0);
    private readonly SyncVar<int> characterID = new SyncVar<int>(0);

    // Player Finances
    private readonly SyncVar<int> money = new SyncVar<int>(10000);
    private readonly SyncVar<int> salary = new SyncVar<int>(0);
    private readonly SyncVar<int> invest = new SyncVar<int>(0);
    private readonly SyncVar<int> debt = new SyncVar<int>(0);
    private readonly SyncList<Investment> investments = new SyncList<Investment>();
    private readonly SyncList<Expense> expenses = new SyncList<Expense>();

    // Player Finances Turn
    private readonly SyncVar<int> income = new SyncVar<int>(0);
    private readonly SyncVar<int> expense = new SyncVar<int>(0);

    public string UID { get => uid.Value; }
    public int Index { get => index.Value; }
    public string Nickname { get => nickName.Value; }
    public int Position { get => position.Value; }
    public int Points { get => points.Value; }
    public int Money { get => money.Value; }
    public int Salary { get => salary.Value; }
    public int Invest { get => invest.Value; }
    public int Debt { get => debt.Value; }
    public int Income { get => income.Value; }
    public int Expense { get => expense.Value; }


    public void Initialize(int i, string name, int model, string id)
    {
        index.Value = i;
        nickName.Value = name;
        characterID.Value = model;
        uid.Value = id;

        //FIXME: Copiar data de player
    }


}
