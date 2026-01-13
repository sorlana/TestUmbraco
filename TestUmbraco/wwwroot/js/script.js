$('document').ready(function () {
    $('#button').on('click', function () {
        if ($('#name').val() != '') {
            // ≈сли поле не пустое удал€ем класс-указание
            $('#name').removeClass('ml');
            document.getElementById('redname').style.display = "none";
        } else {
            // ≈сли поле пустое добавл€ем класс-указание
            $('#name').addClass('ml');
            document.getElementById('redname').style.display = "flex";
        }
        if ($('#mob').val() != '') {
            // ≈сли поле не пустое удал€ем класс-указание
            $('#mob').removeClass('ml');
            document.getElementById('redmob').style.display = "none";
        } else {
            // ≈сли поле пустое добавл€ем класс-указание
            $('#mob').addClass('ml');
            document.getElementById('redmob').style.display = "flex";
        }
        if ($('#ml').val() != '') {
            // ≈сли поле не пустое удал€ем класс-указание
            $('#ml').removeClass('ml');
            document.getElementById('redml').style.display = "none";
        } else {
            // ≈сли поле пустое добавл€ем класс-указание
            $('#ml').addClass('ml');
            document.getElementById('redml').style.display = "flex";
        }
        if ($('#comment').val() != '') {
            // ≈сли поле не пустое удал€ем класс-указание
            $('#comment').removeClass('ml');
            document.getElementById('redcom').style.display = "none";
        } else {
            // ≈сли поле пустое добавл€ем класс-указание
            $('#comment').addClass('ml');
            document.getElementById('redcom').style.display = "flex";
        }
    });
    $('#button4').on('click', function () {
        if ($('#wmob').val() != '') {
            // ≈сли поле не пустое удал€ем класс-указание
            $('#wmob').removeClass('ml');
            document.getElementById('redmob').style.display = "none";
        } else {
            // ≈сли поле пустое добавл€ем класс-указание
            $('#wmob').addClass('ml');
            document.getElementById('redmob').style.display = "flex";
        }
        if ($('#wtime').val() != '') {
            // ≈сли поле не пустое удал€ем класс-указание
            $('#wtime').removeClass('ml');
            document.getElementById('redml').style.display = "none";
        } else {
            // ≈сли поле пустое добавл€ем класс-указание
            $('#wtime').addClass('ml');
            document.getElementById('redml').style.display = "flex";
        }
    });
    $('#button3').on('click', function () {
        document.getElementById('windform').style.display = "flex";
        document.getElementById('windform').className = "activediv";
        document.getElementById('button3').style.display = "none";
    });
    $('#button4').on('click', function () {
        document.getElementById('windform').style.display = "none";
        document.getElementById('button3').style.display = "flex";
    });
});



function sendRequest() {
    if (($('#name').val() != '') && ($('#mob').val() != '') && ($('#ml').val() != '') && ($('#comment').val() != '') || ($('#wmob').val() != '' && $('#wtime').val() != '')) {
        if (!window.smartCaptcha) {
            return;
        }
        window.smartCaptcha.execute();

    }
}


function onloadFunction() {
    if (!window.smartCaptcha) {
        return;
    }

    window.smartCaptcha.render('captcha-container', {
        sitekey: 'XLylbaahfiGammzcPESVKOUqnnRpdyKozHlnIc7M',
        invisible: true, // —делать капчу невидимой
        callback: callback,
    });
}

function callback(token) {
    if ($('#name').val() != '') {
        $.ajax({
            url: '/Home/SendRequest',
            type: 'POST',
            data: {
                id: token,
                name: $("#name").val(),
                call: $("#mob").val(),
                mail: $("#ml").val(),
                comment: $("#comment").val()
            }
        });
        $("#name").val("");
        $("#mob").val("");
        $("#ml").val("");
        $("#comment").val("");
        document.getElementById('okmodal').style.display = "flex";
        setTimeout(function () {
            document.getElementById('okmodal').style.display = 'none';
        }, 5000);
    } else if ($('#wmob').val() != '') {

        var test = $("#wmob").val();

        $.ajax({
            url: '/Home/SendRequest2',
            type: 'POST',
            data: {
                id: token,
                wmob: $("#wmob").val(),
                wtime: $("#wtime").val()
            }
        });
        $("#wmob").val("");
        $("#wtime").val("");
        document.getElementById('okmodal2').style.display = "flex";
        setTimeout(function () {
            document.getElementById('okmodal2').style.display = 'none';
        }, 5000);
    }

}
