

var UpdateRow;
var table;
var datatable;
var exportedCols=[];
function showSuccessMeesage(message = "Saved Succesfuly") {
    Swal.fire({
        icon: "success",
        title: "sucess...",
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
    var modal = $("#Modal");
    modal.modal('hide');
}
function showErrorMessage(message = "Something went wrong!") {
    Swal.fire({
        icon: "error",
        title: "Oops...",
        text: message.responseText != undefined ? message.responseText : message,
        footer: '<a href="#">Why do I have this issue?</a>',
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}

function OnModelBegin() {
    disabledSubmitButton();
}
function disabledSubmitButton() {
    $('body :submit').attr('disabled', 'disabled');
} 

function select_2(){
    $(".js-select2").select2();
    //Select 2 for validation
    $('.js-select2').on('select2:select', function (e) {
        var select = $(this);
        $("form").not('#SignOut').validate().element('#' + select.attr('id'));
    });
}
function onModelSuccess(item) {
    console.log(item);
    showSuccessMeesage(); // Ensure this function is correctly spelled and exists

    var modal = $("#Modal");
    modal.modal('hide');

    // Check if UpdateRow is defined before attempting to remove it
    if (typeof UpdateRow !== "undefined" && UpdateRow !== null) {
        datatable.row(UpdateRow).remove().draw();
        UpdateRow = undefined;
    }

    // Ensure item is a valid DataTable row
    if (item) {
        var newrow = $(item);
        datatable.row.add(newrow).draw();
    } else {
        console.error("Error: item is undefined or null.");
    }
    KTMenu.init();
    KTMenu.initGlobalHandlers();
    
    //if (UpdateRow === undefined) {
    //    $('tbody').append(item);
    //}
    //else {
    //    $(UpdateRow).replaceWith(item);
    //    UpdateRow = undefined
    //}
 
}
function OnModelComplete() {
    $('body :submit').removeAttr('disabled');

}
//Data Tables
var headers = $('th');
$.each(headers, function (i) {

    var col = $(this);
    if (!col.hasClass("js-no-export"))
        exportedCols.push(i);
});


//Datatables
//class definition
var KTDatatables = function () {
    // Private functions
    var initDatatable = function () {

        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'pageLength': 10,
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $('.js-datatables').data("document-title"); 
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Public methods
    return {
        init: function () {
            table = document.querySelector('.js-datatables');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();

$(document).ready(function () {
    //Disable submit button
    $('form').not('#SignOut').on('submit', function () {
        if ($('.js-tinymce').length > 0) {
            $('.js-tinymce').each(function () {

                var input = $(this)
                var content = tinyMCE.get(input.attr('id')).getContent();
                input.val(content);

            });
        }
        var Isvalid = $(this).valid();
        if (Isvalid) {
            disabledSubmitButton();
        }
    });
    //select 2 For Drop Down List
  
    //date picker For Date
    $(".js-datepicker").daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        drops: 'up',
        maxDate:new Date()
    });

    //tinymce For TextArea to render Image only in form Book
    if ($(".js-tinymce").length > 0) {
        var options = { selector: ".js-tinymce", height: "433px" };
        if (KTThemeMode.getMode() === "dark") {
            options["skin"] = "oxide-dark";
            options["content_css"] = "dark";
        }
        tinymce.init(options);
    }
    //SweetAlert
    var message = $("#Message").text();
    if (message !== '') {
        showSuccessMeesage(message);
    } 
    //Data Tables 
    KTUtil.onDOMContentLoaded(function () {
        KTDatatables.init();
    });

    //Handle bootstrap Modal
    $("body").delegate('.js-render-modal','click', function () {
        if ($(this).data("update") !== undefined) {
            UpdateRow = $(this).parents('tr');
            console.log(UpdateRow);
        }
        var title = $(this).data("title");
        var modal = $("#Modal");
        $("#ModalLabel").text(title);
        modal.modal('show');

        $.get({
            url: $(this).data('url'),
            success: function (data) {
                console.log(data);
                $(".modal-body").html(data);
                $.validator.unobtrusive.parse(modal);
                select_2();
            },
            error: function () {
                showErrorMessage();
            }
        });
      
    });
    $("body").delegate('.js-toggle-status', 'click', function () {
        var btn = $(this);

        bootbox.confirm({
            message: "Are you sure that you need to toggle the state?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-danger'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: btn.data('url'),
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (data) {
                            console.log(data);

                            var row = btn.parents('tr');
                            var status = row.find('.js-status');
                            var newStatus = status.text().trim() === 'Deleted' ? 'Available' : 'Deleted';

                            status.text(newStatus)
                                .toggleClass('badge-light-success badge-light-danger');
                            row.find('.js-update-on').text(data);
                            row.addClass('animate__animated animate__flash');
                            showSuccessMeesage();
                        },
                        error: function () {
                            showErrorMessage();
                        }
                    });
                }
            }
        });
    });

    $("body").delegate('.js-password', 'click', function () {
        var title = $(this).data("title");
        var modal = $("#Modal");
        $("#ModalLabel").text(title);
        modal.modal('show');
        $.get({
            url: $(this).data('url'),
            success: function (data) {
                console.log(data);
                $(".modal-body").html(data);
                $.validator.unobtrusive.parse(modal);
                select_2();
            },
            error: function () {
                showErrorMessage();
            }
        });
    });


    $(".js-signout").on('click', function () {
        $('#SignOut').submit();
    });

});
//data - title="F Category" >
