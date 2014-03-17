﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GameMode.Definitions;
using GameMode.Events;
using GameMode.World;

namespace GameMode
{
    public class Server
    {
        #region Fields

        private static readonly Dictionary<int, TimerTickHandler> TimerHandlers = new Dictionary<int, TimerTickHandler>();

        #endregion

        #region a_players natives
        /// <summary>
        /// This function can be used to change the spawn information of a specific player. It allows you to automatically set someone's spawn weapons, their team, skin and spawn position, normally used in case of minigames or automatic-spawn systems. This function is more crash-safe then using SetPlayerSkin in OnPlayerSpawn and/or OnPlayerRequestClass, even though this has been fixed in 0.2.
        /// </summary>
        /// <param name="playerid">The PlayerID of who you want to set the spawn information.</param>
        /// <param name="team">The Team-ID of the chosen player.</param>
        /// <param name="skin">The skin which the player will spawn with.</param>
        /// <param name="x">The X-coordinate of the player's spawn position.</param>
        /// <param name="y">The Y-coordinate of the player's spawn position.</param>
        /// <param name="z">The Z-coordinate of the player's spawn position.</param>
        /// <param name="rotation">The direction in which the player needs to be facing after spawning.</param>
        /// <param name="weapon1">The first spawn-weapon for the player.</param>
        /// <param name="weapon1Ammo">The amount of ammunition for the primary spawnweapon.</param>
        /// <param name="weapon2">The second spawn-weapon for the player.</param>
        /// <param name="weapon2Ammo">The amount of ammunition for the second spawnweapon.</param>
        /// <param name="weapon3">The third spawn-weapon for the player.</param>
        /// <param name="weapon3Ammo">The amount of ammunition for the third spawnweapon.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetSpawnInfo(int playerid, int team, int skin, float x, float y, float z,
            float rotation, int weapon1, int weapon1Ammo, int weapon2, int weapon2Ammo, int weapon3, int weapon3Ammo);

        /// <summary>
        /// (Re)Spawns a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to spawn.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SpawnPlayer(int playerid);

        /// <summary>
        /// Set a player's position.
        /// </summary>
        /// <param name="playerid">The ID of the player to set the position of.</param>
        /// <param name="x">The X coordinate to position the player at.</param>
        /// <param name="y">The Y coordinate to position the player at.</param>
        /// <param name="z">The Z coordinate to position the player at.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerPos(int playerid, float x, float y, float z);

        /// <summary>
        /// This sets the players position then adjusts the players z-coordinate to the nearest solid ground under the position.
        /// </summary>
        /// <param name="playerid">The ID of the player to set the position of.</param>
        /// <param name="x">The X coordinate to position the player at.</param>
        /// <param name="y">The Y coordinate to position the player at.</param>
        /// <param name="z">The Z coordinate to position the player at.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerPosFindZ(int playerid, float x, float y, float z);

        /// <summary>
        /// Get the X Y Z coordinates of a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to get the position of</param>
        /// <param name="x">A float to store the X coordinate in, passed by reference.</param>
        /// <param name="y">A float to store the Y coordinate in, passed by reference.</param>
        /// <param name="z">A float to store the Z coordinate in, passed by reference.</param>
        /// <returns>This function doesn't return a specific value</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerPos(int playerid, out float x, out float y, out float z);

        /// <summary>
        /// Set a player's facing angle.
        /// </summary>
        /// <remarks>
        /// Angles are reversed in GTA:SA - 90 degrees would be East in the real world, but in GTA:SA 90 is in fact West. North and South are still 0/360 and 180. To convert this, simply do 360 - angle.
        /// </remarks>
        /// <param name="playerid">The ID of the player to set the facing angle of.</param>
        /// <param name="angle">The angle the player should face.</param>
        /// <returns>This function doesn't return a specific value</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerFacingAngle(int playerid, float angle);

        /// <summary>
        /// Return angle of the direction the player is facing.
        /// </summary>
        /// <param name="playerid">The player you want to get the angle of.</param>
        /// <param name="angle">The Float to store the angle in, passed by reference.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerFacingAngle(int playerid, out float angle);

        /// <summary>
        /// Check if a player is in range of a point.
        /// </summary>
        /// <param name="playerid">The ID of the player.</param>
        /// <param name="range">The furthest distance the player can be from the point to be in range.</param>
        /// <param name="x">The X coordinate of the point to check the range to.</param>
        /// <param name="y">The Y coordinate of the point to check the range to.</param>
        /// <param name="z">The Z coordinate of the point to check the range to.</param>
        /// <returns>True if the player is in range of the point, otherwise False.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsPlayerInRangeOfPoint(int playerid, float range, float x, float y, float z);

        /// <summary>
        /// Calculate the distance between a player and a map coordinate.
        /// </summary>
        /// <param name="playerid">The ID of the player to calculate the distance from.</param>
        /// <param name="x">The X map coordinate.</param>
        /// <param name="y">The Y map coordinate.</param>
        /// <param name="z">The Z map coordinate.</param>
        /// <returns>The distance between the player and the point as a float.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern float GetPlayerDistanceFromPoint(int playerid, float x, float y, float z);

        /// <summary>
        /// Checks if a player is streamed in another player's client.
        /// </summary>
        /// <remarks>
        /// Players aren't streamed in on their own client, so if playerid is the same as forplayerid it will return false!
        /// </remarks>
        /// <remarks>
        /// Players stream out if they are more than 150 meters away (see server.cfg - stream_distance)
        /// </remarks>
        /// <param name="playerid">The ID of the player to check is streamed in.</param>
        /// <param name="forplayerid">The ID of the player to check if playerid is streamed in for.</param>
        /// <returns>True if forplayerid is streamed in for playerid, False if not.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsPlayerStreamedIn(int playerid, int forplayerid);

        /// <summary>
        /// Set the player's interior.
        /// </summary>
        /// <param name="playerid">The ID of the player to setthe interior of.</param>
        /// <param name="interiorid">The interior ID to set the player in.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerInterior(int playerid, int interiorid);

        /// <summary>
        /// Retrieves the player's current interior.
        /// </summary>
        /// <param name="playerid">The player to get the interior ID of.</param>
        /// <returns>The interior ID the player is currently in.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerInterior(int playerid);

        /// <summary>
        /// Set the health level of a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to set the health of.</param>
        /// <param name="health">The value to set the player's health to.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerHealth(int playerid, float health);

        /// <summary>
        /// The function GetPlayerHealth allows you to retrieve the health of a player. Useful for cheat detection, among other things.
        /// </summary>
        /// <param name="playerid">The ID of the player.</param>
        /// <param name="health">Float to store health, passed by reference.</param>
        /// <returns>True if succeeded, False if not.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerHealth(int playerid, out float health);

        /// <summary>
        /// Set a player's armour level.
        /// </summary>
        /// <param name="playerid">The ID of the player to set the armour of.</param>
        /// <param name="armour">The amount of armour to set, as a percentage (float).</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerArmour(int playerid, float armour);

        /// <summary>
        /// This function stores the armour of a player into a variable.
        /// </summary>
        /// <param name="playerid">The ID of the player that you want to get the armour of.</param>
        /// <param name="armour">The float to to store the armour in, passed by reference.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerArmour(int playerid, out float armour);

        /// <summary>
        /// Set the ammo of a player's weapon.
        /// </summary>
        /// <param name="playerid">The ID of the player to set the weapon ammo of.</param>
        /// <param name="weaponid">The ID of the weapon to set the ammo of.</param>
        /// <param name="ammo">The amount of ammo to set.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerAmmo(int playerid, int weaponid, int ammo);

        /// <summary>
        /// Returns the amount of ammunition the player has in his active weapon slot.
        /// </summary>
        /// <remarks>
        /// The ammo can hold 16-bit values, therefore values over 32767 will return erroneous values.
        /// </remarks>
        /// <param name="playerid">ID of the player.</param>
        /// <returns>The amount of ammunition the player has in his active weapon slot.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerAmmo(int playerid);

        /// <summary>
        /// Checks the state of a player's weapon.
        /// </summary>
        /// <param name="playerid">The ID of the player to obtain the state of.</param>
        /// <returns>The state of the player's weapon.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerWeaponState(int playerid);

        /// <summary>
        /// Check who a player is aiming at.
        /// </summary>
        /// <param name="playerid">The ID of the player to get the target of.</param>
        /// <returns>The ID of the target player, or <see cref="Misc.InvalidPlayerId"/> if none.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerTargetPlayer(int playerid);

        /// <summary>
        /// Set the team of a player.
        /// </summary>
        /// <remarks>
        /// Players can not damage/kill players on the same team unless they use a knife to slit their throat. As of SA-MP 0.3x, players are also unable to damage vehicles driven by a player from the same team. This can be enabled with <see cref="EnableVehicleFriendlyFire"/>.
        /// 255 (or <see cref="Misc.NoTeam"/>) is the default team to be able to shoot other players, not 0.
        /// </remarks>
        /// <param name="playerid">The ID of the player you want to set the team of.</param>
        /// <param name="teamid">The team to put the player in. Use <see cref="Misc.NoTeam"/> to remove the player from any team.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerTeam(int playerid, int teamid);

        /// <summary>
        /// Get the ID of the team the player is on.
        /// </summary>
        /// <param name="playerid">The ID of the player to return the team of.</param>
        /// <returns>The ID of the team the player is on, or 255 (defined as <see cref="Misc.NoTeam"/>) if they aren't on a team (default).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerTeam(int playerid);

        /// <summary>
        /// Set a player's score. Players' scores are shown in the scoreboard (hold TAB).
        /// </summary>
        /// <param name="playerid">The ID of the player to set the score of.</param>
        /// <param name="score">The value to set the player's score to.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerScore(int playerid, int score);

        /// <summary>
        /// This function returns a player's score as it was set using <see cref="SetPlayerScore"/>
        /// </summary>
        /// <param name="playerid">The player to get the score of.</param>
        /// <returns>The player's score.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerScore(int playerid);

        /// <summary>
        /// Checks the player's level of drunkenness.
        /// </summary>
        /// <remarks>
        /// If the level is less than 2000, the player is sober. The player's level of drunkness goes down slowly automatically (26 levels per second) but will always reach 2000 at the end (in 0.3b it will stop at zero). The higher drunkenness levels affect the player's camera, and the car driving handling. The level of drunkenness increases when the player drinks from a bottle (You can use <see cref="SetPlayerSpecialAction"/> to give them bottles).
        /// </remarks>
        /// <param name="playerid">The player you want to check the drunkenness level of.</param>
        /// <returns>An integer with the level of drunkenness of the player.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerDrunkLevel(int playerid);

        /// <summary>
        /// Sets the drunk level of a player which makes the player's camera sway and vehicles hard to control.
        /// </summary>
        /// <remarks>
        /// Players' drunk level will automatically decrease over time, based on their FPS (players with 50 FPS will lose 50 'levels' per second. This is useful for determining a player's FPS!).
        /// In 0.3a the drunk level will decrement and stop at 2000. In 0.3b+ the drunk level decrements to zero.)
        /// Levels over 2000 make the player drunk (camera swaying and vehicles difficult to control).
        /// Max drunk level is 50000.
        /// While the drunk level is above 5000, the player's HUD (radar etc.) will be hidden.
        /// </remarks>
        /// <param name="playerid">The ID of the player to set the drunkenness of.</param>
        /// <param name="level">The level of drunkenness to set.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerDrunkLevel(int playerid, int level);

        /// <summary>
        /// This function allows you to change the color of a player currently in-game.
        /// </summary>
        /// <param name="playerid">The player to change the color of.</param>
        /// <param name="color">The color to set, as an integer</param>
        /// <returns>his function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerColor(int playerid, int color);

        /// <summary>
        /// This function returns the color the player is currently using.
        /// </summary>
        /// <remarks>
        /// GetPlayerColor will return nothing unless SetPlayerColor has been used!
        /// </remarks>
        /// <param name="playerid">The player you want to know the color of.</param>
        /// <returns>The players color.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerColor(int playerid);

        /// <summary>
        /// Set the skin of a player.
        /// </summary>
        /// <remarks>
        /// If a player's skin is set when they are crouching, in a vehicle, or performing certain animations, they will become frozen or otherwise glitched. This can be fixed by using <see cref="TogglePlayerControllable"/>. Players can be detected as being crouched through <see cref="GetPlayerSpecialAction"/>.
        /// </remarks>
        /// <param name="playerid">The ID of the player to set the skin of.</param>
        /// <param name="skinid">The skin the player should use.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerSkin(int playerid, int skinid);

        /// <summary>
        /// Returns the class of the players skin.
        /// </summary>
        /// <param name="playerid">The player you want to get the skin from.</param>
        /// <returns>The skin id (0 if invalid).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerSkin(int playerid);

        /// <summary>
        /// Give a player a weapon with a specified amount of ammo.
        /// </summary>
        /// <param name="playerid">The ID of the player to give a weapon to.</param>
        /// <param name="weaponid">The ID of the weapon to give to the player.</param>
        /// <param name="ammo">The amount of ammo to give to the player.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GivePlayerWeapon(int playerid, int weaponid, int ammo);

        /// <summary>
        /// Removes all weapons from a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to remove the weapons of.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ResetPlayerWeapons(int playerid);

        /// <summary>
        /// Sets the armed weapon of the player.
        /// </summary>
        /// <param name="playerid">The ID of the player to arm with a weapon.</param>
        /// <param name="weaponid">The ID of the weapon that the player should be armed with.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerArmedWeapon(int playerid, int weaponid);

        /// <summary>
        /// Get the weapon and ammo in a specific player's weapon slot.
        /// </summary>
        /// <param name="playerid">The ID of the player whose weapon data to retrieve.</param>
        /// <param name="slot">The weapon slot to get data for (0-12).</param>
        /// <param name="weapon">The variable in which to store the weapon ID, passed by reference.</param>
        /// <param name="ammo">The variable in which to store the ammo, passed by reference.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerWeaponData(int playerid, int slot, out int weapon, out int ammo);

        /// <summary>
        /// Give (or take) money to/from a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to give money to.</param>
        /// <param name="money">The amount of money to give the player. Use a minus value to take money.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GivePlayerMoney(int playerid, int money);

        /// <summary>
        /// Reset a player's money to $0.
        /// </summary>
        /// <param name="playerid">The ID of the player to reset the money of.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ResetPlayerMoney(int playerid);

        /// <summary>
        /// Sets the name of a player.
        /// </summary>
        /// <remarks>
        /// If you set the player's name to the same name except different cased letters (i.e. "heLLO" to "hello"), it will not work. If used in <see cref="OnPlayerConnect"/>, the new name will not be shown for the connecting player.
        /// </remarks>
        /// <param name="playerid">The ID of the player to set the name of.</param>
        /// <param name="name">The name to set.</param>
        /// <returns>1 if the name was changed, 0 if the player is already using that name or -1 when the name cannot be changed. (it's in use, too long or has invalid characters)</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int SetPlayerName(int playerid, string name);

        /// <summary>
        /// Retrieves the amount of money a player has.
        /// </summary>
        /// <param name="playerid">The ID of the player to get the money of.</param>
        /// <returns>The amount of money the player has.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerMoney(int playerid);

        /// <summary>
        /// Get a player's current state.
        /// </summary>
        /// <param name="playerid">The ID of the player to get the current state of.</param>
        /// <returns>The player's current state as an integer.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerState(int playerid);

