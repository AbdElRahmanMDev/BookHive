﻿namespace BookHive.Web.Core.Models
{
    public class BaseModel
    {
        public bool IsDeleted { get; set; }

        public string? CreatedById { get; set; }

        public ApplicationUser? CreatedBy { get; set; }

        public DateTime? LastUpdateOn { get; set; }


        public string? LastUpdatedById{ get; set; }    

        public ApplicationUser? LastUpdatedBy { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

    }
}
