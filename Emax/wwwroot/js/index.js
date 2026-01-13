$(function () {
    $('.link [href]').each(function () {
        if (this.href === window.location.href) {
            $(this).addClass('active');
        }
    });
});

