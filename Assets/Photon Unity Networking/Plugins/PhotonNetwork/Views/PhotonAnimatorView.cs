using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class helps you to synchronize Mecanim animations
/// Simply add the component to your GameObject and make sure that
/// the PhotonAnimatorView is added to the list of observed components
/// </summary>
[RequireComponent( typeof( Animator ) )]
[RequireComponent( typeof( PhotonView ) )]
[AddComponentMenu("Photon Networking/Photon Animator View")]
public class PhotonAnimatorView : MonoBehaviour
{
    #region Enums
    public enum ParameterType
    {
        Float = 1,
        Int = 3,
        Bool = 4,
        Trigger = 9,
    }

    public enum SynchronizeType
    {
        Disabled = 0,
        Discrete = 1,
        Continuous = 2,
    }

    [System.Serializable]
    public class SynchronizedParameter
    {
        public ParameterType Type;
        public SynchronizeType SynchronizeType;
        public string Name;
    }

    [System.Serializable]
    public class SynchronizedLayer
    {
        public SynchronizeType SynchronizeType;
        public int LayerIndex;
    }
    #endregion

    #region Properties
#if PHOTON_DEVELOP
    public PhotonAnimatorView ReceivingSender;
#endif
    #endregion

    #region Members
    Animator m_Animator;

    PhotonStreamQueue m_StreamQueue;

    //These fields are only used in the CustomEditor for this script and would trigger a 
    //"this variable is never used" warning, which I am suppressing here
#pragma warning disable 0414
    [HideInInspector]
    [SerializeField]
    bool ShowLayerWeightsInspector = true;

    [HideInInspector]
    [SerializeField]
    bool ShowParameterInspector = true;
#pragma warning restore 0414

    [HideInInspector]
    [SerializeField]
    List<SynchronizedParameter> m_SynchronizeParameters = new List<SynchronizedParameter>();

    [HideInInspector]
    [SerializeField]
    List<SynchronizedLayer> m_SynchronizeLayers = new List<SynchronizedLayer>();

    Vector3 m_ReceiverPosition;
    float m_LastDeserializeTime;
    bool m_WasSynchronizeTypeChanged = true;
    PhotonView m_PhotonView;
    #endregion

    #region Unity
    void Awake() 
    {
        m_PhotonView = GetComponent<PhotonView>();
        m_StreamQueue = new PhotonStreamQueue( 120 );

        m_Animator = GetComponent<Animator>();
    }

    void Update()
    {
        if( m_PhotonView.isMine == false && PhotonNetwork.connected == true )
        {
            m_Animator.applyRootMotion = false;
        }

        if( m_PhotonView.isMine == true )
        {
            SerializeDataContinuously();
        }
        else
        {
            DeserializeDataContinuously();
        }
    }
    #endregion

    #region Setup Synchronizing Methods
    /// <summary>
    /// Check if a specific layer is configured to be synchronize
    /// </summary>
    /// <param name="layerIndex">Index of the layer.</param>
    /// <returns>True if the layer is synchronized</returns>
    public bool DoesLayerSynchronizeTypeExist( int layerIndex )
    {
        return m_SynchronizeLayers.FindIndex( item => item.LayerIndex == layerIndex ) != -1;
    }

    /// <summary>
    /// Check if the specified parameter is configured to be synchronized
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <returns>True if the parameter is synchronized</returns>
    public bool DoesParameterSynchronizeTypeExist( string name )
    {
        return m_SynchronizeParameters.FindIndex( item => item.Name == name ) != -1;
    }

    /// <summary>
    /// Get a list of all synchronized layers
    /// </summary>
    /// <returns>List of SynchronizedLayer objects</returns>
    public List<SynchronizedLayer> GetSynchronizedLayers()
    {
        return m_SynchronizeLayers;
    }

    /// <summary>
    /// Get a list of all synchronized parameters
    /// </summary>
    /// <returns>List of SynchronizedParameter objects</returns>
    public List<SynchronizedParameter> GetSynchronizedParameters()
    {
        return m_SynchronizeParameters;
    }

    /// <summary>
    /// Gets the type how the layer is synchronized
    /// </summary>
    /// <param name="layerIndex">Index of the layer.</param>
    /// <returns>Disabled/Discrete/Continuous</returns>
    public SynchronizeType GetLayerSynchronizeType( int layerIndex )
    {
        int index = m_SynchronizeLayers.FindIndex( item => item.LayerIndex == layerIndex );

        if( index == -1 )
        {
            return SynchronizeType.Disabled;
        }

        return m_SynchronizeLayers[ index ].SynchronizeType;
    }

