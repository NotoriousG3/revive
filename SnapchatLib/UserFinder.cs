using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;

namespace SnapchatLib.Exceptions
{
    public class UsernameNotFoundException : Exception
    {
        public UsernameNotFoundException(string username) : base($"User with name \"{username}\" not found")
        {
        }
    }

    public class UserNumberNotFoundException : Exception
    {
        public UserNumberNotFoundException(string number) : base($"User with phone number \"{number}\" not found")
        {
        }
    }
}

namespace SnapchatLib
{
    internal class UserFinder
    {
        private static readonly ConcurrentDictionary<string, string> LookupCache = new();
        private static readonly ConcurrentDictionary<Tuple<string, string, string>, string> NumberLookupCache = new();
        private readonly ISnapchatHttpClient m_HttpClient;
        private readonly IUtilities m_Utilities;
        public UserFinder(ISnapchatHttpClient httpClient, IUtilities utilities)
        {
            m_HttpClient = httpClient;
            m_Utilities = utilities;
        }

        internal async Task CacheFriendsList()
        {

            var sync = await m_HttpClient.Friend.SyncFriends();

            foreach (var friend in sync.friends)
                if (friend.user_id != null)
                    LookupCache[friend.mutable_username] = friend.user_id;

            foreach (var friend in sync.added_friends)
                if (friend.user_id != null)
                    LookupCache[friend.mutable_username] = friend.user_id;
        }

        public string FindUserFromFriendsListCache(string username)
        {
            return LookupCache.TryGetValue(username, out var userId) ? userId : throw new UsernameNotFoundException(username);
        }

        public async Task<string> FindUserFromCache(string username)
        {
            if (LookupCache.TryGetValue(username, out var userId)) return userId;

            userId = await m_HttpClient.Search.GetUserId(username);

            if (string.IsNullOrWhiteSpace(userId)) throw new UsernameNotFoundException(username);

            LookupCache[username] = userId;
            return userId;
        }
    }
}