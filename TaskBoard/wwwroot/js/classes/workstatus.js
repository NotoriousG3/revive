// Keep in sync with WorkStatus in WorkRequest
class WorkStatus {
    static NotRun = 0;
    static Error = 1;
    static Incomplete = 2;
    static Ok = 3;
    static Cancelled = 4;
    static PendingCancellation = 5;
    static Waiting = 7;

    static ToString(statusId) {
        switch (statusId) {
            case this.NotRun:
                return "Not Run";
            case this.Error:
                return "Error";
            case this.Incomplete:
                return "Incomplete";
            case this.Ok:
                return "Ok";
            case this.Cancelled:
                return "Cancelled";
            case this.PendingCancellation:
                return "Pending Cancellation";
            case this.Waiting:
                return "Waiting";
            default:
                return "Status not defined";
        }
    }
}
