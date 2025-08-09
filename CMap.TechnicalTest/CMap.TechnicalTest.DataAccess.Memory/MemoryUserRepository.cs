using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.DataAccess.Memory;

public class MemoryUserRepository : IUserRepository
{
    private readonly List<User> _users =
    [
        new User { Id = Guid.NewGuid(), Name = "User 1" },
        new User { Id = Guid.NewGuid(), Name = "User 2" }
    ];
    
    public User? GetUserById(Guid id)
    {
        if(id == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(id), id, $"{nameof(id)} cannot be empty");
        
        return _users.FirstOrDefault(x => x.Id == id);
    }

    public IEnumerable<User> GetUsers() => _users;
}