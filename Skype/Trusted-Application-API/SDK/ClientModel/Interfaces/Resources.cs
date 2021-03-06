﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Rtc.Internal.Platform.ResourceContract;
using Microsoft.SfB.PlatformService.SDK.Common;

namespace Microsoft.SfB.PlatformService.SDK.ClientModel
{
    #region public interface IPlatformResource

    /// <summary>
    /// Represents a Resource returned by PlatformService.
    /// </summary>
    /// <typeparam name="TCapabilities">
    /// An <see cref="enum"/> listing all the capabilties that this <see cref="IPlatformResource{TCapabilities}"/> supports. Capabilities
    /// might not be available at runtime, such cases can be handled by invoking <see cref="Supports(TCapabilities)"/> when a capabilty
    /// needs to be used.
    /// </typeparam>
    public interface IPlatformResource<TCapabilities>
    {
        /// <summary>
        /// Base <see cref="Uri"/> of this Resource. It is defined as the <see cref="string"/> concatenation of
        /// <see cref="ResourceUri"/>'s <see cref="Uri.Scheme"/> and <see cref="Uri.Authority"/>.
        /// </summary>
        /// <remarks>
        /// Most of the <see cref="IPlatformResource{TCapabilities}"/> will have same value of this property.
        /// </remarks>
        /// <example>https://platformservice.example.com/</example>
        Uri BaseUri { get; }

        /// <summary>
        /// HTTP <see cref="Uri"/> of this Resource.
        /// </summary>
        /// <remarks>
        /// Each Resource has a unique <see cref="Uri"/>, which can never change once a Resource is created.
        /// </remarks>
        Uri ResourceUri { get; }

        /// <summary>
        /// Resources returned by PlatformService are in a hierarchy. This property returns the parent of current Resource.
        /// </summary>
        object Parent { get; }

        /// <summary>
        /// <see cref="EventHandler"/> that will be invoked when this Resource gets updated.
        /// </summary>
        EventHandler<PlatformResourceEventArgs> HandleResourceUpdated { get; set; }

        /// <summary>
        /// <see cref="EventHandler"/> that will be invoked when this Resource receives a Completed event.
        /// </summary>
        EventHandler<PlatformResourceEventArgs> HandleResourceCompleted { get; set; }

        /// <summary>
        /// <see cref="EventHandler"/> that will be invoked when this Resource gets removed/deleted.
        /// </summary>
        EventHandler<PlatformResourceEventArgs> HandleResourceRemoved { get; set; }

        /// <summary>
        /// Fetches this Resource from PlatformService and updates it.
        /// </summary>
        /// <param name="loggingContext"><see cref="LoggingContext"/> to be used for logging all related events.</param>
        /// <returns><see cref="void"/> it is an async method.</returns>
        /// <remarks><see cref="HandleResourceUpdated"/> <see cref="EventHandler"/> will be invoked when the refresh succeeds.</remarks>
        Task RefreshAsync(LoggingContext loggingContext);

        /// <summary>
        /// Deletes this Resource from PlatformService.
        /// </summary>
        /// <param name="loggingContext"><see cref="LoggingContext"/> to be used for logging all related events.</param>
        /// <returns><see cref="void"/> it is an async method.</returns>
        /// <remarks><see cref="HandleResourceRemoved"/> <see cref="EventHandler"/> will be invoked when the delete succeeds.</remarks>
        Task DeleteAsync(LoggingContext loggingContext);

        /// <summary>
        /// Gets whether a particular capability is available or not.
        /// </summary>
        /// <param name="capability">Capability that needs to be checked.</param>
        /// <returns><code>true</code> iff the capability is available at the time of invoking.</returns>
        /// <remarks>
        /// Capabilities can change when a resource is updated. So, this method returning <code>true</code> doesn't guarantee that
        /// the capability will be available when it is actually used. Make sure to catch <see cref="CapabilityNotAvailableException"/>
        /// when you are using a capability.
        /// </remarks>
        bool Supports(TCapabilities capability);
    }

    #endregion

    #region public interface IApplication

    /// <summary>
    /// Represents your real-time communication application.
    /// </summary>
    /// <remarks>
    /// This resource represents an application which is similar to a bot in functionality and is not bound to any user.
    /// </remarks>
    public interface IApplication : IPlatformResource<ApplicationCapability>
    {
        /// <summary>
        /// <see cref="ICommunication"/> resource of this <see cref="IApplication"/> that can be used to initiate calls and conversations.
        /// </summary>
        ICommunication Communication { get; }

        /// <summary>
        /// Refreshes this <see cref="IApplication"/> and populates all encapsulated Resources.
        /// </summary>
        /// <param name="loggingContext"><see cref="LoggingContext"/> to be used for logging all related events.</param>
        /// <returns><see cref="void"/> it is an async method.</returns>
        Task RefreshAndInitializeAsync(LoggingContext loggingContext);

