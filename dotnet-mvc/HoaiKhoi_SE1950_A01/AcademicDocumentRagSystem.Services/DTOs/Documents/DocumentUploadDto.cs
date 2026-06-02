using AcademicDocumentRagSystem.Services.DTOs.Courses;
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

        [Required(ErrorMessage = "Please select a course.")]
        public int CourseId { get; set; }

        // Derived server-side from the validated course; never trusted from the form.
        public string CourseCode { get; set; } = string.Empty;

        public string? Chapter { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;

        /// <summary>
        /// Courses the current teacher is allowed to upload to. Populated for display
        /// only (not bound on post); the backend re-validates the chosen course.
        /// </summary>
        public List<CourseDto> AvailableCourses { get; set; } = new();
    }
}
