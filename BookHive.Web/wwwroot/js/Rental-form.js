var selectedCopies = [];
var isEditMode = false;
var currentcopies = [];
function OnAddCopySuccess(data) {
    $("#Value").val('');
    var bookId = $(data).find('.js-copy').data('book');
  
    if (selectedCopies.find(c => c.bookId == bookId)) {
        showErrorMessage('You cannot add more than one copy for the same book');
        return;
    }
    $("#CopiesForm").prepend(data);
    $("#CopiesForm").find(':submit').removeClass('d-none');
  
    prepareInput();

}
$(document).ready(function () {

    if ($('.js-copy').length > 0) {
        prepareInput();
        currentcopies = selectedCopies;
        isEditMode = true;
    }
       

    $('.js-search').on('click', function (e) {
        e.preventDefault();

        var serial = $("#Value").val();

        if (selectedCopies.find(c => c.serial == serial)) {
            showErrorMessage('You cannot add the same copy');
            return;
        }

        if (selectedCopies.length >= max) {
            showErrorMessage(`You can add up to ${max} books`)
            return;
        }

        $("#SearchForm").submit();
    })

    $("body").delegate('.js-remove', 'click', function () {

        var btn = $(this)
        var container = btn.parents('.js-copy-container');

        if (isEditMode) {
            btn.toggleClass('btn-light-danger btn-light-success js-remove js-readd').text('Re-Add');
            container.find('img').css('opacity', '0.5');
            container.find('h4').css('text-decoration', 'line-through');
            container.find('.js-copy').toggleClass('js-copy js-removed').removeAttr('name').removeAttr('id');

        } else {
            container.remove();
        }


        prepareInput();
        if (selectedCopies.length == 0 || JSON.stringify(currentcopies) == JSON.stringify(selectedCopies)) {
            $("#CopiesForm").find(':submit').addClass('d-none');
        }
        else {
            $("#CopiesForm").find(':submit').removeClass('d-none');

        }
    });
    $("body").delegate('.js-readd', 'click', function () {

        var btn = $(this)
        var container = btn.parents('.js-copy-container');


        btn.toggleClass('btn-light-danger btn-light-success js-remove js-readd').text('Remove');
        container.find('img').css('opacity', '1');
        container.find('h4').css('text-decoration', 'none');
        container.find('.js-removed').toggleClass('js-copy js-removed');
        prepareInput();


        if (JSON.stringify(currentcopies) == JSON.stringify(selectedCopies)) {
            $("#CopiesForm").find(':submit').addClass('d-none');

        } else {
            $("#CopiesForm").find(':submit').removeClass('d-none');
        }
            
        

        
    });

});

   


function prepareInput() {
    var copies = $(".js-copy");
    selectedCopies = [];
    $.each(copies, function (i, input) {
        var $input = $(input);
        selectedCopies.push({ serial: $input.val(), bookId: $input.data('book') });
        $input.attr('name', `SelectedCopies[${i}]`).attr('id', `SelectedCopies_${i}_`);
    });
}