        /// <summary>
        /// Gets an anonymous application token for a meeting. This token can be given to a user domain application. Using this token,
        /// the user can sign in and join the meeting.
        /// </summary>
        /// <param name="loggingContext"><see cref="LoggingContext"/> to be used for logging all related events.</param>
        /// <param name="input">Specifyies properties for the required token.</param>
        /// <returns>A token that can be used by a user to join the specified meeting.</returns>
        Task<AnonymousApplicationTokenResource> GetAnonApplicationTokenAsync(LoggingContext loggingContext, AnonymousApplicationTokenInput input);
        
        
        /// <summary>
        /// Gets adhoc meeting tokens
        /// </summary>
        /// <param name="loggingContext"><see cref="LoggingContext"/> to be used for logging all related events.</param>
        /// <param name="input">Specifyies properties for the required token.</param>
        /// <returns>A adhoc meeting tokens.</returns>
        Task<AdhocMeetingResource> GetAdhocMeetingResourceAsync(LoggingContext loggingContext, AdhocMeetingInput input); 
    }

    #endregion

    #region public interface IApplicationEndpoint

    public interface IApplicationEndpoint
    {
        event EventHandler<IncomingInviteEventArgs<IMessagingInvitation>> HandleIncomingInstantMessagingCall;

        event EventHandler<IncomingInviteEventArgs<IAudioVideoInvitation>> HandleIncomingAudioVideoCall;

        IApplication Application { get; }

        Uri ApplicationEndpointId { get; }

        IClientPlatform ClientPlatform { get; }

        Task InitializeAsync(LoggingContext loggingContext);

        Task InitializeApplicationAsync(LoggingContext loggingContext);

        void Uninitialize();
    }

    #endregion

    #region public interface IApplications

    public interface IApplications : IPlatformResource<ApplicationsCapability>
    {
        IApplication Application { get; }

        Task RefreshAndInitializeAsync(LoggingContext loggingContext);
    }

    #endregion

    #region public interface IAudioVideoCall

    public interface IAudioVideoCall : ICall<IAudioVideoInvitation>, IPlatformResource<AudioVideoCallCapability>
    {
        event EventHandler<AudioVideoFlowUpdatedEventArgs> AudioVideoFlowConnected;

        string CallContext { get; }

        IAudioVideoFlow AudioVideoFlow { get; }

        Task<ITransfer> TransferAsync(string transferTarget, string replacesCallContext, LoggingContext loggingContext);

        Task<IAudioVideoFlow> WaitForAVFlowConnected(int timeoutInSeconds = 30);
    }

    #endregion

    #region public interface IAudioVideoFlow

    public interface IAudioVideoFlow : IPlatformResource<AudioVideoFlowCapability>
    {
        event EventHandler<ToneReceivedEventArgs> ToneReceivedEvent;

        FlowState State { get; }

        Task<IPrompt> PlayPromptAsync(Uri promptUri, LoggingContext loggingContext);
    }

    #endregion

    #region public interface IAudioVideoInvitation

    public interface IAudioVideoInvitation : IInvitation, IPlatformResource<AudioVideoInvitationCapability>
    {
        Task<HttpResponseMessage> AcceptAsync(LoggingContext loggingContext);

        Task<HttpResponseMessage> ForwardAsync(LoggingContext loggingContext, string forwardTarget);

        Task<HttpResponseMessage> DeclineAsync(LoggingContext loggingContext);
    }

    #endregion

    #region public interface IBridgedParticipant

    public interface IBridgedParticipant : IPlatformResource<BridgedParticipantCapability>
    {
        Task UpdateAsync(LoggingContext logginContext, string displayName, bool isEnableFilter);
    }

    #endregion

    #region public interface IBridgedParticipants

    public interface IBridgedParticipants : IPlatformResource<BridgedParticipantsCapability>
    {
    }

    #endregion

    #region public interface ICall

    public interface ICall<TInvitation>
    {
        CallState State { get; }

        event EventHandler<CallStateChangedEventArgs> CallStateChanged;

        Task<TInvitation> EstablishAsync(LoggingContext loggingContext);

        Task TerminateAsync(LoggingContext loggingContext);
    }

    #endregion

    #region public interface IClientPlatform

    public interface IClientPlatform
    {
        Uri DiscoverUri { get; }

        Guid AADClientId { get; }

        string AADClientSecret { get; }

        X509Certificate2 AADAppCertificate { get; }

        bool IsInternalPartner { get; }

        bool IsSandBoxEnv { get; }
    }

    #endregion

    #region public interface ICommunication

    public interface ICommunication : IPlatformResource<CommunicationCapability>
    {
        Task<IMessagingInvitation> StartMessagingAsync(string subject, string to, string callbackUrl, LoggingContext loggingContext = null);

        Task<IMessagingInvitation> StartMessagingWithIdentityAsync(string subject, string to, string callbackUrl, string localUserDisplayName, string localUserUri, LoggingContext loggingContext = null);

        Task<IAudioVideoInvitation> StartAudioVideoAsync(string subject, string to, string callbackUrl, LoggingContext loggingContext = null);

        Task<IAudioVideoInvitation> StartAudioAsync(string subject, string to, string callbackUrl, LoggingContext loggingContext = null);
    }

    #endregion

    #region public interface IConversation

