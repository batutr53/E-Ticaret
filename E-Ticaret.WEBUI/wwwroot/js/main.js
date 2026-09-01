// Toggle Mobile Menu
function toggleMobileMenu(button) {
    const mainMenu = document.getElementById('mainMenu');
    if (!mainMenu) return;

    mainMenu.classList.toggle('active');
    const isOpen = mainMenu.classList.contains('active');
    button?.setAttribute('aria-expanded', isOpen.toString());
    const icon = button?.querySelector('i');
    icon?.classList.toggle('fa-bars', !isOpen);
    icon?.classList.toggle('fa-xmark', isOpen);
}

// Toggle Dropdown in Mobile
function toggleDropdown(event, menuClass) {
    event.preventDefault();

    if (window.innerWidth <= 1124) {
        // Tüm menüleri kapat
        document.querySelectorAll('.dropdown-menu').forEach(menu => {
            if (!menu.classList.contains(menuClass)) {
                menu.style.display = 'none';
                menu.closest('.menu-item')?.classList.remove('active');
            }
        });

        const targetMenu = document.querySelector(`.${menuClass}`);
        if (!targetMenu) return;
        const menuItem = targetMenu.closest('.menu-item');
        if (targetMenu.style.display === 'block') {
            targetMenu.style.display = 'none';
            menuItem?.classList.remove('active');
        } else {
            targetMenu.style.display = 'block';
            menuItem?.classList.add('active');
        }
    }
}


// Close mobile menu when clicking outside
document.addEventListener('click', function (event) {
    const mainMenu = document.getElementById('mainMenu');
    const mobileToggle = document.querySelector('.mobile-toggle');

    if (mainMenu && mobileToggle && !mainMenu.contains(event.target) && !mobileToggle.contains(event.target)) {
        mainMenu.classList.remove('active');
        mobileToggle.setAttribute('aria-expanded', 'false');
        mobileToggle.querySelector('i')?.classList.replace('fa-xmark', 'fa-bars');
    }
});

// Handle window resize
window.addEventListener('resize', function () {
    if (window.innerWidth > 1124) {
        document.getElementById('mainMenu').classList.remove('active');
        const mobileToggle = document.querySelector('.mobile-toggle');
        mobileToggle?.setAttribute('aria-expanded', 'false');
        mobileToggle?.querySelector('i')?.classList.replace('fa-xmark', 'fa-bars');
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

    if (!toggle || !parentLi) return;
    toggle.addEventListener("click", function (e) {
        e.preventDefault();
    parentLi.classList.toggle("open");
        });
    });
window.addEventListener('DOMContentLoaded', function () {
    const navbar = document.getElementById('mainNavbar');
    if (!navbar) return;
    const navOffsetTop = navbar.offsetTop;

    window.addEventListener('scroll', function () {
        if (window.scrollY >= navOffsetTop) {
            navbar.classList.add('sticky');
        } else {
            navbar.classList.remove('sticky');
        }
    });
});