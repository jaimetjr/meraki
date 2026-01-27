using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CourseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string? Image { get; set; }
        public string Instructor { get; set; } = default!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Modality { get; set; } // Enum as string
        public decimal? Price { get; set; }
        public string Currency { get; set; } = "BRL";
        public string Type { get; set; } = default!; // Enum as string
        public string Status { get; set; } = default!; // Enum as string
        public string? Link { get; set; }

    }
}
