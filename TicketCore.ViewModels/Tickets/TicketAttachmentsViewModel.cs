﻿using System;

namespace TicketCore.ViewModels.Tickets
{
    public class TicketAttachmentsViewModel
    {
        public string OriginalAttachmentName { get; set; }
        public string GenerateAttachmentName { get; set; }
        public string AttachmentType { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public long? TicketId { get; set; }
        public string AttachmentBase64 { get; set; }
        public string BucketName { get; set; }
        public string DirectoryName { get; set; }
    }


}