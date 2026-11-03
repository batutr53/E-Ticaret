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
    const currentInputs = steps[step - 1].querySelectorAll('input, textarea, select');
    let valid = true;

    currentInputs.forEach(input => {
        if (!input.checkValidity()) {
            input.classList.add('input-error');
            valid = false;
        } else {
            input.classList.remove('input-error');
        }
    });

    if (!valid) {
        showStep(step);
        const firstInvalid = steps[step - 1].querySelector(':invalid');
        firstInvalid?.focus();
        firstInvalid?.reportValidity();
    }
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
    for (let step = 1; step <= steps.length; step++) {
        if (!validateStep(step)) {
            e.preventDefault();
            showStep(step);
            steps[step - 1].querySelector(':invalid')?.reportValidity();
            return;
        }
    }
    const submit = form.querySelector('[type=submit]');
    if (submit) { submit.disabled = true; submit.value = 'Ödeme hazırlanıyor…'; }
});

// Reveal the invalid step before asking the browser to focus its field.
form.noValidate = true;
const regionSelect = document.getElementById('deliveryNeighborhoodId');
function updateCheckoutTotals() {
    const subtotal = Number(document.getElementById('checkout-price-source')?.dataset.subtotal || 0);
    const selected = regionSelect?.selectedOptions[0];
    const pending = regionSelect && !regionSelect.value;
    const fee = Number(selected?.dataset.fee || 0);
    const money = value => new Intl.NumberFormat('tr-TR', {style: 'currency', currency: 'TRY'}).format(value);
    document.querySelectorAll('[data-checkout-subtotal]').forEach(el => el.textContent = money(subtotal));
    document.querySelectorAll('[data-checkout-fee]').forEach(el => el.textContent = pending ? 'Mahalle seçiniz' : fee === 0 ? 'Ücretsiz' : money(fee));
    document.querySelectorAll('[data-checkout-total]').forEach(el => el.textContent = pending ? 'Mahalle seçiminizden sonra hesaplanır' : money(subtotal + fee));
}
regionSelect?.addEventListener('change', updateCheckoutTotals);
updateCheckoutTotals();
window.addEventListener('pageshow', () => {
    const submit = form.querySelector('[type=submit]');
    if (submit) { submit.disabled = submit.dataset?.deliveryUnavailable === 'true'; submit.value = 'PayTR ile Güvenli Öde'; }
});
