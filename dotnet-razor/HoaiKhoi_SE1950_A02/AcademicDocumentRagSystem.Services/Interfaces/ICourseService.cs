using AcademicDocumentRagSystem.Services.DTOs.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<CourseDto>> GetAllAsync();

        Task<List<CourseDto>> SearchAsync(string? searchTerm);

        Task<CourseDto?> GetByIdAsync(int id);

        Task CreateAsync(CreateCourseDto dto);

        Task UpdateAsync(UpdateCourseDto dto);

        Task DeleteAsync(int id);
    }
}
