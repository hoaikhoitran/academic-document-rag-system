using AcademicDocumentRagSystem.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByEmailAndPasswordAsync(string email, string password);

        Task<List<Account>> GetAllAsync(string? searchTerm, int? role, bool? status);

        Task<Account?> GetByIdAsync(int id);

        Task<Account?> GetByEmailAsync(string email);

        Task<Account?> GetByEmailExceptIdAsync(string email, int accountId);

        Task AddAsync(Account account);

        void Update(Account account);

        void Delete(Account account);

        Task SaveChangesAsync();
    }
}
