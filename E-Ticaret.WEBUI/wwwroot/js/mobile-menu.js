// Mobile menu toggle functionality
document.addEventListener('DOMContentLoaded', function() {
    const mobileToggle = document.getElementById('mobileToggle');
    const mainMenu = document.getElementById('mainMenu');
    const menuItems = document.querySelectorAll('.menu-item > a');
    const mainNav = document.querySelector('.main-nav');
    const header = document.querySelector('header');
    let lastScroll = 0;

    // Sticky header on scroll
    if (mainNav) {
        window.addEventListener('scroll', function() {
            const currentScroll = window.pageYOffset;
            
            if (currentScroll <= 0) {
                mainNav.classList.remove('sticky');
                return;
            }
            
            if (currentScroll > lastScroll && currentScroll > 100) {
                // Scrolling down
                mainNav.classList.add('sticky');
                mainNav.style.top = '0';
            } else {
                // Scrolling up
                mainNav.classList.add('sticky');
                mainNav.style.top = '0';
            }
            
            lastScroll = currentScroll;
        });
    }

    // Toggle mobile menu
    if (mobileToggle) {
        mobileToggle.addEventListener('click', function(e) {
            e.stopPropagation();
            mainMenu.classList.toggle('active');
            this.classList.toggle('active');
            
            // Toggle between hamburger and close icon
            const icon = this.querySelector('i');
            if (icon) {
                if (this.classList.contains('active')) {
                    icon.classList.remove('fa-bars');
                    icon.classList.add('fa-times');
                    document.body.style.overflow = 'hidden';
                } else {
                    icon.classList.remove('fa-times');
                    icon.classList.add('fa-bars');
                    document.body.style.overflow = '';
                }
            }
        });
    }


    // Close menu when clicking outside
    document.addEventListener('click', function(e) {
        if (mainMenu && mobileToggle && !mainMenu.contains(e.target) && !mobileToggle.contains(e.target)) {
            mainMenu.classList.remove('active');
            mobileToggle.classList.remove('active');
            const icon = mobileToggle.querySelector('i');
            if (icon) {
                icon.classList.remove('fa-times');
                icon.classList.add('fa-bars');
            }
            document.body.style.overflow = '';
        }
    });

    // Handle dropdown toggles on mobile
    menuItems.forEach(item => {
        if (item.nextElementSibling && item.nextElementSibling.classList.contains('dropdown-menu')) {
            item.addEventListener('click', function(e) {
                if (window.innerWidth <= 1124) { // Mobile breakpoint
                    e.preventDefault();
                    e.stopPropagation();
                    
                    // Close other open dropdowns
                    document.querySelectorAll('.menu-item').forEach(menuItem => {
                        if (menuItem !== this.parentElement) {
                            menuItem.classList.remove('active');
                        }
                    });
                    
                    // Toggle current dropdown
                    this.parentElement.classList.toggle('active');
                }
            });
        }
    });
    
    // Ensure Font Awesome icons are loaded
    if (typeof FontAwesomeConfig !== 'undefined') {
        FontAwesomeConfig.autoReplaceSvg = 'nest';
    }
});
