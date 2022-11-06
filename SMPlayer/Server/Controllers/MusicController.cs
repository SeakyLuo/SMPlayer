using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Services;

namespace SMPlayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MusicController : ControllerBase
    {
        [HttpGet("get/{id?}")]
        public MusicView GetMusicById(long id)
        {
            return MusicService.FindMusic(id).ToVO();
        }
    }
}
