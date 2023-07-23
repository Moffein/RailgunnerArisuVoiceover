﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RailgunnerArisuVoiceover.Components
{
    public class RailgunnerArisuVoiceoverComponent : BaseVoiceoverComponent
    {
        public static ItemIndex ScepterIndex;
        public static List<SkinDef> requiredSkinDefs = new List<SkinDef>();
        public static NetworkSoundEventDef nseSpecial;
        public static NetworkSoundEventDef nseBlock;

        private float blockedCooldown = 0f;
        private float specialCooldown = 0f;
        private float levelCooldown = 0f;
        private float coffeeCooldown = 0f;
        private float elixirCooldown = 0f;
        private float lowHealthCooldown = 0f;
        private bool acquiredScepter = false;
        protected override void Awake()
        {
            spawnVoicelineDelay = 3f;
            if (Run.instance && Run.instance.stageClearCount == 0)
            {
                spawnVoicelineDelay = 6.5f;
            }
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            CharacterBody cb = base.GetComponent<CharacterBody>();
            if (cb && cb.inventory && cb.inventory.GetItemCount(ScepterIndex) > 0) acquiredScepter = true;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (blockedCooldown > 0f) blockedCooldown -= Time.fixedDeltaTime;
            if (specialCooldown > 0f) specialCooldown -= Time.fixedDeltaTime;
            if (levelCooldown > 0f) levelCooldown -= Time.fixedDeltaTime;
            if (coffeeCooldown > 0f) coffeeCooldown -= Time.fixedDeltaTime;
            if (elixirCooldown > 0f) elixirCooldown -= Time.fixedDeltaTime;
            if (lowHealthCooldown > 0f) lowHealthCooldown -= Time.fixedDeltaTime;
        }

        public override void PlayDamageBlockedServer()
        {
            if (!NetworkServer.active || blockedCooldown > 0f) return;
            bool played = TryPlayNetworkSound(nseBlock, 1.2f, false);
            if (played) blockedCooldown = 30f;
        }

        public override void PlayDeath()
        {
            TryPlaySound("Play_RailgunnerArisu_Defeat", 3.5f, true);
        }

        public override void PlayHurt(float percentHPLost)
        {
            if (percentHPLost >= 0.1f)
            {
                TryPlaySound("Play_RailgunnerArisu_TakeDamage", 0f, false);
            }
        }

        public override void PlayJump() { }

        public override void PlayLevelUp()
        {
            if (levelCooldown > 0f) return;
            bool played = TryPlaySound("Play_RailgunnerArisu_LevelUp", 6f, false);
            if (played) levelCooldown = 60f;
        }

        public override void PlayLowHealth()
        {
            if (lowHealthCooldown > 0f) return;
            if (TryPlaySound("Play_RailgunnerArisu_LowHealth", 1.9f, false)) lowHealthCooldown = 60f;
        }

        public override void PlayPrimaryAuthority() { }

        public override void PlaySecondaryAuthority() { }

        public override void PlaySpawn()
        {
            TryPlaySound("Play_RailgunnerArisu_Spawn", 5f, true);
        }

        public override void PlaySpecialAuthority()
        {
            if (specialCooldown > 0f) return;
            bool played = TryPlayNetworkSound(nseSpecial, 4f, false);
            if (played) specialCooldown = 10f;
        }

        public override void PlayTeleporterFinish()
        {
            TryPlaySound("Play_RailgunnerArisu_Victory", 3f, false);
        }

        public override void PlayTeleporterStart()
        {
            TryPlaySound("Play_RailgunnerArisu_TeleporterStart", 1.4f, false);
        }

        public override void PlayUtilityAuthority() { }

        public override void PlayVictory()
        {
            TryPlaySound("Play_RailgunnerArisu_Victory", 3f, true);
        }

        protected override void Inventory_onItemAddedClient(ItemIndex itemIndex)
        {
            base.Inventory_onItemAddedClient(itemIndex);
            if (RailgunnerArisuVoiceoverComponent.ScepterIndex != ItemIndex.None && itemIndex == RailgunnerArisuVoiceoverComponent.ScepterIndex)
            {
                PlayAcquireScepter();
            }
            else if (itemIndex == DLC1Content.Items.AttackSpeedAndMoveSpeed.itemIndex)
            {
                PlayCoffee();
            }
            else if (itemIndex == DLC1Content.Items.HealingPotion.itemIndex)
            {
                PlayElixir();
            }
            else
            {
                ItemDef id = ItemCatalog.GetItemDef(itemIndex);
                if (id && id.deprecatedTier == ItemTier.Tier3)
                {
                    PlayAcquireLegendary();
                }
            }
        }

        public void PlayAcquireScepter()
        {
            if (acquiredScepter) return;
            TryPlaySound("Play_RailgunnerArisu_AcquireScepter", 5.6f, true);
            acquiredScepter = true;
        }

        public void PlayAcquireLegendary()
        {
            if (Util.CheckRoll(20f))
            {
                TryPlaySound("Play_RailgunnerArisu_Panpakapan", 1.5f, false);
                return;
            }
            if (Util.CheckRoll(50f))
            {
                TryPlaySound("Play_RailgunnerArisu_Relationship_Short", 4.6f, false);
                return;
            }
            TryPlaySound("Play_RailgunnerArisu_Relationship_Long", 15f, false);
        }

        public void PlayCoffee()
        {
            if (coffeeCooldown > 0) return;
            if (TryPlaySound("Play_RailgunnerArisu_Cafe_2", 2f, false)) coffeeCooldown = 60f;
        }

        public void PlayElixir()
        {
            if (elixirCooldown > 0) return;
            if (TryPlaySound("Play_RailgunnerArisu_Heal", 1.5f, false)) coffeeCooldown = 60f;
        }
    }
}
