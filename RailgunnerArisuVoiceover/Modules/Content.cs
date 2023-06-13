using System;
using System.Collections.Generic;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace RailgunnerArisuVoiceover.Modules
{
    public class Content : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => "com.Schale.RailgunnerArisuVoiceover";

        public static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();

        public void Initialize()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = this.identifier;
            contentPack.networkSoundEventDefs.Add(networkSoundEventDefs.ToArray());

            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public static NetworkSoundEventDef CreateNSE(string eventName)
        {
            NetworkSoundEventDef nse = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            nse.eventName = eventName;
            networkSoundEventDefs.Add(nse);
            return nse;
        }
    }
}