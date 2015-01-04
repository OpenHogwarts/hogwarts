// ----------------------------------------------------------------------------
// <copyright file="LoadbalancingPeer.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Provides the operations needed to use the loadbalancing server app(s).
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;
using System;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;


namespace ExitGames.Client.Photon
{
    /// <summary>
    /// Internally used by PUN, a LoadbalancingPeer provides the operations and enum
    /// definitions needed to use the Photon Loadbalancing server (or the Photon Cloud).
    /// </summary>
    /// <remarks>
    /// The LoadBalancingPeer does not keep a state, instead this is done by a LoadBalancingClient.
    /// </remarks>
    internal class LoadbalancingPeer : PhotonPeer
    {
        public LoadbalancingPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType) : base(listener, protocolType)
        {
        }

        public virtual bool OpGetRegions(string appId)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>();
            parameters[(byte)ParameterCode.ApplicationId] = appId;

            return this.OpCustom(OperationCode.GetRegions, parameters, true, 0, true);
        }

        /// <summary>
        /// Joins the lobby on the Master Server, where you get a list of RoomInfos of currently open rooms.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpJoinLobby(TypedLobby lobby)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpJoinLobby()");
            }

            Dictionary<byte, object> parameters = null;
            if (lobby != null && !lobby.IsDefault)
            {
                parameters = new Dictionary<byte, object>();
                parameters[(byte)ParameterCode.LobbyName] = lobby.Name;
                parameters[(byte)ParameterCode.LobbyType] = (byte)lobby.Type;
            }

            return this.OpCustom(OperationCode.JoinLobby, parameters, true);
        }

        /// <summary>
        /// Leaves the lobby on the Master Server.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpLeaveLobby()
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpLeaveLobby()");
            }

            return this.OpCustom(OperationCode.LeaveLobby, null, true);
        }


        /// <summary>
        /// Don't use this method directly, unless you know how to cache and apply customActorProperties.
        /// The PhotonNetwork methods will handle player and room properties for you and call this method.
        /// </summary>
        public virtual bool OpCreateRoom(string roomName, RoomOptions roomOptions, TypedLobby lobby, Hashtable playerProperties, bool onGameServer)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpCreateRoom()");
            }

            Dictionary<byte, object> op = new Dictionary<byte, object>();

            if (!string.IsNullOrEmpty(roomName))
            {
                op[ParameterCode.RoomName] = roomName;
            }
            if (lobby != null)
            {
                op[ParameterCode.LobbyName] = lobby.Name;
                op[ParameterCode.LobbyType] = (byte)lobby.Type;
            }

            if (onGameServer)
            {
                if (playerProperties != null && playerProperties.Count > 0)
                {
                    op[ParameterCode.PlayerProperties] = playerProperties;
                    op[ParameterCode.Broadcast] = true; // TODO: check if this also makes sense when creating a room?! // broadcast actor properties
                }


                if (roomOptions == null)
                {
                    roomOptions = new RoomOptions();
                }

                Hashtable gameProperties = new Hashtable();
                op[ParameterCode.GameProperties] = gameProperties;
                gameProperties.MergeStringKeys(roomOptions.customRoomProperties);

                gameProperties[GameProperties.IsOpen] = roomOptions.isOpen; // TODO: check default value. dont send this then
                gameProperties[GameProperties.IsVisible] = roomOptions.isVisible; // TODO: check default value. dont send this then
                gameProperties[GameProperties.PropsListedInLobby] = roomOptions.customRoomPropertiesForLobby;
                if (roomOptions.maxPlayers > 0)
                {
                    gameProperties[GameProperties.MaxPlayers] = roomOptions.maxPlayers;
                }
                if (roomOptions.cleanupCacheOnLeave)
                {
                    op[ParameterCode.CleanupCacheOnLeave] = true;               // this is actually setting the room's config
                    gameProperties[GameProperties.CleanupCacheOnLeave] = true;  // this is only informational for the clients which join
                }
            }

            // UnityEngine.Debug.Log("CreateGame: " + SupportClass.DictionaryToString(op));
            return this.OpCustom(OperationCode.CreateGame, op, true);
        }


        /// <summary>LoadBalancingPeer.OpJoinRoom</summary>
        public virtual bool OpJoinRoom(string roomName, RoomOptions roomOptions, TypedLobby lobby, bool createIfNotExists, Hashtable playerProperties, bool onGameServer)
        {
            Dictionary<byte, object> op = new Dictionary<byte, object>();

            if (!string.IsNullOrEmpty(roomName))
            {
                op[ParameterCode.RoomName] = roomName;
            }
            if (createIfNotExists)
            {
                op[ParameterCode.CreateIfNotExists] = true;
                if (lobby != null)
                {
                    op[ParameterCode.LobbyName] = lobby.Name;
                    op[ParameterCode.LobbyType] = (byte)lobby.Type;
                }
            }

            if (onGameServer)
            {
                if (playerProperties != null && playerProperties.Count > 0)
                {
                    op[ParameterCode.PlayerProperties] = playerProperties;
                    op[ParameterCode.Broadcast] = true; // broadcast actor properties
                }


                if (createIfNotExists)
                {
                    if (roomOptions == null)
                    {
                        roomOptions = new RoomOptions();
                    }

                    Hashtable gameProperties = new Hashtable();
                    op[ParameterCode.GameProperties] = gameProperties;
                    gameProperties.MergeStringKeys(roomOptions.customRoomProperties);

                    gameProperties[GameProperties.IsOpen] = roomOptions.isOpen;
                    gameProperties[GameProperties.IsVisible] = roomOptions.isVisible;
                    gameProperties[GameProperties.PropsListedInLobby] = roomOptions.customRoomPropertiesForLobby;
                    if (roomOptions.maxPlayers > 0)
                    {
                        gameProperties[GameProperties.MaxPlayers] = roomOptions.maxPlayers;
                    }
                    if (roomOptions.cleanupCacheOnLeave)
                    {
                        op[ParameterCode.CleanupCacheOnLeave] = true;               // this is actually setting the room's config
                        gameProperties[GameProperties.CleanupCacheOnLeave] = true;  // this is only informational for the clients which join
                    }
                }
            }

            // UnityEngine.Debug.Log("JoinGame: " + SupportClass.DictionaryToString(op));
            return this.OpCustom(OperationCode.JoinGame, op, true);
        }


        /// <summary>
        /// Operation to join a random, available room. Overloads take additional player properties.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// If all rooms are closed or full, the OperationResponse will have a returnCode of ErrorCode.NoRandomMatchFound.
        /// If successful, the OperationResponse contains a gameserver address and the name of some room.
        /// </summary>
        /// <param name="expectedCustomRoomProperties">Optional. A room will only be joined, if it matches these custom properties (with string keys).</param>
        /// <param name="expectedMaxPlayers">Filters for a particular maxplayer setting. Use 0 to accept any maxPlayer value.</param>
        /// <param name="playerProperties">This player's properties (custom and well known).</param>
        /// <param name="matchingType">Selects one of the available matchmaking algorithms. See MatchmakingMode enum for options.</param>
        /// <returns>If the operation could be sent currently (requires connection).</returns>
        public virtual bool OpJoinRandomRoom(Hashtable expectedCustomRoomProperties, byte expectedMaxPlayers, Hashtable playerProperties, MatchmakingMode matchingType, TypedLobby typedLobby, string sqlLobbyFilter)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpJoinRandomRoom()");
            }

            Hashtable expectedRoomProperties = new Hashtable();
            expectedRoomProperties.MergeStringKeys(expectedCustomRoomProperties);
            if (expectedMaxPlayers > 0)
            {
                expectedRoomProperties[GameProperties.MaxPlayers] = expectedMaxPlayers;
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (expectedRoomProperties.Count > 0)
            {
                opParameters[ParameterCode.GameProperties] = expectedRoomProperties;
            }

            if (playerProperties != null && playerProperties.Count > 0)
            {
                opParameters[ParameterCode.PlayerProperties] = playerProperties;
            }

            if (matchingType != MatchmakingMode.FillRoom)
            {
                opParameters[ParameterCode.MatchMakingType] = (byte)matchingType;
            }

            if (typedLobby != null)
            {
                opParameters[ParameterCode.LobbyName] = typedLobby.Name;
                opParameters[ParameterCode.LobbyType] = (byte)typedLobby.Type;
            }

            if (!string.IsNullOrEmpty(sqlLobbyFilter))
            {
                opParameters[ParameterCode.Data] = sqlLobbyFilter;
            }

            // UnityEngine.Debug.LogWarning("OpJoinRandom: " + opParameters.ToStringFull());
            return this.OpCustom(OperationCode.JoinRandomGame, opParameters, true);
        }

        /// <summary>
        /// Request the rooms and online status for a list of friends (each client must set a unique username via OpAuthenticate).
        /// </summary>
        /// <remarks>
        /// Used on Master Server to find the rooms played by a selected list of users.
        /// Users identify themselves by using OpAuthenticate with a unique username.
        /// The list of usernames must be fetched from some other source (not provided by Photon).
        ///
        /// The server response includes 2 arrays of info (each index matching a friend from the request):
        /// ParameterCode.FindFriendsResponseOnlineList = bool[] of online states
        /// ParameterCode.FindFriendsResponseRoomIdList = string[] of room names (empty string if not in a room)
        /// </remarks>
        /// <param name="friendsToFind">Array of friend's names (make sure they are unique).</param>
        /// <returns>If the operation could be sent (requires connection).</returns>
        public virtual bool OpFindFriends(string[] friendsToFind)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (friendsToFind != null && friendsToFind.Length > 0)
            {
                opParameters[ParameterCode.FindFriendsRequestList] = friendsToFind;
            }

            return this.OpCustom(OperationCode.FindFriends, opParameters, true);
        }

        public bool OpSetCustomPropertiesOfActor(int actorNr, Hashtable actorProperties, bool broadcast, byte channelId)
        {
            return this.OpSetPropertiesOfActor(actorNr, actorProperties.StripToStringKeys(), broadcast, channelId);
        }

        protected bool OpSetPropertiesOfActor(int actorNr, Hashtable actorProperties, bool broadcast, byte channelId)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor()");
            }

            if (actorNr <= 0 || actorProperties == null)
            {
                if (this.DebugOut >= DebugLevel.INFO)
                {
                    this.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor not sent. ActorNr must be > 0 and actorProperties != null.");
                }
                return false;
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(ParameterCode.Properties, actorProperties);
            opParameters.Add(ParameterCode.ActorNr, actorNr);
            if (broadcast)
            {
                opParameters.Add(ParameterCode.Broadcast, broadcast);
            }

            return this.OpCustom((byte)OperationCode.SetProperties, opParameters, broadcast, channelId);
        }

        protected void OpSetPropertyOfRoom(byte propCode, object value)
        {
            Hashtable properties = new Hashtable();
            properties[propCode] = value;
            this.OpSetPropertiesOfRoom(properties, true, (byte)0);
        }

        public bool OpSetCustomPropertiesOfRoom(Hashtable gameProperties, bool broadcast, byte channelId)
        {
            return this.OpSetPropertiesOfRoom(gameProperties.StripToStringKeys(), broadcast, channelId);
        }

        public bool OpSetPropertiesOfRoom(Hashtable gameProperties, bool broadcast, byte channelId)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfRoom()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(ParameterCode.Properties, gameProperties);
            if (broadcast)
            {
                opParameters.Add(ParameterCode.Broadcast, true);
            }

            return this.OpCustom((byte)OperationCode.SetProperties, opParameters, broadcast, channelId);
        }

        /// <summary>
        /// Sends this app's appId and appVersion to identify this application server side.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <remarks>
        /// This operation makes use of encryption, if that is established before.
        /// See: EstablishEncryption(). Check encryption with IsEncryptionAvailable.
        /// This operation is allowed only once per connection (multiple calls will have ErrorCode != Ok).
        /// </remarks>
        /// <param name="appId">Your application's name or ID to authenticate. This is assigned by Photon Cloud (webpage).</param>
        /// <param name="appVersion">The client's version (clients with differing client appVersions are separated and players don't meet).</param>
        /// <param name="userId"></param>
        /// <param name="authValues"></param>
        /// <param name="regionCode">When authenticating for a specific region, a NameServer will forward you to that region's MasterServer.</param>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpAuthenticate(string appId, string appVersion, string userId, AuthenticationValues authValues, string regionCode)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpAuthenticate()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (authValues != null && authValues.Secret != null)
            {
                opParameters[ParameterCode.Secret] = authValues.Secret;
                return this.OpCustom(OperationCode.Authenticate, opParameters, true, (byte)0, false);
            }

            opParameters[ParameterCode.AppVersion] = appVersion;
            opParameters[ParameterCode.ApplicationId] = appId;

            if (!string.IsNullOrEmpty(regionCode))
            {
                opParameters[ParameterCode.Region] = regionCode;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                opParameters[ParameterCode.UserId] = userId;
            }


            if (authValues != null && authValues.AuthType != CustomAuthenticationType.None)
            {
                if (!this.IsEncryptionAvailable)
                {
                    this.Listener.DebugReturn(DebugLevel.ERROR, "OpAuthenticate() failed. When you want Custom Authentication encryption is mandatory.");
                    return false;
                }

                opParameters[ParameterCode.ClientAuthenticationType] = (byte)authValues.AuthType;
                if (!string.IsNullOrEmpty(authValues.Secret))
                {
                    opParameters[ParameterCode.Secret] = authValues.Secret;
                }
                //else
                //{
                    if (!string.IsNullOrEmpty(authValues.AuthParameters))
                    {
                        opParameters[ParameterCode.ClientAuthenticationParams] = authValues.AuthParameters;
                    }
                    if (authValues.AuthPostData != null)
                    {
                        opParameters[ParameterCode.ClientAuthenticationData] = authValues.AuthPostData;
                    }
                //}
            }

            bool sent = this.OpCustom(OperationCode.Authenticate, opParameters, true, (byte)0, this.IsEncryptionAvailable);
            if (!sent)
            {
                this.Listener.DebugReturn(DebugLevel.ERROR, "Error calling OpAuthenticate! Did not work. Check log output, CustomAuthenticationValues and if you're connected.");
            }
            return sent;
        }


        /// <summary>
        /// Operation to handle this client's interest groups (for events in room).
        /// </summary>
        /// <remarks>
        /// Note the difference between passing null and byte[0]:
        ///   null won't add/remove any groups.
        ///   byte[0] will add/remove all (existing) groups.
        /// First, removing groups is executed. This way, you could leave all groups and join only the ones provided.
        /// </remarks>
        /// <param name="groupsToRemove">Groups to remove from interest. Null will not remove any. A byte[0] will remove all.</param>
        /// <param name="groupsToAdd">Groups to add to interest. Null will not add any. A byte[0] will add all current.</param>
        /// <returns></returns>
        public virtual bool OpChangeGroups(byte[] groupsToRemove, byte[] groupsToAdd)
        {
            if (this.DebugOut >= DebugLevel.ALL)
            {
                this.Listener.DebugReturn(DebugLevel.ALL, "OpChangeGroups()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (groupsToRemove != null)
            {
                opParameters[(byte)LiteOpKey.Remove] = groupsToRemove;
            }
            if (groupsToAdd != null)
            {
                opParameters[(byte)LiteOpKey.Add] = groupsToAdd;
            }

            return this.OpCustom((byte)LiteOpCode.ChangeGroups, opParameters, true, 0);
        }

        /// <summary>
        /// Send an event with custom code/type and any content to the other players in the same room.
        /// </summary>
        /// <remarks>This override explicitly uses another parameter order to not mix it up with the implementation for Hashtable only.</remarks>
        /// <param name="eventCode">Identifies this type of event (and the content). Your game's event codes can start with 0.</param>
        /// <param name="customEventContent">Any serializable datatype (including Hashtable like the other OpRaiseEvent overloads).</param>
        /// <param name="sendReliable">If this event has to arrive reliably (potentially repeated if it's lost).</param>
        /// <param name="raiseEventOptions">Contains (slightly) less often used options. If you pass null, the default options will be used.</param>
        /// <returns>If operation could be enqueued for sending. Sent when calling: Service or SendOutgoingCommands.</returns>
        public virtual bool OpRaiseEvent(byte eventCode, object customEventContent, bool sendReliable, RaiseEventOptions raiseEventOptions)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[(byte)LiteOpKey.Code] = (byte)eventCode;
            if (customEventContent != null)
            {
                opParameters[(byte) LiteOpKey.Data] = customEventContent;
            }

            if (raiseEventOptions == null)
            {
                raiseEventOptions = RaiseEventOptions.Default;
            }
            else
            {
                if (raiseEventOptions.CachingOption != EventCaching.DoNotCache)
                {
                    opParameters[(byte) LiteOpKey.Cache] = (byte) raiseEventOptions.CachingOption;
                }
                if (raiseEventOptions.Receivers != ReceiverGroup.Others)
                {
                    opParameters[(byte) LiteOpKey.ReceiverGroup] = (byte) raiseEventOptions.Receivers;
                }
                if (raiseEventOptions.InterestGroup != 0)
                {
                    opParameters[(byte) LiteOpKey.Group] = (byte) raiseEventOptions.InterestGroup;
                }
                if (raiseEventOptions.TargetActors != null)
                {
                    opParameters[(byte) LiteOpKey.ActorList] = raiseEventOptions.TargetActors;
                }
                if (raiseEventOptions.ForwardToWebhook)
                {
                    opParameters[(byte) ParameterCode.EventForward] = true; //TURNBASED
                }
            }

            return this.OpCustom((byte)LiteOpCode.RaiseEvent, opParameters, sendReliable, raiseEventOptions.SequenceChannel, false);
        }
    }


    /// <summary>
    /// Class for constants. These (int) values represent error codes, as defined and sent by the Photon LoadBalancing logic.
    /// Pun uses these constants internally.
    /// </summary>
    /// <remarks>Codes from the Photon Core are negative. Default-app error codes go down from short.max.</remarks>
    internal class ErrorCode
    {
        /// <summary>(0) is always "OK", anything else an error or specific situation.</summary>
        public const int Ok = 0;

        // server - Photon low(er) level: <= 0

        /// <summary>
        /// (-3) Operation can't be executed yet (e.g. OpJoin can't be called before being authenticated, RaiseEvent cant be used before getting into a room).
        /// </summary>
        /// <remarks>
        /// Before you call any operations on the Cloud servers, the automated client workflow must complete its authorization.
        /// In PUN, wait until State is: JoinedLobby (with AutoJoinLobby = true) or ConnectedToMaster (AutoJoinLobby = false)
        /// </remarks>
        public const int OperationNotAllowedInCurrentState = -3;

        /// <summary>(-2) The operation you called is not implemented on the server (application) you connect to. Make sure you run the fitting applications.</summary>
        public const int InvalidOperationCode = -2;

        /// <summary>(-1) Something went wrong in the server. Try to reproduce and contact Exit Games.</summary>
        public const int InternalServerError = -1;

        // server - PhotonNetwork: 0x7FFF and down
        // logic-level error codes start with short.max

        /// <summary>(32767) Authentication failed. Possible cause: AppId is unknown to Photon (in cloud service).</summary>
        public const int InvalidAuthentication = 0x7FFF;

        /// <summary>(32753) The Authentication ticket expired. Usually, this is refreshed behind the scenes. Connect (and authorize) again.</summary>
        public const int AuthenticationTicketExpired = 0x7FF1;

        /// <summary>(32766) GameId (name) already in use (can't create another). Change name.</summary>
        public const int GameIdAlreadyExists = 0x7FFF - 1;

        /// <summary>(32765) Game is full. This rarely happens when some player joined the room before your join completed.</summary>
        public const int GameFull = 0x7FFF - 2;

        /// <summary>(32764) Game is closed and can't be joined. Join another game.</summary>
        public const int GameClosed = 0x7FFF - 3;

        [Obsolete("No longer used, cause random matchmaking is no longer a process.")] public const int AlreadyMatched =
            0x7FFF - 4;

        /// <summary>(32762) Not in use currently.</summary>
        public const int ServerFull = 0x7FFF - 5;

        /// <summary>(32761) Not in use currently.</summary>
        public const int UserBlocked = 0x7FFF - 6;

        /// <summary>(32760) Random matchmaking only succeeds if a room exists thats neither closed nor full. Repeat in a few seconds or create a new room.</summary>
        public const int NoRandomMatchFound = 0x7FFF - 7;

        /// <summary>(32758) Join can fail if the room (name) is not existing (anymore). This can happen when players leave while you join.</summary>
        public const int GameDoesNotExist = 0x7FFF - 9;

        /// <summary>(32757) Authorization on the Photon Cloud failed because the concurrent users (CCU) limit of the app's subscription is reached.</summary>
        /// <remarks>
        /// Unless you have a plan with "CCU Burst", clients might fail the authentication step during connect.
        /// Affected client are unable to call operations. Please note that players who end a game and return
        /// to the master server will disconnect and re-connect, which means that they just played and are rejected
        /// in the next minute / re-connect.
        /// This is a temporary measure. Once the CCU is below the limit, players will be able to connect an play again.
        ///
        /// OpAuthorize is part of connection workflow but only on the Photon Cloud, this error can happen.
        /// Self-hosted Photon servers with a CCU limited license won't let a client connect at all.
        /// </remarks>
        public const int MaxCcuReached = 0x7FFF - 10;

        /// <summary>(32756) Authorization on the Photon Cloud failed because the app's subscription does not allow to use a particular region's server.</summary>
        /// <remarks>
        /// Some subscription plans for the Photon Cloud are region-bound. Servers of other regions can't be used then.
        /// Check your master server address and compare it with your Photon Cloud Dashboard's info.
        /// https://cloud.exitgames.com/dashboard
        ///
        /// OpAuthorize is part of connection workflow but only on the Photon Cloud, this error can happen.
        /// Self-hosted Photon servers with a CCU limited license won't let a client connect at all.
        /// </remarks>
        public const int InvalidRegion = 0x7FFF - 11;

        /// <summary>
        /// (32755) Custom Authentication of the user failed due to setup reasons (see Cloud Dashboard) or the provided user data (like username or token). Check error message for details.
        /// </summary>
        public const int CustomAuthenticationFailed = 0x7FFF - 12;

        /// <summary>
        /// (32752) Also known as "PluginReportedError". A call to an external web service (WebHook) failed and in turn, caused the operation to fail. Check the debug message (increase the logging level, if needed).
        /// </summary>
        public const int WebHookCallFailed = 0x7FFF - 15;
    }


    /// <summary>
    /// Class for constants. These (byte) values define "well known" properties for an Actor / Player.
    /// Pun uses these constants internally.
    /// </summary>
    /// <remarks>
    /// "Custom properties" have to use a string-type as key. They can be assigned at will.
    /// </remarks>
    public class ActorProperties
    {
        /// <summary>(255) Name of a player/actor.</summary>
        public const byte PlayerName = 255; // was: 1

        /// <summary>(254) Tells you if the player is currently in this game (getting events live).</summary>
        /// <remarks>A server-set value for async games, where players can leave the game and return later.</remarks>
        public const byte IsInactive = 254;
    }


    /// <summary>
    /// Class for constants. These (byte) values are for "well known" room/game properties used in Photon Loadbalancing.
    /// Pun uses these constants internally.
    /// </summary>
    /// <remarks>
    /// "Custom properties" have to use a string-type as key. They can be assigned at will.
    /// </remarks>
    public class GameProperties
    {
        /// <summary>(255) Max number of players that "fit" into this room. 0 is for "unlimited".</summary>
        public const byte MaxPlayers = 255;
        /// <summary>(254) Makes this room listed or not in the lobby on master.</summary>
        public const byte IsVisible = 254;
        /// <summary>(253) Allows more players to join a room (or not).</summary>
        public const byte IsOpen = 253;
        /// <summary>(252) Current count of players in the room. Used only in the lobby on master.</summary>
        public const byte PlayerCount = 252;
        /// <summary>(251) True if the room is to be removed from room listing (used in update to room list in lobby on master)</summary>
        public const byte Removed = 251;
        /// <summary>(250) A list of the room properties to pass to the RoomInfo list in a lobby. This is used in CreateRoom, which defines this list once per room.</summary>
        public const byte PropsListedInLobby = 250;
        /// <summary>(249) Equivalent of Operation Join parameter CleanupCacheOnLeave.</summary>
        public const byte CleanupCacheOnLeave = 249;
    }


    /// <summary>
    /// Class for constants. These values are for events defined by Photon Loadbalancing.
    /// Pun uses these constants internally.
    /// </summary>
    /// <remarks>They start at 255 and go DOWN. Your own in-game events can start at 0.</remarks>
    public class EventCode
    {
        /// <summary>(230) Initial list of RoomInfos (in lobby on Master)</summary>
        public const byte GameList = 230;
        /// <summary>(229) Update of RoomInfos to be merged into "initial" list (in lobby on Master)</summary>
        public const byte GameListUpdate = 229;
        /// <summary>(228) Currently not used. State of queueing in case of server-full</summary>
        public const byte QueueState = 228;
        /// <summary>(227) Currently not used. Event for matchmaking</summary>
        public const byte Match = 227;
        /// <summary>(226) Event with stats about this application (players, rooms, etc)</summary>
        public const byte AppStats = 226;
        /// <summary>(224) This event provides a list of lobbies with their player and game counts.</summary>
        public const byte TypedLobbyStats = 224;
        /// <summary>(210) Internally used in case of hosting by Azure</summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        public const byte AzureNodeInfo = 210;
        /// <summary>(255) Event Join: someone joined the game. The new actorNumber is provided as well as the properties of that actor (if set in OpJoin).</summary>
        public const byte Join = (byte)LiteEventCode.Join;
        /// <summary>(254) Event Leave: The player who left the game can be identified by the actorNumber.</summary>
        public const byte Leave = (byte)LiteEventCode.Leave;
        /// <summary>(253) When you call OpSetProperties with the broadcast option "on", this event is fired. It contains the properties being set.</summary>
        public const byte PropertiesChanged = (byte)LiteEventCode.PropertiesChanged;
        /// <summary>(253) When you call OpSetProperties with the broadcast option "on", this event is fired. It contains the properties being set.</summary>
        [Obsolete("Use PropertiesChanged now.")]
        public const byte SetProperties = (byte)LiteEventCode.PropertiesChanged;
    }


    /// <summary>
    /// Class for constants. Codes for parameters of Operations and Events.
    /// Pun uses these constants internally.
    /// </summary>
    public class ParameterCode
    {
        /// <summary>(237) Optional parameter to suppress events Join and Leave for a room (which might be used as lobby/chat room then).</summary>
        public const byte SuppressRoomEvents = 237;
        /// <summary>(234) Optional parameter of OpRaiseEvent to forward the event to some web-service.</summary>
        public const byte EventForward = 234;

        /// <summary>(233) Used in EvLeave to describe if a user is inactive (and might come back) or not. In async / Turnbased games, inactive is default.</summary>
        public const byte IsInactive = (byte)233;

        /// <summary>(232) Used when creating rooms to define if any userid can join the room only once.</summary>
        public const byte CheckUserOnJoin = (byte)232;

        /// <summary>(230) Address of a (game) server to use.</summary>
        public const byte Address = 230;
        /// <summary>(229) Count of players in rooms (connected to game servers for this application, used in stats event)</summary>
        public const byte PeerCount = 229;
        /// <summary>(228) Count of games in this application (used in stats event)</summary>
        public const byte GameCount = 228;
        /// <summary>(227) Count of players on the master server (connected to master server for this application, looking for games, used in stats event)</summary>
        public const byte MasterPeerCount = 227;
        /// <summary>(225) User's ID</summary>
        public const byte UserId = 225;
        /// <summary>(224) Your application's ID: a name on your own Photon or a GUID on the Photon Cloud</summary>
        public const byte ApplicationId = 224;
        /// <summary>(223) Not used (as "Position" currently). If you get queued before connect, this is your position</summary>
        public const byte Position = 223;
        /// <summary>(223) Modifies the matchmaking algorithm used for OpJoinRandom. Allowed parameter values are defined in enum MatchmakingMode.</summary>
        public const byte MatchMakingType = 223;
        /// <summary>(222) List of RoomInfos about open / listed rooms</summary>
        public const byte GameList = 222;
        /// <summary>(221) Internally used to establish encryption</summary>
        public const byte Secret = 221;
        /// <summary>(220) Version of your application</summary>
        public const byte AppVersion = 220;

        // Codes for Azure / TCP Proxy are removed

        /// <summary>(255) Code for the gameId/roomName (a unique name per room). Used in OpJoin and similar.</summary>
        public const byte RoomName = (byte)LiteOpKey.GameId;
        /// <summary>(250) Code for broadcast parameter of OpSetProperties method.</summary>
        public const byte Broadcast = (byte)LiteOpKey.Broadcast;
        /// <summary>(252) Code for list of players in a room. Currently not used.</summary>
        public const byte ActorList = (byte)LiteOpKey.ActorList;
        /// <summary>(254) Code of the Actor of an operation. Used for property get and set.</summary>
        public const byte ActorNr = (byte)LiteOpKey.ActorNr;
        /// <summary>(249) Code for property set (Hashtable).</summary>
        public const byte PlayerProperties = (byte)LiteOpKey.ActorProperties;
        /// <summary>(245) Code of data/custom content of an event. Used in OpRaiseEvent.</summary>
        public const byte CustomEventContent = (byte)LiteOpKey.Data;
        /// <summary>(245) Code of data of an event. Used in OpRaiseEvent.</summary>
        public const byte Data = (byte)LiteOpKey.Data;
        /// <summary>(244) Code used when sending some code-related parameter, like OpRaiseEvent's event-code.</summary>
        /// <remarks>This is not the same as the Operation's code, which is no longer sent as part of the parameter Dictionary in Photon 3.</remarks>
        public const byte Code = (byte)LiteOpKey.Code;
        /// <summary>(248) Code for property set (Hashtable).</summary>
        public const byte GameProperties = (byte)LiteOpKey.GameProperties;
        /// <summary>
        /// (251) Code for property-set (Hashtable). This key is used when sending only one set of properties.
        /// If either ActorProperties or GameProperties are used (or both), check those keys.
        /// </summary>
        public const byte Properties = (byte)LiteOpKey.Properties;
        /// <summary>(253) Code of the target Actor of an operation. Used for property set. Is 0 for game</summary>
        public const byte TargetActorNr = (byte)LiteOpKey.TargetActorNr;
        /// <summary>(246) Code to select the receivers of events (used in Lite, Operation RaiseEvent).</summary>
        public const byte ReceiverGroup = (byte)LiteOpKey.ReceiverGroup;
        /// <summary>(247) Code for caching events while raising them.</summary>
        public const byte Cache = (byte)LiteOpKey.Cache;
        /// <summary>(241) Bool parameter of CreateGame Operation. If true, server cleans up roomcache of leaving players (their cached events get removed).</summary>
        public const byte CleanupCacheOnLeave = (byte)241;

        /// <summary>(240) Code for "group" operation-parameter (as used in Op RaiseEvent).</summary>
        public const byte Group = LiteOpKey.Group;
        /// <summary>(239) The "Remove" operation-parameter can be used to remove something from a list. E.g. remove groups from player's interest groups.</summary>
        public const byte Remove = LiteOpKey.Remove;
        /// <summary>(238) The "Add" operation-parameter can be used to add something to some list or set. E.g. add groups to player's interest groups.</summary>
        public const byte Add = LiteOpKey.Add;


        /// <summary>(236) Time To Live (TTL) for a room when the last player leaves. Keeps room in memory for case a player re-joins soon. In milliseconds.</summary>
        public const byte EmptyRoomTTL = 236;

        /// <summary>(235) Time To Live (TTL) for an 'actor' in a room. If a client disconnects, this actor is inactive first and removed after this timeout. In milliseconds.</summary>
        public const byte PlayerTTL = 235;


        /// <summary>(217) This key's (byte) value defines the target custom authentication type/service the client connects with. Used in OpAuthenticate</summary>
        public const byte ClientAuthenticationType = 217;

        /// <summary>(216) This key's (string) value provides parameters sent to the custom authentication type/service the client connects with. Used in OpAuthenticate</summary>
        public const byte ClientAuthenticationParams = 216;

        /// <summary>(215) Makes the server create a room if it doesn't exist. OpJoin uses this to always enter a room, unless it exists and is full/closed.</summary>
        public const byte CreateIfNotExists = 215;

        /// <summary>(215) The JoinMode enum defines which variant of joining a room will be executed: Join only if available, create if not exists or re-join.</summary>
        /// <remarks>Replaces CreateIfNotExists which was only a bool-value.</remarks>
        public const byte JoinMode = 215;

        /// <summary>(214) This key's (string or byte[]) value provides parameters sent to the custom authentication service setup in Photon Dashboard. Used in OpAuthenticate</summary>
        public const byte ClientAuthenticationData = 214;

        /// <summary>(213) Used in matchmaking-related methods and when creating a room to name a lobby (to join or to attach a room to).</summary>
        public const byte LobbyName = (byte)213;

        /// <summary>(212) Used in matchmaking-related methods and when creating a room to define the type of a lobby. Combined with the lobby name this identifies the lobby.</summary>
        public const byte LobbyType = (byte)212;

        /// <summary>(211) This (optional) parameter can be sent in Op Authenticate to turn on Lobby Stats (info about lobby names and their user- and game-counts). See: PhotonNetwork.Lobbies</summary>
        public const byte LobbyStats = (byte)211;

        /// <summary>(210) Used for region values in OpAuth and OpGetRegions.</summary>
        public const byte Region = (byte)210;

        /// <summary>(209) Path of the WebRPC that got called. Also known as "WebRpc Name". Type: string.</summary>
        public const byte UriPath = 209;

        /// <summary>(208) Parameters for a WebRPC as: Dictionary<string, object>. This will get serialized to JSon.</summary>
        public const byte WebRpcParameters = 208;

        /// <summary>(207) ReturnCode for the WebRPC, as sent by the web service (not by Photon, which uses ErrorCode). Type: byte.</summary>
        public const byte WebRpcReturnCode = 207;

        /// <summary>(206) Message returned by WebRPC server. Analog to Photon's debug message. Type: string.</summary>
        public const byte WebRpcReturnMessage = 206;


        /// <summary>(1) Used in Op FindFriends request. Value must be string[] of friends to look up.</summary>
        public const byte FindFriendsRequestList = (byte)1;

        /// <summary>(1) Used in Op FindFriends response. Contains bool[] list of online states (false if not online).</summary>
        public const byte FindFriendsResponseOnlineList = (byte)1;

        /// <summary>(2) Used in Op FindFriends response. Contains string[] of room names ("" where not known or no room joined).</summary>
        public const byte FindFriendsResponseRoomIdList = (byte)2;
    }


    /// <summary>
    /// Class for constants. Contains operation codes.
    /// Pun uses these constants internally.
    /// </summary>
    public class OperationCode
    {
        /// <summary>(230) Authenticates this peer and connects to a virtual application</summary>
        public const byte Authenticate = 230;
        /// <summary>(229) Joins lobby (on master)</summary>
        public const byte JoinLobby = 229;
        /// <summary>(228) Leaves lobby (on master)</summary>
        public const byte LeaveLobby = 228;
        /// <summary>(227) Creates a game (or fails if name exists)</summary>
        public const byte CreateGame = 227;
        /// <summary>(226) Join game (by name)</summary>
        public const byte JoinGame = 226;
        /// <summary>(225) Joins random game (on master)</summary>
        public const byte JoinRandomGame = 225;

        // public const byte CancelJoinRandom = 224; // obsolete, cause JoinRandom no longer is a "process". now provides result immediately

        /// <summary>(254) Code for OpLeave, to get out of a room.</summary>
        public const byte Leave = (byte)LiteOpCode.Leave;
        /// <summary>(253) Raise event (in a room, for other actors/players)</summary>
        public const byte RaiseEvent = (byte)LiteOpCode.RaiseEvent;
        /// <summary>(252) Set Properties (of room or actor/player)</summary>
        public const byte SetProperties = (byte)LiteOpCode.SetProperties;
        /// <summary>(251) Get Properties</summary>
        public const byte GetProperties = (byte)LiteOpCode.GetProperties;

        /// <summary>(248) Operation code to change interest groups in Rooms (Lite application and extending ones).</summary>
        public const byte ChangeGroups = (byte)LiteOpCode.ChangeGroups;

        /// <summary>(222) Request the rooms and online status for a list of friends (by name, which should be unique).</summary>
        public const byte FindFriends = 222;

        /// <summary>(221) Request statistics about a specific list of lobbies (their user and game count).</summary>
        public const byte GetLobbyStats = 221;

        /// <summary>(220) Get list of regional servers from a NameServer.</summary>
        public const byte GetRegions = 220;

        /// <summary>(219) WebRpc Operation.</summary>
        public const byte WebRpc = 219;
    }
}


