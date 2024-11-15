﻿using EveWatch.Librarys;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EveWatch.Mods
{
    public class Lucy
    {
        static bool lastTriggedButton;
        public static void LucyGun(bool summoned, bool chase)
        {
            GunLib.GunLibData gunLibData = GunLib.ShootLock();

            if (gunLibData.isTriggered && gunLibData.isShooting && !lastTriggedButton && gunLibData.isLocked)
            {
                LucyShoot(chase, summoned, gunLibData.lockedPlayer.Creator);
            }
            lastTriggedButton = gunLibData.isTriggered;
        }

        static void LucyShoot(bool chase, bool summonded, NetPlayer player)
        {
            HalloweenGhostChaser lucy = GameObject.FindFirstObjectByType<HalloweenGhostChaser>(); 

            if (lucy.IsMine)
            {
                lucy.grabTime = Time.time;
                lucy.isSummoned = summonded;
                lucy.currentState = chase ? HalloweenGhostChaser.ChaseState.Chasing : HalloweenGhostChaser.ChaseState.Grabbing;
                lucy.targetPlayer = player;
            }
        }
    }
}
