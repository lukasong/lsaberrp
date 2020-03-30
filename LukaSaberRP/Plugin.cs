using IPA;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using BS_Utils;
using BS_Utils.Gameplay;
using BS_Utils.Utilities;

namespace LukaSaberRP
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class LukaSaberRP
    {

        //Establishing variables
        public string assemblyName = "LukaSaberRp";
        public string Name => "LSaberRP";
        public string Author => "LukaSong";
        public string Version => "2.0.0";
        private MainFlowCoordinator MainFlowCoord;
        public GameplayCoreSceneSetupData SceneSetupData;
        public GameplayCoreSceneSetup SceneSetup;
        public static readonly DiscordRpc.RichPresence Presence = new DiscordRpc.RichPresence();
        public string BeatsaberDiscordID = "528804834850373652";

        //Configuration variables 
        //Menu
        public string menuDetails = "In Menus";
        public string menuState = "Idling...";
        public string menuLargeImageText = "In the menus...";
        public string menuSmallImageText = "Running a modified version of BeatSaber";
        public Boolean showMenuTimer = true;
        //In game
        public Boolean igShowIfSongIsCustom = true;
        public Boolean igShowNoFailOption = true;
        public Boolean igShowInstantFailOption = true;
        public string igLargeImageText = "Slashing blocks!";
        public string igSmallImageText = "Playing BeatSaber!";

        //For use in frankensteining stuff together

        public static IPA.Logging.Logger Logger { get; private set; }

        [Init]
        public LukaSaberRP(IPA.Logging.Logger logger)
        {
            LukaSaberRP.Logger = logger;
        }

        [OnStart]
        public void Start()
        {
            Logger.Info(Name + " | Attempting Discord RPC connection.");
            var handlers = new DiscordRpc.EventHandlers();
            DiscordRpc.Initialize(
                BeatsaberDiscordID,
                ref handlers,
                false,
                string.Empty);
            Logger.Info(Name + " | Established connection with Discord RPC.");
            Logger.Info(Name + " | No congiuration required in this version! A bit silly to have one, isn't it..");
            Logger.Info(Name + " | and a waste of time ;-; well, we are all done here!");
            Logger.Info(Name + " | Updating Discord Info to 'In Menu'.");
            Presence.details = menuDetails;
            Presence.state = menuState;
            Presence.startTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            Presence.largeImageKey = "beatsaberlogo";
            Presence.largeImageText = menuLargeImageText;
            Presence.smallImageKey = "ismoddedlul";
            Presence.smallImageText = menuSmallImageText;
            DiscordRpc.UpdatePresence(Presence);
            BSEvents.gameSceneLoaded += this.onGameLoaded;
            BSEvents.menuSceneLoaded += this.onMenuLoaded;
        }

  
        private void onGameLoaded()
        {
            try
            {
                //Fetching!
                Boolean safetyNet = false;
                Logger.Info(Name + " | Fetching resources for Discord RP.");
                BS_Utils.Gameplay.LevelData thisData = BS_Utils.Plugin.LevelData;
                string songDifficulty = thisData.GameplayCoreSceneSetupData.difficultyBeatmap.difficulty.ToString();
                string songName = thisData.GameplayCoreSceneSetupData.difficultyBeatmap.level.songName.ToString();
                string songAuthor = thisData.GameplayCoreSceneSetupData.difficultyBeatmap.level.songAuthorName.ToString();
                string songDuration = thisData.GameplayCoreSceneSetupData.difficultyBeatmap.level.songDuration.ToString();
                try
                {
                    songDuration = songDuration.Substring(0, songDuration.IndexOf("."));
                }catch(Exception e)
                {
                    Logger.Info(Name + " | Skipping substringing the song duration... custom song? Take a look: " + songDuration);
                    safetyNet = true;
                }
                songDuration = songDuration.Replace(".", "");
                int songSeconds = Int32.Parse(songDuration);
                songDifficulty += GetGamemode();
                Logger.Info(songDuration.ToString());
                //Building Dicord Information for in game 
                Logger.Info(Name + " | Updating Discord Info to 'In Game'.");
                Presence.details = songName;
                if (!songAuthor.Equals(null))
                {
                    Presence.details += " [" + songAuthor + "]";
                }
                Presence.state = songDifficulty;
                /*if (safetyNet == true)
                {
                    Logger.Info(Name + " | Safety net e nabled, setting time to be elapsed rather than remaining. I'll look for a future update to BS Utils or a fix on my own for this, not sure whats going on. Sorry!");
                    Presence.startTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                }
                else
                {
                    Presence.endTimestamp = (long)(DateTime.UtcNow + +new TimeSpan(0, 0, songSeconds)).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                }*/
                Presence.startTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                Presence.largeImageKey = "beatsaberplaying";
                Presence.largeImageText = igLargeImageText;
                Presence.smallImageKey = "beatsaberlogo";
                Presence.smallImageText = igSmallImageText;
                DiscordRpc.UpdatePresence(Presence);
            }catch(Exception e)
            {
                Logger.Info("Error geting game details...." + e); 
            }
        }


        private void onMenuLoaded()
        {
            try
            {
                //Building Discord Information for menu scene
                Logger.Info(Name + " | Updating Discord Info to 'In Menu'.");
                Presence.details = menuDetails;
                Presence.state = menuState;
                Presence.startTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                Presence.largeImageKey = "beatsaberlogo";
                Presence.largeImageText = menuLargeImageText;
                Presence.smallImageKey = "ismoddedlul";
                Presence.smallImageText = menuSmallImageText;
                DiscordRpc.UpdatePresence(Presence);
            }
            catch
            {
                //Just in case ;)
                Logger.Info(Name + " | Error sending Menu DiscordRP data. Report errors to: luka#8375 !");
            }
        }

        //Beatsaber closed 
        public void OnApplicationQuit()
        {
            Logger.Info(Name + " | Attempting to close connection with Discord RP.");
            DiscordRpc.Shutdown();
            BSEvents.gameSceneLoaded -= this.onGameLoaded;
            BSEvents.menuSceneLoaded -= this.onMenuLoaded;
            Logger.Info(Name + " | Connection closed with Discord RP.");
        }

        public void OnUpdate()
        {
            DiscordRpc.RunCallbacks();
        }

        private string GetGamemode()
        {
            try
            {
                Type typeGamemode = MainFlowCoord.childFlowCoordinator.GetType();
                if (typeGamemode == typeof(SoloFreePlayFlowCoordinator))
                {
                    return ""; //not gonna add anything for solo
                }
                else if (typeGamemode == typeof(ArcadeFlowCoordinator))
                {
                    return " [Arcade]";
                }
                else if (typeGamemode == typeof(CampaignFlowCoordinator))
                {
                    return " [Campaign]";
                }
                else if (typeGamemode == typeof(PartyFreePlayFlowCoordinator))
                {
                    return " [Party!]";
                }
                else
                {
                    return ""; //not gonna add anything for solo
                }
            }catch(Exception e)
            {
                Logger.Info(Name + "| Error getting gamemode. Setting gamemode to empty.");
                return "";
            }
        }


    }
}
