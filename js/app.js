// Key sequence detection for Weekly Tracker
let keySequence = [];
let keyTimeout = null;

window.initKeySequenceDetector = () => {
    document.addEventListener('keydown', (e) => {
        if (e.key === '.') {
            clearTimeout(keyTimeout);
            keySequence.push('.');
            keyTimeout = setTimeout(() => { keySequence = []; }, 2000);
            if (keySequence.length === 3) {
                document.getElementById('weekly-tracker-btn')?.classList.remove('hidden');
                document.getElementById('weekly-tracker-card')?.classList.remove('hidden');
                document.getElementById('team-tracker-card')?.classList.remove('hidden');
                keySequence = [];
            }
        }
    });
};

// Champion carousel - transform-based (no native scroll)
window.eldenCarousel = {
    init: function (containerId, sliderId) {
        console.log('[Carousel] init called with:', containerId, sliderId);

        var container = document.getElementById(containerId);
        if (!container) {
            console.error('[Carousel] Container not found:', containerId);
            return;
        }

        var slider = sliderId ? document.getElementById(sliderId) : null;
        if (!slider) {
            console.warn('[Carousel] Slider not found:', sliderId);
        }

        var track = container.querySelector('.carousel-track');
        if (!track) {
            console.error('[Carousel] Track not found inside container');
            return;
        }

        var childCount = track.children.length;
        var trackW = track.scrollWidth;
        var containerW = container.clientWidth;
        var maxOffset = Math.max(trackW - containerW, 0);

        console.log('[Carousel] children:', childCount,
            '| track scrollWidth:', trackW,
            '| container clientWidth:', containerW,
            '| maxOffset:', maxOffset);

        var offset = 0;
        var isDragging = false;
        var startX = 0;
        var dragOffset = 0;

        function getMax() {
            return Math.max(track.scrollWidth - container.clientWidth, 0);
        }

        function clampOffset(val) {
            var m = getMax();
            if (val > 0) return 0;
            if (val < -m) return -m;
            return val;
        }

        function setTransform(smooth) {
            if (smooth) {
                track.classList.remove('is-dragging');
            } else {
                track.classList.add('is-dragging');
            }
            track.style.transform = 'translateX(' + offset + 'px)';
        }

        function updateSlider() {
            if (!slider) return;
            var m = getMax();
            slider.max = m > 0 ? m : 1;
            slider.value = Math.round(-offset);
        }

        // --- Slider ---
        if (slider) {
            slider.min = 0;
            slider.max = maxOffset > 0 ? maxOffset : 1;
            slider.value = 0;
            console.log('[Carousel] Slider configured: max=' + slider.max);

            slider.addEventListener('input', function () {
                var val = Number(slider.value);
                offset = clampOffset(-val);
                setTransform(false);
                console.log('[Carousel] Slider input:', val, '-> offset:', offset);
            });

            slider.addEventListener('change', function () {
                setTransform(true);
            });

            try {
                new ResizeObserver(function () {
                    offset = clampOffset(offset);
                    setTransform(true);
                    updateSlider();
                }).observe(container);
            } catch (e) {
                console.warn('[Carousel] ResizeObserver not supported');
            }
        }

        // --- Mouse drag ---
        track.addEventListener('mousedown', function (e) {
            isDragging = true;
            startX = e.clientX;
            dragOffset = offset;
            e.preventDefault();
        });

        window.addEventListener('mousemove', function (e) {
            if (!isDragging) return;
            var dx = e.clientX - startX;
            offset = clampOffset(dragOffset + dx);
            setTransform(false);
            updateSlider();
        });

        window.addEventListener('mouseup', function () {
            if (!isDragging) return;
            isDragging = false;
            setTransform(true);
        });

        // --- Touch ---
        track.addEventListener('touchstart', function (e) {
            isDragging = true;
            startX = e.touches[0].clientX;
            dragOffset = offset;
        }, { passive: true });

        track.addEventListener('touchmove', function (e) {
            if (!isDragging) return;
            var dx = e.touches[0].clientX - startX;
            offset = clampOffset(dragOffset + dx);
            setTransform(false);
            updateSlider();
        }, { passive: true });

        track.addEventListener('touchend', function () {
            if (!isDragging) return;
            isDragging = false;
            setTransform(true);
        });

        console.log('[Carousel] Fully initialized. Ready.');
    }
};

window.scrollToCenter = (element) => {
    if (!element) return;
    element.scrollIntoView({ behavior: 'smooth', block: 'center', inline: 'center' });
};