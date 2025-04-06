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

// Arama Fonksiyonları
const searchInput = document.querySelector('.search-input');
const searchBtn = document.querySelector('.search-btn');

searchBtn.addEventListener('click', () => {
    const searchTerm = searchInput.value.trim();
    if (searchTerm) {
        alert(`Arama yapılıyor: ${searchTerm}`);
        // Normal şartlarda arama sonuçları sayfasına yönlendirme yapılır
        // Here you would normally redirect to search results page
    }
});

searchInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
        searchBtn.click();
    }
});

// Cart İşlevleri
document.querySelectorAll('.add-to-cart-btn').forEach(button => {
    button.addEventListener('click', function () {
        const productCard = this.closest('.product-card');
        const productTitle = productCard.querySelector('.product-title').textContent;
        const productPrice = productCard.querySelector('.product-price').textContent;

        // Sepet sayacını güncelle
        const cartCount = document.querySelector('.cart-count');
        cartCount.textContent = parseInt(cartCount.textContent) + 1;

        // Kullanıcıya bildirim göster
        alert(`"${productTitle}" sepetinize eklendi!`);
    });
});

// Hızlı Görünüm İşlevi
document.querySelectorAll('.quick-view').forEach(quickView => {
    quickView.addEventListener('click', function (e) {
        e.stopPropagation(); // Kart hover durumunu etkilemesin

        const productCard = this.closest('.product-card');
        const productTitle = productCard.querySelector('.product-title').textContent;
        const productImg = productCard.querySelector('.product-image img').src;

        alert(`Hızlı görünüm: ${productTitle}`);
        // Gerçek uygulamada burada bir modal açılabilir
    });
});

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

//// Duyarlı Görsel Yükleme
//function loadResponsiveImages() {
//    const windowWidth = window.innerWidth;
//    const bannerImages = document.querySelectorAll('.banner-item img');

//    // Unsplash'ten yüksek kaliteli çiçek görselleri
//    const imageSources = {
//        desktop: [
//            'https://images.unsplash.com/photo-1586968695411-35c7c919195a?w=800&h=500&fit=crop',
//            'https://images.unsplash.com/photo-1589244159943-460088ed5c1e?w=800&h=500&fit=crop',
//            'https://images.unsplash.com/photo-1563241527-3004b7be0ffd?w=800&h=500&fit=crop',
//            'https://images.unsplash.com/photo-1561181286-d3fee7d55364?w=800&h=500&fit=crop'
//        ],
//        mobile: [
//            'https://images.unsplash.com/photo-1586968695411-35c7c919195a?w=400&h=300&fit=crop',
//            'https://images.unsplash.com/photo-1589244159943-460088ed5c1e?w=400&h=300&fit=crop',
//            'https://images.unsplash.com/photo-1563241527-3004b7be0ffd?w=400&h=300&fit=crop',
//            'https://images.unsplash.com/photo-1561181286-d3fee7d55364?w=400&h=300&fit=crop'
//        ]
//    };

//    bannerImages.forEach((img, index) => {
//        try {
//            if (windowWidth <= 768) {
//                img.src = imageSources.mobile[index];
//            } else {
//                img.src = imageSources.desktop[index];
//            }
//        } catch (e) {
//            console.error('Görsel yüklenirken hata:', e);
//        }
//    });
//}
///*feather.replace();*/
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

// Coupon toggle functionality
const couponHeader = document.querySelector('.basket-coupon-header');
const couponIcon = document.querySelector('.basket-coupon-icon');
const couponContent = document.querySelector('.basket-coupon-content');

//couponHeader.addEventListener('click', () => {
//    couponIcon.classList.toggle('open');
//    couponContent.classList.toggle('open');
//});

const ordercouponHeader = document.querySelector('.order-coupon-header');
const ordercouponIcon = document.querySelector('.order-coupon-icon');
const ordercouponContent = document.querySelector('.order-coupon-content');

//ordercouponHeader.addEventListener('click', () => {
//    ordercouponIcon.classList.toggle('open');
//    ordercouponContent.classList.toggle('open');
//});
// Sayfa yüklendiğinde ve yeniden boyutlandırıldığında çağır
window.addEventListener('load', loadResponsiveImages);
window.addEventListener('resize', loadResponsiveImages);