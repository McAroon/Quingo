window.scrollToElementId = (elementId) => {
    const element = document.getElementById(elementId);
    if(!element)
    {
        return false;
    }
    
    window.scroll({ top: element.offsetTop, behavior: 'smooth' });
    return true;
};
