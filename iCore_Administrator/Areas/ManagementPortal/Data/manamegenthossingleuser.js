$('#Hospitality_SingleUser_Table').DataTable();
$(".select2").select2({ dropdownAutoWidth: true, width: '100%' });
function SelectUserType(TypeCode) {
    document.getElementById("Txt_Hospitality_SingleUser_CompanyName_Tag").style.display = "none";
    document.getElementById("Txt_Hospitality_SingleUser_Firstname_Tag").style.display = "none";
    document.getElementById("Txt_Hospitality_SingleUser_Lastname_Tag").style.display = "none";
    var LastTest = "";
    var LastTest2 = "";
    if (TypeCode == 1) {
        LastTest = document.getElementById("Txt_Hospitality_SingleUser_Firstname").value.trim() + " " + document.getElementById("Txt_Hospitality_SingleUser_Lastname").value.trim();
        document.getElementById("Txt_Hospitality_SingleUser_CompanyName").value = "";
        document.getElementById("Txt_Hospitality_SingleUser_Firstname").value = "";
        document.getElementById("Txt_Hospitality_SingleUser_Lastname").value = "";
        document.getElementById("Txt_Hospitality_SingleUser_CompanyName").value = LastTest;
        document.getElementById("Txt_Hospitality_SingleUser_CompanyName_Tag").style.display = "block";
    }
    else {
        var Strmain = document.getElementById("Txt_Hospitality_SingleUser_CompanyName").value.trim();
        var res = Strmain.split(" ");
        try {
            if (res.length > 1) {
                for (var i = 0; i < (res.length - 1); i++) {
                    LastTest += res[i].trim() + " ";
                }
                LastTest = LastTest.trim();
                LastTest2 = res[res.length - 1].trim();
            }
            else {
                LastTest = Strmain.trim();
            }
        } catch (e) {
            LastTest = Strmain.trim();
        }
        document.getElementById("Txt_Hospitality_SingleUser_CompanyName").value = "";
        document.getElementById("Txt_Hospitality_SingleUser_Firstname").value = "";
        document.getElementById("Txt_Hospitality_SingleUser_Lastname").value = "";
        document.getElementById("Txt_Hospitality_SingleUser_Firstname").value = LastTest;
        document.getElementById("Txt_Hospitality_SingleUser_Lastname").value = LastTest2;
        document.getElementById("Txt_Hospitality_SingleUser_Firstname_Tag").style.display = "block";
        document.getElementById("Txt_Hospitality_SingleUser_Lastname_Tag").style.display = "block";
    }
}
function SelectUserTypeModal(TypeCode) {
    document.getElementById("Hospitality_SingleUser_Txt_E2").style.display = "none";
    document.getElementById("Hospitality_SingleUser_Txt_E3").style.display = "none";
    document.getElementById("Hospitality_SingleUser_Txt_E4").style.display = "none";
    var LastTest = "";
    var LastTest2 = "";
    if (TypeCode == 1) {
        LastTest = document.getElementById("Modal_Hospitality_SingleUser_Firstname").value.trim() + " " + document.getElementById("Modal_Hospitality_SingleUser_Lastname").value.trim();
        document.getElementById("Modal_Hospitality_SingleUser_CompanyName").value = "";
        document.getElementById("Modal_Hospitality_SingleUser_Firstname").value = "";
        document.getElementById("Modal_Hospitality_SingleUser_Lastname").value = "";
        document.getElementById("Modal_Hospitality_SingleUser_CompanyName").value = LastTest;
        document.getElementById("Hospitality_SingleUser_Txt_E2").style.display = "block";
    }
    else {
        var Strmain = document.getElementById("Modal_Hospitality_SingleUser_CompanyName").value.trim();
        var res = Strmain.split(" ");
        try {
            if (res.length > 1) {
                for (var i = 0; i < (res.length - 1); i++) {
                    LastTest += res[i].trim() + " ";
                }
                LastTest = LastTest.trim();
                LastTest2 = res[res.length - 1].trim();
            }
            else {
                LastTest = Strmain.trim();
            }
        } catch (e) {
            LastTest = Strmain.trim();
        }
        document.getElementById("Modal_Hospitality_SingleUser_CompanyName").value = "";
        document.getElementById("Modal_Hospitality_SingleUser_Firstname").value = "";
        document.getElementById("Modal_Hospitality_SingleUser_Lastname").value = "";
        document.getElementById("Modal_Hospitality_SingleUser_Firstname").value = LastTest;
        document.getElementById("Modal_Hospitality_SingleUser_Lastname").value = LastTest2;
        document.getElementById("Hospitality_SingleUser_Txt_E3").style.display = "block";
        document.getElementById("Hospitality_SingleUser_Txt_E4").style.display = "block";
    }
}
function Hospitality_SingleUser_Clear() {
    document.getElementById("Txt_Hospitality_SingleUser_CompanyName_Tag").style.display = "block";
    document.getElementById("Txt_Hospitality_SingleUser_Firstname_Tag").style.display = "none";
    document.getElementById("Txt_Hospitality_SingleUser_Lastname_Tag").style.display = "none";
    document.getElementById('Btn_Hospitality_SingleUser').style.display = 'block';
    document.getElementById('Btn_Hospitality_SingleUser_Wait').style.display = 'none';
    document.getElementById('Sec_Hospitality_SingleUser_AddElement').style.opacity = '1';
    document.getElementById('Sec_Hospitality_SingleUser_AddElement').style.pointerEvents = 'auto';
    $('#Txt_Hospitality_SingleUser_Type').val("1");
    $('#Txt_Hospitality_SingleUser_Type').select2({ dropdownAutoWidth: true, width: '100%' }).trigger('change');
    document.getElementById('Txt_Hospitality_SingleUser_CompanyName').value = "";
    document.getElementById('Txt_Hospitality_SingleUser_Firstname').value = "";
    document.getElementById('Txt_Hospitality_SingleUser_Lastname').value = "";
    document.getElementById('Txt_Hospitality_SingleUser_Email').value = "";
    document.getElementById('Txt_Hospitality_SingleUser_Description').value = "";
    document.getElementById('Txt_Hospitality_SingleUser_Type').focus();
    $('html, body').animate({ scrollTop: $(Sec_Hospitality_Single_UserAdd).offset().top - 165 }, 'slow');
}
function Hospitality_SingleUser_ReloadGrid() {
    try {
        document.getElementById("Sec_Hospitality_SingleUser_Grid").style.opacity = "0.7";
        document.getElementById("Sec_Hospitality_SingleUser_Grid").style.pointerEvents = "none";
        document.getElementById("Sec_Hospitality_SingleUser_Grid_Wait").style.display = "block";
        var formData = new FormData();
        formData.append("PID", app2.Urls.PID);
        $.ajax({
            url: app2.Urls.U1, data: formData, type: "POST", dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById("Sec_Hospitality_SingleUser_Grid_Wait").style.display = "none";
                document.getElementById("Sec_Hospitality_SingleUser_Grid").style.opacity = "1";
                document.getElementById("Sec_Hospitality_SingleUser_Grid").style.pointerEvents = "auto";
                toastr.error('An error occurred while processing your request to reload list', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    toastr.success("Information list successfully synced", 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    document.getElementById("Sec_Hospitality_SingleUser_Grid_Wait").style.display = "none";
                    document.getElementById("Sec_Hospitality_SingleUser_Grid").style.opacity = "1";
                    document.getElementById("Sec_Hospitality_SingleUser_Grid").style.pointerEvents = "auto";
                    $('#Hospitality_SingleUser_Table').DataTable().destroy();
                    $('#Hospitality_SingleUser_Table').find('tbody').empty();
                    $('#Hospitality_SingleUser_Table').find('tbody').append(ErrorResult);
                    $('#Hospitality_SingleUser_Table').DataTable().draw();
                    oTable = $('#Hospitality_SingleUser_Table').DataTable();
                    oTable.fnPageChange(0);
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById("Sec_Hospitality_SingleUser_Grid_Wait").style.display = "none";
                        document.getElementById("Sec_Hospitality_SingleUser_Grid").style.opacity = "1";
                        document.getElementById("Sec_Hospitality_SingleUser_Grid").style.pointerEvents = "auto";
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app2.Urls.U2; }
                }
            }
        });
    } catch (e) {
        document.getElementById("Sec_Hospitality_SingleUser_Grid_Wait").style.display = "none";
        document.getElementById("Sec_Hospitality_SingleUser_Grid").style.opacity = "1";
        document.getElementById("Sec_Hospitality_SingleUser_Grid").style.pointerEvents = "auto";
        toastr.error('An error occurred while processing your request to reload list', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function Hospitality_SingleUser_AddNew() {
    try {
        if ($("#Txt_Hospitality_SingleUser_Type :selected").val().trim() == "1") {
            if (document.getElementById('Txt_Hospitality_SingleUser_CompanyName').value.trim() == "") { toastr.warning('It is necessary to enter the name, to add a new user as organization', 'An error has occurred ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 }); return; }
        }
        else {
            if (document.getElementById('Txt_Hospitality_SingleUser_Firstname').value.trim() == "") { toastr.warning('It is necessary to enter the first name, to add a new user as person', 'An error has occurred ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 }); return; }
            if (document.getElementById('Txt_Hospitality_SingleUser_Lastname').value.trim() == "") { toastr.warning('It is necessary to enter the last name, to add a new user as person', 'An error has occurred ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 }); return; }
        }
        if (document.getElementById('Txt_Hospitality_SingleUser_Email').value.trim() == "") { toastr.warning('It is necessary to enter the email, to add a new user', 'An error has occurred ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 }); return; }
        document.getElementById('Btn_Hospitality_SingleUser').style.display = 'none';
        document.getElementById('Btn_Hospitality_SingleUser_Wait').style.display = 'block';
        document.getElementById('Sec_Hospitality_SingleUser_AddElement').style.opacity = '0.7';
        document.getElementById('Sec_Hospitality_SingleUser_AddElement').style.pointerEvents = 'none';
        var formData = new FormData();
        formData.append("PID", app2.Urls.PID);
        formData.append("RID", app2.Urls.RID);
        formData.append("TC", $("#Txt_Hospitality_SingleUser_Type :selected").val().trim());
        formData.append("TN", $("#Txt_Hospitality_SingleUser_Type :selected").text().trim());
        formData.append("CN", document.getElementById('Txt_Hospitality_SingleUser_CompanyName').value.trim());
        formData.append("FN", document.getElementById('Txt_Hospitality_SingleUser_Firstname').value.trim());
        formData.append("LN", document.getElementById('Txt_Hospitality_SingleUser_Lastname').value.trim());
        formData.append("EM", document.getElementById('Txt_Hospitality_SingleUser_Email').value.trim());
        formData.append("DC", document.getElementById('Txt_Hospitality_SingleUser_Description').value.trim());

        $.ajax({
            url: app2.Urls.U3, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById('Btn_Hospitality_SingleUser').style.display = 'block';
                document.getElementById('Btn_Hospitality_SingleUser_Wait').style.display = 'none';
                document.getElementById('Sec_Hospitality_SingleUser_AddElement').style.opacity = '1';
                document.getElementById('Sec_Hospitality_SingleUser_AddElement').style.pointerEvents = 'auto';
                toastr.error('An error occurred while processing your request', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    Hospitality_SingleUser_Clear();
                    toastr.success(ErrorResult.trim(), 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    Hospitality_SingleUser_ReloadGrid();
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById('Btn_Hospitality_SingleUser').style.display = 'block';
                        document.getElementById('Btn_Hospitality_SingleUser_Wait').style.display = 'none';
                        document.getElementById('Sec_Hospitality_SingleUser_AddElement').style.opacity = '1';
                        document.getElementById('Sec_Hospitality_SingleUser_AddElement').style.pointerEvents = 'auto';
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app2.Urls.U2; }
                }
            }
        });
    } catch (e) {
        document.getElementById('Btn_Hospitality_SingleUser').style.display = 'block';
        document.getElementById('Btn_Hospitality_SingleUser_Wait').style.display = 'none';
        document.getElementById('Sec_Hospitality_SingleUser_AddElement').style.opacity = '1';
        document.getElementById('Sec_Hospitality_SingleUser_AddElement').style.pointerEvents = 'auto';
        toastr.error('An error occurred while processing your request', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function Hospitality_SingleUser_UserDetails(GID) { var BUrl = app2.Urls.U4; window.open(BUrl.replace("EMASGID", GID), '_blank'); }
function Hospitality_SingleUser_Remove(ID, NM) {
    try {
        Swal.fire({
            title: "Are you sure?",
            text: "If you delete the user " + NM + ", access to the all information is not possible",
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
                formData.append("ID", ID.trim());
                formData.append("NM", NM.trim());
                $.ajax({
                    url: app2.Urls.U5, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
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
                            Hospitality_SingleUser_ReloadGrid();
                        }
                        else {
                            if (ErrorCode != "2") {
                                toastr.clear();
                                toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                            }
                            else { window.location.href = app2.Urls.U2 }
                        }
                    }
                });
            }
        })
    } catch (e) {
        toastr.error('An error occurred while processing your request', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function Hospitality_SingleUser_ChangeStatus(ID, Name) {
    try {
        toastr.clear();
        toastr.info('The system is reviewing your request', 'Please wait ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
        var formData = new FormData();
        formData.append("ID", ID.trim());
        formData.append("NM", Name.trim());
        $.ajax({
            url: app2.Urls.U6, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
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
                    Hospitality_SingleUser_ReloadGrid();
                }
                else {
                    if (ErrorCode != "2") {
                        toastr.clear();
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app2.Urls.U2; }
                }
            }
        });
    } catch (e) {
        toastr.error('An error occurred while processing your request', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function Hospitality_SingleUser_Edit(ID, TP, FM, LN, EM, DC) {
    ID = ID.trim(); TP = TP.trim(); FM = FM.trim(); LN = LN.trim(); EM = EM.trim(); DC = DC.trim();
    var Lstnm = FM; if (TP == "2") { Lstnm = Lstnm + " " + LN}
    document.getElementById("Modal_Hospitality_SingleUser_Editor_Title").innerText = " Edit User Information [ " + Lstnm + " ]";
    document.getElementById("Hospitality_SingleUser_Txt_E1").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E1").style.pointerEvents = "auto";
    document.getElementById("Hospitality_SingleUser_Txt_E2").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E2").style.pointerEvents = "auto";
    document.getElementById("Hospitality_SingleUser_Txt_E3").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E3").style.pointerEvents = "auto";
    document.getElementById("Hospitality_SingleUser_Txt_E4").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E4").style.pointerEvents = "auto";
    document.getElementById("Hospitality_SingleUser_Txt_E5").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E5").style.pointerEvents = "auto";
    document.getElementById("Hospitality_SingleUser_Txt_E6").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E6").style.pointerEvents = "auto";
    $('#Modal_Hospitality_SingleUser_Editor_Type').val("1");
    $('#Modal_Hospitality_SingleUser_Editor_Type').select2({ dropdownAutoWidth: true, width: '100%' }).trigger('change');
    document.getElementById("Modal_Hospitality_SingleUser_CompanyName").value = "";
    document.getElementById("Modal_Hospitality_SingleUser_Firstname").value = "";
    document.getElementById("Modal_Hospitality_SingleUser_Lastname").value = "";
    document.getElementById("Modal_Hospitality_SingleUser_Email").value = "";
    document.getElementById("Modal_Hospitality_SingleUser_Description").value = "";
    try {
        $('#Modal_Hospitality_SingleUser_Editor_Type').val(TP);
        $('#Modal_Hospitality_SingleUser_Editor_Type').select2({ dropdownAutoWidth: true, width: '100%' }).trigger('change');
        document.getElementById("Modal_Hospitality_SingleUser_CompanyName").value = FM;
        document.getElementById("Modal_Hospitality_SingleUser_Firstname").value = FM;
        document.getElementById("Modal_Hospitality_SingleUser_Lastname").value = LN;
        document.getElementById("Modal_Hospitality_SingleUser_Email").value = EM;
        document.getElementById("Modal_Hospitality_SingleUser_Description").value = DC;
    } catch (e) { }
    document.getElementById("Modal_Hospitality_SingleUser_Wait").style.display = "none";
    document.getElementById("Modal_Hospitality_SingleUser_Btn").style.opacity = "1";
    document.getElementById("Modal_Hospitality_SingleUser_Btn").style.pointerEvents = "auto";
    document.getElementById("Modal_Hospitality_SingleUser_ID").value = ID;
    $('#Modal_Hospitality_SingleUser_Editor').modal('show');
    document.getElementById('Modal_Hospitality_SingleUser_Editor_Type').focus();
}



function Hospitality_SingleUser_Edit_Save() {
    try {


        if ($("#Modal_Hospitality_SingleUser_Editor_Type :selected").val().trim() == "1") {
            if (document.getElementById('Modal_Hospitality_SingleUser_CompanyName').value.trim() == "") { toastr.warning('It is necessary to enter the name, to save user as organization', 'An error has occurred ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 }); return; }
        }
        else {
            if (document.getElementById('Modal_Hospitality_SingleUser_Firstname').value.trim() == "") { toastr.warning('It is necessary to enter the first name, to save user as person', 'An error has occurred ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 }); return; }
            if (document.getElementById('Modal_Hospitality_SingleUser_Lastname').value.trim() == "") { toastr.warning('It is necessary to enter the last name, to save user as person', 'An error has occurred ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 }); return; }
        }
        if (document.getElementById('Modal_Hospitality_SingleUser_Email').value.trim() == "") { toastr.warning('It is necessary to enter the email, to save user', 'An error has occurred ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 }); return; }
        document.getElementById("Hospitality_SingleUser_Txt_E1").style.opacity = "0.7"; document.getElementById("Hospitality_SingleUser_Txt_E1").style.pointerEvents = "none";
        document.getElementById("Hospitality_SingleUser_Txt_E2").style.opacity = "0.7"; document.getElementById("Hospitality_SingleUser_Txt_E2").style.pointerEvents = "none";
        document.getElementById("Hospitality_SingleUser_Txt_E3").style.opacity = "0.7"; document.getElementById("Hospitality_SingleUser_Txt_E3").style.pointerEvents = "none";
        document.getElementById("Hospitality_SingleUser_Txt_E4").style.opacity = "0.7"; document.getElementById("Hospitality_SingleUser_Txt_E4").style.pointerEvents = "none";
        document.getElementById("Hospitality_SingleUser_Txt_E5").style.opacity = "0.7"; document.getElementById("Hospitality_SingleUser_Txt_E5").style.pointerEvents = "none";
        document.getElementById("Hospitality_SingleUser_Txt_E6").style.opacity = "0.7"; document.getElementById("Hospitality_SingleUser_Txt_E6").style.pointerEvents = "none";
        document.getElementById("Modal_Hospitality_SingleUser_Wait").style.display = "block";
        document.getElementById("Modal_Hospitality_SingleUser_Btn").style.opacity = "0.7";
        document.getElementById("Modal_Hospitality_SingleUser_Btn").style.pointerEvents = "none";
        var formData = new FormData();
        formData.append("PID", app2.Urls.PID);
        formData.append("ID", document.getElementById('Modal_Hospitality_SingleUser_ID').value.trim());
        formData.append("TC", $("#Modal_Hospitality_SingleUser_Editor_Type :selected").val().trim());
        formData.append("TN", $("#Modal_Hospitality_SingleUser_Editor_Type :selected").text().trim());
        formData.append("CN", document.getElementById('Modal_Hospitality_SingleUser_CompanyName').value.trim());
        formData.append("FN", document.getElementById('Modal_Hospitality_SingleUser_Firstname').value.trim());
        formData.append("LN", document.getElementById('Modal_Hospitality_SingleUser_Lastname').value.trim());
        formData.append("EM", document.getElementById('Modal_Hospitality_SingleUser_Email').value.trim());
        formData.append("DC", document.getElementById('Modal_Hospitality_SingleUser_Description').value.trim());
        $.ajax({
            url: app2.Urls.U7, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById("Hospitality_SingleUser_Txt_E1").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E1").style.pointerEvents = "auto";
                document.getElementById("Hospitality_SingleUser_Txt_E2").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E2").style.pointerEvents = "auto";
                document.getElementById("Hospitality_SingleUser_Txt_E3").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E3").style.pointerEvents = "auto";
                document.getElementById("Hospitality_SingleUser_Txt_E4").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E4").style.pointerEvents = "auto";
                document.getElementById("Hospitality_SingleUser_Txt_E5").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E5").style.pointerEvents = "auto";
                document.getElementById("Hospitality_SingleUser_Txt_E6").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E6").style.pointerEvents = "auto";
                document.getElementById("Modal_Hospitality_SingleUser_Wait").style.display = "none";
                document.getElementById("Modal_Hospitality_SingleUser_Btn").style.opacity = "1";
                document.getElementById("Modal_Hospitality_SingleUser_Btn").style.pointerEvents = "auto";
                toastr.error('An error occurred while processing your request', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    $('#Modal_Hospitality_SingleUser_Editor').modal('hide');
                    toastr.success(ErrorResult.trim(), 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    Hospitality_SingleUser_ReloadGrid();
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById("Hospitality_SingleUser_Txt_E1").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E1").style.pointerEvents = "auto";
                        document.getElementById("Hospitality_SingleUser_Txt_E2").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E2").style.pointerEvents = "auto";
                        document.getElementById("Hospitality_SingleUser_Txt_E3").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E3").style.pointerEvents = "auto";
                        document.getElementById("Hospitality_SingleUser_Txt_E4").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E4").style.pointerEvents = "auto";
                        document.getElementById("Hospitality_SingleUser_Txt_E5").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E5").style.pointerEvents = "auto";
                        document.getElementById("Hospitality_SingleUser_Txt_E6").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E6").style.pointerEvents = "auto";
                        document.getElementById("Modal_Hospitality_SingleUser_Wait").style.display = "none";
                        document.getElementById("Modal_Hospitality_SingleUser_Btn").style.opacity = "1";
                        document.getElementById("Modal_Hospitality_SingleUser_Btn").style.pointerEvents = "auto";
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app2.Urls.U2; }
                }
            }
        });
    } catch (e) {
        document.getElementById("Hospitality_SingleUser_Txt_E1").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E1").style.pointerEvents = "auto";
        document.getElementById("Hospitality_SingleUser_Txt_E2").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E2").style.pointerEvents = "auto";
        document.getElementById("Hospitality_SingleUser_Txt_E3").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E3").style.pointerEvents = "auto";
        document.getElementById("Hospitality_SingleUser_Txt_E4").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E4").style.pointerEvents = "auto";
        document.getElementById("Hospitality_SingleUser_Txt_E5").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E5").style.pointerEvents = "auto";
        document.getElementById("Hospitality_SingleUser_Txt_E6").style.opacity = "1"; document.getElementById("Hospitality_SingleUser_Txt_E6").style.pointerEvents = "auto";
        document.getElementById("Modal_Hospitality_SingleUser_Wait").style.display = "none";
        document.getElementById("Modal_Hospitality_SingleUser_Btn").style.opacity = "1";
        document.getElementById("Modal_Hospitality_SingleUser_Btn").style.pointerEvents = "auto";
        toastr.error('An error occurred while processing your request', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
