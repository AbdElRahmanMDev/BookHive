$(document).ready(function () {
    console.log($('.js-count').val());
    $('.js-renew').on('click', function () {

        var subscriberKey = $(this).data('key');

        bootbox.confirm({
            message: "Are you sure that you need to renew this subscription?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: `/Subscribers/Renew?sKey=${subscriberKey}`,
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (row) {
                            $('#SubscriptionsTable').find('tbody').append(row);
                            $('#RentalButton').removeClass('d-none');
                            var activeIcon = $('#ActiveStatusIcon');
                            activeIcon.removeClass('d-none');
                            activeIcon.siblings('svg').remove();
                            activeIcon.parents('.card').removeClass('bg-warning').addClass('bg-success');



                            $('#CardStatus').text('Active subscriber');
                            $('#StatusBadge').removeClass('badge-light-warning').addClass('badge-light-success').text('Active subscriber');
                            showSuccessMessage();
                        },
                        error: function () {
                            showErrorMessage();
                        }
                    });
                }
            }
        });
    });
   
    $('.js-cancel-rental').on('click', function () {

        var btn = $(this);

        bootbox.confirm({
            message: "Are you sure that you need to Cancel this Rental?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: `/Rentals/Cancel/` + btn.data('id'),
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (count) {
                            btn.parents('tr').remove();

                            if ($('#RentalsTable tbody tr').length === 0) {
                                $('#RentalsTable').fadeOut(function () {
                                    $('#Alert').fadeIn();
                                });
                            }

                            var totalCopies = $("#TotalCopies");
                            var CurrentCount = parseInt(totalCopies.text());
                            totalCopies.text(CurrentCount - count);

                        },
                        error: function () {
                            showErrorMessage();
                        }
                    });
                }
            }
        });
    });
});             