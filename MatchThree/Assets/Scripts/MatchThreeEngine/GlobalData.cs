using System;
using System.Collections;
using System.Collections.Generic;
using MatchThreeEngine;
using UnityEngine;

public static class GlobalData 
{
    [Serializable]
    public enum AudioClipType
    {
        OnButtonClick, OnTileClick, OnMatch, OnWin, OnLose, Explosion 
    }
    public const string LAST_PLAYED_LEVEL = "LAST_PLAYED_LEVEL";
    public const string AVAILABLE_TIPS = "AVAILABLE_TIPS";
    public const string IN_GAME_SCENE = "InGame";
    public const string MASTER_VOLUME = "MasterVolume";
    public const string MUSIC_VOLUME = "BGMusicVolume";
    
    public static void OnLevelComplet(int levelNumber, int onStarsComplet)
    {
        PlayerPrefs.SetInt($"Level{levelNumber}Complet", onStarsComplet);
    }
    public static int IsLevelComplet(int levelNumber)
    {
        return PlayerPrefs.GetInt($"Level{levelNumber}Complet", 0);
    }
    public static void AddAvailableTips(int numberOfTips)
    {
        var currentTips = PlayerPrefs.GetInt(AVAILABLE_TIPS, 0);
        currentTips += numberOfTips;
        PlayerPrefs.SetInt(AVAILABLE_TIPS, currentTips);
        PlayerPrefs.Save();
    }
    public static bool UseTip()
    {
        var currentTips = PlayerPrefs.GetInt(AVAILABLE_TIPS, 0);
        if (currentTips > 0)
        {
            currentTips -= 1;
            PlayerPrefs.SetInt(AVAILABLE_TIPS, currentTips);
            PlayerPrefs.Save();
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool IsSpecialTile(TileData tile)
    {
        return tile.TypeId > 100;
    }
    public static bool IsTile(TileData tile)
    {
        return tile.TypeId > 0;
    }
}
