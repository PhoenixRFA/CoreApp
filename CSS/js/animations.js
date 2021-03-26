window.addEventListener('load', function(){
    const btnTransition1 = document.getElementById('run-transition-1');
    const transition1 = document.getElementById('transition-1');

    btnTransition1.addEventListener('click', function(){
        runTransition1(transition1);
    });

    transition1.addEventListener('transitionstart', function(){
        this.style.borderBottom = '1px solid #990000';
    });
    transition1.addEventListener('transitionend', function(){
        this.style.borderBottomColor = '#009900';

        setTimeout(()=>this.style.borderBottom = '', 1500);
    });
    
    
    const btnTransition2Opacity = document.getElementById('run-transition-2');
    const btnTransition2Width = document.getElementById('run-transition-3');
    const btnTransition2Height = document.getElementById('run-transition-4');

    window.transition2Buttons = {
        'opacity': btnTransition2Opacity,
        'width': btnTransition2Width,
        'height': btnTransition2Height
    };

    const transition2 = document.getElementById('transition-2');
    btnTransition2Opacity.addEventListener('click', function(){
        toggleVisibility(transition2, 'opacity', this);
    });
    btnTransition2Width.addEventListener('click', function(){
        toggleVisibility(transition2, 'width', this);
    });
    btnTransition2Height.addEventListener('click', function(){
        toggleVisibility(transition2, 'height', this);
    });
});

function runTransition1(elem){
    const state = window.tranistion1State || false;

    if(state){
        elem.style.fontSize = '16px';
        elem.style.color = '#000000';
    } else {
        elem.style.fontSize = '24px';
        elem.style.color = '#990099';
    }

    window.tranistion1State = !state;
}

function toggleVisibility(elem, mode, btn){
    const makeVisible = elem.isHidden || false;
    
    const btns = Array.from(document.querySelectorAll('.btn-transition2') );
    if(makeVisible){
        elem.style.display = 'block';

        btns.forEach(b => b.disabled = false );
    } else {
        elem.addEventListener('transitionend', hideAndRemoveEventListener);

        btns.filter(x=>x != btn).forEach(b => b.disabled = true );
    }

    setTimeout(()=>{
        switch(mode){
            case 'opacity': toggleOpacity(elem, makeVisible); break;
            case 'width': toggleWidth(elem, makeVisible); break;
            case 'height': toggleHeight(elem, makeVisible); break;
        }
    });

    elem.isHidden = !makeVisible;

    btn.querySelector('.state').innerText = makeVisible ? 'Hide' : 'Show';
}

function toggleOpacity(elem, makeVisible){
    elem.style.opacity = makeVisible ? 1 : 0;
}
function toggleHeight(elem, makeVisible){
    elem.style.height = makeVisible ? '' : 0;
}
function toggleWidth(elem, makeVisible){
    elem.style.width = makeVisible ? '' : 0;
}
function hideAndRemoveEventListener(){
    this.style.display = 'none';
    this.removeEventListener('transitionend', hideAndRemoveEventListener);
}