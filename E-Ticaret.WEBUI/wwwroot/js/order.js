let currentStep = 1;
const steps = document.querySelectorAll('.steporder-step');
const indicators = document.querySelectorAll('.steporder-steps li');
const form = document.getElementById('steporder-form');

// İlk adımı göster
showStep(currentStep);

// Step gösterme fonksiyonu
function showStep(step) {
    steps.forEach((el, i) => {
        el.classList.toggle('active', i + 1 === step);
    });
    if (indicators.length > 0) {
        indicators.forEach((el, i) => {
            el.classList.toggle('active', i + 1 <= step);
        });
    }
    currentStep = step;
}

// Şu anki adımın inputlarını validate et
function validateStep(step) {
    const currentInputs = steps[step - 1].querySelectorAll('input, textarea');
    let valid = true;

    currentInputs.forEach(input => {
        if (!input.checkValidity()) {
            input.classList.add('input-error');
            valid = false;
        } else {
            input.classList.remove('input-error');
        }
    });

    return valid;
}

// İleri butonları
document.querySelectorAll('.steporder-next').forEach(btn => {
    btn.addEventListener('click', () => {
        if (validateStep(currentStep)) {
            if (currentStep < steps.length) {
                showStep(currentStep + 1);
            }
        }
    });
});

// Geri butonları
document.querySelectorAll('.steporder-prev').forEach(btn => {
    btn.addEventListener('click', () => {
        if (currentStep > 1) {
            showStep(currentStep - 1);
        }
    });
});

// Form Submit kontrolü (son adımda)
form.addEventListener('submit', (e) => {
    if (!validateStep(currentStep)) {
        e.preventDefault();
    }
});

// ✅ Inputmask ayarlamaları
document.addEventListener('DOMContentLoaded', function () {
    if (typeof Inputmask !== "undefined") {
        Inputmask({
            mask: "9999 9999 9999 9999",
            placeholder: " ",
            showMaskOnHover: false
        }).mask(document.querySelector('input[name="CardNumber"]'));

        Inputmask({
            mask: "99",
            placeholder: " ",
            showMaskOnHover: false
        }).mask(document.querySelector('input[name="CardExpireMonth"]'));

        Inputmask({
            mask: "99",
            placeholder: " ",
            showMaskOnHover: false
        }).mask(document.querySelector('input[name="CardExpireYear"]'));

        Inputmask({
            mask: "999[9]",
            placeholder: " ",
            showMaskOnHover: false
        }).mask(document.querySelector('input[name="CardCvc"]'));
    } else {
        console.error("Inputmask kütüphanesi yüklü değil.");
    }
});
