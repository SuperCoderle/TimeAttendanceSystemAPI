namespace TimeAttendanceSystemAPI.Models
{
    public class JwtToken
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public bool isSuccess { get; set; }
    }
}
