namespace SnapchatLib.Extras;

public enum AccountState
{
    Normal = 0,
    Unlock = 1,
    Locked = 2,
    Banned = 3,
    Unknown = 4
}

public enum UserState
{
    Unused = 0,
    Used = 1,
    Failed = 2
}

public enum FriendsEnums
{
    Mutual = 0,
    Outgoing = 1,
    Subscribed = 6,
}

public enum AddedFriendsEnums
{
    Mutual = 0,
    Pending = 1,
    Subscribers = 6,
}

public enum ValidationStatus
{
    NotValidated = 0,
    Validated = 1,
    FailedValidation = 2,
    PartiallyValidated = 3
}

public enum WebCreationStatus
{
    Created = 0,
    Failed = 1
}