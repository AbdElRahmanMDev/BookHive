$(document).ready(function () {
    $('#GovernorateId').on('change', function () {

        var goverment = $(this).val();
        var areaList = $('#AreaId');
        areaList.empty();
        areaList.append('<option></option>');
        if (goverment !== '') {
            $.ajax({
                url: 'Subscriber/get-area?govermentId=' + goverment,
                success: function (Areas) {
                    //Areas ----> Ienum of Area that has 
                    $.each(Areas, function (i, area) {
                        var item = $('<option></option>').attr("value", area.value).text(area.text);
                        areaList.append(item);
                    })
                },
                error: function () {
                    showErrorMessage();
                }

            })
        }

    });
    $('.js-renew').on('click', function () {
        var btn = $(this)
        var table = $('#SubscriptionsTable');

        $.ajax({
            url: btn.data('url'),
            success: function (data) {
                table.append(data);
            },
            error: function () {
                showErrorMessage();
            }
        })
    })

});