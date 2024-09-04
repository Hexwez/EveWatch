using BepInEx;
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

namespace EveWatch
{
    [BepInPlugin("Eve.EveWatch", "EveWatch", "1.4.0")]
    public class Main : BaseUnityPlugin
    {
        static int counter;
        static float PageCoolDown;
        int modCount;
        static Dictionary<Mod, bool> Mods;

        bool doneDeletion;
        void Start()
        {
            Mods = new Dictionary<Mod, bool>()
            {
                { new Mod("Eve Watch!", "Welcome to\nEveWatch! Look at\nthe CoC board\nfor the controls!", Empty, Empty, Empty), false },
                { new Mod("Disconnect","Makes you leave\nthe lobby!", ()=>NetworkSystem.Instance.ReturnToSinglePlayer(), Empty, Empty), false },
                { new Mod("Swap Theme","Changes the menus\ntheme!", Themes.SwitchTheme, Empty, Empty), false },
                { new Mod("Platforms ","Press grip to\nuse them!", Empty, Movement.Platforms, Movement.OnPlatformDisable), false },
                { new Mod("Frozone   ", "Press grip to\nspawn slip plats!", Empty, Movement.Frozone, Empty), false },
                { new Mod("Noclip    ", "Disables every\ncollider!\n(Plats suggested)", Movement.Noclip, Empty, Movement.NoclipDisable), false },
                { new Mod("Flight    ", "Press A to\nfly!", Empty, Movement.Fly, Empty), false },
                { new Mod("Iron Monk ", "Press grip to\nfly like iron\nman!", Empty, Movement.IronMonk, Empty), false }
            };
            modCount = Mods.Count - 1;
        }
        public static GorillaHuntComputer huntComputer;
        Text huntText;
        bool lookedAtMainPage;
        bool lastY;
        bool hideAndLock;
        void Update()
        {
            huntComputer = GorillaTagger.Instance.offlineVRRig.huntComputer.GetComponent<GorillaHuntComputer>();
            if (InModded() && GorillaTagger.Instance.offlineVRRig != null)
            {
                if (!doneDeletion)
                {
                    huntText = huntComputer.text;
                    huntText.transform.localPosition = new Vector3(0.023f, 0.0004f, 0);
                    huntText.transform.localScale = new Vector3(0.0006f, 0.0006f, 0.0006f);
                    huntText.rectTransform.sizeDelta = new Vector2(160f, 60f);
                    huntComputer.enabled = false;
                    Destroy(huntComputer.badge);
                    Destroy(huntComputer.leftHand);
                    Destroy(huntComputer.rightHand);
                    Destroy(huntComputer.hat);
                    Destroy(huntComputer.face);
                    Material mat = new Material(huntComputer.material.material);
                    huntComputer.material.material = mat;
                    huntComputer.material.transform.localPosition = new Vector3(0.0197f, -0.0096f, 0);
                    foreach(Transform obj in huntComputer.material.transform.parent)
                    {
                        if (obj.name != "Text" && obj.name != "Material") GameObject.Destroy(obj.gameObject);
                    }
                    
                    GameObject title = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/CodeOfConduct_Group/CodeOfConduct");
                    title.GetComponent<TextMeshPro>().richText = true;
                    title.GetComponent<TextMeshPro>().text = "<color=#FF0000>E</color><color=#FFAA00>V</color><color=#AAFF00>E</color> <color=#00FFAA>W</color><color=#00A9FF>A</color><color=#0000FF>T</color><color=#AA00FF>C</color><color=#FF00AA>H</color>";
                    title.GetComponent<TextMeshPro>().font = TMP_FontAsset.CreateFontAsset(huntText.font);

                    GameObject desc = title.transform.GetChild(0).gameObject;
                    desc.GetComponent<TextMeshPro>().richText = true;
                    desc.GetComponent<TextMeshPro>().text = new WebClient().DownloadString("https://pastebin.com/raw/wErPZy4f").ToUpper();
                    desc.GetComponent<TextMeshPro>().font = TMP_FontAsset.CreateFontAsset(huntText.font);
                    
                    Debug.Log("EveWatch Has Loaded Successfully");
                    doneDeletion = true;
                }

                if ((ControllerInputPoller.instance.leftControllerSecondaryButton && !lastY) || Keyboard.current.hKey.wasPressedThisFrame) hideAndLock = !hideAndLock;

                if (!hideAndLock)
                {
                    huntComputer.gameObject.SetActive(true);
                    if ((ControllerInputPoller.instance.leftControllerIndexFloat >= .5f || Keyboard.current.rightArrowKey.isPressed) && Time.time > PageCoolDown + 0.5)
                    {
                        PageCoolDown = Time.time;
                        counter++;
                        lookedAtMainPage = true;
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, true, 1f);
                    }
                    if ((ControllerInputPoller.instance.leftControllerGripFloat >= .5f || Keyboard.current.leftArrowKey.isPressed) && Time.time > PageCoolDown + 0.5)
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
                            PageCoolDown = Time.time;
                            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(66, true, 1f);
                            if (counter == 1 || counter == 2)
                            {
                                Mods.ElementAt(counter).Key.OnEnabledMethod();
                                return;
                            }
                            Mods[Mods.ElementAt(counter).Key] = !Mods.ElementAt(counter).Value;
                            if (Mods[Mods.ElementAt(counter).Key]) Mods.ElementAt(counter).Key.OnEnabledMethod();
                            else Mods.ElementAt(counter).Key.OnDisabledMethod();
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
            }else huntComputer.gameObject.SetActive(false);
        }
        void Empty(){} //Used for the mods and if you want them to be empty!

        void OnGUI()
        {
            if (!hideAndLock)
            {
                GUIStyle style = new GUIStyle();
                style.font = huntText.font;
                GUI.Label(new Rect(0, 0, 200000000, 20000000), huntText.text, style);
            }
        }

        bool InModded()
        {
            //if (PhotonNetwork.InRoom)
            //{
            //    return PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().Contains("MODDED");
            //}
            //return false;
            return true;
        }
    }

    public class Mod
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public Action OnEnabledMethod { get; set; }
        public Action StayEnabledMethod { get; set; }
        public Action OnDisabledMethod { get; set; }

        public Mod(string name, string desc, Action onEnabled, Action stayEnabled, Action onDisabled)
        {
            Name = name;
            Desc = desc;
            OnEnabledMethod = onEnabled;
            StayEnabledMethod = stayEnabled;
            OnDisabledMethod = onDisabled;
        }
    }
}