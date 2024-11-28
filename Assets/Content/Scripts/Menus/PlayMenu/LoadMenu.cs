using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;
using System.Collections;
using System.Diagnostics;

public class LoadMenu : MonoBehaviour
{
    [SerializeField] private GameData gameData;

    [Header("Rooms")]
    [SerializeField] private SingleMenu singleMenu;             // index = 0
    [SerializeField] private LocalMultiRoom localMultiRoom;     // index = 1
    [SerializeField] private LocalPassMenu localPassRoom;       // index = 2
    [SerializeField] private OnlineCreateMenu onlineCreateRoom; // index = 3
    //[SerializeField] private OnlineJoinMenu onlineJoinRoom;     // index = 4
    // FIXME: Eliminar onlineJoinRoom

    [Header("Create Room")]
    [SerializeField] private PlayMenu playMenu;
    [SerializeField] private CreateLocalMenu createLocalRoom;
    [SerializeField] private CreateOnlineMenu createOnlineRoom;

    private int indexRoom;
    private int slotData;

    public void ExisPanel()
    {
        gameObject.SetActive(false);
    }

    public void ShowPanel(int index)
    {
        indexRoom = index;
        switch (index)
        {
            case 0:
                slotData = 1;
                break;
            case 1:
            case 2:
                slotData = 2;
                break;
            case 3:
            //case 4:
                slotData = 3;
                break;
            default:
                slotData = 0;
                break;
        }
        bool exists = SaveSystem.CheckSaveFile(slotData);
        gameObject.SetActive(exists);
        if (!exists) NewGameData();
    }

    public void NewGameData()
    {
        gameData.ClearGameData();
        LoadRoom(indexRoom);
    }

    public void LoadGameData()
    {
        gameData.ClearGameData();
        StartCoroutine(LoadData());
        LoadRoom(indexRoom);
    }

    private IEnumerator LoadData()
    {
        gameData.ClearGameData();
        yield return SaveSystem.LoadGame(gameData, slotData);
    }

    private void LoadRoom(int index)
    {
        switch (index)
        {
            case 0:
                singleMenu.gameObject.SetActive(true);
                singleMenu.ShowPanel(true);
                playMenu.ShowPanel(false);
                break;
            case 1:
                localMultiRoom.gameObject.SetActive(true);
                localMultiRoom.ShowPanel(true);
                createLocalRoom.gameObject.SetActive(false);
                break;
            case 2:
                localPassRoom.gameObject.SetActive(true);
                localPassRoom.ShowPanel(true);
                createLocalRoom.gameObject.SetActive(false);
                break;
            case 3:
                onlineCreateRoom.gameObject.SetActive(true);
                onlineCreateRoom.ShowPanel(true);
                createOnlineRoom.ShowPanel(false);
                break;
            // case 4:
            //     onlineJoinRoom.gameObject.SetActive(true);
            //     onlineJoinRoom.ShowPanel(true);
            //     createOnlineRoom.ShowPanel(false);
            //     break;
            default:
                break;
        }
        gameObject.SetActive(false);
    }
}
