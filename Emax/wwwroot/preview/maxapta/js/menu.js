function resizeScrenn(){
        if ($(window).width() <= 1023){$(".menu").fadeOut();} else{
	$(".menu").fadeIn();
      	}	
      }
      resizeScrenn();
      $(window).resize(function(){
      	resizeScrenn();
      });

$(document).ready(function(){

$('#menu-wrap').prepend("<div id='menu-icon'><a id='nav-toggle' href='#'><span></span></a><a href='#' id='btnn' class='b2' onclick='openbox(\"parent_popup\")'>Попробовать бесплатно</a></div>");
	
$("#nav-toggle").on("click", function(){
  $(".menu").slideToggle();
});


document.querySelector( "#nav-toggle" )
  .addEventListener( "click", function() {
   this.classList.toggle( "active" );
  });

});



