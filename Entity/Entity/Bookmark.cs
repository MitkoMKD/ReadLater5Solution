using System;
using System.ComponentModel.DataAnnotations;

namespace Entity {
    public class Bookmark
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(500)]
        public string URL { get; set; }

        [Required, MaxLength(100)]
        public string ShortDescription { get; set; }

        public int? CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public DateTime CreateDate { get; set; }

        public string ApplicationUserId { get; set; }

        //public ApplicationUser ApplicationUser { get; set; }
    }
}
