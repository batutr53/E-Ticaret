(() => {
    const form = document.getElementById('delivery-region-form');
    if (!form) return;
    const province = document.getElementById('region-province');
    const district = document.getElementById('region-district');
    const options = document.getElementById('region-neighborhood-options');
    const selectedContainer = document.getElementById('selected-neighborhoods');
    const count = document.getElementById('selected-neighborhood-count');
    const status = document.getElementById('region-address-status');
    const selected = new Map();
    selectedContainer.querySelectorAll('[data-neighborhood-id]').forEach(element => selected.set(Number(element.dataset.neighborhoodId), element.dataset.label));
    let provinceVersion = 0, districtVersion = 0;
    function renderSelected() {
        selectedContainer.replaceChildren();
        selected.forEach((label, id) => {
            const row = document.createElement('div');
            row.className = 'd-flex align-items-center justify-content-between border rounded p-2 mb-2';
            const hidden = document.createElement('input');
            hidden.type = 'hidden'; hidden.name = 'NeighborhoodIds'; hidden.value = String(id);
            const text = document.createElement('span'); text.textContent = label;
            const remove = document.createElement('button');
            remove.type = 'button'; remove.className = 'btn btn-sm btn-outline-danger ml-2'; remove.textContent = 'Çıkar';
            remove.addEventListener('click', () => {
                selected.delete(id);
                const checkbox = options.querySelector(`input[value="${id}"]`);
                if (checkbox) checkbox.checked = false;
                renderSelected();
            });
            row.append(hidden, text, remove); selectedContainer.append(row);
        });
        count.textContent = String(selected.size);
    }
    const read = async url => {
        const response = await fetch(url, { headers: { Accept: 'application/json' } });
        if (!response.ok) throw new Error('address');
        const data = await response.json();
        if (!Array.isArray(data)) throw new Error('address');
        return data;
    };
    async function loadDistricts(initialDistrict) {
        const version = ++provinceVersion;
        ++districtVersion;
        district.replaceChildren(new Option('Önce il seçiniz', '')); district.disabled = true;
        options.replaceChildren(); status.textContent = '';
        if (!province.value) return;
        status.textContent = 'İlçeler yükleniyor…';
        try {
            const rows = await read(`/Admin/DeliveryRegions/Districts?provinceId=${encodeURIComponent(province.value)}`);
            if (version !== provinceVersion) return;
            district.replaceChildren(new Option('İlçe seçiniz', ''));
            rows.forEach(row => district.add(new Option(row.name, String(row.id))));
            district.disabled = false; status.textContent = '';
            if (initialDistrict) { district.value = initialDistrict; await loadNeighborhoods(); }
        } catch { if (version === provinceVersion) status.textContent = 'İlçeler yüklenemedi. İl seçimini tekrarlayın. Mevcut mahalle seçimleriniz korunuyor.'; }
    }
    async function loadNeighborhoods() {
        const version = ++districtVersion;
        options.replaceChildren(); status.textContent = '';
        if (!district.value) return;
        const districtId = district.value;
        const provinceLabel = province.selectedOptions[0].text;
        const districtLabel = district.selectedOptions[0].text;
        status.textContent = 'Mahalleler yükleniyor…';
        try {
            const rows = await read(`/Admin/DeliveryRegions/Neighborhoods?districtId=${encodeURIComponent(districtId)}`);
            if (version !== districtVersion) return;
            rows.forEach(row => {
                const wrapper = document.createElement('div'); wrapper.className = 'col-md-6 mb-2';
                const label = document.createElement('label'); label.className = 'd-block border rounded p-2 mb-0';
                const input = document.createElement('input'); input.type = 'checkbox'; input.value = String(row.id); input.className = 'mr-2';
                const occupied = row.deliveryRegionId != null && Number(row.deliveryRegionId) !== Number(form.dataset.regionId);
                input.checked = selected.has(row.id); input.disabled = occupied;
                const text = document.createElement('span');
                text.textContent = row.name + (occupied ? ` — ${row.regionName} bölgesine bağlı` : '');
                input.addEventListener('change', () => {
                    if (input.checked) selected.set(row.id, `${provinceLabel} / ${districtLabel} / ${row.name}`);
                    else selected.delete(row.id);
                    renderSelected();
                });
                label.append(input, text); wrapper.append(label); options.append(wrapper);
            });
            status.textContent = rows.length ? 'Bağlamak istediğiniz mahalleleri işaretleyin.' : 'Bu ilçede kayıtlı mahalle bulunmuyor.';
        } catch { if (version === districtVersion) status.textContent = 'Mahalleler yüklenemedi. İlçe seçimini tekrarlayın. Mevcut seçimleriniz korunuyor.'; }
    }
    province.addEventListener('change', () => loadDistricts());
    district.addEventListener('change', loadNeighborhoods);
    renderSelected();
    if (province.value) loadDistricts(form.dataset.initialDistrict);
})();
