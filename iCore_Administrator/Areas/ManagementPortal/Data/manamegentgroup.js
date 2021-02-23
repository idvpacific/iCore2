$('#GroupUser_Table').DataTable();
$(".select2").select2({ dropdownAutoWidth: true, width: '100%' });
function GroupUser_ReloadGrid() {
    try {
        document.getElementById("Sec_GroupUser_Grid").style.opacity = "0.7";
        document.getElementById("Sec_GroupUser_Grid").style.pointerEvents = "none";
        document.getElementById("Sec_GroupUser_Grid_Wait").style.display = "block";
        var formData = new FormData();
        formData.append("PID", app.Urls.PID);
        $.ajax({
            url: app.Urls.U1, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById("Sec_GroupUser_Grid_Wait").style.display = "none";
                document.getElementById("Sec_GroupUser_Grid").style.opacity = "1";
                document.getElementById("Sec_GroupUser_Grid").style.pointerEvents = "auto";
                toastr.error('An error occurred while processing your request to reload group list', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    toastr.success("Information list successfully synced", 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    document.getElementById("Sec_GroupUser_Grid_Wait").style.display = "none";
                    document.getElementById("Sec_GroupUser_Grid").style.opacity = "1";
                    document.getElementById("Sec_GroupUser_Grid").style.pointerEvents = "auto";
                    $('#GroupUser_Table').DataTable().destroy();
                    $('#GroupUser_Table').find('tbody').empty();
                    $('#GroupUser_Table').find('tbody').append(ErrorResult);
                    $('#GroupUser_Table').DataTable().draw();
                    oTable = $('#GroupUser_Table').DataTable();
                    oTable.fnPageChange(0);
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById("Sec_GroupUser_Grid_Wait").style.display = "none";
                        document.getElementById("Sec_GroupUser_Grid").style.opacity = "1";
                        document.getElementById("Sec_GroupUser_Grid").style.pointerEvents = "auto";
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app.Urls.U2; }
                }
            }
        });
    } catch (e) {
        document.getElementById("Sec_GroupUser_Grid_Wait").style.display = "none";
        document.getElementById("Sec_GroupUser_Grid").style.opacity = "1";
        document.getElementById("Sec_GroupUser_Grid").style.pointerEvents = "auto";
        toastr.error('An error occurred while processing your request to reload group list', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function GroupUser_Clear() {
    document.getElementById('Btn_GroupUser').style.display = 'block';
    document.getElementById('Btn_GroupUser_Wait').style.display = 'none';
    document.getElementById('Sec_GroupUser_AddElement').style.opacity = '1';
    document.getElementById('Sec_GroupUser_AddElement').style.pointerEvents = 'auto';
    document.getElementById('Txt_GroupUser_Name').value = "";
    $('#Txt_GroupUser_Industry').val("1");
    $('#Txt_GroupUser_Industry').select2({ dropdownAutoWidth: true, width: '100%' }).trigger('change');
    document.getElementById('Txt_GroupUser_Description').value = "";
    document.getElementById('Txt_GroupUser_Name').focus();
    $('html, body').animate({ scrollTop: 0 }, 'slow');
}
function GroupUser_AddNew() {
    try {
        var NM = document.getElementById('Txt_GroupUser_Name').value.trim();
        if (NM == "") { toastr.warning('It is necessary to enter the name, to add a new group', 'An error has occurred ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 }); return; }
        document.getElementById('Btn_GroupUser').style.display = 'none';
        document.getElementById('Btn_GroupUser_Wait').style.display = 'block';
        document.getElementById('Sec_GroupUser_AddElement').style.opacity = '0.7';
        document.getElementById('Sec_GroupUser_AddElement').style.pointerEvents = 'none';
        var formData = new FormData();
        formData.append("PID", app.Urls.PID);
        formData.append("NM", NM);
        formData.append("DC", document.getElementById('Txt_GroupUser_Description').value.trim());
        formData.append("RID", app.Urls.RID);
        $.ajax({
            url: app.Urls.U3, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById('Btn_GroupUser').style.display = 'block';
                document.getElementById('Btn_GroupUser_Wait').style.display = 'none';
                document.getElementById('Sec_GroupUser_AddElement').style.opacity = '1';
                document.getElementById('Sec_GroupUser_AddElement').style.pointerEvents = 'auto';
                toastr.error('An error occurred while processing your request', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    GroupUser_Clear();
                    toastr.success(ErrorResult.trim(), 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    GroupUser_ReloadGrid();
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById('Btn_GroupUser').style.display = 'block';
                        document.getElementById('Btn_GroupUser_Wait').style.display = 'none';
                        document.getElementById('Sec_GroupUser_AddElement').style.opacity = '1';
                        document.getElementById('Sec_GroupUser_AddElement').style.pointerEvents = 'auto';
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app.Urls.U2; }
                }
            }
        });
    } catch (e) {
        document.getElementById('Btn_GroupUser').style.display = 'block';
        document.getElementById('Btn_GroupUser_Wait').style.display = 'none';
        document.getElementById('Sec_GroupUser_AddElement').style.opacity = '1';
        document.getElementById('Sec_GroupUser_AddElement').style.pointerEvents = 'auto';
        toastr.error('An error occurred while processing your request', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function GroupUser_GroupShow(GID) { var BUrl = app.Urls.U4; window.location.href = BUrl.replace("EMASGID", GID); }
function GroupUser_Remove(ID, Name) {
    try {
        Swal.fire({
            title: "Are you sure?",
            text: "If you delete the group named " + Name + ", access to the all subgroups and members is not possible",
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!',
            confirmButtonClass: 'btn btn-primary',
            cancelButtonClass: 'btn btn-danger ml-1',
            buttonsStyling: false,
        }).then(function (result) {
            if (result.value) {
                toastr.clear();
                toastr.info('The system is reviewing your request', 'Please wait ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                var formData = new FormData();
                formData.append("PID", app.Urls.PID);
                formData.append("ID", ID.trim());
                formData.append("NM", Name.trim());
                $.ajax({
                    url: app.Urls.U5, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
                    error: function () {
                        toastr.clear();
                        toastr.error('An error occurred while processing your request', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    },
                    success: function (data) {
                        var ErrorCode = ""; var ErrorResult = "";
                        $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                        if (ErrorCode == "0") {
                            toastr.clear();
                            toastr.success(ErrorResult.trim(), 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                            GroupUser_ReloadGrid();
                        }
                        else {
                            if (ErrorCode != "2") {
                                toastr.clear();
                                toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                            }
                            else { window.location.href = app.Urls.U2 }
                        }
                    }
                });
            }
        })
    } catch (e) {
        toastr.error('An error occurred while processing your request', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function GroupUser_ChangeStatus(ID, Name) {
    try {
        toastr.clear();
        toastr.info('The system is reviewing your request', 'Please wait ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
        var formData = new FormData();
        formData.append("PID", app.Urls.PID);
        formData.append("ID", ID.trim());
        formData.append("NM", Name.trim());
        $.ajax({
            url: app.Urls.U6, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                toastr.clear();
                toastr.error('An error occurred while processing your request', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    toastr.clear();
                    toastr.success(ErrorResult.trim(), 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    GroupUser_ReloadGrid();
                }
                else {
                    if (ErrorCode != "2") {
                        toastr.clear();
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app.Urls.U2; }
                }
            }
        });
    } catch (e) {
        toastr.error('An error occurred while processing your request', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function GroupUser_Edit(ID, Name, Desc) {
    ID = ID.trim();
    Name = Name.trim();
    Desc = Desc.trim();
    document.getElementById("Modal_GroupUser_Editor_Title").innerText = " Edit Group Information [ " + Name + " ]";
    document.getElementById("GroupUser_Txt_E1").style.opacity = "1";
    document.getElementById("GroupUser_Txt_E1").style.pointerEvents = "auto";
    document.getElementById("Modal_GroupUser_Editor_Name").value = Name;
    document.getElementById("GroupUser_Txt_E2").style.opacity = "1";
    document.getElementById("GroupUser_Txt_E2").style.pointerEvents = "auto";
    document.getElementById("Modal_GroupUser_Editor_Description").value = Desc;
    document.getElementById("Modal_GroupUser_Wait").style.display = "none";
    document.getElementById("Modal_GroupUser_Btn").style.opacity = "1";
    document.getElementById("Modal_GroupUser_Btn").style.pointerEvents = "auto";
    document.getElementById("Modal_GroupUser_ID").value = ID;
    $('#Modal_GroupUser_Editor').modal('show');
    document.getElementById('Modal_GroupUser_Editor_Name').focus();
}
function GroupUser_Edit_Save() {
    try {
        var ID = document.getElementById('Modal_GroupUser_ID').value.trim();
        var NM = document.getElementById('Modal_GroupUser_Editor_Name').value.trim();
        var DC = document.getElementById('Modal_GroupUser_Editor_Description').value.trim();
        if (NM == "") { toastr.warning('It is necessary to enter the name, to edit group', 'An error has occurred ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 }); return; }
        document.getElementById("GroupUser_Txt_E1").style.opacity = "0.7";
        document.getElementById("GroupUser_Txt_E1").style.pointerEvents = "none";
        document.getElementById("GroupUser_Txt_E2").style.opacity = "0.7";
        document.getElementById("GroupUser_Txt_E2").style.pointerEvents = "none";
        document.getElementById("Modal_GroupUser_Wait").style.display = "block";
        document.getElementById("Modal_GroupUser_Btn").style.opacity = "0.7";
        document.getElementById("Modal_GroupUser_Btn").style.pointerEvents = "none";
        var formData = new FormData();
        formData.append("PID", app.Urls.PID);
        formData.append("ID", ID);
        formData.append("NM", NM);
        formData.append("DC", DC);
        $.ajax({
            url: app.Urls.U7, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById("GroupUser_Txt_E1").style.opacity = "1";
                document.getElementById("GroupUser_Txt_E1").style.pointerEvents = "auto";
                document.getElementById("GroupUser_Txt_E2").style.opacity = "1";
                document.getElementById("GroupUser_Txt_E2").style.pointerEvents = "auto";
                document.getElementById("Modal_GroupUser_Wait").style.display = "none";
                document.getElementById("Modal_GroupUser_Btn").style.opacity = "1";
                document.getElementById("Modal_GroupUser_Btn").style.pointerEvents = "auto";
                toastr.error('An error occurred while processing your request', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    $('#Modal_GroupUser_Editor').modal('hide');
                    toastr.success(ErrorResult.trim(), 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    GroupUser_ReloadGrid();
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById("GroupUser_Txt_E1").style.opacity = "1";
                        document.getElementById("GroupUser_Txt_E1").style.pointerEvents = "auto";
                        document.getElementById("GroupUser_Txt_E2").style.opacity = "1";
                        document.getElementById("GroupUser_Txt_E2").style.pointerEvents = "auto";
                        document.getElementById("Modal_GroupUser_Wait").style.display = "none";
                        document.getElementById("Modal_GroupUser_Btn").style.opacity = "1";
                        document.getElementById("Modal_GroupUser_Btn").style.pointerEvents = "auto";
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app.Urls.U2; }
                }
            }
        });
    } catch (e) {
        document.getElementById("GroupUser_Txt_E1").style.opacity = "1";
        document.getElementById("GroupUser_Txt_E1").style.pointerEvents = "auto";
        document.getElementById("GroupUser_Txt_E2").style.opacity = "1";
        document.getElementById("GroupUser_Txt_E2").style.pointerEvents = "auto";
        document.getElementById("Modal_GroupUser_Wait").style.display = "none";
        document.getElementById("Modal_GroupUser_Btn").style.opacity = "1";
        document.getElementById("Modal_GroupUser_Btn").style.pointerEvents = "auto";
        toastr.error('An error occurred while processing your request', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
