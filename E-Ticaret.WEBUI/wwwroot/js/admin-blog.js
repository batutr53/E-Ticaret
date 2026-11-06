(() => {
    const title = document.getElementById('Title');
    const slug = document.getElementById('Slug');
    let customSlug = Boolean(slug?.value);
    const slugify = value => value.toLocaleLowerCase('tr-TR').replace(/[ığüşöç]/g, letter => ({'ı':'i','ğ':'g','ü':'u','ş':'s','ö':'o','ç':'c'}[letter])).normalize('NFD').replace(/[\u0300-\u036f]/g,'').replace(/[^a-z0-9]+/g,'-').replace(/^-|-$/g,'');
    slug?.addEventListener('input', () => { customSlug = Boolean(slug.value); });
    title?.addEventListener('input', () => { if (slug && !customSlug && slug.dataset.slugLocked !== 'true') slug.value = slugify(title.value); });
    let previewUrl;
    document.getElementById('Cover')?.addEventListener('change', event => {
        const file = event.target.files[0];
        if (!file) return;
        if (file.size > 10 * 1024 * 1024) { event.target.setCustomValidity('En fazla 10 MB görsel seçin.'); event.target.reportValidity(); return; }
        event.target.setCustomValidity('');
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        previewUrl = URL.createObjectURL(file);
        document.getElementById('blogCoverPreview').src = previewUrl;
    });
    if (typeof tinymce === 'undefined') return;
    const editorReady = tinymce.init({
        selector: '#blogBodyEditor', height: 650, menubar: false, branding: false,
        language: 'tr', language_url: 'https://cdn.jsdelivr.net/npm/tinymce-i18n@24.2.26/langs6/tr.js',
        entity_encoding: 'raw', plugins: 'lists link image table code autoresize',
        toolbar: 'undo redo | blocks | bold italic | bullist numlist blockquote | link image table | removeformat code',
        block_formats: 'Paragraf=p;Ana Başlık=h2;Alt Başlık=h3;Küçük Başlık=h4',
        content_style: 'body{font-family:Arial,sans-serif;font-size:16px;line-height:1.9;color:#415447;padding:15px 25px}img{max-width:100%;height:auto}h2{font-size:26px}h3{font-size:21px}',
        valid_elements: 'p,br,h2,h3,h4,strong/b,em/i,u,ul,ol[start],li,blockquote,a[href|title|target|rel],img[src|alt|width|height],table,thead,tbody,tr,th,td',
        automatic_uploads: true, images_file_types: 'jpg,jpeg,png,webp',
        images_upload_handler: async blobInfo => {
            const data = new FormData();
            data.append('file', blobInfo.blob(), blobInfo.filename());
            data.append('__RequestVerificationToken', document.querySelector('#blogEditorForm input[name="__RequestVerificationToken"]').value);
            const response = await fetch('/Admin/Blog/UploadImage', { method:'POST', body:data, credentials:'same-origin' });
            if (!response.ok) throw new Error('Görsel yüklenemedi. JPG, PNG veya WebP, en fazla 10 MB görsel seçin; oturumunuzun açık olduğundan emin olun.');
            const result = await response.json();
            if (!result.location) throw new Error('Görsel yüklenemedi.');
            return result.location;
        }
    });
    let submitting = false;
    document.getElementById('blogEditorForm')?.addEventListener('submit', async event => {
        if (submitting) return;
        event.preventDefault();
        const button = event.submitter;
        try {
            const editors = await editorReady;
            const uploads = await editors[0].uploadImages();
            if (uploads.some(upload => !upload.status)) throw new Error('Görsellerin yüklenmesini kontrol edin.');
            editors[0].save();
            submitting = true;
            event.target.requestSubmit(button);
        } catch (error) {
            tinymce.activeEditor?.notificationManager.open({ text: 'Yazı kaydedilmedi. Görsel yükleme işlemini kontrol edip yeniden deneyin.', type: 'error' });
        }
    });
})();
