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
    }
}
