using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.DataAccess.Interfaces;

public interface IUserRepository
{
    User? GetUserById(Guid id);
    
    IEnumerable<User> GetUsers();
}