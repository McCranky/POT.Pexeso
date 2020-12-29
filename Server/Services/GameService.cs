using Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using POT.Pexeso.Server.Hubs;
using POT.Pexeso.Shared;
using POT.Pexeso.Shared.Pexeso;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace POT.Pexeso.Server.Services
{
    public class GameService
    {
        private readonly IHubContext<GameHub> _hubContext;
        private ConcurrentBag<GameInfo> _games;
        private readonly LobbyService _lobbyService;
        private readonly Timer _gameTimer;
        private readonly int _refreshRate;
        private PexesoDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;

        public GameService(IHubContext<GameHub> hubContext, LobbyService lobbyService, IServiceProvider serviceProvider)
        {
            _games = new ConcurrentBag<GameInfo>();
            _hubContext = hubContext;
            _lobbyService = lobbyService;
            _serviceProvider = serviceProvider;

            _refreshRate = 20;
            _gameTimer = new Timer(_refreshRate);
            _gameTimer.Elapsed += new ElapsedEventHandler(UpdateGames);
            _gameTimer.Start();
        }

        public async void UpdateGames(Object source, ElapsedEventArgs e)
        {
            var games = _games.Where(game => game.GameState != GameState.Starting && game.GameState != GameState.Ended);
            foreach (var game in games) {
                game.Time += _refreshRate;
                var ids = new string[] { _lobbyService.GetConnectionId(game.Challenger.Nickname),
                                    _lobbyService.GetConnectionId(game.Opponent.Nickname) };


                switch (game.GameState) {
                    case GameState.Running:
                        if (game.IsTimeout) {
                            await _hubContext.Clients.Clients(ids).SendAsync("TimeoutChanged", GameSettings.ResponseDelay - game.Time / 1000);

                            if (game.Time / 1000 >= GameSettings.ResponseDelay) {
                                await _hubContext.Clients.Clients(ids).SendAsync("ExitGame");
                                game.GameState = GameState.Ended;
                                game.Time = 0;
                            }

                        } else {
                            if (game.Time / 1000 >= GameSettings.ResponseDelay) {
                                await _hubContext.Clients.Clients(ids).SendAsync("TimeoutWarning");
                                game.IsTimeout = true;
                                game.Time = 0;
                            }
                        }

                        break;

                    case GameState.CardShowDelay:
                        if (game.Time / 1000 >= GameSettings.CardShowDelay) {
                            game.GameState = GameState.Running;
                            game.Time = 0;
                            // otočiť posledne dve karty
                            var currentIndex = game.Moves.Count - 1;
                            var currX = game.Moves[currentIndex].X;
                            var currY = game.Moves[currentIndex].Y;
                            var lastX = game.Moves[currentIndex - 1].X;
                            var lastY = game.Moves[currentIndex - 1].Y;

                            game.Board.Flip(lastY, lastX);
                            game.Board.Flip(currY, currX);

                            await _hubContext.Clients.Clients(ids).SendAsync("FlipCards",
                                currY, currX,
                                lastY, lastX);
                            // zmeniť stav
                            await _hubContext.Clients.Clients(ids).SendAsync("ChangeGameState", game.GameState);
                            // zmeniť hrača na ťahu
                            game.OnTheMove = game.OnTheMove == game.Challenger ? game.Opponent : game.Challenger;
                            await _hubContext.Clients.Clients(ids).SendAsync("ChangeTurn", game.OnTheMove);
                        }
                        break;

                    case GameState.Gameover:
                        if (game.Time / 1000 >= GameSettings.GameoverDelay) {
                            game.Time = 0;
                            game.GameState = GameState.Ended;
                            await _hubContext.Clients.Clients(ids).SendAsync("Gameover");
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        public void AddGame(GameSettings settings, string challenger, string opponent)
        {
            var playerChallenger = new Player { Nickname = challenger };
            var playerOpponent = new Player { Nickname = opponent };

            _games.Add(new GameInfo {
                Challenger = playerChallenger,
                Opponent = playerOpponent,
                GameSettings = settings,
                OnTheMove = playerChallenger,
                Board = new Board(settings.BoardSize),
                Moves = new List<Shared.Pexeso.MoveRecord>(),
                Started = DateTime.Now
            });
        }

        public async Task JoinToGame(string nick, string connection)
        {
            _lobbyService.ConnectUser(nick, connection, Status.InGame);
            var game = GetGame(nick);
            if (game == null) return;

            await _hubContext.Clients.Client(connection).SendAsync("GetGameInfo", game.Challenger, game.Opponent, game.OnTheMove, game.GameSettings, game.Board.Cards);
            if (++game.Connected == 2 && game.GameState == GameState.Starting) {
                // TODO start game a oznamiť klientom
                game.GameState = GameState.Running;
            }
        }

        public void LeaveGame(string nick)
        {
            var game = GetGame(nick);
            _lobbyService.DisconnectUser(nick);

            if (game != null && --game.Connected <= 0) {
                game.GameState = GameState.Ended;

                var tmpBag = new ConcurrentBag<GameInfo>();
                while (_games.TryTake(out var g)) {
                    if (g != game) {
                        tmpBag.Add(g);
                    }
                }
                _games = tmpBag;
            }
        }

        public async Task FlipCard(string nick, int height, int width)
        {
            var game = GetGame(nick);
            game.Board.Flip(height, width);
            game.IsTimeout = false;
            game.Time = 0;

            game.Moves.Add(new MoveRecord {
                Nickname = nick,
                X = width,
                Y = height,
                Time = DateTime.Now,
                Data = game.Board[height, width].Data
            });

            var currentPlayer = game.Challenger.Nickname == nick ? game.Challenger : game.Opponent;
            var otherPlayer = game.Challenger.Nickname != currentPlayer.Nickname ? game.Challenger : game.Opponent;

            ++currentPlayer.MovesCount;

            var ids = new string[] { _lobbyService.GetConnectionId(currentPlayer.Nickname),
                                    _lobbyService.GetConnectionId(otherPlayer.Nickname) };

            var gameover = false;
            if (game.LastFliped == null) {
                game.LastFliped = game.Board[height, width];

            } else {
                if (game.Board[height, width].Data == game.LastFliped.Data) {
                    // zhoda
                    ++game.OnTheMove.Score;
                    await _hubContext.Clients.Clients(ids).SendAsync("ChangeScore", currentPlayer);
                    if (++game.Matches == game.Board.Cards.Count / 2) {
                        gameover = true;
                    }

                } else {
                    // nezhoda, tah konci
                    await _hubContext.Clients.Clients(ids).SendAsync("ChangeGameState", GameState.CardShowDelay);
                    game.GameState = GameState.CardShowDelay;
                }
                game.LastFliped = null;
            }

            await _hubContext.Clients.Client(ids[1]).SendAsync("FlipCard", height, width);
            if (gameover) {
                await WriteGameResult(game);

                await _hubContext.Clients.Clients(ids).SendAsync("ChangeGameState", GameState.Gameover);
                game.GameState = GameState.Gameover;
            }
        }

        private async Task WriteGameResult(GameInfo game)
        {
            game.Ended = DateTime.Now;

            using var scope = _serviceProvider.CreateScope();
            _dbContext = scope.ServiceProvider.GetService<PexesoDbContext>();

            string winner = null;
            if (game.Challenger.Score > game.Opponent.Score) {
                winner = game.Challenger.Nickname;
                var user = await _dbContext.Users.FirstAsync(user => user.Nickname == winner);
                ++user.Wins;

                var loser = await _dbContext.Users.FirstAsync(user => user.Nickname == game.Opponent.Nickname);
                ++loser.Loses;

                _dbContext.Users.UpdateRange(new[] { user, loser });
                await _dbContext.SaveChangesAsync();

            } else if (game.Challenger.Score < game.Opponent.Score) {
                winner = game.Opponent.Nickname;
                var user = await _dbContext.Users.FirstAsync(user => user.Nickname == winner);
                ++user.Wins;

                var loser = await _dbContext.Users.FirstAsync(user => user.Nickname == game.Challenger.Nickname);
                ++loser.Loses;

                _dbContext.Users.UpdateRange(new[] { user, loser });
                await _dbContext.SaveChangesAsync();

            } else {
                var user1 = await _dbContext.Users.FirstAsync(user => user.Nickname == game.Challenger.Nickname);
                ++user1.Draws;

                var user2 = await _dbContext.Users.FirstAsync(user => user.Nickname == game.Opponent.Nickname);
                ++user2.Draws;

                _dbContext.Users.UpdateRange(new[] { user1, user2 });
                await _dbContext.SaveChangesAsync();
            }

            var record = new Database.Models.GameRecord {
                Opponent = game.Opponent.Nickname,
                OpponentMoves = game.Opponent.MovesCount,
                Challenger = game.Challenger.Nickname,
                ChallengerMoves = game.Challenger.MovesCount,
                Started = game.Started,
                Ended = game.Ended,
                BoardSize = game.GameSettings.BoardSize,
                CardId = game.GameSettings.CardBack.Id,
                Winner = winner,
                Moves = game.Moves
            };
            await _dbContext.Records.AddAsync(record);

            await _dbContext.SaveChangesAsync();
        }

        private GameInfo GetGame(string nick)
        {
            return _games.FirstOrDefault(i => i.Challenger.Nickname == nick || i.Opponent.Nickname == nick);
        }
    }
}
