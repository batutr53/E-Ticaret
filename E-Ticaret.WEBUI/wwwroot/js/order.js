let currentStep = 1;
const steps = document.querySelectorAll('.steporder-step');
const indicators = document.querySelectorAll('.steporder-steps li');

function showStep(step) {
    steps.forEach((el, i) => {
        el.classList.toggle('active', i + 1 === step);
    });
    indicators.forEach((el, i) => {
        el.classList.toggle('active', i + 1 <= step);
    });
    currentStep = step;
}

document.querySelectorAll('.steporder-next').forEach(btn => {
    btn.addEventListener('click', () => {
        if (currentStep < steps.length) showStep(currentStep + 1);
    });
});

document.querySelectorAll('.steporder-prev').forEach(btn => {
    btn.addEventListener('click', () => {
        if (currentStep > 1) showStep(currentStep - 1);
    });
});

document.getElementById('steporder-form').addEventListener('submit', (e) => {
 
});

showStep(currentStep);
