window.eldenParticles = (function () {
    let canvas = null;
    let ctx = null;
    let particles = [];
    let animFrame = null;
    let isRunning = false;
    const PARTICLE_COUNT = 40;

    function Particle() {
        this.reset();
        this.y = Math.random() * (canvas ? canvas.height : 800);
    }

    Particle.prototype.reset = function () {
        this.x = Math.random() * (canvas ? canvas.width : 1200);
        this.y = canvas ? canvas.height + 10 : 810;
        this.size = Math.random() * 2.5 + 0.5;
        this.speedY = -(Math.random() * 0.4 + 0.15);
        this.speedX = (Math.random() - 0.5) * 0.2;
        this.opacity = 0;
        this.maxOpacity = Math.random() * 0.5 + 0.2;
        this.fadeInRate = Math.random() * 0.008 + 0.003;
        this.life = 0;
        this.maxLife = Math.random() * 600 + 400;
        this.flickerSpeed = Math.random() * 0.02 + 0.01;
        this.wobble = Math.random() * Math.PI * 2;
    };

    Particle.prototype.update = function () {
        this.life++;
        this.wobble += this.flickerSpeed;

        if (this.life < this.maxLife * 0.15) {
            this.opacity = Math.min(this.maxOpacity, this.opacity + this.fadeInRate);
        } else if (this.life > this.maxLife * 0.7) {
            this.opacity *= 0.995;
        }

        this.x += this.speedX + Math.sin(this.wobble) * 0.15;
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

        ctx.restore();
    };

    function resize() {
        if (!canvas) return;
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
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

            canvas = document.getElementById(canvasId);
            if (!canvas) return;

            ctx = canvas.getContext('2d');
            resize();
            window.addEventListener('resize', resize);

            particles = [];
            for (var i = 0; i < PARTICLE_COUNT; i++) {
                particles.push(new Particle());
            }

            isRunning = true;
            animate();
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
