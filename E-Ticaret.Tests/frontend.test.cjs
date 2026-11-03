const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const source = file => readFileSync(path.join(__dirname, '../E-Ticaret.WEBUI/wwwroot/js', file), 'utf8');
function checkout() {
    const events = target => Object.assign(target, { listeners: {}, addEventListener(name, callback) { this.listeners[name] = callback; } });
    const field = () => ({ valid: true, checkValidity() { return this.valid; }, focus() {}, reportValidity() {}, classList: { add() {}, remove() {} } });
    const sender = field();
    const region = events(Object.assign(field(), { value: '', selectedOptions: [{ dataset: {} }] }));
    region.valid = false;
    const steps = [sender, region, field(), field()].map(input => ({
        active: false, querySelectorAll: () => [input], querySelector: () => input.valid ? null : input,
        classList: { toggle(_, active) { this.owner.active = active; } }
    }));
    steps.forEach(step => step.classList.owner = step);
    const next = events({}), prev = events({}), submit = {};
    const form = events({ querySelector: () => submit });
    const totals = { '[data-checkout-subtotal]': [{}], '[data-checkout-fee]': [{}], '[data-checkout-total]': [{}] };
    const document = {
        getElementById: id => ({ 'steporder-form': form, deliveryNeighborhoodId: region, 'checkout-price-source': { dataset: { subtotal: '2001.00' } } })[id],
        querySelectorAll: selector => ({ '.steporder-step': steps, '.steporder-steps li': [], '.steporder-next': [next], '.steporder-prev': [prev] })[selector] || totals[selector]
    };
    const window = events({});
    vm.runInNewContext(source('order.js'), { document, window, Intl });
    return { sender, region, steps, next, prev, form, submit, totals, window };
}

test('region selection is required before advancing and totals follow the selected fee', () => {
    const ui = checkout();
    ui.next.listeners.click();
    assert.equal(ui.steps[1].active, true);
    ui.next.listeners.click();
    assert.equal(ui.steps[1].active, true);
    ui.region.value = '1'; ui.region.valid = true;
    ui.region.selectedOptions = [{ dataset: { fee: '175.50' } }];
    ui.region.listeners.change();
    assert.match(ui.totals['[data-checkout-total]'][0].textContent, /2\.176,50/);
    ui.next.listeners.click();
    assert.equal(ui.steps[2].active, true);
    ui.region.selectedOptions = [{ dataset: { fee: '0' } }];
    ui.region.listeners.change();
    assert.equal(ui.totals['[data-checkout-fee]'][0].textContent, 'Ücretsiz');
});

test('submit reveals an invalid earlier step and disables the payment button only for a valid form', () => {
    const ui = checkout();
    ui.next.listeners.click();
    ui.sender.valid = false;
    let prevented = false;
    ui.form.listeners.submit({ preventDefault() { prevented = true; } });
    assert.equal(prevented, true);
    assert.equal(ui.steps[0].active, true);
    assert.notEqual(ui.submit.disabled, true);
    ui.sender.valid = true; ui.region.valid = true;
    ui.form.listeners.submit({ preventDefault() { assert.fail('valid checkout was blocked'); } });
    assert.equal(ui.submit.disabled, true);
    ui.window.listeners.pageshow();
    assert.equal(ui.submit.disabled, false);
});

test('image protection blocks product-image saving gestures without blocking other page elements', () => {
    const callbacks = {};
    class Element {
        constructor(product) { this.product = product; }
        matches() { return this.product; }
        setAttribute() {}
    }
    vm.runInNewContext(source('product-images.js'), {
        Element,
        document: { addEventListener(name, callback) { callbacks[name] = callback; }, querySelectorAll: () => [] }
    });
    for (const name of ['contextmenu', 'dragstart']) {
        let prevented = false;
        callbacks[name]({ target: new Element(true), preventDefault() { prevented = true; } });
        assert.equal(prevented, true);
        callbacks[name]({ target: new Element(false), preventDefault() { assert.fail('ordinary element was blocked'); } });
    }
});