/// <summary>
/// Options for matchmaking rules for OpJoinRandom.
/// </summary>
public enum MatchmakingMode : byte
{
    /// <summary>Fills up rooms (oldest first) to get players together as fast as possible. Default.</summary>
    /// <remarks>Makes most sense with MaxPlayers > 0 and games that can only start with more players.</remarks>
    FillRoom = 0,
    /// <summary>Distributes players across available rooms sequentially but takes filter into account. Without filter, rooms get players evenly distributed.</summary>
    SerialMatching = 1,
    /// <summary>Joins a (fully) random room. Expected properties must match but aside from this, any available room might be selected.</summary>
    RandomMatching = 2
}

/// <summary>
/// Options for optional "Custom Authentication" services used with Photon. Used by OpAuthenticate after connecting to Photon.
/// </summary>
public enum CustomAuthenticationType : byte
{
    /// <summary>Use a custom authentification service. Currently the only implemented option.</summary>
    Custom = 0,

    /// <summary>Authenticates users by their Steam Account. Set auth values accordingly!</summary>
    Steam = 1,

    /// <summary>Authenticates users by their Facebook Account. Set auth values accordingly!</summary>
    Facebook = 2,

    /// <summary>Disables custom authentification. Same as not providing any AuthenticationValues for connect (more precisely for: OpAuthenticate).</summary>
    None = byte.MaxValue
}

