$(document).ready(() => {
    const controller = $("#crud-container").data("controller")
    $('#btn-crud-create').click(function () {
        let url = '/' + controller + '/Create'
        $('#modalTitle').text("Новая запись");
        $('#modalBodyContainer').load(url, function () {
            $('#createModal').modal('show')
        })
    })
    $('.tbody-operations').on('click', '.btn-crud-modify', function (e) {
        e.stopPropagation();
        let operationId = $(this).data('id')
        let url = '/' + controller + "/Modify" + '/' + operationId
        $('#modalTitle').text("Редактирование записи")
        $('#modalBodyContainer').load(url, function () {
            $('#createModal').modal('show')
        })
    })
        .on('click', '.btn-crud-delete', function (e) {
        e.stopPropagation();
        let operationId = $(this).data('id');
        let url = '/' + controller + '/Delete';
        let $row = $(this).closest('tr');
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
    })
        .on('click', '.tbody-tr', function () {

        let techId = $(this).find('td[data="tech-id"]').text().trim();
        window.location.href = '/Technologies/Operations/' + techId;
    })
        .on('click', '.op-row', function (e) {
            if ($(e.target).closest('.btn-crud-modify, .btn-crud-delete').length > 0) {
                return;
            }
            let $radio = $(this).find('.op-radio');
            $radio.prop('checked', true);
            $('.op-row').removeClass('table-primary fw-bold');
            $(this).addClass('table-primary fw-bold');
            $('#btn-move-up, #btn-move-down').prop('disabled', false);
        });
    
    $('#createModal').on('submit', 'form', function (e) {
        e.preventDefault();
        let $form = $(this);
        let url = $form.attr('action');
        let currentUrl = window.location.href;
        let formData = $form.serialize();
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
                    let $newForm = $('#modalBodyContainer').find('form');
                    if (typeof $.validator !== 'undefined' && $.validator.unobtrusive) {
                        $.validator.unobtrusive.parse($newForm);
                    }
                } else {
                    $('#createModal').modal('hide');
                    window.location.reload();
                }
            },
            error: function (xhr, status, error) {
                let errorMessage = error;
                if (xhr.responseText) {
                    errorMessage = xhr.responseText;
                }
                alert("Ошибка при отправке данных: " + errorMessage);
            }
        });
    });

    $('#btn-move-up').on('click', function () {
        let selectedId = $('.op-radio:checked').val();
        if (selectedId) {
            window.location.replace('/TechOps/Move?techopid=' + selectedId + '&direction=1&selectedOpId=' + selectedId);
        }
    });

    $('#btn-move-down').on('click', function () {
        let selectedId = $('.op-radio:checked').val();
        if (selectedId) {
            window.location.replace('/TechOps/Move?techopid=' + selectedId + '&direction=0&selectedOpId=' + selectedId);
        }
    });
    
    $(".table-calendar-fixed").on("click", "day-cell", function (e) {
        e.stopPropagation();

    })
    
    $(".btn-calendar").on("click", function (e) {
        if ($(e.target).closest('.btn-calendar-delete').length > 0) {
            return;
        }
        e.preventDefault();
        let date = $(this).data('date');
        let $exception = $(this).closest('td').find('small');
        let isEditing = $exception.length > 0;
        let action = isEditing ? '/Modify' : '/Create';
        let url = '/' + controller + action + '/' + date;
        $('#modalTitle').text(isEditing ? "Редактировать исключение" : "Новое исключение");
        $('#modalBodyContainer').load(url, function () {
            $('#createModal').modal('show');
        });
    });

    $(".btn-calendar-delete").on("click", function (e) {
        e.stopPropagation();
        e.preventDefault();

        let $btn = $(this);
        let $cell = $btn.closest('td');
        let date = $btn.data('date');
        let url = '/' + controller + "/Delete/" + date;

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
                    let errorMessage = xhr.responseText || error;
                    alert('Ошибка удаления записи: ' + errorMessage);
                }
            });
        }
    })
})
        
        