        /// <summary>
        /// Get the specified player's IP and store it in a string.
        /// </summary>
        /// <remarks>
        /// This function does not work when used in <see cref="OnPlayerConnect"/> because the player is already disconnected. It will return an invalid IP (255.255.255.255). Save players' IPs under <see cref="OnPlayerConnect"/> if they need to be used under <see cref="OnPlayerConnect"/>.
        /// </remarks>
        /// <param name="playerid">The ID of the player to get the IP of.</param>
        /// <param name="ip">The string to store the player's IP in, passed by reference</param>
        /// <param name="size">The maximum size of the IP. (Recommended 16)</param>
        /// <returns>True on success, False otherwise.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerIp(int playerid, out string ip, int size);

        /// <summary>
        /// Get the ping of a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to get the ping of.</param>
        /// <returns>The current ping of the player (expressed in milliseconds).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerPing(int playerid);

        /// <summary>
        /// Returns the ID of the player's current weapon.
        /// </summary>
        /// <param name="playerid">The ID of the player to get the weapon of.</param>
        /// <returns>The ID of the player's current weapon.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerWeapon(int playerid);

        /// <summary>
        /// Check which keys a player is pressing.
        /// </summary>
        /// <remarks>
        /// Only the FUNCTION of keys can be detected; not actual keys. You can not detect if a player presses space, but you can detect if they press sprint (which can be mapped (assigned) to ANY key, but is space by default)).
        /// </remarks>
        /// <param name="playerid">The ID of the player to detect the keys of.</param>
        /// <param name="keys">A set of bits containing the player's key states</param>
        /// <param name="updown">Up or Down value, passed by reference.</param>
        /// <param name="leftright">Left or Right value, passed by reference.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerKeys(int playerid, out int keys, out int updown, out int leftright);

        /// <summary>
        /// Get a player's name.
        /// </summary>
        /// <remarks>
        /// A player's name can be up to 24 characters long (as of 0.3d R2).
        /// This is defined as <see cref="Limits.MaxPlayerName"/>.
        /// Strings to store names in should be made this size, plus one extra cell for the null terminating character.
        /// </remarks>
        /// <param name="playerid">The ID of the player to get the name of.</param>
        /// <param name="name">The string to store the name in, passed by reference.</param>
        /// <param name="size">The length of the string that should be stored.</param>
        /// <returns>The length of the player's name.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerName(int playerid, out string name, int size);

        /// <summary>
        /// Sets the clock of the player to a specific value. This also changes the daytime. (night/day etc.)
        /// </summary>
        /// <param name="playerid">The ID of the player.</param>
        /// <param name="hour">Hour to set (0-23).</param>
        /// <param name="minute">Minutes to set (0-59).</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerTime(int playerid, int hour, int minute);

        /// <summary>
        /// Get the player's current game time. Set by <see cref="SetWorldTime"/>, <see cref="SetWorldTime"/>, or by <see cref="TogglePlayerClock"/>.
        /// </summary>
        /// <param name="playerid">The ID of the player that you want to get the time of.</param>
        /// <param name="hour">The variable to store the hour in, passed by reference.</param>
        /// <param name="minute">The variable to store the minutes in, passed by reference.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerTime(int playerid, out int hour, out int minute);

        /// <summary>
        /// Show/Hide the in-game clock (top right corner) for a specific player.
        /// </summary>
        /// <remarks>
        /// Time is not synced with other players!
        /// </remarks>
        /// <param name="playerid">The player whose clock you want to enable/disable.</param>
        /// <param name="toggle">True to show, False to hide.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TogglePlayerClock(int playerid, bool toggle);

        /// <summary>
        /// Set a player's weather. If <see cref="TogglePlayerClock"/> has been used to enable a player's clock, weather changes will interpolate (gradually change), otherwise will change instantly.
        /// </summary>
        /// <param name="playerid">The ID of the player whose weather to set.</param>
        /// <param name="weather">The weather to set.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerWeather(int playerid, int weather);

        /// <summary>
        /// Forces a player to go back to class selection.
        /// </summary>
        /// <remarks>
        /// The player will not return to class selection until they re-spawn. This can be achieved with <see cref="TogglePlayerSpectating"/>
        /// </remarks>
        /// <param name="playerid">The player to send back to class selection.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ForceClassSelection(int playerid);

        /// <summary>
        /// Set a player's wanted level (6 brown stars under HUD).
        /// </summary>
        /// <param name="playerid">The ID of the player to set the wanted level of.</param>
        /// <param name="level">The wanted level to set for the player (0-6).</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerWantedLevel(int playerid, int level);

        /// <summary>
        /// Gets the wanted level of a player.
        /// </summary>
        /// <param name="playerid">The ID of the player that you want to get the wanted level of.</param>
        /// <returns>The player's wanted level.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerWantedLevel(int playerid);

        /// <summary>
        /// Set a player's special fighting style. To use in-game, aim and press the 'secondary attack' key (ENTER by default).
        /// </summary>
        /// <remarks>
        /// This does not affect normal fist attacks - only special/secondary attacks (aim + press 'secondary attack' key).
        /// </remarks>
        /// <param name="playerid">The ID of player to set the fighting style of.</param>
        /// <param name="style">The fighting style that should be set.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerFightingStyle(int playerid, int style);

        /// <summary>
        /// Returns what fighting style the player currently using.
        /// </summary>
        /// <param name="playerid">The player you want to know the fighting style of.</param>
        /// <returns>Returns the fighting style of the player.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerFightingStyle(int playerid);

        /// <summary>
        /// Makes the player move in that direction at the given speed.
        /// </summary>
        /// <param name="playerid">The player to apply the speed to.</param>
        /// <param name="x">How much speed in the X direction will be applied.</param>
        /// <param name="y">How much speed in the Y direction will be applied.</param>
        /// <param name="z">How much speed in the Z direction will be applied.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerVelocity(int playerid, float x, float y, float z);

        /// <summary>
        /// Gets the velocity at which the player is moving in the three directions, X, Y and Z. This can be useful for speedometers.
        /// </summary>
        /// <param name="playerid">The player to get the speed from.</param>
        /// <param name="x">The float to store the X velocity in, passed by reference.</param>
        /// <param name="y">The float to store the Y velocity in, passed by reference.</param>
        /// <param name="z">The float to store the Z velocity in, passed by reference.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerVelocity(int playerid, out float x, out float y, out float z);

        /// <summary>
        /// This function plays a crime report for a player - just like in single-player when CJ commits a crime.
        /// </summary>
        /// <param name="playerid">The ID of the player that will hear the crime report.</param>
        /// <param name="suspectid">The ID of the suspect player which will be described in the crime report.</param>
        /// <param name="crime">The crime ID, which will be reported as a 10-code (i.e. 10-16 if 16 was passed as the crimeid).</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayCrimeReportForPlayer(int playerid, int suspectid, int crime);

