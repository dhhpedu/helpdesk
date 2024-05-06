using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketCore.Models.Department
{
    [Table("Department")]
    public class Departments
    {
        [Key]
        public int DepartmentId { get; set; }


        [DisplayName("Tên phòng ban")]
        [Required(ErrorMessage = "Vui lòng nhập tên phòng ban")]
        public string DepartmentName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập trạng thái")]
        [Display(Name = "Trạng thái")]
        public bool Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã phòng ban")]
        [DisplayName("Mã phòng ban")]
        [StringLength(4, MinimumLength = 1, ErrorMessage = "Mã phòng ban yêu cầu tối thiểu một ký tự")]
        public string Code { get; set; }

        [DisplayName("Mô tả")]
        public string DepartmentDescription { get; set; }

    }
}