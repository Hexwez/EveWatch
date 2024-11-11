using EveWatch;
using GorillaGameModes;
using GorillaNetworking;
using MonkeNotificationLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Watch.Mods
{
    internal class Other
    {
        #region JoinLast
        public static string LastRoom = "";

        public static void JoinLast()
        {
            if (!PhotonNetwork.InRoom)
            {
                if (LastRoom == "")
                {
                    NotificationController.AppendMessage("EveWatch", "No Last Room To Join.");
                }
                else
                {
                    NotificationController.AppendMessage("EveWatch", $"Joining Last Room: {LastRoom}");
                    PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(LastRoom, JoinType.Solo);
                }
            }
            else
            {
                NotificationController.AppendMessage("EveWatch", "Already In A Room.");
            }
        }
        #endregion

        #region Rejoin

        public static void Rejoin()
        {
            if (PhotonNetwork.InRoom)
            {
                NetworkSystem.Instance.ReturnToSinglePlayer();
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(LastRoom, JoinType.Solo);
            }
            else
            {
                NotificationController.AppendMessage("EveWatch", "Not in a room.");
            }
        }
        #endregion
    }
}
