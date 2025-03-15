async function ReloadProxiesDatatable() {
    try {
        $('#proxiesTable').DataTable().ajax.reload();
    }catch(e){
        logger.PrintException(e);
    }
}

function CreateControls(options, groups) {
    let controls = [];

    if (options.showDeleteButton) {
        controls.push(`<button type=button class="btn btn-danger btn-sm mx-1 my-1" onClick="DeleteProxy(this)" title="Delete Proxy"><i class="fa fa-trash"></i></button>`);
    }
        
    if (options.showAddToGroupButton) {
        controls.push(`<button type=button class="btn btn-success btn-sm mx-1 my-1" onClick="AddToCurrentGroup(this)" title="Add To Group"><i class="fa fa-plus"></i></button>`);
    }
    
    if (options.showEditGroupButton) {
        let groupCount = groups == undefined ? 0 : groups.length;
        controls.push(`<button type=button class="btn btn-success btn-sm mx-1 my-1" onClick="ShowManageGroupsModal(this)" title="Manage Groups"><i class="fa fa-layer-group"></i> ${groupCount > 99 ? "99+" : groupCount}</button>`);
    }
    
    let div = $('<div class="d-flex justify-content-between align-content-center"></div>')
    $(div).html(controls.join(''))
    return div.html();
}

async function LoadProxiesDatatable(options) {
    try {
        $('#proxiesTable').dataTable({
            ajax: {
                url: '/api/proxy/data',
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
                {data: 'address'},
                {data: 'user'},
                {data: 'password', defaultContent: ""},
                {data: 'accountsCount', defaultContent: 0}
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
    return $('#manageProxyGroupsModal').find('.groupContainer');
}

function GetProxyIdFromDatatableButton(btn) {
    let siblings = $(btn).parent('td').siblings();
    return $(siblings[0]).text();
}

async function ShowManageGroupsModal(btn) {
    let id = GetProxyIdFromDatatableButton(btn);
    
    $('#proxyGroupModalProxyId').val(id);
    $('#manageProxyGroupsModal').find('.modal-title').html(`Manage proxy groups`);
    let groupContainer = GetCurrentGroupsContainer();
    groupContainer.empty();
    
    let groups = await api.GetProxyGroups(id);
    
    groups.forEach(g => AddCurrentGroupRow(id, g.id, g.name, groupContainer));

    let modal = new bootstrap.Modal(document.getElementById('manageProxyGroupsModal'), { backdrop: false });
    modal.show();
}

async function AddCurrentGroupRow(proxyId, groupId, groupName, targetContainer) {
    let row = $(`<div><button type="button" class="btn btn-sm btn-outline-danger me-2 proxyGroup${groupId}" onclick="RemoveFromGroup(${proxyId}, ${groupId});"><i class="fa fa-minus"></i></button>${groupName}</div>`);
    $(targetContainer).append(row);
}

async function RemoveFromGroup(proxyId, groupId) {
    if (groupId == undefined || groupId == 0) return;

    try {
        await api.RemoveProxyFromGroup(proxyId, groupId);

        $(`.proxyGroup${groupId}`).parent('div').remove();
    } catch (e) {
        logger.PrintException(e);
    }
}

// Used in ProxyManager to add a proxy to a selected group in the ProxyGroup modal
async function AddProxyToSelectedGroup() {
    let proxyId = $('#proxyGroupModalProxyId').val();
    let groupId = $('#proxyGroupModalGroupSelect').find('option:selected').val();
    if (groupId == undefined || groupId == 0) return;
    
    try {
        let response = await api.AddProxyToGroup(proxyId, groupId);
        let group = response.data;

        await AddCurrentGroupRow(proxyId, groupId, group.name, GetCurrentGroupsContainer());
    } catch (e) {
        logger.PrintException(e);
    }
}
