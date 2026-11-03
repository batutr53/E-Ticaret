const { test } = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const { readFileSync } = require('node:fs');
const path = require('node:path');

function fixture(fetch) {
    class Option {
        constructor(text, value) { this.text = text; this.value = String(value); this.dataset = {}; }
    }
    class Event { constructor(type) { this.type = type; } }
    const select = () => ({
        value: '', options: [], disabled: false, listeners: {}, validity: '',
        get selectedOptions() { return this.options.filter(o => o.value === this.value); },
        addEventListener(type, callback) { (this.listeners[type] ||= []).push(callback); },
        add(option) { this.options.push(option); },
        replaceChildren(...options) { this.options = options; this.value = options[0]?.value || ''; },
        setCustomValidity(message) { this.validity = message; },
        dispatchEvent(event) { this.listeners[event.type]?.forEach(callback => callback(event)); },
        fire() { return Promise.all((this.listeners.change || []).map(callback => callback(new Event('change')))); }
    });
    const province = select(), district = select(), neighborhood = select(), quote = {}, status = {}, summary = {};
    province.options = [new Option('İstanbul', 34), new Option('Ankara', 6)];
    const elements = { deliveryProvinceId: province, deliveryDistrictId: district, deliveryNeighborhoodId: neighborhood,
        quotedDeliveryFee: quote, deliveryAddressStatus: status };
    vm.runInNewContext(readFileSync(path.join(__dirname, '../E-Ticaret.WEBUI/wwwroot/js/delivery-address.js'), 'utf8'), {
        document: { getElementById: id => elements[id], querySelectorAll: () => [summary] }, Option, Event, fetch, Intl
    });
    return { province, district, neighborhood, quote, status, summary };
}
const response = rows => ({ ok: true, json: async () => rows });

test('address hierarchy sets the fee and changing province clears the previous quote immediately', async () => {
    let finish;
    const ui = fixture(async url => {
        if (url.includes('provinceId=34')) return response([{ id: 1852, name: 'Ümraniye' }]);
        if (url.includes('districtId=1852')) return response([{ id: 98689, name: 'Çamlık', fee: 150.25 }]);
        return new Promise(resolve => finish = resolve);
    });
    ui.province.value = '34'; await ui.province.fire();
    ui.district.value = '1852'; await ui.district.fire();
    ui.neighborhood.value = '98689'; await ui.neighborhood.fire();
    assert.equal(ui.quote.value, '150.25');
    assert.equal(ui.summary.textContent, 'Çamlık / Ümraniye / İstanbul');
    ui.province.value = '6';
    const pending = ui.province.fire();
    assert.equal(ui.quote.value, '');
    assert.equal(ui.neighborhood.disabled, true);
    assert.notEqual(ui.province.validity, '');
    finish(response([])); await pending;
    assert.notEqual(ui.province.validity, '');
    assert.equal(ui.district.disabled, true);
});

test('a delayed response for the previous province cannot overwrite the current district list', async () => {
    const pending = [];
    const ui = fixture(() => new Promise(resolve => pending.push(resolve)));
    ui.province.value = '34'; const first = ui.province.fire();
    ui.province.value = '6'; const second = ui.province.fire();
    pending[1](response([{ id: 1231, name: 'Çankaya' }])); await second;
    pending[0](response([{ id: 1852, name: 'Ümraniye' }])); await first;
    assert.deepEqual(ui.district.options.map(o => o.text), ['İlçe seçiniz', 'Çankaya']);
});

test('failed neighborhood loading keeps address validation blocked and does not retain an old fee', async () => {
    const ui = fixture(async url => {
        if (url.includes('provinceId')) return response([{ id: 1852, name: 'Ümraniye' }]);
        throw new Error('network');
    });
    ui.province.value = '34'; await ui.province.fire();
    ui.quote.value = '0';
    ui.district.value = '1852'; await ui.district.fire();
    assert.equal(ui.quote.value, '');
    assert.equal(ui.neighborhood.disabled, true);
    assert.notEqual(ui.district.validity, '');
    assert.match(ui.status.textContent, /yüklenemedi/);
});
