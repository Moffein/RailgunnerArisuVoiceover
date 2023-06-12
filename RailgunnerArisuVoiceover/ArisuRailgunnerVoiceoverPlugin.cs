using RailgunnerArisuVoiceover.Components;
using BanditMikaVoiceover.Modules;
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

namespace RailgunnerArisuVoiceover
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Alicket.TendouArisTheRailgunner")]
    [BepInPlugin("com.Schale.RailgunnerArisuVoiceover", "RailgunnerArisuVoiceover", "1.0.0")]
    public class ArisuRailgunnerVoiceoverPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> enableVoicelines;
        private static SurvivorDef railgunnerSurvivorDef = Addressables.LoadAssetAsync<SurvivorDef>("RoR2/DLC1/Railgunner/Railgunner.asset").WaitForCompletion();
        public static bool playedSeasonalVoiceline = false;
        public static AssetBundle assetBundle;

        public void Awake()
        {
            Files.PluginInfo = this.Info;
            BaseVoiceoverComponent.Init();
            RoR2.RoR2Application.onLoad += OnLoad;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ArisuRailgunnerVoiceover.arisurailgunnervoiceoverbundle"))
            {
                assetBundle = AssetBundle.LoadFromStream(stream);
            }

            InitNSE();

            enableVoicelines = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Enable Voicelines"), true, new ConfigDescription("Enable voicelines when using the Aris Railgunner Skin."));
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                RiskOfOptionsCompat();
            }

            RailgunnerArisuVoiceoverComponent.nseSpecial = Content.CreateNSE("Play_RailgunnerArisu_ExSkill");
            RailgunnerArisuVoiceoverComponent.nseBlock = Content.CreateNSE("Play_RailgunnerArisu_Blocked");
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
            bool foundSkin = false;
            SkinDef[] banditSkins = SkinCatalog.FindSkinsForBody(BodyCatalog.FindBodyIndex("RailgunnerBody"));
            foreach (SkinDef skinDef in banditSkins)
            {
                if (skinDef.name == "ArisSkinDef")
                {
                    foundSkin = true;
                    RailgunnerArisuVoiceoverComponent.requiredSkinDefs.Add(skinDef);
                    break;
                }
            }

            if (!foundSkin)
            {
                Debug.LogError("ArisuRailgunnerVoiceover: Aris Railgunner SkinDef not found. Voicelines will not work!");
            }
            else
            {
                On.RoR2.CharacterBody.Start += AttachVoiceoverComponent;

                On.RoR2.SurvivorMannequins.SurvivorMannequinSlotController.RebuildMannequinInstance += (orig, self) =>
                {
                    orig(self);
                    if (self.currentSurvivorDef == railgunnerSurvivorDef)
                    {
                        //Loadout isn't loaded first time this is called, so we need to manually get it.
                        //Probably not the most elegant way to resolve this.
                        if (self.loadoutDirty)
                        {
                            if (self.networkUser)
                            {
                                self.networkUser.networkLoadout.CopyLoadout(self.currentLoadout);
                            }
                        }

                        //Check SkinDef
                        BodyIndex bodyIndexFromSurvivorIndex = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(self.currentSurvivorDef.survivorIndex);
                        int skinIndex = (int)self.currentLoadout.bodyLoadoutManager.GetSkinIndex(bodyIndexFromSurvivorIndex);
                        SkinDef safe = HG.ArrayUtils.GetSafe<SkinDef>(BodyCatalog.GetBodySkins(bodyIndexFromSurvivorIndex), skinIndex);
                        if (RailgunnerArisuVoiceoverComponent.requiredSkinDefs.Contains(safe))
                        {
                            bool played = false;
                            if (!playedSeasonalVoiceline)
                            {
                                if (System.DateTime.Today.Month == 1 && System.DateTime.Today.Day == 1)
                                {
                                    Util.PlaySound("Play_RailgunnerArisu_Lobby_Newyear", self.mannequinInstanceTransform.gameObject);
                                    played = true;
                                }
                                else if (System.DateTime.Today.Month == 5 && System.DateTime.Today.Day == 8)
                                {
                                    Util.PlaySound("Play_RailgunnerArisu_Lobby_bday", self.mannequinInstanceTransform.gameObject);
                                    played = true;
                                }
                                else if (System.DateTime.Today.Month == 10 && System.DateTime.Today.Day == 31)
                                {
                                    Util.PlaySound("Play_RailgunnerArisu_Lobby_Halloween", self.mannequinInstanceTransform.gameObject);
                                    played = true;
                                }
                                else if (System.DateTime.Today.Month == 12 && System.DateTime.Today.Day == 25)
                                {
                                    Util.PlaySound("Play_RailgunnerArisu_Lobby_xmas", self.mannequinInstanceTransform.gameObject);
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
                                    Util.PlaySound("Play_RailgunnerArisu_TitleDrop", self.mannequinInstanceTransform.gameObject);
                                }
                                else
                                {
                                    Util.PlaySound("Play_RailgunnerArisu_Lobby", self.mannequinInstanceTransform.gameObject);
                                }
                            }
                        }
                    }
                };
            }
            RailgunnerArisuVoiceoverComponent.ScepterIndex = ItemCatalog.FindItemIndex("ITEM_ANCIENT_SCEPTER");
        }

        private void InitNSE()
        {

        }


        private void AttachVoiceoverComponent(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (self)
            {
                if (RailgunnerArisuVoiceoverComponent.requiredSkinDefs.Contains(SkinCatalog.GetBodySkinDef(self.bodyIndex, (int)self.skinIndex)))
                {
                    BaseVoiceoverComponent existingVoiceoverComponent = self.GetComponent<BaseVoiceoverComponent>();
                    if (!existingVoiceoverComponent) self.gameObject.AddComponent<RailgunnerArisuVoiceoverComponent>();
                }
            }
        }
    }
}
