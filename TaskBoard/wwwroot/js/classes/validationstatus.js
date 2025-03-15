class ValidationStatus {
    static NotValidated = 0;
    static Validated = 1;
    static FailedValidation = 2;
    static PartiallyValidated = 3;

    static OKAY = 0;
    static BAD_PROXY = 1;
    static NEEDS_RELOG = 2;
    static RATE_LIMITED = 3;
    static LOCKED = 4;
    static BANNED = 5;
    static NEEDS_FRIEND_RELOAD = 6;
    static BUSY = 7;
    static NEEDS_CHECKED = 8;

    static ToText(validationStatus) {
        switch (validationStatus) {
            case this.NotValidated:
                return "Not Validated";
            case this.Validated:
                return "Validated";
            case this.FailedValidation:
                return "Failed";
        }
    }

    static AccountStatus(accountStatus) {
        switch (accountStatus) {
            case this.OKAY:
                return "Okay";
            case this.BAD_PROXY:
                return "Bad Proxy";
            case this.NEEDS_RELOG:
                return "Needs Relog";
            case this.RATE_LIMITED:
                return "Rate Limited";
            case this.LOCKED:
                return "Locked";
            case this.BANNED:
                return "Banned";
            case this.NEEDS_FRIEND_RELOAD:
                return "Marked for Friend Refresh";
            case this.BUSY:
                return "Busy";
            case this.NEEDS_CHECKED:
                return "Needs Checked";
        }
    }

    static FriendCount(count) {
        if(count == -1){
            return "UNKNOWN";
        }

        return count;
    }
}