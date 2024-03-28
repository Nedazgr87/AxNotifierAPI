using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace AxNotifierAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotesController : ControllerBase
    {

        private readonly ILogger<NotesController> _logger;

        public NotesController(ILogger<NotesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{page:int}")]
        public ApiResult<List<Note>> Get(int page)
        {
            using var db = new SqlConnection(Statics.ConnectionString);
            var query = "select * from AxLogs order by DateTime desc OFFSET (@Skip) ROWS FETCH NEXT (100) ROWS ONLY";
            var data = db.Query<Note>(query, new { Skip = page * 100 }).ToList();
            return new ApiResult<List<Note>>("Ok", data);
        }

        [HttpGet("SetRead/{id:int}", Name = "SetRead")]
        public ApiResult<bool> SetRead(int id)
        {
            using var db = new SqlConnection(Statics.ConnectionString);
            var query = "update AxLogs set IsRead = 1 where id = @id";
            db.Execute(query, new { id });
            return new ApiResult<bool>("Ok", true);
        }
    }

}