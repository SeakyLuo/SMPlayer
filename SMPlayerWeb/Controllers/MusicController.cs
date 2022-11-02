using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SMPlayerWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MusicController : ControllerBase
    {
        //[HttpGet("get/{id?}")]
        //public SMPlayer.Models.MusicView GetMusicById(long id)
        //{
        //    return SMPlayer.Services.MusicService.FindMusic(id);
        //}
    }
}
