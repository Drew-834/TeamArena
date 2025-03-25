// Carousel functionality
let isDragging = false;
let startPosition = 0;
let currentTranslate = 0;
let prevTranslate = 0;
let animationID = 0;
let currentCarousel = null;
let currentInner = null;

window.initCarousel = (containerElement, innerElement) => {
    if (!containerElement || !innerElement) return;

    currentCarousel = containerElement;
    currentInner = innerElement;

    // Mouse events
    innerElement.addEventListener('mousedown', dragStart);
    innerElement.addEventListener('mouseup', dragEnd);
    innerElement.addEventListener('mouseleave', dragEnd);
    innerElement.addEventListener('mousemove', drag);

    // Touch events
    innerElement.addEventListener('touchstart', dragStart);
    innerElement.addEventListener('touchend', dragEnd);
    innerElement.addEventListener('touchmove', drag);

    // Prevent context menu on long press
    innerElement.addEventListener('contextmenu', e => e.preventDefault());

    // Add transition class when not dragging
    innerElement.classList.add('transition-transform');
};

function dragStart(event) {
    if (!currentInner) return;

    // Get initial position
    startPosition = getPositionX(event);
    isDragging = true;

    // Remove transition during drag
    currentInner.classList.remove('transition-transform');

    // Save current transform
    const transform = window.getComputedStyle(currentInner).getPropertyValue('transform');
    if (transform !== 'none') {
        // Parse the matrix
        const matrix = transform.match(/^matrix\((.+)\)$/);
        if (matrix) {
            // The 4th value in the matrix is the X translation
            prevTranslate = parseInt(matrix[1].split(', ')[4], 10);
        } else {
            prevTranslate = 0;
        }
    } else {
        prevTranslate = 0;
    }

    // Start animation frame
    animationID = requestAnimationFrame(animation);
}

function dragEnd() {
    if (!isDragging || !currentInner) return;

    isDragging = false;
    cancelAnimationFrame(animationID);

    // Add transition back
    currentInner.classList.add('transition-transform');

    // Snap to closest slide
    const containerWidth = currentCarousel.offsetWidth;
    const slideWidth = 240 + 16; // card width + margin
    const maxSlides = currentInner.children.length;
    const maxTranslate = 0;
    const minTranslate = -((maxSlides * slideWidth) - containerWidth);

    // Calculate snap position
    let snapPosition = Math.round(currentTranslate / slideWidth) * slideWidth;

    // Enforce boundaries
    if (snapPosition > maxTranslate) snapPosition = maxTranslate;
    if (snapPosition < minTranslate) snapPosition = minTranslate;

    // Apply snap
    setTranslate(snapPosition);
}

function drag(event) {
    if (!isDragging || !currentInner) return;

    const currentPosition = getPositionX(event);
    currentTranslate = prevTranslate + currentPosition - startPosition;
}

function animation() {
    if (!isDragging || !currentInner) return;

    setTranslate(currentTranslate);
    requestAnimationFrame(animation);
}

function setTranslate(translate) {
    if (!currentInner) return;

    // Apply constraints
    const containerWidth = currentCarousel.offsetWidth;
    const innerWidth = currentInner.scrollWidth;
    const maxTranslate = 0;
    const minTranslate = -(innerWidth - containerWidth);

    if (translate > maxTranslate) translate = maxTranslate;
    if (translate < minTranslate) translate = minTranslate;

    currentInner.style.transform = `translateX(${translate}px)`;
}

function getPositionX(event) {
    return event.type.includes('mouse')
        ? event.pageX
        : event.touches[0].clientX;
}

// Function to scroll to a specific page
window.scrollCarouselToPage = (innerElement, pageIndex, itemsPerPage) => {
    if (!innerElement) return;

    const slideWidth = 240 + 16; // card width + margin
    const translate = -(pageIndex * slideWidth * itemsPerPage);

    innerElement.classList.add('transition-transform');
    innerElement.style.transform = `translateX(${translate}px)`;
};