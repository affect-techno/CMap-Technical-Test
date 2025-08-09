using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.BusinessLogic.Interfaces;

public interface IUserLogic
{
    public User[] GetUsers();
    User? GetUserById(Guid userId);
}