        /// <summary>
        /// Play an 'audio stream' for a player. Normal audio files also work (e.g. MP3).
        /// </summary>
        /// <param name="playerid">The ID of the player to play the audio for.</param>
        /// <param name="url">The url to play. Valid formats are mp3 and ogg/vorbis. A link to a .pls (playlist) file will play that playlist.</param>
        /// <param name="posX">The X position at which to play the audio. Default 0.0. Has no effect unless usepos is set to True.</param>
        /// <param name="posY">The Y position at which to play the audio. Default 0.0. Has no effect unless usepos is set to True.</param>
        /// <param name="posZ">The Z position at which to play the audio. Default 0.0. Has no effect unless usepos is set to True.</param>
        /// <param name="distance">The distance over which the audio will be heard. Has no effect unless usepos is set to True.</param>
        /// <param name="usepos">Use the positions and distance specified. Default False.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayAudioStreamForPlayer(int playerid, string url, float posX, float posY, float posZ,
            float distance, bool usepos);

        /// <summary>
        /// Stops the current audio stream for a player.
        /// </summary>
        /// <param name="playerid">The player you want to stop the audio stream for.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool StopAudioStreamForPlayer(int playerid);

        /// <summary>
        /// Loads or unloads an interior script for a player. (for example the ammunation menu)
        /// </summary>
        /// <param name="playerid">The ID of the player to load the interior script for.</param>
        /// <param name="shopname"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerShopName(int playerid, string shopname);

        /// <summary>
        /// Set the skill level of a certain weapon type for a player.
        /// </summary>
        /// <remarks>
        /// The skill parameter is NOT the weapon ID, it is the skill type.
        /// </remarks>
        /// <param name="playerid">The ID of the player to set the weapon skill of.</param>
        /// <param name="skill">The weapon type you want to set the skill of.</param>
        /// <param name="level">The skill level to set for that weapon, ranging from 0 to 999. (A level out of range will max it out)</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerSkillLevel(int playerid, int skill, int level);

        /// <summary>
        /// Get the ID of the vehicle that the player is surfing.
        /// </summary>
        /// <param name="playerid">The ID of the player you want to know the surfing vehicle ID of.</param>
        /// <returns>The ID of the vehicle that the player is surfing, or <see cref="Misc.InvalidVehicleId"/> if they are not surfing or the vehicle has no driver.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerSurfingVehicleID(int playerid);

        /// <summary>
        /// Returns the ID of the object the player is surfing on.
        /// </summary>
        /// <param name="playerid">The ID of the player surfing the object.</param>
        /// <returns>The ID of the moving object the player is surfing. If the player isn't surfing a moving object, it will return <see cref="Misc.InvalidObjectId"/></returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerSurfingObjectID(int playerid);

        /// <summary>
        /// Removes a standard San Andreas model for a single player within a specified range.
        /// </summary>
        /// <param name="playerid">The ID of the player to remove the objects for.</param>
        /// <param name="modelid">The model to remove.</param>
        /// <param name="fX">The X coordinate around which the objects will be removed.</param>
        /// <param name="fY">The Y coordinate around which the objects will be removed.</param>
        /// <param name="fZ">The Z coordinate around which the objects will be removed.</param>
        /// <param name="fRadius">The radius. Objects within this radius from the coordinates above will be removed.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool RemoveBuildingForPlayer(int playerid, int modelid, float fX, float fY, float fZ,
            float fRadius);

        /// <summary>
        /// Attach an object to a specific bone on a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to attach the object to.</param>
        /// <param name="index">The index (slot) to assign the object to (0-9 since 0.3d).</param>
        /// <param name="modelid">The model to attach.</param>
        /// <param name="bone">The bone to attach the object to.</param>
        /// <param name="fOffsetX">X axis offset for the object position.</param>
        /// <param name="fOffsetY">Y axis offset for the object position.</param>
        /// <param name="fOffsetZ">Z axis offset for the object position.</param>
        /// <param name="fRotX">X axis rotation of the object.</param>
        /// <param name="fRotY">Y axis rotation of the object.</param>
        /// <param name="fRotZ">Z axis rotation of the object.</param>
        /// <param name="fScaleX">X axis scale of the object.</param>
        /// <param name="fScaleY">Y axis scale of the object.</param>
        /// <param name="fScaleZ">Z axis scale of the object.</param>
        /// <param name="materialcolor1">The first object color to set, as an integer or hex in ARGB color format.</param>
        /// <param name="materialcolor2">The second object color to set, as an integer or hex in ARGB color format.</param>
        /// <returns>True on success, False otherwise.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerAttachedObject(int playerid, int index, int modelid, int bone, float fOffsetX,
            float fOffsetY, float fOffsetZ, float fRotX, float fRotY, float fRotZ, float fScaleX, float fScaleY,
            float fScaleZ, int materialcolor1, int materialcolor2);

        /// <summary>
        /// Remove an attached object from a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to remove the object from.</param>
        /// <param name="index">The index of the object to remove (set with <see cref="SetPlayerAttachedObject"/>).</param>
        /// <returns>True on success, False otherwise.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool RemovePlayerAttachedObject(int playerid, int index);

        /// <summary>
        /// Check if a player has an object attached in the specified index (slot).
        /// </summary>
        /// <param name="playerid">The ID of the player to check.</param>
        /// <param name="index">The index (slot) to check.</param>
        /// <returns>True if the slot is used, False otherwise.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsPlayerAttachedObjectSlotUsed(int playerid, int index);

        /// <summary>
        /// Enter edition mode for an attached object.
        /// </summary>
        /// <param name="playerid">The ID of the player to enter in to edition mode.</param>
        /// <param name="index">The index (slot) of the attached object to edit.</param>
        /// <returns>True on success, False otherwise.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool EditAttachedObject(int playerid, int index);

        /// <summary>
        /// Creates a textdraw for a single player. This can be used as a way around the global text-draw limit.
        /// </summary>
        /// <param name="playerid">The ID of the player to create the textdraw for.</param>
        /// <param name="x">X-Coordinate.</param>
        /// <param name="y">Y-Coordinate.</param>
        /// <param name="text">The text in the textdraw.</param>
        /// <returns>The ID of the created textdraw.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int CreatePlayerTextDraw(int playerid, float x, float y, string text);

        /// <summary>
        /// Destroy a player-textdraw.
        /// </summary>
        /// <param name="playerid">The ID of the player who's player-textdraw to destroy.</param>
        /// <param name="text">The ID of the textdraw to destroy.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawDestroy(int playerid, int text);

        /// <summary>
        /// Sets the width and height of the letters in a player-textdraw.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player-textdraw to set the letter size of.</param>
        /// <param name="text">The ID of the player-textdraw to change the letter size of.</param>
        /// <param name="x">Width of a char.</param>
        /// <param name="y">Height of a char.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawLetterSize(int playerid, int text, float x, float y);

        /// <summary>
        /// Change the size of a player-textdraw (box if <see cref="PlayerTextDrawUseBox"/> is enabled and/or clickable area for use with <see cref="PlayerTextDrawSetSelectable"/>).
        /// </summary>
        /// <remarks>
        /// When used with <see cref="PlayerTextDrawAlignment"/> of alignment 3 (right), the x and y are the coordinates of the left most corner of the box. For alignment 2 (center) the x and y values need to inverted (switch the two) and the x value is the overall width of the box. For all other alignments the x and y coordinates are for the right most corner of the box.
        /// The TextDraw box starts 10.0 units up and 5.0 to the left as the origin (<see cref="TextDrawCreate"/> coordinate)
        /// This function defines the clickable area for use with <see cref="PlayerTextDrawSetSelectable"/>, whether a box is shown or not.
        /// </remarks>
        /// <param name="playerid">The ID of the player whose player-textdraw to set the size of.</param>
        /// <param name="text">The ID of the player-textdraw to set the size of.</param>
        /// <param name="x">he size on the X axis (left/right) following the same 640x480 grid as <see cref="TextDrawCreate"/>.</param>
        /// <param name="y">The size on the Y axis (up/down) following the same 640x480 grid as <see cref="TextDrawCreate"/>.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawTextSize(int playerid, int text, float x, float y);

        /// <summary>
        /// Set the text alignment of a player-textdraw.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player-textdraw to set the alignment of.</param>
        /// <param name="text">The ID of the player-textdraw to set the alignment of.</param>
        /// <param name="alignment">Algihnment of the player-textdraw, 1-left, 2-centered, 3-right.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawAlignment(int playerid, int text, int alignment);

        /// <summary>
        /// Sets the text color of a player-textdraw.
        /// </summary>
        /// <remarks>
        /// You can also use Gametext colors in textdraws.
        /// </remarks>
        /// <param name="playerid">The ID of the player who's textdraw to set the color of.</param>
        /// <param name="text">The TextDraw to change.</param>
        /// <param name="color">The color in hexadecimal format.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawColor(int playerid, int text, int color);

        /// <summary>
        /// Toggle the box on a player-textdraw.
        /// </summary>
        /// <param name="playerid">The ID of the player whose textdraw to toggle the box of.</param>
        /// <param name="text">The ID of the player-textdraw to toggle the box of.</param>
        /// <param name="use">True to use a box or False to not use a box.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawUseBox(int playerid, int text, bool use);

        /// <summary>
        /// Adjusts the text box colour (only used if <see cref="TextDrawUseBox"/> 'use' parameter is True).
        /// </summary>
        /// <remarks>
        /// <see cref="PlayerTextDrawUseBox"/> must be used in conjunction with this (duh).
        /// </remarks>
        /// <param name="playerid">The ID of the player who's textdraw to set the color of.</param>
        /// <param name="text">The TextDraw to change the box color of.</param>
        /// <param name="color">The color in hexadecimal format.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawBoxColor(int playerid, int text, int color);

        /// <summary>
        /// Adds a shadow to the lower right side of the text in a player-textdraw. The shadow font matches the text font. The shadow can be cut by the box area if the size is set too big for the area.
        /// </summary>
        /// <remarks>
        /// <see cref="PlayerTextDrawUseBox"/> must be used in conjunction with this (duh).
        /// </remarks>
        /// <param name="playerid">The ID of the player whose player-textdraw to set the shadow of.</param>
        /// <param name="text">The ID of the player-textdraw to change the shadow of.</param>
        /// <param name="size">The size of the shadow.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawSetShadow(int playerid, int text, int size);

        /// <summary>
        /// Set the outline of a player-textdraw. The outline colour cannot be changed unless <see cref="PlayerTextDrawBackgroundColor"/> is used.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player-textdraw to set the outline of.</param>
        /// <param name="text">The ID of the player-textdraw to set the outline of.</param>
        /// <param name="size">The thickness of the outline.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawSetOutline(int playerid, int text, int size);

        /// <summary>
        /// Adjust the background color of a player-textdraw.
        /// </summary>
        /// <remarks>
        /// If <see cref="PlayerTextDrawSetOutline"/> is used with size > 0, the outline color will match the color used in <see cref="PlayerTextDrawBackgroundColor"/>. 
        /// Changing the value of color seems to alter the color used in <see cref="PlayerTextDrawColor"/>.
        /// </remarks>
        /// <param name="playerid">The ID of the player whose player-textdraw to set the background color of.</param>
        /// <param name="text">The ID of the player-textdraw to set the background color of.</param>
        /// <param name="color">The color that the textdraw should be set to.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawBackgroundColor(int playerid, int text, int color);

        /// <summary>
        /// Change the font of a player-textdraw.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player-textdraw to change the font of.</param>
        /// <param name="text">The ID of the player-textdraw to change the font of</param>
        /// <param name="font">There are four font styles as shown below. A font value greater than 3 does not display, and anything greater than 16 crashes.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawFont(int playerid, int text, int font);

        /// <summary>
        /// Appears to scale text spacing to a proportional ratio. Useful when using PlayerTextDrawLetterSize to ensure the text has even character spacing.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player-textdraw to set the proportionality of.</param>
        /// <param name="text">The ID of the player-textdraw to set the proportionality of.</param>
        /// <param name="set">True to enable proportionality, False to disable.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawSetProportional(int playerid, int text, bool set);

        /// <summary>
        /// Toggles whether a player-textdraw can be selected or not.
        /// </summary>
        /// <remarks>
        /// <see cref="PlayerTextDrawSetSelectable"/> MUST be used BEFORE the textdraw is shown to the player.
        /// </remarks>
        /// <param name="playerid">The ID of the player whose player-textdraw to make selectable.</param>
        /// <param name="text">The ID of the player-textdraw to set the selectability of.</param>
        /// <param name="set">et the player-textdraw selectable (True) or non-selectable (False). By default this is False.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawSetSelectable(int playerid, int text, bool set);

        /// <summary>
        /// Show a player-textdraw to the player it was created for.
        /// </summary>
        /// <param name="playerid">The ID of the player to show the textdraw for.</param>
        /// <param name="text">The ID of the textdraw to show.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawShow(int playerid, int text);

        /// <summary>
        /// Hide a player-textdraw from the player it was created for.
        /// </summary>
        /// <param name="playerid">The ID of the player to hide the textdraw for.</param>
        /// <param name="text">The ID of the textdraw to hide.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawHide(int playerid, int text);

        /// <summary>
        /// Change the text of a player-textdraw.
        /// </summary>
        /// <remarks>
        /// Although the textdraw string limit is 1024 characters, if colour codes (e.g. ~r~) are used beyond the 255th character it may crash the client.
        /// </remarks>
        /// <param name="playerid">The ID of the player who's textdraw string to set.</param>
        /// <param name="text">The ID of the textdraw to change.</param>
        /// <param name="str">The new string for the TextDraw.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawSetString(int playerid, int text, string str);

        /// <summary>
        /// Sets a player textdraw 2D preview sprite of a specified model ID.
        /// </summary>
        /// <param name="playerid">The PlayerTextDraw player ID.</param>
        /// <param name="text">The textdraw id that will display the 3D preview.</param>
        /// <param name="modelindex">The model ID to display.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawSetPreviewModel(int playerid, int text, int modelindex);

        /// <summary>
        /// Sets the rotation and zoom of a 3D model preview player-textdraw.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player-textdraw to change.</param>
        /// <param name="text">The ID of the player-textdraw to change.</param>
        /// <param name="fRotX">The X rotation value.</param>
        /// <param name="fRotY">The Y rotation value.</param>
        /// <param name="fRotZ">The Z rotation value.</param>
        /// <param name="fZoom">The zoom value, default value 1.0, smaller values make the camera closer and larger values make the camera further away</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawSetPreviewRot(int playerid, int text, float fRotX, float fRotY,
            float fRotZ, float fZoom);

        /// <summary>
        /// Set the color of a vehicle in a player-textdraw model preview (if a vehicle is shown).
        /// </summary>
        /// <param name="playerid">The ID of the player whose player-textdraw to change.</param>
        /// <param name="text">The ID of the player's player-textdraw to change.</param>
        /// <param name="color1">The color to set the vehicle's primary color to.</param>
        /// <param name="color2">The color to set the vehicle's secondary color to.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerTextDrawSetPreviewVehCol(int playerid, int text, int color1, int color2);

        /// <summary>
        /// Sets an integer to a player variable.
        /// </summary>
        /// <remarks>
        /// Variables aren't reset until after <see cref="OnPlayerDisconnect"/> is called, so the values are still accessible in <see cref="OnPlayerDisconnect"/>.
        /// </remarks>
        /// <param name="playerid">The ID of the player whose player variable will be set.</param>
        /// <param name="varname">The name of the player variable.</param>
        /// <param name="value">The integer to be set.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPVarInt(int playerid, string varname, int value);

        /// <summary>
        /// Gets a player variable as an integer.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player variable to get.</param>
        /// <param name="varname">The name of the player variable. (case-insensitive)</param>
        /// <returns>The integer value from the specified player variable.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPVarInt(int playerid, string varname);

        /// <summary>
        /// Saves a string into a player variable.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player variable will be set.</param>
        /// <param name="varname">The name of the player variable.</param>
        /// <param name="value">The string you want to save in the player variable.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPVarString(int playerid, string varname, string value);

        /// <summary>
        /// Gets a player variable as a string.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player variable to get.</param>
        /// <param name="varname">The name of the player variable, set by <see cref="SetPVarString"/>.</param>
        /// <param name="value">The array in which to store the string value in, passed by reference.</param>
        /// <param name="size">The maximum length of the returned string.</param>
        /// <returns>The length of the string.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPVarString(int playerid, string varname, out string value, int size);

        /// <summary>
        /// Saves a float variable into a player variable.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player variable will be set.</param>
        /// <param name="varname">The name of the player variable.</param>
        /// <param name="value">The float you want to save in the player variable.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPVarFloat(int playerid, string varname, float value);

        /// <summary>
        /// Gets a player variable as a float.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player variable you want to get.</param>
        /// <param name="varname">The name of the player variable.</param>
        /// <returns>The float from the specified player variable.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern float GetPVarFloat(int playerid, string varname);

        /// <summary>
        /// Deletes a previously set player variable.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player variable to delete.</param>
        /// <param name="varname">The name of the player variable to delete.</param>
        /// <returns>True on success, False on failure (pVar not set or player not connected).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool DeletePVar(int playerid, string varname);

        /// <summary>
        /// Each PVar (player-variable) has its own unique identification number for lookup, this function returns the highest ID set for a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to get the upper PVar index of..</param>
        /// <returns>The highest set PVar ID.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPVarsUpperIndex(int playerid);

        /// <summary>
        /// Retrieve the name of a player's pVar via the index.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player variable to get the name of.</param>
        /// <param name="index">The index of the player's pVar.</param>
        /// <param name="varname">A string to store the pVar's name in, passed by reference.</param>
        /// <param name="size">The max length of the returned string</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPVarNameAtIndex(int playerid, int index, out string varname, int size);

        /// <summary>
        /// Gets the type (integer, float or string) of a player variable.
        /// </summary>
        /// <param name="playerid">The ID of the player whose player variable to get the type of.</param>
        /// <param name="varname">The name of the player variable to get the type of.</param>
        /// <returns>Returns the type of the PVar.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPVarType(int playerid, string varname);

        /// <summary>
        /// Creates a chat bubble above a player's name tag.
        /// </summary>
        /// <param name="playerid">The player which should have the chat bubble.</param>
        /// <param name="text">The text to display.</param>
        /// <param name="color">The text color.</param>
        /// <param name="drawdistance">The distance from where players are able to see the chat bubble.</param>
        /// <param name="expiretime">The time in miliseconds the bubble should be displayed for.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerChatBubble(int playerid, string text, int color, float drawdistance,
            int expiretime);

        /// <summary>
        /// Puts a player in a vehicle
        /// </summary>
        /// <param name="playerid">The ID of the player to put in a vehicle.</param>
        /// <param name="vehicleid">The ID of the vehicle for the player to be put in.</param>
        /// <param name="seatid">The ID of the seat to put the player in.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PutPlayerInVehicle(int playerid, int vehicleid, int seatid);

        /// <summary>
        /// This function gets the ID of the vehicle the player is currently in. Note: NOT the model id of the vehicle. See <see cref="GetVehicleModel"/> for that.
        /// </summary>
        /// <param name="playerid">The ID of the player in the vehicle that you want to get the ID of.</param>
        /// <returns>ID of the vehicle or 0 if not in a vehicle.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerVehicleID(int playerid);

        /// <summary>
        /// Find out what seat a player is in.
        /// </summary>
        /// <remarks>
        /// Sometimes the result can be 128 which is an invalid seat ID. Circumstances of this are not yet known, but it is best to discard information when returned seat number is 128.
        /// </remarks>
        /// <param name="playerid">The ID of the player you want to get the seat of.</param>
        /// <returns>Seat ID (-1 if the player is not in a vehicle, 0 driver, 1 co-driver, 2&3 back seat passengers, 4+ if the vehicle has enough seats (i.e coach)).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerVehicleSeat(int playerid);

        /// <summary>
        /// Removes/ejects a player from their vehicle.
        /// </summary>
        /// <remarks>
        /// The exiting animation is not synced for other players.
        /// This function will not work when used in <see cref="OnPlayerEnterVehicle"/>, because the player isn't in the vehicle when the callback is called. Use <see cref="OnPlayerStateChange"/> instead.
        /// </remarks>
        /// <param name="playerid">The ID of the player to remove from their vehicle.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool RemovePlayerFromVehicle(int playerid);

        /// <summary>
        /// Toggles whether a player can control themselves, basically freezes them.
        /// </summary>
        /// <param name="playerid">The ID of the player to toggle the controllability of.</param>
        /// <param name="toggle">False to freeze the player or True to unfreeze them.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TogglePlayerControllable(int playerid, bool toggle);

        /// <summary>
        /// Plays the specified sound for a player.
        /// </summary>
        /// <remarks>
        /// Only use the coordinates if you want the sound to be played at a certain position. Set coordinates all to 0 to just play the sound.
        /// </remarks>
        /// <param name="playerid">The ID of the player for whom to play the sound.</param>
        /// <param name="soundid">The sound to play.</param>
        /// <param name="x">X coordinate for the sound to play at. (0 for no position)</param>
        /// <param name="y">Y coordinate for the sound to play at. (0 for no position)</param>
        /// <param name="z">Z coordinate for the sound to play at. (0 for no position)</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerPlaySound(int playerid, int soundid, float x, float y, float z);

        /// <summary>
        /// Apply an animation to a player.
        /// </summary>
        /// <remarks>
        /// The <paramref name="forcesync"/> parameter, which defaults to False, in most cases is not needed since players sync animations themselves. The <paramref name="forcesync"/> parameter can force all players who can see <paramref name="playerid"/> to play the animation regardless of whether the player is performing that animation. This is useful in circumstances where the player can't sync the animation themselves. For example, they may be paused.
        /// </remarks>
        /// <param name="playerid">The ID of the player to apply the animation to.</param>
        /// <param name="animlib">The name of the animation library in which the animation to apply is in.</param>
        /// <param name="animname">The name of the animation, within the library specified.</param>
        /// <param name="fDelta">The speed to play the animation (use 4.1).</param>
        /// <param name="loop">Set to True for looping otherwise set to False for playing animation sequence only once.</param>
        /// <param name="lockx">Set to False to return player to original x position after animation is complete for moving animations. The opposite effect occurs if set to True.</param>
        /// <param name="locky">Set to False to return player to original y position after animation is complete for moving animations. The opposite effect occurs if set to True.</param>
        /// <param name="freeze">Will freeze the player in position after the animation finishes.</param>
        /// <param name="time">Timer in milliseconds. For a never ending loop it should be 0.</param>
        /// <param name="forcesync">Set to True to force playerid to sync animation with other players in all instances</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ApplyAnimation(int playerid, string animlib, string animname, float fDelta, bool loop,
            bool lockx, bool locky, bool freeze, int time, bool forcesync);

        /// <summary>
        /// Clears all animations for the given player.
        /// </summary>
        /// <param name="playerid">The ID of the player to clear the animations of.</param>
        /// <param name="forcesync">Specifies whether the animation should be shown to streamed in players.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ClearAnimations(int playerid, bool forcesync);

        /// <summary>
        /// Returns the index of any running applied animations.
        /// </summary>
        /// <param name="playerid">ID of the player of whom you want to get the animation index of.</param>
        /// <returns>0 if there is no animation applied, otherwise the index of the playing animation.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerAnimationIndex(int playerid);

        /// <summary>
        /// Get the animation library/name for the index.
        /// </summary>
        /// <param name="index">The animation index, returned by <see cref="GetPlayerAnimationIndex"/>.</param>
        /// <param name="animlib">String variable that stores the animation library.</param>
        /// <param name="animlibSize">Size of the string that stores the animation library.</param>
        /// <param name="animname">String variable that stores the animation name.</param>
        /// <param name="animnameSize">Size of the string that stores the animation name.</param>
        /// <returns>True on success, False otherwise.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetAnimationName(int index, out string animlib, int animlibSize, out string animname,
            int animnameSize);

        /// <summary>
        /// Retrieves a player's current special action.
        /// </summary>
        /// <param name="playerid">The ID of the player to get the special action of.</param>
        /// <returns>The special action of the player.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerSpecialAction(int playerid);

        /// <summary>
        /// This Function allows to set players special action.
        /// </summary>
        /// <param name="playerid">The player that should perform the action.</param>
        /// <param name="actionid">The action that should be performed.</param>ob
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerSpecialAction(int playerid, int actionid);

        /// <summary>
        /// Sets a checkpoint (red circle) for a player. Also shows a red blip on the radar.
        /// </summary>
        /// <remarks>
        /// Checkpoints created on server-created objects (<see cref="CreateObject"/>/<see cref="CreatePlayerObject"/>) will appear down on the 'real' ground, but will still function correctly. There is no fix available for this issue. A pickup can be used instead.
        /// </remarks>
        /// <param name="playerid">The ID of the player to set the checkpoint of.</param>
        /// <param name="x">The X coordinate to set the checkpoint at.</param>
        /// <param name="y">The Y coordinate to set the checkpoint at.</param>
        /// <param name="z">The Z coordinate to set the checkpoint at.</param>
        /// <param name="size">The size of the checkpoint.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerCheckpoint(int playerid, float x, float y, float z, float size);

        /// <summary>
        /// Disable any initialized checkpoints for a specific player, since you can only have one at any given time.
        /// </summary>
        /// <param name="playerid">The player to disable the current checkpoint for.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool DisablePlayerCheckpoint(int playerid);

        /// <summary>
        /// Creates a race checkpoint. When the player enters it, the OnPlayerEnterRaceCheckpoint callback is called.
        /// </summary>
        /// <param name="playerid">The ID of the player to set the checkpoint for.</param>
        /// <param name="type">Type of checkpoint.0-Normal, 1-Finish, 2-Nothing(Only the checkpoint without anything on it), 3-Air normal, 4-Air finish.</param>
        /// <param name="x">X-Coordinate.</param>
        /// <param name="y">X-Coordinate.</param>
        /// <param name="z">X-Coordinate.</param>
        /// <param name="nextx">X-Coordinate of the next point, for the arrow facing direction.</param>
        /// <param name="nexty">X-Coordinate of the next point, for the arrow facing direction.</param>
        /// <param name="nextz">X-Coordinate of the next point, for the arrow facing direction.</param>
        /// <param name="size">Size (diameter) of the checkpoint</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerRaceCheckpoint(int playerid, int type, float x, float y, float z, float nextx,
            float nexty, float nextz, float size);

        /// <summary>
        /// Disable any initialized race checkpoints for a specific player, since you can only have one at any given time.
        /// </summary>
        /// <param name="playerid">The player to disable the current checkpoint for.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool DisablePlayerRaceCheckpoint(int playerid);

        /// <summary>
        /// Set the world boundaries for a player - players can not go out of the boundaries.
        /// </summary>
        /// <remarks>
        /// You can reset the player world bounds by setting the parameters to 20000.0000, -20000.0000, 20000.0000, -20000.0000.
        /// </remarks>
        /// <param name="playerid">The ID of the player to set the boundaries of.</param>
        /// <param name="xMax">The maximum X coordinate the player can go to.</param>
        /// <param name="xMin">The minimum X coordinate the player can go to.</param>
        /// <param name="yMax">The maximum Y coordinate the player can go to.</param>
        /// <param name="yMin">The minimum Y coordinate the player can go to.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerWorldBounds(int playerid, float xMax, float xMin, float yMax, float yMin);

        /// <summary>
        /// Change the colour of a player's nametag and radar blip for another player.
        /// </summary>
        /// <param name="playerid">The player that will see the player's changed blip/nametag color.</param>
        /// <param name="showplayerid">The player whose color will be changed.</param>
        /// <param name="color">New color. Set to 0 for an invisible blip.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerMarkerForPlayer(int playerid, int showplayerid, int color);

        /// <summary>
        /// This functions allows you to toggle the drawing of player nametags, healthbars and armor bars which display above their head. For use of a similar function like this on a global level, <see cref="ShowNameTags"/> function.
        /// </summary>
        /// <remarks>
        /// <see cref="ShowNameTags"/> must be set to True to be able to show name tags with <see cref="ShowPlayerNameTagForPlayer"/>.
        /// </remarks>
        /// <param name="playerid">Player who will see the results of this function.</param>
        /// <param name="showplayerid">Player whose name tag will be shown or hidden.</param>
        /// <param name="show">True to show name tag, False to hide name tag.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ShowPlayerNameTagForPlayer(int playerid, int showplayerid, bool show);

        /// <summary>
        /// This function allows you to place your own icons on the map, enabling you to emphasise the locations of banks, airports or whatever else you want. A total of 63 icons are available in GTA: San Andreas, all of which can be used using this function. You can also specify the color of the icon, which allows you to change the square icon (ID: 0).
        /// </summary>
        /// <param name="playerid">The ID of the player to set the map icon for.</param>
        /// <param name="iconid">The player's icon ID, ranging from 0 to 99, to be used in RemovePlayerMapIcon.</param>
        /// <param name="x">The X coordinate of the place where you want the icon to be.</param>
        /// <param name="y">The Y coordinate of the place where you want the icon to be.</param>
        /// <param name="z">The Z coordinate of the place where you want the icon to be.</param>
        /// <param name="markertype">The icon to set.</param>
        /// <param name="color">The color of the icon, this should only be used with the square icon (ID: 0).</param>
        /// <param name="style">The style of icon.</param>
        /// <returns>True if it was successful, False otherwise (e.g. the player isn't connected).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerMapIcon(int playerid, int iconid, float x, float y, float z, int markertype,
            int color, int style);

        /// <summary>
        /// Removes a map icon that was set earlier for a player.
        /// </summary>
        /// <param name="playerid">The ID of the player whose icon to remove.</param>
        /// <param name="iconid">The ID of the icon to remove. This is the second parameter of <see cref="SetPlayerMapIcon"/>.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool RemovePlayerMapIcon(int playerid, int iconid);

        /// <summary>
        /// Enable/Disable the teleporting ability for a player by right-clicking on the map.
        /// </summary>
        /// <remarks>
        /// DEPRICATED!
        /// This function will work only if <see cref="AllowAdminTeleport"/> is working, and you have to be an admin.
        /// </remarks>
        /// <param name="playerid">playerid</param>
        /// <param name="allow">True-allow, False-disallow</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        [Obsolete("This function is deprecated. Use the OnPlayerClickMap callback instead.")]
        public static extern bool AllowPlayerTeleport(int playerid, bool allow);

        /// <summary>
        /// Sets the camera to a specific position for a player.
        /// </summary>
        /// <param name="playerid">ID of the player.</param>
        /// <param name="x">New x-position of the camera.</param>
        /// <param name="y">New y-position of the camera.</param>
        /// <param name="z">New z-position of the camera.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerCameraPos(int playerid, float x, float y, float z);

        /// <summary>
        /// Set the direction a player's camera looks at. To be used in combination with SetPlayerCameraPos.
        /// </summary>
        /// <param name="playerid">The player to change the camera of.</param>
        /// <param name="x">The X coordinate for the player's camera to look at.</param>
        /// <param name="y">The Y coordinate for the player's camera to look at.</param>
        /// <param name="z">The Z coordinate for the player's camera to look at.</param>
        /// <param name="cut">The style the camera-position changes.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerCameraLookAt(int playerid, float x, float y, float z, int cut);

        /// <summary>
        /// Restore the camera to a place behind the player, after using a function like SetPlayerCameraPos.
        /// </summary>
        /// <param name="playerid">The player you want to restore the camera for.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetCameraBehindPlayer(int playerid);

        /// <summary>
        /// Get the position of the player's camera.
        /// </summary>
        /// <remarks> 
        /// Player's camera positions are only updated once a second, unless aiming.
        /// </remarks>
        /// <param name="playerid">The ID of the player to get the camera position of.</param>
        /// <param name="x">A float variable to store the X coordinate in, passed by reference.</param>
        /// <param name="y">A float variable to store the Y coordinate in, passed by reference.</param>
        /// <param name="z">A float variable to store the Z coordinate in, passed by reference.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerCameraPos(int playerid, out float x, out float y, out float z);

        /// <summary>
        /// This function will return the current direction of player's aiming in 3-D space, the coords are relative to the camera position, see <see cref="GetPlayerCameraPos"/>.
        /// </summary>
        /// <param name="playerid">The ID of the player you want to obtain the camera front vector of.</param>
        /// <param name="x">A float to store the X coordinate, passed by reference.</param>
        /// <param name="y">A float to store the Y coordinate, passed by reference.</param>
        /// <param name="z">A float to store the Z coordinate, passed by reference.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerCameraFrontVector(int playerid, out float x, out float y, out float z);

        /// <summary>
        /// Returns the current GTA camera mode for the requested player. The camera modes are useful in determining whether a player is aiming, doing a passenger driveby etc
        /// </summary>
        /// <param name="playerid">The ID of the player whose camera mode to retrieve</param>
        /// <returns>The camera mode as an integer (or -1 if player is not connected)</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerCameraMode(int playerid);

        /// <summary>
        /// You can use this function to attach the player camera to objects.
        /// </summary>
        /// <remarks>
        /// You need to create the object first, before attempting to attach a player camera for that.
        /// </remarks>
        /// <param name="playerid">The ID of the player which will have your camera attached on object.</param>
        /// <param name="objectid">The object id which you want to attach the player camera.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool AttachCameraToObject(int playerid, int objectid);

        /// <summary>
        /// Attaches a player's camera to a player-object. They are able to move their camera while it is attached to an object. Can be used with <see cref="MovePlayerObject"/> and <see cref="AttachPlayerObjectToVehicle"/>.
        /// </summary>
        /// <param name="playerid">The ID of the player which will have their camera attached to a player-object.</param>
        /// <param name="playerobjectid">	The ID of the player-object to which the player's camera will be attached.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool AttachCameraToPlayerObject(int playerid, int playerobjectid);

        /// <summary>
        /// Move a player's camera from one position to another, within the set time.
        /// </summary>
        /// <param name="playerid">The ID of the player the camera should be moved for.</param>
        /// <param name="fromX">The X position the camera should start to move from.</param>
        /// <param name="fromY">The Y position the camera should start to move from.</param>
        /// <param name="fromZ">The Z position the camera should start to move from.</param>
        /// <param name="toX">The X position the camera should move to.</param>
        /// <param name="toY">The Y position the camera should move to.</param>
        /// <param name="toZ">The Z position the camera should move to.</param>
        /// <param name="time">Time in milliseconds.</param>
        /// <param name="cut">The jumpcut to use. Defaults to CameraCut.Cut. Set to CameraCut.Move for a smooth movement.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool InterpolateCameraPos(int playerid, float fromX, float fromY, float fromZ, float toX,
            float toY, float toZ, int time, int cut);

        /// <summary>
        /// Interpolate a player's camera's 'look at' point between two coordinates with a set speed. Can be be used with <see cref="InterpolateCameraPos"/>.
        /// </summary>
        /// <param name="playerid">The ID of the player the camera should be moved for.</param>
        /// <param name="fromX">The X position the camera should start to move from.</param>
        /// <param name="fromY">The Y position the camera should start to move from.</param>
        /// <param name="fromZ">The Z position the camera should start to move from.</param>
        /// <param name="toX">The X position the camera should move to.</param>
        /// <param name="toY">The Y position the camera should move to.</param>
        /// <param name="toZ">The Z position the camera should move to.</param>
        /// <param name="time">Time in milliseconds to complete interpolation.</param>
        /// <param name="cut">The 'jumpcut' to use. Defaults to CameraCut.Cut (pointless). Set to CameraCut.Move for interpolation.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool InterpolateCameraLookAt(int playerid, float fromX, float fromY, float fromZ, float toX,
            float toY, float toZ, int time, int cut);

        /// <summary>
        /// This function can be used to check if a player is connected to the server via SA:MP.
        /// </summary>
        /// <remarks>This function can be omitted in a lot of cases. Many other natives already have some sort of connection check built in.</remarks>
        /// <param name="playerid">The playerid you would like to check.</param>
        /// <returns>Returns true if the player is connected and false if the player is not.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsPlayerConnected(int playerid);

        /// <summary>
        /// Checks if a player is in a specific vehicle.
        /// </summary>
        /// <param name="playerid">ID of the player.</param>
        /// <param name="vehicleid">ID of the vehicle.</param>
        /// <returns>True if player is in the vehicle, otherwise False.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsPlayerInVehicle(int playerid, int vehicleid);

        /// <summary>
        /// Check if a player is inside any vehicle.
        /// </summary>
        /// <param name="playerid">The ID of the player to check.</param>
        /// <returns>True if player is in a vehicle, otherwise False.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsPlayerInAnyVehicle(int playerid);

        /// <summary>
        /// Check if the player is currently inside a checkpoint, this could be used for properties or teleport points for example.
        /// </summary>
        /// <param name="playerid">The player you want to know the status of.</param>
        /// <returns>True if player is in his checkpoint, otherwise False.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsPlayerInCheckpoint(int playerid);

        /// <summary>
        /// Check if the player is inside their current set race checkpoint (<see cref="SetPlayerRaceCheckpoint"/>).
        /// </summary>
        /// <param name="playerid">The ID of the player to check.</param>
        /// <returns>True if player is in his checkpoint, otherwise False.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsPlayerInRaceCheckpoint(int playerid);

        /// <summary>
        /// Set the virtual world of a player. They can only see other players or vehicles if they are in that same world.
        /// </summary>
        /// <remarks>
        /// The default virtual world is 0.
        /// </remarks>
        /// <param name="playerid">The ID of the player you want to set the virtual world of.</param>
        /// <param name="worldid">The virtual world ID to put the player in.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetPlayerVirtualWorld(int playerid, int worldid);

        /// <summary>
        /// Retrieves the current virtual world the player is in. Note this is not the same as the interior.
        /// </summary>
        /// <param name="playerid">The ID of the player to get the virtual world of.</param>
        /// <returns>The ID of the world the player is currently in.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerVirtualWorld(int playerid);

        /// <summary>
        /// Toggle stunt bonuses for a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to toggle stunt bonuses for.</param>
        /// <param name="enable">True to enable stunt bonuses, False to disable them.</param>
        /// <returns>This function doesn't return a specific value</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool EnableStuntBonusForPlayer(int playerid, bool enable);

        /// <summary>
        /// Enables or disables stunt bonuses for all players.
        /// </summary>
        /// <param name="enable">True to enable stunt bonuses, False to disable.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool EnableStuntBonusForAll(bool enable);

        /// <summary>
        /// Toggle a player's spectate mode.
        /// </summary>
        /// <remarks>
        /// When the spectating is turned off, OnPlayerSpawn will automatically be called.
        /// </remarks>
        /// <param name="playerid">The ID of the player who should spectate.</param>
        /// <param name="toggle">True to enable spectating and False to disable.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TogglePlayerSpectating(int playerid, bool toggle);

        /// <summary>
        /// Makes a player spectate (watch) another player.
        /// </summary>
        /// <remarks>
        /// Order is CRITICAL! Ensure that you use <see cref="TogglePlayerSpectating"/> before <see cref="PlayerSpectatePlayer"/>.
        /// </remarks>
        /// <param name="playerid">The ID of the player that will spectate.</param>
        /// <param name="targetplayerid">The ID of the player that should be spectated.</param>
        /// <param name="mode">The mode to spectate with.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerSpectatePlayer(int playerid, int targetplayerid, int mode);

        /// <summary>
        /// Sets a player to spectate another vehicle, i.e. see what its driver sees.
        /// </summary>
        /// <remarks>
        /// Order is CRITICAL! Ensure that you use <see cref="TogglePlayerSpectating"/> before <see cref="PlayerSpectatePlayer"/>.
        /// </remarks>
        /// <param name="playerid">Player ID.</param>
        /// <param name="targetvehicleid">ID of the vehicle to spectate.</param>
        /// <param name="mode">Spectate mode.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool PlayerSpectateVehicle(int playerid, int targetvehicleid, int mode);

        /// <summary>
        /// Starts recording the player's movements to a file, which can then be reproduced by an NPC.
        /// </summary>
        /// <param name="playerid">The ID of the player you want to record.</param>
        /// <param name="recordtype">The type of recording.</param>
        /// <param name="recordname">Name of the file which will hold the recorded data. It will be saved in scriptfiles, with an automatically added .rec extension.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool StartRecordingPlayerData(int playerid, int recordtype, string recordname);

        /// <summary>
        /// Stops all the recordings that had been started with <see cref="StartRecordingPlayerData"/> for a specific player.
        /// </summary>
        /// <param name="playerid">The player you want to stop the recordings of.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool StopRecordingPlayerData(int playerid);

        #endregion

        #region a_samp natives

        /// <summary>
        /// This function sends a message to a specific player with a chosen color in the chat. The whole line in the chatbox will be in the set color unless colour embedding is used.<br />
        /// </summary>
        /// <param name="playerid">The ID of the player to display the message to.</param>
        /// <param name="color">The color of the message.</param>
        /// <param name="message">The text that will be displayed (max 144 characters).</param>
        /// <returns>True: The function was successful (the message was sucessfully displayed (NOTE: success will be returned even if the message is too long (more than 144 characters) and fails to be sent)). False: The function failed (The message was not displayed (player not connected?)).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SendClientMessage(int playerid, int color, string message);

        /// <summary>
        /// Displays a message in chat to all players. This is a multi-player equivalent of <see cref="SendClientMessage"/>.<br />
        /// </summary>
        /// <param name="color">The color of the message (RGBA Hex format).</param>
        /// <param name="message">The message to show (max 144 characters).</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SendClientMessageToAll(int color, string message);

        /// <summary>
        /// Sends a message in the name of a player to another player on the server. The message will appear in the chat box but can only be seen by the user specified with <paramref name="playerid"/>. The line will start with the <paramref name="senderid"/>'s name in his color, followed by the <paramref name="message"/> in white.
        /// </summary>
        /// <param name="playerid">The ID of the player who will recieve the message</param>
        /// <param name="senderid">The sender's ID. If invalid, the message will not be sent.</param>
        /// <param name="message">The message that will be sent.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SendPlayerMessageToPlayer(int playerid, int senderid, string message);

        /// <summary>
        /// Sends a message in the name of a player to all other players on the server. The line will start with the <paramref name="senderid"/>'s name in their color, followed by the <paramref name="message"/> in white.
        /// </summary>
        /// <param name="senderid">The ID of the sender. If invalid, the message will not be sent.</param>
        /// <param name="message">The message that will be sent.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SendPlayerMessageToAll(int senderid, string message);

        /// <summary>
        /// Adds a death to the 'killfeed' on the right-hand side of the screen.
        /// </summary>
        /// <param name="killer">The ID of the killer (can be <see cref="Misc.InvalidPlayerId"/>).</param>
        /// <param name="killee">The ID of the player that died.</param>
        /// <param name="weapon">The reason (not always a weapon) for the <paramref name="killee"/>'s death. Special icons can also be used (ICON_CONNECT and ICON_DISCONNECT).</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SendDeathMessage(int killer, int killee, int weapon);

        /// <summary>
        /// Shows 'game text' (on-screen text) for a certain length of time for all players.
        /// </summary>
        /// <param name="text">The text to be displayed.</param>
        /// <param name="time">The duration of the text being shown in milliseconds.</param>
        /// <param name="style">The style of text to be displayed.</param>
        /// <returns>This function always returns 1.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GameTextForAll(string text, int time, int style);

        /// <summary>
        /// Shows 'game text' (on-screen text) for a certain length of time for a specific player.
        /// </summary>
        /// <param name="playerid">The ID of the player to show the gametext for.</param>
        /// <param name="text">The text to be displayed.</param>
        /// <param name="time">The duration of the text being shown in milliseconds.</param>
        /// <param name="style">The style of text to be displayed.</param>
        /// <returns>True on success, otherwise False.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GameTextForPlayer(int playerid, string text, int time, int style);

        /// <summary>
        /// Returns the uptime of the actual server in milliseconds.
        /// </summary>
        /// <returns>Uptime of the SA:MP server(NOT the physical box).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetTickCount();

        /// <summary>
        /// Returns the maximum number of players that can join the server, as set by the server var 'maxplayers' in server.cfg.
        /// </summary>
        /// <returns>The maximum number of players that can join the server.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetMaxPlayers();

        /// <summary>
        /// Set the name of the game mode, which appears in the server browser.
        /// </summary>
        /// <param name="text">GameMode name.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetGameModeText(string text);

        /// <summary>
        /// This function is used to change the amount of teams used in the gamemode. It has no obvious way of being used, but can help to indicate the number of teams used for better (more effective) internal handling. This function should only be used in the <see cref="OnGameModeInit"/> callback.
        /// </summary>
        /// <remarks>
        /// You can pass 2 billion here if you like, this function has no effect at all.
        /// </remarks>
        /// <param name="count">Number of teams the gamemode knows.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetTeamCount(int count);

        /// <summary>
        /// Adds a class to class selection. Classes are used so players may spawn with a skin of their choice.
        /// </summary>
        /// <param name="modelid">The skin which the player will spawn with.</param>
        /// <param name="spawnX">The X coordinate of the spawnpoint of this class.</param>
        /// <param name="spawnY">The Y coordinate of the spawnpoint of this class.</param>
        /// <param name="spawnZ">The Z coordinate of the spawnpoint of this class.</param>
        /// <param name="zAngle">The direction in which the player should face after spawning.</param>
        /// <param name="weapon1">The first spawn-weapon for the player.</param>
        /// <param name="weapon1Ammo">The amount of ammunition for the primary spawnweapon.</param>
        /// <param name="weapon2">The second spawn-weapon for the player.</param>
        /// <param name="weapon2Ammo">The amount of ammunition for the second spawnweapon.</param>
        /// <param name="weapon3">The third spawn-weapon for the player.</param>
        /// <param name="weapon3Ammo">The amount of ammunition for the third spawnweapon.</param>
        /// <returns>The ID of the class which was just added. 300 if the class limit (300) was reached. The highest possible class ID is 299.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int AddPlayerClass(int modelid, float spawnX, float spawnY, float spawnZ, float zAngle,
            int weapon1, int weapon1Ammo, int weapon2, int weapon2Ammo, int weapon3, int weapon3Ammo);

        /// <summary>
        /// This function is exactly the same as the <see cref="AddPlayerClass"/> function, with the addition of a team parameter.
        /// </summary>
        /// <param name="teamid">The team you want the player to spawn in.</param>
        /// <param name="modelid">The skin which the player will spawn with.</param>
        /// <param name="spawnX">The X coordinate of the class' spawn position.</param>
        /// <param name="spawnY">The Y coordinate of the class' spawn position.</param>
        /// <param name="spawnZ">The Z coordinate of the class' spawn position.</param>
        /// <param name="zAngle">The direction in which the player will face after spawning.</param>
        /// <param name="weapon1">The first spawn-weapon for the player.</param>
        /// <param name="weapon1Ammo">The amount of ammunition for the first spawnweapon.</param>
        /// <param name="weapon2">The second spawn-weapon for the player.</param>
        /// <param name="weapon2Ammo">The amount of ammunition for the second spawnweapon.</param>
        /// <param name="weapon3">The third spawn-weapon for the player.</param>
        /// <param name="weapon3Ammo">The amount of ammunition for the third spawnweapon.</param>
        /// <returns>The ID of the class that was just created.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int AddPlayerClassEx(int teamid, int modelid, float spawnX, float spawnY, float spawnZ,
            float zAngle, int weapon1, int weapon1Ammo, int weapon2, int weapon2Ammo, int weapon3, int weapon3Ammo);

        /// <summary>
        /// Adds a 'static' vehicle (models are pre-loaded for players)to the gamemode. Can only be used when the server first starts (in <see cref="OnGameModeInit"/>).
        /// </summary>
        /// <param name="modelid">The Model ID for the vehicle.</param>
        /// <param name="spawnX">The X-coordinate for the vehicle.</param>
        /// <param name="spawnY">The Y-coordinate for the vehicle.</param>
        /// <param name="spawnZ">The Z-coordinate for the vehicle.</param>
        /// <param name="zAngle">Direction of vehicle - angle.</param>
        /// <param name="color1">The primary color ID.</param>
        /// <param name="color2">The secondary color ID.</param>
        /// <returns> The vehicle ID of the vehicle created (1 - <see cref="Limits.MaxVehicles"/>). <see cref="Misc.InvalidVehicleId"/> (65535) if vehicle was not created (vehicle limit reached or invalid vehicle model ID passed).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int AddStaticVehicle(int modelid, float spawnX, float spawnY, float spawnZ,
            float zAngle, int color1, int color2);

        /// <summary>
        /// Adds a 'static' vehicle (models are pre-loaded for players)to the gamemode. Can only be used when the server first starts (under <see cref="OnGameModeInit"/>). Differs from <see cref="AddStaticVehicle"/> in only one way: allows a respawn time to be set for when the vehicle is left unoccupied by the driver.
        /// </summary>
        /// <param name="modelid">The Model ID for the vehicle.</param>
        /// <param name="spawnX">The X-coordinate for the vehicle.</param>
        /// <param name="spawnY">The Y-coordinate for the vehicle.</param>
        /// <param name="spawnZ">The Z-coordinate for the vehicle.</param>
        /// <param name="zAngle">The facing - angle for the vehicle.</param>
        /// <param name="color1">The primary color ID.</param>
        /// <param name="color2">The secondary color ID.</param>
        /// <param name="respawnDelay">The delay until the car is respawned without a driver, in seconds.</param>
        /// <returns> The vehicle ID of the vehicle created (1 - <see cref="Limits.MaxVehicles"/>). <see cref="Misc.InvalidVehicleId"/> (65535) if vehicle was not created (vehicle limit reached or invalid vehicle model ID passed).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int AddStaticVehicleEx(int modelid, float spawnX, float spawnY, float spawnZ,
            float zAngle, int color1, int color2, int respawnDelay);

        /// <summary>
        /// This function adds a 'static' pickup to the game. These pickups support weapons, health, armor etc., with the ability to function without scripting them (weapons/health/armor will be given automatically).
        /// </summary>
        /// <param name="model">The model of the pickup.</param>
        /// <param name="type">The pickup spawn type.</param>
        /// <param name="x">The X coordinate to create the pickup at.</param>
        /// <param name="y">The Y coordinate to create the pickup at.</param>
        /// <param name="z">The Z coordinate to create the pickup at.</param>
        /// <param name="virtualworld">The virtual world ID of the pickup. Use -1 to show the pickup in all worlds.</param>
        /// <returns>1 if the pickup is successfully created.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int AddStaticPickup(int model, int type, float x, float y, float z, int virtualworld);

        /// <summary>
        /// This function does exactly the same as <see cref="AddStaticPickup"/>, except it returns a pickup ID which can be used to destroy it afterwards and be tracked using <see cref="OnPlayerPickUpPickup"/>.
        /// </summary>
        /// <param name="model">The model of the pickup.</param>
        /// <param name="type">The pickup spawn type.</param>
        /// <param name="x">The X coordinate to create the pickup at.</param>
        /// <param name="y">The Y coordinate to create the pickup at.</param>
        /// <param name="z">The Z coordinate to create the pickup at.</param>
        /// <param name="virtualworld">The virtual world ID of the pickup. Use -1 to make the pickup show in all worlds.</param>
        /// <returns>The ID of the created pickup, -1 on failure (pickup max limit).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int CreatePickup(int model, int type, float x, float y, float z, int virtualworld);

        /// <summary>
        /// Destroys a pickup created with <see cref="CreatePickup"/>.
        /// </summary>
        /// <param name="pickupid">The ID of the pickup to destroy (returned by <see cref="CreatePickup"/>).</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool DestroyPickup(int pickupid);

        /// <summary>
        /// Toggle the drawing of player nametags, healthbars and armor bars above players.
        /// </summary>
        /// <param name="show">False to disable, True to enable.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ShowNameTags(bool show);

        /// <summary>
        /// A function that can be used in <see cref="OnGameModeInit"/> to enable or disable the players markers, which would normally be shown on the radar. If you want to change the marker settings at some other point in the gamemode, have a look at <see cref="SetPlayerMarkerForPlayer"/> which does exactly that.
        /// </summary>
        /// <param name="mode">The mode you want to use.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ShowPlayerMarkers(int mode);

        /// <summary>
        /// Ends the currently active gamemode.
        /// </summary>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GameModeExit();

        /// <summary>
        /// Sets the world time to a specific hour.
        /// </summary>
        /// <param name="hour">Which time to set.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetWorldTime(int hour);

        /// <summary>
        /// Get the name of a weapon.
        /// </summary>
        /// <param name="weaponid">The ID of the weapon to get the name of.</param>
        /// <param name="name">An array to store the weapon's name in, passed by reference.</param>
        /// <param name="size">The length of the weapon name.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetWeaponName(int weaponid, out string name, int size);

        /// <summary>
        /// With this function you can enable or disable tire popping.
        /// </summary>
        /// <param name="enable">True to enable, False to disable tire popping.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool EnableTirePopping(bool enable);

        /// <summary>
        /// Enable friendly fire for team vehicles.
        /// </summary>
        /// <remarks>
        /// Players will be unable to damage teammates' vehicles (<see cref="SetPlayerTeam"/> must be used!)
        /// </remarks>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool EnableVehicleFriendlyFire();

        /// <summary>
        /// Toggle whether the usage of weapons in interiors is allowed or not.
        /// </summary>
        /// <param name="allow">True to enable weapons in interiors (enabled by default), False to disable weapons in interiors.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool AllowInteriorWeapons(bool allow);

        /// <summary>
        /// Set the world weather for all players.
        /// </summary>
        /// <param name="weatherid">The weather to set.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetWeather(int weatherid);

        /// <summary>
        /// Set the gravity for all players.
        /// </summary>
        /// <param name="gravity">The value that the gravity should be set to (between -50 and 50).</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetGravity(float gravity);

        /// <summary>
        /// This function will determine whether RCON admins will be teleported to their waypoint when they set one.
        /// </summary>
        /// <param name="allow">False to disable and True to enable.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool AllowAdminTeleport(bool allow);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This function does not work in the current SA:MP version.
        /// </remarks>
        /// <param name="amount">.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetDeathDropAmount(int amount);

        /// <summary>
        /// Create an explosion at the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate of the explosion.</param>
        /// <param name="y">The Y coordinate of the explosion.</param>
        /// <param name="z">The Z coordinate of the explosion.</param>
        /// <param name="type">The type of explosion.</param>
        /// <param name="radius">The explosion radius.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool CreateExplosion(float x, float y, float z, int type, float radius);

        /// <summary>
        /// This function allows to turn on zone / area names such as the "Vinewood" or "Doherty" text at the bottom-right of the screen as they enter the area. This is a gamemode option and should be set in the callback <see cref="OnGameModeInit"/>.
        /// </summary>
        /// <remarks>
        /// DEPRICATED!
        /// </remarks>
        /// <param name="enable">A toggle option for whether or not you'd like zone names on or off. False is off and True is on.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        [Obsolete("This function is deprecated. You must create your own textdraws to show zone names.")]
        public static extern bool EnableZoneNames(bool enable);

        /// <summary>
        /// Uses standard player walking animation (animation of CJ) instead of custom animations for every skin (e.g. skating for skater skins).
        /// </summary>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool UsePlayerPedAnims();

        /// <summary>
        /// Disable all the interior entrances and exits in the game (the yellow arrows at doors).
        /// </summary>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool DisableInteriorEnterExits();

        /// <summary>
        /// Set the maximum distance to display the names of players.
        /// </summary>
        /// <param name="distance">The distance to set.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetNameTagDrawDistance(float distance);

        /// <summary>
        /// Disables the nametag Line-Of-Sight checking so players can see nametags through objects.
        /// </summary>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool DisableNameTagLOS();

        /// <summary>
        /// Set a radius limitation for the chat. Only players at a certain distance from the player will see their message in the chat. Also changes the distance at which a player can see other players on the map at the same distance.
        /// </summary>
        /// <param name="chatRadius">Radius limit.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool LimitGlobalChatRadius(float chatRadius);

        /// <summary>
        /// Set the player marker radius.
        /// </summary>
        /// <param name="markerRadius">The radius that markers will show at.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool LimitPlayerMarkerRadius(float markerRadius);

        /// <summary>
        /// Connect an NPC to the server.
        /// </summary>
        /// <param name="name">The name the NPC should connect as. Must follow the same rules as normal player names.</param>
        /// <param name="script">The NPC script name that is located in the npcmodes folder (without the .amx extension).</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ConnectNPC(string name, string script);

        /// <summary>
        /// Check if a player is an actual player or an NPC.
        /// </summary>
        /// <param name="playerid">The ID of the player to check.</param>
        /// <returns>1 if the player is an NPC or 0 if it's an actual player.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsPlayerNPC(int playerid);

        /// <summary>
        /// Check if a player is logged into RCON.
        /// </summary>
        /// <param name="playerid">The ID of the player to check.</param>
        /// <returns>1 if the player is logged into RCON, otherwise 0.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsPlayerAdmin(int playerid);

        /// <summary>
        /// Kicks a player from the server. They will have to quit the game and re-connect if they wish to continue playing.
        /// </summary>
        /// <param name="playerid">The ID of the player to kick.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool Kick(int playerid);

        /// <summary>
        /// Ban a player who currently in the server. The ban will be IP-based, and be saved in the samp.ban file in the server's root directory. <see cref="BanEx"/> allows you to ban with a reason, while you can ban and unban IPs using the RCON banip and unbanip commands.
        /// </summary>
        /// <param name="playerid">The ID of the player to ban.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool Ban(int playerid);

        /// <summary>
        /// Ban a player with a reason.
        /// </summary>
        /// <param name="playerid">The ID of the player to ban.</param>
        /// <param name="reason">The reason for the ban.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool BanEx(int playerid, string reason);

        /// <summary>
        /// Sends an RCON command.
        /// </summary>
        /// <param name="command">The RCON command to be executed.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SendRconCommand(string command);

        /// <summary>
        /// Retrieve a string server variable, for example 'hostname'. Typing 'varlist' in the console will display a list of available server variables.
        /// </summary>
        /// <param name="varname">The name of the string variable to retrieve.</param>
        /// <param name="value">An array to store the retrieved string in.</param>
        /// <param name="size">Maximum length of the return string.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetServerVarAsString(string varname, out string value, int size);

        /// <summary>
        /// Get the integer value of a server variable, for example 'port'.
        /// </summary>
        /// <param name="varname">A string containing the name of the integer variable to retrieve.</param>
        /// <returns>The value of the specified server variable as an integer.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetServerVarAsInt(string varname);

        /// <summary>
        /// Gets a boolean parameter from the server.cfg file for use in scripts. Typing varlist in the server will give a list of server.cfg vars and their types.
        /// </summary>
        /// <param name="varname">Name of the server.cfg var you want to get the value of.</param>
        /// <returns>The value of the specified server var as a boolean.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetServerVarAsBool(string varname);

        /// <summary>
        /// Gets a player's network stats and saves them into a string.
        /// </summary>
        /// <param name="playerid">The ID of the player you want to get the networkstats of.</param>
        /// <param name="retstr">The string to store the networkstats in, passed by reference.</param>
        /// <param name="size">The length of the string that should be stored.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerNetworkStats(int playerid, out string retstr, int size);

        /// <summary>
        /// Gets the server's network stats and stores them in a string.
        /// </summary>
        /// <param name="retstr">The string to store the network stats in, passed by reference.</param>
        /// <param name="size">The length of the string to be stored.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetNetworkStats(out string retstr, int size);

        /// <summary>
        /// Returns the SA-MP client revision as reported by the player.
        /// </summary>
        /// <param name="playerid">The ID of the player to get the version of.</param>
        /// <param name="version">The string to store the player's version in, passed by reference.</param>
        /// <param name="len">The maximum size of the version.</param>
        /// <returns>1 on success and 0 on failure.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GetPlayerVersion(int playerid, out string version, int len);

        /// <summary>
        /// Create a menu.
        /// </summary>
        /// <param name="title">The title for the new menu.</param>
        /// <param name="columns">How many colums shall the new menu have.</param>
        /// <param name="x">The X position of the menu (640x460 canvas - 0 would put the menu at the far left).</param>
        /// <param name="y">The Y position of the menu (640x460 canvas - 0 would put the menu at the far top).</param>
        /// <param name="col1Width"> The width for the first column.</param>
        /// <param name="col2Width"> The width for the second column.</param>
        /// <returns>The ID of the new menu or -1 on failure.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int CreateMenu(string title, int columns, float x, float y, float col1Width,
            float col2Width);

        /// <summary>
        /// Destroys the specified menu.
        /// </summary>
        /// <param name="menuid">The menu ID to destroy.</param>
        /// <returns>True if the destroying was successful, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool DestroyMenu(int menuid);

        /// <summary>
        /// Adds an item to a specified menu.
        /// </summary>
        /// <param name="menuid">The menu id to add an item to.</param>
        /// <param name="column">The column to add the item to.</param>
        /// <param name="menutext">The title for the new menu item.</param>
        /// <returns>This function always returns 0.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int AddMenuItem(int menuid, int column, string menutext);

        /// <summary>
        /// Sets the caption of a column in a menu.
        /// </summary>
        /// <param name="menuid">ID of the menu which shall be manipulated.</param>
        /// <param name="column">Which column in the menu shall be manipulated.</param>
        /// <param name="columnheader">The caption-text for the column.</param>
        /// <returns>Nothing specific. Ignore it.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SetMenuColumnHeader(int menuid, int column, string columnheader);

        /// <summary>
        /// Shows a previously created menu for a player.
        /// </summary>
        /// <param name="menuid">The ID of the menu to show.</param>
        /// <param name="playerid">The ID of the player to whom the menu will be shown.</param>
        /// <returns>1 if succeeded, 0 if failed.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ShowMenuForPlayer(int menuid, int playerid);

        /// <summary>
        /// Hides a menu for a player.
        /// </summary>
        /// <param name="menuid">The ID of the menu to hide.</param>
        /// <param name="playerid">The ID of the player that the menu will be hidden for.</param>
        /// <returns>1 if succeeeded, 0 if failed.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool HideMenuForPlayer(int menuid, int playerid);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsValidMenu(int menuid);

        /// <summary>
        /// Disable a menu.
        /// </summary>
        /// <param name="menuid">The menu to disable.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool DisableMenu(int menuid);

        /// <summary>
        /// Disable a specific row in a menu.
        /// </summary>
        /// <param name="menuid">The menu to disable a row of.</param>
        /// <param name="row">The row to disable.</param>
        /// <returns>1 if succeeded, 0 if failed.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool DisableMenuRow(int menuid, int row);

        /// <summary>
        /// Gets the ID of the menu the player is currently viewing.
        /// </summary>
        /// <param name="playerid">The ID of the player to check whether the menu is show for.</param>
        /// <returns>The ID of the player's currently shown menu or 255 on failure.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetPlayerMenu(int playerid);

        /// <summary>
        /// Creates a textdraw.
        /// </summary>
        /// <param name="x">X-Coordinate.</param>
        /// <param name="y">Y-Coordinate.</param>
        /// <param name="text">The text in the textdraw.</param>
        /// <returns>The ID of the created textdraw.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int TextDrawCreate(float x, float y, string text);

        /// <summary>
        /// Destroys a textdraw.
        /// </summary>
        /// <param name="text">The ID of the textdraw to destroy.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawDestroy(int text);

        /// <summary>
        /// Sets the width and height of the letters.
        /// </summary>
        /// <param name="text">The TextDraw to change.</param>
        /// <param name="x">Width of a char.</param>
        /// <param name="y">Height of a char.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawLetterSize(int text, float x, float y);

        /// <summary>
        /// Change the size of a textdraw (box if <see cref="TextDrawUseBox"/> is enabled and/or clickable area for use with <see cref="TextDrawSetSelectable"/>).
        /// </summary>
        /// <param name="text">The TextDraw to set the size of.</param>
        /// <param name="x">The size on the X axis (left/right) following the same 640x480 grid as TextDrawCreate.</param>
        /// <param name="y">The size on the Y axis (up/down) following the same 640x480 grid as TextDrawCreate.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawTextSize(int text, float x, float y);

        /// <summary>
        /// Aligns the text in the draw area.
        /// </summary>
        /// <param name="text">The ID of the textdraw to set the alignment of.</param>
        /// <param name="alignment">1-left 2-centered 3-right.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawAlignment(int text, int alignment);

        /// <summary>
        /// Sets the text color of a textdraw.
        /// </summary>
        /// <param name="text">The TextDraw to change.</param>
        /// <param name="color">The color in hexadecimal format.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawColor(int text, int color);

        /// <summary>
        /// Toggle whether a textdraw uses a box.
        /// </summary>
        /// <param name="text">The textdraw to toggle the box on.</param>
        /// <param name="use">1 to show a box or 0 to not show a box.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawUseBox(int text, bool use);

        /// <summary>
        /// Adjusts the text box colour (only used if TextDrawUseBox 'use' parameter is 1).
        /// </summary>
        /// <remarks>
        /// Opacity is set by the alpha intensity of colour (eg. color 0x000000FF has a solid black box opacity, whereas 0x000000AA has a semi-transparent black box opacity)
        /// </remarks>
        /// <param name="text">The TextDraw to change.</param>
        /// <param name="color">The colour.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawBoxColor(int text, int color);

        /// <summary>
        /// Adds a shadow to the lower right side of the text. The shadow font matches the text font. The shadow can be cut by the box area if the size is set too big for the area.
        /// </summary>
        /// <param name="text">The textdraw to change the shadow of.</param>
        /// <param name="size">The size of the shadow.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawSetShadow(int text, int size);

        /// <summary>
        /// Sets the thickness of a textdraw's text's outline. <see cref="TextDrawBackgroundColor"/> can be used to change the color.
        /// </summary>
        /// <param name="text">The ID of the text draw to set the outline thickness of.</param>
        /// <param name="size">The thickness of the outline, as an integer. 0 for no outline.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawSetOutline(int text, int size);

        /// <summary>
        /// Adjusts the text draw area background color (the outline/shadow - NOT the box. For box color, see <see cref="TextDrawBoxColor"/>).
        /// </summary>
        /// <param name="text">The ID of the textdraw to set the background color of.</param>
        /// <param name="color">The color that the textdraw should be set to.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawBackgroundColor(int text, int color);

        /// <summary>
        /// Changes the text font.
        /// </summary>
        /// <param name="text">The TextDraw to change.</param>
        /// <param name="font">There are four font styles as shown below. Font value 4 specifies that this is a txd sprite; 5 specifies that this textdraw can display preview models. A font value greater than 5 does not display, and anything greater than 16 crashes the client.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawFont(int text, int font);

        /// <summary>
        /// Appears to scale text spacing to a proportional ratio. Useful when using TextDrawLetterSize to ensure the text has even character spacing.
        /// </summary>
        /// <param name="text">The ID of the textdraw to set the proportionality of.</param>
        /// <param name="set">True to enable proportionality, False to disable.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawSetProportional(int text, bool set);

        /// <summary>
        /// Sets the text draw to be selectable or not.
        /// </summary>
        /// <param name="text">The textdraw id that should be made selectable.</param>
        /// <param name="set">Set the textdraw selectable (True) or non-selectable (False). By default this is False.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawSetSelectable(int text, bool set);

        /// <summary>
        /// Shows a textdraw for a specific player.
        /// </summary>
        /// <param name="playerid">The ID of the player to show the textdraw for.</param>
        /// <param name="text">The ID of the textdraw to show.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawShowForPlayer(int playerid, int text);

        /// <summary>
        /// Hides a textdraw for a specific player.
        /// </summary>
        /// <param name="playerid">The ID of the player that the textdraw should be hidden for.</param>
        /// <param name="text">The ID of the textdraw to hide.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawHideForPlayer(int playerid, int text);

        /// <summary>
        /// Shows a textdraw for all players.
        /// </summary>
        /// <param name="text">The textdraw to show.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawShowForAll(int text);

        /// <summary>
        /// Hides a text draw for all players.
        /// </summary>
        /// <param name="text">The TextDraw to hide.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawHideForAll(int text);

        /// <summary>
        /// Changes the text on a textdraw.
        /// </summary>
        /// <param name="text">The TextDraw to change.</param>
        /// <param name="str">The new string for the TextDraw.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawSetString(int text, string str);

        /// <summary>
        /// Set the model for a textdraw model preview.
        /// </summary>
        /// <param name="text">The textdraw id that will display the 3D preview.</param>
        /// <param name="modelindex">The GTA SA or SA:MP model ID to display.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawSetPreviewModel(int text, int modelindex);

        /// <summary>
        /// Sets the rotation and zoom of a 3D model preview textdraw.
        /// </summary>
        /// <param name="text">The textdraw id that displays the 3D preview.</param>
        /// <param name="fRotX">The X rotation value.</param>
        /// <param name="fRotY">The Y rotation value.</param>
        /// <param name="fRotZ">The Z rotation value.</param>
        /// <param name="fZoom">The zoom value, default value 1.0, smaller values make the camera closer and larger values make the camera further away.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawSetPreviewRot(int text, float fRotX, float fRotY, float fRotZ, float fZoom);

        /// <summary>
        /// If a vehicle model is used in a 3D preview textdraw, this sets the two colour values for that vehicle.
        /// </summary>
        /// <param name="text">The textdraw id that is set to display a 3D vehicle model preview.</param>
        /// <param name="color1">The primary Color ID to set the vehicle to.</param>
        /// <param name="color2">The secondary Color ID to set the vehicle to.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TextDrawSetPreviewVehCol(int text, int color1, int color2);

        /// <summary>
        /// Display the cursor and allow the player to select a textdraw.
        /// </summary>
        /// <param name="playerid">The ID of the player that should be able to select a textdraw.</param>
        /// <param name="hovercolor">The color of the textdraw when hovering over with mouse.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool SelectTextDraw(int playerid, int hovercolor);

        /// <summary>
        /// Cancel textdraw selection with the mouse.
        /// </summary>
        /// <param name="playerid">The ID of the player that should be the textdraw selection disabled.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool CancelSelectTextDraw(int playerid);

        /// <summary>
        /// Create a gangzone (colored radar area).
        /// </summary>
        /// <param name="minx">The X coordinate for the west side of the gangzone.</param>
        /// <param name="miny">The Y coordinate for the south side of the gangzone.</param>
        /// <param name="maxx">The X coordinate for the east side of the gangzone.</param>
        /// <param name="maxy">The Y coordinate for the north side of the gangzone.</param>
        /// <returns>The ID of the created zone.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GangZoneCreate(float minx, float miny, float maxx, float maxy);

        /// <summary>
        /// Destroy a gangzone.
        /// </summary>
        /// <param name="zone">The ID of the zone to destroy.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GangZoneDestroy(int zone);

        /// <summary>
        /// Show a gangzone for a player.
        /// </summary>
        /// <param name="playerid">The ID of the player you would like to show the gangzone for.</param>
        /// <param name="zone">The ID of the gang zone to show for the player.</param>
        /// <param name="color">The color to show the gang zone as. Alpha transparency supported.</param>
        /// <returns>True if the gangzone was shown, otherwise False.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GangZoneShowForPlayer(int playerid, int zone, int color);

        /// <summary>
        /// GangZoneShowForAll shows a gangzone with the desired color to all players.
        /// </summary>
        /// <param name="zone">The ID of the gangzone to show.</param>
        /// <param name="color">The color of the gangzone.</param>
        /// <returns>1 if the gangzone was shown, otherwise 0 (non-existant).</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GangZoneShowForAll(int zone, int color);

        /// <summary>
        /// Hides a gangzone for a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to hide the gangzone for.</param>
        /// <param name="zone">The ID of the zone to hide.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GangZoneHideForPlayer(int playerid, int zone);

        /// <summary>
        /// GangZoneHideForAll hides a gangzone from all players.
        /// </summary>
        /// <param name="zone">The zone to hide.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GangZoneHideForAll(int zone);

        /// <summary>
        /// Makes a gangzone flash for a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to flash the gangzone for.</param>
        /// <param name="zone">The ID of the zone to flash.</param>
        /// <param name="flashcolor">The color the zone will flash.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GangZoneFlashForPlayer(int playerid, int zone, int flashcolor);

        /// <summary>
        /// GangZoneFlashForAll flashes a gangzone for all players.
        /// </summary>
        /// <param name="zone">The zone to flash.</param>
        /// <param name="flashcolor">The color the zone will flash.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GangZoneFlashForAll(int zone, int flashcolor);

        /// <summary>
        /// Stops a gangzone flashing for a player.
        /// </summary>
        /// <param name="playerid">The ID of the player to stop the gangzone flashing for.</param>
        /// <param name="zone">The ID of the gangzonezone to stop flashing.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GangZoneStopFlashForPlayer(int playerid, int zone);

        /// <summary>
        /// GangZoneStopFlashForAll stops a gangzone flashing for all players.
        /// </summary>
        /// <param name="zone">The zone to stop from flashing.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool GangZoneStopFlashForAll(int zone);

        /// <summary>
        /// Creates a 3D Text Label at a specific location in the world.
        /// </summary>
        /// <param name="text">The initial text string.</param>
        /// <param name="color">The text Color.</param>
        /// <param name="x">X-Coordinate.</param>
        /// <param name="y">Y-Coordinate.</param>
        /// <param name="z">Z-Coordinate.</param>
        /// <param name="drawDistance">The distance from where you are able to see the 3D Text Label.</param>
        /// <param name="virtualWorld">The virtual world in which you are able to see the 3D Text.</param>
        /// <param name="testLOS">Whether to test the line-of-sight so this text can't be seen through objects.</param>
        /// <returns>The ID of the newly created 3D Text Label.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int Create3DTextLabel(string text, int color, float x, float y, float z, float drawDistance,
            int virtualWorld, bool testLOS);

        /// <summary>
        /// Delete a 3D text label (created with <a href="/wiki/Create3DTextLabel" title="Create3DTextLabel">Create3DTextLabel</a>).
        /// </summary>
        /// <param name="id">The ID of the 3D text label to delete.</param>
        /// <returns>1 if the 3D text label was deleted, otherwise 0.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool Delete3DTextLabel(int id);

        /// <summary>
        /// Attatch a 3D text label to player.
        /// </summary>
        /// <param name="id">The ID of the 3D Text label to attach.</param>
        /// <param name="playerid">The ID of the player to attach the 3D text label to.</param>
        /// <param name="offsetX">The X offset from the player.</param>
        /// <param name="offsetY">The Y offset from the player.</param>
        /// <param name="offsetZ">The Z offset from the player.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool Attach3DTextLabelToPlayer(int id, int playerid, float offsetX, float offsetY,
            float offsetZ);

        /// <summary>
        /// Attaches a 3D Text Label to a specific vehicle.
        /// </summary>
        /// <param name="id">The 3D Text Label you want to attach.</param>
        /// <param name="vehicleid">The vehicle you want to attach the 3D Text Label to.</param>
        /// <param name="offsetX">The Offset-X coordinate of the vehicle.</param>
        /// <param name="offsetY">The Offset-Y coordinate of the vehicle.</param>
        /// <param name="offsetZ">The Offset-Z coordinate of the vehicle.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool Attach3DTextLabelToVehicle(int id, int vehicleid, float offsetX, float offsetY,
            float offsetZ);

        /// <summary>
        /// Updates a 3D Text Label text and color.
        /// </summary>
        /// <param name="id">The 3D Text Label you want to update.</param>
        /// <param name="color">The color the 3D Text Label should have from now on.</param>
        /// <param name="text">The new text which the 3D Text Label should have from now on.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool Update3DTextLabelText(int id, int color, string text);

        /// <summary>
        /// Creates a 3D Text Label only for a specific player.
        /// </summary>
        /// <param name="playerid">The player which should see the newly created 3DText Label.</param>
        /// <param name="text">The text to display.</param>
        /// <param name="color">The text color.</param>
        /// <param name="x">X Coordinate (or offset if attached).</param>
        /// <param name="y">Y Coordinate (or offset if attached).</param>
        /// <param name="z">Z Coordinate (or offset if attached).</param>
        /// <param name="drawDistance">The distance where you are able to see the 3D Text Label.</param>
        /// <param name="attachedplayer">The player you want to attach the 3D Text Label to. (None: <see cref="Misc.InvalidVehicleId"/>).</param>
        /// <param name="attachedvehicle">The vehicle you want to attach the 3D Text Label to. (None: <see cref="Misc.InvalidVehicleId"/>).</param>
        /// <param name="testLOS">Whether to test the line-of-sight so this text can't be seen through walls.</param>
        /// <returns>The ID of the newly created Player 3D Text Label.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int CreatePlayer3DTextLabel(int playerid, string text, int color, float x, float y, float z,
            float drawDistance, int attachedplayer, int attachedvehicle, bool testLOS);

        /// <summary>
        /// Destroy a 3D Text Label.
        /// </summary>
        /// <param name="playerid">The player whose 3D text label to destroy.</param>
        /// <param name="id">The ID of the 3D Text Label to destroy.</param>
        /// <returns>1 if destroyed, 0 if not.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool DeletePlayer3DTextLabel(int playerid, int id);

        /// <summary>
        /// Updates a player 3D Text Label's text and color.
        /// </summary>
        /// <param name="playerid">The ID of the player for which the 3D Text Label was created.</param>
        /// <param name="id">The 3D Text Label you want to update.</param>
        /// <param name="color">The color the 3D Text Label should have from now on.</param>
        /// <param name="text">The new text which the 3D Text Label should have from now on.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool UpdatePlayer3DTextLabelText(int playerid, int id, int color, string text);

        /// <summary>
        /// Shows the player a synchronous (only one at a time) dialog box.
        /// </summary>
        /// <param name="playerid">The ID of the player to show the dialog to.</param>
        /// <param name="dialogid">An ID to assign this dialog to, so responses can be processed. Max dialogid is 32767. Using negative values will close any open dialog.</param>
        /// <param name="style">The style of the dialog.</param>
        /// <param name="caption">The title at the top of the dialog. The length of the caption can not exceed more than 64 characters before it starts to cut off.</param>
        /// <param name="info">The text to display in the main dialog. Use \n to start a new line and \t to tabulate.</param>
        /// <param name="button1">The text on the left button.</param>
        /// <param name="button2">The text on the right button. Leave it blank to hide it.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool ShowPlayerDialog(int playerid, int dialogid, int style, string caption, string info,
            string button1, string button2);

        /// <summary>
        /// Sets a timer to call a function after some time.
        /// </summary>
        /// <param name="interval">Interval in milliseconds.</param>
        /// <param name="repeat">Boolean if the timer should occur repeatedly or only once.</param>
        /// <param name="args">An object containing information about the timer.</param>
        /// <returns>The ID of the timer that was started.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int SetTimer(int interval, bool repeat, object args);

        /// <summary>
        /// Kills (stops) a running timer.
        /// </summary>
        /// <param name="timerid">The ID of the timer to kill.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool KillTimer(int timerid);

        /// <summary>
        /// Returns a hash build from information of the player's PC.
        /// </summary>
        /// <remarks>
        /// It is a non-reversible (lossy) hash derived from information about your San Andreas installation path.
        /// It is not a unique ID.
        /// It was added to assist owners of large servers who deal with constant attacks from cheaters and botters.
        /// It has been in SA-MP for 2 years.
        /// /// </remarks>
        /// <param name="playerid">The ID of the player whose gpci you'd like</param>
        /// <param name="buffer">A string to store the gpci, passed by reference.</param>
        /// <param name="size">The length of the string that should be stored.</param>
        /// <returns>This function doesn't return a specific value.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        // ReSharper disable once InconsistentNaming
        public static extern bool gpci(int playerid, out string buffer, int size);

        #endregion

        #region Wrapping methods

        public static bool SetSpawnInfo(int playerid, int team, int skin, float x, float y, float z,
            float rotation, Weapon weapon1, int weapon1Ammo, Weapon weapon2, int weapon2Ammo, Weapon weapon3,
            int weapon3Ammo)
        {
            return SetSpawnInfo(playerid, team, skin, x, y, z, rotation, (int) weapon1, weapon1Ammo, (int) weapon2,
                weapon2Ammo, (int) weapon3, weapon3Ammo);
        }

        public static float GetPlayerFacingAngle(int playerid)
        {
            float angle;
            GetPlayerFacingAngle(playerid, out angle);
            return angle;
        }

        public static float GetPlayerHealth(int playerid)
        {
            float health;
            GetPlayerHealth(playerid, out health);
            return health;
        }

        public static float GetPlayerArmour(int playerid)
        {
            float armour;
            GetPlayerArmour(playerid, out armour);
            return armour;
        }

        public static string GetPlayerIp(int playerid)
        {
            string ip;
            GetPlayerIp(playerid, out ip, 16);
            return ip;
        }

        public static string GetPlayerName(int playerid)
        {
            string name;
            GetPlayerName(playerid, out name, Limits.MaxPlayerName);
            return name;
        }

        public static string GetPVarString(int playerid, string varname)
        {
            string value;
            GetPVarString(playerid, varname, out value, 64);
            return value;
        }

        public static string GetPVarNameAtIndex(int playerid, int index)
        {
            string varname;
            GetPVarNameAtIndex(playerid, index, out varname, 64);
            return varname;
        }

        public static string GetWeaponName(int weaponid)
        {
            string name;
            GetWeaponName(weaponid, out name, 32);
            return name;
        }

        public static string GetServerVarAsString(string varname)
        {
            string value;
            GetServerVarAsString(varname, out value, 64);
            return value;
        }

        public static string GetPlayerNetworkStats(int playerid)
        {
            string retstr;
            GetPlayerNetworkStats(playerid, out retstr, 256);
            return retstr;
        }

        public static string GetNetworkStats()
        {
            string retstr;
            GetNetworkStats(out retstr, 256);
            return retstr;
        }

        public static string GetPlayerVersion(int playerid)
        {
            string version;
            GetPlayerVersion(playerid, out version, 64);
            return version;
        }

        // ReSharper disable once InconsistentNaming
        public static string gpci(int playerid)
        {
            string buffer;
            gpci(playerid, out buffer, 64);
            return buffer;
        }

        public static int SetTimer(int interval, bool repeat, TimerTickHandler handler, object args)
        {
            int timerid = SetTimer(interval, repeat, args);

            TimerHandlers[timerid] = handler;
            return timerid;
        }

        #endregion

        #region Events

        public delegate bool TimerTickHandler(int timerid, object args);

        public delegate void GameModeHandler(object sender, GameModeEventArgs e);

        public delegate void PlayerHandler(object sender, PlayerEventArgs e);

        public delegate void PlayerDisconnectedHandler(object sender, PlayerDisconnectedEventArgs e);

        public delegate void PlayerDeathHandler(object sender, PlayerDeathEventArgs e);

        public delegate void VehicleSpawnedHandler(object sender, VehicleEventArgs e);

        public delegate void VehicleDeathHandler(object sender, PlayerVehicleEventArgs e);

        public delegate void PlayerTextHandler(object sender, PlayerTextEventArgs e);

        public delegate void PlayerRequestClassHandler(object sender, PlayerRequestClassEventArgs e);

        public delegate void PlayerEnterVehicleHandler(object sender, PlayerEnterVehicleEventArgs e);

        public delegate void PlayerAndVehicleHandler(object sender, PlayerVehicleEventArgs r);

        public delegate void PlayerStateHandler(object sender, PlayerStateEventArgs e);

        public delegate void RconHandler(object sender, RconEventArgs e);

        public delegate void ObjectHandler(object sender, ObjectEventArgs e);

        public delegate void PlayerObjectHandler(object sender, PlayerObjectEventArgs e);

        public delegate void PlayerPickupHandler(object sender, PlayerPickupEventArgs e);

        public delegate void VehicleModHandler(object sender, VehicleModEventArgs e);

        public delegate void PlayerEnterModShopHandler(object sender, PlayerEnterModShopEventArgs e);

        public delegate void VehiclePaintjobHandler(object sender, VehiclePaintjobEventArgs e);

        public delegate void VehicleResprayedHandler(object sender, VehicleResprayedEventArgs e);

        public delegate void UnoccupiedVehicleUpdatedHandler(object sender, UnoccupiedVehicleEventArgs e);

        public delegate void PlayerSelectedMenuRowHandler(object sender, PlayerSelectedMenuRowEventArgs e);

        public delegate void PlayerInteriorChangedHandler(object sender, PlayerInteriorChangedEventArgs e);

        public delegate void PlayerKeyStateChangedHandler(object sender, PlayerKeyStateChangedEventArgs e);

        public delegate void RconLoginAttemptHandler(object sender, RconLoginAttemptEventArgs e);

        public delegate void StreamPlayerHandler(object sender, StreamPlayerEventArgs e);

        public delegate void DialogResponseHandler(object sender, DialogResponseEventArgs e);

        public delegate void PlayerDamageHandler(object sender, PlayerDamageEventArgs e);

        public delegate void PlayerClickMapHandler(object sender, PlayerClickMapEventArgs e);

        public delegate void PlayerClickTextDrawHandler(object sender, PlayerClickTextDrawEventArgs e);

        public delegate void PlayerClickPlayerHandler(object sender, PlayerClickPlayerEventArgs e);

        public delegate void PlayerEditObjectHandler(object sender, PlayerEditObjectEventArgs e);

        public delegate void PlayerEditAttachedObjectHandler(object sender, PlayerEditAttachedObjectEventArgs e);

        public delegate void PlayerSelectObjectHandler(object sender, PlayerSelectObjectEventArgs e);

        public delegate void WeaponShotHandler(object sender, WeaponShotEventArgs e);

        public event GameModeHandler GameModeInitialized;

        public event GameModeHandler GameModeExited;

        public event PlayerHandler PlayerConnected;

        public event PlayerDisconnectedHandler PlayerDisconnected;

        public event PlayerHandler PlayerSpawned;

        public event PlayerDeathHandler PlayerDied;

        public event VehicleSpawnedHandler VehicleSpawned;

        public event VehicleDeathHandler VehicleDied;

        public event PlayerTextHandler PlayerText;

        public event PlayerTextHandler PlayerCommandText;

        public event PlayerRequestClassHandler PlayerRequestClass;

        public event PlayerEnterVehicleHandler PlayerEnterVehicle;

        public event PlayerAndVehicleHandler PlayerExitVehicle;

        public event PlayerStateHandler PlayerStateChanged;

        public event PlayerHandler PlayerEnterCheckpoint;

        public event PlayerHandler PlayerLeaveCheckpoint;

        public event PlayerHandler PlayerEnterRaceCheckpoint;

        public event PlayerHandler PlayerLeaveRaceCheckpoint;

        public event RconHandler RconCommand;

        public event PlayerHandler PlayerRequestSpawn;

        public event ObjectHandler ObjectMoved;

        public event PlayerObjectHandler PlayerObjectMoved;

        public event PlayerPickupHandler PlayerPickUpPickup;

        public event VehicleModHandler VehicleMod;

        public event PlayerEnterModShopHandler PlayerEnterExitModShop;

        public event VehiclePaintjobHandler VehiclePaintjobApplied;

        public event VehicleResprayedHandler VehicleResprayed;

        public event PlayerHandler VehicleDamageStatusUpdated;

        public event UnoccupiedVehicleUpdatedHandler UnoccupiedVehicleUpdated;

        public event PlayerSelectedMenuRowHandler PlayerSelectedMenuRow;

        public event PlayerHandler PlayerExitedMenu;

        public event PlayerInteriorChangedHandler PlayerInteriorChanged;

        public event PlayerKeyStateChangedHandler PlayerKeyStateChanged;

        public event RconLoginAttemptHandler RconLoginAttempt;

        public event PlayerHandler PlayerUpdate;

        public event StreamPlayerHandler StreamPlayerIn;

        public event StreamPlayerHandler StreamPlayerOut;

        public event PlayerAndVehicleHandler StreamVehicleIn;

        public event PlayerAndVehicleHandler StreamVehicleOut;

        public event DialogResponseHandler DialogResponse;

        public event PlayerDamageHandler PlayerTakeDamage;

        public event PlayerDamageHandler PlayerGiveDamage;

        public event PlayerClickMapHandler PlayerClickMap;

        public event PlayerClickTextDrawHandler PlayerClickTextDraw;

        public event PlayerClickTextDrawHandler PlayerClickPlayerTextDraw;

        public event PlayerClickPlayerHandler PlayerClickPlayer;

        public event PlayerEditObjectHandler PlayerEditObject;

        public event PlayerEditAttachedObjectHandler PlayerEditAttachedObject;

        public event PlayerSelectObjectHandler PlayerSelectObject;

        public event WeaponShotHandler PlayerWeaponShot;

        #endregion

        #region Callbacks

        public bool OnTimerTick(int timerid, object args)
        {
            if (TimerHandlers.ContainsKey(timerid) && !TimerHandlers[timerid](timerid, args))
                KillTimer(timerid);
            return true;
        }

        public virtual bool OnGameModeInit()
        {
            var args = new GameModeEventArgs();

            if (GameModeInitialized != null)
                GameModeInitialized(this, args);

            return args.Success;
        }


        public virtual bool OnGameModeExit()
        {
            var args = new GameModeEventArgs();

            if (GameModeExited != null)
                GameModeExited(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerConnect(int playerid)
        {
            var args = new PlayerEventArgs(playerid);

            if (PlayerConnected != null)
                PlayerConnected(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerDisconnect(int playerid, int reason)
        {
            var args = new PlayerDisconnectedEventArgs(playerid, (PlayerDisconnectReason)reason);

            if (PlayerDisconnected != null)
                PlayerDisconnected(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerSpawn(int playerid)
        {
            var args = new PlayerEventArgs(playerid);

            if (PlayerSpawned != null)
                PlayerSpawned(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerDeath(int playerid, int killerid, int reason)
        {
            var args = new PlayerDeathEventArgs(playerid, killerid, (Weapon)reason);

            if (PlayerDied != null)
                PlayerDied(this, args);

            return args.Success;
        }

        public virtual bool OnVehicleSpawn(int vehicleid)
        {
            var args = new VehicleEventArgs(vehicleid);

            if (VehicleSpawned != null)
                VehicleSpawned(this, args);

            return args.Success;
        }

        public virtual bool OnVehicleDeath(int vehicleid, int killerid)
        {
            var args = new PlayerVehicleEventArgs(killerid, vehicleid);

            if (VehicleDied != null)
                VehicleDied(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerText(int playerid, string text)
        {
            var args = new PlayerTextEventArgs(playerid, text);

            if (PlayerText != null)
                PlayerText(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerCommandText(int playerid, string cmdtext)
        {
            var args = new PlayerTextEventArgs(playerid, cmdtext) {Success = false};

            if (PlayerCommandText != null)
                PlayerCommandText(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerRequestClass(int playerid, int classid)
        {
            var args = new PlayerRequestClassEventArgs(playerid, classid);

            if (PlayerRequestClass != null)
                PlayerRequestClass(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerEnterVehicle(int playerid, int vehicleid, bool ispassenger)
        {
            var args = new PlayerEnterVehicleEventArgs(playerid, vehicleid, ispassenger);

            if (PlayerEnterVehicle != null)
                PlayerEnterVehicle(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerExitVehicle(int playerid, int vehicleid)
        {
            var args = new PlayerVehicleEventArgs(playerid, vehicleid);

            if (PlayerExitVehicle != null)
                PlayerExitVehicle(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerStateChange(int playerid, int newstate, int oldstate)
        {
            var args = new PlayerStateEventArgs(playerid, (PlayerState)newstate, (PlayerState)oldstate);

            if (PlayerStateChanged != null)
                PlayerStateChanged(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerEnterCheckpoint(int playerid)
        {
            var args = new PlayerEventArgs(playerid);

            if (PlayerEnterCheckpoint != null)
                PlayerEnterCheckpoint(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerLeaveCheckpoint(int playerid)
        {
            var args = new PlayerEventArgs(playerid);

            if (PlayerLeaveCheckpoint != null)
                PlayerLeaveCheckpoint(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerEnterRaceCheckpoint(int playerid)
        {
            var args = new PlayerEventArgs(playerid);

            if (PlayerEnterRaceCheckpoint != null)
                PlayerEnterRaceCheckpoint(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerLeaveRaceCheckpoint(int playerid)
        {
            var args = new PlayerEventArgs(playerid);

            if (PlayerLeaveRaceCheckpoint != null)
                PlayerLeaveRaceCheckpoint(this, args);

            return args.Success;
        }

        public virtual bool OnRconCommand(string command)
        {
            var args = new RconEventArgs(command);

            if (RconCommand != null)
                RconCommand(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerRequestSpawn(int playerid)
        {
            var args = new PlayerEventArgs(playerid);

            if (PlayerRequestSpawn != null)
                PlayerRequestSpawn(this, args);

            return args.Success;
        }

        public virtual bool OnObjectMoved(int objectid)
        {
            var args = new ObjectEventArgs(objectid);

            if (ObjectMoved != null)
                ObjectMoved(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerObjectMoved(int playerid, int objectid)
        {
            var args = new PlayerObjectEventArgs(playerid, objectid);

            if (PlayerObjectMoved != null)
                PlayerObjectMoved(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerPickUpPickup(int playerid, int pickupid)
        {
            var args = new PlayerPickupEventArgs(playerid, pickupid);

            if (PlayerPickUpPickup != null)
                PlayerPickUpPickup(this, args);

            return args.Success;
        }

        public virtual bool OnVehicleMod(int playerid, int vehicleid, int componentid)
        {
            var args = new VehicleModEventArgs(playerid, vehicleid, componentid);

            if (VehicleMod != null)
                VehicleMod(this, args);

            return args.Success;
        }

        public virtual bool OnEnterExitModShop(int playerid, int enterexit, int interiorid)
        {
            var args = new PlayerEnterModShopEventArgs(playerid, (EnterExit)enterexit, interiorid);

            if (PlayerEnterExitModShop != null)
                PlayerEnterExitModShop(this, args);

            return args.Success;
        }

        public virtual bool OnVehiclePaintjob(int playerid, int vehicleid, int paintjobid)
        {
            var args = new VehiclePaintjobEventArgs(playerid, vehicleid, paintjobid);

            if (VehiclePaintjobApplied != null)
                VehiclePaintjobApplied(this, args);

            return args.Success;
        }

        public virtual bool OnVehicleRespray(int playerid, int vehicleid, int color1, int color2)
        {
            var args = new VehicleResprayedEventArgs(playerid, vehicleid, color1, color2);

            if (VehicleResprayed != null)
                VehicleResprayed(this, args);

            return args.Success;
        }

        public virtual bool OnVehicleDamageStatusUpdate(int vehicleid, int playerid)
        {
            var args = new PlayerVehicleEventArgs(playerid, vehicleid);

            if (VehicleDamageStatusUpdated != null)
                VehicleDamageStatusUpdated(this, args);

            return args.Success;
        }

        public virtual bool OnUnoccupiedVehicleUpdate(int vehicleid, int playerid, int passengerSeat)
        {
            var args = new UnoccupiedVehicleEventArgs(playerid, vehicleid, passengerSeat);

            if (UnoccupiedVehicleUpdated != null)
                UnoccupiedVehicleUpdated(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerSelectedMenuRow(int playerid, int row)
        {
            var args = new PlayerSelectedMenuRowEventArgs(playerid, row);

            if (PlayerSelectedMenuRow != null)
                PlayerSelectedMenuRow(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerExitedMenu(int playerid)
        {
            var args = new PlayerEventArgs(playerid);

            if (PlayerExitedMenu != null)
                PlayerExitedMenu(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerInteriorChange(int playerid, int newinteriorid, int oldinteriorid)
        {
            var args = new PlayerInteriorChangedEventArgs(playerid, newinteriorid, oldinteriorid);

            if (PlayerInteriorChanged != null)
                PlayerInteriorChanged(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerKeyStateChange(int playerid, int newkeys, int oldkeys)
        {
            var args = new PlayerKeyStateChangedEventArgs(playerid, (Keys)newkeys, (Keys)oldkeys);

            if (PlayerKeyStateChanged != null)
                PlayerKeyStateChanged(this, args);

            return args.Success;
        }

        public virtual bool OnRconLoginAttempt(string ip, string password, bool success)
        {
            var args = new RconLoginAttemptEventArgs(ip, password, success);

            if (RconLoginAttempt != null)
                RconLoginAttempt(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerUpdate(int playerid)
        {
            var args = new PlayerEventArgs(playerid);

            if (PlayerUpdate != null)
                PlayerUpdate(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerStreamIn(int playerid, int forplayerid)
        {
            var args = new StreamPlayerEventArgs(playerid, forplayerid);

            if (StreamPlayerIn != null)
                StreamPlayerIn(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerStreamOut(int playerid, int forplayerid)
        {
            var args = new StreamPlayerEventArgs(playerid, forplayerid);

            if (StreamPlayerOut != null)
                StreamPlayerOut(this, args);

            return args.Success;
        }

        public virtual bool OnVehicleStreamIn(int vehicleid, int forplayerid)
        {
            var args = new PlayerVehicleEventArgs(forplayerid, vehicleid);

            if (StreamVehicleIn != null)
                StreamVehicleIn(this, args);

            return args.Success;
        }

        public virtual bool OnVehicleStreamOut(int vehicleid, int forplayerid)
        {
            var args = new PlayerVehicleEventArgs(forplayerid, vehicleid);

            if (StreamVehicleOut != null)
                StreamVehicleOut(this, args);

            return args.Success;
        }

        public virtual bool OnDialogResponse(int playerid, int dialogid, int response, int listitem, string inputtext)
        {
            var args = new DialogResponseEventArgs(playerid, dialogid, response, listitem, inputtext);

            if (DialogResponse != null)
                DialogResponse(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerTakeDamage(int playerid, int issuerid, float amount, int weaponid, int bodypart)
        {
            var args = new PlayerDamageEventArgs(playerid, issuerid, amount, (Weapon)weaponid, (BodyPart)bodypart);

            if (PlayerTakeDamage != null)
                PlayerTakeDamage(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerGiveDamage(int playerid, int damagedid, float amount, int weaponid, int bodypart)
        {
            var args = new PlayerDamageEventArgs(playerid, damagedid, amount, (Weapon)weaponid, (BodyPart)bodypart);

            if (PlayerGiveDamage != null)
                PlayerGiveDamage(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerClickMap(int playerid, float fX, float fY, float fZ)
        {
            var args = new PlayerClickMapEventArgs(playerid, new Position(fX, fY, fZ));

            if (PlayerClickMap != null)
                PlayerClickMap(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerClickTextDraw(int playerid, int clickedid)
        {
            var args = new PlayerClickTextDrawEventArgs(playerid, clickedid);

            if (PlayerClickTextDraw != null)
                PlayerClickTextDraw(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerClickPlayerTextDraw(int playerid, int playertextid)
        {
            var args = new PlayerClickTextDrawEventArgs(playerid, playertextid);

            if (PlayerClickPlayerTextDraw != null)
                PlayerClickPlayerTextDraw(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerClickPlayer(int playerid, int clickedplayerid, int source)
        {
            var args = new PlayerClickPlayerEventArgs(playerid, clickedplayerid, (PlayerClickSource)source);

            if (PlayerClickPlayer != null)
                PlayerClickPlayer(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerEditObject(int playerid, bool playerobject, int objectid, int response, float fX,
            float fY,
            float fZ, float fRotX, float fRotY, float fRotZ)
        {
            var args = new PlayerEditObjectEventArgs(playerid, playerobject, objectid, (EditObjectResponse) response,
                new Position(fX, fY, fZ), new Rotation(fRotX, fRotY, fRotZ));

            if (PlayerEditObject != null)
                PlayerEditObject(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerEditAttachedObject(int playerid, int response, int index, int modelid, int boneid,
            float fOffsetX, float fOffsetY, float fOffsetZ, float fRotX, float fRotY, float fRotZ, float fScaleX,
            float fScaleY, float fScaleZ)
        {
            var args = new PlayerEditAttachedObjectEventArgs(playerid, (EditObjectResponse) response, index, modelid,
                boneid, new Position(fOffsetX, fOffsetY, fOffsetZ), new Rotation(fRotX, fRotY, fRotZ),
                new Position(fScaleX, fScaleY, fScaleZ));

            if (PlayerEditAttachedObject != null)
                PlayerEditAttachedObject(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerSelectObject(int playerid, int type, int objectid, int modelid, float fX, float fY,
            float fZ)
        {
            var args = new PlayerSelectObjectEventArgs(playerid, (ObjectType) type, objectid, modelid,
                new Position(fX, fY, fZ));

            if (PlayerSelectObject != null)
                PlayerSelectObject(this, args);

            return args.Success;
        }

        public virtual bool OnPlayerWeaponShot(int playerid, int weaponid, int hittype, int hitid, float fX, float fY,
            float fZ)
        {
            var args = new WeaponShotEventArgs(playerid, (Weapon)weaponid, (BulletHitType)hittype, hitid, new Position(fX, fY, fZ));

            if (PlayerWeaponShot != null)
                PlayerWeaponShot(this, args);

            return args.Success;
        }

        #endregion
    }
}
