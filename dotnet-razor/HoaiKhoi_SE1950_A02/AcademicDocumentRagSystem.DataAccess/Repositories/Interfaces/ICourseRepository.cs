using AcademicDocumentRagSystem.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<List<Course>> GetAllAsync();

        Task<List<Course>> SearchAsync(string? searchTerm);

        Task<Course?> GetByIdAsync(int id);

        Task<Course?> GetByCodeAsync(string code);

        Task AddAsync(Course course);

        void Update(Course course);

        void Delete(Course course);

        Task SaveChangesAsync();
    }
}
