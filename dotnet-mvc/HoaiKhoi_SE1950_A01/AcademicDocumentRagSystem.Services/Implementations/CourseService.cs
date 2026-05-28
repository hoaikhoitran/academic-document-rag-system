using AcademicDocumentRagSystem.DataAccess.Models;
using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;
using AcademicDocumentRagSystem.Services.DTOs.Courses;
using AcademicDocumentRagSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }
        public async Task CreateAsync(CreateCourseDto dto)
        {
            var existingCourse = await _courseRepository.GetByCodeAsync(dto.Code);
            if (existingCourse != null)
            {
                throw new ArgumentException("Course with the same code already exists.");
            }

            var course = new Course
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status
            };
            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                throw new ArgumentException("Course not found.");
            }
            _courseRepository.Delete(course);
            await _courseRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdateCourseDto dto)
        {
            var course = await _courseRepository.GetByIdAsync(dto.CourseId);
            if (course == null)
            {
                throw new ArgumentException("Course not found.");
            }
            var existingCourse = await _courseRepository.GetByCodeAsync(dto.Code);
            if (existingCourse != null && existingCourse.CourseId != dto.CourseId)
            {
                throw new ArgumentException("Another course with the same code already exists.");
            }
            course.Code = dto.Code;
            course.Name = dto.Name;
            course.Description = dto.Description;
            course.Status = dto.Status;
            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();
        }

        public async Task<List<CourseDto>> GetAllAsync()
        {
            var courses = await _courseRepository.GetAllAsync();
            return courses.Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                Status = c.Status
            }).ToList();
        }

        public async Task<CourseDto?> GetByIdAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return null;
            }
            return new CourseDto
            {
                CourseId = course.CourseId,
                Code = course.Code,
                Name = course.Name,
                Description = course.Description,
                Status = course.Status
            };
        }
    }
}