    /// <summary>
    /// Gets the type how the parameter is synchronized
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <returns>Disabled/Discrete/Continuous</returns>
    public SynchronizeType GetParameterSynchronizeType( string name )
    {
        int index = m_SynchronizeParameters.FindIndex( item => item.Name == name );

        if( index == -1 )
        {
            return SynchronizeType.Disabled;
        }

        return m_SynchronizeParameters[ index ].SynchronizeType;
    }

    /// <summary>
    /// Sets the how a layer should be synchronized
    /// </summary>
    /// <param name="layerIndex">Index of the layer.</param>
    /// <param name="synchronizeType">Disabled/Discrete/Continuous</param>
    public void SetLayerSynchronized( int layerIndex, SynchronizeType synchronizeType )
    {
        if( Application.isPlaying == true )
        {
            m_WasSynchronizeTypeChanged = true;
        }

        int index = m_SynchronizeLayers.FindIndex( item => item.LayerIndex == layerIndex );

        if( index == -1 )
        {
            m_SynchronizeLayers.Add( new SynchronizedLayer { LayerIndex = layerIndex, SynchronizeType = synchronizeType } );
        }
        else
        {
            m_SynchronizeLayers[ index ].SynchronizeType = synchronizeType;
        }
    }

    /// <summary>
    /// Sets the how a parameter should be synchronized
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="synchronizeType">Disabled/Discrete/Continuous</param>
    public void SetParameterSynchronized( string name, ParameterType type, SynchronizeType synchronizeType )
    {
        if( Application.isPlaying == true )
        {
            m_WasSynchronizeTypeChanged = true;
        }

        int index = m_SynchronizeParameters.FindIndex( item => item.Name == name );

        if( index == -1 )
        {
            m_SynchronizeParameters.Add( new SynchronizedParameter { Name = name, Type = type, SynchronizeType = synchronizeType } );
        }
        else
        {
            m_SynchronizeParameters[ index ].SynchronizeType = synchronizeType;
        }
    }
    #endregion

    #region Serialization
    void SerializeDataContinuously()
    {
        if( m_Animator == null )
        {
            return;
        }

        for( int i = 0; i < m_SynchronizeLayers.Count; ++i )
        {
            if( m_SynchronizeLayers[ i ].SynchronizeType == SynchronizeType.Continuous )
            {
                m_StreamQueue.SendNext( m_Animator.GetLayerWeight( m_SynchronizeLayers[ i ].LayerIndex ) );
            }
        }

        for( int i = 0; i < m_SynchronizeParameters.Count; ++i )
        {
            SynchronizedParameter parameter = m_SynchronizeParameters[ i ];

            if( parameter.SynchronizeType == SynchronizeType.Continuous )
            {
                
                switch( parameter.Type )
                {
                case ParameterType.Bool:
                    m_StreamQueue.SendNext( m_Animator.GetBool( parameter.Name ) );
                    break;
                case ParameterType.Float:
                    m_StreamQueue.SendNext( m_Animator.GetFloat( parameter.Name ) );
                    break;
                case ParameterType.Int:
                    m_StreamQueue.SendNext( m_Animator.GetInteger( parameter.Name ) );
                    break;
                case ParameterType.Trigger:

                    break;
                }
            }
        }
    }

    void DeserializeDataContinuously()
    {
        if( m_StreamQueue.HasQueuedObjects() == false )
        {
            return;
        }

        for( int i = 0; i < m_SynchronizeLayers.Count; ++i )
        {
            if( m_SynchronizeLayers[ i ].SynchronizeType == SynchronizeType.Continuous )
            {
                m_Animator.SetLayerWeight( m_SynchronizeLayers[ i ].LayerIndex, (float)m_StreamQueue.ReceiveNext() );
            }
        }

        for( int i = 0; i < m_SynchronizeParameters.Count; ++i )
        {
            SynchronizedParameter parameter = m_SynchronizeParameters[ i ];

            if( parameter.SynchronizeType == SynchronizeType.Continuous )
            {
                switch( parameter.Type )
                {
                case ParameterType.Bool:
                    m_Animator.SetBool( parameter.Name, (bool)m_StreamQueue.ReceiveNext() );
                    break;
                case ParameterType.Float:
                    m_Animator.SetFloat( parameter.Name, (float)m_StreamQueue.ReceiveNext() );
                    break;
                case ParameterType.Int:
                    m_Animator.SetInteger( parameter.Name, (int)m_StreamQueue.ReceiveNext() );
                    break;
                case ParameterType.Trigger:

                    break;
                }
            }
        }
    }

