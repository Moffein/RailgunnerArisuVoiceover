using RailgunnerArisuVoiceover.Components;
using RailgunnerArisuVoiceover.Modules;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.Audio;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Collections.Generic;
using BaseVoiceoverLib;

namespace RailgunnerArisuVoiceover
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Alicket.TendouArisTheRailgunner")]
    [BepInPlugin("com.Schale.RailgunnerArisuVoiceover", "RailgunnerArisVoiceover", "1.1.0")]
    public class RailgunnerArisuVoiceoverPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> enableVoicelines;
        public static bool playedSeasonalVoiceline = false;
        public static AssetBundle assetBundle;

        public void Awake()
        {
            new Content().Initialize();
            Files.PluginInfo = this.Info;
            RoR2.RoR2Application.onLoad += OnLoad;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RailgunnerArisuVoiceover.arisurailgunnervoiceoverbundle"))
            {
                assetBundle = AssetBundle.LoadFromStream(stream);
            }

            InitNSE();

            enableVoicelines = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Enable Voicelines"), true, new ConfigDescription("Enable voicelines when using the Aris Railgunner Skin."));
            enableVoicelines.SettingChanged += EnableVoicelines_SettingChanged;
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                RiskOfOptionsCompat();
            }
        }

        private void EnableVoicelines_SettingChanged(object sender, EventArgs e)
        {
            RefreshNSE();
        }

        private void Start()
        {
            SoundBanks.Init();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void RiskOfOptionsCompat()
        {
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(enableVoicelines));
            RiskOfOptions.ModSettingsManager.SetModIcon(assetBundle.LoadAsset<Sprite>("ClanChat_Emoji_14_Jp"));
        }

        private void OnLoad()
        {
            SkinDef arisuSkin = null;
            SkinDef[] banditSkins = SkinCatalog.FindSkinsForBody(BodyCatalog.FindBodyIndex("RailgunnerBody"));
            foreach (SkinDef skinDef in banditSkins)
            {
                if (skinDef.name == "ArisSkinDef")
                {
                    arisuSkin = skinDef;
                    break;
                }
            }

            if (!arisuSkin)
            {
                Debug.LogError("ArisuRailgunnerVoiceover: Aris Railgunner SkinDef not found. Voicelines will not work!");
            }
            else
            {
                VoiceoverInfo vo = new VoiceoverInfo(typeof(RailgunnerArisuVoiceoverComponent), arisuSkin, "RailgunnerBody");
                vo.selectActions += ArisuSelect;
            }
            RefreshNSE();
        }

        private void ArisuSelect(GameObject mannequinObject)
        {
            if (!enableVoicelines.Value) return;
            bool played = false;
            if (!playedSeasonalVoiceline)
            {
                if (System.DateTime.Today.Month == 1 && System.DateTime.Today.Day == 1)
                {
                    Util.PlaySound("Play_RailgunnerArisu_Lobby_Newyear", mannequinObject);
                    played = true;
                }
                else if (System.DateTime.Today.Month == 5 && System.DateTime.Today.Day == 8)
                {
                    Util.PlaySound("Play_RailgunnerArisu_Lobby_bday", mannequinObject);
                    played = true;
                }
                else if (System.DateTime.Today.Month == 10 && System.DateTime.Today.Day == 31)
                {
                    Util.PlaySound("Play_RailgunnerArisu_Lobby_Halloween", mannequinObject);
                    played = true;
                }
                else if (System.DateTime.Today.Month == 12 && System.DateTime.Today.Day == 25)
                {
                    Util.PlaySound("Play_RailgunnerArisu_Lobby_xmas", mannequinObject);
                    played = true;
                }

                if (played)
                {
                    playedSeasonalVoiceline = true;
                }
            }
            if (!played)
            {
                if (Util.CheckRoll(5f))
                {
                    Util.PlaySound("Play_RailgunnerArisu_TitleDrop", mannequinObject);
                }
                else
                {
                    Util.PlaySound("Play_RailgunnerArisu_Lobby", mannequinObject);
                }
            }
        }

        private void InitNSE()
        {
            RailgunnerArisuVoiceoverComponent.nseSpecial = RegisterNSE("Play_RailgunnerArisu_ExSkill");
            RailgunnerArisuVoiceoverComponent.nseBlock = RegisterNSE("Play_RailgunnerArisu_Blocked");
            RailgunnerArisuVoiceoverComponent.nsePanpakapan = RegisterNSE("Play_RailgunnerArisu_Panpakapan");
        }

        private NetworkSoundEventDef RegisterNSE(string eventName)
        {
            NetworkSoundEventDef nse = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            nse.eventName = eventName;
            Content.networkSoundEventDefs.Add(nse);
            nseList.Add(new NSEInfo(nse));
            return nse;
        }

        public void RefreshNSE()
        {
            foreach (NSEInfo nse in nseList)
            {
                nse.ValidateParams();
            }
        }

        public static List<NSEInfo> nseList = new List<NSEInfo>();
        public class NSEInfo
        {
            public NetworkSoundEventDef nse;
            public uint akId = 0u;
            public string eventName = string.Empty;

            public NSEInfo(NetworkSoundEventDef source)
            {
                this.nse = source;
                this.akId = source.akId;
                this.eventName = source.eventName;
            }

            private void DisableSound()
            {
                nse.akId = 0u;
                nse.eventName = string.Empty;
            }

            private void EnableSound()
            {
                nse.akId = this.akId;
                nse.eventName = this.eventName;
            }

            public void ValidateParams()
            {
                if (this.akId == 0u) this.akId = nse.akId;
                if (this.eventName == string.Empty) this.eventName = nse.eventName;

                if (!enableVoicelines.Value)
                {
                    DisableSound();
                }
                else
                {
                    EnableSound();
                }
            }
        }
    }
}
