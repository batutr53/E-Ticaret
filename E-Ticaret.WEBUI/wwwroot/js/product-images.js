(() => {
    const selector = '.product-image img, .product-detail-main-image img, .product-detail-thumbnails img, .zoom-image, .basket-product-img, .order-item img';
    document.addEventListener('contextmenu', event => {
        if (event.target instanceof Element && event.target.matches(selector)) event.preventDefault();
    });
    document.addEventListener('dragstart', event => {
        if (event.target instanceof Element && event.target.matches(selector)) event.preventDefault();
    });
    document.querySelectorAll(selector).forEach(img => img.setAttribute('draggable', 'false'));
})();
