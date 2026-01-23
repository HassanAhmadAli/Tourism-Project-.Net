using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism.Data;
using Tourism.Models;

namespace Tourism.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ApplicationDbContext _dbContext) : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet()]
    public async Task<List<User>> Get()
    {
        return await _dbContext.UserAccounts.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<User?> GetById(int id)
    {
        var res = await _dbContext.UserAccounts.FirstOrDefaultAsync(x => x.Id == id);
        return res;
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] User userAcccount)
    {
        if (string.IsNullOrWhiteSpace(userAcccount.FullName) ||
            string.IsNullOrWhiteSpace(userAcccount.PasswordHash) ||
            string.IsNullOrWhiteSpace(userAcccount.UserName)
           )
        {
            return BadRequest("Invalid Request");
        }

        await _dbContext.UserAccounts.AddAsync(userAcccount);
        await _dbContext.SaveChangesAsync();
        // u.PasswordHash = hash
        return CreatedAtAction(nameof(GetById), new { id = userAcccount.Id }, userAcccount);
    }
    
}