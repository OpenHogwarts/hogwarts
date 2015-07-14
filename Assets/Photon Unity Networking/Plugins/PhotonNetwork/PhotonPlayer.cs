// ----------------------------------------------------------------------------
// <copyright file="PhotonPlayer.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Represents a player, identified by actorID (a.k.a. ActorNumber).
//   Caches properties of a player.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;


/// <summary>
/// Summarizes a "player" within a room, identified (in that room) by actorID.
/// </summary>
/// <remarks>
/// Each player has an actorId (or ID), valid for that room. It's -1 until it's assigned by server.
/// Each client can set it's player's custom properties with SetCustomProperties, even before being in a room.
/// They are synced when joining a room.
/// </remarks>
/// \ingroup publicApi
public class PhotonPlayer
{
    /// <summary>This player's actorID</summary>
    public int ID
    {
        get { return this.actorID; }
    }

    /// <summary>Identifier of this player in current room.</summary>
    private int actorID = -1;

    private string nameField = "";

    /// <summary>Nickname of this player.</summary>
    public string name {
        get
        {
            return this.nameField;
        }
        set
        {
            if (!isLocal)
            {
                Debug.LogError("Error: Cannot change the name of a remote player!");
                return;
            }
            if (string.IsNullOrEmpty(value) || value.Equals(this.nameField))
            {
                return;
            }

            this.nameField = value;
            PhotonNetwork.playerName = value;   // this will sync the local player's name in a room
        }
    }

    /// <summary>Only one player is controlled by each client. Others are not local.</summary>
    public readonly bool isLocal = false;

    /// <summary>
    /// True if this player is the Master Client of the current room.
    /// </summary>
    /// <remarks>
    /// See also: PhotonNetwork.masterClient.
    /// </remarks>
    public bool isMasterClient
    {
        get { return (PhotonNetwork.networkingPeer.mMasterClientId == this.ID); }
    }

    /// <summary>Read-only cache for custom properties of player. Set via Player.SetCustomProperties.</summary>
    /// <remarks>
    /// Don't modify the content of this Hashtable. Use SetCustomProperties and the
    /// properties of this class to modify values. When you use those, the client will
    /// sync values with the server.
    /// </remarks>
    public Hashtable customProperties { get; internal set; }

    /// <summary>Creates a Hashtable with all properties (custom and "well known" ones).</summary>
    /// <remarks>If used more often, this should be cached.</remarks>
    public Hashtable allProperties
    {
        get
        {
            Hashtable allProps = new Hashtable();
            allProps.Merge(this.customProperties);
            allProps[ActorProperties.PlayerName] = this.name;
            return allProps;
        }
    }

    /// <summary>Can be used to store a reference that's useful to know "by player".</summary>
    /// <remarks>Example: Set a player's character as Tag by assigning the GameObject on Instantiate.</remarks>
    public object TagObject;


    /// <summary>
    /// Creates a PhotonPlayer instance.
    /// </summary>
    /// <param name="isLocal">If this is the local peer's player (or a remote one).</param>
    /// <param name="actorID">ID or ActorNumber of this player in the current room (a shortcut to identify each player in room)</param>
    /// <param name="name">Name of the player (a "well known property").</param>
    public PhotonPlayer(bool isLocal, int actorID, string name)
    {
        this.customProperties = new Hashtable();
        this.isLocal = isLocal;
        this.actorID = actorID;
        this.nameField = name;
    }

    /// <summary>
    /// Internally used to create players from event Join
    /// </summary>
    internal protected PhotonPlayer(bool isLocal, int actorID, Hashtable properties)
    {
        this.customProperties = new Hashtable();
        this.isLocal = isLocal;
        this.actorID = actorID;

        this.InternalCacheProperties(properties);
    }

    /// <summary>
    /// Makes PhotonPlayer comparable
    /// </summary>
    public override bool Equals(object p)
    {
        PhotonPlayer pp = p as PhotonPlayer;
        return (pp != null && this.GetHashCode() == pp.GetHashCode());
    }

