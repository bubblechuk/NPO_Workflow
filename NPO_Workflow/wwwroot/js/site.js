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
    $('.tbody-operations').on('click', '.btn-crud-modify', function (e) {
        e.stopPropagation();
        var operationId = $(this).data('id')
        var url = '/' + controller + "/Modify" + '/' + operationId
        $('#modalTitle').text("Редактирование записи")
        $('#modalBodyContainer').load(url, function () {
            $('#createModal').modal('show')
        })
    })
    $('.tbody-operations').on('click', '.btn-crud-delete', function (e) {
        e.stopPropagation();
        var operationId = $(this).data('id');
        var url = '/' + controller + '/Delete';
        var $row = $(this).closest('tr');
        if (confirm(`Вы уверены что хотите удалить запись с ID: ${operationId}?`)) {
            $.ajax({
                url: url,
                type: 'POST',
                data: {id: operationId},
                success: function (result) {
                    $row.fadeOut(400, function () {
                        $(this).remove();
                        window.location.reload();
                    });
                },
                error: function (xhr, status, error) {
                    alert('Ошибка удаления записи: ' + error);
                }
            });
        }
    });
    $('#createModal').on('submit', 'form', function (e) {
        e.preventDefault();

        var $form = $(this);
        var url = $form.attr('action');
        var currentUrl = window.location.href;
        var formData = $form.serialize();
        if (currentUrl.includes("Technologies/Operations")) {
            url += '?techid=' + currentUrl.split("/")[5];
        }
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
    $('.tbody-operations').on('click', '.tbody-tr', function () {

        var techId = $(this).find('td[data="tech-id"]').text().trim();
        window.location.href = '/Technologies/Operations/' + techId;
    });
    $('.tbody-operations').on('click', '.op-row', function (e) {
        if ($(e.target).closest('.btn-crud-modify, .btn-crud-delete').length > 0) {
            return;
        }
        var $radio = $(this).find('.op-radio');
        $radio.prop('checked', true);
        $('.op-row').removeClass('table-primary fw-bold');
        $(this).addClass('table-primary fw-bold');
        $('#btn-move-up, #btn-move-down').prop('disabled', false);
    });

    $('#btn-move-up').on('click', function () {
        var selectedId = $('.op-radio:checked').val();
        if (selectedId) {
            window.location.replace('/TechOps/Move?techopid=' + selectedId + '&direction=1&selectedOpId=' + selectedId);
        }
    });

    $('#btn-move-down').on('click', function () {
        var selectedId = $('.op-radio:checked').val();
        if (selectedId) {
            window.location.replace('/TechOps/Move?techopid=' + selectedId + '&direction=0&selectedOpId=' + selectedId);
        }
    });

    // const $daycells = $(".table-calendar-fixedr").find('.day-cell')
    // if ($daycells.length > 0) {
    //     $daycells.each(function () {
    //         $(this).find("strong").first().val() == "0.0 ч"
    //     })
    // }
    $(".table-calendar-fixed").on("click", "day-cell", function (e) {
        e.stopPropagation();

    })
    $(".btn-calendar").on("click", function (e) {
        if ($(e.target).closest('.btn-calendar-delete').length > 0) {
            return;
        }
        e.preventDefault();
        var date = $(this).data('date');
        var $exception = $(this).closest('td').find('small');
        var isEditing = $exception.length > 0;
        var action = isEditing ? '/Modify' : '/Create';
        var url = '/' + controller + action + '/' + date;
        $('#modalTitle').text(isEditing ? "Редактировать исключение" : "Новое исключение");
        $('#modalBodyContainer').load(url, function () {
            $('#createModal').modal('show');
        });
    });
    $(document).on("click", ".btn-calendar-delete", function (e) {
        e.stopPropagation();
        e.preventDefault();
        
        var $btn = $(this);
        var $cell = $btn.closest('td');
        var hasException = $cell.find('small').length > 0;
        if (hasException) {
            var date = $btn.data('date');
            var url = '/' + controller + "/Delete/" + date;

            if (confirm(`Вы уверены, что хотите удалить исключение на дату: ${date}?`)) {
                $.ajax({
                    url: url,
                    type: 'DELETE',
                    success: function (result) {
                        $cell.fadeOut(400, function () {
                            window.location.reload();
                        });
                    },
                    error: function (xhr, status, error) {
                        alert('Ошибка удаления записи: ' + error);
                    }
                });
            }
        }
    });
})
        
        