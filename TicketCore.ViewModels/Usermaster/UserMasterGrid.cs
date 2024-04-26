﻿using System;

namespace TicketCore.ViewModels.Usermaster
{
    public class UserMasterGrid
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string Gender { get; set; }
        public string RoleName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Status { get; set; }
        public int RoleId { get; set; }
        public string IsFirstLogin { get; set; }
    }

    public class AgentUserGrid
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string Gender { get; set; }
        public string RoleName { get; set; }
        public string Department { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Status { get; set; }
        public int RoleId { get; set; }
        public string IsFirstLogin { get; set; }
    }
}