using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using System;
using System.Linq;
using UnityEngine;

namespace SPTExfilNow {
    [BepInPlugin("net.skydust.SPTExfilNowPlugin", "SPTExfilNowPlugin", "1.0.3")]
    [BepInProcess("EscapeFromTarkov")]
    public class SPTExfilNowPlugin : BaseUnityPlugin {
        private Boolean IsBusy { get; set; } = false;

        private AssemblyCSharp__EFT__LocalGame.SpawnPatch? SpawnPatch { get; set; } = null;

        public static ConfigEntry<KeyboardShortcut>? ExfilShortcutKey { get; private set; } = null;

        //public static ConfigEntry<Boolean>? EnableESP{get;private set;} = null;

        public static EFT.LocalGame? LocalGame { get; set; } = null;

        protected void Awake () {
            SPTExfilNowPlugin.ExfilShortcutKey = this.Config.Bind<KeyboardShortcut>("config", "exfil-shortcut", new KeyboardShortcut(KeyCode.Backslash, KeyCode.LeftControl), "shortcut for exfil now, default is [CTRL + \\]");
            //SPTExfilNowPlugin.EnableESP = this.Config.Bind<Boolean>("config","enable-ESP",false,"exfil point ESP");
            this.SpawnPatch = new AssemblyCSharp__EFT__LocalGame.SpawnPatch();
            this.Logger.LogDebug("plugin loaded");
        }

        protected void Start () {
            this.SpawnPatch?.Enable();
            this.Logger.LogDebug("plugin actived");
        }

        protected void Update () {
            if (this.IsBusy) { return; }
            if (SPTExfilNowPlugin.ExfilShortcutKey?.Value.IsUp() != true) { return; }
            if (SPTExfilNowPlugin.LocalGame?.GameType != EGameType.Offline) { return; }
            this.IsBusy = true;
            this.ExfilNow();
        }

        protected void OnDestroy () {
            this.SpawnPatch?.Disable();
            SPTExfilNowPlugin.LocalGame = null;
            this.Logger.LogDebug("plugin deactived");
        }

        private void ExfilNow () {
            GameWorld? gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) {
                this.IsBusy = false;
                return;
            }
            if (SPTExfilNowPlugin.LocalGame?.GameType != EGameType.Offline) {
                this.IsBusy = false;
                return;
            }
            ExfiltrationControllerClass? exfiltrationController = gameWorld.ExfiltrationController;
            if (exfiltrationController == null) {
                this.IsBusy = false;
                return;
            }
            ExfiltrationPoint? exfiltrationPoint = null;
            switch (gameWorld.MainPlayer.Fraction) {
                case ETagStatus.Scav:
                exfiltrationPoint = exfiltrationController.ScavExfiltrationPoints.FirstOrDefault(x => x.isActiveAndEnabled && !x.HasRequirements);
                break;
                case ETagStatus.Usec:
                case ETagStatus.Bear:
                exfiltrationPoint = exfiltrationController.ExfiltrationPoints.FirstOrDefault(x => x.isActiveAndEnabled && !x.HasRequirements);
                break;
                default:
                break;
            }
            if (exfiltrationPoint == null) {
                this.IsBusy = false;
                NotificationManagerClass.DisplayMessageNotification("not found any available exfil point");
                return;
            }
            String exfilName = exfiltrationPoint.Settings.Name.Localized();
            SPTExfilNowPlugin.LocalGame.Stop(gameWorld.MainPlayer.ProfileId, ExitStatus.Survived, exfilName);
            SPTExfilNowPlugin.LocalGame = null;
            this.IsBusy = false;
            NotificationManagerClass.DisplayMessageNotification(String.Concat("exfil at ", exfilName));
            this.Logger.LogDebug(String.Concat("exfil at ", exfilName));
        }
    }
}
