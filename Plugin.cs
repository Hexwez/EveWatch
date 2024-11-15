﻿using BepInEx;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using EveWatch.Mods;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Net;
using TMPro;
using ExitGames.Client.Photon;
using MonkeNotificationLib;
using EveWatch.Librarys;
using Watch.Mods;

namespace EveWatch
{
    [BepInPlugin("Eve.EveWatch", "EveWatch", "0.1.9")]
    public class Main : BaseUnityPlugin
    {
        static int counter;
        static float PageCoolDown;
        int modCount;
        public static Dictionary<Mod, bool> Mods;

        bool doneDeletion;

        public static GorillaHuntComputer huntComputer;
        Text huntText;
        bool lookedAtMainPage;
        bool lastY;
        bool hideAndLock;

        public static GameObject button;

        void Start()
        {
            Movement.SwitchBoostType(true);
            Infection.SwitchTagType(true);
            Mods = new Dictionary<Mod, bool>()
            {
                // {new Mod(TITLE, DESCRIPTION, ON ENABLED, UPDATED ENABLED (gets called constantly while the mod is enabled) ON DISABLE, toggleable (optional, if true, it will instantly disable itself), ENABLED (if its enabled}
                //Title
                { new Mod("Eve Watch!", "Welcome to\nEveWatch! Look at\nthe CoC board\nfor the controls!", Empty, Empty, Empty, true), false},

                //Room
                { new Mod("Disconnect","Makes you leave\nthe lobby!", ()=>NetworkSystem.Instance.ReturnToSinglePlayer(), Empty, Empty, true), false },
                //{ new Mod("Rejoin","Makes you rejoin\nthe lobby!", Other.Rejoin, Empty, Empty, true), false },
                { new Mod("Join Last","Makes you join\nthe last lobby!", Other.JoinLast, Empty, Empty, true), false },

                //Movement
                { new Mod("Platforms","Press grip to\nuse them!", Empty, Movement.Platforms, Movement.OnPlatformDisable), false },
                { new Mod("Speed Boost", $"Type: {Movement.CurrentSpeedName}\nGives you a\nlittle boost\nin speed!", Empty, Movement.SpeedBoost, Movement.DisableSpeedBoost), false },
                { new Mod("No Freeze", "Just no tag\nfreeze pretty easy\nto understand.", Empty, ()=>GorillaLocomotion.Player.Instance.disableMovement = false, Empty), false },

                //Infection
                { new Mod("Tag Aura", $"Type: {Infection.CurrentTagAuraName}\nLets you tag\npeople easier!", Empty, ()=>Infection.TagAura(), Empty), false },

                //Flights
                { new Mod("Flight", "Press A to\nfly!", Empty, Movement.Fly, Empty), false },
                { new Mod("Iron Monk", "Press grip to\nfly like iron\nman!", Empty, Movement.IronMonk, Empty), false },

                //Visual
                { new Mod("Mod List", "Shows all your\nenabled mods!", () => Visual.modListEnabled = true, Empty, () => Visual.modListEnabled = false), true },
                { new Mod("Tracers", "Tracers to every\nmonke!\nGreen = Untagged\nRed = Tagged", Visual.Tracers, Empty, Visual.DisableTracers), false },
                { new Mod("Box ESP", "Boxes around every\nmonke!\nGreen = Untagged\nRed = Tagged", Visual.BoxESP, Empty, Visual.DisableBoxESP), false },
                { new Mod("Watch ESP", "Boxes around every\nEvewatch user!", Visual.WatchESP, Empty, Visual.DisableWatchESP), false },
                { new Mod("Skell ESP", "Turns everyone\ninto skels\nthat you can see\nthrough\nwalls!", Visual.SkellESP, Empty, Visual.DisableSkellESP), false },
                { new Mod("Aura Rad", "Shows the radius\nof the tag\naura!", Visual.TagAuraRad, Empty, Visual.TagAuraRadDisable), false },

                //Guns
                { new Mod("Tp Gun", "Teleport around\nwith a gun!", Empty, Guns.TpGun, Empty), false },

                //Settings
                { new Mod("Swap Speed", $"Changes your speed\nboost, boost.\nType: {Movement.CurrentSpeedName}", ()=>Movement.SwitchBoostType(), Empty, Empty, true), false },
                { new Mod("Tag Dist", $"Distance: {Infection.CurrentTagAuraName}\nChanges your\nTag Aura Distance", ()=>Infection.SwitchTagType(), Empty, Empty, true), false },
            };
            modCount = Mods.Count - 1;

            GorillaTagger.OnPlayerSpawned(delegate
            {
                huntComputer = GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>();
                huntText = huntComputer.text;
                huntText.transform.localPosition = new Vector3(0.023f, 0.0004f, 0);
                huntText.transform.localScale = new Vector3(0.0006f, 0.0006f, 0.0006f);
                huntText.rectTransform.sizeDelta = new Vector2(160f, 60f);
                huntComputer.enabled = false;


                Material huntComputerMat = new Material(Main.huntComputer.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial);
                Main.huntComputer.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = huntComputerMat;

                Material mat = new Material(huntComputer.material.material);
                huntComputer.material.material = mat;
                huntComputer.material.transform.localPosition = new Vector3(0.0197f, -0.0096f, 0);

                foreach (Transform obj in huntComputer.material.transform.parent)
                {
                    if (obj.name != "Text" && obj.name != "Material")
                    {
                        if (obj.GetComponent<Image>() != null) GameObject.Destroy(obj.GetComponent<Image>());
                        GameObject.Destroy(obj.gameObject);
                    }
                }
                button = GameObject.CreatePrimitive(PrimitiveType.Cube);
                button.transform.SetParent(huntComputer.material.transform, false);
                button.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                button.AddComponent<Librarys.Button>();
                button.transform.localScale = new Vector3(12, 12, 10);
                button.layer = 18;
                button.GetComponent<BoxCollider>().isTrigger = true;
                Destroy(button.GetComponent<Renderer>());
                Destroy(button.GetComponent<MeshFilter>());

                GameObject title = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConduct");
                title.GetComponent<TextMeshPro>().richText = true;
                title.GetComponent<TextMeshPro>().text = "<color=#FF0000>E</color><color=#FFAA00>V</color><color=#AAFF00>E</color><color=#00FFAA>W</color><color=#00A9FF>A</color><color=#0000FF>T</color><color=#AA00FF>C</color><color=#FF00AA>H</color>";

                GameObject desc = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COC Text");
                desc.GetComponent<TextMeshPro>().richText = true;
                desc.GetComponent<TextMeshPro>().text = new WebClient().DownloadString("https://pastebin.com/raw/DP3nsjBg").ToUpper();

                GameObject holder = new GameObject("EveWatch");
                holder.AddComponent<Visual>();
                holder.AddComponent<Callbacks>();

                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
                {
                    {"EveWatch", true}
                });

                Debug.Log("EveWatch Has Loaded Successfully");
                new NotificationManager();
                NotificationController.AppendMessage("Evewatch", "EveWatch Has Loaded");
                doneDeletion = true;
            });
        }
        void Update()
        {
            if (!doneDeletion) return;

            if ((ControllerInputPoller.instance.leftControllerSecondaryButton && !lastY) || Keyboard.current.hKey.wasPressedThisFrame) hideAndLock = !hideAndLock;

            if (!hideAndLock)
            {
                huntComputer.gameObject.SetActive(true);
                if ((ControllerInputPoller.instance.leftControllerIndexFloat >= .5f || Keyboard.current.rightArrowKey.isPressed) && Time.time > PageCoolDown + 0.25)
                {
                    PageCoolDown = Time.time;
                    counter++;
                    lookedAtMainPage = true;
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, true, 1f);
                }
                if ((ControllerInputPoller.instance.leftControllerGripFloat >= .5f || Keyboard.current.leftArrowKey.isPressed) && Time.time > PageCoolDown + 0.25)
                {
                    PageCoolDown = Time.time;
                    counter--;
                    lookedAtMainPage = true;
                    GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, true, 1f);
                }
                if (counter < (lookedAtMainPage ? 1 : 0)) counter = modCount;
                if (counter > modCount) counter = (lookedAtMainPage ? 1 : 0);