    void SerializeDataDiscretly( PhotonStream stream )
    {
        for( int i = 0; i < m_SynchronizeLayers.Count; ++i )
        {
            if( m_SynchronizeLayers[ i ].SynchronizeType == SynchronizeType.Discrete )
            {
                stream.SendNext( m_Animator.GetLayerWeight( m_SynchronizeLayers[ i ].LayerIndex ) );
            }
        }

        for( int i = 0; i < m_SynchronizeParameters.Count; ++i )
        {
            SynchronizedParameter parameter = m_SynchronizeParameters[ i ];

            if( parameter.SynchronizeType == SynchronizeType.Discrete )
            {
                switch( parameter.Type )
                {
                case ParameterType.Bool:
                    stream.SendNext( m_Animator.GetBool( parameter.Name ) );
                    break;
                case ParameterType.Float:
                    stream.SendNext( m_Animator.GetFloat( parameter.Name ) );
                    break;
                case ParameterType.Int:
                    stream.SendNext( m_Animator.GetInteger( parameter.Name ) );
                    break;
                case ParameterType.Trigger:

                    break;
                }
            }
        }
    }

    void DeserializeDataDiscretly( PhotonStream stream )
    {
        for( int i = 0; i < m_SynchronizeLayers.Count; ++i )
        {
            if( m_SynchronizeLayers[ i ].SynchronizeType == SynchronizeType.Discrete )
            {
                m_Animator.SetLayerWeight( m_SynchronizeLayers[ i ].LayerIndex, (float)stream.ReceiveNext() );
            }
        }

        for( int i = 0; i < m_SynchronizeParameters.Count; ++i )
        {
            SynchronizedParameter parameter = m_SynchronizeParameters[ i ];

            if( parameter.SynchronizeType == SynchronizeType.Discrete )
            {
                switch( parameter.Type )
                {
                case ParameterType.Bool:
                    if( stream.PeekNext() is bool == false )
                    {
                        return;
                    }

                    m_Animator.SetBool( parameter.Name, (bool)stream.ReceiveNext() );
                    break;
                case ParameterType.Float:
                    if( stream.PeekNext() is float == false )
                    {
                        return;
                    }

                    m_Animator.SetFloat( parameter.Name, (float)stream.ReceiveNext() );
                    break;
                case ParameterType.Int:
                    if( stream.PeekNext() is int == false )
                    {
                        return;
                    }

                    m_Animator.SetInteger( parameter.Name, (int)stream.ReceiveNext() );
                    break;
                case ParameterType.Trigger:

                    break;
                }
            }
        }
    }

    void SerializeSynchronizationTypeState( PhotonStream stream )
    {
        byte[] states = new byte[ m_SynchronizeLayers.Count + m_SynchronizeParameters.Count ];

        for( int i = 0; i < m_SynchronizeLayers.Count; ++i )
        {
            states[ i ] = (byte)m_SynchronizeLayers[ i ].SynchronizeType;
        }

        for( int i = 0; i < m_SynchronizeParameters.Count; ++i )
        {
            states[ m_SynchronizeLayers.Count + i ] = (byte)m_SynchronizeParameters[ i ].SynchronizeType;
        }

        stream.SendNext( states );
    }

    void DeserializeSynchronizationTypeState( PhotonStream stream )
    {
        byte[] state = (byte[])stream.ReceiveNext();

        for( int i = 0; i < m_SynchronizeLayers.Count; ++i )
        {
            m_SynchronizeLayers[ i ].SynchronizeType = (SynchronizeType)state[ i ];
        }

        for( int i = 0; i < m_SynchronizeParameters.Count; ++i )
        {
            m_SynchronizeParameters[ i ].SynchronizeType = (SynchronizeType)state[ m_SynchronizeLayers.Count + i ];
        }
    }

    void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        if( m_Animator == null )
        {
            return;
        }

        if( stream.isWriting == true )
        {
            if( m_WasSynchronizeTypeChanged == true )
            {
                m_StreamQueue.Reset();
                SerializeSynchronizationTypeState( stream );

                m_WasSynchronizeTypeChanged = false;
            }

            m_StreamQueue.Serialize( stream );
            SerializeDataDiscretly( stream );
        }
        else
        {
#if PHOTON_DEVELOP
            if( ReceivingSender != null )
            {
                ReceivingSender.OnPhotonSerializeView( stream, info );
            }
            else
#endif
            {
                if( stream.PeekNext() is byte[] )
                {
                    DeserializeSynchronizationTypeState( stream );
                }

                m_StreamQueue.Deserialize( stream );
                DeserializeDataDiscretly( stream );
            }
        }
    }
    #endregion
}
