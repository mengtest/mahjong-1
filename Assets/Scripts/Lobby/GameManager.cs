using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace com.Room
{
    public class GameManager : Photon.PunBehaviour
    {
         
        public RectTransform playersPanel;
        public Button btnExit;
        public Button btnStart;

        void Start()
        {
            btnExit.onClick.AddListener(delegate { ExitGame(); });
            btnStart.onClick.AddListener(delegate { StartGame(); });

            StartCoroutine(Refrech());
        }

        void ExitGame()
        {
            PhotonNetwork.LeaveRoom();
        }

        void StartGame()
        {
            if (!PhotonNetwork.isMasterClient)
            {
                return;
            }

            if (PhotonNetwork.playerList.Length == 4)
            {
                PhotonNetwork.LoadLevel("Desktop");
            }
        }

        IEnumerator Refrech()
        {
            yield return new WaitForSeconds(2f);

            yield return Refrech();
        }

        public override void OnJoinedRoom()
        {
            Text[] ts = playersPanel.GetComponentsInChildren<Text>();
            foreach (Text t in ts)
            {
                Destroy(t.gameObject.transform.parent.gameObject);
            }
            PhotonPlayer[] players = PhotonNetwork.playerList;
            foreach (PhotonPlayer player in players)
            {
                GameObject g = GameObject.Instantiate(Resources.Load("Lobby/PlayerItem") as GameObject);
                Text t = g.transform.Find("Text").GetComponent<Text>();
                t.text = player.NickName;
                g.name = player.NickName;
                g.transform.SetParent(playersPanel);
                g.transform.localScale = Vector3.one;
            }
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            GameObject g = GameObject.Instantiate(Resources.Load("Lobby/PlayerItem") as GameObject);
            Text t = g.transform.Find("Text").GetComponent<Text>();
            t.text = newPlayer.NickName;
            g.name = newPlayer.NickName;
            g.transform.SetParent(playersPanel);
            g.transform.localScale = Vector3.one;

            
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            GameObject g = playersPanel.FindChild(otherPlayer.NickName).gameObject;
            Destroy(g);
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("Launcher");
        }

    }
}