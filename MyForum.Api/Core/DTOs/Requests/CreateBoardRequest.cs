using System.ComponentModel.DataAnnotations;

namespace MyForum.Api.Core.DTOs.Requests
{
    public class CreateBoardRequest
    {
        [MaxLength(10)]
        public string ShortName { get; set; } = null!;

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}