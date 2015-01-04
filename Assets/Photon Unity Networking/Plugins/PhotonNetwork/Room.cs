// ----------------------------------------------------------------------------
// <copyright file="Room.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Represents a room/game on the server and caches the properties of that.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using System;
using ExitGames.Client.Photon;
using UnityEngine;


/// <summary>
/// This class resembles a room that PUN joins (or joined).
/// The properties are settable as opposed to those of a RoomInfo and you can close or hide "your" room.
/// </summary>
/// \ingroup publicApi
public class Room : RoomInfo
{
    /// <summary>Count of players in this room.</summary>
    public new int playerCount
    {
        get
        {
            if (PhotonNetwork.playerList != null)
            {
                return PhotonNetwork.playerList.Length;
            }
            else
            {
                return 0;
            }
        }
    }


    /// <summary>The name of a room. Unique identifier (per Loadbalancing group) for a room/match.</summary>
    public new string name
    {
        get
        {
            return this.nameField;
        }

        internal set
        {
            this.nameField = value;
        }
    }

    /// <summary>
    /// Sets a limit of players to this room. This property is shown in lobby, too.
    /// If the room is full (players count == maxplayers), joining this room will fail.
    /// </summary>
    public new int maxPlayers
    {
        get
        {
            return (int)this.maxPlayersField;
        }

        set
        {
            if (!this.Equals(PhotonNetwork.room))
            {
                UnityEngine.Debug.LogWarning("Can't set maxPlayers when not in that room.");
            }

            if (value > 255)
            {
                UnityEngine.Debug.LogWarning("Can't set Room.maxPlayers to: " + value + ". Using max value: 255.");
                value = 255;
            }

            if (value != this.maxPlayersField && !PhotonNetwork.offlineMode)
            {
                PhotonNetwork.networkingPeer.OpSetPropertiesOfRoom(new Hashtable() { { GameProperties.MaxPlayers, (byte)value } }, true, (byte)0);
            }

            this.maxPlayersField = (byte)value;
        }
    }

    /// <summary>
    /// Defines if the room can be joined.
    /// This does not affect listing in a lobby but joining the room will fail if not open.
    /// If not open, the room is excluded from random matchmaking.
    /// Due to racing conditions, found matches might become closed before they are joined.
    /// Simply re-connect to master and find another.
    /// Use property "visible" to not list the room.
    /// </summary>
    public new bool open
    {
        get
        {
            return this.openField;
        }

        set
        {
            if (!this.Equals(PhotonNetwork.room))
            {
                UnityEngine.Debug.LogWarning("Can't set open when not in that room.");
            }

            if (value != this.openField && !PhotonNetwork.offlineMode)
            {
                PhotonNetwork.networkingPeer.OpSetPropertiesOfRoom(new Hashtable() { { GameProperties.IsOpen, value } }, true, (byte)0);
            }

            this.openField = value;
        }
    }

    /// <summary>
    /// Defines if the room is listed in its lobby.
    /// Rooms can be created invisible, or changed to invisible.
    /// To change if a room can be joined, use property: open.
    /// </summary>
    public new bool visible
    {
        get
        {
            return this.visibleField;
        }

        set
        {
            if (!this.Equals(PhotonNetwork.room))
            {
                UnityEngine.Debug.LogWarning("Can't set visible when not in that room.");
            }

            if (value != this.visibleField && !PhotonNetwork.offlineMode)
            {
                PhotonNetwork.networkingPeer.OpSetPropertiesOfRoom(new Hashtable() { { GameProperties.IsVisible, value } }, true, (byte)0);
            }

            this.visibleField = value;
        }
    }

    /// <summary>
    /// A list of custom properties that should be forwarded to the lobby and listed there.
    /// </summary>
    public string[] propertiesListedInLobby { get; private set; }

    /// <summary>
    /// Gets if this room uses autoCleanUp to remove all (buffered) RPCs and instantiated GameObjects when a player leaves.
    /// </summary>
    public bool autoCleanUp
    {
        get
        {
            return this.autoCleanUpField;
        }
    }

    internal Room(string roomName, RoomOptions options) : base(roomName, null)
    {
        if (options == null)
        {
            options = new RoomOptions();
        }

        this.visibleField = options.isVisible;
        this.openField = options.isOpen;
        this.maxPlayersField = (byte)options.maxPlayers;
        this.autoCleanUpField = false;  // defaults to false, unless set to true when room gets created.

        this.CacheProperties(options.customRoomProperties);
        this.propertiesListedInLobby = options.customRoomPropertiesForLobby;
    }

    /// <summary>
    /// Updates and synchronizes the named properties of this Room with the values of propertiesToSet.
    /// </summary>
    /// <remarks>
    /// Any player can set a Room's properties. Room properties are available until changed, deleted or
    /// until the last player leaves the room.
    /// Access them by: Room.CustomProperties (read-only!).
    ///
    /// To reduce network traffic, set only values that actually changed.
    ///
    /// New properties are added, existing values are updated.
    /// Other values will not be changed, so only provide values that changed or are new.
    /// To delete a named (custom) property of this room, use null as value.
    /// Only string-typed keys are applied (everything else is ignored).
    ///
    /// Local cache is updated immediately, other clients are updated through Photon with a fitting operation.
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
        if (!PhotonNetwork.offlineMode)
        {
            PhotonNetwork.networkingPeer.OpSetCustomPropertiesOfRoom(customProps, true, 0);
        }
        NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonCustomRoomPropertiesChanged, propertiesToSet);
    }

    /// <summary>
    /// Enables you to define the properties available in the lobby if not all properties are needed to pick a room.
    /// </summary>
    /// <remarks>
    /// It makes sense to limit the amount of properties sent to users in the lobby as this improves speed and stability.
    /// </remarks>
    /// <param name="propsListedInLobby">An array of custom room property names to forward to the lobby.</param>
    public void SetPropertiesListedInLobby(string[] propsListedInLobby)
    {
        Hashtable customProps = new Hashtable();
        customProps[GameProperties.PropsListedInLobby] = propsListedInLobby;
        PhotonNetwork.networkingPeer.OpSetPropertiesOfRoom(customProps, false, 0);

        this.propertiesListedInLobby = propsListedInLobby;
    }


    /// <summary>Returns a summary of this Room instance as string.</summary>
    /// <returns>Summary of this Room instance.</returns>
    public override string ToString()
    {
        return string.Format("Room: '{0}' {1},{2} {4}/{3} players.", this.nameField, this.visibleField ? "visible" : "hidden", this.openField ? "open" : "closed", this.maxPlayersField, this.playerCount);
    }

    /// <summary>Returns a summary of this Room instance as longer string, including Custom Properties.</summary>
    /// <returns>Summary of this Room instance.</returns>
    public new string ToStringFull()
    {
        return string.Format("Room: '{0}' {1},{2} {4}/{3} players.\ncustomProps: {5}", this.nameField, this.visibleField ? "visible" : "hidden", this.openField ? "open" : "closed", this.maxPlayersField, this.playerCount, this.customProperties.ToStringFull());
    }
}
