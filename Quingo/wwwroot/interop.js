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