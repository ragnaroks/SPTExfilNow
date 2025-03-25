using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using CommonAssets.Scripts.Game;
using EFT;
using EFT.Interactive;
using System;
using System.Linq;
using UnityEngine;

namespace SPTExfilNow {
    /// <summary>exfil plugin for SPTarkov</summary>
    [BepInPlugin("net.skydust.SPTExfilNowPlugin", "SPTExfilNowPlugin", "1.0.1")]
    [BepInProcess("EscapeFromTarkov")]
    public class SPTExfilNowPlugin : BaseUnityPlugin {        
        private Boolean IsBusy{get;set;} = false;
        
        public ConfigEntry<KeyboardShortcut>? ExfilShortcutKey{get;private set;} =null;

        protected void Awake () {
            this.ExfilShortcutKey = this.Config.Bind<KeyboardShortcut>("config","exfil-shortcut",new KeyboardShortcut(KeyCode.Backslash,KeyCode.LeftControl),"shortcut for exfil now, default is [CTRL + \\]");
            this.Logger.LogDebug("plugin loaded");
        }

        protected void Start () {
            this.Logger.LogDebug("plugin actived");
        }

        protected void Update () {
            if(this.IsBusy){return;}
            if(this.ExfilShortcutKey==null || !this.ExfilShortcutKey.Value.IsUp()) {return;}
            ThreadingHelper.Instance.StartSyncInvoke(this.ExfilNow);
        }

        protected void OnDestroy() {
            this.Logger.LogDebug("plugin deactived");
        }

        private void ExfilNow () {
            this.IsBusy = true;
            GameWorld? gameWorld = Singleton<GameWorld>.Instance;
            if(gameWorld==null){
                this.IsBusy = false;
                return;
            }
            // copy from "BufferInnerZone.cs"
            if(!(Singleton<AbstractGame>.Instance is EndByExitTrigerScenario.GInterface122 ginterface)){
                this.IsBusy = false;
                this.Logger.LogDebug("AbstractGame instance invalid");
                return;
            }
            ExfiltrationControllerClass? exfiltrationController = gameWorld.ExfiltrationController;
            if (exfiltrationController == null) {
                this.IsBusy = false;
                this.Logger.LogDebug("ExfiltrationControllerClass invalid");
                return;
            }
            ExfiltrationPoint? exfiltrationPoint = exfiltrationController.ExfiltrationPoints.FirstOrDefault(x=>x.isActiveAndEnabled && !x.HasRequirements);
            if(exfiltrationPoint==null){
                this.IsBusy = false;
                NotificationManagerClass.DisplayMessageNotification("not found any available exfil point");
                this.Logger.LogDebug("not found any available exfil point");
                return;
            }
            this.IsBusy = false;
            ginterface.StopSession(gameWorld.MainPlayer.ProfileId, ExitStatus.Survived,exfiltrationPoint.name);
            NotificationManagerClass.DisplayMessageNotification(String.Concat("exfil at ",exfiltrationPoint.name));
            this.Logger.LogDebug(String.Concat("exfil at ",exfiltrationPoint.name));
        }
    }
}