    public override int GetHashCode()
    {
        return this.ID;
    }

    /// <summary>
    /// Used internally, to update this client's playerID when assigned.
    /// </summary>
    internal void InternalChangeLocalID(int newID)
    {
        if (!this.isLocal)
        {
            Debug.LogError("ERROR You should never change PhotonPlayer IDs!");
            return;
        }

        this.actorID = newID;
    }

    /// <summary>
    /// Caches custom properties for this player.
    /// </summary>
    internal void InternalCacheProperties(Hashtable properties)
    {
        if (properties == null || properties.Count == 0 || this.customProperties.Equals(properties))
        {
            return;
        }

        if (properties.ContainsKey(ActorProperties.PlayerName))
        {
            this.nameField = (string)properties[ActorProperties.PlayerName];
        }
        if (properties.ContainsKey(ActorProperties.IsInactive))
        {
            // TODO: implement isinactive
        }

        this.customProperties.MergeStringKeys(properties);
        this.customProperties.StripKeysWithNullValues();
    }

    /// <summary>
    /// Updates and synchronizes the named properties of this Player with the values of propertiesToSet.
    /// </summary>
    /// <remarks>
    /// Any player's properties are available in a Room only and only until the player disconnect or leaves.
    /// Access any player's properties by: Player.CustomProperties (read-only!) but don't modify that hashtable.
    ///
    /// New properties are added, existing values are updated.
    /// Other values will not be changed, so only provide values that changed or are new.
    /// To delete a named (custom) property of this player, use null as value.
    /// Only string-typed keys are applied (everything else is ignored).
    ///
    /// Local cache is updated immediately, other players are updated through Photon with a fitting operation.
    /// To reduce network traffic, set only values that actually changed.
    /// </remarks>
    /// <param name="propertiesToSet">Hashtable of props to udpate, set and sync. See description.</param>
    public void SetCustomProperties(Hashtable propertiesToSet)
    {
        if (propertiesToSet == null)
        {
            return;
        }

        // merge (delete null-values)
        this.customProperties.MergeStringKeys(propertiesToSet); // includes a Equals check (simplifying things)
        this.customProperties.StripKeysWithNullValues();

        // send (sync) these new values
        Hashtable customProps = propertiesToSet.StripToStringKeys() as Hashtable;
        if (this.actorID > 0 && !PhotonNetwork.offlineMode)
        {
            PhotonNetwork.networkingPeer.OpSetCustomPropertiesOfActor(this.actorID, customProps, true, 0);
        }
        NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerPropertiesChanged, this, propertiesToSet);
    }

    /// <summary>
    /// Will update properties on the server, if the expectedValues are matching the current (property)values on the server.
    /// </summary>
    /// <remarks>
    /// This variant of SetCustomProperties uses server side Check-And-Swap (CAS) to update valuzes only if the expected values are correct.
    /// The expectedValues can't be null or empty, but they can be different key/values than the propertiesToSet.
    /// 
    /// If the client's knowledge of properties is wrong or outdated, it can't set values (with CAS).
    /// This can be useful to keep players from concurrently setting values. For example: If all players
    /// try to pickup some card or item, only one should get it. With CAS, only the first SetProperties 
    /// gets executed server-side and any other (sent at the same time) fails.
    /// 
    /// The server will broadcast successfully changed values and the local "cache" of customProperties 
    /// only gets updated after a roundtrip (if anything changed).
    /// </remarks>
    /// <param name="propertiesToSet">The new properties to be set. </param>
    /// <param name="expectedValues">At least one property key/value set to check server-side. Key and value must be correct.</param>
    public void SetCustomProperties(Hashtable propertiesToSet, Hashtable expectedValues)
    {
        if (propertiesToSet == null)
        {
            return;
        }
        if (expectedValues == null || expectedValues.Count == 0)
        {
            Debug.LogWarning("SetCustomProperties(props, expected) requires some expectedValues. Use SetCustomProperties(props) to simply set some without check.");
            return;
        }

        if (this.actorID > 0 && !PhotonNetwork.offlineMode)
        {
            Hashtable customProps = propertiesToSet.StripToStringKeys() as Hashtable;
            Hashtable customPropsToCheck = expectedValues.StripToStringKeys() as Hashtable;
            PhotonNetwork.networkingPeer.OpSetPropertiesOfActor(this.actorID, customProps, false, 0, customPropsToCheck);
        }
    }

    /// <summary>
    /// Try to get a specific player by id.
    /// </summary>
    /// <param name="ID">ActorID</param>
    /// <returns>The player with matching actorID or null, if the actorID is not in use.</returns>
    public static PhotonPlayer Find(int ID)
    {
        if (PhotonNetwork.networkingPeer != null)
        {
            return PhotonNetwork.networkingPeer.GetPlayerWithId(ID);
        }
        return null;
    }

    public PhotonPlayer Get(int id)
    {
        return PhotonPlayer.Find(id);
    }

    public PhotonPlayer GetNext()
    {
        return GetNextFor(this.ID);
    }

    public PhotonPlayer GetNextFor(PhotonPlayer currentPlayer)
    {
        if (currentPlayer == null)
        {
            return null;
        }
        return GetNextFor(currentPlayer.ID);
    }

    public PhotonPlayer GetNextFor(int currentPlayerId)
    {
        if (PhotonNetwork.networkingPeer == null || PhotonNetwork.networkingPeer.mActors == null || PhotonNetwork.networkingPeer.mActors.Count < 2)
        {
            return null;
        }

        Dictionary<int, PhotonPlayer> players = PhotonNetwork.networkingPeer.mActors;
        int nextHigherId = int.MaxValue;    // we look for the next higher ID
        int lowestId = currentPlayerId;     // if we are the player with the highest ID, there is no higher and we return to the lowest player's id

        foreach (int playerid in players.Keys)
        {
            if (playerid < lowestId)
            {
                lowestId = playerid;        // less than any other ID (which must be at least less than this player's id).
            }
            else if (playerid > currentPlayerId && playerid < nextHigherId)
            {
                nextHigherId = playerid;    // more than our ID and less than those found so far.
            }
        }

        //UnityEngine.Debug.LogWarning("Debug. " + currentPlayerId + " lower: " + lowestId + " higher: " + nextHigherId + " ");
        //UnityEngine.Debug.LogWarning(this.RoomReference.GetPlayer(currentPlayerId));
        //UnityEngine.Debug.LogWarning(this.RoomReference.GetPlayer(lowestId));
        //if (nextHigherId != int.MaxValue) UnityEngine.Debug.LogWarning(this.RoomReference.GetPlayer(nextHigherId));
        return (nextHigherId != int.MaxValue) ? players[nextHigherId] : players[lowestId];
    }

    /// <summary>
    /// Brief summary string of the PhotonPlayer. Includes name or player.ID and if it's the Master Client.
    /// </summary>
    public override string ToString()
    {
        if (string.IsNullOrEmpty(this.name))
        {
            return string.Format("#{0:00}{1}",  this.ID, this.isMasterClient ? "(master)":"");
        }

        return string.Format("'{0}'{1}", this.name, this.isMasterClient ? "(master)" : "");
    }

    /// <summary>
    /// String summary of the PhotonPlayer: player.ID, name and all custom properties of this user.
    /// </summary>
    /// <remarks>
    /// Use with care and not every frame!
    /// Converts the customProperties to a String on every single call.
    /// </remarks>
    public string ToStringFull()
    {
        return string.Format("#{0:00} '{1}' {2}", this.ID, this.name, this.customProperties.ToStringFull());
    }
}
