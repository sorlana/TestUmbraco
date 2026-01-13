$(document).ready(function(){ 
	var touch 	= $('#touch-menu');
	var nav 	= $('nav');

	$(touch).on('click', function(e) {
		e.preventDefault();
		nav.slideToggle();
	});
	
	$(window).resize(function(){
		var w = $(window).width();
		if (w > 945 && nav.is(':hidden')) {
			nav.removeAttr('style');
		}
	});
	
	$('nav a').on('click', function(){ 
		if ($('#touch-menu').css('display') != 'none'){
            $("#touch-menu").trigger( "click" );
        }
    });
	
});

