using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using System;
using System.Linq;
using UnityEngine;

namespace SPTExfilNow {
    /// <summary>exfil plugin for SPTarkov</summary>
    [BepInPlugin("net.skydust.SPTExfilNowPlugin", "SPTExfilNowPlugin", "1.0.2")]
    [BepInProcess("EscapeFromTarkov")]
    public class SPTExfilNowPlugin : BaseUnityPlugin {        
        private Boolean IsBusy{get;set;} = false;
                
        public ConfigEntry<KeyboardShortcut>? ExfilShortcutKey{get;private set;} =null;

        public ConfigEntry<Boolean>? Debug{get;private set;} = null;

        private AssemblyCSharp__EFT__LocalGame.SpawnPatch? SpawnPatch{get;set;} = null;

        public static EFT.LocalGame? LocalGame{get;set;} = null;

        protected void Awake () {
            this.ExfilShortcutKey = this.Config.Bind<KeyboardShortcut>("config","exfil-shortcut",new KeyboardShortcut(KeyCode.Backslash,KeyCode.LeftControl),"shortcut for exfil now, default is [CTRL + \\]");
            this.Debug = this.Config.Bind<Boolean>("config","debug",false,"don't touch this");
            this.SpawnPatch = new AssemblyCSharp__EFT__LocalGame.SpawnPatch();
            this.Logger.LogDebug("plugin loaded");
        }

        protected void Start () {
            this.SpawnPatch?.Enable();
            this.Logger.LogDebug("plugin actived");
        }

        protected void Update () {
            if(this.Debug?.Value==true){
                this.Logger.LogDebug(String.Concat("current gamemode: ",SPTExfilNowPlugin.LocalGame?.GameType.ToString()));
            }
            if(this.IsBusy){return;}
            if(this.ExfilShortcutKey==null || !this.ExfilShortcutKey.Value.IsUp()) {return;}
            ThreadingHelper.Instance.StartSyncInvoke(this.ExfilNow);
        }

        protected void OnDestroy() {
            this.SpawnPatch?.Disable();
            this.SpawnPatch = null;
            this.Logger.LogDebug("plugin deactived");
        }

        private void ExfilNow () {
            this.IsBusy = true;
            GameWorld? gameWorld = Singleton<GameWorld>.Instance;
            if(gameWorld==null){
                this.IsBusy = false;
                return;
            }
            if (SPTExfilNowPlugin.LocalGame?.GameType==EGameType.Hideout || SPTExfilNowPlugin.LocalGame == null) {
                this.Logger.LogDebug("LocalGame instance invalid");
                this.IsBusy = false;
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
            SPTExfilNowPlugin.LocalGame.Stop(gameWorld.MainPlayer.ProfileId, ExitStatus.Survived,exfiltrationPoint.name);
            SPTExfilNowPlugin.LocalGame = null;
            NotificationManagerClass.DisplayMessageNotification(String.Concat("exfil at ",exfiltrationPoint.name));
            this.Logger.LogDebug(String.Concat("exfil at ",exfiltrationPoint.name));
        }
    }
}
