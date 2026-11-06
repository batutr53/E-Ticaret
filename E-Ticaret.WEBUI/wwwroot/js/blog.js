(() => {
    const content = document.getElementById('journalContent');
    const toc = document.querySelector('.journal-toc');
    if (content && toc) {
        const headings = content.querySelectorAll('h2,h3');
        headings.forEach((heading, index) => {
            heading.id = `bolum-${index + 1}`;
            const item = document.createElement('li');
            const link = document.createElement('a');
            link.href = `#${heading.id}`;
            link.textContent = heading.textContent;
            if (heading.tagName === 'H3') item.classList.add('is-subheading');
            item.append(link);
            toc.querySelector('ol').append(item);
        });
        toc.hidden = headings.length === 0;
    }
    document.querySelector('[data-copy-blog]')?.addEventListener('click', async event => {
        const status = document.querySelector('[data-copy-status]');
        try {
            await navigator.clipboard.writeText(event.currentTarget.dataset.copyBlog);
            status.textContent = 'Bağlantı kopyalandı.';
        } catch { status.textContent = 'Bağlantıyı adres çubuğundan kopyalayabilirsiniz.'; }
    });
})();