/// <summary>
/// Container for "Custom Authentication" values in Photon (default: user and token). Set AuthParameters before connecting - all else is handled.
/// </summary>
/// <remarks>
/// Custom Authentication lets you verify end-users by some kind of login or token. It sends those
/// values to Photon which will verify them before granting access or disconnecting the client.
///
/// The Photon Cloud Dashboard will let you enable this feature and set important server values for it.
/// https://cloud.exitgames.com/dashboard
/// </remarks>
public class AuthenticationValues
{
    /// <summary>The type of custom authentication provider that should be used. Currently only "Custom" or "None" (turns this off).</summary>
    public CustomAuthenticationType AuthType = CustomAuthenticationType.Custom;

    /// <summary>This string must contain any (http get) parameters expected by the used authentication service. By default, username and token.</summary>
    /// <remarks>Standard http get parameters are used here and passed on to the service that's defined in the server (Photon Cloud Dashboard).</remarks>
    public string AuthParameters;   // { get { return a; } set { a = value; UnityEngine.Debug.LogWarning("AuthParameters set: " + value + " server: " + PhotonNetwork.ServerAddress); } }
    //private string a;

    /// <summary>After initial authentication, Photon provides a secret for this client / user, which is subsequently used as (cached) validation.</summary>
    public string Secret;   // { get { return s; } set { s = value; UnityEngine.Debug.LogWarning("Secret set: " + value + " server: " + PhotonNetwork.ServerAddress); } }
    //private string s;

