// Tüm sayfa için gerekli fonksiyonlar

// Mobil Menü Geçişi
const mobileMenuBtn = document.querySelector('.mobile-menu-btn');
const mobileMenu = document.querySelector('.mobile-menu');
const mobileMenuClose = document.querySelector('.mobile-menu-close');

mobileMenuBtn.addEventListener('click', () => {
    mobileMenu.classList.add('active');
    document.body.style.overflow = 'hidden';
});

mobileMenuClose.addEventListener('click', () => {
    mobileMenu.classList.remove('active');
    document.body.style.overflow = 'auto';
});

// Mobil Alt Kategori Geçişleri
const mobileCategoryToggles = document.querySelectorAll('.toggle-submenu');
mobileCategoryToggles.forEach(toggle => {
    toggle.addEventListener('click', (e) => {
        e.preventDefault();
        const parent = toggle.closest('.mobile-category');
        parent.classList.toggle('active');

        // İkon değiştir
        const icon = toggle.querySelector('i');
        if (parent.classList.contains('active')) {
            icon.classList.remove('fa-chevron-down');
            icon.classList.add('fa-chevron-up');
        } else {
            icon.classList.remove('fa-chevron-up');
            icon.classList.add('fa-chevron-down');
        }
    });
});



//// Hızlı Görünüm İşlevi
//document.querySelectorAll('.quick-view').forEach(quickView => {
//    quickView.addEventListener('click', function (e) {
//        e.stopPropagation(); // Kart hover durumunu etkilemesin

//        const productCard = this.closest('.product-card');
//        const productTitle = productCard.querySelector('.product-title').textContent;
//        const productImg = productCard.querySelector('.product-image img').src;

//        alert(`Hızlı görünüm: ${productTitle}`);
//        // Gerçek uygulamada burada bir modal açılabilir
//    });
//});

// Masaüstü için Navigasyon Açılır Menüsü (gerekirse)
document.querySelectorAll('.nav-links li').forEach(item => {
    if (item.querySelector('ul')) {
        item.addEventListener('mouseover', () => {
            item.querySelector('ul').style.display = 'block';
        });

        item.addEventListener('mouseout', () => {
            item.querySelector('ul').style.display = 'none';
        });
    }
});

// Duyarlı Görsel Yükleme

feather.replace();
function changeImage(thumbnail) {
    document.getElementById('mainImage').src = thumbnail.src;
    document.querySelectorAll('.product-detail-thumbnails img').forEach(img => img.classList.remove('active'));
    thumbnail.classList.add('active');
}

document.querySelectorAll('.sepet-ekle-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        alert('Ürün sepete eklendi!');
    });
});
const optionBtns = document.querySelectorAll('.basket-option-btn');
optionBtns.forEach(btn => {
    btn.addEventListener('click', () => {
        optionBtns.forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
    });
});

// Quantity selector functionality
const quantitySelectors = document.querySelectorAll('.basket-quantity-selector');
quantitySelectors.forEach(selector => {
    const minusBtn = selector.querySelector('.basket-minus');
    const plusBtn = selector.querySelector('.basket-plus');
    const input = selector.querySelector('.basket-quantity-input');

    minusBtn.addEventListener('click', () => {
        let value = parseInt(input.value);
        if (value > 0) {
            input.value = value - 1;
        }
    });

    plusBtn.addEventListener('click', () => {
        let value = parseInt(input.value);
        input.value = value + 1;
    });
});