    public interface IConversation : IPlatformResource<ConversationCapability>
    {
        /// <summary>
        /// Gets the list of active modalities for this Conversation.
        /// </summary>
        IReadOnlyCollection<ConversationModalityType> ActiveModalities { get; }

        /// <summary>
        /// State of Conversation
        /// </summary>
        ConversationState State { get; }

        /// <summary>
        /// Get The messaging call
        /// </summary>
        IMessagingCall MessagingCall { get; }

        IAudioVideoCall AudioVideoCall { get; }

        /// <summary>
        ///
        /// </summary>
        IConversationBridge ConversationBridge { get; }

        IConversationConference ConversationConference { get; }

        /// <summary>
        /// Get the participants
        /// </summary>
        List<IParticipant> Participants { get; }

        event EventHandler<ParticipantChangeEventArgs> HandleParticipantChange;

        event EventHandler<ConversationStateChangedEventArgs> ConversationStateChanged;

        IParticipant TryGetParticipant(string href);

        Task<IParticipantInvitation> AddParticipantAsync(string targetSip, LoggingContext loggingContext);
    }

    #endregion

    #region public interface IConversationBridge

    public interface IConversationBridge : IPlatformResource<ConversationBridgeCapability>
    {
        List<IBridgedParticipant> BridgedParticipants { get; }

        Task<IBridgedParticipant> AddBridgedParticipantAsync(LoggingContext logginContext, string displayName, string sipUri, bool isEnableFilter);
    }

    #endregion

    #region public interface IConversationConference

    public interface IConversationConference : IPlatformResource<ConversationConferenceCapability>
    {
        string OnlineMeetingUri { get; }

        Task TerminateAsync(LoggingContext loggingContext = null);
    }

    #endregion

    #region public interface IDiscover

    public interface IDiscover : IPlatformResource<DiscoverCapability>
    {
        /// <summary>
        /// Get Appplication
        /// </summary>
        IApplications Applications { get; }

        Task RefreshAndInitializeAsync(LoggingContext loggingContext, string endpointId);
    }

    #endregion

    #region public interface IEventChannel

    public interface IEventChannel
    {
        /// <summary>
        /// Event handler for incoming events
        /// </summary>
        event EventHandler<EventsChannelArgs> HandleIncomingEvents;

        /// <summary>
        /// Start Event Channel
        /// </summary>
        /// <returns></returns>
        Task TryStartAsync();

        /// <summary>
        /// Stop Event Channel
        /// </summary>
        /// <returns></returns>
        Task TryStopAsync();
    }

    #endregion

    #region public interface IInvitation

    public interface IInvitation
    {
        /// <summary>
        /// Wait for invite complete
        /// </summary>
        /// <returns></returns>
        Task WaitForInviteCompleteAsync();

        /// <summary>
        /// The related conversation
        /// </summary>
        IConversation RelatedConversation { get; }
    }

    #endregion

    #region public interface IMessagingInvitation

    public interface IMessagingInvitation : IInvitation, IPlatformResource<MessagingInvitationCapability>
    {
        Task<IOnlineMeetingInvitation> StartAdhocMeetingAsync(string subject, string callbackUrl, LoggingContext loggingContext = null);

        Task AcceptAndBridgeAsync(LoggingContext loggingContext, string meetingUrl, string displayName);
    }

    #endregion

    #region public interface IMessagingCall

    public interface IMessagingCall : ICall<IMessagingInvitation>, IPlatformResource<MessagingCallCapability>
    {
        EventHandler<IncomingMessageEventArgs> IncomingMessageReceived { get; set; }

        Task SendMessageAsync(string message, LoggingContext loggingContext, string contentType = Constants.TextPlainContentType);
    }

    #endregion

    #region public interface IOnlineMeetingInvitation

    public interface IOnlineMeetingInvitation : IInvitation, IPlatformResource<OnlineMeetingInvitationCapability>
    {
        /// <summary>
        /// Anonymous meeting url
        /// </summary>
        string MeetingUrl { get; }
    }

    #endregion

    #region public interface IParticipant

    public interface IParticipant : IPlatformResource<ParticipantCapability>
    {
        /// <summary>
        /// Get the uri of participant
        /// </summary>
        string Uri { get; }

        /// <summary>
        /// Get the name of participant
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The participant messaging resource
        /// </summary>
        IParticipantMessaging ParticipantMessaging { get; }

        event EventHandler<ParticipantModalityChangeEventArgs> HandleParticipantModalityChange;
    }

    #endregion

    #region public interface IParticipantInvitation

    public interface IParticipantInvitation : IInvitation, IPlatformResource<ParticipantInvitationCapability>
    {
    }

    #endregion

    #region public interface IParticipantMessaging

    public interface IParticipantMessaging : IPlatformResource<ParticipantMessagingCapability>
    {
    }

    #endregion

    #region public interface IPrompt

    public interface IPrompt : IPlatformResource<PromptCapability>
    {
    }

    #endregion

    #region public interface ITransfer

    public interface ITransfer : IPlatformResource<TransferCapability>
    {
        Task WaitForTransferCompleteAsync();
    }

    #endregion
}
