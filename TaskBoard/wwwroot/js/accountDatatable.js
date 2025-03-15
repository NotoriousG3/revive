async function ReloadAccountDatatable() {
    try {
        $('#accounts').DataTable().ajax.reload();
    }catch(e){
        logger.PrintException(e);
    }
}

function CreateControls(options, groupCount) {
    let controls = [];

    if (options.showDeleteButton) {
        controls.push(`<button type=button class="btn btn-danger btn-sm mx-1 my-1" onClick="DeleteAccount(this)" title="Delete Account"><i class="fa fa-trash"></i></button>`);
    }
        
    if (options.showAddToGroupButton) {
        controls.push(`<button type=button class="btn btn-success btn-sm mx-1 my-1" onClick="AddToCurrentGroup(this)" title="Add To Group"><i class="fa fa-plus"></i></button>`);
    }
    
    if (options.showEditGroupButton) {
        controls.push(`<button type=button class="btn btn-success btn-sm mx-1 my-1" onClick="ShowManageGroupsModal(this)" title="Manage Groups"><i class="fa fa-people-group"></i> ${groupCount > 99 ? "99+" : groupCount}</button>`);
    }
    
    let div = $('<div class="d-flex justify-content-between align-content-center"></div>')
    $(div).html(controls.join(''))
    return div.html();
}

async function LoadAccountDatatable(options) {
    try {
        $('#accounts').dataTable({
            ajax: {
                url: '/api/account/data',
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                content: 'json',
                data: function (d) {
                    return JSON.stringify(d);
                }
            },
            search: {
                return: true
            },
            processing: true,
            serverSide: true,
            columns: [
                {data: (row) => {
                    return CreateControls(options, row.groups);
                }},
                {data: 'id'},
                {data: (row, type) => {
                        if (type == 'display') {
                            let name = row.username;

                            return `${name}`;
                            //return `<div class="editUsername">${name}</div>`;
                        }
                        return "";
                    }},
                {data: (row, type) => {
                        if (type == 'display') {
                            let friendP = ValidationStatus.FriendCount((row.outgoingFriendCount + row.incomingFriendCount));
                            let friendM = ValidationStatus.FriendCount(row.friendCount);

                            return `${friendP} Pending / ${friendM} Mutual`;
                        }
                        return "";
                    }},
                {data: 'email'},
                {data: (row, type) => {
                        if (type === 'display') {
                            if (row.creationDate == undefined) return "";

                            let d = new Date(row.creationDate);
                            return ("0" + d.getDate()).slice(-2) + "-" + ("0"+(d.getMonth()+1)).slice(-2) + "-" +
                                d.getFullYear() + " " + ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2);
                        }
                        return "";
                    }},
                {data: (row, type) => {
                        if (type === 'display') {
                            return "Android";
                        }
                        return "";
                    }},
                {data: (row, type, set, meta) => {
                        if (type == 'display') {
                            let phoneValidationStatus = ValidationStatus.ToText(row.phoneValidated);
                            let emailValidationStatus = ValidationStatus.ToText(row.emailValidated);

                            return `Phone: ${phoneValidationStatus} | E-Mail: ${emailValidationStatus}`;
                        }
                        return "";
                    }},
                {data: (row, type, set, meta) => {
                        if (type == 'display') {
                            let AccStatus = ValidationStatus.AccountStatus(row.accountStatus);

                            return `${AccStatus}`;
                        }
                        return "";
                    }},
            ]
        });
    } catch (e) {
        if (e.status == 403) {
            LogAccessError();
            return;
        }
        throw e;
    }
}

function GetCurrentGroupsContainer() {
    return $('#manageAccountGroupsModal').find('.groupContainer');
}

function GetUserIdFromDatatableButton(btn) {
    let siblings = $(btn).parent('td').siblings();
    return $(siblings[0]).text();
}

function GetUsernameFromDatatableButton(btn) {
    let siblings = $(btn).parent('td').siblings();
    return $(siblings[1]).text();
}

async function ShowManageGroupsModal(btn) {
    let username = GetUsernameFromDatatableButton(btn);
    let id = GetUserIdFromDatatableButton(btn);
    
    $('#accountGroupModalAccountId').val(id);
    $('#manageAccountGroupsModal').find('.modal-title').html(`Manage Groups of ${username}`);
    let groupContainer = GetCurrentGroupsContainer();
    groupContainer.empty();
    
    let groups = await api.GetAccountGroups(id);
    
    groups.forEach(g => AddCurrentGroupRow(id, g.id, g.name, groupContainer));

    let modal = new bootstrap.Modal(document.getElementById('manageAccountGroupsModal'), { backdrop: false });
    modal.show();
}
async function AddCurrentGroupRow(accountId, groupId, groupName, targetContainer) {
    let row = $(`<div><button type="button" class="btn btn-sm btn-outline-danger me-2 accountGroup${groupId}" onclick="RemoveFromGroup(${accountId}, ${groupId});"><i class="fa fa-minus"></i></button>${groupName}</div>`);
    $(targetContainer).append(row);
}

async function RemoveFromGroup(accountId, groupId) {
    if (groupId == undefined || groupId == 0) return;

    try {
        await api.RemoveAccountFromGroup(accountId, groupId);

        $(`.accountGroup${groupId}`).parent('div').remove();
    } catch (e) {
        logger.PrintException(e);
    }
}

// Used in AccountTools to add an account to a selected group in the AccountGroup modal
async function AddAccountToSelectedGroup() {
    let accountId = $('#accountGroupModalAccountId').val();
    let groupId = $('#accountGroupModalGroupSelect').find('option:selected').val();
    if (groupId == undefined || groupId == 0) return;
    
    try {
        let response = await api.AddAccountToGroup(accountId, groupId);
        let group = response.data;

        await AddCurrentGroupRow(accountId, groupId, group.name, GetCurrentGroupsContainer());
    } catch (e) {
        logger.PrintException(e);
    }
}

// Used in AccountGroup management section
function IsInCurrentGroup(accountId) {
    return $(`div[data-accountid='${accountId}'`).length > 0;
}

function CreateAccountPill(accountId, accountName) {
    return $(`<div class='border rounded px-1 text-bg-primary d-inline-flex mx-1 mb-1 account-pill' data-accountid="${accountId}" onclick="RemoveFromCurrentGroup(this);">${accountName}</div>`);
}

function RefreshAccountIds() {
    let selectedControl = $('#SelectedAccounts');

    let ids = [];
    $('div.account-pill').each((_, el) => {
        let accountId = $(el).attr('data-accountid');
        let accountName = $(el).text();
        ids.push({ Id: accountId, Name: accountName });
    });

    selectedControl.val(JSON.stringify(ids));
}

function AppendPill(accountId, accountName) {
    let pill = CreateAccountPill(accountId, accountName);
    $('#accountsContainer').append(pill);
    
    RefreshAccountIds();
}

function RemoveFromCurrentGroup(pill) {
    pill.remove();
    
    RefreshAccountIds();
}

function AddToCurrentGroup(btn) {
    let accountId = GetUserIdFromDatatableButton(btn);
    let accountName = GetUsernameFromDatatableButton(btn);
    
    if (IsInCurrentGroup(accountId)) return;
    
    AppendPill(accountId, accountName);
}

function AddAlreadySelectedUsers() {
    let selected = $('#SelectedAccounts').val();
    if (selected == "") return;
    let accounts = JSON.parse(selected);
    if (!accounts) return;
    
    accounts.forEach(a => AppendPill(a.Id, a.Name));
}