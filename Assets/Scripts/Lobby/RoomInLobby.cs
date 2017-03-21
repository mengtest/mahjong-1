using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.Lobby
{
    public class RoomInLobby : MonoBehaviour
    {

        public Text t;

        public Button btnJoin;

        private void Start()
        {
            btnJoin.onClick.AddListener(delegate { JoinRoom(); });
        }

        void JoinRoom()
        {
            PhotonNetwork.JoinRoom(t.text);
        }
    }
}