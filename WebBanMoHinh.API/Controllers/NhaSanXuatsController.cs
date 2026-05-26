using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NhaSanXuatsController : ControllerBase
    {
        private readonly WebBanMoHinhContext _context;

        public NhaSanXuatsController(WebBanMoHinhContext context)
        {
            _context = context;
        }

        // GET: api/NhaSanXuats
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NhaSanXuat>>> GetNhaSanXuats()
        {
            return await _context.NhaSanXuat.ToListAsync();
        }

        // POST: api/NhaSanXuats
        [HttpPost]
        public async Task<ActionResult<NhaSanXuat>> PostNhaSanXuat(NhaSanXuat nsx)
        {
            _context.NhaSanXuat.Add(nsx);
            await _context.SaveChangesAsync();
            return Ok(nsx);
        }
    }
}