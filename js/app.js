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

        if (container.__eldenCarouselAbortController) {
            container.__eldenCarouselAbortController.abort();
        }
        if (container.__eldenCarouselResizeObserver) {
            try {
                container.__eldenCarouselResizeObserver.disconnect();
            } catch (e) {
                console.warn('[Carousel] ResizeObserver disconnect failed:', e);
            }
        }

        var abortController = new AbortController();
        var signal = abortController.signal;
        container.__eldenCarouselAbortController = abortController;
        container.dataset.carouselInitialized = 'true';

        var childCount = track.children.length;
        var trackW = track.scrollWidth;
        var containerW = container.clientWidth;
        var maxOffset = Math.max(trackW - containerW, 0);
        var snapMode = container.dataset.snap === 'true';
        var guideButtons = Array.from(container.parentElement?.querySelectorAll('[data-carousel-index]') || []);
        var currentIndex = 0;

        console.log('[Carousel] children:', childCount,
            '| track scrollWidth:', trackW,
            '| container clientWidth:', containerW,
            '| maxOffset:', maxOffset,
            '| snap:', snapMode);

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

        function getSlideOffsets() {
            return Array.from(track.children).map(function (child) {
                return Math.min(child.offsetLeft, getMax());
            });
        }

        function getNearestIndex() {
            var offsets = getSlideOffsets();
            if (!offsets.length) return 0;

            var current = Math.abs(offset);
            var nearestIndex = 0;
            var nearestDistance = Number.MAX_VALUE;

            offsets.forEach(function (value, index) {
                var distance = Math.abs(value - current);
                if (distance < nearestDistance) {
                    nearestDistance = distance;
                    nearestIndex = index;
                }
            });

            return nearestIndex;
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
            if (snapMode) {
                slider.max = childCount > 1 ? childCount - 1 : 1;
                slider.step = 1;
                slider.value = currentIndex;
                return;
            }

            var m = getMax();
            slider.max = m > 0 ? m : 1;
            slider.step = 1;
            slider.value = Math.round(-offset);
        }

        function updateGuides() {
            guideButtons.forEach(function (button, index) {
                var isActive = index === currentIndex;
                button.classList.toggle('is-active', isActive);
                button.setAttribute('aria-selected', isActive ? 'true' : 'false');
            });
        }

        function snapToIndex(index, smooth) {
            var offsets = getSlideOffsets();
            if (!offsets.length) return;

            currentIndex = Math.max(0, Math.min(index, offsets.length - 1));
            offset = clampOffset(-offsets[currentIndex]);
            setTransform(smooth !== false);
            updateSlider();
            updateGuides();
        }

        // --- Slider ---
        if (slider) {
            slider.min = 0;
            slider.max = snapMode ? (childCount > 1 ? childCount - 1 : 1) : (maxOffset > 0 ? maxOffset : 1);
            slider.step = 1;
            slider.value = 0;
            console.log('[Carousel] Slider configured: max=' + slider.max);

            slider.addEventListener('input', function () {
                var val = Number(slider.value);
                if (snapMode) {
                    snapToIndex(val, true);
                } else {
                    offset = clampOffset(-val);
                    setTransform(false);
                    console.log('[Carousel] Slider input:', val, '-> offset:', offset);
                }
            }, { signal: signal });

            slider.addEventListener('change', function () {
                if (snapMode) {
                    snapToIndex(Number(slider.value), true);
                } else {
                    setTransform(true);
                }
            }, { signal: signal });

            try {
                var resizeObserver = new ResizeObserver(function () {
                    if (snapMode) {
                        snapToIndex(currentIndex, true);
                    } else {
                        offset = clampOffset(offset);
                        setTransform(true);
                        updateSlider();
                    }
                });
                resizeObserver.observe(container);
                container.__eldenCarouselResizeObserver = resizeObserver;
            } catch (e) {
                console.warn('[Carousel] ResizeObserver not supported');
            }
        }

        guideButtons.forEach(function (button) {
            button.addEventListener('click', function () {
                var index = Number(button.getAttribute('data-carousel-index'));
                snapToIndex(index, true);
            }, { signal: signal });
        });

        // --- Mouse drag ---
        track.addEventListener('mousedown', function (e) {
            isDragging = true;
            startX = e.clientX;
            dragOffset = offset;
            e.preventDefault();
        }, { signal: signal });

        window.addEventListener('mousemove', function (e) {
            if (!isDragging) return;
            var dx = e.clientX - startX;
            offset = clampOffset(dragOffset + dx);
            setTransform(false);
            updateSlider();
        }, { signal: signal });

        window.addEventListener('mouseup', function () {
            if (!isDragging) return;
            isDragging = false;
            if (snapMode) {
                snapToIndex(getNearestIndex(), true);
            } else {
                setTransform(true);
            }
        }, { signal: signal });

        // --- Touch ---
        track.addEventListener('touchstart', function (e) {
            isDragging = true;
            startX = e.touches[0].clientX;
            dragOffset = offset;
        }, { passive: true, signal: signal });

        track.addEventListener('touchmove', function (e) {
            if (!isDragging) return;
            var dx = e.touches[0].clientX - startX;
            offset = clampOffset(dragOffset + dx);
            setTransform(false);
            updateSlider();
        }, { passive: true, signal: signal });

        track.addEventListener('touchend', function () {
            if (!isDragging) return;
            isDragging = false;
            if (snapMode) {
                snapToIndex(getNearestIndex(), true);
            } else {
                setTransform(true);
            }
        }, { signal: signal });

        if (snapMode) {
            snapToIndex(0, true);
        } else {
            updateSlider();
        }
        updateGuides();

        console.log('[Carousel] Fully initialized. Ready.');
    }
};

window.scrollToCenter = (element) => {
    if (!element) return;
    element.scrollIntoView({ behavior: 'smooth', block: 'center', inline: 'center' });
};