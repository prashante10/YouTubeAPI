$(function () {
    $("#btnUpload").click(function () {
        $.ajax({
            type: "GET",
            url: "/Home/Upload",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function () {
                alert('Upoaded Successfully');
            },
            error: function () {
                alert('Error occured while uploading.');
            }
        });
    });
});