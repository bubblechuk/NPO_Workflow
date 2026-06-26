// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(() => {
    const controller = $("#crud-container").data("controller")
    $('#btn-crud-create').click(function () {
        var url = '/' + controller + '/Create'
        $('#modalTitle').text("Новая запись");
        $('#modalBodyContainer').load(url, function () {
            $('#createModal').modal('show')
        })
    })
    $('.tbody-operations').on('click', '.btn-crud-modify', function () {
        var operationId = $(this).data('id')
        var url = '/' + controller + "/Modify" + '/' + operationId
        $('#modalTitle').text("Редактирование записи")
        $('#modalBodyContainer').load(url, function () {
            $('#createModal').modal('show')
        })
    })
    $('.tbody-operations').on('click', '.btn-crud-delete', function () {
        var operationId = $(this).data('id')
        var url = '/' + controller + '/Delete';
        var $row = $(this).closest('tr')
        if (confirm(`Вы уверены что хотите удалить запись с ID: ${operationId}?`)) {
            $.ajax({
                url: url,
                type: 'POST',
                data: { id: operationId },
                success: function (result) {
                    $row.fadeOut(400, function () {
                        $(this).remove();
                        window.location.reload();
                    })
                },
                error: function (xhr, status, error) {
                    alert('Ошибка удаления записи: ' + error)
                }
            })
        }
    })

    $('#createModal').on('submit', 'form', function (e) {
        e.preventDefault();

        var $form = $(this);
        var url = $form.attr('action');
        var formData = $form.serialize();

        $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            success: function (response, status, xhr) {
                if (response && response.indexOf('<form') !== -1) {
                    $('#modalBodyContainer').html(response);
                    var $newForm = $('#modalBodyContainer').find('form');
                    if (typeof $.validator !== 'undefined' && $.validator.unobtrusive) {
                        $.validator.unobtrusive.parse($newForm);
                    }
                } else {
                    $('#createModal').modal('hide');
                    window.location.reload();
                }
            },
            error: function (xhr, status, error) {
                alert("Ошибка при отправке данных: " + error);
            }
        });
    });
})