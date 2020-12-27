using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POT.Pexeso.Data;
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

        [HttpGet("getall")]
        public async Task<ActionResult<List<CardBackInfo>>> GetAll()
        {
            var cards = await _dataContext.Cards
                .Select(card => new CardBackInfo { 
                                        Id = card.Id,
                                        Type = card.Type, 
                                        Name = card.Name, 
                                        Source = card.Source })
                .ToListAsync();
            return Ok(cards);
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<CardBackInfo>> Get([FromRoute] int id)
        {
            var card = await _dataContext.Cards.FirstOrDefaultAsync(card => card.Id == id);
            return Ok(card);
        }
    }
}
