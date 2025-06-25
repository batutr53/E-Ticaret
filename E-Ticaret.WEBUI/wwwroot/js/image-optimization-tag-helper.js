// Image optimization helper
class ImageOptimizer {
    constructor() {
        this.supportedFormats = {
            'image/webp': () => this.supportsWebP()
        };
        
        this.quality = 80;
        this.maxWidth = 1920;
        this.cache = {};
    }
    
    // Check if browser supports WebP
    async supportsWebP() {
        if (this.webpSupport !== undefined) return this.webpSupport;
        
        if (!self.createImageBitmap) return false;
        
        const webpData = 'data:image/webp;base64,UklGRh4AAABXRUJQVlA4TBEAAAAvAAAAAAfQ//73v/+BiOh/AAA=';
        const blob = await fetch(webpData).then(r => r.blob());
        this.webpSupport = await createImageBitmap(blob).then(() => true, () => false);
        
        return this.webpSupport;
    }
    
    // Optimize image
    async optimizeImage(img) {
        // Skip if already processed
        if (img.dataset.optimized === 'true') return;
        
        // Skip if not in viewport
        if (!this.isInViewport(img)) {
            this.lazyLoadImage(img);
            return;
        }
        
        // Check cache
        const cacheKey = img.src;
        if (this.cache[cacheKey]) {
            this.applyOptimizedImage(img, this.cache[cacheKey]);
            return;
        }
        
        // Process image
        try {
            const response = await fetch(img.src, { mode: 'cors' });
            const blob = await response.blob();
            
            // Skip if image is already small
            if (blob.size < 10240) { // 10KB
                img.dataset.optimized = 'true';
                return;
            }
            
            // Create canvas
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            const bitmap = await createImageBitmap(blob);
            
            // Calculate new dimensions
            let width = bitmap.width;
            let height = bitmap.height;
            
            if (width > this.maxWidth) {
                height = Math.round((this.maxWidth / width) * height);
                width = this.maxWidth;
            }
            
            // Set canvas dimensions
            canvas.width = width;
            canvas.height = height;
            
            // Draw image
            ctx.drawImage(bitmap, 0, 0, width, height);
            
            // Get optimized image as WebP
            const supportedFormats = await this.getSupportedFormats();
            let optimizedBlob;
            
            if (supportedFormats['image/webp']) {
                optimizedBlob = await new Promise(resolve => 
                    canvas.toBlob(resolve, 'image/webp', this.quality / 100)
                );
            } else {
                // Fallback to original format if WebP not supported
                optimizedBlob = await new Promise(resolve => 
                    canvas.toBlob(resolve, blob.type || 'image/jpeg', this.quality / 100)
                );
            }
            
            // Cache and apply
            const optimizedUrl = URL.createObjectURL(optimizedBlob);
            this.cache[cacheKey] = optimizedUrl;
            this.applyOptimizedImage(img, optimizedUrl);
            
        } catch (error) {
            console.error('Error optimizing image:', error);
            img.dataset.optimized = 'true';
        }
    }
    
    // Apply optimized image
    applyOptimizedImage(img, url) {
        // Add fade-in effect
        img.style.opacity = '0';
        img.style.transition = 'opacity 0.3s ease';
        
        // Set new image source
        img.onload = () => {
            img.style.opacity = '1';
            img.dataset.optimized = 'true';
            
            // Revoke object URL after image is loaded
            setTimeout(() => {
                URL.revokeObjectURL(url);
            }, 0);
        };
        
        img.src = url;
    }
    
    // Lazy load image
    lazyLoadImage(img) {
        if (img.dataset.lazyLoaded === 'true') return;
        
        const observer = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    this.optimizeImage(img);
                    observer.unobserve(img);
                    img.dataset.lazyLoaded = 'true';
                }
            });
        });
        
        observer.observe(img);
    }
    
    // Check if element is in viewport
    isInViewport(element) {
        const rect = element.getBoundingClientRect();
        return (
            rect.top >= 0 &&
            rect.left >= 0 &&
            rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
            rect.right <= (window.innerWidth || document.documentElement.clientWidth)
        );
    }
    
    // Get supported image formats
    async getSupportedFormats() {
        const results = {};
        
        for (const [format, check] of Object.entries(this.supportedFormats)) {
            results[format] = await check();
        }
        
        return results;
    }
}

// Initialize image optimizer
document.addEventListener('DOMContentLoaded', () => {
    const optimizer = new ImageOptimizer();
    
    // Process all images
    const images = document.querySelectorAll('img:not([data-no-optimize])');
    images.forEach(img => {
        if (img.complete) {
            optimizer.optimizeImage(img);
        } else {
            img.addEventListener('load', () => optimizer.optimizeImage(img));
        }
    });
});
