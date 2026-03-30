window.eldenAudio = (function () {
    let audioCtx = null;
    let isInitialized = false;
    let isMuted = false;
    let masterVolume = 0.3;
    let ambientGain = null;
    let ambientSource = null;

    function getContext() {
        if (!audioCtx) {
            audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        }
        if (audioCtx.state === 'suspended') {
            audioCtx.resume();
        }
        return audioCtx;
    }

    function playTone(freq, duration, type, volume, fadeIn, fadeOut) {
        if (isMuted) return;
        try {
            const ctx = getContext();
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.type = type || 'sine';
            osc.frequency.setValueAtTime(freq, ctx.currentTime);

            const vol = (volume || 0.1) * masterVolume;
            gain.gain.setValueAtTime(0, ctx.currentTime);
            gain.gain.linearRampToValueAtTime(vol, ctx.currentTime + (fadeIn || 0.02));
            gain.gain.linearRampToValueAtTime(0, ctx.currentTime + duration - (fadeOut || 0.05));

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.start(ctx.currentTime);
            osc.stop(ctx.currentTime + duration);
        } catch (e) { }
    }

    function playChord(freqs, duration, type, volume) {
        freqs.forEach(function (f) {
            playTone(f, duration, type, (volume || 0.05) / freqs.length);
        });
    }

    return {
        init: function () {
            if (isInitialized) return;
            try {
                getContext();
                isInitialized = true;
            } catch (e) { }
        },

        playHover: function () {
            if (isMuted) return;
            playTone(800, 0.08, 'sine', 0.04, 0.01, 0.03);
            playTone(1200, 0.06, 'sine', 0.02, 0.01, 0.02);
        },

        playClick: function () {
            if (isMuted) return;
            playTone(220, 0.15, 'triangle', 0.08, 0.01, 0.08);
            playTone(330, 0.12, 'sine', 0.04, 0.02, 0.06);
        },

        playTransition: function () {
            if (isMuted) return;
            try {
                const ctx = getContext();
                const dur = 0.4;
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();

                osc.type = 'sine';
                osc.frequency.setValueAtTime(300, ctx.currentTime);
                osc.frequency.exponentialRampToValueAtTime(100, ctx.currentTime + dur);

                gain.gain.setValueAtTime(0, ctx.currentTime);
                gain.gain.linearRampToValueAtTime(0.06 * masterVolume, ctx.currentTime + 0.05);
                gain.gain.linearRampToValueAtTime(0, ctx.currentTime + dur);

                osc.connect(gain);
                gain.connect(ctx.destination);
                osc.start(ctx.currentTime);
                osc.stop(ctx.currentTime + dur);
            } catch (e) { }
        },

        startAmbient: function () {
            if (isMuted || ambientSource) return;
            try {
                const ctx = getContext();
                ambientGain = ctx.createGain();
                ambientGain.gain.setValueAtTime(0, ctx.currentTime);
                ambientGain.gain.linearRampToValueAtTime(0.015 * masterVolume, ctx.currentTime + 3);
                ambientGain.connect(ctx.destination);

                // Layered drone: low fundamental + fifth + octave
                var freqs = [55, 82.5, 110];
                freqs.forEach(function (f) {
                    var osc = ctx.createOscillator();
                    osc.type = 'sine';
                    osc.frequency.setValueAtTime(f, ctx.currentTime);
                    var oscGain = ctx.createGain();
                    oscGain.gain.setValueAtTime(f === 55 ? 1.0 : 0.4, ctx.currentTime);
                    osc.connect(oscGain);
                    oscGain.connect(ambientGain);
                    osc.start();
                    if (!ambientSource) ambientSource = [];
                    ambientSource.push(osc);
                });
            } catch (e) { }
        },

        stopAmbient: function () {
            if (ambientSource) {
                ambientSource.forEach(function (osc) {
                    try { osc.stop(); } catch (e) { }
                });
                ambientSource = null;
            }
        },

        toggleMute: function () {
            isMuted = !isMuted;
            if (isMuted) {
                this.stopAmbient();
            } else {
                this.startAmbient();
            }
            return isMuted;
        },

        isMuted: function () {
            return isMuted;
        },

        setVolume: function (vol) {
            masterVolume = Math.max(0, Math.min(1, vol));
        }
    };
})();
