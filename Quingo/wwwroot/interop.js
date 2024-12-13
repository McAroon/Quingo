window.scrollToElementId = (elementId) => {
    const element = document.getElementById(elementId);
    if(!element) {
        return false;
    }
    
    window.scroll({ top: element.offsetTop, behavior: 'smooth' });
    return true;
};

window.scrollToLastChild = (elementId) => {
  const element = document.getElementById(elementId);
  if (!element) {
      return false;
  }
  element.lastElementChild.scrollIntoView({behavior: 'smooth', block: 'nearest', inline: 'center'});
  return true;
};

window.getBrowserTimeZone = () => {
    const options = Intl.DateTimeFormat().resolvedOptions(); 
    return options.timeZone;
};

class GameKeyListener {
    actionBarRef = null;
    handler = null;

    handleKeyDown(e) {
        if (!this.actionBarRef) return;
        if (e.code !== 'Space' && e.code !== 'Enter') return;

        e.preventDefault();
        if (e.repeat) return;

        this.actionBarRef.invokeMethodAsync('Draw');
    }
    
    add(ref) {
        this.actionBarRef = ref;
        if (this.handler) return;
        
        this.handler = this.handleKeyDown.bind(this);
        window.document.addEventListener('keydown', this.handler);
    }
    
    remove() {
        this.actionBarRef = null;
        window.document.removeEventListener('keydown', this.handler);
        this.handler = null;
    }
}

window.gameKeyListener = new GameKeyListener();