using AcademicDocumentRagSystem.DataAccess.Models;
using AcademicDocumentRagSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademicDocumentRagSystem.DataAccess.Repositories.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AcademicRagDbContext _context;
        public AccountRepository(AcademicRagDbContext context)
        {
            _context = context;
        }
        public async Task<Account?> GetByEmailAndPasswordAsync(string email, string password)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == email && a.Password == password);
        }
    }
}
