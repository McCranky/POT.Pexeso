using Database;
using Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POT.Pexeso.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POT.Pexeso.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResourceController : Controller
    {
        private readonly PexesoDbContext _dataContext;

        public ResourceController(PexesoDbContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet("getCards")]
        public async Task<ActionResult<List<CardBackInfo>>> GetCards()
        {
            var cards = await _dataContext.Cards
                .Select(card => new CardBackInfo {
                    Id = card.Id,
                    Type = card.Type,
                    Name = card.Name,
                    Source = card.Source
                })
                .ToListAsync();
            return Ok(cards);
        }

        [HttpGet("getCard/{id}")]
        public async Task<ActionResult<CardBackInfo>> GetCard([FromRoute] int id)
        {
            var card = await _dataContext.Cards.FirstOrDefaultAsync(card => card.Id == id);
            return Ok(card);
        }

        [HttpGet("getGameRecords/{nick}")]
        public ActionResult<List<GameRecord>> GetGameRecords([FromRoute] string nick)
        {
            var games = _dataContext.Records.Where(game => game.Challenger == nick || game.Opponent == nick);
            return Ok(games);
        }

        [HttpGet("getMoves/{gameId}")]
        public ActionResult<List<GameRecord>> GetMoves([FromRoute] int gameId)
        {
            var game = _dataContext.Records
                .Include(r => r.Moves)
                .Where(record => record.Id == gameId)
                .First();
            if (game != null) {
                return Ok(game.Moves.OrderBy(move => move.Time).ToList());
            }
            return BadRequest(null);
        }

        [HttpGet("getGlobalStats")]
        public async Task<ActionResult<GlobalStats>> GetGlobalStats()
        {
            var games = await _dataContext.Records.ToListAsync();
            var totalTime = TimeSpan.Zero;
            foreach (var game in games) {
                totalTime += game.Ended - game.Started;
            }

            return Ok(new GlobalStats { TotalTime = totalTime.ToString("hh':'mm':'ss"), GamesCount = games.Count });
        }
    }
}
