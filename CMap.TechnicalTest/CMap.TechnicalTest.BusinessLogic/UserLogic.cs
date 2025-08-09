using CMap.TechnicalTest.BusinessLogic.Interfaces;
using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.BusinessLogic;

public class UserLogic(IUserRepository userRepository) : IUserLogic
{
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    public User[] GetUsers() => _userRepository.GetUsers()?.ToArray() ?? [];
    
    public User? GetUserById(Guid userId) => _userRepository.GetUserById(userId);
}