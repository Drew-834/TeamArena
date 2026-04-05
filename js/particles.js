window.eldenParticles = (function () {
    let canvas = null;
    let ctx = null;
    let particles = [];
    let animFrame = null;
    let isRunning = false;
    const BASE_PARTICLE_COUNT = 52;

    function Particle() {
        this.reset();
        this.y = Math.random() * (canvas ? canvas.height : 800);
    }

    Particle.prototype.reset = function () {
        this.x = Math.random() * (canvas ? canvas.width : 1200);
        this.y = canvas ? canvas.height + 10 : 810;
        this.size = Math.random() * 3 + 0.6;
        this.speedY = -(Math.random() * 0.38 + 0.12);
        this.speedX = (Math.random() - 0.5) * 0.22;
        this.opacity = 0;
        this.maxOpacity = Math.random() * 0.42 + 0.18;
        this.fadeInRate = Math.random() * 0.008 + 0.003;
        this.life = 0;
        this.maxLife = Math.random() * 720 + 420;
        this.flickerSpeed = Math.random() * 0.02 + 0.01;
        this.wobble = Math.random() * Math.PI * 2;
        this.driftRadius = Math.random() * 0.18 + 0.06;
    };

    Particle.prototype.update = function () {
        this.life++;
        this.wobble += this.flickerSpeed;

        if (this.life < this.maxLife * 0.15) {
            this.opacity = Math.min(this.maxOpacity, this.opacity + this.fadeInRate);
        } else if (this.life > this.maxLife * 0.7) {
            this.opacity *= 0.995;
        }

        this.x += this.speedX + Math.sin(this.wobble) * this.driftRadius;
        this.y += this.speedY;

        if (this.life > this.maxLife || this.y < -20 || this.opacity < 0.01) {
            this.reset();
        }
    };

    Particle.prototype.draw = function () {
        if (this.opacity <= 0) return;

        var flicker = 0.7 + Math.sin(this.wobble) * 0.3;
        var alpha = this.opacity * flicker;

        ctx.save();
        ctx.globalAlpha = alpha;
        ctx.fillStyle = '#c8a94e';
        ctx.shadowColor = 'rgba(200, 169, 78, 0.6)';
        ctx.shadowBlur = this.size * 4;

        ctx.beginPath();
        ctx.arc(this.x, this.y, this.size, 0, Math.PI * 2);
        ctx.fill();

        if (this.size > 2.3) {
            ctx.globalAlpha = alpha * 0.3;
            ctx.strokeStyle = 'rgba(232, 210, 130, 0.8)';
            ctx.lineWidth = 0.6;
            ctx.beginPath();
            ctx.arc(this.x, this.y, this.size * 2, 0, Math.PI * 2);
            ctx.stroke();
        }

        ctx.restore();
    };

    function resize() {
        if (!canvas) return;
        var ratio = window.devicePixelRatio || 1;
        canvas.width = Math.floor(window.innerWidth * ratio);
        canvas.height = Math.floor(window.innerHeight * ratio);
        canvas.style.width = window.innerWidth + 'px';
        canvas.style.height = window.innerHeight + 'px';
        if (ctx) {
            ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
        }
        updateParticleCount();
    }

    function updateParticleCount() {
        var target = Math.max(34, Math.min(70, Math.floor(window.innerWidth / 30)));
        while (particles.length < target) particles.push(new Particle());
        while (particles.length > target) particles.pop();
    }

    function animate() {
        if (!isRunning || !ctx || !canvas) return;

        ctx.clearRect(0, 0, canvas.width, canvas.height);

        for (var i = 0; i < particles.length; i++) {
            particles[i].update();
            particles[i].draw();
        }

        animFrame = requestAnimationFrame(animate);
    }

    return {
        init: function (canvasId) {
            var reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            if (reducedMotion) return;

            var self = this;
            // Defer so Blazor-rendered canvas is in the DOM and laid out
            requestAnimationFrame(function () {
                canvas = document.getElementById(canvasId);
                if (!canvas) return;

                ctx = canvas.getContext('2d');
                resize();
                window.addEventListener('resize', resize);

                particles = [];
                for (var i = 0; i < BASE_PARTICLE_COUNT; i++) {
                    particles.push(new Particle());
                }
                updateParticleCount();

                isRunning = true;
                animate();
            });
        },

        stop: function () {
            isRunning = false;
            if (animFrame) {
                cancelAnimationFrame(animFrame);
                animFrame = null;
            }
        },

        setCount: function (count) {
            while (particles.length < count) particles.push(new Particle());
            while (particles.length > count) particles.pop();
        }
    };
})();
