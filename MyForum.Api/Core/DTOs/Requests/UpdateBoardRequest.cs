using System.ComponentModel.DataAnnotations;

namespace MyForum.Api.Core.DTOs.Requests
{
    public class UpdateBoardRequest
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(10)]
        public string? ShortName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsHidden { get; set; }
    }
}