using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LukaSaberRP
{
    public class Plugin : IPlugin
    {
        //Based on Xyonico's open source BeatSaberDiscordPresence plugin
        //Current BeatSaber version supported: 0.12.2


        //Beatsaber Information
        public string Name => "LSaberRP";
        public string Author => "Luka";
        public string Version => "0.0.1";
        private StandardLevelSceneSetupDataSO bsSetupData;


        //Discord RPC Information
        public static readonly DiscordRpc.RichPresence Presence = new DiscordRpc.RichPresence();
        public string BeatsaberDiscordID = "528804834850373652";


        //Configuration variables 
        //Menu
        public String menuDetails = "In Menus";
        public String menuState = "Idling...";
        public String menuLargeImageText = "In the menus...";
        public String menuSmallImageText = "Running a modified version of BeatSaber";
        public Boolean showMenuTimer = true;
        //In game
        public Boolean igShowIfSongIsCustom = true;
        public Boolean igShowNoFailOption = true;
        public Boolean igShowInstantFailOption = true;
        public String igLargeImageText = "Chopping blocks!";
        public String igSmallImageText = "Playing BeatSaber!";
        //If nothing else is possible at all
        //public String lastResortDetails = "Chopping blocks...";


        //Beatsaber started
        public void OnApplicationStart()
        {
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            Console.WriteLine(Name + " | Attempting Discord RPC connection.");
            var handlers = new DiscordRpc.EventHandlers();
            DiscordRpc.Initialize(
                BeatsaberDiscordID, 
                ref handlers, 
                false, 
                string.Empty);
            Console.WriteLine(Name + " | Established connection with Discord RPC.");
            Console.WriteLine(Name + " | Attempting to read configuration file.");
            try
            {
                //Reading
                String bsDirectory = Environment.CurrentDirectory;
                bsDirectory = bsDirectory.Replace('\\', '/');
                String customInfoFilePath = bsDirectory + "/UserData/LSaberRP.txt";
                String usersOptions = System.IO.File.ReadAllText(customInfoFilePath);
                String lineReader;
                String booleanReader;
                System.IO.StreamReader configFile =new System.IO.StreamReader(customInfoFilePath);
                //Applying
                while ((lineReader = configFile.ReadLine()) != null)
                {
                    if (lineReader.StartsWith("##menuDetails:")) //menu details
                    {
                        menuDetails = lineReader.Substring(14);
                    }
                    else if (lineReader.StartsWith("##menuState:")) //menu state
                    {
                        menuState = lineReader.Substring(12);
                    }
                    else if (lineReader.StartsWith("##menuLargeImageText:")) //menu large image text
                    {
                        menuLargeImageText = lineReader.Substring(21);
                    }
                    else if (lineReader.StartsWith("##menuSmallImageText:")) //menu small image text
                    {
                        menuSmallImageText = lineReader.Substring(21);
                    }
                    else if (lineReader.StartsWith("##showMenuTimer:")) //show menu timer 
                    {
                        booleanReader = lineReader.Substring(16).ToLower();
                        if (booleanReader.Equals("true")) showMenuTimer = true; else showMenuTimer = false; 
                    }
                    else if (lineReader.StartsWith("##igShowIfSongIsCustom:")) //game show if song is custom
                    {
                        booleanReader = lineReader.Substring(23).ToLower();
                        if (booleanReader.Equals("true")) igShowIfSongIsCustom = true; else igShowIfSongIsCustom = false;
                    }
                    else if (lineReader.StartsWith("##igShowNoFailOption:")) //game show no fail option
                    {
                        booleanReader = lineReader.Substring(21).ToLower();
                        if (booleanReader.Equals("true")) igShowNoFailOption = true; else igShowNoFailOption = false;
                    }
                    else if (lineReader.StartsWith("##igShowInstantFailOption:")) //game show instant fail option
                    {
                        booleanReader = lineReader.Substring(26).ToLower();
                        if (booleanReader.Equals("true")) igShowInstantFailOption = true; else igShowInstantFailOption = false;
                    }
                    else if (lineReader.StartsWith("##igLargeImageText:")) //game large image text
                    {
                        igLargeImageText = lineReader.Substring(19);
                    }
                    else if (lineReader.StartsWith("##igSmallImageText:")) //game small image text
                    {
                        igSmallImageText = lineReader.Substring(19);
                    }
                }
                configFile.Close();
                Console.WriteLine(Name + " | Configuration read and applied.");
            }
            catch(Exception e)
            {
                Console.WriteLine(Name + " | Reading configuration failed. Reverting to original settings.");
                Console.WriteLine(Name + " | Is it a possible user side error? Check the technical report:");
                Console.WriteLine(Name + " | Error Report: " + e);
                //Menu
                menuDetails = "In Menus";
                menuState = "Idling...";
                menuLargeImageText = "In the menus...";
                menuSmallImageText = "Running a modified version of BeatSaber";
                showMenuTimer = true;
                //In game
                igShowIfSongIsCustom = true;
                igShowNoFailOption = true;
                igShowInstantFailOption = true;
                igLargeImageText = "Chopping blocks!";
                igSmallImageText = "Playing BeatSaber!";
                Console.WriteLine(Name + " | Settings reverted.");
                //If nothing else is possible
                //lastResortDetails = "Chopping blocks...";
            }

        }



        //Beatsaber scene changed
        private void SceneManagerOnActiveSceneChanged(Scene oldSceneName, Scene newSceneName)
        {

            try
            {


                //Updating the Discord Rich Presence information based upon the scene loaded
                if (newSceneName.name.Equals("Menu"))
                {

                    try
                    {

                        //Building Discord Information for menu scene
                        Console.WriteLine(Name + " | Updating Discord Info to 'In Menu'.");
                        Presence.details = menuDetails;
                        Presence.state = menuState;
                        if(showMenuTimer == true) Presence.startTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                        if (showMenuTimer == false) Presence.startTimestamp = default(long);
                        Presence.largeImageKey = "beatsaberlogo";
                        Presence.largeImageText = menuLargeImageText;
                        Presence.smallImageKey = "ismoddedlul";
                        Presence.smallImageText = menuSmallImageText;
                        DiscordRpc.UpdatePresence(Presence);

                    }
                    catch
                    {
                        //Just in case ;)
                        Console.WriteLine(Name + " | Error sending Menu DiscordRP data. Report errors to: luka#8375 !");
                    }


                }
                else if (newSceneName.name.Equals("GameCore"))
                {

                    try
                    {

                        //Gathering data needed for Discord Information
                        Console.WriteLine(Name + " | Fetching resources for Discord RP.");
                        bsSetupData = Resources.FindObjectsOfTypeAll<StandardLevelSceneSetupDataSO>().FirstOrDefault();

                        //Error proofing with other mods that change the state for some reason
                        if (bsSetupData == null)
                        {
                            Console.WriteLine(Name + " | Unable to locate resources for Discord RP; returning.");
                            return;
                        }

                        String drpcDetails;
                        String drpcState;
                        var songHardness = bsSetupData.difficultyBeatmap.difficulty;
                        var songName = bsSetupData.difficultyBeatmap.level.songName;
                        Boolean customSong = false;
                        Boolean noFail = false;
                        Boolean instaFail = false;
                        if (bsSetupData.difficultyBeatmap.level.levelID.Contains('∎')) customSong = true;
                        if (bsSetupData.gameplayCoreSetupData.gameplayModifiers.noFail) noFail = true;
                        if (bsSetupData.gameplayCoreSetupData.gameplayModifiers.instaFail) instaFail = true;


                        //Determining game mode
                        String gameType = null;



                        //Filling in information
                        drpcDetails = songName;
                        if (customSong == true && igShowIfSongIsCustom == true) drpcDetails += " [Custom]";
                        drpcState = songHardness.ToString();
                        if (noFail == true && igShowNoFailOption == true) drpcState += " [No Fail]";
                        if (instaFail == true && igShowInstantFailOption == true) drpcState += " [Instafail]";
               
                  

                        //Building Dicord Information for in game 
                        Console.WriteLine(Name + " | Updating Discord Info to 'In Game'.");
                        Presence.details = drpcDetails;
                        Presence.state = drpcState;
                        Presence.startTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                        Presence.largeImageKey = "beatsaberplaying";
                        Presence.largeImageText = igLargeImageText;
                        Presence.smallImageKey = "beatsaberlogo";
                        Presence.smallImageText = igSmallImageText;
                        DiscordRpc.UpdatePresence(Presence);



                    }
                    catch
                    {
                        //Just in case ;)
                        Console.WriteLine(Name + " | Error sending Game DiscordRP data. Report errors to: luka#8375 !");
                    }




                }
                //Fallback text isabled because buggy!
                /*else
                {
                    //no multiplayer mode support yet :( ~ ?EmptyTransition?
                    //In case not in menu or in game or something like that
                    Presence.details = lastResortDetails;
                    Presence.state = null;
                    Presence.startTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    Presence.largeImageKey = "beatsaberlogo";
                    Presence.largeImageText = menuLargeImageText;
                    Presence.smallImageKey = "ismoddedlul";
                    Presence.smallImageText = menuSmallImageText;
                    DiscordRpc.UpdatePresence(Presence);
                }*/


            }
            catch
            {
                //Just in case ;)
                Console.WriteLine(Name + " | Error getting scene type (menu/in game). Report errors to: luka#8375 !");
            }

        }



        //On update
        public void OnUpdate()
        {
            DiscordRpc.RunCallbacks();
        }



        //Beatsaber closed 
        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            Console.WriteLine(Name + " | Attempting to close connection with Discord RP.");
            DiscordRpc.Shutdown();
            Console.WriteLine(Name + " | Connection closed with Discord RP.");
        }




        //Stuff i don't use 
        private void SceneManager_sceneLoaded(Scene sceneName, LoadSceneMode gameMode)
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnFixedUpdate()
        {
        }



        //Method to set the game type (~totally not based on xyonico's original method :3) 
        //Unused
        public static String SetGameType(SoloModeSelectionViewController.MenuType gameplayMode)
        {    
            if(gameplayMode == SoloModeSelectionViewController.MenuType.FreePlayMode)
            {
                return "Freeplay";
            }
            else if(gameplayMode == SoloModeSelectionViewController.MenuType.OneSaberMode)
            {
                return "OneSaber";
            }
            else if(gameplayMode == SoloModeSelectionViewController.MenuType.NoArrowsMode)
            {
                return "NoArrows";
            }
            else
            {
                return "Freeplay";
            }
        }



    }
}
