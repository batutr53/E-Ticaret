(() => {
    const province = document.getElementById('deliveryProvinceId');
    const district = document.getElementById('deliveryDistrictId');
    const neighborhood = document.getElementById('deliveryNeighborhoodId');
    const quote = document.getElementById('quotedDeliveryFee');
    const status = document.getElementById('deliveryAddressStatus');
    if (!province || !district || !neighborhood || !quote || !status) return;
    let provinceVersion = 0, districtVersion = 0;
    const reset = (select, label) => {
        select.replaceChildren(new Option(label, ''));
        select.value = '';
        select.disabled = true;
        select.setCustomValidity('');
    };
    const changed = () => neighborhood.dispatchEvent(new Event('change', { bubbles: true }));
    const read = async url => {
        const response = await fetch(url, { headers: { Accept: 'application/json' } });
        if (!response.ok) throw new Error('address');
        const data = await response.json();
        if (!Array.isArray(data)) throw new Error('address');
        return data;
    };
    province.addEventListener('change', async () => {
        const version = ++provinceVersion;
        ++districtVersion;
        reset(district, 'Önce il seçiniz');
        reset(neighborhood, 'Önce ilçe seçiniz');
        changed();
        status.textContent = '';
        province.setCustomValidity('');
        if (!province.value) return;
        province.setCustomValidity('İlçelerin yüklenmesini bekleyin.');
        status.textContent = 'İlçeler yükleniyor…';
        try {
            const rows = await read(`/DeliveryAddress/Districts?provinceId=${encodeURIComponent(province.value)}`);
            if (version !== provinceVersion) return;
            reset(district, 'İlçe seçiniz');
            rows.forEach(row => district.add(new Option(row.name, String(row.id))));
            district.disabled = rows.length === 0;
            province.setCustomValidity(rows.length ? '' : 'Bu ilde teslimata açık ilçe bulunmuyor.');
            status.textContent = rows.length ? '' : 'Bu ilde teslimata açık ilçe bulunmuyor.';
        } catch {
            if (version === provinceVersion) { province.setCustomValidity('İlçeler yüklenemedi. İli tekrar seçin.'); status.textContent = 'İlçeler yüklenemedi. İli tekrar seçerek deneyin.'; }
        }
    });
    district.addEventListener('change', async () => {
        const version = ++districtVersion;
        reset(neighborhood, 'Önce ilçe seçiniz');
        changed();
        status.textContent = '';
        district.setCustomValidity('');
        if (!district.value) return;
        district.setCustomValidity('Mahallelerin yüklenmesini bekleyin.');
        status.textContent = 'Mahalleler yükleniyor…';
        try {
            const rows = await read(`/DeliveryAddress/Neighborhoods?districtId=${encodeURIComponent(district.value)}`);
            if (version !== districtVersion) return;
            reset(neighborhood, 'Mahalle seçiniz');
            const money = value => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
            rows.forEach(row => {
                const option = new Option(`${row.name} — ${row.fee === 0 ? 'Ücretsiz' : money(row.fee)}`, String(row.id));
                option.dataset.fee = String(row.fee);
                option.dataset.name = row.name;
                neighborhood.add(option);
            });
            neighborhood.disabled = rows.length === 0;
            district.setCustomValidity(rows.length ? '' : 'Bu ilçede teslimata açık mahalle bulunmuyor.');
            status.textContent = rows.length ? '' : 'Bu ilçede teslimata açık mahalle bulunmuyor.';
        } catch {
            if (version === districtVersion) { district.setCustomValidity('Mahalleler yüklenemedi. İlçeyi tekrar seçin.'); status.textContent = 'Mahalleler yüklenemedi. İlçeyi tekrar seçerek deneyin.'; }
        }
    });
    neighborhood.addEventListener('change', () => {
        const selected = neighborhood.selectedOptions[0];
        quote.value = neighborhood.value ? selected?.dataset.fee || '' : '';
        const label = neighborhood.value ? [selected?.dataset.name, district.selectedOptions[0]?.text, province.selectedOptions[0]?.text].filter(Boolean).join(' / ') : 'Mahalle seçiniz';
        document.querySelectorAll('[data-checkout-address]').forEach(element => element.textContent = label);
    });
})();
