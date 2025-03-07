using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShacabWf.Web.Models
{
    /// <summary>
    /// Represents a comment on a change request
    /// </summary>
    public class ChangeRequestComment
    {
        [Key]
        public int Id { get; set; }

        // Foreign key for change request
        [Required]
        public int ChangeRequestId { get; set; }

        // Navigation property for change request
        [ForeignKey("ChangeRequestId")]
        public virtual ChangeRequest? ChangeRequest { get; set; }

        // Foreign key for commenter
        [Required]
        public int CommenterId { get; set; }

        // Navigation property for commenter
        [ForeignKey("CommenterId")]
        public virtual User? Commenter { get; set; }

        // Comment text
        [Required]
        public string Text { get; set; } = string.Empty;

        // Date when the comment was created
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Date when the comment was last updated
        public DateTime? UpdatedAt { get; set; }

        // Flag to indicate if the comment is internal (only visible to CAB members)
        public bool IsInternal { get; set; } = false;
    }
} 