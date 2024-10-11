using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TicketCore.Web.Models
{

    public class UserResponse
    {
        [JsonPropertyName("data")]
        public UserData Data { get; set; } = new();
    }

    public class UserData
    {
        [JsonPropertyName("userName")]
        public string UserName { get; set; } = default!;
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
        [JsonPropertyName("departmentId")]
        public int DepartmentId { get; set; }
        [JsonPropertyName("userType")]
        public UserType UserType { get; set; }
        [JsonPropertyName("subDepartmentId")]
        public int? SubDepartmentId { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}

public enum UserType
{
    [Display(Name = "Sinh viên")]
    Student,
    [Display(Name = "Giảng viên")]
    Lecturer,
    [Display(Name = "Trưởng bộ môn")]
    Leader,
    [Display(Name = "Trưởng Khoa")]
    Dean,
    [Display(Name = "Phó trưởng đơn vị")]
    Deputy,
    [Display(Name = "Ban giám hiệu")]
    Administrator
}