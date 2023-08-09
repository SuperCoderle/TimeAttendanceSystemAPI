namespace TimeAttendanceSystemAPI.Models
{
    public class UserToken
    {
        public string? token { get; set; }
        public RefreshToken? refreshToken { get; set; }
    }
}