    /// <summary>Data to be passed-on to the auth service via POST. Default: null (not sent). Either string or byte[] (see setters).</summary>
    public object AuthPostData { get; private set; }

    /// <summary>Sets the data to be passed-on to the auth service via POST.</summary>
    /// <param name="byteData">Binary token / auth-data to pass on. Empty string will set AuthPostData to null.</param>
    public virtual void SetAuthPostData(string stringData)
    {
        this.AuthPostData = (string.IsNullOrEmpty(stringData)) ? null : stringData;
    }

    /// <summary>Sets the data to be passed-on to the auth service via POST.</summary>
    /// <param name="byteData">Binary token / auth-data to pass on.</param>
    public virtual void SetAuthPostData(byte[] byteData)
    {
        this.AuthPostData = byteData;
    }

    /// <summary>Creates the default parameter string from a user and token value, escaping both. Alternatively set AuthParameters yourself.</summary>
    /// <remarks>The default parameter string is: "username={user}&token={token}"</remarks>
    /// <param name="user">Name or other end-user ID used in custom authentication service.</param>
    /// <param name="token">Token provided by authentication service to be used on initial "login" to Photon.</param>
    public virtual void SetAuthParameters(string user, string token)
    {
        this.AuthParameters = "username=" + System.Uri.EscapeDataString(user) + "&token=" + System.Uri.EscapeDataString(token);
    }

    public override string ToString()
    {
        return AuthParameters + " s: " + Secret;
    }
}
