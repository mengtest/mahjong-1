using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace com.Lobby
{
    public class Launcher : Photon.PunBehaviour
    {
        #region PUBLIC

        //客户端版本
        public string _gameVersion = "1.0";

        //玩家名字
        public InputField nameField;

        //房间列表
        public RectTransform LobbyPanel;

        #endregion

        #region PRIVATE 

        private bool isConnecting;
        #endregion

        private void Awake()
        {
            //#不重要
            //强制Log等级为全部
            PhotonNetwork.logLevel = PhotonLogLevel.Full;

            //#关键
            //我们不加入大厅 这里不需要得到房间列表所以不用加入大厅去
            PhotonNetwork.autoJoinLobby = true;

            //#关键
            //这里保证所有主机上调用 PhotonNetwork.LoadLevel() 的时候主机和客户端能同时进入新的场景
            PhotonNetwork.automaticallySyncScene = true;
        }

        // Use this for initialization
        void Start()
        {
            Connect();

            SetPlayerName();
        }

        private void Connect()
        {
            isConnecting = true;

            //已經連接上了服務器
            if (PhotonNetwork.connected)
            {
                Debug.Log("Connected");
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings(_gameVersion);
            }
        }

        private void OnFailedToConnect(NetworkConnectionError error)
        {
            Debug.Log("fail to Connect");
        }

        public void CreateARoom()
        {
            if (PhotonNetwork.connected)
            {
                //创建房间成功
                if (PhotonNetwork.CreateRoom(nameField.text, new RoomOptions { MaxPlayers = 4 }, null))
                {
                    Debug.Log("Launcher.CreateARoom 成功");
                    PhotonNetwork.LoadLevel("Room");
                }
            }
        }

        public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            Debug.Log("Launcher Create Room faileds");
        }

        public void SetPlayerName()
        {
            if (string.IsNullOrEmpty(nameField.text))
            {
                if (PlayerPrefs.HasKey("PlayerName"))
                {
                    nameField.text = PlayerPrefs.GetString("PlayerName");
                }
            }

            PhotonNetwork.playerName = nameField.text;
            PlayerPrefs.SetString("PlayerName", nameField.text);
        }

        public override void OnReceivedRoomListUpdate()
        {

            Debug.Log("OnReceivedRoomListUpdate");

            RoomInLobby[] ts = LobbyPanel.GetComponentsInChildren<RoomInLobby>();
            foreach (RoomInLobby t in ts)
            {
                Destroy(t.gameObject);
            }

            RoomInfo[] rooms = PhotonNetwork.GetRoomList();
            foreach (RoomInfo room in rooms)
            {
                GameObject g = GameObject.Instantiate(Resources.Load("Lobby/RoomItem") as GameObject);
                Text t = g.transform.Find("Text").GetComponent<Text>();
                t.text = room.Name;
                g.name = room.Name;
                g.transform.SetParent(LobbyPanel);
                g.transform.localScale = Vector3.one;
            }
        }
    }
}