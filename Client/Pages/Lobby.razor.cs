using Blazored.Modal;
using Blazored.Modal.Services;
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
        [Inject] protected NavigationManager Navigation { get; set; }
        [CascadingParameter] public IModalService InvitationOptionsModal { get; set; }

        protected List<string> _onlineUsers = new List<string>();
        private bool _isWaiting;
        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/hub-lobby"))
                .Build();

            hubConnection.On<List<string>>("GetAllUsers", (users) => {
                _onlineUsers = users;
                StateHasChanged();
            });

            hubConnection.On<string>("UserLeft", (nickname) => {
                _onlineUsers.Remove(nickname);
                StateHasChanged();
            });

            hubConnection.On<string>("UserJoined", (nickname) => {
                _onlineUsers.Add(nickname);
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

                _isWaiting = true;
                //await WaitForConfirmation(invitationDetails);
            }
        }

        //private async Task WaitForConfirmation(InvitationDetails details)
        //{
        //    var parameters = new ModalParameters();
        //    parameters.Add(nameof(InvitationDetails), details);
        //    var formModal = InvitationOptionsModal.Show<InvitaionWaitingModal>("Waiting...", parameters, );
        //    var result = await formModal.Result;
            
        //    if (result.Cancelled) {
        //        await hubConnection.SendAsync("InvitationCancelled", details);
        //    } else {
        //        // TODO navigovať do hry
        //    }
        //}

        public async ValueTask DisposeAsync()
        {
            await hubConnection.DisposeAsync();
        }
    }
}
