using System;
using System.Linq;
using TicketCore.Common;
using TicketCore.Data;

namespace TicketCore.Core
{
    public class TicketHistoryHelper : ITicketHistoryHelper
    {
        private readonly VueTicketDbContext _vueTicketDbContext;
        public TicketHistoryHelper(VueTicketDbContext vueTicketDbContext)
        {
            _vueTicketDbContext = vueTicketDbContext;
        }

        public string CreateMessage(int? priorityId, int? departmentId)
        {
            string message = string.Empty;
            string priorityName = GetPriorityNameBypriorityId(priorityId);
            string status = GetStatusBystatusId(Convert.ToInt16(StatusMain.Status.Open));
            string DepartmentName = GetDepartmentNameByDepartmentId(departmentId);

            if (priorityId != null && departmentId != null)
            {
                message = $"Tạo phiếu với trạng thái: {status}, mức độ: {priorityName}, phòng ban: {DepartmentName}";
            }

            return message;
        }

        public string StatusMessage(int? statusId)
        {
            string message = string.Empty;
            string status = GetStatusBystatusId(statusId);

            // Open | Resolved | InProgress | OnHold | Recently |Edited |Replied
            if (statusId != null &&
                   statusId == Convert.ToInt16(StatusMain.Status.Open)
                || statusId == Convert.ToInt16(StatusMain.Status.Resolved)
                || statusId == Convert.ToInt16(StatusMain.Status.InProgress)
                || statusId == Convert.ToInt16(StatusMain.Status.OnHold)
                || statusId == Convert.ToInt16(StatusMain.Status.RecentlyEdited)
                || statusId == Convert.ToInt16(StatusMain.Status.Replied)
            )
            {
                message = $"Cập nhật trạng thái phiếu: {status}";
            }

            // Deleted
            if (statusId != null && statusId == Convert.ToInt16(StatusMain.Status.Deleted))
            {
                message = $"Xóa phiếu";
            }

            // Closed
            if (statusId != null && statusId == Convert.ToInt16(StatusMain.Status.Closed))
            {
                message = $"Đóng phiếu";
            }

            if (statusId != null && statusId == Convert.ToInt16(StatusMain.Status.ReOpened))
            {
                message = $"Mở lại phiếu";
            }


            return message;
        }

        public string PriorityMessage(int? priorityId)
        {
            string message = string.Empty;
            string priorityName = GetPriorityNameBypriorityId(priorityId);

            // Deleted
            if (priorityId != null)
            {
                message = $"Cập nhật mức độ: {priorityName}";
            }

            return message;
        }

        public string DepartmentMessage(int? DepartmentId)
        {
            string message = string.Empty;
            string departmentName = GetDepartmentNameByDepartmentId(DepartmentId);

            // Deleted
            if (DepartmentId != null)
            {
                message = $"Cập nhật phòng ban: {departmentName}";
            }

            return message;
        }

        public string ReplyMessage(int? statusId)
        {
            string message = string.Empty;
            string status = GetStatusBystatusId(statusId);

            if (status != null)
            {
                message = $"Đã trả lời và cập nhật trạng thái: {status}";
            }

            return message;
        }

        public string DeleteTicketReplyMessage()
        {
            var message = $"Xóa câu trả lời";
            return message;
        }

        public string EditTicket()
        {
            var message = "Chỉnh sửa phiếu.";
            return message;
        }

        public string EditReplyTicket()
        {
            var message = "Chỉnh sửa câu trả lời";
            return message;
        }

        public string DeleteTicketAttachment(string ticketid)
        {
            var message = $"Xóa đính kèm phiếu #{ticketid}";
            return message;
        }

        public string DeleteTicketReplyAttachment(string ticketid)
        {
            var message = "Xóa đính kém câu trả lời phiếu #{ticketid}";
            return message;
        }

        public string TicketDelete()
        {
            var message = "Xóa phiếu";
            return message;
        }

        public string TicketRestore()
        {
            var message = "Khôi phục phiếu";
            return message;
        }

        public string AssignTickettoUser()
        {
            var message = "Gán phiếu thủ công";
            return message;
        }


        private string GetPriorityNameBypriorityId(int? priorityId)
        {
            var priorityList = (from priority in _vueTicketDbContext.Priority
                                where priority.PriorityId == priorityId
                                select priority.PriorityName).FirstOrDefault();
            return priorityList;
        }

        private string GetStatusBystatusId(int? statusId)
        {
            var statusList = (from status in _vueTicketDbContext.Status
                              where status.StatusId == statusId
                              select status.StatusText).FirstOrDefault();
            return statusList;
        }

        private string GetDepartmentNameByDepartmentId(int? departmentId)
        {
            var departmentname = (from Department in _vueTicketDbContext.Department
                                  where Department.DepartmentId == departmentId
                                  select Department.DepartmentName).FirstOrDefault();
            return departmentname;
        }

        public string TransferMessage(int? fromdepartmentId, int? todepartmentId)
        {
            var fromdepartment = GetDepartmentNameByDepartmentId(fromdepartmentId);
            var todepartment = GetDepartmentNameByDepartmentId(todepartmentId);
            var message = $"Phiếu được chuyển từ phòng ban {fromdepartment} sang {todepartment}";
            return message;
        }

    }
}