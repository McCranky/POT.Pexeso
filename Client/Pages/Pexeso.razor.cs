using Blazored.LocalStorage;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using POT.Pexeso.Shared;
using POT.Pexeso.Shared.Pexeso;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace POT.Pexeso.Client.Pages
{
    public partial class Pexeso : IAsyncDisposable
    {
        [Inject] public NavigationManager Navigation { get; set; }
        [Inject] public IToastService Toast { get; set; }
        [Inject] public ILocalStorageService Storage { get; set; }
        [Inject] public HttpClient Http { get; set; }

        [CascadingParameter]
        public Task<AuthenticationState> AuthState { get; set; }

        private HubConnection _hubConnection;
        private GameState _gameState;
        private Player _me = new Player();
        private Player _opponent = new Player();
        private Player _onTheMove = new Player();
        private GameSettings _gameSettings;
        private Board _board;
        private int _timeout;
        private bool _isTimeout;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthState;
            _me.Nickname = authState.User.Identity.Name;
            //var invDetails = await Storage.GetItemAsync<InvitationDetails>("invitationDetails");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/hub-game"))
                .Build();

            _hubConnection.On<Player, Player, Player, GameSettings, List<Card>>("GetGameInfo", async (challenger, opponent, onTheMove, settings, cards) => {
                _opponent = challenger.Nickname == _me.Nickname ? opponent : challenger;
                _me = challenger.Nickname == _me.Nickname ? challenger : opponent;

                _gameState = GameState.Running;
                _gameSettings = settings;
                var card = await Http.GetFromJsonAsync<CardBackInfo>($"resource/get/{_gameSettings.CardBack.Id}");
                _gameSettings.CardBack = card;

                _onTheMove = onTheMove.Nickname == _me.Nickname ? _me : _opponent;
                _board = new Board(settings.BoardSize, cards);
                StateHasChanged();
            });

            _hubConnection.On<Player>("ChangeTurn", (onTheMove) => {
                _onTheMove = onTheMove.Nickname == _me.Nickname ? _me : _opponent;
                StateHasChanged();
            });

            _hubConnection.On<GameState>("ChangeGameState", (state) => {
                if (_gameState == GameState.Running && state == _gameState) {
                    Toast.ShowInfo("Game Started");
                }
                _gameState = state;

                if (_gameState == GameState.Gameover) {
                    Toast.ShowInfo("Game is over. Thanks for playing :)");
                }
                StateHasChanged();
            });

            _hubConnection.On("TimeoutWarning", () => {
                if (_onTheMove == _me) {
                    Toast.ShowWarning("Make a move until time runs up.");
                } else {
                    Toast.ShowInfo("Countdown started.");
                }
                _isTimeout = true;
                StateHasChanged();
            });

            _hubConnection.On<int>("TimeoutChanged", (time) => {
                _timeout = time;
                StateHasChanged();
            });

            _hubConnection.On("ExitGame", async () => {
                Toast.ShowInfo("Time runs out. Game record will be discarded.");
                await Task.Delay(3000);
                Navigation.NavigateTo("/");
            });

            _hubConnection.On("Gameover", async () => {
                Toast.ShowInfo("Game result is stored in database, see History tab.");
                await Task.Delay(3000);
                Navigation.NavigateTo("/");
            });

            _hubConnection.On<Player>("ChangeScore", (player) => {
                if (player.Nickname == _me.Nickname) {
                    _me.Score = player.Score;
                } else {
                    _opponent.Score = player.Score;
                }

                StateHasChanged();
            });

            _hubConnection.On<int, int>("FlipCard", (height, width) => {
                _board[height, width].IsFlipped = !_board[height, width].IsFlipped;
                _isTimeout = false;
                StateHasChanged();
            });

            _hubConnection.On<int, int, int, int>("FlipCards", (y1, x1, y2, x2) => {
                _board[y1, x1].IsFlipped = !_board[y1, x1].IsFlipped;
                _board[y2, x2].IsFlipped = !_board[y2, x2].IsFlipped;
                StateHasChanged();
            });

            await _hubConnection.StartAsync();
        }

        public async Task HandleCardSelect(int height, int width)
        {
            var card = _board[height, width];
            if (card.IsFlipped || 
                _onTheMove != _me || 
                _gameState != GameState.Running) return;

            card.IsFlipped = true;
            _isTimeout = false;
            await _hubConnection.SendAsync("CardFliped", height, width);

            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
