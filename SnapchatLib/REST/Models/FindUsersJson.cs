using System.Collections.Generic;

namespace SnapchatLib.REST.Models;

public class FindUsersJson
{
    public List<User> users { get; set; }
    public List<object> verified_users { get; set; }
}