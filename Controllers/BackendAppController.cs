using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace InsecureApiApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class BackendAppController : ControllerBase
    {
        private readonly string _conn = "Server=localhost;Database=Test;User Id=admin;Password=Password123!";

        private readonly ILogger<BackendAppController> _logger;

        public BackendAppController(ILogger<BackendAppController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "UserLogin")]
        public IActionResult Login(string username, string password)
        {
            var query = $"SELECT * FROM Users WHERE Username = '{username}' AND Password = '{password}'";

            using var conn = new SqlConnection(_conn);
            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return Ok("Login Successful!");
            }

            return Unauthorized();
        }

        [HttpPost(Name = "UserUpload")]
        public IActionResult UserFileUploader()
        {
            var file = Request.Form.Files[0];
            var path = Path.Combine("wwwroot/uploads", file.FileName);
            using var stream = new FileStream(path, FileMode.Create);
            file.CopyTo(stream);

            return Ok("File uploaded!");
        }

        [HttpPost(Name = "ConvertVideo")]
        public IActionResult VideoConverter([FromBody] string videoName)
        {
            string outputFile = videoName.Replace(".mp4", "_converted.mp4");

            string command = $"ffmpeg -i wwwroot/uploads/{videoName} -vcodec libx264 wwwroot/uploads/converted/{outputFile}";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return Ok(new { Message = "Video conversion started", Output = output, Error = error });
        }
    }
}
