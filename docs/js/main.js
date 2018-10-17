/*Begin Slideshow Module*/

//Variables

/* pic1.jpg by Isa Martine on Unsplash */
/* pic2.jpg by Jack Anstey on Unsplash */
const pics = ["url(assets/pic1.jpg)", "url(assets/screen3.jpg)", "url(assets/pic2.jpg)", "url(assets/screen1.jpg)", "url(assets/screen2.jpg)"];

const slider = document.querySelector('.slider');
let current = 0;

// Init slide
function startSlide() {
    slider.style.backgroundImage = pics[0];
}


function slideRight() {
    slider.style.backgroundImage = pics[current + 1];
  current++;
}

//Timer
setInterval(function(){
    if (current === pics.length - 1) {
      current = -1;
    }
    
    slideRight();
  }, 3000);

  startSlide();
  
  /*End Slideshow Module*/