                if (counter != 0)
                {
                    huntText.text = $"{Mods.ElementAt(counter).Key.Name} ({counter}/{modCount})\n{Mods.ElementAt(counter).Key.Desc}".ToUpper();
                    if ((ControllerInputPoller.instance.leftControllerPrimaryButton || Keyboard.current.enterKey.isPressed) && Time.time > PageCoolDown + .5)
                    {
                        Toggle();
                    }
                }
                else huntText.text = Mods.ElementAt(counter).Key.Name + "\n" + Mods.ElementAt(counter).Key.Desc;

                if (counter == 0) huntComputer.material.enabled = false;
                else huntComputer.material.enabled = true;

                if (Mods.ElementAt(counter).Value) huntComputer.material.material.color = Color.green;
                else huntComputer.material.material.color = new Color(1, 0, 0, 255);
            }
            else
            {
                huntComputer.gameObject.SetActive(false);
            }

            foreach (var modInfo in Mods)
            {
                if (modInfo.Value == true)
                {
                    modInfo.Key.StayEnabledMethod();
                }
            }

            lastY = ControllerInputPoller.instance.leftControllerSecondaryButton;
        }
        void Empty() { } //Used for the mods and if you want them to be empty!

        public static void Toggle()
        {
            PageCoolDown = Time.time;
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(66, true, 1f);
            var Mod = Mods.ElementAt(counter);
            if (Mod.Key.Toggle)
            {
                Visual.RestartText();
                Mod.Key.OnEnabledMethod();
                return;
            }
            Mods[Mod.Key] = !Mod.Value;
            NotificationController.AppendMessage("EveWatch", $"{(Mod.Value ? "Disabled" : "Enabled")}, {Mod.Key.Name}".WrapColor(Mod.Value ? "red" : "green"));
            if (Mods[Mod.Key]) Mod.Key.OnEnabledMethod();
            else Mod.Key.OnDisabledMethod();
            Visual.RestartText();
        }

        void OnGUI()
        {
            if (!hideAndLock)
            {
                GUIStyle style = new GUIStyle();
                style.font = huntText.font;
                GUI.Label(new Rect(0, 0, 200000000, 20000000), huntText.text, style);
            }
        }

        public static Mod GetMod(string name)
        {
            foreach (Mod mod in Mods.Keys)
            {
                if (mod.Name == name) return mod;
            }
            return null;
        }
    }

    public class Mod
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public Action OnEnabledMethod { get; set; }
        public Action StayEnabledMethod { get; set; }
        public Action OnDisabledMethod { get; set; }

        public bool Toggle { get; set; }

        public Mod(string name, string desc, Action onEnabled, Action stayEnabled, Action onDisabled, bool toggle = false)
        {
            Name = name;
            Desc = desc;
            OnEnabledMethod = onEnabled;
            StayEnabledMethod = stayEnabled;
            OnDisabledMethod = onDisabled;
            Toggle = toggle;
        }
    }
}
