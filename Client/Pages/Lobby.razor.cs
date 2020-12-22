using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using POT.Pexeso.Client.Shared;
using POT.Pexeso.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POT.Pexeso.Client.Pages
{
    public partial class Lobby : IAsyncDisposable
    {
        [Inject] public NavigationManager Navigation { get; set; }
        [Inject] public IToastService Toast { get; set; }
        [CascadingParameter] public IModalService InvitationOptionsModal { get; set; }

        protected List<UserDisplayInfo> _onlineUsers = new List<UserDisplayInfo>();
        private HubConnection hubConnection;

        // attribute bind to invitation phase
        private bool _isWaiting;
        private int _timer = 15;
        private InvitationDetails _invitationDetails;

        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/hub-lobby"))
                .Build();

            hubConnection.On<List<UserDisplayInfo>>("GetAllUsers", (users) => {
                _onlineUsers = users;
                StateHasChanged();
            });

            hubConnection.On<string>("UserLeft", (nickname) => {
                var user = _onlineUsers.First(user => user.Nickname == nickname);
                _onlineUsers.Remove(user);
                StateHasChanged();
            });

            hubConnection.On<string>("UserJoined", (nickname) => {
                _onlineUsers.Add(new UserDisplayInfo { Nickname = nickname, Status = Status.Online });
                StateHasChanged();
            });

            hubConnection.On<InvitationDetails>("ReceiveInvitation", (details) => {
                _invitationDetails = details;
                _isWaiting = true;
                StateHasChanged();
            });

            hubConnection.On<string, Status>("UserStatusChanged", (nick, status) => {
                var user = _onlineUsers.First(user => user.Nickname == nick);
                user.Status = status;
                StateHasChanged();
            });

            hubConnection.On<int>("TimerChanged", (ticks) => {
                _timer = ticks;
                StateHasChanged();
            });

            hubConnection.On<bool>("InvitationCancelled", (rejected) => {
                ResetInvitationAttributes();
                if (rejected) {
                    Toast.ShowInfo("Invitation was rejected.");
                } else {
                    Toast.ShowInfo("Invitation was cancelled.");
                }
                StateHasChanged();
            });

            await hubConnection.StartAsync();
        }

        public bool IsConnected =>
            hubConnection.State == HubConnectionState.Connected;

        protected async Task HandleInvitation(string nick)
        {
            var formModal = InvitationOptionsModal.Show<InvitationForm>("Game Settings");
            var result = await formModal.Result;

            if (!result.Cancelled) {
                var gameDetails = (GameDetails)result.Data;
                var invitationDetails = new InvitationDetails { GameDetails = gameDetails, NicknameTo = nick };
                await hubConnection.SendAsync("SendInvitation", invitationDetails);

                _invitationDetails = invitationDetails;
                _isWaiting = true;
            }
        }

        Task Send() =>
        hubConnection.SendAsync("CancelInvitation", _invitationDetails);


        private async Task CancelInvitation(bool rejected = false)
        {
            _invitationDetails.Rejected = rejected;
            await hubConnection.SendAsync("CancelInvitation", _invitationDetails);
            ResetInvitationAttributes();
        }

        private async Task AcceptInvitation()
        {
            await hubConnection.SendAsync("AcceptInvitation", _invitationDetails);
            ResetInvitationAttributes();
        }

        private void ResetInvitationAttributes()
        {
            _isWaiting = false;
            _invitationDetails = null;
            _timer = GameDetails.ResponseDelay;
        }

        public async ValueTask DisposeAsync()
        {
            await hubConnection.DisposeAsync();
        }
    }
}
