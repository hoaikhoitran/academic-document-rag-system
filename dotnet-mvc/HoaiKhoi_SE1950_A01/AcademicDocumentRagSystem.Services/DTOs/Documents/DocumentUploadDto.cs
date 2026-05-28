using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.Services.DTOs.Documents
{
    public class DocumentUploadDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string CourseCode { get; set; } = string.Empty;

        public string? Chapter { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
