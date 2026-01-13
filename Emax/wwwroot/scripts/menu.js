$(document).ready(function () { 
	var touch 	= $('#touch-menu');
	var nav 	= $('nav');

	$(touch).on('click', function(e) {
		e.preventDefault();
		nav.slideToggle();
	});
	
	$(window).resize(function(){
		var w = $(window).width();
		if(w > 945 && nav.is(':hidden')) {
			nav.removeAttr('style');
		}
	});	
});

$(document).ready(function () {
    $('.mobile-menu').click(function () {
        $(this).toggleClass('open');
    });
});


$(function () {
    $('nav [href]').each(function () {
        if (this.href === window.location.href) {
            $(this).addClass('active');
        }
    });
});


                        
                        