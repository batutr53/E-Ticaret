// Toggle Mobile Menu
function toggleMobileMenu() {
    const mainMenu = document.getElementById('mainMenu');
    mainMenu.classList.toggle('active');
}

// Toggle Dropdown in Mobile
function toggleDropdown(event, menuClass) {
    event.preventDefault();

    if (window.innerWidth <= 768) {
        // Tüm menüleri kapat
        document.querySelectorAll('.dropdown-menu').forEach(menu => {
            menu.style.display = 'none';
        });

        const targetMenu = document.querySelector(`.${menuClass}`);
        if (targetMenu.style.display === 'block') {
            targetMenu.style.display = 'none';
        } else {
            targetMenu.style.display = 'block';
        }
    }
}


// Close mobile menu when clicking outside
document.addEventListener('click', function (event) {
    const mainMenu = document.getElementById('mainMenu');
    const mobileToggle = document.querySelector('.mobile-toggle');

    if (!mainMenu.contains(event.target) && !mobileToggle.contains(event.target)) {
        mainMenu.classList.remove('active');
    }
});

// Handle window resize
window.addEventListener('resize', function () {
    if (window.innerWidth > 768) {
        document.getElementById('mainMenu').classList.remove('active');
        document.querySelectorAll('.menu-item').forEach(item => {
            item.classList.remove('active');
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
    document.addEventListener("DOMContentLoaded", function () {
        const toggle = document.querySelector(".toggle-submenu");
    const parentLi = document.querySelector(".mobile-category");

    toggle.addEventListener("click", function (e) {
        e.preventDefault();
    parentLi.classList.toggle("open");
        });
    });
window.addEventListener('DOMContentLoaded', function () {
    const navbar = document.getElementById('mainNavbar');
    const navOffsetTop = navbar.offsetTop;

    window.addEventListener('scroll', function () {
        if (window.scrollY >= navOffsetTop) {
            navbar.classList.add('sticky');
        } else {
            navbar.classList.remove('sticky');
        }
    });
});